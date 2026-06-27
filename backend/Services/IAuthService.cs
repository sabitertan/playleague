using PlayLeague.Api.Responses;
using PlayLeague.Api.Requests;

namespace PlayLeague.Api.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
