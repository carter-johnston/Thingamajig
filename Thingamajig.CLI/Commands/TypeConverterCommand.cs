
using System.CommandLine;
using System.Text;
using Thingamajig.Core.Services;
using Thingamajig.Transpiler;

namespace Thingamajig.CLI.Commands
{
    public class TypeConverterCommand : Command<TypeConverterCommandOptions, TypeConverterCommandOptionsHandler>
    {
        public TypeConverterCommand()
            : base("type", "Sync CSharp files from one directory to another. Convert the cSharp files into Typescript")
        {
            AddOption(new Option<string>("--in", "input dir"));
            AddOption(new Option<string>("--out", "output dir"));
        }
    }

    public class TypeConverterCommandOptions : ICommandOptions
    {
        public required string In { get; set; }
        public required string Out { get; set; }
    }

    public class TypeConverterCommandOptionsHandler : ICommandOptionsHandler<TypeConverterCommandOptions>
    {
        private readonly IConsole _console;
        private readonly PeriodicCodeRunner _periodicCodeRunner;
        private readonly ICsharpTranspilerService _transpilerService;

        public TypeConverterCommandOptionsHandler(
            IConsole console, 
            PeriodicCodeRunner periodicCodeRunner, 
            ICsharpTranspilerService transpilerService)
        {
            _console = console;
            _periodicCodeRunner = periodicCodeRunner;
            _transpilerService = transpilerService;
        }

        public async Task<int> HandleAsync(TypeConverterCommandOptions options, CancellationToken cancellationToken)
        {
            _console.WriteLine("Running type sync command...");

            var inputPath = Environment.CurrentDirectory + "\\" + options.In;
            var outputPath = Environment.CurrentDirectory + "\\" + options.Out;

            await _periodicCodeRunner.RunPeriodicallyAsync((cancellationToken) =>
            {
                _console.WriteLine("Running type sync...");

                CreateOrUpdateFiles(inputPath, outputPath);
                DeleteFilesIfNotInInput(inputPath, outputPath);
            },
            cancellationToken);

            return await Task.FromResult(0);
        }

        private void CreateOrUpdateFiles(string inputPath, string outputPath)
        {
            foreach (var inputFilePath in Directory.GetFiles(inputPath, "*.cs"))
            {
                var inputFileName = Path.GetFileNameWithoutExtension(inputFilePath);
                var outputFilePath = Path.Combine(outputPath, inputFileName + ".ts");

                var isFileToBeCreated = !File.Exists(outputFilePath);
                var isFileToBeUpdated = File.GetLastWriteTime(inputFilePath) > File.GetLastWriteTime(outputFilePath);

                if (Path.GetExtension(inputFilePath) != ".cs") 
                    continue;

                if (isFileToBeCreated || isFileToBeUpdated)
                {
                    using var fs = File.Create(outputFilePath);

                    try
                    {
                        using var stream = new StreamReader(inputFilePath);
                        var transpiledCode = _transpilerService.ConvertCSharpToTypescript(stream.ReadToEnd());

                        fs.Write(new UTF8Encoding(true).GetBytes(transpiledCode));
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLine($"Error transpiling {inputFilePath}: {ex.Message}");
                    }

                    if (isFileToBeCreated)
                        _console.WriteLine($"{inputFileName} created.");
                    if (isFileToBeUpdated)
                        _console.WriteLine($"{inputFileName} updated.");
                }
            }
        }

        private void DeleteFilesIfNotInInput(string inputPath, string outputPath)
        {
            Directory.GetFiles(outputPath)
                .Where(file => Path.GetFileName(file).EndsWith(".cs"))
                .Where(file => !File.Exists(Path.Combine(
                    inputPath, 
                    Path.GetFileNameWithoutExtension(file) + ".cs")))
                .ToList()
                .ForEach(file =>
                {
                    File.Delete(file);
                    _console.WriteLine($"{Path.GetFileName(file)} deleted.");
                });
        }
    }
}
