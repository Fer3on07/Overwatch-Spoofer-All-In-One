using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverwatchSpooferAllInOne
{
    /*
    مصمم اكتبهالك بالعربي دي بقى
    الدنيا دي كلها تحت رجلك يا باشا واعمل حاجتك بنفسك
    هدية وصدقة لوجه الله تعالى بدل ما تدفع فلوس في الأونطه
    */

    internal class Program
    {
        public static void DeleteDirectories(char drive)
        {
            string user = Environment.UserName;

            var directories = new List<string>
    {
        $@"{drive}:\Windows\Temp",
        $@"{drive}:\Windows\Prefetch",
        $@"{drive}:\Users\{user}\AppData\Local\Battle.net",
        $@"{drive}:\Users\{user}\AppData\Local\Blizzard",
        $@"{drive}:\Users\{user}\AppData\Local\Blizzard Entertainment",
        $@"{drive}:\Users\{user}\AppData\Roaming\Battle.net",
        /*
        I removed the documents just so that the saved settings don't get lost
        like the game’s size, sound, and quality
        */
        //$@"{drive}:\Users\{user}\Documents",
        $@"{drive}:\ProgramData\Battle.net",
        $@"{drive}:\ProgramData\Blizzard Entertainment"
    };

            foreach (var dir in directories)
            {
                try
                {
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  [+] Cleaned {dir}");
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public static void DeleteRegistryKeys()
        {
            var keys = new List<string>
    {
        @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Blizzard Entertainment",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Blizzard Entertainment\Battle.net",
        @"HKEY_CURRENT_USER\SOFTWARE\Blizzard Entertainment",
        @"HKEY_CURRENT_USER\SOFTWARE\Activision",
        @"HKEY_CLASSES_ROOT\Applications\Overwatch.exe"
    };

            foreach (var key in keys)
            {
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(key, false);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  [+] Cleaned {key}");
                }
                catch (Exception)
                {
                }
            }
        }

        static string ReadRegistryValue(RegistryKey baseKey, string subKey, string valueName)
        {
            try
            {
                using var key = baseKey.OpenSubKey(subKey);
                return key?.GetValue(valueName)?.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        static void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  [+] Cleaned {path}");
                }
            }
            catch (Exception)
            {
            }
        }

        static void CleanupAgentDirectories(string agentsPath)
        {
            if (!Directory.Exists(agentsPath)) return;

            string latestAgent = null;
            int greatest = 0;

            var directories = Directory.GetDirectories(agentsPath)
                .Where(dir => Path.GetFileName(dir).StartsWith("Agent"));

            foreach (var dir in directories)
            {
                var parts = Path.GetFileName(dir).Split('.');
                if (parts.Length > 1 && int.TryParse(parts[1], out int agentID) && agentID > greatest)
                {
                    greatest = agentID;
                    latestAgent = dir;
                }
            }

            foreach (var dir in directories)
            {
                if (dir != latestAgent)
                {
                    DeleteDirectory(dir);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  [+] Cleaned {dir}");
                }
            }
        }

        static void DeleteRegistryKey(string keyPath)
        {
            try
            {
                if (keyPath.StartsWith(@"HKEY_LOCAL_MACHINE"))
                {
                    string subKey = keyPath.Substring(@"HKEY_LOCAL_MACHINE\".Length);
                    Registry.LocalMachine.DeleteSubKeyTree(subKey, false);
                }
                else if (keyPath.StartsWith(@"HKEY_CURRENT_USER"))
                {
                    string subKey = keyPath.Substring(@"HKEY_CURRENT_USER\".Length);
                    Registry.CurrentUser.DeleteSubKeyTree(subKey, false);
                }
                else if (keyPath.StartsWith(@"HKEY_CLASSES_ROOT"))
                {
                    string subKey = keyPath.Substring(@"HKEY_CLASSES_ROOT\".Length);
                    using var baseKey = Registry.ClassesRoot;
                    baseKey.DeleteSubKeyTree(subKey, false);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  [+] Deleted {keyPath}");
            }
            catch (Exception)
            {
            }
        }

        static bool DeletePath(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true); // Delete all contents
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  [+] Deleted {path}");
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        static void RunCleanup()
        {
            // List of directories to delete
            List<string> directoriesToDelete = new List<string>
        {
            Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "Battle.net"),
            Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "Blizzard"),
            Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "Blizzard Entertainment"),
            Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Battle.net"),
            Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "Documents\\Overwatch\\Logs"),
            Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Battle.net\\Setup"),
            Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Battle.net\\Agent\\data"),
            Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Battle.net\\Agent\\Logs"),
            Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Blizzard Entertainment")
        };

            // Delete specified directories
            foreach (string dir in directoriesToDelete)
            {
                DeletePath(dir);
            }

            // Registry keys to delete
            List<string> registryKeys = new List<string>
        {
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Blizzard Entertainment",
            @"HKEY_CURRENT_USER\Software\Blizzard Entertainment"
        };

            foreach (string key in registryKeys)
            {
                DeleteRegistryKey(key);
            }

            // Read Overwatch install location from registry
            string installLocation = ReadRegistryValue(
                Registry.LocalMachine,
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Overwatch",
                "InstallLocation"
            );

            // Delete cache and GPUCache directories
            if (!string.IsNullOrEmpty(installLocation))
            {
                DeletePath(Path.Combine(installLocation, @"_retail_\cache"));
                DeletePath(Path.Combine(installLocation, @"_retail_\GPUCache"));
            }

            // Cleanup old Battle.net agents, keeping the latest
            string agentPath = Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Battle.net\\Agent");
            if (Directory.Exists(agentPath))
            {
                CleanupAgentDirectories(agentPath);
            }
        }








        private static void ConsoleDelay(int delay = 2000)
        {
            Thread.Sleep(delay);
        }

        private static void ConsoleDelayThenClear(int delay = 5000)
        {
            Thread.Sleep(delay);
            Console.Clear();
        }

        public static string RandomId(int length)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string result = "";
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result += chars[random.Next(chars.Length)];
            }

            return result;
        }

        public static string RandomIdprid(int length)
        {
            const string digits = "0123456789";
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();
            var id = new char[length];
            int dashIndex = 5;
            int letterIndex = 17;
            for (int i = 0; i < length; i++)
            {
                if (i == dashIndex)
                {
                    id[i] = '-';
                    dashIndex += 6;
                }
                else if (i == letterIndex)
                {
                    id[i] = letters[random.Next(letters.Length)];
                }
                else if (i == letterIndex + 1)
                {
                    id[i] = letters[random.Next(letters.Length)];
                }
                else
                {
                    id[i] = digits[random.Next(digits.Length)];
                }
            }
            return new string(id);
        }

        public static string RandomIdprid2(int length)
        {
            const string digits = "0123456789";
            const string letters = "abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            var id = new char[32];
            int letterIndex = 0;

            for (int i = 0; i < 32; i++)
            {
                if (i == 8 || i == 13 || i == 18 || i == 23)
                {
                    id[i] = '-';
                }
                else if (i % 5 == 4)
                {
                    id[i] = letters[random.Next(letters.Length)];
                    letterIndex++;
                }
                else
                {
                    id[i] = digits[random.Next(digits.Length)];
                }
            }

            return new string(id);
        }

        public static int RandomDisplayId()
        {
            Random rnd = new Random();
            return rnd.Next(1, 9);
        }

        public static string RandomMac()
        {
            string chars = "ABCDEF0123456789";
            string windows = "26AE";
            string result = "";
            Random random = new Random();

            result += chars[random.Next(chars.Length)];
            result += windows[random.Next(windows.Length)];

            for (int i = 0; i < 5; i++)
            {
                result += "-";
                result += chars[random.Next(chars.Length)];
                result += chars[random.Next(chars.Length)];

            }

            return result;
        }

        public static void Enable_LocalAreaConection(string adapterId, bool enable = true)
        {
            string interfaceName = "Ethernet";
            foreach (NetworkInterface i in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (i.Id == adapterId)
                {
                    interfaceName = i.Name;
                    break;
                }
            }

            string control;
            if (enable)
                control = "enable";
            else
                control = "disable";

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("netsh", $"interface set interface \"{interfaceName}\" {control}");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
        }

        public static void SpoofPCName()
        {
            string randomName = RandomId(8); // Generate a random PC name
            using RegistryKey computerName = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\ComputerName\\ComputerName", true);
            computerName.SetValue("ComputerName", randomName);
            computerName.SetValue("ActiveComputerName", randomName);
            computerName.SetValue("ComputerNamePhysicalDnsDomain", "");

            using RegistryKey activeComputerName = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\ComputerName\\ActiveComputerName", true);
            activeComputerName.SetValue("ComputerName", randomName);
            activeComputerName.SetValue("ActiveComputerName", randomName);
            activeComputerName.SetValue("ComputerNamePhysicalDnsDomain", "");

            using RegistryKey tcpipParams = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters", true);
            tcpipParams.SetValue("Hostname", randomName);
            tcpipParams.SetValue("NV Hostname", randomName);

            using RegistryKey tcpipInterfaces = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces", true);
            foreach (string interfaceKey in tcpipInterfaces.GetSubKeyNames())
            {
                using RegistryKey interfaceSubKey = tcpipInterfaces.OpenSubKey(interfaceKey, true);
                interfaceSubKey.SetValue("Hostname", randomName);
                interfaceSubKey.SetValue("NV Hostname", randomName);
            }
        }

        public static void SpoofInstallationID()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", true))
            {
                if (key != null)
                {
                    string newInstallationID = Guid.NewGuid().ToString();
                    key.SetValue("InstallationID", newInstallationID);
                    key.Close();
                }
            }
        }

        public static void SpoofProductID()
        {
            try
            {
                using (RegistryKey productKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", true))
                {
                    if (productKey != null)
                    {
                        string originalProductId = productKey.GetValue("ProductId")?.ToString();

                        string newProductId = RandomIdprid(20);
                        productKey.SetValue("ProductId", newProductId);

                        ConsoleDelay(1000);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("  [+] Product ID spoofed");
                    }
                    else
                    {
                        ConsoleDelay(1000);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("  [-] Product registry key not found");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [X] " + ex.Message);
            }
        }

        public static void SpoofDisks()
        {
            using RegistryKey ScsiPorts = Registry.LocalMachine.OpenSubKey("HARDWARE\\DEVICEMAP\\Scsi");
            foreach (string port in ScsiPorts.GetSubKeyNames())
            {
                using RegistryKey ScsiBuses = Registry.LocalMachine.OpenSubKey($"HARDWARE\\DEVICEMAP\\Scsi\\{port}");
                foreach (string bus in ScsiBuses.GetSubKeyNames())
                {
                    using RegistryKey ScsuiBus = Registry.LocalMachine.OpenSubKey($"HARDWARE\\DEVICEMAP\\Scsi\\{port}\\{bus}\\Target Id 0\\Logical Unit Id 0", true);
                    if (ScsuiBus != null)
                    {
                        if (ScsuiBus.GetValue("DeviceType").ToString() == "DiskPeripheral")
                        {
                            string identifier = RandomId(14);
                            string serialNumber = RandomId(14);

                            ScsuiBus.SetValue("DeviceIdentifierPage", Encoding.UTF8.GetBytes(serialNumber));
                            ScsuiBus.SetValue("Identifier", identifier);
                            ScsuiBus.SetValue("InquiryData", Encoding.UTF8.GetBytes(identifier));
                            ScsuiBus.SetValue("SerialNumber", serialNumber);
                        }
                    }
                }
            }

            using RegistryKey DiskPeripherals = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\MultifunctionAdapter\\0\\DiskController\\0\\DiskPeripheral");
            foreach (string disk in DiskPeripherals.GetSubKeyNames())
            {
                using RegistryKey DiskPeripheral = Registry.LocalMachine.OpenSubKey($"HARDWARE\\DESCRIPTION\\System\\MultifunctionAdapter\\0\\DiskController\\0\\DiskPeripheral\\{disk}", true);
                DiskPeripheral.SetValue("Identifier", $"{RandomId(8)}-{RandomId(8)}-A");
            }
        }

        public static void SpoofGUIDs()
        {
            try
            {
                using RegistryKey HardwareGUID = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\IDConfigDB\\Hardware Profiles\\0001", true);
                HardwareGUID.SetValue("HwProfileGuid", $"{{{Guid.NewGuid()}}}");

                using RegistryKey MachineGUID = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Cryptography", true);
                MachineGUID.SetValue("MachineGuid", Guid.NewGuid().ToString());

                using RegistryKey MachineId = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\SQMClient", true);
                MachineId.SetValue("MachineId", $"{{{Guid.NewGuid()}}}");

                using RegistryKey SystemInfo = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\SystemInformation", true);

                Random rnd = new Random();
                int day = rnd.Next(1, 31);
                string dayStr = "";
                if (day < 10) dayStr = $"0{day}";
                else dayStr = day.ToString();

                int month = rnd.Next(1, 13);
                string monthStr = "";
                if (month < 10) monthStr = $"0{month}";
                else monthStr = month.ToString();

                SystemInfo.SetValue("BIOSReleaseDate", $"{dayStr}/{monthStr}/{rnd.Next(2000, 2023)}");
                SystemInfo.SetValue("BIOSVersion", RandomId(10));
                SystemInfo.SetValue("ComputerHardwareId", $"{{{Guid.NewGuid()}}}");
                SystemInfo.SetValue("ComputerHardwareIds", $"{{{Guid.NewGuid()}}}\n{{{Guid.NewGuid()}}}\n{{{Guid.NewGuid()}}}\n");
                SystemInfo.SetValue("SystemManufacturer", RandomId(15));
                SystemInfo.SetValue("SystemProductName", RandomId(6));

                using RegistryKey Update = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate", true);
                if (Update != null)
                {
                    Update.SetValue("SusClientId", Guid.NewGuid().ToString());
                    Update.SetValue("SusClientIdValidation", Encoding.UTF8.GetBytes(RandomId(25)));
                }
                else
                {
                    Console.WriteLine("  [X] Registry key 'WindowsUpdate' not found");

                    using RegistryKey Updatex = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate", true);
                    Updatex.SetValue("SusClientId", Guid.NewGuid().ToString());
                    Updatex.SetValue("SusClientIdValidation", Encoding.UTF8.GetBytes(RandomId(25)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("  [X] " + ex.ToString());
            }
        }

        public static void SpoofEFIVariableId()
        {
            try
            {
                RegistryKey efiVariables = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Nsi\\{eb004a03-9b1a-11d4-9123-0050047759bc}\\26", true);
                if (efiVariables != null)
                {
                    string efiVariableId = Guid.NewGuid().ToString();
                    efiVariables.SetValue("VariableId", efiVariableId);
                    efiVariables.Close();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\n[X] Start the spoofer in admin mode to spoof your MAC address!");
            }
        }

        public static void SpoofSMBIOSSerialNumber()
        {
            try
            {
                RegistryKey smbiosData = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\BIOS", true);

                if (smbiosData != null)
                {
                    string serialNumber = RandomId(10);
                    smbiosData.SetValue("SystemSerialNumber", serialNumber);
                    smbiosData.Close();
                }
                else
                {
                    Console.WriteLine("\n  [X] Cant find the SMBIOS");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\n  [X] Start the spoofer in admin mode to spoof your MAC address!");
            }
        }

        public static bool SpoofMAC()

        {
            bool err = false;

            using RegistryKey NetworkAdapters = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}");
            foreach (string adapter in NetworkAdapters.GetSubKeyNames())
            {
                if (adapter != "Properties")
                {
                    try
                    {
                        using RegistryKey NetworkAdapter = Registry.LocalMachine.OpenSubKey($"SYSTEM\\CurrentControlSet\\Control\\Class\\{{4d36e972-e325-11ce-bfc1-08002be10318}}\\{adapter}", true);
                        if (NetworkAdapter.GetValue("BusType") != null)
                        {
                            NetworkAdapter.SetValue("NetworkAddress", RandomMac());
                            string adapterId = NetworkAdapter.GetValue("NetCfgInstanceId").ToString();
                            Enable_LocalAreaConection(adapterId, false);
                            Enable_LocalAreaConection(adapterId, true);

                        }
                    }
                    catch (System.Security.SecurityException)
                    {
                        Console.WriteLine("  [X] Start the spoofer in admin mode to spoof your MAC address!");
                        err = true;
                        break;
                    }
                }
            }

            return err;
        }

        public static void SpoofGPU()
        {
            string keyName = @"SYSTEM\CurrentControlSet\Enum\PCI\VEN_10DE&DEV_0DE1&SUBSYS_37621462&REV_A1";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true))
            {
                if (key != null)
                {
                    string newHardwareID = "PCIVEN_8086&DEV_1234&SUBSYS_5678ABCD&REV_01";
                    string oldHardwareID = key.GetValue("HardwareID") as string;

                    key.SetValue("HardwareID", newHardwareID);
                    key.SetValue("CompatibleIDs", new string[] { newHardwareID });
                    key.SetValue("Driver", "pci.sys");
                    key.SetValue("ConfigFlags", 0x00000000, RegistryValueKind.DWord);
                    key.SetValue("ClassGUID", "{4d36e968-e325-11ce-bfc1-08002be10318}");
                    key.SetValue("Class", "Display");

                    key.Close();
                }
            }
        }

        public static void SpoofDisplay()
        {
            try
            {
                RegistryKey displaySettings = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RunMRU", true);

                if (displaySettings != null)
                {
                    string originalDisplayId = displaySettings.GetValue("MRU0")?.ToString();
                    int displayId = RandomDisplayId();
                    string spoofedDisplayId = $"SpoofedDisplay{displayId}";

                    displaySettings.SetValue("MRU0", spoofedDisplayId);
                    displaySettings.SetValue("MRU1", spoofedDisplayId);
                    displaySettings.SetValue("MRU2", spoofedDisplayId);
                    displaySettings.SetValue("MRU3", spoofedDisplayId);
                    displaySettings.SetValue("MRU4", spoofedDisplayId);

                    ConsoleDelay(1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  [+] Display spoofed");
                }
                else
                {
                    ConsoleDelay(1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  [-] Display settings registry key not found");
                }
            }
            catch (Exception ex)
            {
                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [X] Error: " + ex.Message);
            }
        }

        public static void DisplaySystemData()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  [!] System Data:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;
            try
            {
                // Display HWID
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", true))
                {
                    string installationID = key.GetValue("InstallationID") as string;
                    Console.WriteLine("  [+] HWID:              " + installationID);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  [-] Error retrieving HWID: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.Green;
            }

            try
            {
                // Display GUIDs
                using (RegistryKey machineGuidKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Cryptography"))
                {
                    string machineGuid = machineGuidKey.GetValue("MachineGuid") as string;
                    Console.WriteLine("  [+] Machine GUID:      " + machineGuid);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  [-] Error retrieving Machine GUID: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.Green;
            }

            try
            {
                // Display MAC ID
                foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    PhysicalAddress physicalAddress = networkInterface.GetPhysicalAddress();
                    Console.WriteLine("  [+] MAC ID (" + networkInterface.Name + "):     " + physicalAddress.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  [-] Error retrieving MAC ID: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.Green;
            }

            try
            {
                // Display Installation ID
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", true))
                {
                    string installationID = key.GetValue("InstallationID") as string;
                    Console.WriteLine("  [+] Installation ID:    " + installationID);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  [-] Error retrieving Installation ID: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.Green;
            }

            try
            {
                // Display PC Name
                using (RegistryKey computerName = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\ComputerName\\ComputerName"))
                {
                    string pcName = computerName.GetValue("ComputerName") as string;
                    Console.WriteLine("  [+] PC Name:           " + pcName);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  [-] Error retrieving PC Name: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ------------------------------------------------------\n");
        }

        public static void FlushDnsCache()
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.FileName = "ipconfig";
                startInfo.Arguments = "/flushdns";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [+] DNS-Cache cleared");
            }
            catch (Exception ex)
            {
                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [X] " + ex.Message);
            }
        }

        public static void ClearWindowsLogs()
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.FileName = "wevtutil";
                startInfo.Arguments = "el";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [+] Windows Logs cleared");
            }
            catch (Exception ex)
            {
                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [X] " + ex.Message);
            }
        }

        public static void ClearTemporaryCache()
        {
            try
            {
                string tempFolderPath = Path.GetTempPath();
                DirectoryInfo tempDirectory = new DirectoryInfo(tempFolderPath);

                DateTime thresholdDate = DateTime.Now.AddDays(-3);

                // Delete files older than 3 days
                foreach (FileInfo file in tempDirectory.GetFiles())
                {
                    try
                    {
                        if (file.LastWriteTime < thresholdDate)
                        {
                            file.Delete();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"  [!] Could not delete file: {file.FullName}. Reason: {ex.Message}");
                    }
                }

                // Delete subdirectories
                foreach (DirectoryInfo subDirectory in tempDirectory.GetDirectories())
                {
                    try
                    {
                        subDirectory.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"  [!] Could not delete directory: {subDirectory.FullName}. Reason: {ex.Message}");
                    }
                }

                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [+] Temporary cache cleared");
            }
            catch (Exception ex)
            {
                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [X] " + ex.Message);
            }

        }

        public static void ClearWindowsTempLol()
        {
            try
            {
                string tempFolderPath = Path.GetTempPath();
                DirectoryInfo tempDirectory = new DirectoryInfo(tempFolderPath);

                foreach (FileInfo file in tempDirectory.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo subDirectory in tempDirectory.GetDirectories())
                {
                    subDirectory.Delete(true);
                }

                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [+] Windows Temp folder cleared");
            }
            catch (Exception ex)
            {
                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [X] " + ex.Message);
            }
        }

        public static void TcpRst()
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.FileName = "netsh";
                startInfo.Arguments = "int ip reset";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [+] TCP/IP reset successful");
            }
            catch (Exception ex)
            {
                ConsoleDelay(1000);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [X] " + ex.Message);
            }
        }
        /*
        I removed the documents just so that the saved settings don't get lost
        like the game’s size, sound, and quality
        */

        //public static void DocsClear()
        //{
        //    try
        //    {
        //        string recentDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
        //        string[] recentDocuments = Directory.GetFiles(recentDocumentsPath);
        //
        //        foreach (string document in recentDocuments)
        //        {
        //            File.Delete(document);
        //        }
        //
        //        ConsoleDelay(1000);
        //        Console.ForegroundColor = ConsoleColor.Green;
        //        Console.WriteLine($"  [+] Recent documents cleared");
        //    }
        //    catch (Exception ex)
        //    {
        //        ConsoleDelay(1000);
        //        Console.ForegroundColor = ConsoleColor.Red;
        //        Console.WriteLine($"  [X] {ex.Message}");
        //    }
        //}

        static void Main(string[] args)
        {
            Console.SetWindowSize(90, 25);
            Console.SetBufferSize(90, 25);

            Console.Title = "Overwatch 2 Spoofer By Fer3on From theSMURFS Discord Server";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();

            Console.WriteLine("                                            ");
            Console.WriteLine("       ███████╗███████╗██████╗ ██████╗  ██████╗ ███╗   ██╗");
            Console.WriteLine("       ██╔════╝██╔════╝██╔══██╗╚════██╗██╔═══██╗████╗  ██║");
            Console.WriteLine("       █████╗  █████╗  ██████╔╝ █████╔╝██║   ██║██╔██╗ ██║");
            Console.WriteLine("       ██╔══╝  ██╔══╝  ██╔══██╗ ╚═══██╗██║   ██║██║╚██╗██║");
            Console.WriteLine("       ██║     ███████╗██║  ██║██████╔╝╚██████╔╝██║ ╚████║");
            Console.WriteLine("       ╚═╝     ╚══════╝╚═╝  ╚═╝╚═════╝  ╚═════╝ ╚═╝  ╚═══╝");
            Console.WriteLine("                                                     ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  !!!  Overwatch 2 Spoofer made with love in Egypt by Fer3on  !!!");
            Console.WriteLine("          !!!  Made Exclusively for theSMURFS only  !!!");
            Console.WriteLine("                                                     ");
            Console.WriteLine("                                                     ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [#] Press Enter to start Overwatch Cleaner...");
            Console.ReadLine();
            Console.Clear();
            Process.Start("https://discord.gg/279P422TWZ");
            List<Action> patches = new List<Action>
            {
                () =>
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\n  [#] Killing overwatch process");
                    var processes = new List<string> { "battle.net", "agent", "overwatch" };
                    foreach (var proc in processes)
                    {
                        foreach (var process in Process.GetProcessesByName(proc))
                        {
                            try
                            {
                                process.Kill();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"  [+] {proc} killed");
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"  [-] Failed to kill process {proc}: {ex.Message}");
                            }
                        }
                    }

                    ConsoleDelay(4000);
                    Console.Clear();

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"\n  [#] Patching drives");
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        if (!drive.IsReady) continue;
                        var driveLetter = drive.Name[0];

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"  [!] Patching {driveLetter}:");
                        DeleteDirectories(driveLetter);
                        DeleteRegistryKeys();
                    }
                }
            };

            // Execute all patches
            foreach (var patch in patches)
            {
                patch.Invoke();
            }

            ConsoleDelay(4000);
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [#] Cleaning overwatch files");

            // Read Install Location from Registry
            string installLocation = ReadRegistryValue(
                Registry.LocalMachine,
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Overwatch",
                "InstallLocation"
            );

            if (!string.IsNullOrEmpty(installLocation))
            {
                // Delete directories in Overwatch installation folder
                DeleteDirectory(Path.Combine(installLocation, @".battle.net"));
                DeleteDirectory(Path.Combine(installLocation, @"_retail_\cache"));
                DeleteDirectory(Path.Combine(installLocation, @"_retail_\GPUCache"));
            }

            // Agent path cleanup
            string agentsPath = @"C:\ProgramData\Battle.net\Agent";
            CleanupAgentDirectories(agentsPath);

            // Registry keys to delete
            var registryKeys = new List<string>
        {
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Blizzard Entertainment",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Blizzard Entertainment\Battle.net",
            @"HKEY_CURRENT_USER\SOFTWARE\Blizzard Entertainment",
            @"HKEY_CURRENT_USER\SOFTWARE\Activision",
            @"HKEY_CLASSES_ROOT\Applications\Overwatch.exe",
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone\NonPackaged",
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\RADAR\HeapLeakDetection\DiagnosedApplications\Overwatch.exe",
            @"HKEY_CURRENT_USER\VirtualStore\MACHINE\SOFTWARE\WOW6432Node\Activision",
            @"HKEY_CURRENT_USER\SOFTWARE\Classes\VirtualStore\MACHINE\SOFTWARE\WOW6432Node\Activision"
        };

            foreach (var key in registryKeys)
            {
                DeleteRegistryKey(key);
            }

            RunCleanup();

            ConsoleDelay(3000);
            Console.Clear();

            // Spoof PC Name
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [!] Spoofing PC name");
            SpoofPCName();
            ConsoleDelay(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [+] PC name spoofed");
            ConsoleDelay();

            // Spoof Installation ID
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [!] Spoofing Installation ID");
            SpoofInstallationID();
            ConsoleDelay(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [+] Installation ID spoofed");
            ConsoleDelay();

            // Spoof Product ID
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [!] Spoofing Product ID");
            SpoofProductID();
            ConsoleDelay();

            // Spoof disks
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [!] Spoofing Disks");
            SpoofDisks();
            ConsoleDelay(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [+] Disks spoofed");
            ConsoleDelay();

            // Spoof Guid
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [!] Spoofing GUIDs");
            SpoofGUIDs();
            ConsoleDelay(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [+] GUIDs spoofed");
            ConsoleDelay();

            // Spoof GPU
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [!] Spoofing GPU");
            SpoofGPU();
            ConsoleDelay(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [+] GPU spoofed");
            ConsoleDelay();
            
            // Spoof Display
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [!] Spoofing Display");
            SpoofDisplay();
            ConsoleDelay();

            // Spoof EFI
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [!] Spoofing EFI Variable ID");
            SpoofEFIVariableId();
            ConsoleDelay(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [+] EFI Variable ID spoofed");
            ConsoleDelay();

            // Spoof smbios
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [!] Spoofing smBIOS Serial Number");
            SpoofSMBIOSSerialNumber();
            ConsoleDelay(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [+] smBIOS Serial Number spoofed");
            ConsoleDelay();

            // Spoof MAC address
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  [!] Spoofing MAC address");
            SpoofMAC();
            ConsoleDelay(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [+] MAC address spoofed");
            ConsoleDelay();

            Console.Clear();

            // get system data
            DisplaySystemData();
            ConsoleDelay(3000);

            FlushDnsCache();
            ClearWindowsLogs();


            ClearTemporaryCache();
            ClearWindowsTempLol();
            TcpRst();
            //DocsClear();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  [#] Press Enter to end the spoofer...");
            Console.ReadLine();

            Environment.Exit(0);
        }
    }
}
