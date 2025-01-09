/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Treachery.Client;

public class Browser(IJSRuntime jsRuntime)
{
    private LocalStorage Storage { get; set; } = new LocalStorage(jsRuntime);

    public async Task EnablePopover(ElementReference element)
    {
        if (!element.Equals(default(ElementReference))) await JsInvoke("EnablePopover", element);
    }

    public async Task EnablePopovers(ElementReference element)
    {
        if (!element.Equals(default(ElementReference))) await JsInvoke("EnablePopovers", element);
    }

    public async Task RemovePopover(ElementReference element)
    {
        if (!element.Equals(default(ElementReference))) await JsInvoke("RemovePopover", element);
    }

    public async Task RemovePopovers(ElementReference element)
    {
        if (!element.Equals(default(ElementReference))) await JsInvoke("RemovePopovers", element);
    }

    public async Task RefreshPopover(ElementReference element)
    {
        if (!element.Equals(default(ElementReference))) await JsInvoke("RefreshPopover", element);
    }

    public async Task RefreshPopovers(ElementReference element)
    {
        if (!element.Equals(default(ElementReference))) await JsInvoke("RefreshPopovers", element);
    }

    public async Task RemoveFocusFromButtons()
    {
        await JsInvoke("RemoveFocusFromButtons");
    }

    public async Task HideModal(string modalId)
    {
        await JsInvoke("HideModal", modalId);
    }

    public async Task Save(string filename, string data)
    {
        await JsInvoke<object>("saveFile", filename, data);
    }

    public async Task<string> LoadFile(object fileDialogRef)
    {
        return await JsInvoke<string>("readFile", fileDialogRef);
    }

    public async Task ClearFileInput(string fileDialogId)
    {
        await JsInvoke<object>("Clear", fileDialogId);
    }

    public async Task<bool> UrlExists(string url)
    {
        return await JsInvoke<bool>("UrlExists", url);
    }

    public async Task SetPlanetMapScale()
    {
        await JsInvoke("SetPlanetMapScale");
    }

    public async Task<Dimensions> GetWindowDimensions()
    {
        return await JsInvoke<Dimensions>("GetWindowDimensions");
    }

    public async Task<Dimensions> GetScreenDimensions()
    {
        return await JsInvoke<Dimensions>("GetScreenDimensions");
    }

    public async Task<Dimensions> GetImageDimensions(string imgUrl)
    {
        return await JsInvoke<Dimensions>("GetImageDimensions", imgUrl);
    }

    public async Task PlaySound(string sound, float volume = 100f, bool loop = false)
    {
        if (IsValidSound(sound)) await JsInvoke("PlaySound", sound, 0, CalculateVolume(volume), loop);
    }

    public async Task ChangeSoundVolume(string sound, float volume)
    {
        if (IsValidSound(sound)) await JsInvoke("ChangeSoundVolume", sound, CalculateVolume(volume));
    }

    public async Task StopSound(string sound)
    {
        if (IsValidSound(sound)) await JsInvoke("StopSound", sound, 3000);
    }

    public async Task StopSounds()
    {
        await JsInvoke("StopSounds", null, 0);
    }

    private static float CalculateVolume(float volumeOnLinearScaleFrom0To100)
    {
        if (volumeOnLinearScaleFrom0To100 <= 0)
            return 0;
        if (volumeOnLinearScaleFrom0To100 >= 100)
            return 1;
        return (float)(Math.Exp(0.05f * volumeOnLinearScaleFrom0To100 - 4) / Math.E);
    }

    private static bool IsValidSound(string sound)
    {
        return !string.IsNullOrEmpty(sound) && sound != "?";
    }

    public async Task SaveSetting(string name, object value)
    {
        await Storage.SetAsync(name, value);
    }

    public async Task SaveStringSetting(string name, string value)
    {
        await Storage.SetStringAsync(name, value);
    }

    public async Task<T> LoadSetting<T>(string name)
    {
        return await Storage.GetAsync<T>(name);
    }

    public async Task<string> LoadStringSetting(string name)
    {
        return await Storage.GetStringAsync(name);
    }

    public async Task ToggleFullScreen()
    {
        await JsInvoke("ToggleFullScreen");
    }

    public async Task<bool> IsFullScreen()
    {
        return await JsInvoke<bool>("IsFullScreen");
    }

    public async Task Print(string elementName)
    {
        await JsInvoke("Print", elementName);
    }

    private async Task<T> JsInvoke<T>(string method, params object[] args)
    {
        try
        {
            return await jsRuntime.InvokeAsync<T>(method, args);
        }
        catch (Exception e)
        {
            Support.Log("Error invoking method: {0}", e);
            return default;
        }
    }

    private async Task JsInvoke(string method, params object[] args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(method, args);
        }
        catch (Exception e)
        {
            Support.Log("Error invoking method: {0}", e.Message);
        }
    }
}