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
    public class DataGridLoader
    {
        private DBConnection conn;
        ICollectionView collectionP; //projects
        ICollectionView collectionS; //speakers
        ICollectionView collectionA; //analysis
        private List<String> tableNames;
        private ObservableCollection<Row> rowA = new ObservableCollection<Row>(); //DAtagrid row item
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

        public void loadProjects()
        {


            rowP.Clear();
            using (DBConnection db = new DBConnection())
            {
                MySqlCommand query = new MySqlCommand("SELECT PID, DateCreated, Description FROM Project");

                var table = db.getFromDB(query);
                foreach (DataRow dr in table.Rows)
                {

                    string[] dateOnly = dr["dateCreated"].ToString().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    rowP.Add(new ProjectRow { PID = dr["PID"].ToString(), DateCreated = dateOnly[0], Description = dr["Description"].ToString() });
                }
            }
            collectionP = new ListCollectionView(rowP);
        }

        public void loadAnalysis(string fileName)
        {
            rowA.Clear();
            using (DBConnection db = new DBConnection())
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = @"SELECT a.AID, a.Description, a.FileType FROM Analysis a 
                                                INNER JOIN File2Analysis f2a ON a.AID = f2a.Analysis_AID
                                                INNER JOIN File f ON f.FID = f2a.File_FID
                                                WHERE f.Name = @ID";

                cmd.Parameters.AddWithValue("@ID", fileName);

                var table = db.getFromDB(cmd);
                foreach (DataRow dr in table.Rows)
                {
                    rowA.Add(new AnalysisRow { AID = dr["AID"].ToString(), Description = dr["Description"].ToString(), FileType = dr["FileType"].ToString() });
                }
            }

            collectionA = new ListCollectionView(rowA);



        }

        public void loadSpeakers(string PID)
        {
            
            rowS = new ObservableCollection<Row>();
            //if (PID == null)
            //{
            //    PID = "DefaultProject";
            //}
            
            using (DBConnection db = new DBConnection())
            {
                MySqlCommand cmd = new MySqlCommand();



                cmd.CommandText = "SELECT * FROM File WHERE PID = @pName"; // @name" ; // WHERE ProjectName = '" + PID + "'";
                //cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@pName", PID);

                var table = db.getFromDB(cmd);
                foreach (DataRow dr in table.Rows)
                {
                    var speaker = dr["Speaker"].ToString();
                    rowS.Add(new SpeakerRow { ID = dr["FID"].ToString(), Name = dr["Name"].ToString(), PID = dr["PID"].ToString(), Speaker = speaker, FileType = dr["FileType"].ToString(), Age = speaker[0].ToString() });

                }

            }



            //Pass in the collection made of the datagrid rows
            collectionS = new ListCollectionView(rowS);

        }



    }
}
