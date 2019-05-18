using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UdemyIdentity.Models;
using UdemyIdentity.ViewModes;

namespace UdemyIdentity.Controllers
{
    //[Authorize(Roles = "admin")]
    public class AdminController : BaseController
    {
        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager) : base(userManager, signInManager, roleManager)
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

        public async Task<IActionResult> RoleClaimAssign(string id)
        {
            AppRole appRole = roleManager.Roles.First(x => x.Id == id);

            IList<Claim> Roleclaims = await roleManager.GetClaimsAsync(appRole);

            IList<Claim> userClaim = userManager.GetClaimsAsync(CurrentUser).Result;

            List<ClaimAssignViewModel> claimAssignViewModels = new List<ClaimAssignViewModel>();

            foreach (var item in Roleclaims)
            {
                ClaimAssignViewModel c = new ClaimAssignViewModel();
                c.ClaimValue = item.Value;
                c.ClaimType = item.Type;
                if (userClaim.Any(x => x.Value == c.ClaimValue))
                {
                    c.Exist = true;
                }

                claimAssignViewModels.Add(c);
            }
            return View(claimAssignViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> RoleClaimAssign(List<ClaimAssignViewModel> claimAssignViewModels)
        {
            AppUser user = CurrentUser;

            IList<Claim> userClaims = userManager.GetClaimsAsync(user).Result;

            foreach (var item in claimAssignViewModels)
            {
                Claim c = new Claim(item.ClaimType, item.ClaimValue);

                if (item.Exist)

                {
                    if (!userClaims.Any(x => x.Type == item.ClaimType && x.Value == item.ClaimValue))
                    {
                        await userManager.AddClaimAsync(user, c);
                    }
                }
                else
                {
                    await userManager.RemoveClaimAsync(user, c);
                }
                c = null;
            }

            await signInManager.SignOutAsync();
            await signInManager.SignInAsync(user, true);

            return RedirectToAction("Users");
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

            return View(roleAssignViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> RoleAssign(List<RoleAssignViewModel> roleAssignViewModels)
        {
            AppUser user = userManager.FindByIdAsync(TempData["userId"].ToString()).Result;

            foreach (var item in roleAssignViewModels)
            {
                if (item.Exist)

                {
                    await userManager.AddToRoleAsync(user, item.RoleName);
                }
                else
                {
                    await userManager.RemoveFromRoleAsync(user, item.RoleName);
                }
            }

            return RedirectToAction("Users");
        }
    }
}