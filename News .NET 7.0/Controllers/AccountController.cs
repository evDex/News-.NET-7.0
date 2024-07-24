using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using News.BLL.DTO;
using News.BLL.Interfaces;
using News.Infrastructure.Common;
using News.Models;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace News.Controllers
{
    public class AccountController : Controller
    {
        IUserService userService;
        public AccountController(IUserService serv)
        {
            userService = serv;
        }

        #region Login
        [HttpGet]
        [Route("Login")]
        public IActionResult Login(string returnUrl)
        {
            return PartialView("Login");
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginViewModel logon, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                UserDTO userDto = new UserDTO { Email = logon.Email, Password = logon.Password };

                var response = await userService.LoginAsync(userDto, CancelTask.GetToken());
                if (response.StatusCode == BLL.Infrastructure.StatusCode.OK)
                {
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(response.Data));

                    if (!string.IsNullOrEmpty(ReturnUrl))
                    {
                        return LocalRedirect(ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                TempData["Error"] = response.Description;
            }
            
            return View(logon);
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Register
        [HttpGet]
        [Route("Register")]
        public IActionResult Register()
        {
            return PartialView("Register");
        }
        [HttpPost]
        [Route("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel reg)
        {
            if (ModelState.IsValid)
            {
                CancellationToken token = CancelTask.GetToken();
                UserDTO userDto = new UserDTO
                {
                    UserName = reg.Login,
                    Email = reg.Email,
                    Password = reg.Password,
                    AvatarPath = "/resources/UserAvatars/default-avatar.png"
                };

                var response = await userService.RegisterAsync(userDto,token);
                if (response.StatusCode == BLL.Infrastructure.StatusCode.OK)
                {
                    //авторизация
                    var identity = await userService.LoginAsync(userDto, token);
                    if (identity.Data != null)
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity.Data));
                    return RedirectToAction("Index", "Home");
                }
                TempData["Error"] = response.Description;

            }
            return View(reg);
        }
        #endregion
    }
}
