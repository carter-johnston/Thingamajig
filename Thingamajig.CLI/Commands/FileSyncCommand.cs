
using System.CommandLine;
using Thingamajig.Core.Services;

namespace Thingamajig.CLI.Commands
{
    public class FileSyncCommand : Command<FileSyncCommandOptions, FileSyncCommandOptionsHandler>
    {
        public FileSyncCommand()
            : base("fsync", "Sync files from one directory to another")
        {
            AddOption(new Option<string>("--in", "input dir"));
            AddOption(new Option<string>("--out", "output dir"));
        }
    }

    public class  FileSyncCommandOptions : ICommandOptions
    {
        public required string In { get; set; }
        public required string Out { get; set; }
    }

    public class FileSyncCommandOptionsHandler: ICommandOptionsHandler<FileSyncCommandOptions>
    {
        private readonly IConsole _console;
        private readonly PeriodicCodeRunner _periodicCodeRunner;

        public FileSyncCommandOptionsHandler(
            IConsole console, 
            PeriodicCodeRunner periodicCodeRunner)
        {
            _console = console;
            _periodicCodeRunner = periodicCodeRunner;
        }

        public async Task<int> HandleAsync(FileSyncCommandOptions options, CancellationToken cancellationToken)
        {
            _console.WriteLine("Running file sync command...");

            var inputPath = Environment.CurrentDirectory + "/" + options.In;
            var outputPath = Environment.CurrentDirectory + "/" + options.Out;

            await _periodicCodeRunner.RunPeriodicallyAsync((cancellationToken) =>
            {
                _console.WriteLine("Running file sync...");
                CreateOrUpdateFiles(inputPath, outputPath);
                DeleteFilesIfNotInInput(inputPath, outputPath);
            },
            cancellationToken);

            return await Task.FromResult(0);
        }

        private void CreateOrUpdateFiles(string inputPath, string outputPath)
        {
            foreach (var inputFilePath in Directory.GetFiles(inputPath))
            {
                var inputFileName = Path.GetFileName(inputFilePath);
                var outputFilePath = Path.Combine(outputPath, inputFileName);

                var isFileToBeCreated = !File.Exists(outputFilePath);
                var isFileToBeUpdated = File.GetLastWriteTime(inputFilePath) > File.GetLastWriteTime(outputFilePath);

                if (isFileToBeCreated || isFileToBeUpdated)
                {
                    File.Copy(inputFilePath, outputFilePath, true);

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
                .Where(file => !File.Exists(Path.Combine(inputPath, Path.GetFileName(file))))
                .ToList()
                .ForEach(file =>
                {
                    File.Delete(file);
                    _console.WriteLine($"{Path.GetFileName(file)} deleted.");
                });
        }
    }
}
