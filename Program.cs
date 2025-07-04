using LibreHardwareMonitor.Hardware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
builder.WebHost.UseUrls("http://localhost:5000");

Computer computer = new()
{
    IsCpuEnabled = true,
    IsGpuEnabled = true,
    IsMemoryEnabled = true,
    IsStorageEnabled = true
};

computer.Open();

ISensor? cpuTempSensor = null, cpuPowerSensor = null, gpuLoadSensor = null, gpuTempSensor = null, gpuFanSensor = null;

var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
var diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");

ulong totalRamMb = GetTotalRamMb();

FindSensors();

app.MapGet("/metrics", () =>
{
    foreach (var hw in computer.Hardware)
        hw.Update();

    float cpuUsage = cpuCounter.NextValue();
    float ramUsage = (float)(totalRamMb - ramCounter.NextValue());
    float diskUsage = diskCounter.NextValue();

    return Results.Json(new
    {
        cpuUsage,
        cpuTemp = cpuTempSensor?.Value ?? 0,
        cpuPower = cpuPowerSensor?.Value ?? 0,
        gpuLoad = gpuLoadSensor?.Value ?? 0,
        gpuTemp = gpuTempSensor?.Value ?? 0,
        gpuFan = gpuFanSensor?.Value ?? 0,
        ramUsage,
        diskUsage
    });
});

app.MapGet("/health", () => Results.Ok("OK"));

app.Run();

void FindSensors()
{
    Console.WriteLine("=== Detected sensors ===");

    foreach (var hw in computer.Hardware)
    {
        hw.Update();
        Console.WriteLine($"[{hw.HardwareType}] {hw.Name}");

        foreach (var sensor in hw.Sensors)
        {
            Console.WriteLine($"  > {sensor.SensorType} | {sensor.Name} | {sensor.Value}");

            if (hw.HardwareType == HardwareType.Cpu)
            {
                if (sensor.SensorType == SensorType.Temperature &&
                    cpuTempSensor == null &&
                    sensor.Name.Contains("Package", StringComparison.OrdinalIgnoreCase))
                {
                    cpuTempSensor = sensor;
                }

                if (sensor.SensorType == SensorType.Power &&
                    cpuPowerSensor == null &&
                    sensor.Name.Contains("Package", StringComparison.OrdinalIgnoreCase))
                {
                    cpuPowerSensor = sensor;
                }
            }

            if (hw.HardwareType == HardwareType.GpuNvidia || hw.HardwareType == HardwareType.GpuAmd)
            {
                if (sensor.SensorType == SensorType.Load && gpuLoadSensor == null)
                    gpuLoadSensor = sensor;

                if (sensor.SensorType == SensorType.Temperature && gpuTempSensor == null)
                    gpuTempSensor = sensor;

                if (sensor.SensorType == SensorType.Fan && gpuFanSensor == null)
                    gpuFanSensor = sensor;
            }
        }

        foreach (var sub in hw.SubHardware)
        {
            sub.Update();
            foreach (var sensor in sub.Sensors)
            {
                Console.WriteLine($"    > {sensor.SensorType} | {sensor.Name} | {sensor.Value}");
            }
        }
    }

    Console.WriteLine("=== End of list ===");
}

ulong GetTotalRamMb()
{
    ulong ramBytes = 0;
    var searcher = new System.Management.ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");

    foreach (var obj in searcher.Get())
    {
        ramBytes += (ulong)obj["Capacity"];
    }

    return ramBytes / (1024 * 1024);
}
