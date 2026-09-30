using BIDADYUManagement.Domain.Entities.Auth;

namespace BIDADYUManagement.Application.Auth.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
}

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
