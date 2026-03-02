
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Constants;
using TechExpress.Service.Contexts;
using TechExpress.Service.Utils;

namespace TechExpress.Service.Services
{
    public class AuthService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly JwtUtils _jwtUtils;
        private readonly UserContext _userContext;
        private readonly OtpUtils _otpUtils;
        private readonly SmtpEmailSender _emailSender;
        private readonly GoogleAuthUtils _googleAuthUtils;

        public AuthService(UnitOfWork unitOfWork, JwtUtils jwtUtils, UserContext userContext, OtpUtils otpUtils, SmtpEmailSender emailSender, GoogleAuthUtils googleAuthUtils)
        {
            _unitOfWork = unitOfWork;
            _jwtUtils = jwtUtils;
            _userContext = userContext;
            _otpUtils = otpUtils;
            _emailSender = emailSender;
            _googleAuthUtils = googleAuthUtils;
        }

        public async Task<(User user, string accessToken, string refreshToken)> LoginAsyncWithUser(string email, string password)
        {
            var user = await _unitOfWork.UserRepository.FindUserByEmailAsync(email) ?? throw new UnauthorizedException("Sai thông tin đăng nhập");

            if (!PasswordEncoder.VerifyPassword(password, user.PasswordHash))
            {
                throw new UnauthorizedException("Sai thông tin đăng nhập");
            }

            if (user.Status != UserStatus.Active)
            {
                throw new ForbiddenException("Tài khoản của bạn không hoạt động");
            }

            var accessToken = _jwtUtils.GenerateAccessToken(user);
            var refreshToken = _jwtUtils.GenerateRefreshToken(user);

            return (user, accessToken, refreshToken);
        }

        public async Task<(User user, string accessToken, string refreshToken)> RegisterAsync(
            string email, 
            string password, 
            string? firstName, 
            string? lastName, 
            string? phone)
        {
            if (await _unitOfWork.UserRepository.UserExistByEmailAsync(email))
            {
                throw new BadRequestException("Email đã tồn tại");
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (await _unitOfWork.UserRepository.UserExistByPhoneAsync(phone))
                {
                    throw new BadRequestException("Số điện thoại đã tồn tại");
                }
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = PasswordEncoder.HashPassword(password),
                Role = UserRole.Customer,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Status = UserStatus.Active
            };

            await _unitOfWork.UserRepository.AddUserAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            var accessToken = _jwtUtils.GenerateAccessToken(newUser);
            var refreshToken = _jwtUtils.GenerateRefreshToken(newUser);

            return (newUser, accessToken, refreshToken);
        }

        public async Task<(User user, string accessToken, string refreshToken)> RegisterStaffAsync(
            string email, 
            string password, 
            string? firstName, 
            string? lastName, 
            string? phone)
        {
            var existingUser = await _unitOfWork.UserRepository.FindUserByEmailAsync(email);
            if (await _unitOfWork.UserRepository.UserExistByEmailAsync(email))
            {
                throw new BadRequestException("Email đã tồn tại");
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (await _unitOfWork.UserRepository.UserExistByPhoneAsync(phone))
                {
                    throw new BadRequestException("Số điện thoại đã tồn tại");
                }
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = PasswordEncoder.HashPassword(password),
                Role = UserRole.Staff,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Status = UserStatus.Active
            };

            await _unitOfWork.UserRepository.AddUserAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            var accessToken = _jwtUtils.GenerateAccessToken(newUser);
            var refreshToken = _jwtUtils.GenerateRefreshToken(newUser);

            return (newUser, accessToken, refreshToken);
        }
    

