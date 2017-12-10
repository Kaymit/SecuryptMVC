using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SecuryptMVC.DAL;
using SecuryptMVC.Models;
using SecuryptMVC.Utility;
using Microsoft.AspNet.Identity;
using System.IO;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

//https://support.microsoft.com/en-us/help/323246/how-to-upload-a-file-to-a-web-server-in-asp-net-by-using-visual-c--net
namespace SecuryptMVC.Controllers
{
    public class EncryptedItemsController : Controller
    {
        /// <summary>
        /// Database context reference
        /// </summary>
        private FileContext db = new FileContext();
        /// <summary>
        /// ASP.NET Identity ApplicationDbContext reference
        /// </summary>
        ApplicationDbContext dbID = new ApplicationDbContext();
        /// <summary>
        /// CryptoHandler object reference
        /// </summary>
        CryptoHandler ch = new CryptoHandler();

        /// <summary>
        /// GET: displays user's accessible items
        /// </summary>
        /// <returns>list of all items user is permitted to view</returns>
        public async Task<ActionResult> Index()
        {
            string userID = User.Identity.GetUserId();

            //Query to list all items user is permitted by item owner to view, including user's own
            IQueryable<EncryptedItem> queryPermitted = from item in db.EncryptedItems
                            where item.PermittedUserIDsAsString.Contains(userID) || item.IsPrivate == false
                            select item;

            //async execute query
            List<EncryptedItem> items = await queryPermitted.ToListAsync();

            foreach(EncryptedItem item in items)
            {
                item.OwnerEmail = System.Web.HttpContext.Current.
                GetOwinContext().
                GetUserManager<ApplicationUserManager>().
                FindById(item.OwnerID).
                UserName;
            }

            return View(items);
        }

        /// <summary>
        /// Initiates decryption and download of file without saving decrypted file on server at any point
        /// </summary>
        /// <param name="id">ID of EncryptedItem held in database</param>
        /// <returns>FileStreamResult with decrypted item, in correct file format</returns>
        public FileStreamResult Download(int id)
        {
            //Register CryptoHandler and ensure keys are current
            ch.RegisterKeys();

            //get current User ID
            string ownerID = User.Identity.GetUserId();

            //query a single EncryptedItem whose ID equals the id of desired item,
            //only if public item or the user is permitted to access item
            EncryptedItem encryptedItem = (from item in db.EncryptedItems
                                           where (item.PermittedUserIDsAsString.Contains(ownerID) || item.IsPrivate == false)
                                                && item.ID == id
                                           select item).Single();

            if (encryptedItem == null)
            {
                //return HttpNotFound();
            }

            //get byte stream of decrypted bytes, return a FileStreamResult with
            //content type and FileStream
            string contentType = MimeMapping.GetMimeMapping(encryptedItem.Name);
            MemoryStream decryptedFileStream = ch.DecryptFile(encryptedItem.StorageLocation);

            return File(decryptedFileStream, contentType, encryptedItem.Name);      //construct and return filestream to client
        }

        /// <summary>
        /// GET: Returns a list of details about the Item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EncryptedItem encryptedItem = await db.EncryptedItems.FindAsync(id);
            if (encryptedItem == null)
            {
                return HttpNotFound();
            }

            //lookup owner's email and add to model before returning it
            encryptedItem.OwnerEmail = System.Web.HttpContext.Current.
                GetOwinContext().
                GetUserManager<ApplicationUserManager>().
                FindById(encryptedItem.OwnerID).
                UserName;
            return View(encryptedItem);
        }

