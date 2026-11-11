using System.Net.Http.Json;
using SistemaVisitas.Shared.DTOs;

namespace SistemaVisitas.Client.Services
{
    public class EmpresasService
    {
        private readonly HttpClient _httpClient;

        public EmpresasService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<EmpresaDto>> GetEmpresasAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<EmpresaDto>>("api/Empresas") ?? new List<EmpresaDto>();
        }

        // --- MODIFICADO: Ahora devuelve HttpResponseMessage para leer el error ---
        public async Task<HttpResponseMessage> CrearEmpresaAsync(EmpresaDto empresa)
        {
            return await _httpClient.PostAsJsonAsync("api/Empresas", empresa);
        }

        public async Task<bool> EliminarEmpresaAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Empresas/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}