using System;
using System.Diagnostics;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("List of Running Application Processes:");

        var runningProcesses = Process.GetProcesses()
            .Where(process => !string.IsNullOrEmpty(process.MainWindowTitle) && process.MainModule != null)
            .GroupBy(process => process.ProcessName.ToLower())
            .Select(group => new
            {
                ProcessName = group.Key,
                Count = group.Count(),
                FirstProcess = group.First()
            });

        int totalProcesses = 0;

        foreach (var processGroup in runningProcesses)
        {
            Console.WriteLine($"{processGroup.ProcessName} (ID: {processGroup.FirstProcess.Id}) - Copies: {processGroup.Count}");
            totalProcesses += processGroup.Count;
        }

        Console.WriteLine($"\nTotal Number of Processes: {totalProcesses}");

        Console.ReadLine();
    }
}
