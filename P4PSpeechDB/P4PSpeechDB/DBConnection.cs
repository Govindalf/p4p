using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

/** Authors: Govindu Samarasinghe, Rodel Rojos
 *  Date: 2015
 * 
 *  Project: The Big Data Speech Processing Platform
 *  Project proposed by the ECE department of The University of Auckland
 */
namespace P4PSpeechDB
{
    /* Class that manages the database connections. Connection pool is managed by
     * compiler, and released automatically when not needed. */
    public class DBConnection : IDisposable
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
                csBuilder.ConvertZeroDateTime = true;
                this.conn = new MySqlConnection(csBuilder.ToString());

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        /*Creates database using aql script. */
        public void createDB()
        {
            MySqlScript script = new MySqlScript(conn, File.ReadAllText(@"C:\Users\Govindu\Dropbox\P4P\p4p\schema.sql"));
            script.Execute();
        }

        /*Opens MySQL connection. */
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

        public void Dispose()
        {
            closeConn();
        }

        /*Closes MySQL connection. */
        public void closeConn()
        {
            try
            {
                if (this.conn != null && this.conn.State == ConnectionState.Open)
                {
                    this.conn.Close();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /*Queries database using query, and gets a DataTable representing results of the query. */
        public DataTable getFromDB(MySqlCommand query)
        {
            query.Connection = conn;
            var dataSet = new DataSet();
            var mySQLDataAdapter = new MySqlDataAdapter(query);

            dataSet = new DataSet();
            try
            {
                mySQLDataAdapter.Fill(dataSet);
            }
            catch (Exception e)
            {
                MessageBox.Show("Connection error." + e.Message);
            }

            return dataSet.Tables[0];
        }

        /*Executes query in cmd, usually adding or deleting. */
        public Boolean insertIntoDB(MySqlCommand cmd)
        {
            openConn();
            cmd.Connection = conn;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
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
