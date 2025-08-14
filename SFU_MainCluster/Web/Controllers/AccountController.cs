using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SFU_MainCluster.Web.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace SFU_MainCluster.Web.Controllers
{
    public class AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager) : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly UserManager<IdentityUser> _userManager = userManager;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                TempData["Message"] = $"You are already logged into the session as \"{User.Identity.Name}\".";
            }
            else
            {
                if (returnUrl != null)
                {
                    TempData["Error"] = "This action requires login.";
                }
            }
            
            return View("/Web/Views/Account/Login.cshtml");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (model.IsGuest)
            {
                var guestUser = new IdentityUser { UserName = $"guest_user{Guid.NewGuid().ToString()}" };
                await _userManager.CreateAsync(guestUser);
                await _userManager.AddToRoleAsync(guestUser, "guest");

                await _signInManager.SignInAsync(guestUser, isPersistent: false);

                return Redirect(returnUrl ?? "/Home/Index");
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Login error: Form data incorrect.";
                    return View();
                }
                SignInResult result = await _signInManager.PasswordSignInAsync(model.Login!, model.Password!, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return Redirect(returnUrl ?? "/Home/Index");
                }
            }

            TempData["Error"] = "Login error: Wrong login data.";
            return View(model);
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}