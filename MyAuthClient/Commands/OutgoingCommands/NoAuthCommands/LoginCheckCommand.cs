namespace MyAuthClient.Commands.OutgoingCommands.NoAuthCommands
{
    public sealed class LoginCheckCommand : ICommand
	{
		public CommandType Type => CommandType.OutgoingNoAuth;

		public string CheckUrl => "auth/checkLogin";

		public string CommandName => "/checkLogin";

		public string CommandDisplayDescription => "Checks if the current device is logged in.";

		public IEnumerable<string> ParameterDisplayInput => null;

		public async Task ExecuteCommand(params object[] args)
		{
			var dict = await ICommand.DefaultRequestCommand(CheckUrl);
			Console.WriteLine($"Response: {dict.First().Value}");
			return;
		}
	}
}
