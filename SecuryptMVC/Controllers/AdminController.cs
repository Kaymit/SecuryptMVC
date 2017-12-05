using SecuryptMVC.DAL;
using SecuryptMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SecuryptMVC.Controllers
{
    public class AdminController : Controller
    {
		private ApplicationUserManager _userManager;
		private ApplicationRoleManager _roleManager;
		private ApplicationDbContext dbID = new ApplicationDbContext();

		// GET: Admin
		[Authorize(Roles = "Administrator")]
		public async Task<ActionResult> Index(string searchString)
        {
			if (searchString == null)
				searchString = "";

			var users = await dbID.Users.Where(x => x.UserName.Contains(searchString)).ToListAsync();

			//async execute query
			//List<EncryptedItem> items = await queryUsers.ToListAsync();

			return View("~/Views/Admin/Index.cshtml", users);
        }

        // GET: Admin/Details/5
        public ActionResult ListUsers(int id)
        {
            return View();
        }

		/*
        public ActionResult Delete(int id)
        {
            return View();
        }
		*/

        // POST: Admin/Delete/5
        [HttpPost]
		[Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
				var userToDelete = (from user in _userManager.Users
									where user.Id.Equals(id)
									select user).Single();
				// If we could not find the user, throw an exception
				if (userToDelete == null)
				{
					ViewBag.errorMessage = "User not found";
					return View("Error");
				}

				await DeleteUser(userToDelete);
				return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

		private async Task DeleteUser(ApplicationUser user)
		{
			// If we could not find the user, throw an exception
			if (user == null)
			{
				throw new Exception("Could not find the User");
			}
			var roleList = await _userManager.GetRolesAsync(user.Id);
			var roleArray = roleList.ToArray();
			await _userManager.RemoveFromRolesAsync(user.Id, roleArray);
			await _userManager.UpdateAsync(user);
			await _userManager.DeleteAsync(user);
			return;
		}
	}
}
