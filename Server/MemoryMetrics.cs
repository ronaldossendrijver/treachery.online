// /*
//  * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
//  * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
//  * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
// */

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Treachery.Server;

public class MemoryMetrics
{
    public double Total;
    public double Used;
    public double Free;
}
 
public class MemoryMetricsClient
{
    public MemoryMetrics GetMetrics()
    {
        MemoryMetrics metrics;
 
        if (IsUnix())
        {
            metrics = GetUnixMetrics();
        }
        else
        {
            metrics = GetWindowsMetrics();
        }
  
        return metrics;
    }
 
    private bool IsUnix()
    {
        var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
 
        return isUnix;
    }
 
    private MemoryMetrics GetWindowsMetrics()
    {
        var output = "";
 
        var info = new ProcessStartInfo();
        info.FileName = "wmic";
        info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
        info.RedirectStandardOutput = true;
 
        using (var process = Process.Start(info))
        {
            output = process.StandardOutput.ReadToEnd();
        }
 
        var lines = output.Trim().Split("\n");
        var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
        var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);
 
        var metrics = new MemoryMetrics();
        metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
        metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);
        metrics.Used = metrics.Total - metrics.Free;
 
        return metrics;
    }
 
    private MemoryMetrics GetUnixMetrics()
    {
        var output = "";
 
        var info = new ProcessStartInfo("free -m");
        info.FileName = "/bin/bash";
        info.Arguments = "-c \"free -m\"";
        info.RedirectStandardOutput = true;
 
        using (var process = Process.Start(info))
        {
            output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
        }
 
        var lines = output.Split("\n");
        var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);
 
        var metrics = new MemoryMetrics();
        metrics.Total = double.Parse(memory[1]);
        metrics.Used = double.Parse(memory[2]);
        metrics.Free = double.Parse(memory[3]);
 
        return metrics;
    }
}