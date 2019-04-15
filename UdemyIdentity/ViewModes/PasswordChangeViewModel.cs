using System.ComponentModel.DataAnnotations;

namespace UdemyIdentity.ViewModes
{
    public class PasswordChangeViewModel
    {
        [Display(Name = "Eski şifreniz")]
        [Required(ErrorMessage = "Eski şifreniz gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz en az 4 karakterli olmak zorundadır.")]
        public string PasswordOld { get; set; }

        [Display(Name = "Yeni şifreniz")]
        [Required(ErrorMessage = "Yeni şifreniz gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz en az 4 karakterli olmak zorundadır.")]
        public string PasswordNew { get; set; }

        [Display(Name = "Onay yeni şifreniz")]
        [Required(ErrorMessage = "Onay yeni şifre gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz en az 4 karakterli olmak zorundadır.")]
        [Compare("PasswordNew", ErrorMessage = "Yeni şifreniz  ve onay şifreniz birbirinden farklıdır.")]
        public string PasswordConfirm { get; set; }
    }
}