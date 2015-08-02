using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace P4PSpeechDB
{
    public class DBConnection
    {
        const string databaseRoot = "C:\\Users\\Govindu\\Dropbox\\P4P\\p4p\\P4Ptestfiles"; //Where the P4Ptestfiles folder is
        const string server = "localhost";
        const string database = "p4pdatabase";
        const string uid = "root";
        const string password = "Cirilla_2015";
        const string port = "3306";
        string connectionString;


        MySqlConnection conn;

        public DBConnection()
        {
            connectionString = "SERVER=" + server + ";" + "PORT" + port + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            conn = new MySqlConnection(connectionString);
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
                conn.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /* Execute the query given in the input. */
        public void executeQuery(string query)
        {
            MySqlCommand comm = conn.CreateCommand();
            comm.CommandText = query;
            comm.ExecuteNonQuery();
        }
    }
}
