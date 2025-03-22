namespace MyAuthServer.Services
{
	// Contains several data models for readability with some of the service return values. Should be merged with the request structures really.
	public record struct UserPasswordChangeModel(bool Successful, string ErrorMessage);

	public record struct UserIDFetchModel(bool Successful, int UserId);

	public record struct UserLoginModel(bool Successful, string GeneratedSessionToken, int UserId);
}
