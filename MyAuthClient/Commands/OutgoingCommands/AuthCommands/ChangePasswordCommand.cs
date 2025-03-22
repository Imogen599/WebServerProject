using System.Text;
using System.Text.Json;

namespace MyAuthClient.Commands.OutgoingCommands.AuthCommands
{
	public sealed class ChangePasswordCommand : ICommand
	{
		public CommandType Type => CommandType.OutgoingAuthRequired;

		public string CheckUrl => "auth/changePassword";

		public string CommandName => "/changePassword";

		public string CommandDisplayDescription => "Changes the password for the user you are currently logged in as.";

		public IEnumerable<string> ParameterDisplayInput => ["<newPassword>"];

		public async Task ExecuteCommand(params object[] args)
		{
			using HttpClient client = new();
			var password = args[0];
			var sessionToken = CommandHandler.SessionToken;
			var rawRequestData = new { password, CommandHandler.DeviceId, sessionToken };
			string formattedRequestData = JsonSerializer.Serialize(rawRequestData);
			var content = new StringContent(formattedRequestData, Encoding.UTF8, "application/json");

			var response = await client.PostAsync(Program.BaseURL + CheckUrl, content);
			var responseStr = await response.Content.ReadAsStringAsync();
			var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(responseStr);
			Console.WriteLine($"Response: {dict.First().Value}");
			return;
		}
	}
}
