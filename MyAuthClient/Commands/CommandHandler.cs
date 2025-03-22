using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MyAuthClient.Commands
{
    /// <summary>
    /// Manages running and organising the commands.
    /// </summary>
    public static class CommandHandler
    {
        /// <summary>
        /// Represents a command in display format form.
        /// </summary>
        /// <param name="Name">The name of the command, plus any parameters.</param>
        /// <param name="Description">The description of the command.</param>
        /// <param name="Type">The command type.</param>
        public record struct CommandDisplayInformation(string Name, string Description, CommandType Type);

		private static ConcurrentDictionary<string, ICommand> commands;

        private const string DeviceIdFile = "DeviceId";

        /// <summary>
        /// The unique ID for this device. Used in combination with the session token to keep this client logged in while the session is valid.
        /// </summary>
        internal static string DeviceId
        {
            get;
            private set;
        }

		/// <summary>
		/// The session token for the client. Used in combination with the device ID to keep this client logged in while the session is valid.
		/// </summary>
		internal static string SessionToken = "1";

        internal static void Initialize()
        {
			static List<TContent> loadContentFromAssembly<TContent>(Assembly assembly) where TContent : class
			{
				var loadableTypes = assembly.GetTypes()
					.Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
					.Where(t => t.IsAssignableTo(typeof(TContent)))
					.Where(t =>
					{
						// Has default constructor check.
						bool derivedHasConstructor = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null;
						bool baseHasHasConstructor = t.BaseType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null;
						return derivedHasConstructor || baseHasHasConstructor;
					})
					.OrderBy(type => type.FullName, StringComparer.InvariantCulture);

				List<TContent> result = [];

				foreach (var type in loadableTypes)
					result.Add(RuntimeHelpers.GetUninitializedObject(type) as TContent);

				return result;
			}

			commands = [];
            var commandTypesAsICommand = loadContentFromAssembly<ICommand>(typeof(Program).Assembly);

            commandTypesAsICommand.ForEach(command => commands[command.CommandName] = command);

            if (!File.Exists(DeviceIdFile))
            {
                File.WriteAllText(DeviceIdFile, Guid.NewGuid().ToString());
            }
            else
            {
                DeviceId = File.ReadAllText(DeviceIdFile);
            }
        }

        /// <summary>
        /// Parses the user input, finds the correct command (if any), and executes it.
        /// </summary>
        /// <param name="userInput">The input string provided by the user.</param>
        public static async Task HandleCommandAsync(string userInput)
        {
            // Collect name and params.
            string[] commandNameAndArgs = userInput.Split(' ');

            if (!commandNameAndArgs[0][0].Equals('/'))
                commandNameAndArgs[0] = commandNameAndArgs[0].Insert(0, "/");

            var spacer = "-------------------------------------------------------------------------------";
			Console.WriteLine(spacer);

			if (!commands.TryGetValue(commandNameAndArgs[0], out ICommand command))
            {
                Console.WriteLine("Command not found");
				Console.WriteLine(spacer);
				return;
            }

            if (command.ParameterDisplayInput != null)
            {
                int expectedParamCount = command.ParameterDisplayInput.Count();

                // -1 here due to the name being included.
                if (commandNameAndArgs.Length - 1 != expectedParamCount)
                {
                    Console.WriteLine("The inputted argument count is invalid");
					Console.WriteLine(spacer);
					return;
                }
            }

            try
            {
                // Run the command.
				await command.ExecuteCommand(commandNameAndArgs[1..]);
			}
			catch (Exception ex)
            {
                Console.WriteLine("Exception caught:\n" + ex.ToString());
            }
			Console.WriteLine(spacer);
			return;
        }

        /// <summary>
        /// Parses the commands and constructs lists of <see cref="CommandDisplayInformation"/> organised by the command types.<br/>
        /// <b>As commands are functionally immutable, this should be called once and the result cached.</b>
        /// </summary>
        public static Dictionary<CommandType, List<CommandDisplayInformation>> RequestCommandsInformation()
        {
            Dictionary<CommandType, List<CommandDisplayInformation>> dict = [];

            foreach (var command in commands)
            {
                string paramsFormatted = string.Empty;
                if (command.Value.ParameterDisplayInput != null)
                {
                    foreach (var param in command.Value.ParameterDisplayInput)
                        paramsFormatted += " " + param;
                }
                
                if (!dict.TryGetValue(command.Value.Type, out var _))
                    dict[command.Value.Type] = [];

                dict[command.Value.Type].Add(new(command.Value.CommandName + paramsFormatted, command.Value.CommandDisplayDescription, command.Value.Type));
            }

            return dict;
        }

        /// <summary>
        /// Converts a <see cref="CommandType"/> to a display friendly string.
        /// </summary>
        public static string ToDisplayString(CommandType commandType)
        {
            return commandType switch
			{
				CommandType.Local => "Local, No Authentication Required",
				CommandType.OutgoingNoAuth => "Outgoing, No Authentication Required",
				CommandType.OutgoingAuthRequired => "Outgoing, Authentication Required",
				CommandType.OutgoingAdminAuthRequired => "Outgoing, Administrator Authentication Required",
				_ => "Unknown"
			};
        }
    }
}
