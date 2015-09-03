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

        public List<string> ignoreTables = new List<string>();

        public DataGridLoader(DBConnection conn, List<String> tableNames)
        {
            this.conn = conn;
            this.tableNames = tableNames;

            ignoreTables.Add("projects");
            ignoreTables.Add("analysis");
            ignoreTables.Add("files2analysis");
            ignoreTables.Add("trackOptions");
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

            using (MySqlConnection conn = new DBConnection().getConn())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();


                MySqlDataReader myReader;
                cmd.CommandText = "Select PID from projects";

                using (myReader = cmd.ExecuteReader())
                {
                    while (myReader.Read())
                    {
                        rowP.Add(new ProjectRow { PID = myReader.GetString("PID") });
                    }
                }
                collectionP = new ListCollectionView(rowP);

            }
        }

        public void loadAnalysis(string fileID)
        {

            using (MySqlConnection conn = new DBConnection().getConn())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();

                MySqlDataReader myReader;
                rowA = new ObservableCollection<Row>();
                cmd.CommandText = @"SELECT a.AID, a.Description FROM analysis a INNER JOIN files2analysis f2a
                                                      ON a.AID = f2a.AID WHERE f2a.ID = @ID";

                cmd.Parameters.AddWithValue("@ID", fileID);

                using (myReader = cmd.ExecuteReader())
                {
                    while (myReader.Read())
                    {
                        rowA.Add(new AnalysisRow { AID = myReader.GetString("AID"), Description = myReader.GetString("Description") });

                    }
                }
                collectionA = new ListCollectionView(rowA);

            }
        }

        public void loadSpeakers(string PID)
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
                    if (ignoreTables.Contains(name))
                    {
                        continue;
                    }

                    using (MySqlConnection conn = new DBConnection().getConn())
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        //Exclude the projects table
                        if (name.Equals("projects") || name.Equals("analysis") || name.Equals("files2analysis") || name.Equals("trackOptions"))
                        {
                            continue;
                        }

                        cmd.CommandText = "SELECT ID, ProjectName, Speaker FROM " + name + " WHERE ProjectName = @pName"; // @name" ; // WHERE ProjectName = '" + PID + "'";
                        //cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@pName", PID);

                        using (myReader = cmd.ExecuteReader())
                        {
                            while (myReader.Read())
                            {
                                string projectName = "default";
                                if (myReader.GetValue(1).ToString() != "")
                                {
                                    projectName = myReader.GetValue(1).ToString();
                                }
                                rowS.Add(new SpeakerRow { ID = myReader.GetString("ID"), ProjectName = projectName, Speaker = myReader.GetString("Speaker"), tableName = name, Age = (myReader.GetString("Speaker"))[0].ToString() });

                            }
                        }
                    }
                }

                //Pass in the collection made of the datagrid rows
                collectionS = new ListCollectionView(rowS);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }



    }
}
