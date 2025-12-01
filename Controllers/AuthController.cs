using Microsoft.AspNetCore.Mvc;
using NawiWebAdmin.Models;
using NawiWebAdmin.Services;

namespace NawiWebAdmin.Controllers
{
    public class AuthController : Controller
    {
        private readonly NawiApiService _api;

        public AuthController(NawiApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Si ya tiene sesión, lo mandamos directo adentro
            if (HttpContext.Session.GetString("JwtToken") != null)
                return RedirectToAction("Index", "Eventos");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Intentamos loguear contra la API
            if (await _api.LoginAsync(model.Username, model.Password))
            {
                return RedirectToAction("Index", "Eventos");
            }

            ModelState.AddModelError("", "Usuario o contraseña incorrectos");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JwtToken");
            return RedirectToAction("Login");
        }
    }
}