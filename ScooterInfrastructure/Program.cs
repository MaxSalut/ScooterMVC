using ScooterInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using ScooterDomain.Model;

var builder = WebApplication.CreateBuilder(args);

// Налаштування локалізації
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("uk-UA"),
        new CultureInfo("en-US")
    };
    options.DefaultRequestCulture = new RequestCulture("uk-UA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Налаштування Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ScootersContext>()
.AddDefaultTokenProviders();

// Налаштування аутентифікації через cookies
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

// Додавання контролерів та контексту бази даних
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ScootersContext>(option => option.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
));

var app = builder.Build();

// Ініціалізація ролей
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Список ролей для створення
    string[] roleNames = { "User", "Admin" }; // Додаємо обидві ролі, які вам можуть знадобитися

    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    
}

// Налаштування middleware
app.UseRequestLocalization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();