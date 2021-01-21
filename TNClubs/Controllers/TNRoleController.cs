/* Name: Timothy Nigh
 * Student Number: 7438336
 * Email: tnigh8336@conestogac.on.ca
 * File: TNRoleController.cs
 * Purpose: Controller for the Authentication
 * Rev History:
 *      Created 17 Nov 2020
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TNClubs.Controllers
{
    [Authorize(Roles = ADMIN_ROLE_NAME)]
    public class TNRoleController : Controller
    {
        const string ADMIN_ROLE_NAME = "administrators";
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        public TNRoleController(UserManager<IdentityUser> userManager,
                            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var roles = roleManager.Roles.OrderBy(a => a.Name);
            return View(roles);
        }
        [HttpPost]
        public async Task<IActionResult> Index(string? name)
        {
            /*
            3.Create an action to add a new role(no View):
                a.Stop the insert and return an error message to the role listing if:
                    i.The proposed role name is already on file.
                    ii.The proposed role name is null, empty or just blanks.
                b.In a try-catch, trim leading and trailing spaces before writing the new role to file.
                i.If the insert succeeds, return to the role listing and display a success message with the role name
                ii.If the insert fails, return to the role listing and display the exception’s innermost message, or the first message from IdentityResult.
            */
            bool errorOut = false;
            if (!string.IsNullOrEmpty(name))
            {

                var roles = roleManager.Roles.ToList();
                foreach (var item in roles)
                {
                    if (item.Name == name)
                    {
                        TempData["Message"] = "That role already exists";
                        errorOut = true;
                    }
                }
                if (!errorOut)
                {
                    name = name.Trim();

                    try
                    {
                        IdentityResult identity = await roleManager.CreateAsync(new IdentityRole(name));
                    }
                    catch (Exception ex)
                    {
                        //TODO: Add real logging. If you hit this block, you(the admin) needs to know what happened
                        TempData["Message"] = $"Ya done fucked up kid\r\n{ex.Message}";
                    }

                }
            }
            else
            {
                TempData["Message"] = "Please enter a valid name for the role";
            }
            var newRoles = roleManager.Roles.OrderBy(a => a.Name);
            return View(newRoles);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string name)
        {
            var role = await roleManager.FindByNameAsync(name);
            var users = await userManager.GetUsersInRoleAsync(name);
            
            if (name.ToLower() == ADMIN_ROLE_NAME)
            {
                TempData["Message"] = "You cannot delete the administrator role";
                return RedirectToAction("Index");
            }


            if (users.Count == 0)
            {

                try
                {
                    await roleManager.DeleteAsync(role);
                    TempData["Message"] = $"{name } deleted";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    //TODO: IMPLEMENT REAL LOGGING, YADA YADA YADA
                    TempData["Message"] = "Fatal error, please contact your system administator";
                    return RedirectToAction("Index");
                }
            }
            
            ViewBag.RoleName = name;
            return View(users);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            
                if (roleName == ADMIN_ROLE_NAME)
                {
                    TempData["message"] = "You cannot delete the admin role you dummy";
                    return View("Index");
                }
                else
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    await roleManager.DeleteAsync(role);
                    TempData["Message"] = $"{roleName} deleted";
                    var currentRoles = roleManager.Roles.OrderBy(a => a.Name);
                    return View("Index", currentRoles);
                }
                
        }
        public async Task<IActionResult> Members(string? role)
        {
            //Get all users in role
            var usersInRole = await userManager.GetUsersInRoleAsync(role);
            //Get all users
            var allUsers = userManager.Users;
            var allUsersDupe = userManager.Users.ToList();
            //Derive all users not in the role
            foreach (var user in allUsers)
            {
                if (usersInRole.Contains(user))
                {
                    allUsersDupe.Remove(user);
                }
            }
            usersInRole = usersInRole.OrderBy(u => u.UserName).ToList();
            ViewBag.Role = role;
            ViewData["UsersNotInRole"] = new SelectList(allUsersDupe, "Id", "Email");
            return View(usersInRole);
            /*
            b.Add a hyperlink & action on each line to remove individual users from the role.  Users get confused easily, so don’t use the word “delete”.
                i.Do no allow the current user to remove themselves from the administrators role
                c.Add a drop - down showing all users not in the role(sorted, of course), with a button to add the selected user to the role.You know how to list users in a role and you how to list all users on file … so …
                d.Display a message if the add or remove succeed or, if they fail, show a message with the innermost exception’s message or the IdentityResult’s message.
            */

        }
        [HttpPost]
        public async Task<IActionResult> Members(string Id, string role)
        {
            if (!String.IsNullOrEmpty(Id))
            {
                IdentityUser user = await userManager.FindByIdAsync(Id);
                IdentityResult addResult = await userManager.AddToRoleAsync(user, role);

                TempData["Message"] = $"{user.Email} added to role";
            }
            else
            {
                TempData["Message"] = "No user selected";
            }

            return RedirectToAction("Members", new { role = role });
        }

        public async Task<IActionResult> Remove(string Id, string role)
        {
            string userName = User.Identity.Name;
            if (!String.IsNullOrEmpty(Id))
            {
                IdentityUser user = await userManager.FindByIdAsync(Id);
                if(user.UserName == userName && role == ADMIN_ROLE_NAME)
                {
                    TempData["Message"] = "You can't remove yourself from the admin role dummy";
                    return RedirectToAction("Members", new { role = role });
                }

                try
                {
                    IdentityResult removeUser = await userManager.RemoveFromRoleAsync(user, role);
                    TempData["Message"] = $"{user.UserName} removed from {role}";
                }
                catch (Exception)
                {
                    //TODO: Add real logging here. If you hit this block, 
                    //something is jacked up. Fix it
                    TempData["Message"] = "Something went very wrong on the delete";
                    return View("Index", "TNRoleController");
                }
            }
            else
            {
                TempData["Message"] = "No user selected";
            }

            return RedirectToAction("Members", new { role = role });
        }


    }
}
