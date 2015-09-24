using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace P4PSpeechDB
{
    class MoaCore
    {

        private DataGridLoader dgl;
        private MainWindowViewModel mainWindowVM;
        public MoaCore(MainWindowViewModel mainWindowVM, DataGridLoader dgl)
        {
            this.dgl = dgl;
            this.mainWindowVM = mainWindowVM;
        }

        public void AddFiles()
        {
            // Open file system to select file(s)
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;

            // Set filter for file extension and default file extension 
            //dlg.DefaultExt = ".png";
            //dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


            Nullable<bool> result = dlg.ShowDialog();  // Display OpenFileDialog by calling ShowDialog method 
            List<string> folderDetails = new List<string>();
            if (result.HasValue == true && result.Value == true)
            {
                folderDetails = getFolderName(); // only prompt for folder once always
            }
            byte[] rawData;

            // Add all files selected into the the db. If multiple files added, project destination is the same.
            foreach (String file in dlg.FileNames)
            {
                // Get the selected file name and display in a TextBox 
                if (result.HasValue == true && result.Value == true)
                {
                    rawData = File.ReadAllBytes(file);
                    // Open document 
                    string filename = file;
                    string ext = Path.GetExtension(file);

                    //Stores file in appropriate place in file system
                    //moveFile(filename, databaseRoot  /* + WHATEVER THE NEW LOCATION IS ASK CATH */);

                    executeInsert(filename, ext, dlg, folderDetails, rawData);


                }
            }
        }

        private List<string> getFolderName()
        {
            return FolderMsgPrompt.Prompt("Create new folder", "Folder options", inputType: FolderMsgPrompt.InputType.Text);

        }

        private void executeInsert(String filename, String ext, Microsoft.Win32.OpenFileDialog dlg, List<string> folderDetails, byte[] rawData)
        {
            var path = Path.GetExtension(filename);
            filename = Path.GetFileName(filename);
            string speaker = Path.GetFileNameWithoutExtension(dlg.SafeFileName).Substring(0, 4);

            using (DBConnection db = new DBConnection())
            {



                MySqlCommand comm = new MySqlCommand();
                comm.CommandText = "INSERT INTO File (PID, Name, FileType, Speaker) VALUES(@PID, @Name, @Type, @Speaker)";
                comm.Parameters.AddWithValue("@Name", filename);
                comm.Parameters.AddWithValue("@Type", path);
                comm.Parameters.AddWithValue("@Speaker", speaker);
                comm.Parameters.AddWithValue("@PID", folderDetails.First());
                db.insertIntoDB(comm);

                comm = new MySqlCommand();

                comm.CommandText = "INSERT INTO FileData (FID, FileData) VALUES (LAST_INSERT_ID(), @FileData)";
                comm.Parameters.AddWithValue("@FileData", rawData);
                db.insertIntoDB(comm);

                if (folderDetails != null)
                {

                    comm = new MySqlCommand();
                    comm.CommandText = "INSERT IGNORE INTO Project (PID, DateCreated, Description) VALUES(@PID, @dateCreated, @description)";
                    comm.Parameters.AddWithValue("@PID", folderDetails.First());
                    comm.Parameters.AddWithValue("@dateCreated", DateTime.Now.ToString());
                    if (folderDetails.Count == 2)
                    {
                        comm.Parameters.AddWithValue("@description", folderDetails.Last());
                    }
                    else
                    {
                        comm.Parameters.AddWithValue("@description", "No description given");

                    }
                    db.insertIntoDB(comm);
                }

            }


        }

        public void AddAnalysis()
        {

            ObservableCollection<SpeakerViewModel> speakers = null;
            // Open file system to select file(s)
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;


            Nullable<bool> result = dlg.ShowDialog();  // Display OpenFileDialog by calling ShowDialog method 
            byte[] rawData;
            List<Tuple<string, byte[]>> dataList = new List<Tuple<string, byte[]>>();
            string ext = "";
            string filename = "";

            // Add all files selected into the the db. If multiple files added, project destination is the same.
            foreach (String file in dlg.FileNames)
            {
                // Get the selected file name and display in a TextBox 
                if (result.HasValue == true && result.Value == true)
                {
                    rawData = File.ReadAllBytes(file);
                    dataList.Add(Tuple.Create(file, rawData));
                    ext = Path.GetExtension(file);
                    filename = file;
                }
            }

            AnalysisMsgPrompt a = new AnalysisMsgPrompt(dgl, dgl.getProjects());
            if (a.ShowDialog() == true)
            {
                speakers = dgl.getSpeakers(a.PID);
                foreach (var dataItem in dataList)
                {
                    var comm = new MySqlCommand();
                    filename = Path.GetFileName(dataItem.Item1);

                    using (DBConnection db = new DBConnection())
                    {

                        comm.CommandText = "INSERT INTO Analysis (AID, Description, FileData, FileType) VALUES(@AID, @Desc, @FileAsBlob, @FileType)";
                        comm.Parameters.AddWithValue("@AID", filename);
                        if (a.Desc.Equals(""))
                        {
                            comm.Parameters.AddWithValue("@Desc", "No description");
                        }
                        else
                        {

                            comm.CommandText = "INSERT INTO analysis (AID, File, Description) VALUES(@AID, @FileAsBlob, @Desc)";
                            comm.Parameters.AddWithValue("@AID", dataItem.Item1);
                            comm.Parameters.AddWithValue("@FileAsBlob", dataItem.Item2);
                            if (a.Desc.Equals(""))
                            {
                                comm.Parameters.AddWithValue("@Desc", "No description");
                            }
                            else
                            {

                                comm.Parameters.AddWithValue("@Desc", a.Desc);
                            }
                            comm.ExecuteNonQuery();

                            //Add to the mapping table(to link with speaker)
                            //var startsWithAge = mainWindowVM.Speakers.Where(s => ((SpeakerViewModel)s).SpeakerName.StartsWith(a.Age)).ToList();

                            foreach (var row in speakers)
                            {

                                if (((SpeakerViewModel)row).SpeakerName.StartsWith(a.Age))
                                {

                                    db.insertIntoDB(comm);
                                }


                                comm.CommandText = "INSERT IGNORE INTO files2analysis (ID, AID) VALUES (@ID2, @AID2)";
                                comm.Parameters.Clear();
                                comm.Parameters.AddWithValue("@ID2", ((SpeakerViewModel)row).ID);
                                comm.Parameters.AddWithValue("@AID2", dataItem.Item1);
                                comm.ExecuteNonQuery();
                            }


                            HashSet<Tuple<String, String>> uniqueAnalysis = new HashSet<Tuple<String, String>>();
                            HashSet<Tuple<String, String>> uniqueRowName = new HashSet<Tuple<String, String>>();
                            string previous = "";
                            foreach (var row in speakers)
                            {
                                if (!((SpeakerViewModel)row).Name.Equals(previous))
                                {
                                    previous = ((SpeakerViewModel)row).Name;
                                    uniqueAnalysis.Add(Tuple.Create(((SpeakerViewModel)row).Name, ((SpeakerViewModel)row).ID));
                                }
                            }
                            foreach (var uRow in uniqueAnalysis)
                            {
                                if ((uRow.Item1.StartsWith(a.Age)))
                                {
                                    comm.CommandText = "INSERT IGNORE INTO File2Analysis (File_FID, Analysis_AID) VALUES (@FileID, @AID)";
                                    comm.Parameters.Clear();
                                    comm.Parameters.AddWithValue("@FileID", uRow.Item2);
                                    comm.Parameters.AddWithValue("@AID", filename);
                                    db.insertIntoDB(comm);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
