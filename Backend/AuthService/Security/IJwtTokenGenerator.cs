using AuthService.Entities;

namespace AuthService.Security;

public interface IJwtTokenGenerator
{
    JwtTokenResult Generate(User user, List<string> roles);
}
