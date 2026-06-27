using PlayLeague.Api.Models;

namespace PlayLeague.Api.Services;

public interface ITokenService
{
    string Generate(User user);
}
