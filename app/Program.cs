using Microsoft.EntityFrameworkCore;
using HMS.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using HMS.Models;
using HMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("AccessTokenCookie")
    .AddCookie("AccessTokenCookie", options =>
    {
        options.Cookie.Name = "UserAuthCookie";
        options.LoginPath = "/auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(3);
        options.SlidingExpiration = false;
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email")
);
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<EncryptionService>();

// Configure MySQL DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Port=3306;Database=hms_db;Uid=root;Pwd=password;"; // Placeholder
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString,
        new MySqlServerVersion(new Version(8, 0, 21)))); // Specify your MySQL version

var app = builder.Build();

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

app.UseAuthentication(); // Add this line
app.UseAuthorization();


app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
