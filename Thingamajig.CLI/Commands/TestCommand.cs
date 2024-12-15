using System.CommandLine;

namespace Thingamajig.CLI.Commands
{
    public class TestCommand : Command<TestCommandOptions, TestCommandOptionsHandler>
    {
        // Keep the hard dependency on System.CommandLine here
        public TestCommand() 
            : base("test", "this is a test command to demonstrate the command registration process.")
        {
            AddOption(new Option<string>("--test", "This is a test option"));
        }
    }

    public class TestCommandOptions : ICommandOptions
    {
        // Automatic binding with System.CommandLine.NamingConventionBinder
        public string Test { get; set; } = string.Empty;
    }

    public class  TestCommandOptionsHandler : ICommandOptionsHandler<TestCommandOptions>
    {
        private readonly IConsole _console;

        // Inject anything here, no more hard dependency on System.CommandLine
        public TestCommandOptionsHandler(IConsole console)
        {
            _console = console;
        }

        public Task<int> HandleAsync(TestCommandOptions options, CancellationToken cancellationToken)
        {
            _console.WriteLine("Running test command...");

            // Command logic goes here..

            return Task.FromResult(0);
        }
    }
}
