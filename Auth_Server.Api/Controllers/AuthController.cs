using Authetication.Server.Api.DTOs;
using Authetication.Server.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Authetication.Server.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;
        private readonly IEmailService _emailService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService, IUsuarioService usuarioService, IEmailService emailService)
        {
            _logger = logger;
            _authService = authService;
            _usuarioService = usuarioService;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UsuarioDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
                return BadRequest("Invalid username or password");

            try
            {
                _logger.LogInformation($"Attempting login for user: {loginDto.Username}");
                var token = await _authService.Authenticate(loginDto.Username, loginDto.Password);
                if (token == null)
                {
                    _logger.LogWarning($"Login failed for user: {loginDto.Username}");
                    return Unauthorized();
                }

                _logger.LogInformation($"Login successful for user: {loginDto.Username}");
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword(int userId, [FromBody] ChangePasswordDto changePasswordDto)
        {
            if (changePasswordDto == null || string.IsNullOrWhiteSpace(changePasswordDto.OldPassword) || string.IsNullOrWhiteSpace(changePasswordDto.NewPassword))
            {
                return BadRequest("Invalid password data");
            }

            try
            {
                var user = await _usuarioService.GetUsuarioById(userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.Password);

                if (!isOldPasswordValid)
                {
                    return BadRequest("Invalid old password");
                }

                string hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                user.Password = hashedNewPassword;
                await _usuarioService.UpdateUsuario(user);

                return Ok("Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing the password.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (forgotPasswordDto == null || string.IsNullOrWhiteSpace(forgotPasswordDto.Email))
                return BadRequest("Email is required");

            try
            {
                var user = await _usuarioService.GetUserByEmail(forgotPasswordDto.Email);

                if (user == null)
                {
                    _logger.LogWarning($"Password reset requested for non-existent user: {forgotPasswordDto.Email}");
                    return NotFound("User not found");
                }

                var resetToken = _authService.GeneratePasswordResetToken(user);
                var resetLink = Url.Action(nameof(ResetPassword), "Auth", new { token = resetToken, email = user.Username }, Request.Scheme);

                // Defina o assunto e o corpo do email
                var subject = "Password Reset Request";
                var body = $"Click the link to reset your password: {resetLink}";

                // Envia o email usando o serviço de email adaptado
                await _emailService.SendEmailAsync(user.Username, subject, body);

                _logger.LogInformation($"Password reset link sent to: {forgotPasswordDto.Email}");
                return Ok("Password reset link has been sent to your email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while requesting a password reset.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (string.IsNullOrWhiteSpace(resetPasswordDto.Token) || string.IsNullOrWhiteSpace(resetPasswordDto.NewPassword) || string.IsNullOrWhiteSpace(resetPasswordDto.Email))
                return BadRequest("Token, email, and new password are required");

            try
            {
                var user = await _usuarioService.GetUserByEmail(resetPasswordDto.Email);

                if (user == null)
                {
                    _logger.LogWarning($"Password reset failed for email: {resetPasswordDto.Email}");
                    return NotFound("User not found");
                }

                bool isTokenValid = _authService.ValidatePasswordResetToken(user, resetPasswordDto.Token);

                if (!isTokenValid)
                {
                    return BadRequest("Invalid or expired token");
                }

                string hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
                user.Password = hashedNewPassword;
                await _usuarioService.UpdateUsuario(user);

                _logger.LogInformation($"Password successfully reset for user: {user.Username}");
                return Ok("Password has been reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while resetting the password.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
