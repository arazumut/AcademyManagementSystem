using System.Security.Claims;
using AYS.Entities;
using AYS.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AYS.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    // GET: /Account/Login
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string returnUrl = null)
    {
        // Temizle önceki giriş çerez bilgilerini
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                return View(model);
            }
            
            // Oturum açma girişimi
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Kullanıcı başarıyla giriş yaptı: {Email}", model.Email);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                _logger.LogWarning("Başarısız giriş denemesi: {Email}, Sonuç: {@Result}", model.Email, result);
                
                if (result.IsLockedOut)
                {
                    return RedirectToAction(nameof(Lockout));
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Bu hesap henüz onaylanmamış.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
                }
            }
        }

        // Giriş başarısız olduysa, formu tekrar göster
        return View(model);
    }

    // GET: /Account/Register
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var model = new RegisterViewModel
        {
            Roles = new List<string> { "Student", "Educator" }
        };
        return View(model);
    }

    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser 
            { 
                UserName = model.Email, 
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true // E-posta doğrulamasını atla (geliştirme aşamasında)
            };
            
            // Check if role exists
            string selectedRole = !string.IsNullOrEmpty(model.Role) ? model.Role : "Student";
            if (!await _roleManager.RoleExistsAsync(selectedRole))
            {
                // Role doesn't exist, create it
                await _roleManager.CreateAsync(new IdentityRole(selectedRole));
                _logger.LogInformation("Rol oluşturuldu: {Role}", selectedRole);
            }
            
            // Create user
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Kullanıcıya seçtiği rolü ata
                var roleResult = await _userManager.AddToRoleAsync(user, selectedRole);
                if (!roleResult.Succeeded)
                {
                    // Role assignment failed
                    _logger.LogError("Rol atama başarısız: {UserId}, {Role}, {@Errors}", 
                        user.Id, selectedRole, roleResult.Errors);
                    
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    
                    // Delete the user since role assignment failed
                    await _userManager.DeleteAsync(user);
                    return View(model);
                }

                _logger.LogInformation("Kullanıcı hesabı oluşturuldu ve {Role} rolü atandı.", selectedRole);
                
                // Kullanıcıyı oturum aç
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("Kullanıcı oturum açtı: {Email}", user.Email);
                
                return RedirectToLocal(returnUrl);
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Hata olduysa, modeli tekrar göster
        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Kullanıcı oturumu kapatıldı.");
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    // GET: /Account/Lockout
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Lockout()
    {
        return View();
    }

    // GET: /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // Yardımcı metodlar
    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
} 