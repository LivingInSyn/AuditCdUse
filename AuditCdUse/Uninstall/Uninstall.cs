using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uninstall
{
    class Uninstall
    {
        static void Main(string[] args)
        {
            string dir32 = @"C:\Program Files\AuditCdUse";
            string dir64 = @"C:\Program Files (x86)\AuditCdUse";

            try
            {
                Process[] proc = Process.GetProcessesByName("AuditCdUse");
                proc[0].Kill();
                System.Threading.Thread.Sleep(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine("process not running, continuing");
            }

            if (Directory.Exists(dir64))
            {
                deleteFiles(dir64);
                removeRegEntries();
            }
            else if (Directory.Exists(dir32))
            {
                deleteFiles(dir32);
                removeRegEntries();
            }
            else
            {
                Console.WriteLine("Neither install directory found!\nPress Enter to exit...");
                //Console.ReadLine();
                return;
            }
        }

        private static void deleteFiles(string filePath)
        {
            string[] fileEntries = Directory.GetFiles(filePath);
            foreach (string file in fileEntries)
            {
                //Console.WriteLine(file);
                if (file.EndsWith("Uninstall.exe"))
                {
                    //do something different because we can't delete ourselves yet
                }
                else
                {
                    File.Delete(file);
                }
            }
        }

        private static void removeRegEntries()
        {

            RegistryKey startupkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            RegistryKey uninstallkey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);

            RegistryKey uninstallSubKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\CD_Auditor", true);


            //delete the runtime value. Starup key should never be null, but yaknow...
            if (startupkey != null)
            {
                if (startupkey.GetValue("CD Auditor") != null)
                {
                    startupkey.DeleteValue("CD Auditor");
                }
            }
            else
            {
                Console.WriteLine("runkey not found!");
            }

            //delete the uninstall subkeys
            if (uninstallSubKey != null)
            {
                if (uninstallSubKey.GetValue("DisplayName") != null)
                {
                    uninstallSubKey.DeleteValue("DisplayName");
                }
                if (uninstallSubKey.GetValue("UninstallString") != null)
                {
                    uninstallSubKey.DeleteValue("UninstallString");
                }
            }
            else
            {
                Console.WriteLine("uninstall subkey not found!");
            }

            //now delete the uninstall key, we need to check uninstallSubKey as well to ensure that we don't throw an error
            if (uninstallkey != null && uninstallSubKey != null)
            {
                uninstallkey.DeleteSubKeyTree("CD_Auditor");
            }
            else
            {
                Console.WriteLine("uninstall key not found!");
            }



        }
    }
}
