using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Freyja
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private void Log(string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            txtLog.AppendText(line + Environment.NewLine);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void UpdateRunButtonState()
        {
            bool hasAllPaths =
                !string.IsNullOrWhiteSpace(txtCirculationPath.Text) &&
                !string.IsNullOrWhiteSpace(txtUsersPath.Text) &&
                !string.IsNullOrWhiteSpace(txtItemsPath.Text) &&
                !string.IsNullOrWhiteSpace(txtOutputFolder.Text);

            btnRun.Enabled = hasAllPaths;
        }

        private bool ValidateInputs(out string errorMessage)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(txtCirculationPath.Text) || !File.Exists(txtCirculationPath.Text))
                errors.Add("Circulation Log CSV file is missing or does not exist.");

            if (string.IsNullOrWhiteSpace(txtUsersPath.Text) || !File.Exists(txtUsersPath.Text))
                errors.Add("New Users CSV file is missing or does not exist.");

            if (string.IsNullOrWhiteSpace(txtItemsPath.Text) || !File.Exists(txtItemsPath.Text))
                errors.Add("Item Matched CSV file is missing or does not exist.");

            if (string.IsNullOrWhiteSpace(txtOutputFolder.Text) || !Directory.Exists(txtOutputFolder.Text))
                errors.Add("Output folder is missing or does not exist.");


            if (errors.Count > 0)
            {
                errorMessage = string.Join(Environment.NewLine, errors);
                return false;
            }

            errorMessage = "";
            return true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            btnRun.Enabled = false;
            nudThreshold.Value = 10.00m; 
            toolTipMain.SetToolTip(nudThreshold, "Maximum fine amount eligible for forgiveness (Phase 1: validation only).");
            toolTipMain.SetToolTip(clbPatronGroups, "Optional: patron groups eligible for auto-forgiveness. Leave unchecked to forgive none.");
            toolTipMain.SetToolTip(txtOutputFolder, "Select the folder where output files will be written in Phase 6.");
            toolTipMain.SetToolTip(btnBrowseOutput, "Choose the output folder.");
            
            for (int i = 0; i < clbPatronGroups.Items.Count; i++)
            {
                clbPatronGroups.SetItemChecked(i, false);
            }
            UpdateRunButtonState();
        }

        private void btnBrowseCirculation_Click(object sender, EventArgs e)
        {
            if (openFileDialogCsv.ShowDialog() == DialogResult.OK)
            {
                txtCirculationPath.Text = openFileDialogCsv.FileName;
                Log("Selected Circulation Log: " + openFileDialogCsv.FileName);
                UpdateRunButtonState();
            }
        }

        private void btnBrowseUsers_Click(object sender, EventArgs e)
        {
            if (openFileDialogCsv.ShowDialog() == DialogResult.OK)
            {
                txtUsersPath.Text = openFileDialogCsv.FileName;
                Log("Selected New Users CSV: " + openFileDialogCsv.FileName);
                UpdateRunButtonState();
            }
        }

        private void btnBrowseItems_Click(object sender, EventArgs e)
        {
            if (openFileDialogCsv.ShowDialog() == DialogResult.OK)
            {
                txtItemsPath.Text = openFileDialogCsv.FileName;
                Log("Selected Item Matched CSV: " + openFileDialogCsv.FileName);
                UpdateRunButtonState();
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogOutput.ShowDialog() == DialogResult.OK)
            {
                txtOutputFolder.Text = folderBrowserDialogOutput.SelectedPath;
                Log("Selected Output Folder: " + folderBrowserDialogOutput.SelectedPath);
                UpdateRunButtonState();
            }
        }

        private void clbPatronGroups_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // CheckedItems hasn't updated yet, so defer
            BeginInvoke(new Action(UpdateRunButtonState));
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs(out string error))
            {
                MessageBox.Show(error, "Freyja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log("Validation failed:");
                Log(error);
                return;
            }

            Log("======================================");
            Log("Phase 1 Run Summary");
            Log("Circulation: " + txtCirculationPath.Text);
            Log("Users: " + txtUsersPath.Text);
            Log("Items: " + txtItemsPath.Text);
            Log("Output Folder: " + txtOutputFolder.Text);
            Log("Threshold: " + nudThreshold.Value);

            var groups = clbPatronGroups.CheckedItems.Cast<string>().ToList();
            Log("Selected Groups: " + (groups.Count == 0 ? "(none)" : string.Join(", ", groups)));

            Log("Timestamp: " + DateTime.Now);
            Log("Phase 1 complete: inputs validated. CSV processing will be implemented in Phase 2+.");
            Log("======================================");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtCirculationPath.Clear();
            txtUsersPath.Clear();
            txtItemsPath.Clear();
            txtOutputFolder.Clear();

            nudThreshold.Value = 10.00m;

            for (int i = 0; i < clbPatronGroups.Items.Count; i++)
                clbPatronGroups.SetItemChecked(i, false);

            txtLog.Clear();
            Log("Cleared selections.");

            btnRun.Enabled = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }


}
