using System.ComponentModel.DataAnnotations;

namespace UdemyIdentity.ViewModes
{
    public class PasswordResetViewModel
    {
        [Display(Name = "Email adresiniz")]
        [Required(ErrorMessage = "Email alanı gereklidir")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Yeni şifreniz")]
        [Required(ErrorMessage = "Şifre alanı gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "şifreniz en az 4 karakterli olmalıdır.")]
        public string PasswordNew { get; set; }
    }
}