using ECommerce_Application.Models;
using ECommerce_Application.Services.Email_Service;
using ECommerce_Application.ViewModels;
using ECommerceApplication.Data;
using ECommerceApplication.Models;
using ECommerceApplication.Utilities;
using ECommerceApplication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Application.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager , ApplicationDbContext context
            , IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if(!ModelState.IsValid) 
                return View(registerVM);
            // Validate role selection
            if (registerVM.Role != Roles.Customer && registerVM.Role != Roles.Vendor)
            {
                ModelState.AddModelError("Role", "Invalid role selected.");
                return View(registerVM);
            }
            var user = new ApplicationUser
            {
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                Email = registerVM.Email,
                UserName = registerVM.Email,
                PasswordHash = registerVM.Password,
                VendorStatus = registerVM.Role == Roles.Vendor ? VendorStatus.Pending : VendorStatus.NotApplicable,
            };
            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, registerVM.Role);
                if(registerVM.Role == Roles.Vendor)
                {
                    TempData["Message"] = "Registration successful! Your vendor account is pending approval.";
                    return RedirectToAction("Login");
                }
                //  add cookie to keep user logged in after registration
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(registerVM);

            }
    }
        public IActionResult Login(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);
            var result = await _signInManager.PasswordSignInAsync(loginVM.Email, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(loginVM.Email);
                return await RedirectAfterLogin(user!, loginVM.ReturnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password!");
                return View(loginVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() 
        {
        return View();
        } 

        public async Task <IActionResult> PendingApproval()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.VendorStatus == VendorStatus.Approved)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var model = new ProfileVM
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                Phone = user.PhoneNumber,
                Email = user.Email,
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileVM profileVM)
        {
            if (!ModelState.IsValid)
                return View(profileVM);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            user.FirstName = profileVM.FirstName;
            user.LastName = profileVM.LastName;
            user.Address = profileVM.Address;
            user.PhoneNumber = profileVM.Phone;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Message"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(profileVM);
            }
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task <IActionResult> ChangePassword(ChangePasswordVM changePasswordVM)
        {
            if (ModelState.IsValid)
            {
                return View(changePasswordVM);
            }
            var user= await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            var result= await _userManager.ChangePasswordAsync(user,changePasswordVM.CurrentPassword,changePasswordVM.NewPassword );
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password Changed Successfully!";
                return RedirectToAction("Profile");
            }
            else
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(changePasswordVM);
            }

        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Email not found");
                return View(model);
            }

            string otp = Random.Shared.Next(100000, 999999).ToString();

            PasswordResetOTP item = new()
            {
                Email = model.Email,
                OTP = otp,
                ExpireAt = DateTime.Now.AddMinutes(5),
                IsUsed = false
            };

            _context.PasswordResetOTPs.Add(item);

            await _context.SaveChangesAsync();

            await _emailService.SendOtpAsync(model.Email, otp);

            return RedirectToAction("VerifyOTP",
                new { email = model.Email });
        }

        public async Task<IActionResult> VerifyOTP(string email)
        {
            var model = new VerifyOtpVM
            {
                Email = email
            };

            return View(model);
        }

       [HttpPost]
        public async Task<IActionResult> VerifyOTP(VerifyOtpVM model)
        {
            var otp = await _context.PasswordResetOTPs
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x =>
                    x.Email == model.Email &&
                    x.OTP == model.OTP &&
                    x.IsUsed == false);

            if (otp == null)
            {
                ModelState.AddModelError("", "Wrong OTP");
                return View(model);
            }

            if (otp.ExpireAt < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "OTP Expired");
                return View(model);
            }

            otp.IsUsed = true;

            await _context.SaveChangesAsync();

            TempData["Email"] = model.Email;

            return RedirectToAction("ResetPassword");
        }

        public async Task<IActionResult> ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            string? email = TempData["Email"]?.ToString();

            var user = await _userManager.FindByEmailAsync(email);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            return RedirectToAction("Login");
        }



        private async Task<IActionResult> RedirectAfterLogin(ApplicationUser user, string? returnUrl)
        {
            // Check if the returnUrl is valid and local
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(Roles.Admin))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (roles.Contains(Roles.Vendor))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Vendor" });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
        }
    }
}
