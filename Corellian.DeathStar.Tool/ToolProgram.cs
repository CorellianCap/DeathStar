using System.CommandLine;
using System.CommandLine.IO;
using System.Diagnostics;

namespace Corellian.DeathStar.Tool
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ToolProgram
    {
        public static async Task<int> Main(string[] args)
        {
            var processIdArgument = new Argument<int>(
                name: "process-id", description: "The ID of the process to terminate.")
                {
                    Arity = ArgumentArity.ExactlyOne
                };

            var signalKillRetryCountOption = new Option<int>(
                aliases: new[] { "--signal-count", "-sc" },
                description: "The number of times to attempt to use a signal to terminate the process.",
                getDefaultValue: () => 2);

            var processKillRetryCountOption = new Option<int>(
                aliases: new[] { "--kill-count", "-kc" },
                description: "The number of times to attempt to use .NET process kill to terminate the process.",
                getDefaultValue: () => 2);

            var rootCommand = new RootCommand("Corellian DeathStar")
            {
                Name = "fire",
                Description = "A tool to send a signal or use .NET process kill to terminate to a process."
            };

            rootCommand.AddArgument(processIdArgument);
            rootCommand.AddOption(signalKillRetryCountOption);
            rootCommand.AddOption(processKillRetryCountOption);

            rootCommand.SetHandler(c => Fire(
                c.ParseResult.GetValueForArgument(processIdArgument),
                c.ParseResult.GetValueForOption(signalKillRetryCountOption),
                c.ParseResult.GetValueForOption(processKillRetryCountOption),
                c.Console));

            return await rootCommand.InvokeAsync(args);
        }

        private static async Task Fire(int processId, int signalKillRetryCount, int processKillRetryCount, IConsole console)
        {
            try
            {
                var process = Process.GetProcessById(processId);

                console.Out.WriteLine($"Found process with ID {process.Id} and name {process.ProcessName}.");

                console.Out.WriteLine("Attempting to terminate the process...");

                var stopResult = await process.Stop(
                    2, TimeSpan.FromSeconds(5), 4, TimeSpan.FromSeconds(5),
                    1, TimeSpan.FromSeconds(5), 4, TimeSpan.FromSeconds(5));

                var stopResultMessage = stopResult switch
                {
                    ProcessExs.StopResult.None => "Unable to terminate the process.",
                    ProcessExs.StopResult.Signal => "Successfully used signal to terminate the process.",
                    ProcessExs.StopResult.Kill => "Successfully used .NET process kill to terminate the process.",
                    _ => throw new ArgumentOutOfRangeException(nameof(stopResult), stopResult, null)
                };

                console.Out.WriteLine(stopResultMessage);
            }
            catch (ArgumentException argumentException)
            {
                // Process is not running?
                console.Error.WriteLine(argumentException.Message);
            }
        }
    }
}