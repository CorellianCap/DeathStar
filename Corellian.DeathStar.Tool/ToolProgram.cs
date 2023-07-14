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
                name: "process-id", description: "The ID of the process to kill.")
                {
                    Arity = ArgumentArity.ExactlyOne
                };

            var rootCommand = new RootCommand("Corellian DeathStar")
            {
                Name = "fire",
                Description = "A tool send a signal to a process."
            };

            rootCommand.AddArgument(processIdArgument);

            rootCommand.SetHandler(c => Fire(c.ParseResult.GetValueForArgument(processIdArgument), c.Console));

            return await rootCommand.InvokeAsync(args);
        }

        private static async Task Fire(int processId, IConsole console)
        {
            try
            {
                var process = Process.GetProcessById(processId);

                console.Out.WriteLine($"Found process with ID {process.Id} and name {process.ProcessName}.");

                console.Out.WriteLine("Attempting to kill the process...");

                var stopResult = await process.Stop(
                    2, TimeSpan.FromSeconds(5), 4, TimeSpan.FromSeconds(5),
                    1, TimeSpan.FromSeconds(5), 4, TimeSpan.FromSeconds(5));

                if (stopResult)
                {
                    console.Out.WriteLine("Successfully killed the process.");
                }
                else
                {
                    console.Out.WriteLine("Unable to kill the process.");
                }
            }
            catch (ArgumentException argumentException)
            {
                // Process is not running?
                console.Error.WriteLine(argumentException.Message);
            }
        }
    }
}