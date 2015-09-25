﻿using MySql.Data.MySqlClient;
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

/** Authors: Govindu Samarasinghe, Rodel Rojos
 *  Date: 2015
 * 
 *  Project: The Big Data Speech Processing Platform
 *  Project proposed by the ECE department of The University of Auckland
 */

namespace P4PSpeechDB
{
    /*Loads data from DB into the ViewModel collections , used to update datagrids. */
    public class DataGridLoader
    {
        private ObservableCollection<SpeakerViewModel> speakers = new ObservableCollection<SpeakerViewModel>(); //DAtagrid row item
        private ObservableCollection<ProjectViewModel> projects = new ObservableCollection<ProjectViewModel>(); //DAtagrid row item
        private ObservableCollection<AnalysisViewModel> analysis = new ObservableCollection<AnalysisViewModel>(); //DAtagrid row item

        public List<string> ignoreTables = new List<string>();

        public DataGridLoader()
        {
            ignoreTables.Add("projects");
            ignoreTables.Add("analysis");
            ignoreTables.Add("files2analysis");
            ignoreTables.Add("trackOptions");
        }

        #region GettingCollections
        public ObservableCollection<ProjectViewModel> getProjects()
        {
            return loadProjects();
        }

        public ObservableCollection<SpeakerViewModel> getSpeakers(string PID)
        {
            return loadSpeakers(PID);
        }

        public ObservableCollection<AnalysisViewModel> getAnalysis(string ID)
        {
            return loadAnalysis(ID);
        }
        #endregion
        /*Loads each type of collection from the database into the ViewModel collection, using the
         * params specified. */
        #region Loaders
        public ObservableCollection<ProjectViewModel> loadProjects()
        {


            projects.Clear();
            using (DBConnection db = new DBConnection())
            {
                MySqlCommand query = new MySqlCommand("SELECT PID, DateCreated, Description FROM Project");

                var table = db.getFromDB(query);
                foreach (DataRow dr in table.Rows)
                {

                    string[] dateOnly = dr["dateCreated"].ToString().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    projects.Add(new ProjectViewModel { Project = new Project { PID = dr["PID"].ToString(), DateCreated = dateOnly[0], Description = dr["Description"].ToString() } });
                }
            }

            return this.projects;


        }

        public ObservableCollection<AnalysisViewModel> loadAnalysis(string fileName)
        {
            analysis.Clear();
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
                    analysis.Add(new AnalysisViewModel { Analysis = new Analysis { AID = dr["AID"].ToString(), Description = dr["Description"].ToString(), FileType = dr["FileType"].ToString() } });
                }
            }
            return this.analysis;

        }

        public ObservableCollection<SpeakerViewModel> loadSpeakers(string PID)
        {

            //if (PID == null)
            //{
            //    PID = "DefaultProject";
            //}
            speakers.Clear();
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
                    speakers.Add(new SpeakerViewModel { Speaker = new Speaker { ID = dr["FID"].ToString(), Name = dr["Name"].ToString(), PID = dr["PID"].ToString(), SpeakerName = speaker, FileType = dr["FileType"].ToString(), Age = speaker[0].ToString() } });

                }

            }

            return this.speakers;

        }

        /*Gets the speakers relating to the selected analysis*/
        public ObservableCollection<SpeakerViewModel> GetLinkedSpeaker(string AID)
        {

            //if (PID == null)
            //{
            //    PID = "DefaultProject";
            //}
            speakers.Clear();
            using (DBConnection db = new DBConnection())
            {
                MySqlCommand cmd = new MySqlCommand();



                cmd.CommandText = @"SELECT * FROM File f 
                                                INNER JOIN File2Analysis f2a ON f.FID = f2a.File_FID
                                                INNER JOIN Analysis a ON a.AID = f2a.Analysis_AID
                                                WHERE a.AID = @AID";
                cmd.Parameters.AddWithValue("@AID", AID);

                var table = db.getFromDB(cmd);
                foreach (DataRow dr in table.Rows)
                {
                    var speaker = dr["Speaker"].ToString();
                    speakers.Add(new SpeakerViewModel { Speaker = new Speaker { ID = dr["FID"].ToString(), Name = dr["Name"].ToString(), PID = dr["PID"].ToString(), SpeakerName = speaker, FileType = dr["FileType"].ToString(), Age = speaker[0].ToString() } });

                }

            }

            return this.speakers;

        }

        #endregion


    }
}
