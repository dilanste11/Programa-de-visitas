using System.Net.Http.Json;
using SistemaVisitas.Shared.DTOs;

namespace SistemaVisitas.Client.Services
{
    public class UsuariosService
    {
        private readonly HttpClient _http;
        public UsuariosService(HttpClient http) { _http = http; }

        public async Task<List<UsuarioDto>> GetUsuarios() =>
            await _http.GetFromJsonAsync<List<UsuarioDto>>("api/usuarios") ?? new();

        public async Task Crear(UsuarioCrearDto dto) =>
            await _http.PostAsJsonAsync("api/usuarios", dto);

        public async Task Editar(UsuarioEditarDto dto) =>
            await _http.PutAsJsonAsync($"api/usuarios/{dto.Id}", dto);

        public async Task<List<UsuarioDto>> GetTecnicosAsync()
        {
            var respuesta = await _http.GetFromJsonAsync<List<UsuarioDto>>("api/Usuarios/Tecnicos");
            return respuesta ?? new List<UsuarioDto>();
        }
    }
}