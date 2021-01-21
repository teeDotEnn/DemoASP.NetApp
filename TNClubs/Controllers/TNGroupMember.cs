using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TNClubs.Models;

namespace TNClubs.Controllers
{
    public class TNGroupMember : Controller
    {
        private readonly ClubsContext _context;

        public TNGroupMember(ClubsContext context)
        {
            _context = context;
        }

        // GET: TNGroupMember
        public async Task<IActionResult> Index(string? artistId, string? fullName )
        {
            int nArtistId = 0;
            //If the artistId is in the URL or a QueryString variable,
            //save it to a cookie or session variable.
            if (!string.IsNullOrEmpty(artistId))
            {
                HttpContext.Session.SetString("ArtistId", artistId);
            }
            else if(!string.IsNullOrEmpty(HttpContext.Session.GetString("ArtistId")))
            {
                //If no artistId was passed in the URL or QueryString, 
                //look for a cookie or session variable with it, and use that.
                artistId = HttpContext.Session.GetString(artistId);
                
            }
            else
            {
                //If there’s no artistId in either a cookie or session variable, 
                //return to the XXArtistController with a message asking them to select an artist.
                TempData["Message"] = "Please select an artist";
                return RedirectToAction("Index", "TNArtists");
            }

            if(!int.TryParse(artistId, out nArtistId))
            {
                TempData["Message"] = "Invalid artist ID: not a number";
                return RedirectToAction("Index", "TNArtists");
            }

            /*
              [X].	If the artistId is in the ArtistIdGroup field of any groupMember record:
		      [X].	Filter the groupMember records to ones 
                    that have the given artistId in the field artistIdGroup.
		      [X].	Show the member’s full name, derived like in the artist controller.
             */
            var group = await _context.GroupMember.Include(g => g.ArtistIdGroupNavigation)
                                                    .Include(g => g.ArtistIdMemberNavigation)
                                                    .Include(g => g.ArtistIdMemberNavigation.NameAddress)
                                                    .Include(g=> g.ArtistIdGroupNavigation.NameAddress)
                                                    .Where(g=> g.ArtistIdGroup == nArtistId)
                                                    .OrderByDescending(g=> g.DateLeft)
                                                    .ThenBy(g=> g.DateJoined)
                                                    .ToListAsync();

            var member = await _context.GroupMember.Include(g => g.ArtistIdGroupNavigation)
                                                       .Include(g => g.ArtistIdMemberNavigation)
                                                       .Include(g=> g.ArtistIdMemberNavigation.NameAddress)
                                                       .Include(g => g.ArtistIdGroupNavigation.NameAddress)
                                                       .Where(g => g.ArtistIdMember == nArtistId)
                                                       .ToListAsync();


            if (group.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                if(!string.IsNullOrEmpty(group[0].ArtistIdGroupNavigation.NameAddress.FirstName))
                {
                    sb.Append(group[0].ArtistIdGroupNavigation.NameAddress.FirstName);
                }
                if (!string.IsNullOrEmpty(group[0].ArtistIdGroupNavigation.NameAddress.LastName))
                {
                    sb.Append(group[0].ArtistIdGroupNavigation.NameAddress.LastName);
                }
                
                ViewBag.groupName = sb.ToString();
                ViewBag.artistId = artistId;
                return View(group);
            }
            else if (member.Count != 0)
            {
                string firstName = member[0].ArtistIdMemberNavigation.NameAddress.FirstName;
                string lastName = member[0].ArtistIdMemberNavigation.NameAddress.LastName;
                /*
              Else, if the artistId is not a group, but is in the ArtistIdMember field on any groupMember records:
                 i.	Create a new view called GroupsForArtist, listing all groups the artist belonged to.
                 ii.Show each group’s name along with the member’s joined & left dates
                 [X].	Add a TempData message explaining the artist is an individual, not a group, so here’s their historic group memberships.
                 [X].	Show the artist’s full name in the page heading and browser title … something like “Groups with full name”
                 [X].	Put a hyperlink on each line sending the ArtistIdGroup back to the groupMember controller’s Index action so the user can edit the individual’s group membership.  
                vi.	This view does not need any other hyperlinks … except maybe one back to artist controller’s Index action?
                */
                TempData["Message"] = $"Here are all the groups {firstName} {lastName} is associated with";
                ViewBag.FullName = $"{firstName} {lastName}";

                return View("GroupsForArtist", member);

            }
            else
            {
                TempData["Message"] = "This person is not a group/group member, but you can create a group";
                return RedirectToAction("Create");
            }
            
        }

