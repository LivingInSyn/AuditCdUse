using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditCdInstaller
{
    class Install
    {
        static void Main(string[] args)
        {
            string dir32 = @"C:\Program Files\AuditCdUse";
            string dir64 = @"C:\Program Files (x86)\AuditCdUse";

            //delete the old files
            if (Directory.Exists(dir64))
            {
                deleteFiles(dir64);
                copyNewFiles(dir64);
                makeRegEntries(dir64);
            }
            else if (Directory.Exists(dir32))
            {
                deleteFiles(dir32);
                copyNewFiles(dir32);
                makeRegEntries(dir32);
            }
            else if (Directory.Exists(@"C:\Program Files (x86)"))
            {
                Directory.CreateDirectory(dir64);
                copyNewFiles(dir64);
                makeRegEntries(dir64);
            }
            else
            {
                Directory.CreateDirectory(dir32);
                copyNewFiles(dir32);
                makeRegEntries(dir32);
            }

            //debug to know it's done
            //Console.ReadLine();
            DateTime now = DateTime.Now;
            string timestring = now.ToShortTimeString();
            Console.WriteLine("Finished at " + timestring);
        }

        private static void copyNewFiles(string filePath)
        {
            filePath = filePath + @"\";

            //File.Copy("CdAuditFiles\\AuditCdUse.exe", filePath + "AuditCdUse.exe");
            //File.Copy("CdAuditFiles\\Uninstall.exe", filePath + "Uninstall.exe");
            //File.Copy("CdAuditFiles\\Uninstall.exe", filePath + "AuditCdUse.pdb");

            //aparently, file.copy will only do up to 10kb. Which is....bullshit...
            //so, programatically doing each file...
            string[] fileEntries = Directory.GetFiles("CdAuditFiles");
            foreach (string file in fileEntries)
            {
                using (FileStream sourceStream = new FileStream(file, FileMode.Open))
                {
                    byte[] buffer = new byte[64 * 1024];
                    string filename = file.Substring(file.LastIndexOf('\\') + 1);
                    using (FileStream destStream = new FileStream(filePath+filename, FileMode.Create))
                    {
                        int i;
                        while ((i = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            destStream.Write(buffer, 0, i);
                        }
                    }
                }
            }


        }

        private static void makeRegEntries(string dir)
        {
            RegistryKey startupkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            RegistryKey uninstallkey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);

            string runDir = "\"" + dir + @"\AuditCdUse.exe" + "\"";
            string uninstallString = "\"" + dir + @"\uninstall.exe" + "\"";

            //it doesn't exist
            startupkey.SetValue("CD Auditor", runDir);
            startupkey.Close();

            RegistryKey uninstallSubKey = uninstallkey.CreateSubKey("CD_Auditor");
            if (uninstallSubKey != null)
            {
                uninstallSubKey.SetValue("DisplayName", "CD Usage Auditor");
                uninstallSubKey.SetValue("UninstallString", uninstallString);
            }

        }

        //need to kill the process before we can delete files in that directory
        private static void deleteFiles(string filePath)
        {
            try
            {
                Process[] proc = Process.GetProcessesByName("AuditCdUse");
                for (int i = 0; i < proc.Length; i++)
                {
                    proc[i].Kill();
                }
                System.Threading.Thread.Sleep(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine("process not running, continuing");
            }


            string[] fileEntries = Directory.GetFiles(filePath);
            foreach (string file in fileEntries)
            {
                //Console.WriteLine(file);
                File.Delete(file);
            }
        }
    }
}
