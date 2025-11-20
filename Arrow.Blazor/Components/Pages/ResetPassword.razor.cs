using System.ComponentModel.DataAnnotations;
using Arrow.Blazor.Configuration;
using Arrow.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Arrow.Blazor.Components.Pages;

public partial class ResetPassword : ComponentBase
{
    [Inject]
    private IPasswordResetService PasswordResetService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "token")]
    public string? TokenString { get; set; }

    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    private Guid _token;
    private ResetPasswordModel _model = new();
    private bool _isLoading = true;
    private bool _isValidToken = false;
    private bool _isSubmitting = false;
    private bool _passwordReset = false;
    private bool _isEmailFeatureDisabled;
    private string _errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (!FeatureToggles.IsEmailEnabled)
        {
            SetNotFoundStatus();
            return;
        }

        if (string.IsNullOrEmpty(TokenString) || !Guid.TryParse(TokenString, out _token))
        {
            _isLoading = false;
            _isValidToken = false;
            return;
        }

        _isValidToken = await PasswordResetService.ValidateResetTokenAsync(_token);
        _isLoading = false;
    }

    private async Task HandleSubmit()
    {
        if (_isEmailFeatureDisabled)
        {
            return;
        }

        if (_model.NewPassword != _model.ConfirmPassword)
        {
            _errorMessage = "Passwords do not match";
            return;
        }

        _isSubmitting = true;
        _errorMessage = string.Empty;

        try
        {
            var success = await PasswordResetService.ResetPasswordAsync(_token, _model.NewPassword);
            
            if (success)
            {
                _passwordReset = true;
            }
            else
            {
                _errorMessage = "Failed to reset password. The link may have expired.";
            }
        }
        catch (Exception)
        {
            _errorMessage = "An error occurred. Please try again later.";
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void SetNotFoundStatus()
    {
        _isEmailFeatureDisabled = true;
        _isLoading = false;
        _isValidToken = false;
        HttpContextAccessor.HttpContext?.Response.StatusCode = StatusCodes.Status404NotFound;
    }

    private class ResetPasswordModel
    {
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
