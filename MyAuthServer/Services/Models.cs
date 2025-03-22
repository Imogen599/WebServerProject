namespace MyAuthServer.Services
{
	public record struct UserPasswordChangeModel(bool Successful, string ErrorMessage);

	public record struct UserIDFetchModel(bool Successful, int UserId);

	public record struct UserLoginModel(bool Successful, string GeneratedSessionToken, int UserId);
}
