using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsBlog.Data;
using NewsBlog.Models;
using NewsBlog.Utilities;
using NewsBlog.ViewModels;
using System.Globalization;

namespace NewsBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly INotyfService _notification;
        private readonly AuditLogService _auditLogService;
        private readonly AppDbContext _db;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, INotyfService notification, AuditLogService auditLogService, AppDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notification = notification;
            _auditLogService = auditLogService;
            _db = db;
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var viewModel = users.Select(x => new UserViewModel()
            {
                Id = x.Id,
                UserName = x.UserName,
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Suspended = x.Suspended // Include suspended status
            }).ToList();

            // Fetching role
            foreach (var user in viewModel)
            {
                var getUser = await _userManager.FindByIdAsync(user.Id!);
                var role = await _userManager.GetRolesAsync(getUser!);
                user.Role = role.FirstOrDefault();
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }

            // Trim spaces and capitalize first letters
            registerViewModel.Email = registerViewModel.Email?.Trim();
            registerViewModel.UserName = registerViewModel.UserName?.Trim();
            registerViewModel.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(registerViewModel.FirstName?.Trim().ToLower());
            registerViewModel.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(registerViewModel.LastName?.Trim().ToLower());

            var checkUserEmail = await _userManager.FindByEmailAsync(registerViewModel.Email!);
            if (checkUserEmail != null)
            {
                _notification.Error("Email already exists");
                return View(registerViewModel);
            }

            var checkUsername = await _userManager.FindByNameAsync(registerViewModel.UserName!);
            if (checkUsername != null)
            {
                _notification.Error("Username is taken");
                return View(registerViewModel);
            }

            var newUser = new User()
            {
                Email = registerViewModel.Email,
                UserName = registerViewModel.UserName,
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName
            };

            var result = await _userManager.CreateAsync(newUser, registerViewModel.Password!);
            if (result.Succeeded)
            {
                string role = Roles.Author; // Default role is Author
                if (User.IsInRole(Roles.SuperAdmin!) && registerViewModel.IsAdmin)
                {
                    await _userManager.AddToRoleAsync(newUser, Roles.Admin!);
                    role = Roles.Admin;
                }
                else
                {
                    await _userManager.AddToRoleAsync(newUser, Roles.Author!);
                }
                _notification.Success("User is created!");

                // Log the creation of the user with role details
                await _auditLogService.LogAsync("User Created", $"User <strong>{newUser.UserName}</strong> was created by <strong>{User.Identity!.Name}</strong> and given the <strong>{role}</strong> role.");

                return RedirectToAction("Index", "User", new { area = "Admin" });
            }
            else
            {
                _notification.Error("Password should contain at least 8 letters, a capital letter and a special character.");
            }
            return View(registerViewModel);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> PasswordReset(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _notification.Error("User not found");
                return RedirectToAction(nameof(Index));
            }
            var viewModel = new PasswordResetViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> PasswordReset(PasswordResetViewModel passwordResetViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(passwordResetViewModel);
            }

            var user = await _userManager.FindByIdAsync(passwordResetViewModel.Id!);
            if (user == null)
            {
                _notification.Error("User not found");
                return View(passwordResetViewModel);
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, passwordResetViewModel.OldPassword!);
            if (!passwordCheck)
            {
                _notification.Error("Old password is incorrect");
                return View(passwordResetViewModel);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, passwordResetViewModel.NewPassword!);
            if (result.Succeeded)
            {
                _notification.Success("Password successfully reset");

                // Log the password reset
                await _auditLogService.LogAsync("Password Reset", $"Password for user <strong>{user.UserName}</strong> was reset by <strong>{User.Identity!.Name}</strong>.");

                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(passwordResetViewModel);
            }
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            if (!HttpContext.User.Identity!.IsAuthenticated)
            {
                return View(new LoginViewModel());
            }
            return RedirectToAction("Index", "Post", new { area = "Admin" });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }
            var findUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginViewModel.Username!.ToLower());
            if (findUser == null)
            {
                _notification.Error("Username not found");
                return View(loginViewModel);
            }
            if (findUser.Suspended)
            {
                _notification.Error("Your account is suspended.");
                return View(loginViewModel);
            }
            var verifyPassword = await _userManager.CheckPasswordAsync(findUser, loginViewModel.Password!);
            if (!verifyPassword)
            {
                _notification.Error("Wrong password");
                return View(loginViewModel);
            }
            await _signInManager.PasswordSignInAsync(loginViewModel.Username!, loginViewModel.Password!, loginViewModel.RememberMe, true);
            _notification.Success("Logged in!");

            // Log the login
            await _auditLogService.LogAsync("User Logged In", $"User <strong>{findUser.UserName}</strong> logged in.");

            return RedirectToAction("Index", "Post", new { area = "Admin" });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            // Log the logout
            await _auditLogService.LogAsync("User Logged Out", $"User <strong>{User.Identity!.Name}</strong> logged out.");

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpGet("NoAccess")]
        [Authorize]
        public IActionResult NoAccess()
        {
            return View();
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _notification.Error("User not found");
                return RedirectToAction(nameof(Index));
            }

            // Check the role of the logged-in user
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser!);

            if (user.Id == currentUser?.Id)
            {
                _notification.Error("You cannot delete yourself");
                return RedirectToAction(nameof(Index));
            }

            if (currentUserRoles.Contains(Roles.SuperAdmin!) || (currentUserRoles.Contains(Roles.Admin!) && (await _userManager.IsInRoleAsync(user, Roles.Author!))))
            {
                // Nullify the UserId in related posts before deleting the user
                var relatedPosts = await _db.Posts.Where(p => p.UserId == user.Id).ToListAsync();
                foreach (var post in relatedPosts)
                {
                    post.UserId = null;
                    post.AuthorName = $"{user.FirstName} {user.LastName}";
                }
                await _db.SaveChangesAsync();

                // Delete the user
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _notification.Success("User deleted successfully");

                    // Log the deletion of the user
                    await _auditLogService.LogAsync("User Deleted", $"User <strong>{user.UserName}</strong> was deleted by <strong>{User.Identity!.Name}</strong>.");
                }
                else
                {
                    _notification.Error("Permission denied!");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> ToggleSuspend(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _notification.Error("User not found");
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser!);

            if (user.Id == currentUser?.Id)
            {
                _notification.Error("You cannot suspend yourself");
                return RedirectToAction(nameof(Index));
            }

            if (currentUserRoles.Contains(Roles.SuperAdmin!) || (currentUserRoles.Contains(Roles.Admin!) && (await _userManager.IsInRoleAsync(user, Roles.Author!))))
            {
                user.Suspended = !user.Suspended; // Toggle the suspended status
                await _userManager.UpdateAsync(user);
                _notification.Success($"User {(user.Suspended ? "suspended" : "unsuspended")} successfully");

                // Log the suspension/unsuspension of the user
                await _auditLogService.LogAsync(user.Suspended ? "User Suspended" : "User Unsuspended", $"User <strong>{user.UserName}</strong> was {(user.Suspended ? "suspended" : "unsuspended")} by <strong>{User.Identity!.Name}</strong>.");
            }
            else
            {
                _notification.Error("Permission denied!");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
