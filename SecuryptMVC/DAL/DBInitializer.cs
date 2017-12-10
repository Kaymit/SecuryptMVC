using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SecuryptMVC.Models;

namespace SecuryptMVC.DAL
{
    /// <summary>
    ///MVC EF framework class: if model changes, DB is dropped and recreated with test/dev data
    /// </summary>
    /// <author>
    /// Kevin Mitchell 15/11/2017
    /// </author>
    public class DBInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<FileContext>
    {
        
       protected override void Seed(FileContext context)
       {
            base.Seed(context);
       }
    }
}