using BloggingApp.Data;
using BloggingApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog to log console and daily write to rolling files
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("/logs/app-logs-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30) //rollingInterval = Everyday creates a new file by attaching date to the filename, retainedFileCountLimit = After reaching the specified count old files will be deleted
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDistributedMemoryCache();

// Adding DbContext Services using SqlServer
builder.Services.AddDbContext<ApplicationDbContext>((options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
));

// Adding Identity services
// Option - 1 : using AddDefaultIdentity method
// This method comes with pre-built UI pages
// As this method creates all the necessary razor view pages for Authentication operations like login, register, logout etc
// We have to add role manager explicitly by adding AddRoleManager method through method chaining
//builder.Services.AddDefaultIdentity<IdentityUser>(options =>
//{
//    options.Password.RequiredLength = 8;
//    options.Password.RequireDigit = true;
//    options.Password.RequireLowercase = true;
//    options.Password.RequireUppercase = true;
//    options.Password.RequireNonAlphanumeric = true;
//    options.Lockout.MaxFailedAccessAttempts = 5;
//}).AddRoleManager<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

//Option - 2 : using AddIdentity method 
// This method requires custom razor pages creation for login, register, logout, password change etc
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
}).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Cookie Policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;

    options.SlidingExpiration = true;
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthorization();
// Add services to the container.
builder.Services.AddControllersWithViews();


var app = builder.Build();

app.UseSerilogRequestLogging();

// Seed admin user data to db
using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;

    try
    {
        await AdminUserInitializer.SeedAdminUserAsync(services);
    }
    catch(Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the Identity Database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCookiePolicy();

app.UseAuthentication();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Post}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
