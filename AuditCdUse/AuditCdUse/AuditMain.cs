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
        MySqlConn con;
        ManualResetEvent mre;
        static bool should_exit = false;
        static StreamWriter file;

        static void Main(string[] args)
        {
            //add an event handler for when a user shutdown down or logs off
            SystemEvents.SessionEnding += Detect_Logout;

            AuditMain am = new AuditMain();
            /*ManagementEventWatcher w = null;
            WqlEventQuery q;
            ManagementOperationObserver observer = new ManagementOperationObserver();

            // Bind to local machine
            ConnectionOptions opt = new ConnectionOptions();
            opt.EnablePrivileges = true; //sets required privilege
            ManagementScope scope = new ManagementScope("root\\CIMV2", opt);

            q = new WqlEventQuery();
            q.EventClassName = "__InstanceModificationEvent";
            q.WithinInterval = new TimeSpan(0, 0, 1);

            // DriveType - 5: CDROM
            q.Condition = @"TargetInstance ISA 'Win32_LogicalDisk' and
            TargetInstance.DriveType = 5";
            w = new ManagementEventWatcher(scope, q);

            // register async. event handler
            w.EventArrived += new EventArrivedEventHandler(am.CDREventArrived);
            //it was hanging up with start outside this loop and stop after it, I'll do more experimenting
            //but currently, this works, and doesn't do anything crazy resource wise

            file.WriteLine(DateTime.Now.ToString() + " Before the while");
            while (should_exit != 1)
            {
                w.Start();
                w.WaitForNextEvent();
                w.Stop();
            }*/
            string lastTitle = "";
            while (should_exit == false)
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType==DriveType.CDRom))
                {
                    if(drive.IsReady)
                    {
                        if(drive.VolumeLabel != lastTitle)
                        {
                            lastTitle = drive.VolumeLabel;
                            //need to raise an event here and make ReportCd consume it
                        }
                    }
                    else
                    {
                        Debug.WriteLine("drive not ready");
                    }
                }
                Thread.Sleep(5000);
            }
            
            return;
        }

        public void CDREventArrived(object sender, EventArrivedEventArgs e)
        {
            // Get the Event object and display it
            PropertyData pd = e.NewEvent.Properties["TargetInstance"];

            if (pd != null)
            {
                ManagementBaseObject mbo = pd.Value as ManagementBaseObject;

                // if CD removed VolumeName == null
                if (mbo.Properties["VolumeName"].Value != null)
                {
                    file.WriteLine(DateTime.Now.ToString() + " CD Insert Detected");
                    //Console.WriteLine("CD has been inserted");
                    string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    string machineName = Environment.MachineName;
                    int sent = con.InsertCdEvent(machineName, userName);
                    if(sent == 1)
                    {
                        //sent
                        file.WriteLine(DateTime.Now.ToString() + " event sent");
                    }
                    else
                    {
                        //not sent
                        file.WriteLine(DateTime.Now.ToString() + " event NOT sent");
                    }
                }
                else
                {
                    //Debug.WriteLine("CD has been ejected");
                }
            }
        }

        public void ReportCd()
        {
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string machineName = Environment.MachineName;
            int sent = con.InsertCdEvent(machineName, userName);
        }

        public AuditMain()
        {
            con = new MySqlConn();
            mre = new ManualResetEvent(false);
            file = new StreamWriter(@"C:\uits\auditCdLog.txt");
            file.AutoFlush = true;
            file.WriteLine(DateTime.Now.ToString() + " AuditMain started");
        }

        private static void Detect_Logout(object sender, EventArgs e)
        {
            should_exit = true;
        }

    }
}
