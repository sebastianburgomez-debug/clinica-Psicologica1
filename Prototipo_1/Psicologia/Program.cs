using Microsoft.EntityFrameworkCore;
using Psicologia.Data;
using Psicologia.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Soporte para controladores y vistas MVC
builder.Services.AddControllersWithViews();

// 2. Base de datos (SQL Server / LocalDB). La cadena está en appsettings.json
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Repositorios. Deben ser Scoped porque dependen del DataContext (que es Scoped).
builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

var app = builder.Build();

// 4. Crear la base de datos (y los datos iniciales) si todavía no existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.EnsureCreated();
}

// 5. Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 6. Ruta por defecto: abre el CRUD de Especialidades
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
