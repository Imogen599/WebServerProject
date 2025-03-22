namespace MyAuthClient.Commands.OutgoingCommands.AuthCommands
{
    public sealed class LogoutCommand : ICommand
	{
		public CommandType Type => CommandType.OutgoingAuthRequired;

		public string CheckUrl => "auth/logout";

		public string CommandName => "/logout";

		public string CommandDisplayDescription => "Logs the current user out. Only works if already logged in.";

		public IEnumerable<string> ParameterDisplayInput => null;

		public async Task ExecuteCommand(params object[] args)
		{
			var dict = await ICommand.DefaultRequestCommand(CheckUrl);
			Console.WriteLine($"Response: {dict.First().Value}");
			return;
		}
	}
}