        // GET: TNGroupMember/Details/5
        public async Task<IActionResult> Details(int? artistId, string fullName)
        {
            if (artistId == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMember
                .Include(g => g.ArtistIdGroupNavigation)
                .Include(g => g.ArtistIdMemberNavigation)
                .Where(m=>m.ArtistIdMember == artistId)
                .FirstOrDefaultAsync();
            if (groupMember == null)
            {
                return NotFound();
            }
            
            
            var groupName = await _context.NameAddress
                                          .Where(a => a.NameAddressId ==
                                          groupMember.ArtistIdGroupNavigation.NameAddressid)
                                          .FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(groupName.LastName))
            {
                ViewBag.GroupName
                    = groupName.FirstName;
            }
            else
            {
                ViewBag.GroupName
                    = groupName.LastName + ", "
                    + groupName.FirstName;
            }
            ViewBag.fullName = fullName;
            return View(groupMember);
        }
        /*
         * TODO
         *  For the Create and Edit views
         *      Display the artist’s fullName (as defined in ArtistController) and return their artistId
         * TODO
         */



        // GET: TNGroupMember/Create
        public IActionResult Create(string? artistGroupId)
        {
            //Remove artists that are already groups
            var artists = _context.Artist
                                 .Include(a => a.NameAddress)
                                 .ToList();
            var artistDupes = _context.Artist
                                 .Include(a => a.NameAddress)
                                 .ToList(); 
            foreach (var artist in artists)
            {
                var group = _context.GroupMember.Where(g => g.ArtistIdGroup == artist.ArtistId).FirstOrDefault();
                if (group != null)
                {
                    artistDupes.Remove(artist);
                }
                else
                {
                    //Remove artists that are already in groups
                    var member = _context.GroupMember
                                                .Where(g => g.ArtistIdMember == artist.ArtistId)
                                                .FirstOrDefault();
                    if (member != null && member.DateLeft == null)
                    {
                        artistDupes.Remove(artist);
                    }
                }
            }
            //Generate the artist's full name
            Dictionary<string, string> artistIdFullNames = new Dictionary<string, string>();
            foreach (var artist in artistDupes)
            {
                artistIdFullNames.Add(artist.ArtistId.ToString(),$"{artist.NameAddress.FirstName} {artist.NameAddress.LastName}");
            }
    
            
            ViewBag.ArtistGroupId = artistGroupId;
            ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "ArtistId");
            ViewData["ArtistIdMember"] = new SelectList(artistIdFullNames, "Key","Value");
            return View();
        }

