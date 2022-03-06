/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace Treachery.Client
{
    public static class Browser
    {
        private static IJSRuntime _runtime;

        public static LocalStorage Storage { get; private set; }

        public static void Initialize(IJSRuntime runtime)
        {
            _runtime = runtime;
            Storage = new LocalStorage(runtime);
        }

        public static async Task ReloadPage()
        {
            await JsInvoke("Reload");
        }

        public static async Task EnablePopovers()
        {
            await JsInvoke("EnablePopovers");
        }

        public static async Task RefreshPopovers()
        {
            await JsInvoke("RefreshPopovers");
        }

        public static async Task RemoveFocusFromButtons()
        {
            await JsInvoke("RemoveFocusFromButtons");
        }

        private static readonly Dictionary<string, Dimensions> measuredText = new();
        public static async Task<Dimensions> MeasureText(string text, string font)
        {
            string key = text + font;

            if (measuredText.ContainsKey(key))
            {
                return measuredText[key];
            }
            else
            {

                var value = await JsInvoke<Dimensions>("MeasureText", text, font);

                lock (measuredText)
                {
                    if (!measuredText.ContainsKey(key))
                    {
                        measuredText.Add(key, value);
                    }
                }
            }

            return measuredText[key];
        }

        public static async Task HideModal(string modalId)
        {
            await JsInvoke("HideModal", "#" + modalId);
        }

        public static async Task Save(string filename, string data)
        {
            await JsInvoke<object>("saveFile", filename, data);
        }

        public static async Task<string> LoadFile(object fileDialogRef)
        {
            return await JsInvoke<string>("readFile", fileDialogRef);
        }

        public static async Task ClearFileInput(string fileDialogId)
        {
            await JsInvoke<object>("Clear", fileDialogId);
        }

        public static async Task<bool> UrlExists(string url)
        {
            return await JsInvoke<bool>("UrlExists", url);
        }

        public static async Task EnableEnterEvents()
        {
            await JsInvoke("AddEnterListeners");
        }

        public static async Task SetPlanetMapScale()
        {
            await JsInvoke("SetPlanetMapScale");
        }

        public static async Task<Dimensions> GetWindowDimensions()
        {
            return await JsInvoke<Dimensions>("GetWindowDimensions");
        }

        public static async Task PlaySound(string sound, float volume = 100f, bool loop = false)
        {
            if (sound != null && sound != "" && sound != "?")
            {
                await _runtime.InvokeVoidAsync("PlaySound", sound, CalculateVolume(volume), loop);
            }
        }

        private static float CalculateVolume(float volumeOnLinearScaleFrom0to100)
        {
            if (volumeOnLinearScaleFrom0to100 <= 0)
            {
                return 0;
            }
            else if (volumeOnLinearScaleFrom0to100 >= 100)
            {
                return 1;
            }
            else
            {
                return (float)(Math.Exp(0.05f * volumeOnLinearScaleFrom0to100 - 4) / Math.E);
            }
        }

        public static async Task ChangeSoundVolume(string sound, float volume)
        {
            if (sound != null && sound != null)
            {
                await _runtime.InvokeVoidAsync("ChangeSoundVolume", sound, CalculateVolume(volume));
            }
        }

        public static async Task StopSound(string sound)
        {
            if (sound != null && sound != null)
            {
                await _runtime.InvokeVoidAsync("StopSound", sound);
            }
        }

        public static async Task FadeSound(string sound, float fromVolume, float toVolume, int milliseconds)
        {
            if (sound != null && sound != null && fromVolume != toVolume)
            {
                await _runtime.InvokeVoidAsync("FadeSound", sound, CalculateVolume(fromVolume), CalculateVolume(toVolume), milliseconds);
            }
        }

        public static async Task FadeAndStopSound(string sound, float fromVolume)
        {
            await FadeSound(sound, fromVolume, 0, 3000);
            _ = Task.Delay(3000).ContinueWith(e => StopSound(sound));
        }

        public static async Task StopSounds()
        {
            await _runtime.InvokeVoidAsync("StopSounds");
        }

        public static async Task PlayVideo(string source)
        {
            await _runtime.InvokeVoidAsync("PlayVideo", source);
        }

        public static async Task<IEnumerable<CaptureDevice>> GetCaptureDevices()
        {
            var devices = await _runtime.InvokeAsync<JsonElement[]>("GetCaptureDevices");

            return devices.Select(d => new CaptureDevice() { 
                DeviceId = d.GetProperty("deviceId").GetString(),
                GroupId = d.GetProperty("groupId").GetString(),
                Kind = d.GetProperty("kind").GetString(),
                Label = d.GetProperty("label").GetString()});
        }

        public static async Task InitializeVideo(string videoId)
        {
            await _runtime.InvokeVoidAsync("InitializeVideo", videoId);
        }

        public static async Task PushVideoData(string videoId, byte[] data)
        {
            await _runtime.InvokeVoidAsync("PushVideoData", videoId, data);
        }

        public static async Task CaptureMedia(string deviceId, string videoId, bool audio, bool video)
        {
            await _runtime.InvokeVoidAsync("CaptureMedia", deviceId, videoId, audio, video);
        }

        public static async Task StopCapture(string deviceId)
        {
            await _runtime.InvokeVoidAsync("StopCapture", deviceId);
        }

        public static event Action<byte[]> OnVideoData;

        [JSInvokable("HandleVideoData")]
        public static void HandleVideoData(byte[] data)
        {
            OnVideoData?.Invoke(data);
        }

        public static async Task SaveSetting(string name, object value)
        {
            await Storage.SetAsync(name, value);
        }

        public static async Task ClearSettingsStartingWith(string startOfName)
        {
            var keys = await Storage.GetKeys();
            foreach (var key in keys.Where(k => k.StartsWith(startOfName)))
            {
                await Storage.RemoveAsync(key);
            }
        }

        public static async Task ClearSetting(string name)
        {
            await Storage.RemoveAsync(name);
        }

        public static async Task SaveStringSetting(string name, string value)
        {
            await Storage.SetStringAsync(name, value);
        }

        public static async Task<T> LoadSetting<T>(string name)
        {
            return await Storage.GetAsync<T>(name);
        }

        public static async Task<string> LoadStringSetting(string name)
        {
            return await Storage.GetStringAsync(name);
        }

        public static async Task OpenChatPopup()
        {
            await _runtime.InvokeVoidAsync("OpenChatPopup");
        }

        public static async Task SendToChatPopup(PopupChatCommand msg)
        {
            await _runtime.InvokeVoidAsync("SendToChatPopup", msg);
        }

        public static async Task ToggleFullScreen()
        {
            await _runtime.InvokeVoidAsync("ToggleFullScreen");
        }

        public static async Task<bool> IsFullScreen()
        {
            return (await _runtime.InvokeAsync<bool>("IsFullScreen"));
        }

        public static async Task Print(string elementName)
        {
            try
            {
                await JsInvoke("Print", elementName);
            }
            catch (Exception e)
            {
                Support.Log(e);
            }
        }

        private static async Task<T> JsInvoke<T>(string method, params object[] args)
        {
            try
            {
                return await _runtime.InvokeAsync<T>(method, args);
            }
            catch (Exception e)
            {
                Support.Log("Error invoking method: {0}", e);
                return default;
            }
        }

        private static async Task JsInvoke(string method, params object[] args)
        {
            try
            {
                await _runtime.InvokeVoidAsync(method, args);
            }
            catch (Exception e)
            {
                Support.Log("Error invoking method: {0}", e.Message);
            }
        }
    }

    public abstract class PopupChatCommand
    {
        public abstract string type { get; set; }
    }

    public class PopupChatInitialization : PopupChatCommand
    {
        public override string type { get; set; } = "PopupChatInitialize";
        public string[] playerNames { get; set; }
        public string[] playerStyles { get; set; }

        public PopupChatMessage[] messages { get; set; }
    }

    public class PopupChatMessage : PopupChatCommand
    {
        public override string type { get; set; } = "PopupChatMessage";
        public string style { get; set; }
        public string body { get; set; }
    }

    public class PopupChatClear : PopupChatCommand
    {
        public override string type { get; set; } = "PopupChatClear";
    }

    public class CaptureDevice
    {
        public string Label;
        public string DeviceId;

        public string Kind { get; internal set; }
        public string GroupId { get; internal set; }

        public override string ToString() {

            return DeviceId;
        }
    }
}
