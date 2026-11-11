using System.Net.Http.Json;
using SistemaVisitas.Shared.DTOs;

namespace SistemaVisitas.Client.Services
{
    public class DashboardService
    {
        private readonly HttpClient _http;

        public DashboardService(HttpClient http)
        {
            _http = http;
        }

        public async Task<DashboardDto> GetEstadisticasAsync()
        {
            var response = await _http.GetAsync("api/Dashboard");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DashboardDto>() ?? new DashboardDto();
            }
            return new DashboardDto(); // Si falla, devuelve ceros para no romper la UI
        }
    }
}