        // POST: TNGroupMember/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArtistIdGroup,ArtistIdMember,DateJoined,DateLeft")] GroupMember groupMember)
        {
            groupMember.DateJoined = DateTime.Today;
            groupMember.DateLeft = null;
            
            if (ModelState.IsValid)
            {
                _context.Add(groupMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            //Remove artists that are already groups

            var artists = _context.Artist
                                 .Include(a => a.NameAddress)
                                 .ToList();
            var artistDupes = artists;
            foreach (var artist in artists)
            {
                var group = _context.GroupMember.Where(g => g.ArtistIdGroup == artist.ArtistId).FirstOrDefault();
                if (group != null)
                {
                    artistDupes.Remove(artist);
                }
                else
                {
                    //Remove artists that are already in groups
                    var member = _context.GroupMember
                                                .Where(g => g.ArtistIdMember == artist.ArtistId)
                                                .FirstOrDefault();
                    if (member != null && member.DateLeft == null)
                    {
                        artistDupes.Remove(artist);
                    }
                }
            }
            
            //Generate the artist's full name
            Dictionary<string, string> artistIdFullNames = new Dictionary<string, string>();
            foreach (var artist in artistDupes)
            {
                artistIdFullNames.Add(artist.ArtistId.ToString(), $"{artist.NameAddress.FirstName} {artist.NameAddress.LastName}");
            }


            ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "ArtistId", groupMember.ArtistIdGroup);
            ViewData["ArtistIdMember"] = new SelectList(artistIdFullNames, "Key", "Value", groupMember.ArtistIdGroup);
            return View(groupMember);
        }

        // GET: TNGroupMember/Edit/5
        public async Task<IActionResult> Edit(int? artistId, string? fullName)
        {
            if (artistId == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMember.Where(g=>g.ArtistIdMember == artistId).FirstOrDefaultAsync();
            if (groupMember == null)
            {
                return NotFound();
            }

            //Remove artists that are already groups
            
            var artists = _context.Artist
                                 .Include(a => a.NameAddress)
                                 .ToList();
            var artistDupes = _context.Artist
                                 .Include(a => a.NameAddress)
                                 .ToList(); ;
            foreach (var artist in artists)
            {
                var group = _context.GroupMember.Where(g => g.ArtistIdGroup == artist.ArtistId).FirstOrDefault();
                if (group != null)
                {
                    artistDupes.Remove(artist);
                }
                else
                {
                    //Remove artists that are already in groups
                    var member = _context.GroupMember
                                                .Where(g => g.ArtistIdMember == artist.ArtistId)
                                                .FirstOrDefault();
                    if (member != null && member.DateLeft == null)
                    {
                        artistDupes.Remove(artist);
                    }
                }
            }
            //Generate the artist's full name
            List<string> membersFullName = new List<string>();
            foreach (var artist in artistDupes)
            {
                membersFullName.Add($"{artist.NameAddress.FirstName} {artist.NameAddress.LastName}");
            }

            ViewBag.fullName = fullName;
            ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "ArtistId", groupMember.ArtistIdGroup);
            ViewData["ArtistIdMember"] = new SelectList(_context.Artist, "ArtistId", "ArtistId", groupMember.ArtistIdMember);
            return View(groupMember);
        }

        // POST: TNGroupMember/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArtistIdGroup,ArtistIdMember,DateJoined,DateLeft")] GroupMember groupMember)
        {
            if (id != groupMember.ArtistIdGroup)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupMemberExists(groupMember.ArtistIdGroup))
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


            //Remove artists that are already groups

            var artists = _context.Artist
                                 .Include(a => a.NameAddress)
                                 .ToList();
            var artistDupes = artists;
            foreach (var artist in artists)
            {
                var group = _context.GroupMember.Where(g => g.ArtistIdGroup == artist.ArtistId).FirstOrDefault();
                if (group != null)
                {
                    artistDupes.Remove(artist);
                }
                else
                {
                    //Remove artists that are already in groups
                    var member = _context.GroupMember
                                                .Where(g => g.ArtistIdMember == artist.ArtistId)
                                                .FirstOrDefault();
                    if (member != null && member.DateLeft == null)
                    {
                        artistDupes.Remove(artist);
                    }
                }
            }
            //Generate the artist's full name
            List<string> membersFullName = new List<string>();
            foreach (var artist in artistDupes)
            {
                membersFullName.Add($"{artist.NameAddress.FirstName} {artist.NameAddress.LastName}");
            }

            ViewData["ArtistIdGroup"] = new SelectList(_context.Artist, "ArtistId", "ArtistId", groupMember.ArtistIdGroup);
            ViewData["ArtistIdMember"] = new SelectList(_context.Artist, "ArtistId", "ArtistId", groupMember.ArtistIdMember);
            return View(groupMember);
        }

        // GET: TNGroupMember/Delete/5
        public async Task<IActionResult> Delete(int? artistId, string? fullName)
        {
            if (artistId == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMember
                .Include(g => g.ArtistIdGroupNavigation)
                .Include(g => g.ArtistIdMemberNavigation)
                .FirstOrDefaultAsync(m => m.ArtistIdMember == artistId);
            if (groupMember == null)
            {
                return NotFound();
            }
            ViewBag.fullName = fullName;
            return View(groupMember);
        }

        // POST: TNGroupMember/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var groupMember = await _context.GroupMember.FindAsync(id);
            _context.GroupMember.Remove(groupMember);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupMemberExists(int id)
        {
            return _context.GroupMember.Any(e => e.ArtistIdGroup == id);
        }
    }
}
