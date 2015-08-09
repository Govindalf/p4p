using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace P4PSpeechDB
{
    public class DBConnection
    {
        const string databaseRoot = "C:\\Users\\Govindu\\Dropbox\\P4P\\p4p\\P4Ptestfiles"; //Where the P4Ptestfiles folder is
        //const string databaseRoot = "C:\\Users\\Rodel\\Documents\\SE700A\\P4Ptestfiles"; //Where the P4Ptestfiles folder is

        //const string server = "localhost";
        //const string database = "p4pdatabase";
        //const string uid = "root";
        //const string password = "Cirilla_2015";
        //const string port = "3306";
        const string server = "tcp:gbtd4xmf5m.database.windows.net,1433";
        const string database = "p4pdatabase";
        const string uid = "p4pdatabase@gbtd4xmf5m";
        const string password = "Cirilla_2015";
        const string port = "3306";
        string connectionString;

        SqlConnectionStringBuilder csBuilder;


        MySqlConnection conn;

        public DBConnection()
        {
            try
            {
                csBuilder = new SqlConnectionStringBuilder();
                csBuilder.DataSource = "tcp:gbtd4xmf5m.database.windows.net,1433";
                csBuilder.InitialCatalog = "p4pdatabase";
                csBuilder.Encrypt = true;
                csBuilder.TrustServerCertificate = false;
                csBuilder.UserID = "p4pdatabase@gbtd4xmf5m";
                csBuilder.Password = "Cirilla_2015";

                //connectionString = "DATASOURCE = " + server + "; PORT= " + port + "; USERNAME = " + uid + "; PASSWORD = " + password + "; DATABASE = " + database + ";";
                conn = new MySqlConnection(csBuilder.ToString());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        public bool openConn()
        {
            try
            {
                conn.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server. Contact administrator");
                        break;
                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                    default:
                        MessageBox.Show(ex.Message);
                        break;
                }
                return false;
            }
        }

        public bool closeConn()
        {
            try
            {
                this.conn.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public MySqlCommand getCommand()
        {
            return this.conn.CreateCommand();
        }

        public MySqlConnection getConn()
        {
            return this.conn;
        }
    }
}
