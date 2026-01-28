using Freyja.Csv.CsvServices;
using System.Data;

namespace Freyja
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private readonly CsvLoader _csvLoader = new();
        private Freyja.Models.Normalized.NormalizationResult? _phase3Result;
        private Freyja.Models.Combined.JoinResult? _phase4Result;
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

            btnRun.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                Log("Phase 2: Loading CSV files...");

                // 1) Circulation headers + load
                var circHeaders = _csvLoader.ReadHeaders(txtCirculationPath.Text);
                var circMissing = CsvHeaderValidator.MissingHeaders(
                    circHeaders,
                    "User barcode", "Item barcode", "Circ action", "Date", "Description"
                );
                if (circMissing.Count > 0)
                    throw new InvalidOperationException("Circulation CSV missing headers: " + string.Join(", ", circMissing));

                var circulation = _csvLoader.Load<Freyja.Models.CirculationLogRow, Freyja.Csv.CsvMaps.CirculationLogMap>(txtCirculationPath.Text);
                Log($"Loaded Circulation rows: {circulation.Count}");
                Log($"Distinct user barcodes: {circulation.Select(x => x.UserBarcode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count()}");
                Log($"Distinct item barcodes: {circulation.Select(x => x.ItemBarcode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count()}");

                // 2) Users headers + load
                var userHeaders = _csvLoader.ReadHeaders(txtUsersPath.Text);
                var userMissing = CsvHeaderValidator.MissingHeaders(
                    userHeaders,
                    "groups.group", "users.external_system_id", "users.first_name", "users.last_name"
                );
                if (userMissing.Count > 0)
                    throw new InvalidOperationException("Users CSV missing headers: " + string.Join(", ", userMissing));

                var users = _csvLoader.Load<Freyja.Models.NewUserRow, Freyja.Csv.CsvMaps.NewUserMap>(txtUsersPath.Text);
                Log($"Loaded Users rows: {users.Count}");
                Log($"Distinct external_system_id: {users.Select(x => x.ExternalSystemId).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count()}");

                // 3) Items headers + load
                var itemHeaders = _csvLoader.ReadHeaders(txtItemsPath.Text);
                var itemMissing = CsvHeaderValidator.MissingHeaders(
                    itemHeaders,
                    "Barcode", "Instance (Title, Publisher, Publication date)"
                );
                if (itemMissing.Count > 0)
                    throw new InvalidOperationException("Items CSV missing headers: " + string.Join(", ", itemMissing));

                var items = _csvLoader.Load<Freyja.Models.ItemMatchedRow, Freyja.Csv.CsvMaps.ItemMatchedMap>(txtItemsPath.Text);
                Log($"Loaded Item rows: {items.Count}");
                Log($"Distinct item barcodes: {items.Select(x => x.Barcode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count()}");

                // --------------------
                // Phase 3 starts here
                // --------------------
                Log("Phase 3: Starting normalization and field extraction...");

                var normalizer = new Freyja.Csv.CsvServices.NormalizationService(Log);

                // Store for Phase 4 later
                _phase3Result = normalizer.Normalize(circulation, users, items);

                // Optional: quick summary now
                var missingItems = _phase3Result.Errors.Count(e => e.Type == Freyja.Models.Normalized.NormalizationErrorType.MissingItem);
                Log($"Phase 3: Missing items (for ErrorLog later): {missingItems}");

                Log("Phase 3 complete: normalization and field extraction finished.");

                // --------------------
                // Phase 4 starts here
                // --------------------
                Log("Phase 4: Joining normalized datasets...");

                // _phase3Result should never be null here, but keep it safe:
                if (_phase3Result == null)
                    throw new InvalidOperationException("Phase 3 result is null. Cannot proceed to Phase 4.");

                var joinService = new JoinService();
                var joinResult = joinService.BuildCombinedRecords(_phase3Result);

                Log($"Phase 4: Total events input: {joinResult.TotalEventsInput}");
                Log($"Phase 4: Events joined: {joinResult.EventsJoined}");
                Log($"Phase 4: Combined records created: {joinResult.Records.Count}");
                Log($"Phase 4: Missing users: {joinResult.MissingUsers}");
                Log($"Phase 4: Missing items: {joinResult.MissingItems}");
                Log($"Phase 4: Join errors collected: {joinResult.Errors.Count}");

                Log("Phase 4 complete: CombinedRecord list built.");
                Log("Next: Phase 5 will classify Fine_List vs Forgiven_List using (threshold OR selected patron group).");

                // Keep local variables for now (later you can store at class level for Phase 5)
                var combinedRecords = joinResult.Records;
                var joinErrors = joinResult.Errors;
                _phase4Result = joinResult;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Freyja", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log("Run failed:");
                Log(ex.ToString());
            }
            finally
            {
                Cursor = Cursors.Default;
                UpdateRunButtonState(); // re-enable Run if inputs still valid
            }
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
