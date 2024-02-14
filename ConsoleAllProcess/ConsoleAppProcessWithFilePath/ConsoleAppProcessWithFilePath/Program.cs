using System;
using System.Management;

class Program
{
    static void Main()
    {
        // Log machine name
        string machineName = Environment.MachineName;
        LogToFile($"Machine Name: {machineName}");

      
        ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Process");
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
        ManagementObjectCollection processes = searcher.Get();

        // Log file path for each process
        foreach (ManagementObject process in processes)
        {
            try
            {
                string processName = process["Name"].ToString();
                string processFilePath = process["ExecutablePath"]?.ToString();

                if (!string.IsNullOrEmpty(processFilePath))
                {
                    LogToFile($"Process Name: {processName}, File Path: {processFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing process information: {ex.Message}");
            }
        }

        Console.WriteLine("Process information logged successfully. Press any key to exit.");
        Console.ReadKey();
    }

    static void LogToFile(string message)
    {
        string logFilePath = "process_log.txt"; 

        try
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now} - {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }
}

