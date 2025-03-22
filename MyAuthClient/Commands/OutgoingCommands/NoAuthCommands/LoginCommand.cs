using System.Text;
using System.Text.Json;

namespace MyAuthClient.Commands.OutgoingCommands.NoAuthCommands
{
    public sealed class LoginCommand : ICommand
    {
		public CommandType Type => CommandType.OutgoingNoAuth;

		public string CheckUrl => "auth/login";

        public string CommandName => "/login";

        public string CommandDisplayDescription => "Attempts to log in using the provided credentials.";

        public IEnumerable<string> ParameterDisplayInput => ["<username>", "<password>"];

        public async Task ExecuteCommand(params object[] args)
        {
            using HttpClient client = new();
            var username = args[0];
            var password = args[1];
            var sessionToken = CommandHandler.SessionToken;
            var rawRequestData = new { username, password, CommandHandler.DeviceId, sessionToken };
            string formattedRequestData = JsonSerializer.Serialize(rawRequestData);
            var content = new StringContent(formattedRequestData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Program.BaseURL + CheckUrl, content);
            var responseStr = await response.Content.ReadAsStringAsync();
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(responseStr);
            if (dict["message2"] != string.Empty)
                CommandHandler.SessionToken = dict["message2"];

            Console.WriteLine($"Response: {dict.First().Value}");
            return;
        }
    }
}
