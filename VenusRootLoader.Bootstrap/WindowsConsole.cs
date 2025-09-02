using System.Text;
using Microsoft.Extensions.Hosting;

namespace VenusRootLoader.Bootstrap;

/// <summary>
/// This class handles the binding of Windows's console for usage in our logs. It also prevents the console's streams to
/// be closed by Unity
/// </summary>
internal class WindowsConsole : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // The actual logic that creates the console if needed is done on the C++ side because it is required to perform
        // this logic during DllMain under a loader lock due to the need to do this before UnityPlayer.dll's CRT initialisation.
        // Since it's not possible to initialise the bootstrap under loader lock as of .NET 10, the console's creation
        // has to be handled on the C++ side
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));

        Console.OutputEncoding = Encoding.UTF8;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}