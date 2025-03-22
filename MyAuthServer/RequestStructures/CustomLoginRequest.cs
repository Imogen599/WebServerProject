namespace MyAuthServer.RequestStructures
{
	/// <summary>
	/// A request structure for login requests.
	/// </summary>
	public class CustomLoginRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string DeviceId { get; set; }
		public string SessionToken { get; set; }
	}
}
