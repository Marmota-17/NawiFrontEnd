using Microsoft.AspNetCore.Mvc;
using NawiWebAdmin.Models;
using NawiWebAdmin.Services;

namespace NawiWebAdmin.Controllers
{
    public class EventosController : Controller
    {
        private readonly NawiApiService _api;

        public EventosController(NawiApiService api)
        {
            _api = api;
        }

        // GET: /Eventos
        public async Task<IActionResult> Index()
        {
            // 1. Candado de Seguridad: Si no hay token, volver al login
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login", "Auth");

            // 2. Pedir eventos a la API
            var eventos = await _api.GetEventosAsync();

            // 3. Mandar la lista a la Vista
            return View(eventos);
        }

        // GET: /Eventos/Create (Mostrar formulario)
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login", "Auth");
            return View();
        }

        // POST: /Eventos/Create (Enviar datos)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Evento evento)
        {
            if (ModelState.IsValid)
            {
                // Enviamos a la API
                bool exito = await _api.CrearEventoAsync(evento);

                if (exito)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "La API rechazó la creación (revisa si el Token expiró).");
            }
            return View(evento);
        }
    }
}