        public async Task HandleForgotPasswordRequestOtp(string email)
        {
            var user = await _unitOfWork.UserRepository.FindUserByEmailAsync(email) ?? throw new UnauthorizedException("Không tìm thấy người dùng.");

            var key = RedisKeyConstant.ForgotPasswordOtpKey(user.Id);
            var otp = await _otpUtils.CreateAndStoreResetPasswordOtp(user.Id);

            var subject = "TechExpress - OTP reset password";
            var html = $@"
                <div style=""font-family: Arial, sans-serif;"">
                    <h3>Reset password</h3>
                    <p>Mã OTP của bạn là:</p>
                    <div style=""font-size: 28px; font-weight: 700; letter-spacing: 4px;"">{otp}</div>
                    <p>Mã có hiệu lực trong <b>15 phút</b>.</p>
                </div>";

            await _emailSender.SendAsync(user.Email.Trim(), subject, html);
        }

        public async Task HandleResetPassword(string email, string otp, string newPassword, string confirmNewPassword)
        {
            var user = await _unitOfWork.UserRepository.FindUserByEmailAsync(email) ?? throw new UnauthorizedException("Không tìm thấy người dùng.");

            if (newPassword != confirmNewPassword)
            {
                throw new BadRequestException("Mật khẩu mới và xác nhận mật khẩu không khớp.");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                throw new ForbiddenException("Mật khẩu không hợp lệ (tối thiểu 6 ký tự).");
            }

            await _otpUtils.VerifyResetPasswordOtp(user.Id, otp);

            user.PasswordHash = PasswordEncoder.HashPassword(newPassword);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<string> HandleRefreshNewToken(string refreshToken)
        {
            var userId = JwtUtils.GetUserIdFromToken(refreshToken);
            if (userId == null)
            {
                throw new NotFoundException("Token không hợp lệ");
            }

            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId.Value) ??
                       throw new NotFoundException("Không tìm thấy người dùng");
            return _jwtUtils.GenerateAccessToken(user);
        }

        public async Task<(User user, string accessToken, string refreshToken)> HandleGoogleCallbackAsync(string code, string redirectUri)
        {
            // Exchange authorization code for ID token
            var idToken = await _googleAuthUtils.ExchangeCodeForTokenAsync(code, redirectUri);
            if (string.IsNullOrEmpty(idToken))
            {
                throw new UnauthorizedException("Không thể lấy token từ Google");
            }

            // Get user info from ID token
            var googleUserInfo = await _googleAuthUtils.GetUserInfoFromIdTokenAsync(idToken);
            if (googleUserInfo == null)
            {
                throw new UnauthorizedException("Không thể xác minh thông tin người dùng từ Google");
            }

            // Check if user exists
            var user = await _unitOfWork.UserRepository.FindUserByEmailAsync(googleUserInfo.Email);

            if (user == null)
            {
                // Create new user if doesn't exist
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = googleUserInfo.Email,
                    PasswordHash = PasswordEncoder.HashPassword(Guid.NewGuid().ToString()), // Generate random password for OAuth users
                    Role = UserRole.Customer,
                    FirstName = googleUserInfo.GivenName,
                    LastName = googleUserInfo.FamilyName,
                    AvatarImage = googleUserInfo.Picture,
                    Status = UserStatus.Active
                };

                await _unitOfWork.UserRepository.AddUserAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                // Update user info if email exists
                if (!string.IsNullOrEmpty(googleUserInfo.Picture) && string.IsNullOrEmpty(user.AvatarImage))
                {
                    user.AvatarImage = googleUserInfo.Picture;
                }

                if (string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(googleUserInfo.GivenName))
                {
                    user.FirstName = googleUserInfo.GivenName;
                }

                if (string.IsNullOrEmpty(user.LastName) && !string.IsNullOrEmpty(googleUserInfo.FamilyName))
                {
                    user.LastName = googleUserInfo.FamilyName;
                }

                await _unitOfWork.SaveChangesAsync();
            }

            if (user.Status != UserStatus.Active)
            {
                throw new ForbiddenException("Tài khoản của bạn không hoạt động");
            }

            var accessToken = _jwtUtils.GenerateAccessToken(user);
            var refreshToken = _jwtUtils.GenerateRefreshToken(user);

            return (user, accessToken, refreshToken);
        }
    }
}
