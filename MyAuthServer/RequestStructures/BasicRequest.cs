namespace MyAuthServer.RequestStructures
{
	/// <summary>
	/// A request structure for basic requests that require authentication.
	/// </summary>
	public class BasicRequest
	{
		public string DeviceId { get; set; }
		public string SessionToken { get; set; }
	}
}
