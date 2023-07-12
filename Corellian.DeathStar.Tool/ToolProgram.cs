using System.CommandLine;
using System.Diagnostics;

namespace Corellian.DeathStar.Tool
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ToolProgram
    {
        public static async Task<int> Main(string[] args)
        {
            var processIdArgument = new Argument<int>("process-id");

            var rootCommand = new RootCommand("Corellian DeathStar")
            {
                Name = "fire"
            };

            rootCommand.AddArgument(processIdArgument);

            rootCommand.SetHandler(Fire, processIdArgument);

            return await rootCommand.InvokeAsync(args);
        }

        private static async Task Fire(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);

                Console.WriteLine($"Found process with ID {process.Id} and name {process.ProcessName}.");

                Console.WriteLine("Attempting to kill the process...");

                var stopResult = await process.Stop(
                    2, TimeSpan.FromSeconds(5), 4, TimeSpan.FromSeconds(5),
                    1, TimeSpan.FromSeconds(5), 4, TimeSpan.FromSeconds(5));


                if (stopResult)
                {
                    Console.WriteLine("Successfully killed the process.");
                }
                else
                {
                    Console.WriteLine("Unable to kill the process.");
                }
            }
            catch (ArgumentException argumentException)
            {
                // Process is not running?
                Console.Error.WriteLine(argumentException.Message);
            }
        }
    }
}