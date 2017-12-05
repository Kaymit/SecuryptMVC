using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecuryptMVC.Models
{
    public class AdminListViewModel
    {
        public List<AdminViewModel> AdminUserList { get; set; }
    }

    public class AdminViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
    }
}