using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using SistemaVisitas.Shared.DTOs;

namespace SistemaVisitas.Client.Auth
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<string?> Login(LoginRequestDto loginRequest)
        {
            // Hace la petición a tu API (AuthController)
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    // 1. Guardar el token
                    await _localStorage.SetItemAsync("authToken", result.Token);

                    // 2. Avisarle a Blazor casteando a nuestro Provider personalizado
                    ((CustomAuthStateProvider)_authStateProvider).MarcarUsuarioComoAutenticado(result.Token);

                    return null; // Null significa "sin errores"
                }
            }

            return "Credenciales incorrectas o error en el servidor.";
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((CustomAuthStateProvider)_authStateProvider).MarcarUsuarioComoDeslogueado();
        }
    }
}