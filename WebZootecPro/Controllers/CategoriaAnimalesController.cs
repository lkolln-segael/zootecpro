using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebZootecPro.Data;

namespace WebZootecPro.Controllers
{
    [Authorize(Roles = "SUPERADMIN,ADMIN_EMPRESA")]
    public class CategoriaAnimalesController : Controller
    {
        private readonly ZootecContext _context;

        public CategoriaAnimalesController(ZootecContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var items = _context.CategoriaAnimals.OrderBy(c => c.Nombre).ToListAsync();

            return View(items);
        }

    }
}
