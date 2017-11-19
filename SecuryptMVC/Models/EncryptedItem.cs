using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecuryptMVC.Models
{
    public class EncryptedItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string PublicKey { get; set; }
        public string StorageLocation { get; set; }
    }
}