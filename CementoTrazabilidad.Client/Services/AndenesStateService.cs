using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CementoTrazabilidad.Client.Services
{
    public class AndenesStateService
    {
        private readonly HttpClient _http;

        public List<AndenDto> Andenes { get; private set; } = new();

        public event Action? OnChange;

        public AndenesStateService(HttpClient http)
        {
            _http = http;
        }

        public async Task LoadAsync()
        {
            Andenes = await _http.GetFromJsonAsync<List<AndenDto>>("api/andenes") ?? new List<AndenDto>();
            NotifyStateChanged();
        }

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}