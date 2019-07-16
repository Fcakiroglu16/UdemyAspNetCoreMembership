using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UdemyIdentity.Models;
using UdemyIdentity.ViewModes;

namespace UdemyIdentity.Controllers
{
    //[Authorize(Roles = "admin")]
    public class AdminController : BaseController
    {
        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : base(userManager, null, roleManager)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Claims()
        {
            return View(User.Claims.ToList());
        }

        public IActionResult RoleCreate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RoleCreate(RoleViewModel roleViewModel)
        {
            AppRole role = new AppRole();
            role.Name = roleViewModel.Name;
            IdentityResult result = roleManager.CreateAsync(role).Result;

            if (result.Succeeded)

            {
                return RedirectToAction("Roles");
            }
            else
            {
                AddModelError(result);
            }

            return View(roleViewModel);
        }

        public IActionResult Roles()
        {
            return View(roleManager.Roles.ToList());
        }

        public IActionResult Users()
        {
            return View(userManager.Users.ToList());
        }

        public IActionResult RoleDelete(string id)
        {
            AppRole role = roleManager.FindByIdAsync(id).Result;
            if (role != null)
            {
                IdentityResult result = roleManager.DeleteAsync(role).Result;
            }

            return RedirectToAction("Roles");
        }

        public IActionResult RoleUpdate(string id)
        {
            AppRole role = roleManager.FindByIdAsync(id).Result;

            if (role != null)
            {
                return View(role.Adapt<RoleViewModel>());
            }

            return RedirectToAction("Roles");
        }

        [HttpPost]
        public IActionResult RoleUpdate(RoleViewModel roleViewModel)
        {
            AppRole role = roleManager.FindByIdAsync(roleViewModel.Id).Result;

            if (role != null)
            {
                role.Name = roleViewModel.Name;
                IdentityResult result = roleManager.UpdateAsync(role).Result;

                if (result.Succeeded)
                {
                    return RedirectToAction("Roles");
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Güncelleme işlemi başarısız oldu.");
            }

            return View(roleViewModel);
        }

        public IActionResult RoleAssign(string id)
        {
            TempData["userId"] = id;
            AppUser user = userManager.FindByIdAsync(id).Result;

            ViewBag.userName = user.UserName;

            IQueryable<AppRole> roles = roleManager.Roles;

            List<string> userroles = userManager.GetRolesAsync(user).Result as List<string>;

            List<RoleAssignViewModel> roleAssignViewModels = new List<RoleAssignViewModel>();

            foreach (var role in roles)
            {
                RoleAssignViewModel r = new RoleAssignViewModel();
                r.RoleId = role.Id;
                r.RoleName = role.Name;
                if (userroles.Contains(role.Name))
                {
                    r.Exist = true;
                }
                else
                {
                    r.Exist = false;
                }
                roleAssignViewModels.Add(r);
            }

            var s = new SelectList(roleAssignViewModels, "RoleId", "RoleName", roleAssignViewModels.First(x => x.Exist == true).RoleId);
            var a = new SelectList()
            return View(s);
        }

        [HttpPost]
        public async Task<IActionResult> RoleAssign()
        {
            var userRoleId = Request.Form["userrole"];

            AppUser user = userManager.FindByIdAsync(TempData["userId"].ToString()).Result;

            AppRole role = await roleManager.FindByIdAsync(userRoleId);

            List<string> userRoleNames = userManager.GetRolesAsync(user).Result as List<string>;

            await userManager.RemoveFromRolesAsync(user, userRoleNames);

            await userManager.AddToRoleAsync(user, role.Name);

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> ResetUserPassword(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);

            PasswordResetByAdminViewModel passwordResetByAdminViewModel = new PasswordResetByAdminViewModel();
            passwordResetByAdminViewModel.UserId = user.Id;

            return View(passwordResetByAdminViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResetUserPassword(PasswordResetByAdminViewModel passwordResetByAdminViewModel)
        {
            AppUser user = await userManager.FindByIdAsync(passwordResetByAdminViewModel.UserId);

            string token = await userManager.GeneratePasswordResetTokenAsync(user);

            await userManager.ResetPasswordAsync(user, token, passwordResetByAdminViewModel.NewPassword);

            await userManager.UpdateSecurityStampAsync(user);

            //securitystamp degerini  update etmezsem kullanıcı eski şifresiyle sitemizde dolaşmaya devam eder ne zaman çıkış yaparsa ozaman tekrar yeni şifreyle girmek zorunda
            //eger update edersen kullanıcı  otomatik olarak  sitemize girdiği zaman login ekranına yönlendirilecek.

            //Identity Mimarisi cookie tarafındaki securitystamp ile veritabanındaki security stamp değerini her 30 dakikada bir kontrol eder. Kullanıcı eski şifreyle en fazla server da session açıldıktan sonra 30 dakkika gezebilir. Bunu isterseniz 1 dakkikaya indirebilirsiniz. ama tavsiye edilmez. her bir dakika da  her kullanıcı için veritabanı kontrolü  yük getirir.

            return RedirectToAction("Users");
        }
    }
}