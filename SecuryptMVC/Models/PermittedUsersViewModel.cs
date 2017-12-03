using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SecuryptMVC.Models
{
    public class PermittedUsersViewModel
    {
        public int ItemID { get; set; }
        public string UserIDToAdd { get; set; }
        public List<String> PermittedUserIDs { get; set; }
    }
}