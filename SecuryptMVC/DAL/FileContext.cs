using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using SecuryptMVC.Models;

namespace SecuryptMVC.DAL
{
    /// <summary>
    /// Database context class for EncryptedItem table and ASP.NET Identity tables
    /// </summary>
    /// <author>
    /// Kevin Mitchell 15/11/2017
    /// </author>
	public class FileContext : DbContext
	{
		public FileContext() : base("FileContext")
		{

		}

		public DbSet<EncryptedItem> EncryptedItems { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}