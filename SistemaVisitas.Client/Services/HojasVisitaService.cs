using SistemaVisitas.Shared.DTOs;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace SistemaVisitas.Client.Services
{
    public class HojasVisitaService
    {
        private readonly HttpClient _httpClient;

        public HojasVisitaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> EjecutarVisitaAsync(EjecucionVisitaDto dto)
        {
            return await _httpClient.PostAsJsonAsync("api/HojasVisita/Ejecutar", dto);
        }

        public async Task<List<VisitaResumenDto>> GetHistorialVisitasAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<VisitaResumenDto>>("api/HojasVisita") ?? new();
            }
            catch
            {
                return new List<VisitaResumenDto>();
            }
        }

        public async Task<ActaVisitaDto?> GetVisitaDetalleAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ActaVisitaDto>($"api/HojasVisita/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> AsignarVisitaAsync(CrearAsignacionDto asignacion)
        {
            return await _httpClient.PostAsJsonAsync("api/HojasVisita/Asignar", asignacion);
        }
    } 
}