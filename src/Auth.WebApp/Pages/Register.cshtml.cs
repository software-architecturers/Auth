using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Auth.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auth.WebApp.Pages
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string ReturnUrl { get; set; }
        [BindProperty] public InputModel Input { get; set; }


        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (!ModelState.IsValid) return Page();

            var user = new ApplicationUser {UserName = Input.Email, Email = Input.Email, EmailConfirmed = true};
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public class InputModel
        {
            [Display(Name = "Email")] public string Email { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            public string ConfirmPassword { get; set; }
        }

        public class Validator : AbstractValidator<InputModel>
        {
            public Validator()
            {
                RuleFor(model => model.Email).EmailAddress().NotEmpty();
                RuleFor(model => model.Password).Length(6, 100);
                RuleFor(model => model.ConfirmPassword).Length(6, 100);
                RuleFor(model => model.Password).Equal(model => model.ConfirmPassword);
            }
        }
    }
}