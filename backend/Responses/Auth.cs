namespace PlayLeague.Api.Responses;

public record AuthResponse(string Token, UserResponse User);
public record UserResponse(Guid Id, string Email, string? Name);
