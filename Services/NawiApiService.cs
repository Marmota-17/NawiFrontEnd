using NawiWebAdmin.Models;
using System.Net.Http.Headers;

namespace NawiWebAdmin.Services
{
    public class NawiApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContext;

        public NawiApiService(HttpClient http, IHttpContextAccessor httpContext, IConfiguration config)
        {
            _http = http;
            _httpContext = httpContext;

            // Leemos la URL del appsettings.json
            var baseUrl = config["ApiSettings:BaseUrl"];
            _http.BaseAddress = new Uri(baseUrl);
        }

        // --- Inyectar Token Automáticamente ---
        private void AgregarToken()
        {
            var token = _httpContext.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // 1. LOGIN
        public async Task<bool> LoginAsync(string user, string pass)
        {
            var loginData = new { Username = user, Password = pass };
            // Llamamos al endpoint de tu API Backend
            var response = await _http.PostAsJsonAsync("api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    // ¡Éxito! Guardamos el carnet digital en la sesión
                    _httpContext.HttpContext?.Session.SetString("JwtToken", result.Token);
                    return true;
                }
            }
            return false;
        }

        // 2. OBTENER EVENTOS (GET)
        public async Task<List<Evento>> GetEventosAsync()
        {
            // Si tu GET es público, no necesitas AgregarToken()
            var response = await _http.GetAsync("api/eventos");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Evento>>() ?? new List<Evento>();
            }
            return new List<Evento>();
        }

        // 3. CREAR EVENTO (POST)
        public async Task<bool> CrearEventoAsync(Evento evento)
        {
            AgregarToken(); // <--- ¡Vital! Sin esto la API te rechazará (401)
            var response = await _http.PostAsJsonAsync("api/eventos", evento);
            return response.IsSuccessStatusCode;
        }

        // Clasecita interna para leer la respuesta del login
        private class LoginResponse { public string Token { get; set; } }
    }
}