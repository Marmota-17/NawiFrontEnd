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

        // 4. OBTENER UN SOLO EVENTO (Para llenar el formulario de edición)
        public async Task<Evento?> GetEventoAsync(long id)
        {
            // AgregarToken(); // Descomenta si tu GET {id} en la API es privado
            var response = await _http.GetAsync($"api/eventos/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Evento>();
            }
            return null;
        }

        // 5. EDITAR EVENTO (PUT)
        public async Task<bool> EditarEventoAsync(long id, Evento evento)
        {
            AgregarToken(); // Requiere seguridad
            var response = await _http.PutAsJsonAsync($"api/eventos/{id}", evento);
            return response.IsSuccessStatusCode;
        }

        // 6. ELIMINAR EVENTO (DELETE)
        public async Task<bool> EliminarEventoAsync(long id)
        {
            AgregarToken(); // Requiere seguridad
            var response = await _http.DeleteAsync($"api/eventos/{id}");
            return response.IsSuccessStatusCode;
        }

        // 5. OBTENER CATEGORÍAS (Para el dropdown)
        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            var response = await _http.GetAsync("api/categorias");
            if (response.IsSuccessStatusCode)
            {
                // Usamos la librería System.Net.Http.Json
                return await response.Content.ReadFromJsonAsync<List<Categoria>>() ?? new List<Categoria>();
            }
            return new List<Categoria>();
        }
        // 7. SUBIR IMAGEN (Faltaba este método)
        public async Task<string?> SubirImagenAsync(IFormFile archivo)
        {
            AgregarToken(); // Seguridad

            using var content = new MultipartFormDataContent();
            using var fileStream = archivo.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType);

            content.Add(fileContent, "file", archivo.FileName);

            // Llamamos al endpoint del StorageController en el Backend
            var response = await _http.PostAsync("api/storage/upload", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ImageResponse>();
                return result?.Url; // Retorna la URL pública (ej: https://.../foto.jpg)
            }
            return null;
        }

        // Clase auxiliar para leer la respuesta JSON { "url": "..." }
        private class ImageResponse { public string Url { get; set; } }
    }
}