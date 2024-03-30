/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Treachery.Client
{
    public class LocalStorage
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorage(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetAsync<T>(string key, T value)
        {
            string jsVal = value != null ? JsonSerializer.Serialize(value) : null;
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", new object[] { key, jsVal });
        }

        public async Task SetStringAsync(string key, string value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", new object[] { key, value });
        }

        public async Task<T> GetAsync<T>(string key)
        {
            string val = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

            if (val == null) return default!;

            try
            {
                T result = JsonSerializer.Deserialize<T>(val);
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
            string val = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            if (val == null) return default!;
            return val;
        }

        public async Task RemoveAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}
