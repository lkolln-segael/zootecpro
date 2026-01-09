using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Usuario;

namespace WebZootecPro.Controllers
{
  public class AccountController : Controller
  {
    private readonly ZootecContext _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AccountController(ZootecContext context, IPasswordHasher<Usuario> passwordHasher)
    {
      _context = context;
      _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public IActionResult Index()
    {
      return View();
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
      var rolesNecesarios = new[] {
              "SUPERADMIN","ADMIN_EMPRESA","USUARIO_EMPRESA",
              "LABORATORIO_EMPRESA","INSPECTOR","VETERINARIO"
            };

      var existentes = await _context.Rols
          .Select(r => r.Nombre)
          .ToListAsync();

      var faltantes = rolesNecesarios
          .Where(r => !existentes.Contains(r))
          .Select(r => new Rol { Nombre = r })
          .ToList();

      if (faltantes.Count > 0)
      {
        _context.Rols.AddRange(faltantes);
        await _context.SaveChangesAsync();
      }

      return View(new LoginViewModel { ReturnUrl = returnUrl });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
      if (!ModelState.IsValid)
        return View(model);

      // IMPORTANTE: incluir la navegación rol
      var user = await _context.Usuarios
          .Include(u => u.Rol)
          .FirstOrDefaultAsync(u => u.nombreUsuario == model.UserName);

      if (user == null)
      {
        ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
        return View(model);
      }

      var result = _passwordHasher.VerifyHashedPassword(user, user.contrasena, model.Password);

      if (result == PasswordVerificationResult.Failed)
      {
        ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
        return View(model);
      }

      // Claim de rol: nombre del rol desde la tabla Rol
      var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.nombreUsuario),
                new Claim(ClaimTypes.Role, user.Rol.Nombre)   // <-- AQUÍ
            };

      var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
      var principal = new ClaimsPrincipal(identity);

      await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

      if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        return Redirect(model.ReturnUrl);

      return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      return RedirectToAction("Login");
    }

    // SOLO SUPERADMIN PUEDE CREAR USUARIOS
    //[Authorize(Roles = "SUPERADMIN")]
    [HttpGet]
    public async Task<IActionResult> Register()
    {
      var vm = new RegisterUserViewModel();

      // llenar combo de roles desde la DB
      vm.Roles = await _context.Rols
          .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
          {
            Value = r.Id.ToString(),
            Text = r.Nombre
          })
          .ToListAsync();

      return View(vm);
    }

    //[Authorize(Roles = "SUPERADMIN")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterUserViewModel model)
    {
      if (!ModelState.IsValid)
      {
        model.Roles = await _context.Rols
            .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
              Value = r.Id.ToString(),
              Text = r.Nombre
            })
            .ToListAsync();

        return View(model);
      }

      var exists = await _context.Usuarios
          .AnyAsync(u => u.nombreUsuario == model.UserName);

      if (exists)
      {
        ModelState.AddModelError(nameof(model.UserName), "El usuario ya existe.");

        model.Roles = await _context.Rols
            .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
              Value = r.Id.ToString(),
              Text = r.Nombre
            })
            .ToListAsync();

        return View(model);
      }

      var usuario = new Usuario
      {
        nombreUsuario = model.UserName,
        nombre = model.Nombre,
        RolId = model.IdRol,        // <= usamos el id del rol
        idEstablo = model.IdEstablo,
        idHato = model.IdHato
      };

      usuario.contrasena = _passwordHasher.HashPassword(usuario, model.Password);

      _context.Usuarios.Add(usuario);
      await _context.SaveChangesAsync();

      return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
      return View();
    }
  }
}
