/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

        private static readonly ElementReference defaultElementReferenceValue = default;

        public static async Task EnablePopover(ElementReference element)
        {
            await JsInvoke("EnablePopover", element);
        }

        public static async Task EnablePopovers(ElementReference element)
        {
            if (!defaultElementReferenceValue.Equals(element))
            {
                await JsInvoke("EnablePopovers", element);
            }
        }

        public static async Task RemovePopover(ElementReference element)
        {
            if (!defaultElementReferenceValue.Equals(element))
            {
                await JsInvoke("RemovePopover", element);
            }
        }

        public static async Task RemovePopovers(ElementReference element)
        {
            if (!defaultElementReferenceValue.Equals(element))
            {
                await JsInvoke("RemovePopovers", element);
            }
        }

        public static async Task RefreshPopover(ElementReference element)
        {
            await JsInvoke("RefreshPopover", element);
        }


        public static async Task RefreshPopovers(ElementReference element)
        {
            if (!defaultElementReferenceValue.Equals(element))
            {
                await JsInvoke("RefreshPopovers", element);
            }
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
            await JsInvoke("HideModal", modalId);
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
                await JsInvoke("PlaySound", sound, CalculateVolume(volume), loop);
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
                await JsInvoke("ChangeSoundVolume", sound, CalculateVolume(volume));
            }
        }

        public static async Task StopSound(string sound)
        {
            if (sound != null && sound != null)
            {
                await JsInvoke("StopSound", sound);
            }
        }

        public static async Task FadeSound(string sound, float fromVolume, float toVolume, int milliseconds)
        {
            if (sound != null && sound != null && fromVolume != toVolume)
            {
                await JsInvoke("FadeSound", sound, CalculateVolume(fromVolume), CalculateVolume(toVolume), milliseconds);
            }
        }

        public static async Task FadeAndStopSound(string sound, float fromVolume)
        {
            await FadeSound(sound, fromVolume, 0, 3000);
            _ = Task.Delay(3000).ContinueWith(e => StopSound(sound));
        }

        public static async Task StopSounds()
        {
            await JsInvoke("StopSounds");
        }

        public static async Task<IEnumerable<CaptureDevice>> GetCaptureDevices(bool getPermissionsFirst)
        {
            var devices = await JsInvoke<JsonElement[]>("GetCaptureDevices", getPermissionsFirst);

            if (devices == null) return new List<CaptureDevice>();

            return devices.Select(d => new CaptureDevice()
            {
                DeviceId = d.GetProperty("deviceId").GetString(),
                GroupId = d.GetProperty("groupId").GetString(),
                Kind = d.GetProperty("kind").GetString(),
                Label = DetermineDeviceLabel(d.GetProperty("label").GetString(), d.GetProperty("deviceId").GetString(), d.GetProperty("kind").GetString(), d.GetProperty("groupId").GetString())
            });
        }

        private static string DetermineDeviceLabel(string label, string deviceId, string kind, string groupId)
        {
            if (label != null && label.Length > 0)
            {
                return label;
            }
            else if (deviceId != null && deviceId.Length > 0)
            {
                return deviceId;
            }
            else if (kind != null && kind.Length > 0)
            {
                return kind;
            }
            else if (groupId != null && groupId.Length > 0)
            {
                return groupId;
            }
            else
            {
                return "unknown device";
            }
        }

        public static async Task InitializeVideo(string videoId)
        {
            await JsInvoke("InitializeVideo", videoId);
        }

        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        public static async Task PushVideoData(string videoId, byte[] data, float volume)
        {
            await semaphoreSlim.WaitAsync();

            try
            {

                await JsInvoke("PushVideoData", videoId, data, volume);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public static async Task CaptureMedia(string videoId, string audioDeviceId, string videoDeviceId)
        {
            await JsInvoke("CaptureMedia", videoId, audioDeviceId, videoDeviceId);
        }

        public static async Task StopCapture(string videoId)
        {
            await JsInvoke("StopCapture", videoId);
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
                await ClearSetting(key);
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
            await JsInvoke("OpenChatPopup");
        }

        public static async Task SendToChatPopup(PopupChatCommand msg)
        {
            await JsInvoke("SendToChatPopup", msg);
        }

        public static async Task ToggleFullScreen()
        {
            await JsInvoke("ToggleFullScreen");
        }

        public static async Task<bool> IsFullScreen()
        {
            return await JsInvoke<bool>("IsFullScreen");
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
}
