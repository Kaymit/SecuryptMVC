﻿using System;
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

        // GET: EncryptedItems
        public async Task<ActionResult> Index()
        {
            string userID = User.Identity.GetUserId();

            //Query to list all items user is permitted by item owner to view, including user's own
            IQueryable<EncryptedItem> queryPermitted = from item in db.EncryptedItems
                            where item.PermittedUserIDsAsString.Contains(userID) || item.IsPrivate == false //TODO ***Unsure if this OR statement works***
                            select item;

            //async execute query
            List<EncryptedItem> items = await queryPermitted.ToListAsync();

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
            string contentType = MimeMapping.GetMimeMapping(encryptedItem.Name);       //can get content type from file if file exists
            MemoryStream decryptedFileStream = ch.DecryptFile(encryptedItem.StorageLocation);

            //byte[] bytes = decryptedFileStream.ToArray();
            //System.IO.File.Delete(fileName);                                  //delete temp file

            return File(decryptedFileStream, contentType, encryptedItem.Name);      //construct and return filestream to client
        }

        // GET: EncryptedItems/Details/5
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
            return View(encryptedItem);
        }

        // GET: EncryptedItems/Edit/5
        public async Task<ActionResult> Edit(int? id)
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
            return View(encryptedItem);
        }

        /// <summary>
        /// Edit EncryptedItem Action. Bound only to properties which are possible to edit
        /// </summary>
        /// <param name="encryptedItem"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "OwnerID,IsPrivate")] EncryptedItem encryptedItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(encryptedItem).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
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
                ViewBag.Message = "Only the owner of this file may change the permissions";
                return View("Info");
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
                ViewBag.Message = "File not found: something spooky has happened, consider panicking";
                return View("Info");
            }
            //check if user is owner of file
            if (encryptedItem.OwnerID != userID)
            {
                ViewBag.Message = "You do not have permission to access this file, so you definitely don't have permission " +
                    "to access or CHANGE the permissions!";
                return View("Info");
            }

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var userQuery = (from user in userManager.Users
                                        where user.Email.Equals(model.UserEmail)
                                        select user).Single();

            string userIDToAdd = userQuery.Id;

            encryptedItem.PermittedUserIDs.Add(userIDToAdd);
            await db.SaveChangesAsync();

            PermittedUsersViewModel view = new PermittedUsersViewModel
            {
                PermittedUserIDs = encryptedItem.PermittedUserIDs,
                ItemID = model.ItemID
            };

            ViewBag.Message = "User " + model.UserEmail +  " successfully given permission to file.";
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
                ViewBag.Message = "Only the owner of this file may change the permissions";
                return View("Info");
            }

            AddPermissionViewModel view = new AddPermissionViewModel { ItemID = id };

            return View(view);
        }

        // GET: EncryptedItems/Delete/5
        public async Task<ActionResult> Delete(int? id)
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
            return View(encryptedItem);
        }

        // POST: EncryptedItems/Delete/5
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
