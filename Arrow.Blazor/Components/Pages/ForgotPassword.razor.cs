using System.ComponentModel.DataAnnotations;
using Arrow.Blazor.Services;
using Microsoft.AspNetCore.Components;

namespace Arrow.Blazor.Components.Pages;

public partial class ForgotPassword : ComponentBase
{
    [Inject]
    private IPasswordResetService PasswordResetService { get; set; } = default!;

    private ForgotPasswordModel _model = new();
    private bool _isSubmitting = false;
    private bool _emailSent = false;
    private string _errorMessage = string.Empty;

    private async Task HandleSubmit()
    {
        _isSubmitting = true;
        _errorMessage = string.Empty;

        try
        {
            var success = await PasswordResetService.RequestPasswordResetAsync(_model.Email);
            
            if (success)
            {
                _emailSent = true;
            }
            else
            {
                _errorMessage = "An error occurred. Please try again later.";
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

    private class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;
    }
}
