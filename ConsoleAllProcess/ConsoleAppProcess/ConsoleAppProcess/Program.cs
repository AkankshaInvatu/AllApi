using System;
using System.Diagnostics;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("List of Running Applications:");

        var runningApplications = Process.GetProcesses()
            .Where(process =>
                !string.IsNullOrEmpty(process.MainWindowTitle) &&
                !string.IsNullOrWhiteSpace(process.MainWindowTitle) && 
                process.MainModule != null) 
            .Select(process => process.ProcessName);

        foreach (var application in runningApplications)
        {
            Console.WriteLine(application);
        }

        Console.ReadLine();
    }
}
