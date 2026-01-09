using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebZootecPro.Data;
using WebZootecPro.ViewModels.Admin;

namespace WebZootecPro.Controllers
{
  [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
  public class AdminController : Controller
  {
    private readonly ZootecContext _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AdminController(ZootecContext context,
                           IPasswordHasher<Usuario> passwordHasher)
    {
      _context = context;
      _passwordHasher = passwordHasher;
    }

    public async Task<IActionResult> Index()
    {
      var usuarioId = Int32.Parse(User.Claims.ToArray()[0].Value);
      var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
      var empresas = await (
          from e in _context.Empresas
          where e.usuarioID == usuarioId || (usuario != null && usuario.RolId == 1)
          join u in _context.Usuarios on e.usuarioID equals u.Id
          join r in _context.Rols on u.RolId equals r.Id
          join c in _context.Colaboradors on u.Id equals c.idUsuario into colGroup
          from c in colGroup.DefaultIfEmpty()
          join esp in _context.Especialidads on c.EspecialidadId equals esp.Id into espGroup
          from esp in espGroup.DefaultIfEmpty()
          select new EmpresaAdminIndexViewModel
          {
            Id = e.Id,
            NombreEmpresa = e.NombreEmpresa,
            Ruc = e.ruc,
            Ubicacion = e.ubicacion,
            CapacidadMaxima = e.capacidadMaxima,

            DuenoUserName = u.nombreUsuario,
            DuenoNombre = u.nombre,
            DuenoRol = r.Nombre,

            DniColaborador = c != null ? c.DNI : null,
            EspecialidadColaborador = esp != null ? esp.Nombre : null
          }
      ).ToListAsync();

      return View(empresas);
    }

    [HttpGet]
    public async Task<IActionResult> EditDuenoEmpresa(int id)
    {
      var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == id);
      if (empresa == null)
      {
        ModelState.AddModelError("UserName", "El dueno de la empresa no existe");
        return Redirect("/Admin/Index");
      }
      var duenoEmpresa = await _context.Usuarios.Where(u => u.Id == empresa.usuarioID).FirstOrDefaultAsync();
      if (duenoEmpresa == null)
      {
        ModelState.AddModelError("", "La empresa no tiene colaborador asociado");
        return Redirect("/Admin/Index");
      }
      var colaborador = await _context.Colaboradors.FirstOrDefaultAsync(c => c.idUsuario == duenoEmpresa.Id);

      if (colaborador == null)
      {
        ModelState.AddModelError("", "La empresa no tiene colaborador asociado");
        return Redirect("/Admin/Index");
      }
      var vm = new EditDuenoEmpresaViewModel()
      {
        UserName = duenoEmpresa.nombreUsuario,
        NombrePersona = duenoEmpresa.nombre,
        Dni = colaborador.DNI,
        CodigoPostal = colaborador.CodigoPostal,
        Apellido = colaborador.Apellido,
        Direccion = colaborador.Direccion,
        Provincia = colaborador.Provincia,
        Localidad = colaborador.Localidad,
        Telefono = colaborador.Telefono,
        IdEspecialidad = colaborador.EspecialidadId,
        NombreEmpresa = empresa.NombreEmpresa,
        Ruc = empresa.ruc,
        CapacidadMaxima = empresa.capacidadMaxima ?? 0,
        AreaTotal = empresa.areaTotal ?? 0,
        Ubicacion = empresa.ubicacion
      };

      // cargar combo de especialidades desde la tabla Especialidad
      vm.Especialidades = await _context.Especialidads
          .Select(e => new SelectListItem
          {
            Value = e.Id.ToString(),
            Text = e.Nombre
          })
          .ToListAsync();

      return View(vm);
    }
    [HttpPost]
    public async Task<IActionResult> EditDuenoEmpresa(int id, EditDuenoEmpresaViewModel model)
    {
      // 1) Validación de modelo
      if (!ModelState.IsValid)
      {
        model.Especialidades = await _context.Especialidads
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = e.Nombre
            })
            .ToListAsync();

        return View(model);
      }

      // 3) Rol para el dueño
      var rolAdminEmpresa = await _context.Rols
          .FirstOrDefaultAsync(r => r.Nombre == "ADMIN_EMPRESA");

