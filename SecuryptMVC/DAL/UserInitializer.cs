using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SecuryptMVC.Models;

namespace SecuryptMVC.DAL
{
    public class UserInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
    }
}