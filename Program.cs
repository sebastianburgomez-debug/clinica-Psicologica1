using Psicologia.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar el soporte para controladores y vistas MVC
builder.Services.AddControllersWithViews();

// 2. Registrar la Inyección de Dependencias como Singleton
// (Usamos Singleton para que los datos agregados o eliminados se mantengan vivos en memoria mientras corre el servidor)
builder.Services.AddSingleton<ISpecialtyRepository, SpecialtyRepository>();
builder.Services.AddSingleton<IRoleRepository, RoleRepository>();

var app = builder.Build();

// 3. Configurar el canal de solicitudes HTTP (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Habilita archivos estáticos de Bootstrap/CSS en wwwroot

app.UseRouting();

app.UseAuthorization();

// 4. Configurar la ruta MVC por defecto
// Al abrir la app, cargará automáticamente el CRUD de Especialidades (Specialty/Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Specialty}/{action=Index}/{id?}");

app.Run();
