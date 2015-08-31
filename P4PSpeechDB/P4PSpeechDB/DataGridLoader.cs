using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace P4PSpeechDB
{
    class DataGridLoader
    {
        private DBConnection conn;
        ICollectionView collectionP; //projects
        ICollectionView collectionS; //speakers
        ICollectionView collectionA; //analysis
        private List<String> tableNames;
        private ObservableCollection<Row> rowA; //DAtagrid row item
        private ObservableCollection<Row> rowS; //DAtagrid row item
        private ObservableCollection<Row> rowP = new ObservableCollection<Row>(); //DAtagrid row item

        public DataGridLoader(DBConnection conn, List<String> tableNames)
        {
            this.conn = conn;
            this.tableNames = tableNames;
        }

        public void setUpDataGrids()
        {
            loadProjects();
            loadSpeakers(null);
        }

        public ObservableCollection<Row> getCollection(string type)
        {
            switch (type)
            {
                case "P":
                    return this.rowP;
                case "S":
                    return this.rowS;
                case "A":
                    return this.rowA;
                default:
                    throw new Exception("Invalid type value");
            }
            
        }

        private void loadProjects()
        {
            if (conn.openConn() == true)
            {

                MySqlDataReader myReader;
                MySqlCommand cmd = new MySqlCommand("Select PID from projects", conn.getConn());

                myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    rowP.Add(new ProjectRow { PID = myReader.GetString("PID") });
                }
                myReader.Close();
                collectionP = new ListCollectionView(rowP);

                conn.closeConn();
            }
        }

        public void loadSpeakers(string PID)
        {
            if (conn.openConn() == true)
            {
                if (PID == null)
                {
                    PID = "DefaultProject";
                }

                MySqlDataReader myReader;
                rowS = new ObservableCollection<Row>();
                try
                {

                    //Get number of tables in database, for all tables, do the following
                    DataSet ds = new DataSet();
                    foreach (string name in tableNames)
                    {
                        //Exclude the projects table
                        if (name.Equals("projects") || name.Equals("analysis"))
                        {
                            continue;
                        }

                        MySqlCommand cmd = new MySqlCommand();
                        cmd.Connection = conn.getConn();
                        cmd.CommandText = "SELECT ID, ProjectName, Speaker FROM " + name + " WHERE ProjectName = @pName"; // @name" ; // WHERE ProjectName = '" + PID + "'";
                        //cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@pName", PID);

                        myReader = cmd.ExecuteReader();
                        while (myReader.Read())
                        {
                            string projectName = "default";
                            if (myReader.GetValue(1).ToString() != "")
                            {
                                projectName = myReader.GetValue(1).ToString();
                            }

                            rowS.Add(new SpeakerRow { ID = myReader.GetString("ID"), ProjectName = projectName, Speaker = myReader.GetString("Speaker"), tableName = name });

                        }
                        myReader.Close();

                    }

                    //Pass in the collection made of the datagrid rows
                    collectionS = new ListCollectionView(rowS);
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            conn.closeConn();
        }



    }
}
