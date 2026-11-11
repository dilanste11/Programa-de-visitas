using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace SistemaVisitas.Client.Auth
{
    public class CustomAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public CustomAuthorizationMessageHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Intentamos obtener el token guardado en el navegador
            var token = await _localStorage.GetItemAsync<string>("authToken");

            // Si existe, se lo pegamos a la cabecera de la petición (Authorization: Bearer <token>)
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Enviamos la petición modificada
            return await base.SendAsync(request, cancellationToken);
        }
    }
}