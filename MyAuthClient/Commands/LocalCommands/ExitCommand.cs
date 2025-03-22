namespace MyAuthClient.Commands.LocalCommands
{
	public sealed class ExitCommand : ICommand
	{
		public CommandType Type => CommandType.Local;

		public string CommandName => "/exit";

		public string CommandDisplayDescription => "Exits the program";

		public IEnumerable<string> ParameterDisplayInput => null;

		public string CheckUrl => null;

		public async Task ExecuteCommand(params object[] args)
		{
			Program.ShouldExit = true;
			Console.WriteLine("Exiting...");
			await Task.Delay(250);
			return;
		}
	}
}
