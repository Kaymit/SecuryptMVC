using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SecuryptMVC.Models;

namespace SecuryptMVC.DAL
{
    /// <summary>
    /// Utility class for development: to drop AspNetUsers table if model changes
    /// </summary>
    public class UserInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
    }
}