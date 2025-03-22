namespace MyAuthServer.RequestStructures
{
	/// <summary>
	/// A request structure for a change password request.
	/// </summary>
	public class ChangePasswordRequest
	{
		public string Password { get; set; }
		public string DeviceId { get; set; }
		public string SessionToken { get; set; }
	}
}
