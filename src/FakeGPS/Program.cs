using System;
using System.Diagnostics;
using System.Linq;
using FakeGPS.Common;
using Microsoft.Win32;

namespace FakeGPS
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // 1. Check for -s (Set Location)
                int sIndex = Array.FindIndex(args, x => x.Equals("-s", StringComparison.OrdinalIgnoreCase));
                if (sIndex != -1 && args.Length > sIndex + 1)
                {
                    string coordString = args[sIndex + 1];
                    var latLong = GeolocationHelper.ToLatLong(coordString);

                    SetRegistryLocation(latLong);

                    Console.WriteLine("The following location has been set in the driver's registry settings:");
                    Console.WriteLine("Lat:    {0}", latLong.Latitude);
                    Console.WriteLine("Long:   {0}", latLong.Longitude);

                    RestartDriver();
                    return 0;
                }

                // 2. Check for -g (Get Location)
                if (args.Any(x => x.Equals("-g", StringComparison.OrdinalIgnoreCase)))
                {
                    var latLong = GeolocationHelper.Get();

                    Console.WriteLine("The following location has been got from the Windows location API:");
                    Console.WriteLine("Lat:    {0}", latLong.Latitude);
                    Console.WriteLine("Long:   {0}", latLong.Longitude);

                    return 0;
                }

                // 3. Manual Help Output (Bypasses HelpText library errors)
                PrintUsage();
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return 1;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("FakeGPS v1.1");
            Console.WriteLine("Usage:");
            Console.WriteLine("  FakeGPS -s <lat,long>   Set latitude and longitude");
            Console.WriteLine("  FakeGPS -g              Get current status");
        }

        private static void SetRegistryLocation(LatLong latLong)
        {
            string keyPath = @"System\CurrentControlSet\Enum\ROOT\UNKNOWN\0000\Device Parameters\FakeGPS";
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath))
            {
                if (key != null)
                {
                    key.SetValue("Latitude", latLong.Latitude.ToString(), RegistryValueKind.String);
                    key.SetValue("Longitude", latLong.Longitude.ToString(), RegistryValueKind.String);
                }
                else
                {
                    throw new Exception("Please run as Administrator to update registry.");
                }
            }
        }

        private static void RestartDriver()
        {
            Console.WriteLine("Refreshing driver to clear Windows location cache...");
            try
            {
                // Script updated to target "FakeGPS Sensor" specifically
                string script = @"
                    $device = Get-PnpDevice | Where-Object { $_.FriendlyName -like '*FakeGPS*' } | Select-Object -First 1
                    if ($device) {
                        Disable-PnpDevice -InstanceId $device.InstanceId -Confirm:$false
                        Start-Sleep -Seconds 2
                        Enable-PnpDevice -InstanceId $device.InstanceId -Confirm:$false
                    }";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                }
                Console.WriteLine("Driver refresh complete!");
            }
            catch
            {
                Console.WriteLine("Note: Auto-refresh failed. Please ensure you are running as Administrator.");
            }
        }
    }
}