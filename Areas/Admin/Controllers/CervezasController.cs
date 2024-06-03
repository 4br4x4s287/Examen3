using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelWeb.Data;
using HotelWeb.Models;
using HotelWeb.Data.Migrations;
using Microsoft.AspNetCore.Authorization;

namespace HotelWeb.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class CervezasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostenvironment;
        public CervezasController(ApplicationDbContext context, IWebHostEnvironment hostenvironment)
        {
            _context = context;
            _hostenvironment = hostenvironment;
        }

        // GET: Admin/Cervezas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Cervezas.Include(c => c.Estilo);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/Cervezas/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cerveza = await _context.Cervezas
                .Include(c => c.Estilo)
                .FirstOrDefaultAsync(m => m.id == id);
            if (cerveza == null)
            {
                return NotFound();
            }

            return View(cerveza);
        }

        // GET: Admin/Cervezas/Create
        public IActionResult Create()
        {
            ViewData["idEstilo"] = new SelectList(_context.Estilos, "id", "nombre");
            return View();
        }

        // POST: Admin/Cervezas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,nombre,alcohol,idEstilo,precio, UrlImagen")] Cerveza cerveza)
        {
            if (ModelState.IsValid)
            {
                string rutaPrincipal = _hostenvironment.WebRootPath;
                var archivos = HttpContext.Request.Form.Files;
                if (archivos.Count > 0)
                {
                    string nombreArchivo= Guid.NewGuid ().ToString();
                    var subidas = Path.Combine(rutaPrincipal, @"imagenes\cervezas");
                    var extencion = Path.GetExtension(archivos[0].FileName);
                    using (var fileStream = new FileStream(Path.Combine(subidas, nombreArchivo + extencion), FileMode.Create))
                    {
                        archivos[0].CopyTo(fileStream);
                    }
                    cerveza.UrlImagen = @"imagen\cervezas\" + nombreArchivo + extencion;
                }
                _context.Add(cerveza);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["idEstilo"] = new SelectList(_context.Estilos, "id", "nombre", cerveza.idEstilo);
            return View(cerveza);
        }

        // GET: Admin/Cervezas/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cerveza = await _context.Cervezas.FindAsync(id);
            if (cerveza == null)
            {
                return NotFound();
            }
            ViewData["idEstilo"] = new SelectList(_context.Estilos, "id", "nombre", cerveza.idEstilo);
            return View(cerveza);
        }

        // POST: Admin/Cervezas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("id,nombre,alcohol,idEstilo,precio,Urlimagen")] Cerveza cerveza)
        {
            if (id != cerveza.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string rutaPrincipal = _hostenvironment.WebRootPath;
                    var archivos = HttpContext.Request.Form.Files;
                    if (archivos.Count > 0)
                    {
                        Cerveza? cervezasabd = await _context.Cervezas.FindAsync(id);
                        if (cervezasabd != null && cervezasabd.UrlImagen != null)
                        {
                            var rutaImagenActual = Path.Combine(rutaPrincipal, cervezasabd.UrlImagen);
                            if (System.IO.File.Exists(rutaImagenActual))
                            {
                                System.IO.File.Delete(rutaImagenActual);    
                            }
                            _context.Entry(cervezasabd).State = EntityState.Detached;
                        }
                        string nombreArchivo = Guid.NewGuid().ToString();
                        var subidas = Path.Combine(rutaPrincipal, @"imagenes\cervezas");
                        var extencion = Path.GetExtension(archivos[0].FileName);
                        using (var fileStream = new FileStream(Path.Combine(subidas, nombreArchivo + extencion), FileMode.Create))
                        {
                            archivos[0].CopyTo(fileStream);
                        }
                        cerveza.UrlImagen = @"imagen\cervezas\" + nombreArchivo + extencion;
                        _context.Entry(cerveza).State = EntityState.Modified;
                    }
                        _context.Update(cerveza);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CervezaExists(cerveza.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["idEstilo"] = new SelectList(_context.Estilos, "id", "nombre", cerveza.idEstilo);
            return View(cerveza);
        }

        // GET: Admin/Cervezas/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cerveza = await _context.Cervezas
                .Include(c => c.Estilo)
                .FirstOrDefaultAsync(m => m.id == id);
            if (cerveza == null)
            {
                return NotFound();
            }

            return View(cerveza);
        }

        // POST: Admin/Cervezas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var cerveza = await _context.Cervezas.FindAsync(id);
            if (cerveza != null)
            {
                _context.Cervezas.Remove(cerveza);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CervezaExists(string id)
        {
            return _context.Cervezas.Any(e => e.id == id);
        }
    }
}
