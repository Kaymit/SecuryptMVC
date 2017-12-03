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
            base.Seed(context);
       }
    }
}