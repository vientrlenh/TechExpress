using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Service;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;

        public AuthController(ServiceProviders serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        /// <summary>
        /// Chuc nang dang nhap
        /// </summary>

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (user, accessToken, refreshToken) = await _serviceProvider.AuthService.LoginAsyncWithUser(request.Email.Trim(), request.Password);
            
            var response = ResponseMapper.MapToAuthResponse(accessToken, refreshToken, user);

            return Ok(ApiResponse<AuthResponse>.OkResponse(response));
        }


        /// <summary>
        /// Chuc nang dang ky danh cho customer
        ///</summary>

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
        {
            var (user, accessToken, refreshToken) = await _serviceProvider.AuthService.RegisterAsync(
                request.Email.Trim(),
                request.Password,
                request.FirstName?.Trim(),
                request.LastName?.Trim(),
                request.Phone?.Trim()
            );

            var response = ResponseMapper.MapToAuthResponse(accessToken, refreshToken, user);

            return CreatedAtAction(nameof(Register), ApiResponse<AuthResponse>.CreatedResponse(response));
        }

        /// <summary>
        /// Chuc nang dang ky danh cho staff
        /// </summary>
        [HttpPost("register-staff")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RegisterStaff([FromBody] RegisterStaffRequest request)
        {
            var (user, accessToken, refreshToken) = await _serviceProvider.AuthService.RegisterStaffAsync(
                request.Email.Trim(),
                request.Password,
                request.FirstName?.Trim(),
                request.LastName?.Trim(),
                request.Phone?.Trim()
            );

            var response = ResponseMapper.MapToAuthResponse(accessToken, refreshToken, user);

            return CreatedAtAction(nameof(RegisterStaff), ApiResponse<AuthResponse>.CreatedResponse(response));
        }

        [HttpPost("forgot-password/request-otp")]
        public async Task<IActionResult> RequestForgotPasswordOtp([FromBody] ResetPasswordOtpRequest request)
        {
            await _serviceProvider.AuthService.HandleForgotPasswordRequestOtp(request.Email.Trim());
            return Ok(ApiResponse<string>.OkResponse("Mã OTP đã được gửi đến email của bạn."));
        }

        

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _serviceProvider.AuthService.HandleResetPassword(request.Email.Trim(), request.Otp.Trim(), request.NewPassword, request.ConfirmNewPassword);

            return Ok(ApiResponse<string>.OkResponse("Đặt lại mật khẩu thành công."));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshNewToken([FromBody] RefreshNewTokenRequest request)
        {
            var response = await _serviceProvider.AuthService.HandleRefreshNewToken(request.RefreshToken);
            return Ok(ApiResponse<string>.OkResponse(response));
        }

        /// <summary>
        /// Lấy URL đăng nhập Google
        /// </summary>
        [HttpGet("google-login")]
        public IActionResult GetGoogleLoginUrl([FromQuery] string? redirectUri = null)
        {
            var callbackUrl = redirectUri ?? "https://localhost:7194/api/auth/google-callback";
            
            var googleAuthUtils = new TechExpress.Service.Utils.GoogleAuthUtils(HttpContext.RequestServices.GetRequiredService<IConfiguration>());
            var loginUrl = googleAuthUtils.GetGoogleAuthUrl(callbackUrl);

            return Ok(ApiResponse<string>.OkResponse(loginUrl));
        }

        /// <summary>
        /// Callback từ Google OAuth
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(
            [FromQuery] string? code,
            [FromQuery] string? state,
            [FromQuery] string? error,
            [FromQuery] string? redirectUri = null)
        {
            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = $"Google login error: {error}"
                });
            }

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Authorization code not found"
                });
            }

            try
            {
                var callbackUrl = redirectUri ?? "https://localhost:7194/api/auth/google-callback";
                var (user, accessToken, refreshToken) = await _serviceProvider.AuthService.HandleGoogleCallbackAsync(code, callbackUrl);

                var response = ResponseMapper.MapToAuthResponse(accessToken, refreshToken, user);

                return Ok(ApiResponse<AuthResponse>.OkResponse(response));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                });
            }
        }
    }
}
