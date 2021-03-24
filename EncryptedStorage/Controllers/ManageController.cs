using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EncryptedStorage.Models;
using EncryptedStorage.Models.ManageViewModels;
using Microsoft.AspNetCore.Cors;
using EncryptedStorage.Service;
using EncryptedStorage.Extension;

namespace EncryptedStorage.Controllers
{
    [EnableCors("AllowMyOrigin")]
    //[Authorize]
    [Route("[controller]/[action]")]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IEmailSender emailSender;
        private readonly ILogger logger;
        private readonly UrlEncoder urlEncoder;

        private const string AuthenicatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public ManageController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IEmailSender emailSender,
          ILogger<ManageController> logger,
          UrlEncoder urlEncoder)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.logger = logger;
            this.urlEncoder = urlEncoder;
        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            ApplicationUser user;
            try
            {
                user = await userManager.FindByIdAsync(userManager.GetUserId(User));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message + ex.StackTrace);
            }
            
            if (user == null)
            {
                return new BadRequestObjectResult("Пользователь не найден");
            }

            return new OkObjectResult(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(EmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult("Модель данных некорректна");
            }

            var user = await this.userManager.GetUserAsync(User);
            if (user == null)
            {
                return new BadRequestObjectResult("Пользователь не найден");
            }

            var email = user.Email;
            if (model.Email != email)
            {
                var setEmailResult = await this.userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    return new BadRequestObjectResult("Произошла непредвиденная ошибка при настройке email");
                }
            }

            return new OkObjectResult("Ваша учетная запись обновлена");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail(EmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult("Модель данных некорректна");
            }

            var user = await this.userManager.GetUserAsync(User);
            if (user == null)
            {
                return new BadRequestObjectResult("Пользователь не найден");
            }

            var code = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
            var email = user.Email;
            await this.emailSender.SendEmailConfirmationAsync(email, callbackUrl);

            return new OkObjectResult("Письмо с подтверждением отправлено. Пожалуйста, проверьте вашу электронную почту.");
        }

        //[HttpGet]
        //public async Task<IActionResult> ChangePassword()
        //{
        //    var user = await this.userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load user with ID '{this.userManager.GetUserId(User)}'.");
        //    }

        //    var hasPassword = await this.userManager.HasPasswordAsync(user);
        //    if (!hasPassword)
        //    {
        //        return RedirectToAction(nameof(SetPassword));
        //    }

        //    var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
        //    return View(model);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult("Модель данных некорректна");
            }

            var user = await this.userManager.GetUserAsync(User);
            if (user == null)
            {
                return new BadRequestObjectResult("Пользователь не найден");
            }

            var changePasswordResult = await this.userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                return new BadRequestObjectResult(changePasswordResult.Errors);
            }

            await this.signInManager.SignInAsync(user, isPersistent: false);
            this.logger.LogInformation("User changed their password successfully.");

            return new OkObjectResult("Ваш пароль успешно изменен");
        }

        //[HttpGet]
        //public async Task<IActionResult> SetPassword()
        //{
        //    var user = await this.userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load user with ID '{this.userManager.GetUserId(User)}'.");
        //    }

        //    var hasPassword = await this.userManager.HasPasswordAsync(user);

        //    if (hasPassword)
        //    {
        //        return RedirectToAction(nameof(ChangePassword));
        //    }

        //    var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var user = await this.userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load user with ID '{this.userManager.GetUserId(User)}'.");
        //    }

        //    var addPasswordResult = await this.userManager.AddPasswordAsync(user, model.NewPassword);
        //    if (!addPasswordResult.Succeeded)
        //    {
        //        AddErrors(addPasswordResult);
        //        return View(model);
        //    }

        //    await this.signInManager.SignInAsync(user, isPersistent: false);
        //    StatusMessage = "Your password has been set.";

        //    return RedirectToAction(nameof(SetPassword));
        //}

        #region Helpers



        //private void AddErrors(IdentityResult result)
        //{
        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError(string.Empty, error.Description);
        //    }
        //}

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        #endregion
    }
}
