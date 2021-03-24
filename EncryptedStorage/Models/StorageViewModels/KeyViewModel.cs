using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EncryptedStorage.Models.StorageViewModels
{
    public class KeyViewModel
    {
        [Required]
        public string StorageName { get; set; }
        [Required]
        public string Key { get; set; }
    }
}
