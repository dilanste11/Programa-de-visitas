using System.Net.Http.Json;
using SistemaVisitas.Shared.DTOs;

namespace SistemaVisitas.Client.Services
{
    public class PlantillasVisitaService
    {
        private readonly HttpClient _httpClient;

        public PlantillasVisitaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PlantillaVisitaDto>> GetPlantillasAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<PlantillaVisitaDto>>("api/PlantillasVisita") ?? new();
        }

        public async Task<PlantillaVisitaDto?> GetPlantillaCompletaAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<PlantillaVisitaDto>($"api/PlantillasVisita/{id}");
        }

        public async Task<HttpResponseMessage> CrearPlantillaAsync(PlantillaVisitaDto plantilla)
        {
            return await _httpClient.PostAsJsonAsync("api/PlantillasVisita", plantilla);
        }

        public async Task<HttpResponseMessage> ActualizarPlantillaAsync(PlantillaVisitaDto plantilla)
        {
            return await _httpClient.PutAsJsonAsync($"api/PlantillasVisita/{plantilla.Id}", plantilla);
        }
    }
}