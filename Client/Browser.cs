/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using Treachery.Shared;
using System.Linq;

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

        public static async Task<T> JsInvoke<T>(string method, params object[] args)
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

        public static async Task JsInvoke(string method, params object[] args)
        {
            try
            {
                await _runtime.InvokeVoidAsync(method, args);
            }
            catch (Exception e)
            {
                Support.Log("Error invoking method: {0}", e);
            }
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

        public static async Task EnableMapHover()
        {
            await JsInvoke("EnableMapHover");
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
        public static async Task DetermineCanvas()
        {
            await JsInvoke("DetermineCanvas");
        }

        public static async Task<Dimensions> GetMapDimensions()
        {
            return await JsInvoke<Dimensions>("GetMapDimensions");
        }

        public static async Task<Dimensions> GetWindowDimensions()
        {
            return await JsInvoke<Dimensions>("GetWindowDimensions");
        }

        public static async Task FadeAndPlaySound(string sound, float volume = 100f, bool loop = false)
        {
            await PlaySound(sound, 0, loop);
            await FadeSound(sound, 0, volume, 5000);
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

        /// <summary>
        /// Sets the SVG contents of an image in the browser's DOM
        /// </summary>
        /// <param name="imageId"></param>
        /// <param name="svg"></param>
        /// <returns></returns>
        public static async Task SetImageSVG(string imageId, string svg)
        {
            try
            {
                await JsInvoke<ElementReference>("SetImageSVG", imageId, svg);
            }
            catch (Exception e)
            {
                Support.Log(e);
            }
        }

        public static async Task CreateArrowImage(string arrowImageId, int marginX, int marginY, int radius, Point from, Point to, string arrowColor, string borderColor, float segmentScale)
        {
            try
            {
                int fromX = from.X - marginX - radius;
                int fromY = from.Y - marginY - radius;
                int fromZ = (int)Math.Pow(radius * radius - fromX * fromX - fromY * fromY, 0.5);
                int toX = to.X - marginX - radius;
                int toY = to.Y - marginY - radius;
                int toZ = (int)Math.Pow(radius * radius - toX * toX - toY * toY, 0.5);

                float[] fromP = new float[] { fromX, fromY, fromZ };
                float[] toP = new float[] { toX, toY, toZ };

                await JsInvoke("CreateArrowImage", arrowImageId, radius, fromP, toP, arrowColor, borderColor, segmentScale);
            }
            catch (Exception e)
            {
                Support.Log(e);
            }
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
}
