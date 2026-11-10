using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TurboTrackModels_Dotnet_Core.Models;
using TurboTrackModels_Dotnet_Core.ViewModels;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize]
public class ManageController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IAuthenticationSchemeProvider schemeProvider) : Controller
{
    public async Task<IActionResult> Index(string? message = null)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return NotFound();
        }

        var logins = await userManager.GetLoginsAsync(user);
        var model = new IndexViewModel
        {
            HasPassword = await userManager.HasPasswordAsync(user),
            PhoneNumber = user.PhoneNumber,
            TwoFactor = await userManager.GetTwoFactorEnabledAsync(user),
            Logins = logins.Select(login => login.ProviderDisplayName ?? login.LoginProvider).ToList(),
            StatusMessage = message
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLogin(string loginProvider, string providerKey)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.RemoveLoginAsync(user, loginProvider, providerKey);
        if (!result.Succeeded)
        {
            return RedirectToAction(nameof(ManageLogins), new { message = "Error" });
        }

        await signInManager.RefreshSignInAsync(user);
        return RedirectToAction(nameof(ManageLogins), new { message = "RemoveLoginSuccess" });
    }

    [HttpGet]
    public IActionResult ChangePassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(model);
        }

        await signInManager.RefreshSignInAsync(user);
        return RedirectToAction(nameof(Index), new { message = "ChangePasswordSuccess" });
    }

    [HttpGet]
    public IActionResult SetPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.AddPasswordAsync(user, model.NewPassword);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(model);
        }

        await signInManager.RefreshSignInAsync(user);
        return RedirectToAction(nameof(Index), new { message = "SetPasswordSuccess" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableTwoFactorAuthentication()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return NotFound();
        }

        EnsureSucceeded(await userManager.SetTwoFactorEnabledAsync(user, true));
        await signInManager.RefreshSignInAsync(user);
        return RedirectToAction(nameof(Index), new { message = "SetTwoFactorSuccess" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableTwoFactorAuthentication()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return NotFound();
        }

        EnsureSucceeded(await userManager.SetTwoFactorEnabledAsync(user, false));
        await signInManager.RefreshSignInAsync(user);
        return RedirectToAction(nameof(Index), new { message = "SetTwoFactorSuccess" });
    }

    [HttpGet]
    public IActionResult AddPhoneNumber() =>
        View(new AddPhoneNumberViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddPhoneNumber(AddPhoneNumberViewModel model)
    {
        ModelState.AddModelError(string.Empty, "Phone verification is unavailable until an SMS sender is configured.");
        return View(model);
    }

    [HttpGet]
    public IActionResult VerifyPhoneNumber(string phoneNumber) =>
        string.IsNullOrWhiteSpace(phoneNumber) ? BadRequest() : View(new VerifyPhoneNumberViewModel
        {
            PhoneNumber = phoneNumber
        });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
    {
        ModelState.AddModelError(string.Empty, "Phone verification is unavailable until an SMS sender is configured.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePhoneNumber()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return NotFound();
        }

        EnsureSucceeded(await userManager.SetPhoneNumberAsync(user, null));
        await signInManager.RefreshSignInAsync(user);
        return RedirectToAction(nameof(Index), new { message = "RemovePhoneSuccess" });
    }

    public async Task<IActionResult> ManageLogins(string? message = null)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return NotFound();
        }

        var current = await userManager.GetLoginsAsync(user);
        var schemes = await schemeProvider.GetAllSchemesAsync();
        var model = new ManageLoginsViewModel
        {
            CurrentLogins = current,
            OtherLogins = schemes
                .Where(scheme => !string.IsNullOrEmpty(scheme.DisplayName) &&
                                 current.All(login => login.LoginProvider != scheme.Name))
                .Select(scheme => new AuthenticationSchemeViewModel
                {
                    Name = scheme.Name,
                    DisplayName = scheme.DisplayName!
                })
                .ToList(),
            StatusMessage = message
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LinkLogin(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return BadRequest();
        }

        var redirectUrl = Url.Action(nameof(LinkLoginCallback));
        var properties = signInManager.ConfigureExternalAuthenticationProperties(
            provider, redirectUrl, userManager.GetUserId(User));
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> LinkLoginCallback()
    {
        var user = await GetCurrentUserAsync();
        var info = await signInManager.GetExternalLoginInfoAsync(userManager.GetUserId(User));
        if (user is null || info is null)
        {
            return RedirectToAction(nameof(ManageLogins), new { message = "Error" });
        }

        var result = await userManager.AddLoginAsync(user, info);
        if (result.Succeeded)
        {
            await signInManager.RefreshSignInAsync(user);
            return RedirectToAction(nameof(ManageLogins), new { message = "AddLoginSuccess" });
        }

        AddErrors(result);
        return RedirectToAction(nameof(ManageLogins), new { message = "Error" });
    }

    private Task<User?> GetCurrentUserAsync() => userManager.GetUserAsync(User);

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }
}
