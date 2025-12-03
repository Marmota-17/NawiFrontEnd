using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Necesario para el Dropdown
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

        // --- 1. LISTAR EVENTOS (Index) ---
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login", "Auth");

            var eventos = await _api.GetEventosAsync();
            return View(eventos);
        }

        // --- 2. CREAR (Vista) ---
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login", "Auth");

            await CargarCategorias(); // Carga el Dropdown
            return View();
        }

        // --- 3. CREAR (Procesar) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Evento evento, IFormFile? imagenFlyer)
        {
            // Ignoramos la URL porque la llenaremos nosotros
            ModelState.Remove(nameof(evento.FlyerUrl));

            if (ModelState.IsValid)
            {
                // A. Subir imagen (si hay)
                if (imagenFlyer != null && imagenFlyer.Length > 0)
                {
                    var url = await _api.SubirImagenAsync(imagenFlyer);
                    if (url != null)
                    {
                        evento.FlyerUrl = url;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al subir la imagen.");
                        await CargarCategorias();
                        return View(evento);
                    }
                }

                // B. Guardar evento
                bool exito = await _api.CrearEventoAsync(evento);
                if (exito) return RedirectToAction(nameof(Index));

                ModelState.AddModelError("", "La API rechazó los datos.");
            }

            await CargarCategorias();
            return View(evento);
        }

        // --- 4. EDITAR (Vista) ---
        public async Task<IActionResult> Edit(long id)
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login", "Auth");

            var evento = await _api.GetEventoAsync(id);
            if (evento == null) return NotFound();

            await CargarCategorias();
            return View(evento);
        }

        // --- 5. EDITAR (Procesar) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Evento evento, IFormFile? imagenFlyer)
        {
            if (id != evento.IdEvento) return BadRequest();

            ModelState.Remove(nameof(evento.FlyerUrl));

            if (ModelState.IsValid)
            {
                // A. Si suben NUEVA foto, la actualizamos. Si no, mantenemos la vieja (que viene en el hidden input)
                if (imagenFlyer != null && imagenFlyer.Length > 0)
                {
                    var url = await _api.SubirImagenAsync(imagenFlyer);
                    if (url != null) evento.FlyerUrl = url;
                }

                // B. Guardar cambios
                bool exito = await _api.EditarEventoAsync(id, evento);
                if (exito) return RedirectToAction(nameof(Index));

                ModelState.AddModelError("", "Error al actualizar.");
            }

            await CargarCategorias();
            return View(evento);
        }

        // --- 6. ELIMINAR ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login", "Auth");

            await _api.EliminarEventoAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --- AYUDANTE: Cargar Categorías para el Dropdown ---
        private async Task CargarCategorias()
        {
            var lista = await _api.GetCategoriasAsync();
            // "IdCategoria" es el valor que se guarda, "Nombre" es el texto que se ve
            ViewBag.Categorias = new SelectList(lista, "IdCategoria", "Nombre");
        }
    }
}