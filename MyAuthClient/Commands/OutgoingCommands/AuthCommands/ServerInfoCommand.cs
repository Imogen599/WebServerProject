namespace MyAuthClient.Commands.OutgoingCommands.AuthCommands
{
	public sealed class ServerInfoCommand : ICommand
	{
		public CommandType Type => CommandType.OutgoingAuthRequired;

		public string CheckUrl => "serverInfo/getServerInfo";

		public string CommandName => "/serverInfo";

		public string CommandDisplayDescription => "Displays information about the server responding to the request.";

		public IEnumerable<string> ParameterDisplayInput => null;

		public async Task ExecuteCommand(params object[] args)
		{
			var dict = await ICommand.DefaultRequestCommand(CheckUrl);
			Console.WriteLine($"Response:\n{dict.First().Value}");
			return;
		}
	}
}
