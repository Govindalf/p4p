using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace P4PSpeechDB
{
    public class DBConnection
    {

        MySqlConnectionStringBuilder csBuilder;

        MySqlConnection conn;

        public DBConnection()
        {

            try
            {

                //Conection string for aws
                //string SQLConnectionString = "Data Source=speech-db.cf7gfoeoefop.ap-southeast-2.rds.amazonaws.com;Database=SpeechDB;User Id=p4p;password=Cirilla_2015;port=3306;charset=utf8";
                csBuilder = new MySqlConnectionStringBuilder();
                csBuilder.Server = "speech-db.cf7gfoeoefop.ap-southeast-2.rds.amazonaws.com";
                csBuilder.Database = "SpeechDB";
                csBuilder.UserID = "p4p";
                csBuilder.Password = "Cirilla_2015";
                csBuilder.Port = 3306;
                csBuilder.CharacterSet = "utf8";
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
                if (conn.State != ConnectionState.Open)
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

        public void handleException(SqlException e)
        {
            switch (e.Number)
            {
                case 1586:

                    MessageBox.Show("A file with the same name exists. Insert failed.");
                    break;
            }
        }
    }
}
