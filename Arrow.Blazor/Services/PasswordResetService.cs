using Arrow.Blazor.Configuration;
using Arrow.Blazor.Data;
using Arrow.Blazor.Services.Email;
using Microsoft.AspNetCore.Http;

namespace Arrow.Blazor.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        IUserRepository userRepository,
        IPasswordResetRepository passwordResetRepository,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PasswordResetService> logger)
    {
        _userRepository = userRepository;
        _passwordResetRepository = passwordResetRepository;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        try
        {
            if (!FeatureToggles.IsEmailEnabled)
            {
                _logger.LogWarning("Password reset requested for {Email} but email delivery is disabled", email);
                return true;
            }

            var user = await _userRepository.GetByEmailAsync(email);
            
            // Always return true to prevent email enumeration attacks
            if (user == null)
            {
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
                return true;
            }

            var token = await _passwordResetRepository.CreateResetTokenAsync(user.Id);
            
            // Build reset URL
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";
            var resetUrl = $"{baseUrl}/reset-password?token={token}";

            // Send email
            var emailBody = $"""
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password. Click the link below to reset it:</p>
                <p><a href="{resetUrl}">Reset Password</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't request this, please ignore this email.</p>
                """;

            await _emailService.SendGenericEmailAsync(user.Email, "Password Reset Request", emailBody);
            _logger.LogInformation("Password reset email sent to: {Email}", email);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset request for email: {Email}", email);
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(Guid token, string newPassword)
    {
        try
        {
            var resetToken = await _passwordResetRepository.GetValidTokenAsync(token);
            
            if (resetToken == null)
            {
                _logger.LogWarning("Invalid or expired reset token: {Token}", token);
                return false;
            }

            var user = await _userRepository.GetByIdAsync(resetToken.UserId);
            if (user == null)
            {
                _logger.LogError("User not found for reset token: {Token}", token);
                return false;
            }

            // Update password
            var newPasswordHash = _passwordHasher.Hash(newPassword);
            await _userRepository.UpdatePasswordAsync(user.Id, newPasswordHash);

            // Mark token as used
            await _passwordResetRepository.MarkTokenAsUsedAsync(token);

            _logger.LogInformation("Password successfully reset for user: {UserId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for token: {Token}", token);
            return false;
        }
    }

    public async Task<bool> ValidateResetTokenAsync(Guid token)
    {
        var resetToken = await _passwordResetRepository.GetValidTokenAsync(token);
        return resetToken != null;
    }
}
