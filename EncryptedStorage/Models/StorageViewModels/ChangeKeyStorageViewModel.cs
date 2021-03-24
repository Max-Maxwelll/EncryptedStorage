using System.ComponentModel.DataAnnotations;

namespace EncryptedStorage.Models.StorageViewModels
{
    public class ChangeKeyStorageViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string NewKey { get; set; }
        [Required]
        public string OldKey { get; set; }
        [Required]
        public string ConfirmKey { get; set; }
    }
}
