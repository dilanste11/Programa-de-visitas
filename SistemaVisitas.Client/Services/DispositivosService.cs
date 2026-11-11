using System.Net.Http.Json;
using SistemaVisitas.Shared.DTOs;

namespace SistemaVisitas.Client.Services
{
    public class DispositivosService
    {
        private readonly HttpClient _httpClient;

        public DispositivosService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DispositivoDto>> GetDispositivosAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<DispositivoDto>>("api/Dispositivos") ?? new List<DispositivoDto>();
        }

        public async Task<HttpResponseMessage> CrearDispositivoAsync(DispositivoDto dispositivo)
        {
            // Retornamos el HttpResponseMessage completo para poder leer los mensajes de error 
            // de validación dinámica que nos envíe el backend
            return await _httpClient.PostAsJsonAsync("api/Dispositivos", dispositivo);
        }

        public async Task<HttpResponseMessage> ActualizarDispositivoAsync(DispositivoDto dispositivo)
        {
            return await _httpClient.PutAsJsonAsync($"api/Dispositivos/{dispositivo.Id}", dispositivo);
        }

        public async Task<bool> EliminarDispositivoAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Dispositivos/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<DispositivoDto>> GetPorEmpresaAsync(int empresaId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<DispositivoDto>>($"api/Dispositivos/PorEmpresa/{empresaId}") ?? new();
            }
            catch
            {
                return new List<DispositivoDto>();
            }
        }
    }
}