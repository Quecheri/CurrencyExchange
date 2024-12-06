using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace CurrencyExchange.Controllers
{



    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("AlreadyLoggedIn");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("AlreadyLoggedIn");
            }
            return View();
        }

        public IActionResult AlreadyLoggedIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("AlreadyLoggedIn");
            }

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = username };
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewData["Username"] = username;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("AlreadyLoggedIn");
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            ViewData["Username"] = username;
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }



}
