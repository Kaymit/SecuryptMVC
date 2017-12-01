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
using Microsoft.AspNet.Identity;

//https://support.microsoft.com/en-us/help/323246/how-to-upload-a-file-to-a-web-server-in-asp-net-by-using-visual-c--net
namespace SecuryptMVC.Controllers
{
    public class EncryptedItemsController : Controller
    {
        private FileContext db = new FileContext();

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

        // GET: EncryptedItems/Create
        public ActionResult Create()
        {
            return View();
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

        // POST: EncryptedItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Name,OwnerID,IsPrivate,StorageLocation")] EncryptedItem eItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(eItem).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(eItem);
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
