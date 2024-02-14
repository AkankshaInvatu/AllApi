using System;
using System.Diagnostics;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("List of Running Processes:");

        var runningProcesses = Process.GetProcesses()
            .GroupBy(process => process.ProcessName)
            .Select(group => group.First());

        foreach (var process in runningProcesses)
        {
            Console.WriteLine($"{process.ProcessName} (ID: {process.Id})");
        }

        Console.ReadLine();
    }
}
