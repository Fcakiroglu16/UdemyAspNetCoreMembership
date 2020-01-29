using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UdemyIdentity.Enums
{
    public enum TwoFactor
    {
        [Display(Name = "Hiç biri")]
        None = 0,

        [Display(Name = "Telefon ile kimlik doğrulama")]
        Phone = 1,

        [Display(Name = "Email ile kimlik doğrulama")]
        Email = 2,

        [Display(Name = "Microsoft/Google Authenticator ile kimlik doğrulama")]
        MicrosoftGoogle = 3
    }
}