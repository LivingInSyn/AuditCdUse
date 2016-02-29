using System;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace AuditCdUse
{
    class AuditMain
    {
        static bool should_exit = false;

        static void Main(string[] args)
        {
            //add an event handler for when a user shutdown down or logs off
            SystemEvents.SessionEnding += Detect_Logout;
            MySqlConn connection = new MySqlConn();

            AuditMain am = new AuditMain();
            
            string lastTitle = "";
            bool firstRun = true;
            Dictionary<string, string> cdTitles = new Dictionary<string, string>();
            while (should_exit == false)
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType==DriveType.CDRom))
                {
                    if(firstRun == true) { cdTitles[drive.Name] = ""; }
                    if (drive.IsReady)
                    {
                        if (drive.VolumeLabel != cdTitles[drive.Name])
                        {
                            cdTitles[drive.Name] = drive.VolumeLabel;
                            lastTitle = drive.VolumeLabel;
                            string userName = System.Environment.UserName;
                            string machineName = Environment.MachineName;
                            connection.InsertCdEvent(machineName, userName, lastTitle);
                        }
                        else
                        {
                            Debug.WriteLine("no change to CD");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("drive not ready");
                    }
                }
                firstRun = false;
                Thread.Sleep(5000);
            }
            
            return;
        }
        
        private static void Detect_Logout(object sender, EventArgs e)
        {
            should_exit = true;
        }

    }
}
