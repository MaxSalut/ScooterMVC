using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Додано для авторизації
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScooterDomain.Model;
using ScooterInfrastructure;

namespace ScooterInfrastructure.Controllers
{
    public class DiscountsController : Controller
    {
        private readonly ScootersContext _context;

        public DiscountsController(ScootersContext context)
        {
            _context = context;
        }

        // GET: Discounts
        [Authorize(Roles = "User,Admin")] // Доступ для User і Admin
        public async Task<IActionResult> Index()
        {
            return View(await _context.Discounts.ToListAsync());
        }

        // GET: Discounts/Details/5
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _context.Discounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }

        // GET: Discounts/Create
        [Authorize(Roles = "Admin")] // Лише для Admin
        public IActionResult Create()
        {
            return View();
        }

        // POST: Discounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> Create([Bind("Name,Percentage,Description,Id")] Discount discount)
        {
            if (ModelState.IsValid)
            {
                _context.Add(discount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(discount);
        }

        // GET: Discounts/Edit/5
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }
            return View(discount);
        }

        // POST: Discounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> Edit(int id, [Bind("Name,Percentage,Description,Id")] Discount discount)
        {
            if (id != discount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(discount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiscountExists(discount.Id))
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
            return View(discount);
        }

        // GET: Discounts/Delete/5
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _context.Discounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }

        // POST: Discounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount != null)
            {
                _context.Discounts.Remove(discount);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DiscountExists(int id)
        {
            return _context.Discounts.Any(e => e.Id == id);
        }
    }
}