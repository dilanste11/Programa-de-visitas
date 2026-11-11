using System.Net.Http.Json;
using SistemaVisitas.Shared.DTOs;

namespace SistemaVisitas.Client.Services
{
    public class TiposDispositivosService
    {
        private readonly HttpClient _httpClient;

        public TiposDispositivosService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TipoDispositivoDto>> GetTiposAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TipoDispositivoDto>>("api/TiposDispositivos") ?? new List<TipoDispositivoDto>();
        }

        public async Task<bool> CrearTipoAsync(TipoDispositivoDto tipo)
        {
            var response = await _httpClient.PostAsJsonAsync("api/TiposDispositivos", tipo);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarTipoAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/TiposDispositivos/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarTipoAsync(TipoDispositivoDto tipo)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/TiposDispositivos/{tipo.Id}", tipo);
            return response.IsSuccessStatusCode;
        }
    }
}