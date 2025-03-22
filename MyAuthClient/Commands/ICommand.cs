using System.Text.Json;
using System.Text;

namespace MyAuthClient.Commands
{
	/// <summary>
	/// A basic client command.
	/// </summary>
	public interface ICommand
	{
		/// <summary>
		/// The type this command is.
		/// </summary>
		CommandType Type { get; }

		/// <summary>
		/// The url any requests this command makes should be directed to.
		/// </summary>
		string CheckUrl { get; }

		/// <summary>
		/// The name of the command as used by the user.
		/// </summary>
		string CommandName { get; }

		/// <summary>
		/// The description of the command, in display format.
		/// </summary>
		string CommandDisplayDescription { get; }

		/// <summary>
		/// Any parameters the command needs, in display format.
		/// </summary>
		IEnumerable<string> ParameterDisplayInput { get; }

		/// <summary>
		/// Executes the command, either by performing local actions, or sending a request to the web server.
		/// </summary>
		/// <param name="args">All parameters required for the command. Is guaranteed to be the same length as <see cref="ParameterDisplayInput"/>.</param>
		Task ExecuteCommand(params object[] args);

		/// <summary>
		/// Helper method for performing a basic request that follows the "BasicRequest" structure found on the server ({ DeviceId, SessionToken }).
		/// </summary>
		/// <param name="url">The url to direct the request to.</param>
		/// <returns>A dictionary containing the response from the server.</returns>
		internal async static Task<Dictionary<string, string>> DefaultRequestCommand(string url)
		{
			using HttpClient client = new();
			var deviceId = CommandHandler.DeviceId;
			var sessionToken = CommandHandler.SessionToken;
			var rawRequestData = new { deviceId, sessionToken };
			string formattedRequestData = JsonSerializer.Serialize(rawRequestData);
			var content = new StringContent(formattedRequestData, Encoding.UTF8, "application/json");

			var response = await client.PostAsync(Program.BaseURL + url, content);
			var responseStr = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<Dictionary<string, string>>(responseStr);
		}
	}
}
