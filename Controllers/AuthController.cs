using System.Security.Claims;
using FleetPulse_BackEndDevelopment.Data.DTO;
using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse_BackEndDevelopment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMailService _mailService;
        private readonly IJwtService jwtService;
        private readonly IEmailService _emailService;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IMailService mailService,
            IJwtService jwtService,
            IEmailService emailService,
            IVerificationCodeService verificationCodeService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _mailService = mailService;
            _emailService = emailService;
            _verificationCodeService = verificationCodeService;
            this.jwtService = jwtService;
            _logger = logger;
        }
        
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetUserCount()
        {
            var count = await _authService.GetUserCountAsync();
            return Ok(count);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginDTO userModel)
        {
            var response = new ApiResponse
            {
                Status = true
            };

            try
            {
                if (!ModelState.IsValid)
                {
                    response.Status = false;
                    response.Error = "Invalid Data";
                    return BadRequest(response);
                }

                var user = _authService.IsAuthenticated(userModel.Username, userModel.Password);

                if (user != null)
                {
                    string[] validJobTitles;
                    string errorMessage;

                    if (user.JobTitle == "Admin" || user.JobTitle == "Staff")
                    {
                        validJobTitles = new[] { "Admin", "Staff" };
                        errorMessage = "Unauthorized: Only Admin or Staff can login";
                    }
                    else if (user.JobTitle == "Driver" || user.JobTitle == "Helper")
                    {
                        validJobTitles = new[] { "Driver", "Helper" };
                        errorMessage = "Unauthorized: Only Driver or Helper can login";
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Unauthorized: Your job title does not have access to this endpoint";
                        return new JsonResult(response);
                    }

                    if (validJobTitles.Contains(user.JobTitle))
                    {
                        var accessToken = jwtService.GenerateAccessToken(user.UserName, new []{user.JobTitle});
                        var refreshToken = jwtService.GenerateRefreshToken();
                        
                        var refreshTokenModel = new RefreshToken
                        {
                            Token = refreshToken,
                            Expires = DateTime.UtcNow.AddMinutes(3),
                            IsRevoked = false
                        };
                        
                        await jwtService.AddRefreshTokenAsync(refreshTokenModel);

                        response.Data = new
                        {
                            AccessToken = accessToken,
                            RefreshToken = refreshToken,
                            JobTitle = user.JobTitle,
                            UserId = user.UserId // Make sure this is included
                        };
                        return new JsonResult(response);
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = errorMessage;
                        return new JsonResult(response);
                    }
                }

                response.Status = false;
                response.Message = "Invalid username or password";
                return new JsonResult(response);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "An error occurred while processing the login request: {Message}",
                    error.Message);
                response.Status = false;
                response.Error = "An internal error occurred";
                return StatusCode(500, response);
            }
        }
        
        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDTO model)
        {
            var refreshTokenData = await jwtService.GetRefreshTokenData(model.RefreshToken);
            if (refreshTokenData == null) return BadRequest("Invalid refresh token");
            
            var isValidRefreshToken = refreshTokenData.IsRevoked == false && refreshTokenData.Expires > DateTime.UtcNow;
            if (!isValidRefreshToken)
            {
                await jwtService.DeleteRefreshTokenAsync(model.RefreshToken);
                return BadRequest("Refresh token expired");
            }
            
            var principal = jwtService.GetPrincipalFromExpiredToken(model.AccessToken);
            var userName = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

            if (userName == null) return BadRequest("Invalid access token");

            var newAccessToken = jwtService.GenerateAccessToken(userName, roles);

            return Ok(new { AccessToken = newAccessToken, RefreshToken = model.RefreshToken });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            var response = new ApiResponse
            {
                Status = true
            };

            try
            {
                if (ModelState.IsValid)
                {
                    var emailExists = _authService.DoesEmailExists(model.Email);

                    if (!emailExists)
                    {
                        response.Status = false;
                        response.Message = "Email not found";
                        return new JsonResult(response);
                    }

                    var verificationCode = await _verificationCodeService.GenerateVerificationCode(model.Email);
                    var mailRequest = new MailRequest
                    {
                        Subject = "Password Reset Verification",
                        Body = verificationCode.Code,
                        ToEmail = model.Email
                    };

                    await _mailService.SendEmailAsync(mailRequest);

                    response.Message = "Verification code sent successfully";
                    return new JsonResult(response);
                }
                else
                {
                    response.Message = "Invalid model state";
                    return BadRequest(response);
                }
            }
            catch (Exception error)
            {
                _logger.LogError(error, "An error occurred while processing the forgot password request: {Message}",
                    error.Message);

                response.Status = false;
                response.Message = "An error occurred while processing your request";
                return StatusCode(500, response);
            }
        }

        [AllowAnonymous]
        [HttpPost("validate-verification-code")]
        public async Task<IActionResult> ValidateVerificationCode([FromBody] ValidateVerificationCodeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = new ApiResponse
            {
                Status = true
            };

            bool isValid = await _verificationCodeService.ValidateVerificationCode(request.Email, request.Code);

            if (isValid)
            {
                return new JsonResult(response);
            }
            else
            {
                response.Status = false;
                response.Error = "Invalid Data";
                return BadRequest(response);
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            var response = new ApiResponse
            {
                Status = true
            };

            try
            {
                if (ModelState.IsValid)
                {
                    var emailExists = _authService.DoesEmailExists(model.Email);

                    if (!emailExists)
                    {
                        response.Status = false;
                        response.Message = "Email not found";
                        return new JsonResult(response);
                    }

                    bool passwordReset = _authService.ResetPassword(model.Email, model.NewPassword);

                    if (passwordReset)
                    {
                        response.Message = "Password reset successfully";
                        return new JsonResult(response);
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Failed to reset password";
                        return new JsonResult(response);
                    }
                }
                else
                {
                    response.Message = "Invalid model state";
                    return BadRequest(response);
                }
            }
            catch (Exception error)
            {
                _logger.LogError(error, "An error occurred while processing the reset password request: {Message}",
                    error.Message);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("reset-password-driver")]
        public async Task<IActionResult> ResetDriverPassword([FromBody] ResetDriverPasswordRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.EmailAddress) ||
                string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest("Invalid request.");
            }

            try
            {
                // Check if the email address exists
                var emailExists = _authService.DoesEmailExists(request.EmailAddress);
                if (!emailExists)
                {
                    return BadRequest(new ApiResponse { Status = false, Message = "Email not found" });
                }

                // Reset the password
                var result = await _authService.ResetDriverPasswordAsync(request.EmailAddress, request.NewPassword);
                if (result)
                {
                    // Send email notification
                    var mailRequest = new MailRequest
                    {
                        ToEmail = request.EmailAddress,
                        Subject = "Password Reset Notification",
                        Body = request.NewPassword
                    };

                    await _emailService.SendEmailAsync(mailRequest);

                    // Format the timestamp as "Saturday, July 6, 2024 at 5:13:35 PM"
                    string formattedDate = DateTime.Now.ToString("dddd, MMMM d, yyyy 'at' h:mm:ss tt");

                    // Return a response with formatted timestamp
                    var response = new ApiResponse
                    {
                        Status = true,
                        Message = "Password reset successful.",
                    };

                    return Ok(response);
                }
                else
                {
                    return BadRequest(new ApiResponse { Status = false, Message = "Failed to reset password." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while resetting driver password: {Message}", ex.Message);
                return StatusCode(500,
                    new ApiResponse { Status = false, Error = "An error occurred while resetting password." });
            }
        }

        [AllowAnonymous]
        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordDTO model)
        {
            var response = new ApiResponse
            {
                Status = true
            };

            try
            {
                if (ModelState.IsValid)
                {
                    var user = _authService.GetByUsername(model.Username);

                    if (user == null)
                    {
                        response.Status = false;
                        response.Error = "Failed to change password";
                        return new JsonResult(response);
                    }

                    var isOldPasswordValid = _authService.IsAuthenticated(user.UserName, model.OldPassword);

                    if (isOldPasswordValid == null)
                    {
                        response.Status = false;
                        response.Error = "Old password is incorrect";
                        return new JsonResult(response);
                    }

                    var passwordReset = _authService.ResetPassword(user.EmailAddress, model.NewPassword);

                    if (passwordReset)
                    {
                        response.Message = "Password changed successfully";
                        return new JsonResult(response);
                    }

                    response.Status = false;
                    response.Error = "Failed to change password";
                    return new JsonResult(response);
                }

                response.Error = "Invalid model state";
                return BadRequest(response);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "An error occurred while processing the change password request: {Message}",
                    error.Message);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] StaffDTO staff)
        {
            try
            {
                var oldUser = _authService.GetByUsername(staff.UserName);

                if (oldUser is null)
                {
                    return NotFound("User not found");
                }

                oldUser.FirstName = staff.FirstName;
                oldUser.LastName = staff.LastName;
                oldUser.NIC = staff.NIC;
                oldUser.DateOfBirth = staff.DateOfBirth;
                oldUser.PhoneNo = staff.PhoneNo;
                oldUser.EmailAddress = staff.EmailAddress;
                oldUser.ProfilePicture = staff.ProfilePicture;
                
                var result = await _authService.UpdateUserAsync(oldUser);

                if (result)
                {
                    return Ok("User updated successfully.");
                }
                else
                {
                    return StatusCode(500, "Failed to update user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while updating the user: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpGet("userProfile")]
        public async Task<ActionResult<StaffDTO>> GetUserByUsernameAsync(string username)
        {
            var user = await _authService.GetUserByUsernameAsync(username);

            if (user == null)
                return NotFound();

            var profilePictureBase64 = user.ProfilePicture != null ? Convert.ToBase64String(user.ProfilePicture) : null;

            var staffDTO = new StaffDTO
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                EmailAddress = user.EmailAddress,
                PhoneNo = user.PhoneNo,
                NIC = user.NIC,
                ProfilePicture = user.ProfilePicture
            };

            return Ok(staffDTO);
        }

        [AllowAnonymous]
        [HttpPut("UpdateDriverProfilePicture")]
        public async Task<IActionResult> UpdateDriverProfilePicture([FromBody] ProfilePictureDTO profilePictureDTO)
        {
            try
            {
                var result = await _authService.UpdateUserProfilePictureAsync(profilePictureDTO.UserName,
                    profilePictureDTO.ProfilePicture);

                if (result)
                {
                    return Ok("Profile picture updated successfully.");
                }
                else
                {
                    return StatusCode(500, "Failed to update profile picture.");
                }
            }
            catch (Exception ex)
            {
                // Optionally handle or log the exception
                return StatusCode(500, $"An error occurred while updating the profile picture: {ex.Message}");
            }
        }
        
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            Response.Cookies.Delete("localStorageKey");

            return RedirectToAction("Login");
        }
        
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("drivers/nics")]
        public async Task<ActionResult<List<string>>> GetAllDriverNICs()
        {
            try
            {
                var driverNICs = await _authService.GetAllDriverNICsAsync();
                return Ok(driverNICs);
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, "Error fetching driver NICs: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while fetching driver NICs.");
            }
        }
    }
}