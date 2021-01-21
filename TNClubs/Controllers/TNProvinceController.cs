/* Name: Timothy Nigh
 * Student Number: 7438336
 * Email: tnigh8336@conestogac.on.ca
 * File: TNProvinceController.cs
 * Purpose: Controller for the Province views
 * Rev History:
 *      Created 24 Sept 2020
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TNClubs.Models;

namespace TNClubs.Controllers
{
    public class TNProvinceController : Controller
    {
        private readonly ClubsContext _context;

        public TNProvinceController(ClubsContext context)
        {
            _context = context;
        }

        // GET: TNProvince
        public async Task<IActionResult> Index(string id,string name)
        {
            //[x]If the CountryCode is in the URL or a QueryString variable, save it to a cookie or session variable.
            if(id != null)
            {
                HttpContext.Session.SetString("CountryCode",id);
            }
            else
            {
                //[X]. If there is no CountryCode passed in the URL or QueryString, see if there is a cookie or session variable with it.
                if(String.IsNullOrEmpty(HttpContext.Session.GetString("CountryCode")))
                {
                    TempData["Message"] = "Please Select a Country";
                    //[X]. If there’s no CountryCode in the cookie or session variables either, return to the XXCountryController with a message asking them to select a country.
                    return View("Views/TNCountry/Index.cshtml", await _context.Country.ToListAsync());
                }
            }
            //[X]. Regardless of where you got the CountryCode from, use it to filter the listing to only show provinces on file for that country
            //  [X]. Order the listing by name
            var clubsContext = _context.Province.Where(p => p.CountryCode == HttpContext.Session.GetString("CountryCode")).OrderBy(p=>p.Name);

            //[X].  … otherwise, fetch the record from the country table, then extract & persist its name
            if(String.IsNullOrEmpty(name))
            {
                Country country = await _context.Country.Where(x=>x.CountryCode == HttpContext.Session.GetString("CountryCode")).FirstOrDefaultAsync();
                name = country.Name;
            }
            
            // If the country name had also been passed, persist it the same way.
            HttpContext.Session.SetString("CountryName", name);


            return View(await clubsContext.ToListAsync());
        }

        // GET: TNProvince/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province
                .Include(p => p.CountryCodeNavigation)
                .FirstOrDefaultAsync(m => m.ProvinceCode == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        // GET: TNProvince/Create
        public IActionResult Create()
        {
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode");
            return View();
        }

        // POST: TNProvince/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProvinceCode,Name,CountryCode,SalesTaxCode,SalesTax,IncludesFederalTax,FirstPostalLetter")] Province province)
        {
            string errorMessage = "";
            var provinceName = await _context.Province
                                    .Where(p=>p.Name == province.Name)
                                    .FirstOrDefaultAsync();
            var provinceCode = await _context.Province
                                    .Where(p=>p.ProvinceCode == province.ProvinceCode)
                                    .FirstOrDefaultAsync();
            //Checking to ensure that no province exists with the name or code provided
            if (ModelState.IsValid)
            {
            
                if(provinceName != null)
                {
                    errorMessage += $"The province name {provinceName.Name.ToString()} already exists ";
                    
                }

                if (provinceCode != null)
                {
                    errorMessage += $"The province code {provinceName.ProvinceCode.ToString()} already exists";
                }
                else
                {
                    _context.Add(province);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));   
                }
            }
            TempData["Message"] = errorMessage;
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode", province.CountryCode);
            return View(province);                       
        }

        // GET: TNProvince/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province.FindAsync(id);
            if (province == null)
            {
                return NotFound();
            }
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode", province.CountryCode);
            return View(province);
        }

        // POST: TNProvince/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ProvinceCode,Name,CountryCode,SalesTaxCode,SalesTax,IncludesFederalTax,FirstPostalLetter")] Province province)
        {
            if (id != province.ProvinceCode)
            {
                return NotFound();
            }
            //Get provinces with the same name as the submitted province
            var provinceCheck =  await _context.Province.Where(p=>p.Name == province.Name).AsNoTracking().FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                //check to see if a) we got a province back and b) if that province macs
                if (provinceCheck != null && provinceCheck.ProvinceCode != id)
                {
                    TempData["Message"] = $"The province name {provinceCheck.Name.ToString()} already exists ";
                }
                else
                {
                    try
                    {
                        _context.Update(province);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProvinceExists(province.ProvinceCode))
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
            }
            ViewData["CountryCode"] = new SelectList(_context.Country, "CountryCode", "CountryCode", province.CountryCode);
            return View(province);
        }

        // GET: TNProvince/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Province
                .Include(p => p.CountryCodeNavigation)
                .FirstOrDefaultAsync(m => m.ProvinceCode == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        // POST: TNProvince/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var province = await _context.Province.FindAsync(id);
            _context.Province.Remove(province);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProvinceExists(string id)
        {
            return _context.Province.Any(e => e.ProvinceCode == id);
        }
    }
}
