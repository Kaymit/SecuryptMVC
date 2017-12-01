using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using SecuryptMVC.Utility;
using SecuryptMVC.DAL;
using SecuryptMVC.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace SecuryptMVC.Controllers
{
    /// <summary>
    /// Controller to handle unencrypted files being uploaded to the application.
    /// <para>Author: Michael</para>
    /// Date: 2017-11-15
    /// <para>Based on: http://www.dotnetawesome.com/2017/02/drag-drop-file-upload-aspnet-mvc.html </para>
    /// </summary>
    public class UploadController : Controller
    {
        FileContext db = new FileContext();

        // GET: Upload
        public ActionResult Index()
        {
            return View("~/Views/Upload/Index.cshtml");
        }

        // POST: Upload and Create
        [HttpPost]
        public ActionResult UploadFiles(IEnumerable<HttpPostedFileBase> files)
        {
            //Register CryptoHandler and ensure keys are current
            CryptoHandler ch = new CryptoHandler();
            ch.initProgram();

            string ownerID = User.Identity.GetUserId();

            var f = Request.Files[0];
            if (f == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            foreach (var file in files)
            {
                string fileName = file.FileName;
                string filePath = Guid.NewGuid() + Path.GetExtension(file.FileName);

                //encrypt file and give it the generated GUID

                string storagePath = Path.Combine(Server.MapPath("~/UploadedFiles"), filePath);

                file.SaveAs(storagePath);
                ch.EncryptFile(storagePath);

                if (System.IO.File.Exists(storagePath))
                {
                    System.IO.File.Delete(storagePath);
                    ViewBag.deleteSuccess = "true";
                }

                //files private by default
                db.EncryptedItems.Add(new EncryptedItem { Name = fileName, OwnerID = ownerID, StorageLocation = storagePath, IsPrivate = true });
                db.SaveChangesAsync(); //add new EncryptedItem to database
                                       //TODO move to helper class?

                /*
                return RedirectToAction("Create", "EncryptedItemController", new {
                    Name = fileName,
                    PublicKey = publicKey,
                    StorageLocation = storagePath
                });
                */
            }
            //TODO: replace this with a Create EncryptedItem view call?
            return Json("Error: file was not added to database");
        }
    }
}