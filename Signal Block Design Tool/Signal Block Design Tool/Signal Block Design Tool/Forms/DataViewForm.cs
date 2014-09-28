﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Signal_Block_Design_Tool.Files;
namespace Signal_Block_Design_Tool.Forms
{
    public partial class DataViewForm : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public DataViewForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        public void ClearTreeView()
        {
            this.treeView1.DataBindings.Clear();
            this.treeView1.Nodes.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        public void UpdateTreeView()
        {

            ClearTreeView();
            foreach (TrackSegment t in TrackLayout.Track)
            {
                TreeNode[] children = new TreeNode[14];
                children[0] = new TreeNode("Start Point: " + t.StartPoint.ToString());
                children[1] = new TreeNode("End Point: " + t.EndPoint.ToString());
                children[2] = new TreeNode("Brake Location: " + t.BrakeLocation.ToString());
                children[3] = new TreeNode("Target Location: " + t.TargetLocation.ToString());
                children[4] = new TreeNode("Grade Worst: " + t.GradeWorst.ToString());
                children[5] = new TreeNode("Speed Max: " + t.SpeedMax.ToString());
                children[6] = new TreeNode("Overspeed: " + t.OverSpeed.ToString());
                children[7] = new TreeNode("Vehicle Accel: " + t.VehicleAccel.ToString());
                children[8] = new TreeNode("Reaction Time: " + t.ReactionTime.ToString());
                children[9] = new TreeNode("Brake Rate: " + t.BrakeRate.ToString());
                children[10] = new TreeNode("Runaway Accel: " + t.RunwayAccelSec.ToString());
                children[11] = new TreeNode("Propulsion Rem: " + t.PropulsionRemSec.ToString());
                children[12] = new TreeNode("Brake Build Up: " + t.BrakeBuildUpSec.ToString());
                children[13] = new TreeNode("Overhang Distance: " + t.OverhangDist.ToString());
                TreeNode rootNode = new TreeNode("Circuit: " + t.TrackCircuit.ToString(), children);
                this.treeView1.Nodes.Add(rootNode);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        public void ClearDataView()
        {
            this.dataGridView1.DataBindings.Clear();
            this.dataGridView1.Rows.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        public void UpdateDataView()
        {
            ClearDataView();
            dataGridView2.ColumnCount = 14;
            dataGridView1.Columns[0].Name = "TrackCircuit";
            dataGridView2.Columns[1].Name = "Brake Location: ";
            dataGridView2.Columns[2].Name = "Target Location: ";
            dataGridView2.Columns[3].Name = "Grade Worst: ";
            dataGridView2.Columns[4].Name = "Speed Max: ";
            dataGridView2.Columns[5].Name = "Overspeed: ";
            dataGridView2.Columns[6].Name = "Vehicle Accel: ";
            dataGridView2.Columns[7].Name = "Reaction Time: ";
            dataGridView2.Columns[8].Name = "Brake Rate: ";
            dataGridView2.Columns[9].Name = "Runaway Accel: ";
            dataGridView2.Columns[10].Name = "Propulsion Rem: ";
            dataGridView2.Columns[11].Name = "Brake Build Up: ";
            dataGridView2.Columns[12].Name = "Overhang Distance: ";
            dataGridView2.Columns[13].Name = "Is Safe: ";
            dataGridView1.ColumnCount = 3;
            dataGridView1.Columns[0].Name = "Track Circuit";
            dataGridView1.Columns[1].Name = "Calculated Safe Breaking Distance";
            dataGridView1.Columns[2].Name = "Available Distance";
            int rowNum = 0;
            int rowIndex = 1;
            foreach (TrackSegment t in TrackLayout.Track)
            {
                this.dataGridView2.Rows.Add(t.TrackCircuit.ToString(), t.BrakeLocation.ToString(), t.TargetLocation.ToString(),
                t.GradeWorst.ToString(), t.SpeedMax.ToString(), t.OverSpeed.ToString(), t.VehicleAccel.ToString(), t.ReactionTime.ToString(),
                t.BrakeRate.ToString(), t.RunwayAccelSec.ToString(), t.PropulsionRemSec.ToString(), t.BrakeBuildUpSec.ToString(), t.OverhangDist.ToString(), t.IsSafe.ToString());

                this.dataGridView1.Rows.Add(t.TrackCircuit.ToString(), t.SafeBreakingDistance.ToString(), t.SafeBreakingDistanceRequired.ToString());
                rowNum++;
            }
            String badRows = null;

            for (int i = 0; i < rowNum; i++)
            {
                if (dataGridView2.Rows[i].Cells[12].Value.ToString() == "False")
                {
                    badRows += rowIndex + ", ";
                }
                rowIndex++;
            }
            if (badRows != null)
            {
                badRows = badRows.Substring(0, badRows.Length - 2);
                MessageBox.Show("The following rows have unsafe conditions:\n" + badRows, "Critical Error!",
                   MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
    }
}