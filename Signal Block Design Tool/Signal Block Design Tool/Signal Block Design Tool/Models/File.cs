﻿using MySql.Data.MySqlClient;
using Signal_Block_Design_Tool.Database;
using Signal_Block_Design_Tool.Forms;
using Signal_Block_Design_Tool.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace Signal_Block_Design_Tool.Files
{
    class File
    {
        /// <summary>
        /// 
        /// </summary>
        public static void ImportFromFile()
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Types of files to read
            openFileDialog.Filter = "csv file (*.csv)|*.csv";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog.OpenFile()) != null)
                    {
                        using (myStream)
                        {

                            if (openFileDialog.FilterIndex == 1 || openFileDialog.FilterIndex == 2)
                            {
                                Thread t = new Thread(LoadExcelFile);
                                t.IsBackground = true;
                                t.Start(openFileDialog.FileName);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    LogManager.Logger.Log(ex);
                }
            }
        }

        /// <summary>
        ///  
        /// </summary>
        public static void ClearDataBase()
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete the contents of the database?", "Are you sure...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    LoadFromDatabaseForm databaseForm = new LoadFromDatabaseForm();
                    Database.DatabaseConnection conn = new Database.DatabaseConnection(databaseForm.ServerNameBox.Text,
                    Convert.ToUInt32(databaseForm.PortBox.Text), databaseForm.DatabaseNameBox.Text,
                    databaseForm.UserNameBox.Text, databaseForm.PasswordBox.Text);
                    conn.openConnection();
                    Database.DatabaseOperations.ClearDatabase(conn);
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Log(ex);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public static void LoadFromDatabase()
        {
            LoadFromDatabaseForm databaseForm = new LoadFromDatabaseForm();
            if (databaseForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    // TODO: Set up the database
                    Database.DatabaseConnection conn = new Database.DatabaseConnection(databaseForm.ServerNameBox.Text,
                        Convert.ToUInt32(databaseForm.PortBox.Text), databaseForm.DatabaseNameBox.Text,
                        databaseForm.UserNameBox.Text, databaseForm.PasswordBox.Text);

                    Query q = new Query();
                    List<string> list;

                    String p = Prompt.ShowDialog("Enter A Track Circuit\n\n Type 'ALL' to Display Every Track Circuit", "Track Information Needed!");
                    if (p.Trim().ToUpper() == "ALL")
                    {
                        list = q.runQuery(conn, "SELECT * FROM track_segments");
                    }
                    else
                    {
                        list = q.runQuery(conn, "SELECT * FROM track_segments where trackCircuit = '" + p.Trim() + "'");
                    }

                    int numRows = list.Count / 16;
                    TrackSegment ts;
                    for (int i = 0; i < numRows; i++)
                    {
                        var offset = 16*i;
                        ts = new TrackSegment(list[offset + 3].ToString(), Convert.ToInt32(list[offset + 4].ToString()), Convert.ToInt32(list[offset + 5].ToString()), Convert.ToDouble(list[offset + 6].ToString()),
                             Convert.ToDouble(list[offset + 7].ToString()), Convert.ToDouble(list[offset + 8].ToString()), Convert.ToDouble(list[offset + 9].ToString()), Convert.ToDouble(list[offset + 10].ToString()),
                             Convert.ToDouble(list[offset + 11].ToString()), Convert.ToDouble(list[offset + 12].ToString()), Convert.ToDouble(list[offset + 13].ToString()), Convert.ToInt32(list[offset + 14].ToString()),
                             Convert.ToInt32(list[offset + 15].ToString()));
                        TrackLayout.Track.Add(ts);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Log(ex);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void CreateNewTrack()
        {
            NewTrackLayoutForm newTrackLayout = new NewTrackLayoutForm();
            if (newTrackLayout.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                // TODO: need to do error checking on the input to make sure that all the 
                // fields are filled out.
                // Do we do this with a bunch of if statements?
                try
                {
                    TrackLayout.Customer = newTrackLayout.ProjectNameBox.Text;
                    TrackLayout.ProjectName = newTrackLayout.ProjectNameBox.Text;
                    TrackLayout.Contract = newTrackLayout.ContractBox.Text;
                    TrackLayout.Preparer = newTrackLayout.PreparerBox.Text;
                    TrackLayout.MaxSpeed = Convert.ToDouble(newTrackLayout.MaxSpeedBox.Text);
                    TrackLayout.TrainType = newTrackLayout.TypeBox.Text;
                    TrackLayout.Tonnage = Convert.ToDouble(newTrackLayout.TonnageBox.Text);
                    TrackLayout.MaxBlockLength = Convert.ToDouble(newTrackLayout.MaxBlockLengthBox.Text);
                    TrackLayout.BreakingCharacteristics = newTrackLayout.BreakingCharacteristicsBox.Text;
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Log(ex);
                }
            }
        }

        /// <summary>
        /// Load a saved track 
        /// </summary>
        public static void LoadTrack()
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "xml files (*.xml)|*.xml*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        ProgressBoxForm progress = new ProgressBoxForm();

                        progress.Show();
                        using (myStream)
                        {

                            XDocument segment = XDocument.Load(openFileDialog1.FileName);
                            var result = from q in segment.Descendants("TrackSegment")
                                         select new TrackSegment
                                         {
                                             TrackCircuit = q.Element("Circuit").Value,
                                             BrakeLocation = int.Parse(q.Element("BrakeLocation").Value),
                                             TargetLocation = int.Parse(q.Element("TargetLocation").Value),
                                             GradeWorst = double.Parse(q.Element("GradeWorst").Value),
                                             SpeedMax = double.Parse(q.Element("SpeedMax").Value),
                                             OverSpeed = double.Parse(q.Element("OverSpeed").Value),
                                             VehicleAccel = double.Parse(q.Element("VehicleAccel").Value),
                                             ReactionTime = double.Parse(q.Element("ReactionTime").Value),
                                             BrakeRate = double.Parse(q.Element("BrakeRate").Value),
                                             RunwayAccelSec = double.Parse(q.Element("RunwayAccelSec").Value),
                                             PropulsionRemSec = double.Parse(q.Element("PropulsionRemSec").Value),
                                             BrakeBuildUpSec = int.Parse(q.Element("BrakeBuildUpSec").Value),
                                             OverhangDist = int.Parse(q.Element("OverhangDist").Value),
                                         };
                            foreach (var item in result)
                            {
                                TrackLayout.Track.Add(item);

                                progress.progressBar1.Increment(result.Count());
                                Thread.Sleep(100);
                            }


                        }
                        progress.Close();
                    }

                }
                catch (Exception ex)
                {
                    LogManager.Logger.Log(ex);
                }

            }
        }

        /// <summary>
        /// Save a track to a file
        /// </summary>
        public static void SaveTrack()
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "xml files (*.xml)|*.xml*";
            save.FilterIndex = 0;
            save.RestoreDirectory = true;
            if (save.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "  ";
                    settings.NewLineChars = "\r\n";
                    settings.NewLineHandling = NewLineHandling.Replace;
                    using (XmlWriter writer = XmlWriter.Create(save.FileName, settings))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("TrackLayout");

                        foreach (TrackSegment track in TrackLayout.Track)
                        {
                            writer.WriteStartElement("TrackSegment");

                            writer.WriteElementString("Circuit", track.TrackCircuit.ToString());
                            writer.WriteElementString("BrakeLocation", track.BrakeLocation.ToString());
                            writer.WriteElementString("TargetLocation", track.TargetLocation.ToString());
                            writer.WriteElementString("GradeWorst", track.GradeWorst.ToString());
                            writer.WriteElementString("SpeedMax", track.SpeedMax.ToString());
                            writer.WriteElementString("OverSpeed", track.OverSpeed.ToString());
                            writer.WriteElementString("VehicleAccel", track.VehicleAccel.ToString());
                            writer.WriteElementString("ReactionTime", track.ReactionTime.ToString());
                            writer.WriteElementString("BrakeRate", track.BrakeRate.ToString());
                            writer.WriteElementString("RunwayAccelSec", track.RunwayAccelSec.ToString());
                            writer.WriteElementString("PropulsionRemSec", track.PropulsionRemSec.ToString());
                            writer.WriteElementString("BrakeBuildUpSec", track.BrakeBuildUpSec.ToString());
                            writer.WriteElementString("OverhangDist", track.OverhangDist.ToString());

                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Log(ex);
                }
            }
        }



        /// <summary>
        /// Loads a track from an excel file
        /// </summary>
        /// <param name="filename"></param>
        private static void LoadExcelFile(object filename)
        {
            ExcelParser parser = new ExcelParser(filename.ToString());
            parser.processData();
            try
            {
                ProgressBoxForm progress = new ProgressBoxForm();
                progress.Show();

                DatabaseConnection conn = new DatabaseConnection(
                    Config.ConfigManager.Database,
                    Config.ConfigManager.Port,
                    Config.ConfigManager.DatabaseName,
                    Config.ConfigManager.UserName,
                    Config.ConfigManager.Password);
                conn.openConnection();

                foreach (TrackSegment t in TrackLayout.Track)
                {
                    DatabaseOperations.InsertIntoDatabase(conn, t);
                    progress.progressBar1.Increment(1 / TrackLayout.Track.Count);
                }
                progress.Close();
                conn.closeConnection();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Log(ex);
            }
            parser.cleanUp();
        }


    }
}
