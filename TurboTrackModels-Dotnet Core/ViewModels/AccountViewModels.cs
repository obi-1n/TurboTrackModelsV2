using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using TurboTrackModels_Dotnet_Core.Models;

namespace TurboTrackModels_Dotnet_Core.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Town { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string PostCode { get; set; } = string.Empty;

    [Required, Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class VerifyCodeViewModel
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
    public bool RememberMe { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ExternalLoginConfirmationViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)]
    public string OldPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class SetPasswordViewModel
{
    [Required, DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AddPhoneNumberViewModel
{
    [Required, Phone]
    public string Number { get; set; } = string.Empty;
}

public class VerifyPhoneNumberViewModel
{
    [Required, Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}

public class IndexViewModel
{
    public bool HasPassword { get; set; }
    public string? PhoneNumber { get; set; }
    public bool TwoFactor { get; set; }
    public IList<string> Logins { get; set; } = [];
    public string? StatusMessage { get; set; }
}

public class ManageLoginsViewModel
{
    public IList<UserLoginInfo> CurrentLogins { get; set; } = [];
    public IList<AuthenticationSchemeViewModel> OtherLogins { get; set; } = [];
    public string? StatusMessage { get; set; }
}

public class AuthenticationSchemeViewModel
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class ExternalLoginListViewModel
{
    public string? ReturnUrl { get; set; }
}

public class AddStaffViewModel
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public IEnumerable<SelectListItem> Roles { get; set; } = [];
}

public class StaffListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class AddressViewModel
{
    [Required]
    public string Street { get; set; } = string.Empty;

    [Required]
    public string Town { get; set; } = string.Empty;

    [Required]
    public string PostCode { get; set; } = string.Empty;

    [Required, Phone]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ReturnRequestViewModel
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalCost { get; set; }
    public List<OrderItem> OrderItems { get; set; } = [];

    [Required, StringLength(1000)]
    public string ReturnReason { get; set; } = string.Empty;
}
