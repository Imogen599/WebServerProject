using System.Text.Json;
using System.Text;

namespace MyAuthClient.Commands.OutgoingCommands.NoAuthCommands
{
    public sealed class RegisterCommand : ICommand
    {
		public CommandType Type => CommandType.OutgoingNoAuth;

		public string CheckUrl => "auth/register";

        public string CommandName => "/register";

        public string CommandDisplayDescription => "Registers a new user. The username must be unique.";

        public IEnumerable<string> ParameterDisplayInput => ["<username>", "<password>"];

        public async Task ExecuteCommand(params object[] args)
        {
            using HttpClient client = new();
            var username = args[0];
            var password = args[1];
            var rawRequestData = new { username, password, CommandHandler.DeviceId };
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
