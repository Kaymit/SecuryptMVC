using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecuryptMVC.Models
{
    public class PermittedUsersViewModel
    {
        public int ItemID { get; set; }
        public List<String> PermittedUserIDs { get; set; }
    }
}