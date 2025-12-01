using NawiWebAdmin.Services; // Importante

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar MVC
builder.Services.AddControllersWithViews();

// 2. REGISTRAR NUESTRO SERVICIO
builder.Services.AddHttpClient<NawiApiService>();
builder.Services.AddHttpContextAccessor(); // Necesario para leer la sesión

// 3. ACTIVAR SESIONES (Memoria para guardar el Token)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); // Duración de la sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ... Configuración por defecto ...
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. ¡ORDEN OBLIGATORIO! Sesión antes de Auth
app.UseSession();
app.UseAuthorization();

// Cambiamos la ruta por defecto para que vaya al Login primero
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();