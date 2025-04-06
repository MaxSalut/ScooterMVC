using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScooterDomain.Model;
using ScooterInfrastructure.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ScooterInfrastructure;

namespace ScooterInfrastructure.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ScootersContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ScootersContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Перевірка унікальності номера телефону
                var existingRider = await _context.Riders
                    .FirstOrDefaultAsync(r => r.PhoneNumber == model.PhoneNumber);
                if (existingRider != null)
                {
                    ModelState.AddModelError("PhoneNumber", "Цей номер телефону вже використовується.");
                    return View(model);
                }

                // Створюємо нового користувача ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Створюємо запис у таблиці Rider
                    var rider = new Rider
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        RegistrationDate = DateOnly.FromDateTime(DateTime.Now),
                        AccountBalance = 0,
                        ApplicationUserId = user.Id // Зв'язок з ApplicationUser
                    };

                    _context.Riders.Add(rider);
                    await _context.SaveChangesAsync();

                    // Оновлюємо RiderId у ApplicationUser
                    user.RiderId = rider.Id;
                    await _userManager.UpdateAsync(user);

                    // Додаємо RiderId до Claims
                    await _userManager.AddClaimAsync(user, new Claim("RiderId", user.RiderId.ToString()));

                    // Додаємо користувача до ролі "User"
                    await _userManager.AddToRoleAsync(user, "User");

                    // Виконуємо вхід користувача
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Scooters");
                    }
                    return RedirectToAction("Index", "Scooters");
                }
                ModelState.AddModelError(string.Empty, "Невірний email або пароль.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}