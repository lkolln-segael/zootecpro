using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ZootecContext>(options =>
{
  options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
      options.LoginPath = "/Account/Login";
      options.AccessDeniedPath = "/Account/AccessDenied";
    });


builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();


builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

app.Run();
