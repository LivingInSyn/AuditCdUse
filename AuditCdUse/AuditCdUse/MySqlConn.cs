using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace AuditCdUse
{
    class MySqlConn
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        private void Init()
        {
            server = "CDDB.grove.ad.uconn.edu";
            database = "cddb";
            uid = "cddbUser";
            password = "uwmbtlpup4ldHt7nzA76";
            string connectionString;

            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                                database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);

        }

        public MySqlConn()
        {
            Init();
        }

        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        System.Diagnostics.Debug.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        System.Diagnostics.Debug.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public int InsertCdEvent(string machineName, string userName)
        {
            DateTime dateValue = DateTime.Now;
            string sqlDateString = dateValue.ToString("yyyy-MM-dd HH:mm:ss");
            string insert = String.Format("INSERT INTO cddb.cd_events(MachineName, EventDateTime, UserName) VALUES('{0}','{1}','{2}')", machineName, sqlDateString, userName);
            if(this.OpenConnection() == true)
            { 
                MySqlCommand cmd = new MySqlCommand(insert, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
                return 1;
            }
            else
            {
                return 0;
            }
        }

    }
}
