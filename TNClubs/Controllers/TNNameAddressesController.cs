/*
   TODO:
   5.	Catch any exception that is thrown on Create or Edit, place its innermost message into ModelState, and allow processing to continue to the sad path, which should redisplay the user’s data, along with the error.
   6.	For Delete, put the innermost exception’s message into TempData and return to the Delete view.
   7.	If the insert, update or delete works, display a success message on the Member Index page via TempData.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TNClubs.Models;

namespace TNClubs.Controllers
{
    public class TNNameAddressesController : Controller
    {
        private readonly ClubsContext _context;

        public TNNameAddressesController(ClubsContext context)
        {
            _context = context;
        }
        
        // GET: TNNameAddresses
        public async Task<IActionResult> Index()
        {
            var clubsContext = await _context.NameAddress.Include(n => n.ProvinceCodeNavigation).ToListAsync();

            for (int clubIndex = 0; clubIndex < clubsContext.Count; clubIndex++)
            {
                if(!String.IsNullOrEmpty(clubsContext[clubIndex].LastName) && !String.IsNullOrEmpty(clubsContext[clubIndex].FirstName))
                {
                    clubsContext[clubIndex].FirstName = $"{clubsContext[clubIndex].LastName} , {clubsContext[clubIndex].FirstName}";
                }else if(String.IsNullOrEmpty(clubsContext[clubIndex].FirstName) && string.IsNullOrEmpty(clubsContext[clubIndex].LastName))
                {
                    clubsContext[clubIndex].FirstName = $"{clubsContext[clubIndex].LastName}";
                }
            }
            return View(clubsContext);
        }

        // GET: TNNameAddresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nameAddress = await _context.NameAddress
                .Include(n => n.ProvinceCodeNavigation)
                .FirstOrDefaultAsync(m => m.NameAddressId == id);
            if (nameAddress == null)
            {
                return NotFound();
            }

            return View(nameAddress);
        }

        // GET: TNNameAddresses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TNNameAddresses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NameAddressId,FirstName,LastName,CompanyName,StreetAddress,City,PostalCode,ProvinceCode,Email,Phone")] NameAddress nameAddress)
        {
            if (ModelState.IsValid)
            {
                TempData["Message"] = "Successfully created new record";
                _context.Add(nameAddress);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nameAddress);
        }

        // GET: TNNameAddresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nameAddress = await _context.NameAddress.FindAsync(id);
            if (nameAddress == null)
            {
                return NotFound();
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province.OrderBy(p=>p.Name), "ProvinceCode", "Name", nameAddress.ProvinceCode);
            return View(nameAddress);
        }

        // POST: TNNameAddresses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NameAddressId,FirstName,LastName,CompanyName,StreetAddress,City,PostalCode,ProvinceCode,Email,Phone")] NameAddress nameAddress)
        {
            if (id != nameAddress.NameAddressId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nameAddress);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Successfully edited record";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NameAddressExists(nameAddress.NameAddressId))
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
            ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "ProvinceCode", nameAddress.ProvinceCode);
            return View(nameAddress);
        }

        // GET: TNNameAddresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nameAddress = await _context.NameAddress
                .Include(n => n.ProvinceCodeNavigation)
                .FirstOrDefaultAsync(m => m.NameAddressId == id);
            if (nameAddress == null)
            {
                return NotFound();
            }

            return View(nameAddress);
        }

        // POST: TNNameAddresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                
                var nameAddress = await _context.NameAddress.FindAsync(id);
                _context.NameAddress.Remove(nameAddress);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Delete Successful";
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                TempData["Message"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool NameAddressExists(int id)
        {
            return _context.NameAddress.Any(e => e.NameAddressId == id);
        }
    }
}
