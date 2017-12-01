﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SecuryptMVC.Models
{
    public class EncryptedItem
    {
		/// <summary>
		/// Unique ID auto-generated by Entity Framework
		/// </summary>
        public int		        ID				{ get; set; }

        /// <summary>
        /// USer ID of owner, auto-generated by Entity Framework upon account creation
        /// </summary>
        [Required]
        public string           OwnerID { get; set; }

        /// <summary>
        /// List of UserIDs with permission to decrypt and download item
        /// </summary>
        private List<String> _PermittedUserIDs { get; set; }

        /// <summary>
        /// Workaround for lack of List<string> support in EF
        /// https://stackoverflow.com/questions/20711986/entity-framework-code-first-cant-store-liststring
        /// </summary>
        public List<string> PermittedUserIDs
        {
            get { return _PermittedUserIDs; }
            set { _PermittedUserIDs = value; }
        }

        /// <summary>
        /// Allows storing a list of strings without creating a second table with rows for each permission
        /// </summary>
        public string PermittedUserIDsAsString
        {
            get { return String.Join(",", _PermittedUserIDs); }
            set { _PermittedUserIDs = value.Split(',').ToList(); }
        }

        /// <summary>
        /// Whether or not EncryptedItem is private to user
        /// </summary>
        public bool		        IsPrivate		{ get; set; }

		/// <summary>
		/// Filename available for searching
		/// </summary>
        [Required]
        public string	        Name			{ get; set; }

		/// <summary>
		/// File 
		/// </summary>
        [Required]
        public string	        StorageLocation { get; set; }
    }
}