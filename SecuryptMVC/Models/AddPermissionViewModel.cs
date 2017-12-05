using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecuryptMVC.Models
{
    /// <summary>
    /// ViewModel to display and pass data from browser when adding permissions to items
    /// </summary>
    public class AddPermissionViewModel
    {
        public int ItemID { get; set; }
        public string UserEmail { get; set; }
    }
}