        /// <summary>
        /// GET: Returns an edit view of the item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EncryptedItem encryptedItem = await db.EncryptedItems.FindAsync(id);
			string userID = User.Identity.GetUserId();
			if (encryptedItem.OwnerID != userID)
			{
				ViewBag.errorMessage = "Only the owner of this file may edit this file";
				return View("Error");
			}
			if (encryptedItem == null)
            {
                return HttpNotFound();
            }
            //lookup owner's email and add to model before returning it
            encryptedItem.OwnerEmail = System.Web.HttpContext.Current.
                GetOwinContext().
                GetUserManager<ApplicationUserManager>().
                FindById(encryptedItem.OwnerID).
                UserName;
            return View(encryptedItem);
        }

        /// <summary>
        /// POST: Edit EncryptedItem
        /// </summary>
        /// <param name="encryptedItem"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EncryptedItem encryptedItem)
        {
			string userID = User.Identity.GetUserId();
			if (encryptedItem.OwnerID != userID)
			{
				ViewBag.errorMessage = "Only the owner of this file may edit this file";
				return View("Error");
			}
			if (ModelState.IsValid)
            {
                db.Entry(encryptedItem).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            } else 
            return View(encryptedItem);
        }

        /// <summary>
        /// returns list of users permitted to access this item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> PermittedUsers(int? id)
        {
            string userID = User.Identity.GetUserId();

            EncryptedItem encryptedItem = await db.EncryptedItems.FindAsync(id);

            //if you aren't the owner, you can't access list of permitted users
            if (encryptedItem.OwnerID != userID)
            {
                ViewBag.errorMessage = "Only the owner of this file may view or change its access";
                return View("Error");
            }

            if (encryptedItem == null) { return HttpNotFound(); }


            PermittedUsersViewModel view = new PermittedUsersViewModel
            {
                PermittedUserIDs = encryptedItem.PermittedUserIDs,
                ItemID = (int)id //cast from nullable int
            };

            return View(view);
        }

        /// <summary>
        /// POST: attempts to add a User to the list of permissions for this file
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddPermission(AddPermissionViewModel model)
        {
            string userID = User.Identity.GetUserId();


            EncryptedItem encryptedItem = await db.EncryptedItems.FindAsync(model.ItemID);

            if (encryptedItem == null)
            {
                ViewBag.errorMessage = "File not found";
                return View("Error");
            }
            //check if user is owner of file
            if (encryptedItem.OwnerID != userID)
            {
                ViewBag.errorMessage = "Only the owner of this file may change the permissions";
                return View("Error");
            }

            //get the current UserManager to enable User query
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            try
            {
                var userQuery = (from user in userManager.Users
                                 where user.Email.Equals(model.UserEmail)
                                 select user).Single();

                string userIDToAdd = userQuery.Id;

                if (encryptedItem.PermittedUserIDs.Contains(userIDToAdd))
                {
                    ViewBag.errorMessage = "User already has permission";
                    return View("Info");
                }

                encryptedItem.PermittedUserIDs.Add(userIDToAdd);
                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                ViewBag.errorMessage = "User not found";
                return View("Error");
            }

            PermittedUsersViewModel view = new PermittedUsersViewModel
            {
                PermittedUserIDs = encryptedItem.PermittedUserIDs,
                ItemID = model.ItemID
            };

            ViewBag.Message = "User " + model.UserEmail +  " successfully given permission to file";
            return View("Info");
        }

        /// <summary>
        /// GET: returns AddPermission view for correct item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> AddPermission(int id)
        {
            string userID = User.Identity.GetUserId();

            EncryptedItem encryptedItem = await db.EncryptedItems.FindAsync(id);
            if (encryptedItem == null) { return HttpNotFound(); }

            //check if user is owner of file
            if (encryptedItem.OwnerID != userID)
            {
                ViewBag.errorMessage = "Only the owner of this file may change the permissions";
                return View("Error");
            }

            AddPermissionViewModel view = new AddPermissionViewModel { ItemID = id };

            return View(view);
        }

        /// <summary>
        /// GET: returns a view with the deletable item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string userID = User.Identity.GetUserId();
            EncryptedItem encryptedItem = await db.EncryptedItems.FindAsync(id);
            if (encryptedItem == null)
            {
                ViewBag.errorMessage = "File not found: this is definitely not good";
                return View("Error");
            }
            if (userID != encryptedItem.OwnerID)
            {
                ViewBag.errorMessage = "You are not the owner of this file and you may not delete it";
                return View("Error");
            }
            return View(encryptedItem);
        }

        /// <summary>
        /// POST: attempts to delete an item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            EncryptedItem encryptedItem = await db.EncryptedItems.FindAsync(id);

            db.EncryptedItems.Remove(encryptedItem);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
