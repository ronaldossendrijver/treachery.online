/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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
        public static async Task EnablePopover(ElementReference element)
        {
            if (!element.Equals(default(ElementReference))) await JsInvoke("EnablePopover", element);
        }

        public static async Task EnablePopovers(ElementReference element)
        {
            if (!element.Equals(default(ElementReference))) await JsInvoke("EnablePopovers", element);
        }

        public static async Task RemovePopover(ElementReference element)
        {
            if (!element.Equals(default(ElementReference))) await JsInvoke("RemovePopover", element);
        }

        public static async Task RemovePopovers(ElementReference element)
        {
            if (!element.Equals(default(ElementReference))) await JsInvoke("RemovePopovers", element);
        }

        public static async Task RefreshPopover(ElementReference element)
        {
            if (!element.Equals(default(ElementReference))) await JsInvoke("RefreshPopover", element);
        }

        public static async Task RefreshPopovers(ElementReference element)
        {
            if (!element.Equals(default(ElementReference))) await JsInvoke("RefreshPopovers", element);
        }

        public static async Task RemoveFocusFromButtons() => await JsInvoke("RemoveFocusFromButtons");

        public static async Task HideModal(string modalId) => await JsInvoke("HideModal", modalId);

        public static async Task Save(string filename, string data) => await JsInvoke<object>("saveFile", filename, data);

        public static async Task<string> LoadFile(object fileDialogRef) => await JsInvoke<string>("readFile", fileDialogRef);

        public static async Task ClearFileInput(string fileDialogId) => await JsInvoke<object>("Clear", fileDialogId);

        public static async Task<bool> UrlExists(string url) => await JsInvoke<bool>("UrlExists", url);

        public static async Task SetPlanetMapScale() => await JsInvoke("SetPlanetMapScale");

        public static async Task<Dimensions> GetWindowDimensions() => await JsInvoke<Dimensions>("GetWindowDimensions");

        public static async Task<Dimensions> GetImageDimensions(string imgUrl) => await JsInvoke<Dimensions>("GetImageDimensions", imgUrl);

        public static async Task PlaySound(string sound, float volume = 100f, bool loop = false)
        {
            if (IsValidSound(sound)) await JsInvoke("PlaySound", sound, CalculateVolume(volume), loop);
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
            if (IsValidSound(sound)) await JsInvoke("ChangeSoundVolume", sound, CalculateVolume(volume));
        }

        public static async Task StopSound(string sound)
        {
            if (IsValidSound(sound)) await JsInvoke("StopSound", sound);
        }

        public static async Task FadeSound(string sound, float fromVolume, float toVolume, int milliseconds)
        {
            if (IsValidSound(sound) && fromVolume != toVolume) await JsInvoke("FadeSound", sound, CalculateVolume(fromVolume), CalculateVolume(toVolume), milliseconds);
        }

        public static async Task FadeAndStopSound(string sound, float fromVolume)
        {
            await FadeSound(sound, fromVolume, 0, 3000);
            await Task.Delay(3000).ContinueWith(e => StopSound(sound));
        }

        private static bool IsValidSound(string sound) => sound != null && sound != "" && sound != "?";

        public static async Task StopSounds() => await JsInvoke("StopSounds");

        public static async Task SaveSetting(string name, object value) => await Storage.SetAsync(name, value);

        public static async Task ClearSettingsStartingWith(string startOfName)
        {
            var keys = await Storage.GetKeys();
            foreach (var key in keys.Where(k => k.StartsWith(startOfName)))
            {
                await ClearSetting(key);
            }
        }

        public static async Task ClearSetting(string name) => await Storage.RemoveAsync(name);

        public static async Task SaveStringSetting(string name, string value) => await Storage.SetStringAsync(name, value);

        public static async Task<T> LoadSetting<T>(string name) => await Storage.GetAsync<T>(name);

        public static async Task<string> LoadStringSetting(string name) => await Storage.GetStringAsync(name);

        public static async Task ToggleFullScreen() => await JsInvoke("ToggleFullScreen");

        public static async Task<bool> IsFullScreen() => await JsInvoke<bool>("IsFullScreen");

        public static async Task Print(string elementName) => await JsInvoke("Print", elementName);

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
