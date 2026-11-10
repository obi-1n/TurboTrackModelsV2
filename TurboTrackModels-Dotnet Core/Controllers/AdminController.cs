using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;
using TurboTrackModels_Dotnet_Core.ViewModels;

namespace TurboTrackModels_Dotnet_Core.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    TTMDbContext db,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    public IActionResult Dashboard() => View();

    public async Task<IActionResult> StaffList()
    {
        var users = await userManager.Users.OrderBy(user => user.LastName).ToListAsync();
        var staff = new List<StaffListViewModel>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains("Customer"))
            {
                continue;
            }

            staff.Add(new StaffListViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Role = roles.FirstOrDefault() ?? "-"
            });
        }

        return View(staff.OrderBy(person => person.Role)
            .ThenBy(person => person.FirstName)
            .ThenBy(person => person.LastName)
            .ToList());
    }

    public IActionResult AddStaff() => View(new AddStaffViewModel { Roles = GetAssignableRoles() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStaff(AddStaffViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Roles = GetAssignableRoles();
            return View(model);
        }

        if (model.Role == "Customer" || !await roleManager.RoleExistsAsync(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Select a valid staff role.");
            model.Roles = GetAssignableRoles();
            return View(model);
        }

        var localPart = string.Concat(model.FirstName, model.LastName)
            .Where(char.IsLetterOrDigit)
            .ToArray();
        if (localPart.Length == 0)
        {
            ModelState.AddModelError(nameof(model.FirstName), "Enter a name containing letters or numbers.");
            model.Roles = GetAssignableRoles();
            return View(model);
        }

        var userName = await EnsureUniqueUserNameAsync($"{new string(localPart)}@TTM.com");
        var user = new User
        {
            UserName = userName,
            Email = userName,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddErrors(createResult);
            model.Roles = GetAssignableRoles();
            return View(model);
        }

        var roleResult = await userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            EnsureIdentitySuccess(await userManager.DeleteAsync(user));
            AddErrors(roleResult);
            model.Roles = GetAssignableRoles();
            return View(model);
        }

        db.Staff.Add(new StaffProfile { UserId = user.Id });
        await db.SaveChangesAsync();
        TempData["Success"] = $"Staff member {user.FirstName} {user.LastName} created with username {user.UserName}.";
        return RedirectToAction(nameof(StaffList));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStaff(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        if (userManager.GetUserId(User) == id)
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(StaffList));
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(StaffList));
        }

        if (await userManager.IsInRoleAsync(user, "Customer"))
        {
            TempData["Error"] = "Cannot delete customer accounts here.";
            return RedirectToAction(nameof(StaffList));
        }

        var result = await userManager.DeleteAsync(user);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Staff member removed."
            : string.Join("; ", result.Errors.Select(error => error.Description));
        return RedirectToAction(nameof(StaffList));
    }

    private IEnumerable<SelectListItem> GetAssignableRoles() =>
        roleManager.Roles
            .Where(role => role.Name != "Customer")
            .OrderBy(role => role.Name)
            .Select(role => new SelectListItem(role.Name, role.Name))
            .ToList();

    private async Task<string> EnsureUniqueUserNameAsync(string desiredUserName)
    {
        var candidate = desiredUserName;
        var suffix = 1;
        var atIndex = desiredUserName.LastIndexOf('@');
        while (await userManager.FindByNameAsync(candidate) is not null)
        {
            candidate = $"{desiredUserName[..atIndex]}{suffix}{desiredUserName[atIndex..]}";
            suffix++;
        }

        return candidate;
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private static void EnsureIdentitySuccess(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }
}
