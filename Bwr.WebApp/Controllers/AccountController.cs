using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using BWR.Infrastructure.Common;
using BWR.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BWR.ShareKernel.Interfaces;
using Bwr.WebApp.Identity;
using BWR.Domain.Model.Security;
using Bwr.WebApp.Models;
using System.Security.Claims;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using BWR.ShareKernel.Permisions;
using System.Reflection;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Treasures;

namespace Bwr.WebApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser, Guid> _userManager;
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IGenericRepository<Permission> _permissionRepository;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IGenericRepository<BWR.Domain.Model.Treasures.UserTreasuery> _userTreasuryRepository;
        private readonly IGenericRepository<BWR.Domain.Model.Treasures.Treasury> _treasuryRepository;

        public AccountController(UserManager<IdentityUser, Guid> userManager,
            IUnitOfWork<MainContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleRepository = new GenericRepository<Role>(_unitOfWork);
            _permissionRepository = new GenericRepository<Permission>(_unitOfWork);
            _userTreasuryRepository = new GenericRepository<BWR.Domain.Model.Treasures.UserTreasuery>(_unitOfWork);
            _treasuryRepository = new GenericRepository<BWR.Domain.Model.Treasures.Treasury>(_unitOfWork);
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResetDataBase()
        {
            
            
            MainContext context = new MainContext();
            context.Database.ExecuteSqlCommand("sp_EmptyDB"); 
            #region admin user
            var adminRole = context.Roles.FirstOrDefault(x => x.Name.ToLower() == "admin");
            if (adminRole == null)
            {
                adminRole = new Role()
                {
                    Name = "admin",
                    RoleId = Guid.NewGuid()
                };

                context.SaveChanges();
            }
            else
            {
                var permissions = context.Permissions.Where(x => x.Role.Name.ToLower() == "admin");
                context.Permissions.RemoveRange(permissions);
                context.SaveChanges();
            }
            var options = typeof(AppPermision).GetFields(BindingFlags.Public | BindingFlags.Static |
                     BindingFlags.FlattenHierarchy).
                     Where(fi => fi.IsLiteral && !fi.IsInitOnly).Select(x => x.GetValue(null).ToString()).ToList();

            foreach (var option in options)
            {
                var permission = new Permission()
                {
                    Name = option,
                    GrantedByUser = "",
                    GrantedDate = DateTime.Now,
                    Role = adminRole
                };

                context.Permissions.Add(permission);
            }
            context.SaveChanges();

            var adminUserExist = context.Users.Any(x => x.UserName.ToLower() == "admin");
            if (!adminUserExist)
            {
                var admin = new User()
                {
                    UserId = Guid.NewGuid(),
                    FullName = "Admin",
                    UserName = "admin",
                    //admin
                    PasswordHash = "ANF2VrSJI/ZRaK99ymBl/xHO+jkzMVQOogsWHHCpemIEzQBL5AHqcGnlhDysgiDwtg==",
                    SecurityStamp = "718b64fc-3be0-4467-af63-09ca67e911aa",
                    Email = "admin@bwire.com"
                };
                admin.Roles.Add(adminRole);

                context.Users.Add(admin);
                context.SaveChanges();
            }
            #endregion
            #region country seeder
            //if (!context.Countrys.Any())
            //{
            //    var syria = new Country()
            //    {
            //        Name = "سوريا",
            //        IsEnabled = true,
            //    };
            //    var iraq = new Country()
            //    {
            //        Name = "العراق",
            //        IsEnabled = true,
            //    };
            //    context.Countrys.AddRange(new[] { syria, iraq });
            //    context.SaveChanges();
            //}
            #endregion
            #region mainBranch
            if (!context.Branchs.Any())
            {

                var mainBranch = new BWR.Domain.Model.Branches.Branch()
                {
                    Name = "الفرع الرئيسي ",
                    Address = "",
                };
                context.Branchs.Add(mainBranch);
                context.SaveChanges();
            }
            if (!context.Treasurys.Any())
            {
                var brancheId = context.Branchs.First().Id;
                var trusery = new BWR.Domain.Model.Treasures.Treasury()
                {
                    Name = "الصندوق الرئيسي",
                    IsMainTreasury = true,
                    IsAvilable = false,
                    BranchId = brancheId,
                    IsEnabled = true,
                };
                context.Treasurys.Add(trusery);
                var userTreusery = new UserTreasuery()
                {
                    TreasuryId = trusery.Id,
                    UserId = context.Users.First().UserId,
                    IsDeleted = false,
                    CreatedBy = "النظام",
                    Created = DateTime.Now,
                };
                context.UserTreasueries.Add(userTreusery);
            }
            #endregion
            #region coin
            //if (!context.Coins.Any())
            //{
            //    var dollar = new Coin()
            //    {
            //        Name = "دولار",
            //        IsEnabled = true,
            //    };
            //    var irDinar = new Coin()
            //    {

            //        Name = "دينار عراقي",
            //        IsEnabled = true
            //    };
            //    context.Coins.AddRange(new[] { dollar, irDinar });
            //    context.SaveChanges();
            //    var total = 1000000;
            //    var brancheId = context.Branchs.First().Id;
            //    var sBranchCash = new BranchCash()
            //    {
            //        CoinId = dollar.Id,
            //        BranchId = brancheId,
            //        InitialBalance = total,
            //        Total = total,
            //    };
            //    var iBranchCash = new BranchCash()
            //    {
            //        BranchId = brancheId,
            //        CoinId = irDinar.Id,
            //        Total = total,
            //        InitialBalance = total
            //    };
            //    context.BranchCashs.AddRange(new[] { iBranchCash, sBranchCash });
            //    context.SaveChanges();
            //    var trusery = new BWR.Domain.Model.Treasures.Treasury()
            //    {
            //        Name = "الصندوق الرئيسي",
            //        IsMainTreasury = true,
            //        IsAvilable = false,
            //        BranchId = brancheId,
            //        IsEnabled = true,
            //    };
            //    context.Treasurys.Add(trusery);
            //    var itruseryCash = new TreasuryCash()
            //    {
            //        CoinId = dollar.Id,
            //        Total = total,
            //        IsEnabled = true,
            //        IsDeleted = false,
            //    };
            //    var struseryCash = new TreasuryCash()
            //    {
            //        CoinId = irDinar.Id,
            //        Total = total,
            //        IsEnabled = true,
            //        IsDeleted = false,
            //    };
            //    context.TreasuryCashs.Add(itruseryCash);
            //    context.TreasuryCashs.Add(struseryCash);
            //    var userTreusery = new UserTreasuery()
            //    {
            //        TreasuryId = trusery.Id,
            //        UserId = context.Users.First().UserId,
            //        IsDeleted = false,
            //        CreatedBy = "النظام",
            //        Created = DateTime.Now,
            //    };
            //    context.UserTreasueries.Add(userTreusery);
            //    context.SaveChanges();
            //}
            #endregion
            context.SaveChanges();
            //context.Database.ExecuteSqlCommand("sp_MSforeachtable   @command1 = 'DBCC CHECKIDENT(''?'', RESEED, 1)");
            return RedirectToAction("Login");
        }
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    var roles = _roleRepository.FindBy(x => x.Users.Any(y => y.UserName == user.UserName)).Select(x => x.Name).ToList();
                    var permissions = _permissionRepository.FindBy(x => roles.Contains(x.Role.Name)).Select(x => x.Name).ToList();
                    Session["UserPermissions"] = permissions;

                    var userTreasuries = _userTreasuryRepository.FindBy(x => x.UserId == user.Id && x.DeliveryDate == null);
                    if (userTreasuries.Any())
                    {
                        var treasury = userTreasuries.LastOrDefault().Treasury;
                        Session["CurrentTreasury"] = treasury.Id;
                        if (treasury.IsMainTreasury)
                        {
                            Session["MainTreasury"] = treasury.Id;
                        }
                        else
                        {
                            var mainTreasury = _treasuryRepository.FindBy(x => x.IsMainTreasury).FirstOrDefault();
                            if (mainTreasury != null)
                            {
                                Session["MainTreasury"] = mainTreasury.Id;
                            }
                        }
                    }
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser() { UserName = model.UserName,FullName=model.FullName,Email=model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await _userManager.RemoveLoginAsync(getGuid(User.Identity.GetUserId()), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await _userManager.ChangePasswordAsync(getGuid(User.Identity.GetUserId()), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await _userManager.AddPasswordAsync(getGuid(User.Identity.GetUserId()), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await _userManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await _userManager.AddLoginAsync(getGuid(User.Identity.GetUserId()), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new IdentityUser() { UserName = model.UserName };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpGet]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            Session["UserPermissions"] = new List<string>();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = _userManager.GetLogins(getGuid(User.Identity.GetUserId()));
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userIdentity = await _userManager.FindByNameAsync(User.Identity.Name);
            var result = await _userManager.ChangePasswordAsync(userIdentity.Id, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToAction("Index","Home");
            }

            AddErrors(result);
            return View(model);
        }

        
        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(IdentityUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = _userManager.FindById(getGuid(User.Identity.GetUserId()));
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        private Guid getGuid(string value)
        {
            var result = default(Guid);
            Guid.TryParse(value, out result);
            return result;
        }
        #endregion
    }

   
}