using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UdemyIdentity.Enums;
using UdemyIdentity.Models;
using UdemyIdentity.Service;
using UdemyIdentity.ViewModes;

namespace UdemyIdentity.Controllers
{
    [Authorize]
    public class MemberController : BaseController
    {
        private readonly TwoFactorService _twoFactorService;

        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TwoFactorService twoFactorService) : base(userManager, signInManager)
        {
            _twoFactorService = twoFactorService;
        }

        public IActionResult Index()
        {
            AppUser user = CurrentUser;
            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            return View(userViewModel);
        }

        public IActionResult UserEdit()
        {
            AppUser user = CurrentUser;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel userViewModel, IFormFile userPicture)
        {
            ModelState.Remove("Password");
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;

                string phone = userManager.GetPhoneNumberAsync(user).Result;

                if (phone != userViewModel.PhoneNumber)
                {
                    if (userManager.Users.Any(u => u.PhoneNumber == userViewModel.PhoneNumber))
                    {
                        ModelState.AddModelError("", "Bu telefon numarası başka üye tarafından kullanılmaktadır.");
                        return View(userViewModel);
                    }
                }

                if (userPicture != null && userPicture.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream);

                        user.Picture = "/UserPicture/" + fileName;
                    }
                }

                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;

                user.City = userViewModel.City;

                user.BirthDay = userViewModel.BirthDay;

                user.Gender = (int)userViewModel.Gender;

                IdentityResult result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await userManager.UpdateSecurityStampAsync(user);
                    await signInManager.SignOutAsync();
                    await signInManager.SignInAsync(user, true);

                    ViewBag.success = "true";
                }
                else
                {
                    AddModelError(result);
                }
            }

            return View(userViewModel);
        }

        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PasswordChange(PasswordChangeViewModel passwordChangeViewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;

                bool exist = userManager.CheckPasswordAsync(user, passwordChangeViewModel.PasswordOld).Result;

                if (exist)
                {
                    IdentityResult result = userManager.ChangePasswordAsync(user, passwordChangeViewModel.PasswordOld, passwordChangeViewModel.PasswordNew).Result;

                    if (result.Succeeded)
                    {
                        await userManager.UpdateSecurityStampAsync(user);
                        await signInManager.SignOutAsync();

                        await signInManager.PasswordSignInAsync(user, passwordChangeViewModel.PasswordNew, true, false);

                        ViewBag.success = "true";
                    }
                    else
                    {
                        AddModelError(result);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Eski şifreniz yanlış");
                }
            }

            return View(passwordChangeViewModel);
        }

        public void LogOut()
        {
            signInManager.SignOutAsync();
        }

        public IActionResult AccessDenied(string ReturnUrl)
        {
            if (ReturnUrl.ToLower().Contains("violencegage"))
            {
                ViewBag.message = "Erişmeye çalıştığınız sayfa şiddet videoları içerdiğinden dolayı 15 yaşında büyük olmanız gerekmektedir";
            }
            else if (ReturnUrl.ToLower().Contains("ankarapage"))
            {
                ViewBag.message = "Bu sayfaya sadece şehir alanı ankara olan kullanıcılar erişebilir";
            }
            else if (ReturnUrl.ToLower().Contains("exchange"))
            {
                ViewBag.message = "30 günlük ücretsiz deneme hakkınız sona ermiştir.";
            }
            else
            {
                ViewBag.message = "Bu sayfaya erişim izniniz yoktur. Erişim izni almak için site yöneticisiyle görüşünüz";
            }

            return View();
        }

        [Authorize(Roles = "manager,admin")]
        public IActionResult Manager()
        {
            return View();
        }

        [Authorize(Roles = "editor,admin")]
        public IActionResult Editor()
        {
            return View();
        }

        [Authorize(Policy = "AnkaraPolicy")]
        public IActionResult AnkaraPage()
        {
            return View();
        }

        [Authorize(Policy = "ViolencePolicy")]
        public IActionResult ViolencePage()
        {
            return View();
        }

        public async Task<IActionResult> ExchangeRedirect()
        {
            bool result = User.HasClaim(x => x.Type == "ExpireDateExchange");

            if (!result)
            {
                Claim ExpireDateExchange = new Claim("ExpireDateExchange", DateTime.Now.AddDays(30).Date.ToShortDateString(), ClaimValueTypes.String, "Internal");

                await userManager.AddClaimAsync(CurrentUser, ExpireDateExchange);

                await signInManager.SignOutAsync();
                await signInManager.SignInAsync(CurrentUser, true);
            }

            return RedirectToAction("Exchange");
        }

        [Authorize(Policy = "ExchangePolicy")]
        public IActionResult Exchange()
        {
            return View();
        }

        public async Task<IActionResult> TwoFactorWithAuthenticator()
        {
            string unformattedKey = await userManager.GetAuthenticatorKeyAsync(CurrentUser);

            if (string.IsNullOrEmpty(unformattedKey))
            {
                await userManager.ResetAuthenticatorKeyAsync(CurrentUser);

                unformattedKey = await userManager.GetAuthenticatorKeyAsync(CurrentUser);
            }

            AuthenticatorViewModel authenticatorViewModel = new AuthenticatorViewModel();

            authenticatorViewModel.SharedKey = unformattedKey;

            authenticatorViewModel.AuthenticatorUri = _twoFactorService.GenerateQrCodeUri(CurrentUser.Email, unformattedKey);

            return View(authenticatorViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> TwoFactorWithAuthenticator(AuthenticatorViewModel authenticatorVM)
        {
            var verificationCode = authenticatorVM.VerificationCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2FATokenValid = await userManager.VerifyTwoFactorTokenAsync(CurrentUser, userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (is2FATokenValid)
            {
                CurrentUser.TwoFactorEnabled = true;
                CurrentUser.TwoFactor = (sbyte)TwoFactor.MicrosoftGoogle;

                var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(CurrentUser, 5);

                TempData["recoveryCodes"] = recoveryCodes;
                TempData["message"] = "İki adımlı kimlik doğrulama tipiniz Microsoft/Google Authenticator olarak belirlenmiştir.";

                return RedirectToAction("TwoFactorAuth");
            }
            else
            {
                ModelState.AddModelError("", "Girdiğiniz doğrulama kodu yanlıştır");
                return View(authenticatorVM);
            }
        }

        public IActionResult TwoFactorAuth()
        {
            return View(new AuthenticatorViewModel() { TwoFactorType = (TwoFactor)CurrentUser.TwoFactor });
        }

        [HttpPost]
        public async Task<IActionResult> TwoFactorAuth(AuthenticatorViewModel authenticatorVM)
        {
            switch (authenticatorVM.TwoFactorType)
            {
                case TwoFactor.None:

                    CurrentUser.TwoFactorEnabled = false;
                    CurrentUser.TwoFactor = (sbyte)TwoFactor.None;

                    TempData["message"] = "İki adımlı kimlik doğrulama tipiniz hiçbiri olarak belirlenmiştir.";

                    break;

                case TwoFactor.Phone:

                    if (string.IsNullOrEmpty(CurrentUser.PhoneNumber))
                    {
                        ViewBag.warning = "Telefon numaranız belirtilmemiştir. Lütfen kullanıcı güncelleme sayfasından telefon numaranızı belirtiniz.";
                    }

                    CurrentUser.TwoFactorEnabled = true;

                    CurrentUser.TwoFactor = (sbyte)TwoFactor.Phone;
                    TempData["message"] = "İki adımlı kimlik doğrulama tipimiz telefon olarak belirlenmiştir";

                    break;

                case TwoFactor.Email:

                    CurrentUser.TwoFactorEnabled = true;
                    CurrentUser.TwoFactor = (sbyte)TwoFactor.Email;
                    TempData["message"] = "İki adımlı kimlik doğrulama tipimiz email olarak belirlenmiştir";

                    break;

                case TwoFactor.MicrosoftGoogle:

                    return RedirectToAction("TwoFactorWithAuthenticator");

                default:
                    break;
            }

            await userManager.UpdateAsync(CurrentUser);

            return View(authenticatorVM);
        }
    }
}