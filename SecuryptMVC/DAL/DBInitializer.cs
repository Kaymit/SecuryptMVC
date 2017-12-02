using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SecuryptMVC.Models;

namespace SecuryptMVC.DAL
{
    //MVC EF framework class: if model changes, DB is dropped and recreated with test/dev data
    public class DBInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<FileContext>
    {
        
       protected override void Seed(FileContext context)
       {
            var files = new List<EncryptedItem>
           {
               new EncryptedItem
               {
                   Name = "FileOne",
                   OwnerID = "uyGJT9THj(*&jyoiY7oj978JIGkUF&%^dvbrfJT^&%HJkiY",
                   StorageLocation = "StorageOne/storage/storageone/test",
                   IsPrivate = false,
                   PermittedUserIDs = new List<string>
                   {
                       Guid.NewGuid().ToString(),
                       Guid.NewGuid().ToString(),
                       Guid.NewGuid().ToString(),
                   }
               },

               new EncryptedItem
               {
                   Name = "FileTwo",
                   OwnerID = "uiHi786ht&%rujH&%6fug&^Tr5fh6r78TU&6TU76Ty&^Yt*",
                   StorageLocation = "Storage2/directory/saves.test",
                   IsPrivate = false,
                   PermittedUserIDs = new List<string>
                   {
                       Guid.NewGuid().ToString(),
                       Guid.NewGuid().ToString(),
                       Guid.NewGuid().ToString(),
                       Guid.NewGuid().ToString(),
                   }
               },

               new EncryptedItem
               {
                   Name = "FileTwo",
                   OwnerID = "uiHi786ht&%rujH&%6fug&^Tr5fh6r78TU&6TU76Ty&^Yt*",
                   StorageLocation = "Storage2/directory/saves.test",
                   IsPrivate = true,
                   PermittedUserIDs = new List<string>
                   {
                       Guid.NewGuid().ToString(),
                       Guid.NewGuid().ToString(),
                       Guid.NewGuid().ToString(),
                       Guid.NewGuid().ToString(),
                   }
               },
           };

           files.ForEach(f => context.EncryptedItems.Add(f));
           context.SaveChanges();

       }
       
    }
}