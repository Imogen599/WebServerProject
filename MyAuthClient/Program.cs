using MyAuthClient.Commands;

namespace MyAuthClient
{
    public class Program
	{
		public const string BaseURL = $"https://localhost:{BasePort}/api/";

		public const string BasePort = "5266";

		internal static bool ShouldExit;

		static async Task Main()
		{
			using HttpClient client = new();

			CommandHandler.Initialize();

			while (true)
			{
				string userInput;
				do
				{
					Console.WriteLine("Please enter a command. Use /help to see a list of commands, or /exit to exit.");
					userInput = Console.ReadLine();
				}
				while (userInput == string.Empty);
				await CommandHandler.HandleCommandAsync(userInput);
				if (ShouldExit)
					break;
			}

			return;
		}
	}
}
