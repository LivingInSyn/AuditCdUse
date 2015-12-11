using System;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditCdUse
{
    class AuditMain
    {
        MySqlConn con;

        static void Main(string[] args)
        {
            AuditMain am = new AuditMain();
            ManagementEventWatcher w = null;
            WqlEventQuery q;
            ManagementOperationObserver observer = new
                ManagementOperationObserver();

            // Bind to local machine
            ConnectionOptions opt = new ConnectionOptions();
            opt.EnablePrivileges = true; //sets required privilege
            ManagementScope scope = new ManagementScope("root\\CIMV2", opt);

            try
            {
                q = new WqlEventQuery();
                q.EventClassName = "__InstanceModificationEvent";
                q.WithinInterval = new TimeSpan(0, 0, 1);

                // DriveType - 5: CDROM
                q.Condition = @"TargetInstance ISA 'Win32_LogicalDisk' and
            TargetInstance.DriveType = 5";
                w = new ManagementEventWatcher(scope, q);

                // register async. event handler
                w.EventArrived += new EventArrivedEventHandler(am.CDREventArrived);
                w.Start();

                // Do something usefull,block thread for testing
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                w.Stop();
            }
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
                    Console.WriteLine("CD has been inserted");
                    string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    string machineName = Environment.MachineName;
                    con.InsertCdEvent(machineName, userName);
                }
                else
                {
                    Console.WriteLine("CD has been ejected");
                }
            }
        }

        public AuditMain()
        {
            con = new MySqlConn();
        }

    }
}
