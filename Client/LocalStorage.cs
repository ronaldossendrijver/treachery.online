/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Treachery.Client;

public class LocalStorage
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var jsVal = value != null ? Utilities.Serialize(value) : null;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, jsVal);
    }

    public async Task SetStringAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var val = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

        if (val == null) return default!;

        try
        {
            var result = Utilities.Deserialize<T>(val);
            return result;
        }
        catch (Exception)
        {
            return default!;
        }
    }

    public async Task<IEnumerable<string>> GetKeys()
    {
        return await _jsRuntime.InvokeAsync<string[]>("GetLocalStorageKeys");
    }

    public async Task<string> GetStringAsync(string key)
    {
        var val = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        if (val == null) return default!;
        return val;
    }

    public async Task RemoveAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}