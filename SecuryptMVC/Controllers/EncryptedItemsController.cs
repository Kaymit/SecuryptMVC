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
            if (encryptedItem == null)
            {
                return HttpNotFound();
            }

            PermittedUsersViewModel view = new PermittedUsersViewModel
            {
                PermittedUserIDs = encryptedItem.PermittedUserIDs,
                ItemID = (int)id //cast from nullable int
            };

            return View(view);
        }

        /*
        public async Task<ActionResult> AddPermission(string email)
        {
            string userID = User.Identity.GetUserId();

            EncryptedItem encryptedItem = await db.EncryptedItems.FindAsync(id);
            if (encryptedItem == null)
            {
                return HttpNotFound();
            }

            PermittedUsersViewModel view = new PermittedUsersViewModel
            {
                PermittedUserIDs = encryptedItem.PermittedUserIDs,
                ItemID = (int)id //cast from nullable int
            };

            return View(view);
        }
        */

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
