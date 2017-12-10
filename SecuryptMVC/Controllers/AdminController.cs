using Microsoft.AspNet.Identity.Owin;
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
    /// <summary>
    /// Some Administration funcationality included such as list users, search users, and delete user
    /// </summary>
    /// <author> 
    /// Kevin Mitchell 29/11/2017
    /// Michael O'Connel-Graf 4-9/12/2017
    /// </author>
    public class AdminController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationDbContext dbID;

        public AdminController()
        {
            _userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            dbID = new ApplicationDbContext();
        }

		/// <summary>
        /// GET: Returns a list of all users to admin users
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
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

        /// <summary>
        /// POST: Attempts to delete a user via the UserManager
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Helper method to asyncronously find User and Roles to delete
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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
