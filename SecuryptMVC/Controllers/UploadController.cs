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
    /// Kevin Mitchell 28/11/2017 - 3/12/2017
    /// </summary>
    public class UploadController : Controller
    {
        FileContext db = new FileContext();

        // GET: Upload
		[Authorize]
        public ActionResult Index()
        {
            return View("~/Views/Upload/Index.cshtml");
        }

        /// <summary>
        /// Accepts HttpPostedFileBase array to save to database and file system
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadFiles(IEnumerable<HttpPostedFileBase> files)
        {
            //Register CryptoHandler and ensure keys are current
            CryptoHandler ch = new CryptoHandler();
            ch.RegisterKeys();

            //get current User ID
            string ownerID = User.Identity.GetUserId();

            //create list of permitted user IDs and add current owner ID
            List<string> permittedUserIDs = new List<string>();
            permittedUserIDs.Add(User.Identity.GetUserId());

            var f = Request.Files[0];
            if (f == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            foreach (var file in files)
            {
                //give file the generated GUID
                string fileName = file.FileName;
                string filePath = Guid.NewGuid() + Path.GetExtension(file.FileName);
                
                string storagePath = Path.Combine(Server.MapPath("~/UploadedFiles"), filePath);

                //encrypt and save file to storage path
                file.SaveAs(storagePath);
                ch.EncryptFile(storagePath);

                if (System.IO.File.Exists(storagePath))
                {
                    System.IO.File.Delete(storagePath);
                    ViewBag.deleteSuccess = "true";
                }

                EncryptedItem item = new EncryptedItem {
                    Name = fileName,
                    OwnerID = ownerID,
                    StorageLocation = storagePath,
                    PermittedUserIDs = permittedUserIDs,
                    IsPrivate = true
                };

                //add new EncryptedItem to database
                db.EncryptedItems.Add(item);
                db.SaveChangesAsync(); 
            }
            return Json("Error: file was not added to database");
        }
    }
}