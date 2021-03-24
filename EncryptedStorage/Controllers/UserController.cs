using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EncryptedStorage.Models;
using EncryptedStorage.Models.AccountViewModels;
using Microsoft.AspNetCore.Cors;
using EncryptedStorage.Service;
using EncryptedStorage.Extension;

namespace EncryptedStorage.Controllers
{
    [EnableCors("AllowMyOrigin")]
    [Route("[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IEmailSender emailSender;
        private readonly ILogger logger;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<UserController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody]LoginViewModel model)
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    logger.LogInformation("User logged in.");
                    return new OkObjectResult("Вы авторизированы");
                }
                if (result.IsLockedOut)
                {
                    logger.LogWarning("User account locked out.");
                    return new BadRequestObjectResult("Пользователь заблокирован");
                }
                else
                {
                    return new BadRequestObjectResult("Неверные данные учетной записи");
                }
            }

            // If we got this far, something failed, redisplay form
            return new BadRequestObjectResult("Модель данных некорректна");
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel model)
        {
            HttpContext.Session.Clear();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser {
                    UserName = model.UserName,
                    Email = model.Email
                };
                var userName = await userManager.FindByNameAsync(user.UserName);
                if(userName != null)
                {
                    return new BadRequestObjectResult("Пользователь с таким ID существует");
                }
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    logger.LogInformation("User created a new account with password.");

                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    await signInManager.SignInAsync(user, isPersistent: false);
                    logger.LogInformation("User created a new account with password.");
                    return new OkObjectResult("Учетная запись создана");
                }
                return new BadRequestObjectResult(result.Errors);
            }
            foreach(var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                logger.LogInformation(error.ErrorMessage);
                //logger.LogDebug(error.Exception.StackTrace);
            }
            
            // If we got this far, something failed, redisplay form
            return new BadRequestObjectResult("Модель данных некорректна");
        }

        [HttpGet]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            logger.LogInformation("User logged out.");
            return new OkObjectResult("Сессия была закрыта");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return new BadRequestObjectResult("Неполный набор данных");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new BadRequestObjectResult("Пользователя с таким ID не существует");
            }
            var result = await userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                return new OkObjectResult("Email подтвержден");
            else
                return new BadRequestObjectResult("Неверный код подтверждения");
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return new BadRequestObjectResult("Email не существует либо не подтвержден");
                }

                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                await emailSender.SendEmailAsync(model.Email, "Восстановление пароля",
                   $"Чтобы восстановить пароль нажмите на ссылку: <a href='{callbackUrl}'>ссылка</a>");
                return new BadRequestObjectResult("Сообщение для восстановления пароля отправлено");
            }

            // If we got this far, something failed, redisplay form
            return new BadRequestObjectResult("Модель данных некорректна");
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult ResetPassword(string code = null)
        //{
        //    if (code == null)
        //    {
        //        return new BadRequestObjectResult("Отсутствует код подтверждения");
        //    }
        //    var model = new ResetPasswordViewModel { Code = code };
        //    return View(model);
        //}

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult("Модель данных некорректна");
            }
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return new BadRequestObjectResult("Email не существует");
            }
            var result = await userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return new OkObjectResult("Пароль восстановлен");
            }
            return new BadRequestObjectResult(result.Errors);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ConfirmAction([FromBody]ConfirmActionViewModels model)
        {
            var user = await userManager.GetUserAsync(User);
            var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
                return new OkObjectResult("Действие принято");

            return new BadRequestObjectResult("Пароль неверный");
        }
    }
}
