using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIGRE_PYME.Data;
using SIGRE_PYME.Helpers;
using SIGRE_PYME.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la conexión DefaultConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Usuario/Login");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();

    // await context.Database.MigrateAsync();

    if (!context.Usuarios.Any())
    {
        var admin = new Usuario
        {
            NombreUsuario = "admin",
            RolId = RolesSistema.AdminId,
            Bloqueado = false,
            IntentosFallidos = 0
        };
        admin.Contrasena = hasher.HashPassword(admin, "Admin123*");

        var gerente = new Usuario
        {
            NombreUsuario = "gerente",
            RolId = RolesSistema.GerenteId,
            Bloqueado = false,
            IntentosFallidos = 0
        };
        gerente.Contrasena = hasher.HashPassword(gerente, "Gerente123*");

        var vendedor = new Usuario
        {
            NombreUsuario = "vendedor",
            RolId = RolesSistema.VendedorId,
            Bloqueado = false,
            IntentosFallidos = 0
        };
        vendedor.Contrasena = hasher.HashPassword(vendedor, "Vendedor123*");

        var almacenista = new Usuario
        {
            NombreUsuario = "almacenista",
            RolId = RolesSistema.AlmacenistaId,
            Bloqueado = false,
            IntentosFallidos = 0
        };
        almacenista.Contrasena = hasher.HashPassword(almacenista, "Almacenista123*");

        var cliente = new Usuario
        {
            NombreUsuario = "cliente",
            RolId = RolesSistema.ClienteId,
            Bloqueado = false,
            IntentosFallidos = 0
        };
        cliente.Contrasena = hasher.HashPassword(cliente, "Cliente123*");

        context.Usuarios.AddRange(admin, gerente, vendedor, almacenista, cliente);
        await context.SaveChangesAsync();
    }
}

await app.RunAsync();