      if (rolAdminEmpresa == null)
      {
        ModelState.AddModelError(string.Empty,
            "No existe el rol ADMIN_EMPRESA en la tabla Rol.");

        model.Especialidades = await _context.Especialidads
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = e.Nombre
            })
            .ToListAsync();

        return View(model);
      }
      await using var tx = await _context.Database.BeginTransactionAsync();
      var empresa = await _context.Empresas.FirstAsync(e => e.Id == id);
      var usuario = await _context.Usuarios.FirstAsync(u => u.Id == empresa.usuarioID);

      try
      {
        usuario.nombreUsuario = model.UserName;
        usuario.nombre = model.NombrePersona;
        usuario.RolId = rolAdminEmpresa.Id;
        usuario.idEstablo = null;
        usuario.idHato = null;
        if (model.Password != "No hay cambios")
        {
          usuario.contrasena = _passwordHasher.HashPassword(usuario, model.Password);
        }

        // 4.2) Empresa (ya sin Campo / Clave)
        empresa.Id = id;
        empresa.NombreEmpresa = model.NombreEmpresa;
        empresa.ruc = model.Ruc;
        empresa.capacidadMaxima = model.CapacidadMaxima;
        empresa.areaTotal = model.AreaTotal;
        empresa.ubicacion = model.Ubicacion;
        empresa.usuario = usuario;

        var colaboradorId = await _context.Colaboradors.Where(c => c.EmpresaId == id && c.idUsuario == usuario.Id).Select(e => e.Id).FirstAsync();
        // 4.3) Colaborador (dueño como colaborador)
        var colaborador = new Colaborador
        {
          Id = colaboradorId,
          nombre = model.NombrePersona,
          Apellido = model.Apellido,
          DNI = model.Dni,

          Direccion = model.Direccion,
          Provincia = model.Provincia,
          Localidad = model.Localidad,
          CodigoPostal = model.CodigoPostal,
          Telefono = model.Telefono,

          EspecialidadId = model.IdEspecialidad,
          idUsuarioNavigation = usuario,
          Empresa = empresa
        };


        _context.Update(usuario);
        _context.Update(empresa);
        _context.Update(colaborador);

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        TempData["AdminMessage"] = "Dueño, empresa y colaborador editados correctamente.";
        return RedirectToAction(nameof(Index));
      }
      catch (DbUpdateException ex)
      {
        await tx.RollbackAsync();

        var msg = ex.InnerException?.Message ?? ex.Message;
        ModelState.AddModelError(string.Empty, $"Error al guardar: {msg}");

        model.Especialidades = await _context.Especialidads
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = e.Nombre
            })
            .ToListAsync();

        return View(model);
      }

    }

    [HttpGet]
    public async Task<IActionResult> CrearDuenoEmpresa()
    {
      var vm = new CrearDuenoEmpresaViewModel();

      // cargar combo de especialidades desde la tabla Especialidad
      vm.Especialidades = await _context.Especialidads
          .Select(e => new SelectListItem
          {
            Value = e.Id.ToString(),
            Text = e.Nombre
          })
          .ToListAsync();

      return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearDuenoEmpresa(CrearDuenoEmpresaViewModel model)
    {
      // 1) Validación de modelo
      if (!ModelState.IsValid)
      {
        model.Especialidades = await _context.Especialidads
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = e.Nombre
            })
            .ToListAsync();

        return View(model);
      }

      // 2) Usuario duplicado
      bool existeUsuario = await _context.Usuarios
          .AnyAsync(u => u.nombreUsuario == model.UserName);

      if (existeUsuario)
      {
        ModelState.AddModelError(nameof(model.UserName), "El usuario ya existe.");

        model.Especialidades = await _context.Especialidads
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = e.Nombre
            })
            .ToListAsync();

        return View(model);
      }

      // 3) Rol para el dueño
      var rolAdminEmpresa = await _context.Rols
          .FirstOrDefaultAsync(r => r.Nombre == "ADMIN_EMPRESA");

      if (rolAdminEmpresa == null)
      {
        ModelState.AddModelError(string.Empty,
            "No existe el rol ADMIN_EMPRESA en la tabla Rol.");

        model.Especialidades = await _context.Especialidads
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = e.Nombre
            })
            .ToListAsync();

        return View(model);
      }

      await using var tx = await _context.Database.BeginTransactionAsync();

      try
      {
        // 4.1) Usuario dueño
        var usuario = new Usuario
        {
          nombreUsuario = model.UserName,
          nombre = model.NombrePersona,
          RolId = rolAdminEmpresa.Id,
          idEstablo = null,
          idHato = null
        };

        usuario.contrasena = _passwordHasher.HashPassword(usuario, model.Password);

        // 4.2) Empresa (ya sin Campo / Clave)
        var empresa = new Empresa
        {
          NombreEmpresa = model.NombreEmpresa,
          ruc = model.Ruc,
          capacidadMaxima = model.CapacidadMaxima,
          areaTotal = model.AreaTotal,
          ubicacion = model.Ubicacion,
          usuario = usuario      // navegación, rellena usuarioID
        };

        // 4.3) Colaborador (dueño como colaborador)
        var colaborador = new Colaborador
        {
          nombre = model.NombrePersona,
          Apellido = model.Apellido,
          DNI = model.Dni,

          Direccion = model.Direccion,
          Provincia = model.Provincia,
          Localidad = model.Localidad,
          CodigoPostal = model.CodigoPostal,
          Telefono = model.Telefono,

          EspecialidadId = model.IdEspecialidad,
          idUsuarioNavigation = usuario,
          Empresa = empresa
        };


        _context.Add(usuario);
        _context.Add(empresa);
        _context.Add(colaborador);

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        TempData["AdminMessage"] = "Dueño, empresa y colaborador creados correctamente.";
        return RedirectToAction(nameof(Index));
      }
      catch (DbUpdateException ex)
      {
        await tx.RollbackAsync();

        var msg = ex.InnerException?.Message ?? ex.Message;
        ModelState.AddModelError(string.Empty, $"Error al guardar: {msg}");

        model.Especialidades = await _context.Especialidads
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = e.Nombre
            })
            .ToListAsync();

        return View(model);
      }
    }
    [HttpGet]
    public async Task<IActionResult> IndexRoles()
    {
      var roles = new List<CrearRolViewModel>();
      foreach (var rol in await _context.Rols.ToListAsync())
      {
        roles.Add(new CrearRolViewModel()
        {
          Id = rol.Id,
          Nombre = rol.Nombre
        });
      }
      return View(roles);
    }

    [HttpGet]
    public IActionResult CrearRoles()
    {
      var vm = new CrearRolViewModel();
      return View(vm);
    }
    [HttpPost]
    public async Task<IActionResult> CrearRoles(CrearRolViewModel model)
    {
      bool existsRol = await _context.Rols.Select(rol => rol.Nombre)
          .Where(rol => rol == model.Nombre).AnyAsync();

      if (existsRol)
      {
        return View(model);
      }

      await _context.Rols.AddAsync(new Rol()
      {
        Nombre = model.Nombre.ToUpper()
      });
      await _context.SaveChangesAsync();
      return Redirect("/Admin/IndexRoles");
    }

    [HttpGet]
    public async Task<IActionResult> EditRol(int id)
    {
      var rol = await _context.Rols.FindAsync(id);
      if (rol == null) return NotFound();

      return View(new CrearRolViewModel
      {
        Nombre = rol.Nombre
      });
    }

    [HttpPost]
    public async Task<IActionResult> EditRol(int id, CrearRolViewModel model)
    {
      if (!ModelState.IsValid) return View(model);

      var rol = await _context.Rols.FindAsync(id);
      if (rol == null) return NotFound();

      rol.Nombre = model.Nombre.ToUpper();
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(IndexRoles));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRol(int id)
    {
      var rol = await _context.Rols.FindAsync(id);
      if (rol == null) return NotFound();

      _context.Rols.Remove(rol);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(IndexRoles));
    }
    [HttpGet]
    public async Task<IActionResult> IndexEspecialidades()
    {
      var especialidades = new List<CrearEspecialidadesViewModel>();
      foreach (var especialidad in await _context.Especialidads.ToListAsync())
      {
        especialidades.Add(new CrearEspecialidadesViewModel()
        {
          Id = especialidad.Id,
          Nombre = especialidad.Nombre
        });
      }
      return View(especialidades);
    }

    [HttpGet]
    public IActionResult CrearEspecialidad()
    {
      var vm = new CrearEspecialidadesViewModel();
      return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> CrearEspecialidad(CrearEspecialidadesViewModel model)
    {
      bool existsEspecialidad = await _context.Especialidads.Select(esp => esp.Nombre)
          .Where(esp => esp == model.Nombre).AnyAsync();
      if (existsEspecialidad)
      {
        View(model);
      }
      await _context.Especialidads.AddAsync(new Especialidad()
      {
        Nombre = model.Nombre.ToUpper()
      });
      await _context.SaveChangesAsync();
      return Redirect("/Admin/IndexEspecialidades");
    }

    [HttpGet]
    public async Task<IActionResult> EditEspecialidad(int id)
    {
      var esp = await _context.Especialidads.FindAsync(id);
      if (esp == null) return NotFound();

      return View(new CrearEspecialidadesViewModel
      {
        Nombre = esp.Nombre
      });
    }

    [HttpPost]
    public async Task<IActionResult> EditEspecialidad(int id, CrearEspecialidadesViewModel model)
    {
      if (!ModelState.IsValid) return View(model);

      var esp = await _context.Especialidads.FindAsync(id);
      if (esp == null) return NotFound();

      esp.Nombre = model.Nombre.ToUpper();
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(IndexEspecialidades));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEspecialidad(int id)
    {
      var esp = await _context.Especialidads.FindAsync(id);
      if (esp == null) return NotFound();

      _context.Especialidads.Remove(esp);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(IndexEspecialidades));
    }
    [HttpGet]
    public async Task<IActionResult> IndexTipoEnfermedad()
    {
      var model = new List<CrearTipoEnfermedadesViewModel>();
      foreach (var tipo in await _context.TipoEnfermedades.ToListAsync())
      {
        model.Add(new CrearTipoEnfermedadesViewModel()
        {
          Id = tipo.Id,
          Nombre = tipo.nombre
        });
      }
      return View(model);
    }

    [HttpGet]
    public IActionResult CrearTipoEnfermedades()
    {
      var model = new CrearTipoEnfermedadesViewModel();
      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CrearTipoEnfermedades(CrearTipoEnfermedadesViewModel model)
    {
      bool existsTipoEnfermedad = await _context.Especialidads.Select(esp => esp.Nombre)
          .Where(esp => esp == model.Nombre).AnyAsync();
      if (existsTipoEnfermedad)
      {
        View(model);
      }
      await _context.TipoEnfermedades.AddAsync(new TipoEnfermedade()
      {
        nombre = model.Nombre.ToUpper()
      });
      await _context.SaveChangesAsync();
      return Redirect("/Admin/IndexTipoEnfermedad");
    }

    [HttpGet]
    public async Task<IActionResult> EditTipoEnfermedad(int id)
    {
      var tipo = await _context.TipoEnfermedades.FindAsync(id);
      if (tipo == null) return NotFound();

      return View(new CrearTipoEnfermedadesViewModel
      {
        Nombre = tipo.nombre
      });
    }

    [HttpPost]
    public async Task<IActionResult> EditTipoEnfermedad(int id, CrearTipoEnfermedadesViewModel model)
    {
      if (!ModelState.IsValid) return View(model);

      var tipo = await _context.TipoEnfermedades.FindAsync(id);
      if (tipo == null) return NotFound();

      tipo.nombre = model.Nombre.ToUpper();
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(IndexTipoEnfermedad));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTipoEnfermedad(int id)
    {
      var tipo = await _context.TipoEnfermedades.FindAsync(id);
      if (tipo == null) return NotFound();

      _context.TipoEnfermedades.Remove(tipo);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(IndexTipoEnfermedad));
    }
    [HttpGet]
    public async Task<IActionResult> IndexSintoma()
    {
      var tipoEnferemedadDictionary = new Dictionary<int, string>();
      foreach (var enfermedades in await _context.TipoEnfermedades.Select(tip => tip).ToListAsync())
      {
        tipoEnferemedadDictionary.Add(enfermedades.Id, enfermedades.nombre);
      }
      ViewBag.IdTipoEnfermedad = tipoEnferemedadDictionary;
      var model = new List<CrearSintomaViewModel>();
      foreach (var tipo in await _context.Sintomas.Select(esp => esp).ToListAsync())
      {
        model.Add(new CrearSintomaViewModel()
        {
          Id = tipo.Id,
          Nombre = tipo.nombre,
          IdEnfermedad = tipo.idTipoEnfermedad
        });
      }
      return View(model);
    }
    [HttpGet]
    public IActionResult CrearSintoma()
    {
      var model = new CrearSintomaViewModel()
      {
        Enfermedades = _context.TipoEnfermedades
                    .Select(e => new SelectListItem
                    {
                      Value = e.Id.ToString(),
                      Text = e.nombre
                    })
                    .ToList()
      };
      return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> CrearSintoma(CrearSintomaViewModel model)
    {
      bool existsSintoma = await _context.Sintomas.Select(esp => esp.nombre)
          .Where(esp => esp == model.Nombre).AnyAsync();
      if (existsSintoma)
      {
        View(model);
      }
      await _context.Sintomas.AddAsync(new Sintoma()
      {
        nombre = model.Nombre.ToUpper(),
        idTipoEnfermedad = model.IdEnfermedad
      });
      await _context.SaveChangesAsync();
      return Redirect("/Admin/IndexSintoma");
    }

    [HttpGet]
    public async Task<IActionResult> EditSintoma(int id)
    {
      var sintoma = await _context.Sintomas.FindAsync(id);
      if (sintoma == null) return NotFound();

      var model = new CrearSintomaViewModel
      {
        Nombre = sintoma.nombre,
        IdEnfermedad = sintoma.idTipoEnfermedad,
        Enfermedades = await _context.TipoEnfermedades
              .Select(e => new SelectListItem
              {
                Value = e.Id.ToString(),
                Text = e.nombre
              })
              .ToListAsync()
      };

      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditSintoma(int id, CrearSintomaViewModel model)
    {
      if (!ModelState.IsValid)
      {
        model.Enfermedades = await _context.TipoEnfermedades
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = e.nombre
            })
            .ToListAsync();

        return View(model);
      }

      var sintoma = await _context.Sintomas.FindAsync(id);
      if (sintoma == null) return NotFound();

      sintoma.nombre = model.Nombre.ToUpper();
      sintoma.idTipoEnfermedad = model.IdEnfermedad;

      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(IndexSintoma));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSintoma(int id)
    {
      var sintoma = await _context.Sintomas.FindAsync(id);
      if (sintoma == null) return NotFound();

      _context.Sintomas.Remove(sintoma);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(IndexSintoma));
    }
    [HttpGet]
    public async Task<IActionResult> IndexTipoTratamiento()
    {
      var tipoEnferemedadDictionary = new Dictionary<int, string>();
      foreach (var enfermedades in await _context.TipoEnfermedades.Select(tip => tip).ToListAsync())
      {
        tipoEnferemedadDictionary.Add(enfermedades.Id, enfermedades.nombre);
      }
      ViewBag.IdTipoEnfermedad = tipoEnferemedadDictionary;
      var model = new List<CrearTipoTratamientoViewModel>();
      foreach (var tipo in await _context.TipoTratamientos.Select(esp => esp).ToListAsync())
      {
        model.Add(new CrearTipoTratamientoViewModel()
        {
          IdTipoTratamiento = tipo.Id,
          Nombre = tipo.nombre,
          Costo = tipo.costo ?? 0,
          Cantidad = (int)(tipo.cantidad ?? 0),
          Unidad = tipo.unidad ?? "No posee unidad",
          IdTipoEnfermedad = tipo.idTipoEnfermedad
        });
      }
      return View(model);
    }

    [HttpGet]
    public IActionResult CrearTipoTratamiento()
    {
      var model = new CrearTipoTratamientoViewModel()
      {
        Enfermedades = _context.TipoEnfermedades
                    .Select(e => new SelectListItem
                    {
                      Value = e.Id.ToString(),
                      Text = e.nombre
                    })
                    .ToList()
      };

      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CrearTipoTratamiento(CrearTipoTratamientoViewModel model)
    {
      bool existTipoTratamiento = await _context.TipoTratamientos.Select(esp => esp.nombre)
          .Where(esp => esp == model.Nombre).AnyAsync();
      if (existTipoTratamiento)
      {
        View(model);
      }
      await _context.TipoTratamientos.AddAsync(new TipoTratamiento()
      {
        nombre = model.Nombre,
        costo = model.Costo,
        cantidad = model.Cantidad,
        unidad = model.Unidad,
        idTipoEnfermedad = model.IdTipoEnfermedad,
        retiroLecheDias = model.RetiroLecheDias

      });
      await _context.SaveChangesAsync();
      return Redirect("/Admin/IndexTipoTratamiento");
    }
    [HttpGet]
    public async Task<IActionResult> EditTipoTratamiento(int id)
    {
      var tipo = await _context.TipoTratamientos.FindAsync(id);
      if (tipo == null)
        return NotFound();

      var model = new CrearTipoTratamientoViewModel
      {
        Nombre = tipo.nombre,
        Costo = tipo.costo ?? 0,
        Cantidad = (int)(tipo.cantidad ?? 0),
        Unidad = tipo.unidad ?? "",
        IdTipoEnfermedad = tipo.idTipoEnfermedad,
        Enfermedades = _context.TipoEnfermedades
              .Select(e => new SelectListItem
              {
                Value = e.Id.ToString(),
                Text = e.nombre
              })
              .ToList()
      };

      ViewBag.Id = id;
      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTipoTratamiento(int id, CrearTipoTratamientoViewModel model)
    {
      if (!ModelState.IsValid)
      {
        model.Enfermedades = _context.TipoEnfermedades
            .Select(e => new SelectListItem
            {
              Value = e.Id.ToString(),
              Text = e.nombre
            })
            .ToList();

        ViewBag.Id = id;
        return View(model);
      }

      var tipo = await _context.TipoTratamientos.FindAsync(id);
      if (tipo == null)
        return NotFound();

      tipo.nombre = model.Nombre;
      tipo.costo = model.Costo;
      tipo.cantidad = model.Cantidad;
      tipo.unidad = model.Unidad;
      tipo.idTipoEnfermedad = model.IdTipoEnfermedad;

      _context.TipoTratamientos.Update(tipo);
      await _context.SaveChangesAsync();

      return Redirect("/Admin/IndexTipoTratamiento");
    }

    [HttpGet]
    public async Task<IActionResult> DeleteTipoTratamiento(int id)
    {
      var tipo = await _context.TipoTratamientos.FindAsync(id);
      if (tipo == null)
        return NotFound();

      var model = new CrearTipoTratamientoViewModel
      {
        Nombre = tipo.nombre,
        Costo = tipo.costo ?? 0,
        Cantidad = (int)(tipo.cantidad ?? 0),
        Unidad = tipo.unidad ?? ""
      };

      ViewBag.Id = id;
      return View(model);
    }

    private int? GetCurrentUserId()
    {
      var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
      return int.TryParse(idStr, out var id) ? id : (int?)null;
    }

    private async Task<int?> GetEmpresaIdDelAdminAsync()
    {
      if (User.IsInRole("SUPERADMIN"))
        return null; // superadmin ve todo

      var userId = GetCurrentUserId();
      if (userId == null) return null;

      return await _context.Empresas
          .Where(e => e.usuarioID == userId.Value)
          .Select(e => (int?)e.Id)
          .FirstOrDefaultAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Personal()
    {
      var empresaId = await GetEmpresaIdDelAdminAsync();

      var q = _context.Colaboradors
          .AsNoTracking()
          .Include(c => c.Especialidad)
          .Include(c => c.idUsuarioNavigation)
          .Where(c =>
c.Especialidad.Nombre == "VETERINARIO" ||
c.Especialidad.Nombre == "INSPECTOR" ||
c.Especialidad.Nombre == "USUARIO_EMPRESA" ||
c.Especialidad.Nombre == "LABORATORISTA");


      if (!User.IsInRole("SUPERADMIN"))
      {
        if (empresaId == null) return View(new List<PersonalIndexItemViewModel>());
        q = q.Where(c => c.EmpresaId == empresaId.Value);
      }

      var data = await q
          .OrderBy(c => c.Especialidad.Nombre)
          .ThenBy(c => c.nombre)
          .Select(c => new PersonalIndexItemViewModel
          {
            ColaboradorId = c.Id,
            Tipo = c.Especialidad.Nombre,
            UserName = c.idUsuarioNavigation.nombreUsuario,
            Nombre = c.nombre,
            Apellido = c.Apellido,
            DNI = c.DNI,
            Telefono = c.Telefono
          })
          .ToListAsync();

      return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> CrearPersonal()
    {
      var vm = new CrearPersonalViewModel();

      if (User.IsInRole("SUPERADMIN"))
      {
        vm.Empresas = await _context.Empresas
            .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.NombreEmpresa })
            .ToListAsync();

        // SUPERADMIN: se llena por JS cuando elija Empresa
        vm.Establos = new List<SelectListItem>();
      }
      else
      {
        var empresaId = await GetEmpresaIdDelAdminAsync();
        if (empresaId != null)
          vm.Establos = await GetEstablosSelectAsync(empresaId.Value);
      }

      return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearPersonal(CrearPersonalViewModel model)
    {
      var empresaIdDelAdmin = await GetEmpresaIdDelAdminAsync();

      int? empresaAsignada = null;

      if (User.IsInRole("SUPERADMIN"))
      {
        if (model.EmpresaId == null)
          ModelState.AddModelError(nameof(model.EmpresaId), "Seleccione la empresa.");
        else
          empresaAsignada = model.EmpresaId.Value;
      }
      else
      {
        if (empresaIdDelAdmin == null)
          ModelState.AddModelError(string.Empty, "No se pudo determinar la empresa del administrador.");
        else
          empresaAsignada = empresaIdDelAdmin.Value;
      }

      // ✅ Establo requerido
      if (model.EstabloId == null)
        ModelState.AddModelError(nameof(model.EstabloId), "Seleccione el establo.");

      // ✅ Establo debe pertenecer a la empresa
      if (empresaAsignada != null && model.EstabloId != null)
      {
        var establoOk = await _context.Establos
            .AnyAsync(e => e.Id == model.EstabloId.Value && e.EmpresaId == empresaAsignada.Value);

        if (!establoOk)
          ModelState.AddModelError(nameof(model.EstabloId), "El establo no pertenece a la empresa seleccionada.");
      }

      // ✅ Usuario duplicado
      if (!string.IsNullOrWhiteSpace(model.UserName))
      {
        var existeUsuario = await _context.Usuarios
            .AnyAsync(u => u.nombreUsuario == model.UserName);

        if (existeUsuario)
          ModelState.AddModelError(nameof(model.UserName), "El usuario ya existe.");
      }

      // ✅ DNI duplicado
      if (!string.IsNullOrWhiteSpace(model.DNI))
      {
        var existeDni = await _context.Colaboradors
            .AnyAsync(c => c.DNI == model.DNI);

        if (existeDni)
          ModelState.AddModelError(nameof(model.DNI), "El DNI ya existe.");
      }

      // Si hay errores, recargar combos y volver a la vista
      if (!ModelState.IsValid)
      {
        if (User.IsInRole("SUPERADMIN"))
        {
          model.Empresas = await _context.Empresas
              .Select(e => new SelectListItem
              {
                Value = e.Id.ToString(),
                Text = e.NombreEmpresa
              })
              .ToListAsync();

          model.Establos = model.EmpresaId != null
              ? await GetEstablosSelectAsync(model.EmpresaId.Value)
              : new List<SelectListItem>();
        }
        else
        {
          model.Establos = empresaAsignada != null
              ? await GetEstablosSelectAsync(empresaAsignada.Value)
              : new List<SelectListItem>();
        }

        return View(model);
      }

      // Rol / Especialidad
      var rolNombre = model.Tipo == "LABORATORISTA" ? "LABORATORIO_EMPRESA" : model.Tipo;

      var rol = await _context.Rols.FirstOrDefaultAsync(r => r.Nombre == rolNombre);
      if (rol == null)
      {
        rol = new Rol { Nombre = rolNombre };
        _context.Rols.Add(rol);
        await _context.SaveChangesAsync();
      }

      var esp = await _context.Especialidads.FirstOrDefaultAsync(e => e.Nombre == model.Tipo);
      if (esp == null)
      {
        esp = new Especialidad { Nombre = model.Tipo };
        _context.Especialidads.Add(esp);
        await _context.SaveChangesAsync();
      }

      await using var tx = await _context.Database.BeginTransactionAsync();
      try
      {
        var usuario = new Usuario
        {
          nombreUsuario = model.UserName,
          nombre = model.Nombre,
          RolId = rol.Id,
          idEstablo = model.EstabloId!.Value,
          idHato = null
        };

        usuario.contrasena = _passwordHasher.HashPassword(usuario, model.Password);

        var colaborador = new Colaborador
        {
          nombre = model.Nombre,
          Apellido = model.Apellido,
          DNI = model.DNI,
          Direccion = model.Direccion,
          Provincia = model.Provincia,
          Localidad = model.Localidad,
          CodigoPostal = model.CodigoPostal,
          Telefono = model.Telefono,

          EspecialidadId = esp.Id,
          idUsuarioNavigation = usuario,
          // empresaAsignada viene sí o sí aquí por las validaciones
          EmpresaId = empresaAsignada!.Value
        };

        _context.Add(usuario);
        _context.Add(colaborador);

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToAction(nameof(Personal));
      }
      catch (DbUpdateException ex)
      {
        await tx.RollbackAsync();
        ModelState.AddModelError(string.Empty, ex.InnerException?.Message ?? ex.Message);

        // recargar combos para que la vista no reviente
        if (User.IsInRole("SUPERADMIN"))
        {
          model.Empresas = await _context.Empresas
              .Select(e => new SelectListItem
              {
                Value = e.Id.ToString(),
                Text = e.NombreEmpresa
              })
              .ToListAsync();

          model.Establos = model.EmpresaId != null
              ? await GetEstablosSelectAsync(model.EmpresaId.Value)
              : new List<SelectListItem>();
        }
        else
        {
          model.Establos = empresaAsignada != null
              ? await GetEstablosSelectAsync(empresaAsignada.Value)
              : new List<SelectListItem>();
        }

        return View(model);
      }
    }


    [HttpGet]
    public async Task<IActionResult> PersonalDetalles(int id)
    {
      var empresaId = await GetEmpresaIdDelAdminAsync();

      var q = _context.Colaboradors
          .AsNoTracking()
          .Include(c => c.Especialidad)
          .Include(c => c.idUsuarioNavigation)
          .Where(c => c.Id == id)
         .Where(c =>
c.Especialidad.Nombre == "VETERINARIO" ||
c.Especialidad.Nombre == "USUARIO_EMPRESA" ||
c.Especialidad.Nombre == "INSPECTOR" ||
c.Especialidad.Nombre == "LABORATORISTA");


      if (!User.IsInRole("SUPERADMIN"))
      {
        if (empresaId == null) return NotFound();
        q = q.Where(c => c.EmpresaId == empresaId.Value);
      }

      var c = await q.FirstOrDefaultAsync();
      if (c == null) return NotFound();

      // reutilizamos el mismo VM de edición (sirve para mostrar)
      var vm = new EditarPersonalViewModel
      {
        Id = c.Id,
        Tipo = c.Especialidad.Nombre,
        UserName = c.idUsuarioNavigation?.nombreUsuario ?? "",
        Nombre = c.nombre,
        Apellido = c.Apellido,
        DNI = c.DNI,
        Direccion = c.Direccion,
        Provincia = c.Provincia,
        Localidad = c.Localidad,
        CodigoPostal = c.CodigoPostal,
        Telefono = c.Telefono
      };

      return View(vm);
    }
    [HttpGet]
    public async Task<IActionResult> EditarPersonal(int id)
    {
      var empresaId = await GetEmpresaIdDelAdminAsync();

      var q = _context.Colaboradors
          .AsNoTracking()
          .Include(c => c.Especialidad)
          .Include(c => c.idUsuarioNavigation)
          .Where(c => c.Id == id)
          .Where(c =>
              c.Especialidad.Nombre == "VETERINARIO" ||
              c.Especialidad.Nombre == "USUARIO_EMPRESA" ||
              c.Especialidad.Nombre == "INSPECTOR" ||
              c.Especialidad.Nombre == "LABORATORISTA");

      if (!User.IsInRole("SUPERADMIN"))
      {
        if (empresaId == null) return NotFound();
        q = q.Where(c => c.EmpresaId == empresaId.Value);
      }

      var c = await q.FirstOrDefaultAsync();
      if (c == null) return NotFound();

      var vm = new EditarPersonalViewModel
      {
        Id = c.Id,
        Tipo = c.Especialidad.Nombre,
        UserName = c.idUsuarioNavigation?.nombreUsuario ?? "",
        Nombre = c.nombre,
        Apellido = c.Apellido,
        DNI = c.DNI,
        Direccion = c.Direccion,
        Provincia = c.Provincia,
        Localidad = c.Localidad,
        CodigoPostal = c.CodigoPostal,
        Telefono = c.Telefono,

        // ✅ establo actual del usuario
        EstabloId = c.idUsuarioNavigation?.idEstablo
      };

      // ✅ combo establos de la empresa del colaborador
      vm.Establos = await GetEstablosSelectAsync(c.EmpresaId);

      return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPersonal(EditarPersonalViewModel model)
    {
      // no retornes directo: necesitas recargar combo
      var empresaId = await GetEmpresaIdDelAdminAsync();

      var q = _context.Colaboradors
          .Include(c => c.Especialidad)
          .Include(c => c.idUsuarioNavigation)
          .Where(c => c.Id == model.Id)
          .Where(c =>
              c.Especialidad.Nombre == "VETERINARIO" ||
              c.Especialidad.Nombre == "USUARIO_EMPRESA" ||
              c.Especialidad.Nombre == "INSPECTOR" ||
              c.Especialidad.Nombre == "LABORATORISTA");

      if (!User.IsInRole("SUPERADMIN"))
      {
        if (empresaId == null) return NotFound();
        q = q.Where(c => c.EmpresaId == empresaId.Value);
      }

      var c = await q.FirstOrDefaultAsync();
      if (c == null) return NotFound();

      // recargar combo siempre (por si hay error)
      model.Establos = await GetEstablosSelectAsync(c.EmpresaId);

      if (model.EstabloId == null)
        ModelState.AddModelError(nameof(model.EstabloId), "Seleccione el establo.");

      if (model.EstabloId != null)
      {
        var establoOk = await _context.Establos
            .AnyAsync(e => e.Id == model.EstabloId.Value && e.EmpresaId == c.EmpresaId);

        if (!establoOk)
          ModelState.AddModelError(nameof(model.EstabloId), "El establo no pertenece a la empresa del colaborador.");
      }

      if (!ModelState.IsValid)
        return View(model);

      // Validación DNI duplicado (igual que tu lógica)
      var dniDuplicado = await _context.Colaboradors
          .AnyAsync(x => x.DNI == model.DNI && x.Id != model.Id);

      if (dniDuplicado)
      {
        ModelState.AddModelError(nameof(model.DNI), "El DNI ya existe.");
        return View(model);
      }

      // Actualiza Colaborador
      c.nombre = model.Nombre;
      c.Apellido = model.Apellido;
      c.DNI = model.DNI;
      c.Direccion = model.Direccion;
      c.Provincia = model.Provincia;
      c.Localidad = model.Localidad;
      c.CodigoPostal = model.CodigoPostal;
      c.Telefono = model.Telefono;


      // ✅ actualizar establo del Usuario
      if (c.idUsuarioNavigation != null)
      {
        c.idUsuarioNavigation.nombre = model.Nombre;
        c.idUsuarioNavigation.idEstablo = model.EstabloId!.Value;
      }

      /* if (!model.EstabloId.HasValue || model.EstabloId.Value <= 0)
       {
           ModelState.AddModelError(nameof(model.EstabloId), "Seleccione el establo.");
       }*/

      try
      {
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Personal));
      }
      catch (DbUpdateException ex)
      {
        ModelState.AddModelError(string.Empty, ex.InnerException?.Message ?? ex.Message);
        return View(model);
      }
    }


    private async Task<List<SelectListItem>> GetEstablosSelectAsync(int? empresaId)
    {
      if (!empresaId.HasValue)
        return new List<SelectListItem>();

      int id = empresaId.Value;

      return await _context.Establos
          .AsNoTracking()
          .Where(e => e.EmpresaId == id)
          .OrderBy(e => e.nombre)
          .Select(e => new SelectListItem
          {
            Value = e.Id.ToString(),
            Text = e.nombre
          })
          .ToListAsync();
    }


    [HttpGet]
    public async Task<IActionResult> GetEstablosPorEmpresa(int empresaId)
    {
      // seguridad: ADMIN_EMPRESA solo puede pedir de su empresa
      var empresaIdDelAdmin = await GetEmpresaIdDelAdminAsync();
      if (!User.IsInRole("SUPERADMIN") && empresaIdDelAdmin != empresaId)
        return Forbid();

      var data = await _context.Establos
          .AsNoTracking()
          .Where(e => e.EmpresaId == empresaId)
          .OrderBy(e => e.nombre)
          .Select(e => new { id = e.Id, nombre = e.nombre })
          .ToListAsync();

      return Json(data);
    }

  }

}

