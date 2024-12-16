using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Thingamajig.CLI;
using Thingamajig.CLI.Commands;
using Thingamajig.Core.Services;
using Thingamajig.Transpiler;

var rootCommand = new RootCommand()
{
    new TestCommand(),
    new FileSyncCommand(),
    new TypeConverterCommand()
};

var builder = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseDependencyInjection(services =>
    {
        services.AddSingleton<PeriodicCodeRunner>();
        services.AddSingleton<ICsharpTranspilerService, CsharpTranspilerService>();
    });

return builder.Build().Invoke(args);
