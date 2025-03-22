namespace MyAuthClient.Commands.LocalCommands
{
    public sealed class HelpCommand : ICommand
	{
		public CommandType Type => CommandType.Local;

		public string CommandName => "/help";

		public string CommandDisplayDescription => "Shows a list of available commands";

		public IEnumerable<string> ParameterDisplayInput => null;

		public string CheckUrl => null;

		private Dictionary<CommandType, List<CommandHandler.CommandDisplayInformation>> cachedCommandDisplayInfo = null;

		private static readonly CommandType[] commandTypes = [CommandType.Local, CommandType.OutgoingNoAuth, CommandType.OutgoingAuthRequired, CommandType.OutgoingAdminAuthRequired];

		public Task ExecuteCommand(params object[] args)
		{
			static void displayCommandList(CommandType type, List<CommandHandler.CommandDisplayInformation> list)
			{
				Console.WriteLine();
				Console.WriteLine($"{CommandHandler.ToDisplayString(type)}:");
				foreach (var value in list)
					Console.WriteLine($"\"{value.Name}\": {value.Description}");
			}

			Console.WriteLine("List of commands: (Usage: Description)");

			if (cachedCommandDisplayInfo == null)
			{
				cachedCommandDisplayInfo = CommandHandler.RequestCommandsInformation();
				for (int i = 0; i < commandTypes.Length; i++)
				{
					if (!cachedCommandDisplayInfo.TryGetValue(commandTypes[i], out var commandList))
						continue;

					cachedCommandDisplayInfo[commandTypes[i]] = [.. commandList.OrderBy(x => x.Name)];

					displayCommandList(commandTypes[i], cachedCommandDisplayInfo[commandTypes[i]]);
				}
			}
			else
			{
				for (int i = 0; i < commandTypes.Length; i++)
				{
					if (!cachedCommandDisplayInfo.TryGetValue(commandTypes[i], out var commandList))
						continue;

					displayCommandList(commandTypes[i], commandList);
				}
			}

			return Task.CompletedTask;
		}
	}
}
