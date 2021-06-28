/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;
using System;

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
            string jsVal = null;
            if (value != null)
                jsVal = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem",
                new object[] { key, jsVal });
        }

        public async Task SetStringAsync(string key, string value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem",
                new object[] { key, value });
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

        public async Task ClearAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
    }
}
