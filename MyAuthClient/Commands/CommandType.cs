namespace MyAuthClient.Commands
{
	public enum CommandType : byte
	{
		Local,
		OutgoingNoAuth,
		OutgoingAuthRequired,
		OutgoingAdminAuthRequired
	}
}
