using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EncryptedStorage.Models.AccountViewModels
{
    public class ConfirmActionViewModels
    {
        [Required]
        public string Password { get; set; }
    }
}
