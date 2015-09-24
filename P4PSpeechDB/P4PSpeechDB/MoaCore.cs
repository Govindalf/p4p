using MicroMvvm;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
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
        private string rootFolder;
        public MoaCore(MainWindowViewModel mainWindowVM, DataGridLoader dgl)
        {

            rootFolder = P4PSpeechDB.Properties.Settings.Default.Path;
            this.dgl = dgl;
            this.mainWindowVM = mainWindowVM;
        }

        /*Adds a speech file to the File table on the db. */
        public void AddFiles()
        {
            // Open file system to select file(s)
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;

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

                    executeInsert(filename, ext, dlg, folderDetails, rawData);


                }
            }
        }

        /* Adds analysis file to the database, and links it correctly to a speaker Age group. */
        public void AddAnalysis()
        {
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

            //Prompt user for file
            AnalysisMsgPrompt a = new AnalysisMsgPrompt(dgl, dgl.getProjects());



            if (a.ShowDialog() == true)
            {
                var rowS = dgl.getSpeakers(a.PID);

                //Iterates over all the files selected earlier, and add and link them 1 by 1
                foreach (var dataItem in dataList)
                {
                    var comm = new MySqlCommand();
                    filename = Path.GetFileName(dataItem.Item1);

                    //Add the file and data to the Analysis table
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

                            comm.Parameters.AddWithValue("@Desc", a.Desc);
                        }
                        comm.Parameters.AddWithValue("@FileAsBlob", dataItem.Item2);
                        comm.Parameters.AddWithValue("@FileType", "." + ext);

                        db.insertIntoDB(comm);
                    }

                    //Add to the mapping table(to link with speaker)
                    //List<Row> startsWithAge = rowS.Where(s => ((SpeakerRow)s).Speaker.StartsWith(a.Age)).ToList();
                    HashSet<Tuple<String, String>> uniqueAnalysis = new HashSet<Tuple<String, String>>();
                    HashSet<Tuple<String, String>> uniqueRowName = new HashSet<Tuple<String, String>>();
                    string previous = "";
                    foreach (var row in rowS)
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
                            using (DBConnection db = new DBConnection())
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

        /*Returns a folder options prompt. */
        private List<string> getFolderName()
        {
            return FolderMsgPrompt.Prompt("Create new folder", "Folder options", inputType: FolderMsgPrompt.InputType.Text);

        }

        /* Inserts the specified file at filename with extension ext into the File table, with the file data rawData added to the FileData table. 
         * Can also add whole projects, depending on the folderDetails param. */
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


        /* Generates a template file for a project. */
        public void GenerateTemplate()
        {
            List<List<string>> listResults = GenerateTempPrompt.Prompt("Enter template name", "Generate template file", inputType: GenerateTempPrompt.InputType.Text);
            //Cancel clicked
            if (listResults == null)
            {
                return;
            }
            string pathName = rootFolder;
            string ext = "tpl";
            string tempName = listResults.First().First() + "." + ext;
            List<string> projN = new List<string>();
            if (listResults == null)
            {
                return;
            }

            pathName += "\\" + listResults.First().First() + "." + ext;
            try
            {

                // Create a new file 
                using (FileStream fs = File.Create(pathName))
                {
                    //Add some text to file
                    Byte[] heading = new UTF8Encoding(true).GetBytes("! this file was generated by Moa \n \n");
                    fs.Write(heading, 0, heading.Length);

                    int count = 0;
                    //The second list must be sfb tracks and sf0 third.
                    foreach (List<string> lStr in listResults)
                    {
                        string trackClassName = "";
                        // skip first result list which contain tpl name and project path
                        if (count == 0)
                        {
                            projN.Add(lStr[1]);
                            count += 1;
                            string pathFiles = rootFolder + "\\" + lStr[1] + "\\*";
                            byte[] pathFString = new UTF8Encoding(true).GetBytes("path lab " + pathFiles +
                                "\n" + "path trg " + pathFiles + "\n" + "path hlb " + pathFiles + "\n" + "path wav " + pathFiles + "\n" + "path sfb " + pathFiles + "\n \n");
                            fs.Write(pathFString, 0, pathFString.Length);

                            byte[] sampleWavString = new UTF8Encoding(true).GetBytes("track samples wav\n");
                            fs.Write(sampleWavString, 0, sampleWavString.Length);
                            continue;
                        }
                        if (lStr.LastOrDefault().Equals("sf0Id"))
                        {
                            trackClassName = "sf0";
                            count += 1;
                        }
                        else if (lStr.LastOrDefault().Equals("sfbId"))
                        {
                            trackClassName = "sfb";
                            count += 1;
                        }

                        else
                        {
                            trackClassName = lStr.Last();
                            count += 1;
                        }
                        foreach (string str in lStr)
                        {
                            if (str.Equals(lStr.Last()))
                            {
                                continue;
                            }
                            // make sure to fix hardcoded sfb
                            byte[] trackString = new UTF8Encoding(true).GetBytes("track " + str + " " + trackClassName + "\n");
                            fs.Write(trackString, 0, trackString.Length);
                        }
                    }

                    byte[] primaryExt = new UTF8Encoding(true).GetBytes("\nset PrimaryExtension wav \n");
                    fs.Write(primaryExt, 0, primaryExt.Length);
                }
                byte[] rawData = File.ReadAllBytes(pathName);
                executeInsert(pathName, ext, null, projN, rawData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void openOrPlayFile(ObservableObject obj)
        {
            if (obj is SpeakerViewModel)
            {
                openOrPlaySpeechFile(obj as SpeakerViewModel);
            }
            else if (obj is AnalysisViewModel)
            {
                openOrPlayAnalysisFile(obj as AnalysisViewModel);
            }
        }

        /*Downlaods and opens the speech file selected, or plays it if its a .wav(audio) */
        private void openOrPlaySpeechFile(SpeakerViewModel speaker)
        {
            var cmd = new MySqlCommand();
            cmd.CommandText = @"SELECT f.FileData FROM FileData f INNER JOIN File fi ON fi.FID = f.FID WHERE fi.Name= '" + speaker.Name + "' AND fi.FileType = @Type AND fi.PID = @PID";
            //cmd.Parameters.AddWithValue("@tName", tableName); //THIS DONT WORK. WHY? WHO KNOWS
            cmd.Parameters.AddWithValue("@Type", speaker.FileType);
            cmd.Parameters.AddWithValue("@PID", speaker.PID);

            using (DBConnection db = new DBConnection())
            {

                byte[] rawData;
                FileStream fs;
                string filePath = "";


                Directory.CreateDirectory(rootFolder + @"\" + speaker.PID + "\\" + speaker.SpeakerName);
                fs = new FileStream(rootFolder + @"\" + speaker.PID + "\\" + speaker.SpeakerName + "\\" + speaker.Name + speaker.FileType, FileMode.OpenOrCreate, FileAccess.Write);


                var table = db.getFromDB(cmd);
                foreach (DataRow dr in table.Rows)
                {

                    rawData = (byte[])dr["FileData"]; // convert successfully to byte[]


                    filePath = fs.Name;

                    //Fixed access denied error
                    File.SetAttributes(filePath, FileAttributes.Normal);

                    // Writes a block of bytes to this stream using data from
                    // a byte array.
                    fs.Write(rawData, 0, rawData.Length);

                    // close file stream
                    fs.Close();

                }


                // Filter audio, images etc. to open appropriate program
                if (speaker.FileType.Equals(".wav") || speaker.FileType.Equals(".WAV"))
                {
                    var audio = new AudioWindow(filePath);
                    audio.Show();

                }
                else
                {

                    try
                    {
                        //Process.Start("notepad++.exe", filePath);
                        Process.Start(filePath);
                    }
                    catch
                    {

                    }
                }
            }
        }

        /*Downlaods and opens the analysis file selected*/
        private void openOrPlayAnalysisFile(AnalysisViewModel analysis)
        {


            var cmd = new MySqlCommand();
            cmd.CommandText = "SELECT FileData FROM Analysis where AID = @fileName";
            cmd.Parameters.AddWithValue("@fileName", analysis.AID);


            using (DBConnection db = new DBConnection())
            {

                byte[] rawData;
                FileStream fs;
                string filePath = "";


                Directory.CreateDirectory(rootFolder + "\\ANALYSIS");
                fs = new FileStream(rootFolder + "\\ANALYSIS\\" + analysis.AID + analysis.FileType, FileMode.OpenOrCreate, FileAccess.Write);



                var table = db.getFromDB(cmd);
                foreach (DataRow dr in table.Rows)
                {

                    rawData = (byte[])dr["FileData"]; // convert successfully to byte[]


                    filePath = fs.Name;

                    //Fixed access denied error
                    File.SetAttributes(filePath, FileAttributes.Normal);

                    // Writes a block of bytes to this stream using data from
                    // a byte array.
                    fs.Write(rawData, 0, rawData.Length);

                    // close file stream
                    fs.Close();

                }


                // Filter audio, images etc. to open appropriate program
                if (analysis.FileType.Equals(".wav") || analysis.FileType.Equals(".WAV"))
                {
                    var audio = new AudioWindow(filePath);
                    audio.Show();

                }
                else
                {


                    //Process.Start("notepad++.exe", filePath);
                    Process.Start(filePath);

                }
            }
        }

    }
}
