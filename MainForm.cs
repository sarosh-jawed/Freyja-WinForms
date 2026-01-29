using Freyja.Csv.CsvServices;
using Freyja.Models.Normalized;
using Freyja.Services;
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
        private Freyja.Models.Classification.ClassificationResult? _phase5Result;

        private void Log(string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action(() => AppendLogLine(line)));
            }
            else
            {
                AppendLogLine(line);
            }
        }

        private void SetUiRunning(bool running)
        {
            btnRun.Enabled = !running;
            btnClear.Enabled = !running;
            btnBrowseCirculation.Enabled = !running;
            btnBrowseUsers.Enabled = !running;
            btnBrowseItems.Enabled = !running;
            btnBrowseOutput.Enabled = !running;
            clbPatronGroups.Enabled = !running;
            nudThreshold.Enabled = !running;
            Cursor = running ? Cursors.WaitCursor : Cursors.Default;
        }

        private void AppendLogLine(string line)
        {
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

        private List<string> GetSelectedPatronGroups()
        {
            return clbPatronGroups.CheckedItems
                .Cast<object>()
                .Select(x => x?.ToString() ?? string.Empty)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
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

        private async void btnRun_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs(out string error))
            {
                MessageBox.Show(error, "Freyja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log("Validation failed:");
                Log(error);
                return;
            }

            // Snapshot inputs on UI thread (NO WinForms control access inside Task.Run)
            var circPath = txtCirculationPath.Text.Trim();
            var usersPath = txtUsersPath.Text.Trim();
            var itemsPath = txtItemsPath.Text.Trim();
            var outputFolder = txtOutputFolder.Text.Trim();

            // nudThreshold.Value is decimal
            var threshold = nudThreshold.Value;

            // This touches UI (CheckedListBox), so snapshot here
            var selectedGroups = GetSelectedPatronGroups();

            SetUiRunning(true);

            try
            {
                await Task.Run(() =>
                {
                    RunPipeline(circPath, usersPath, itemsPath, outputFolder, threshold, selectedGroups);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Freyja", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log("Run failed:");
                Log(ex.ToString());
            }
            finally
            {
                SetUiRunning(false);
                UpdateRunButtonState();
            }
        }

        // ---------------------------------------
        // Move your existing Phase 2–6 code here
        // IMPORTANT: use parameters only!
        // ---------------------------------------
        private void RunPipeline(
            string circPath,
            string usersPath,
            string itemsPath,
            string outputFolder,
            decimal threshold,
            List<string> selectedGroups)
        {
            _phase3Result = null;
            _phase4Result = null;
            _phase5Result = null;

            Log("Phase 2: Loading CSV files...");

            // 1) Circulation headers + load
            var circHeaders = _csvLoader.ReadHeaders(circPath);
            var circMissing = CsvHeaderValidator.MissingHeaders(
                circHeaders,
                "User barcode", "Item barcode", "Circ action", "Date", "Description"
            );
            if (circMissing.Count > 0)
                throw new InvalidOperationException("Circulation CSV missing headers: " + string.Join(", ", circMissing));

            var circulation =
                _csvLoader.Load<Freyja.Models.CirculationLogRow, Freyja.Csv.CsvMaps.CirculationLogMap>(circPath);

            Log($"Loaded Circulation rows: {circulation.Count}");
            Log($"Distinct user barcodes: {circulation.Select(x => x.UserBarcode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count()}");
            Log($"Distinct item barcodes: {circulation.Select(x => x.ItemBarcode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count()}");

            // 2) Users headers + load
            var userHeaders = _csvLoader.ReadHeaders(usersPath);
            var userMissing = CsvHeaderValidator.MissingHeaders(
                userHeaders,
                "groups.group", "users.external_system_id", "users.first_name", "users.last_name"
            );
            if (userMissing.Count > 0)
                throw new InvalidOperationException("Users CSV missing headers: " + string.Join(", ", userMissing));

            var users =
                _csvLoader.Load<Freyja.Models.NewUserRow, Freyja.Csv.CsvMaps.NewUserMap>(usersPath);

            Log($"Loaded Users rows: {users.Count}");
            Log($"Distinct external_system_id: {users.Select(x => x.ExternalSystemId).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count()}");

            // 3) Items headers + load
            var itemHeaders = _csvLoader.ReadHeaders(itemsPath);
            var itemMissing = CsvHeaderValidator.MissingHeaders(
                itemHeaders,
                "Barcode", "Instance (Title, Publisher, Publication date)"
            );
            if (itemMissing.Count > 0)
                throw new InvalidOperationException("Items CSV missing headers: " + string.Join(", ", itemMissing));

            var items =
                _csvLoader.Load<Freyja.Models.ItemMatchedRow, Freyja.Csv.CsvMaps.ItemMatchedMap>(itemsPath);

            Log($"Loaded Item rows: {items.Count}");
            Log($"Distinct item barcodes: {items.Select(x => x.Barcode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count()}");

            // --------------------
            // Phase 3
            // --------------------
            Log("Phase 3: Starting normalization and field extraction...");

            var normalizer = new Freyja.Csv.CsvServices.NormalizationService(Log);
            _phase3Result = normalizer.Normalize(circulation, users, items);

            var missingItems = _phase3Result.Errors.Count(e => e.Type == NormalizationErrorType.MissingItem);
            Log($"Phase 3: Missing items (for ErrorLog later): {missingItems}");
            Log("Phase 3 complete: normalization and field extraction finished.");

            // --------------------
            // Phase 4
            // --------------------
            Log("Phase 4: Joining normalized datasets...");

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

            var combinedRecords = joinResult.Records;
            _phase4Result = joinResult;

            // --------------------
            // Phase 5
            // --------------------
            Log("Phase 5: Classifying charges into Forgiven vs Fine (Amount < threshold OR patron group selected)...");

            var classifier = new Freyja.Services.ClassificationService();
            _phase5Result = classifier.Classify(combinedRecords, threshold, selectedGroups);

            Log($"Phase 5: Threshold used: {threshold:0.00}");
            Log($"Phase 5: Selected patron groups: {(selectedGroups.Count == 0 ? "(none)" : string.Join(", ", selectedGroups))}");
            Log($"Phase 5: Combined records processed: {_phase5Result.RecordsProcessed}");
            Log($"Phase 5: Charges processed: {_phase5Result.ChargesProcessed}");
            Log($"Phase 5: Forgiven line-items: {_phase5Result.ForgivenLineItems.Count}");
            Log($"Phase 5: Fine line-items: {_phase5Result.FineLineItems.Count}");

            Log("Phase 5 complete. Next: Phase 6 will generate Forgiven_List.txt, Fine_List.txt, and ErrorLog.");

            // --------------------
            // Phase 6
            // --------------------
            Log("Phase 6: Generating output files...");

            if (_phase5Result == null) throw new InvalidOperationException("Phase 5 result is null. Cannot proceed to Phase 6.");
            if (_phase3Result == null) throw new InvalidOperationException("Phase 3 result is null. Cannot proceed to Phase 6.");

            var outputService = new OutputGenerationService();
            var outResult = outputService.Generate(_phase5Result, _phase3Result, outputFolder);

            Log($"Phase 6: Wrote Forgiven_List.txt lines: {outResult.ForgivenLinesWritten}");
            Log($"Phase 6: Wrote Fine_List.txt lines: {outResult.FineLinesWritten}");
            Log($"Phase 6: Wrote ErrorLog.txt lines: {outResult.ErrorLinesWritten}");
            Log("Phase 6 complete. Next: Phase 7 will focus on robustness and usability.");
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
            _phase3Result = null;
            _phase4Result = null;
            _phase5Result = null;
            btnRun.Enabled = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }


}
