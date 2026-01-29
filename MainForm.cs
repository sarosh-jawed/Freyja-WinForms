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

                var fileName = Path.GetFileName(openFileDialogCsv.FileName);
                Log($"Circulation log selected: {fileName} ({openFileDialogCsv.FileName})");

                UpdateRunButtonState();
            }
        }


        private void btnBrowseUsers_Click(object sender, EventArgs e)
        {
            if (openFileDialogCsv.ShowDialog() == DialogResult.OK)
            {
                txtUsersPath.Text = openFileDialogCsv.FileName;

                var fileName = Path.GetFileName(openFileDialogCsv.FileName);
                Log($"Users file selected: {fileName} ({openFileDialogCsv.FileName})");

                UpdateRunButtonState();
            }
        }


        private void btnBrowseItems_Click(object sender, EventArgs e)
        {
            if (openFileDialogCsv.ShowDialog() == DialogResult.OK)
            {
                txtItemsPath.Text = openFileDialogCsv.FileName;

                var fileName = Path.GetFileName(openFileDialogCsv.FileName);
                Log($"Items file selected: {fileName} ({openFileDialogCsv.FileName})");

                UpdateRunButtonState();
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogOutput.ShowDialog() == DialogResult.OK)
            {
                txtOutputFolder.Text = folderBrowserDialogOutput.SelectedPath;

                Log($"Output folder selected: {folderBrowserDialogOutput.SelectedPath}");

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

                Log("Cannot start processing. One or more required inputs are missing or invalid.");
                foreach (var line in error.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    Log($"• {line}");

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

            var selectedGroupsText = selectedGroups.Count == 0
                ? "None selected"
                : string.Join(", ", selectedGroups);

            Log("Processing started.");
            Log($"Forgiveness settings — Threshold: {threshold:0.00}; Auto-forgive groups: {selectedGroupsText}");

            SetUiRunning(true);

            try
            {
                await Task.Run(() =>
                {
                    RunPipeline(circPath, usersPath, itemsPath, outputFolder, threshold, selectedGroups);
                });

                Log("Processing completed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Freyja", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Log("Processing failed.");
                Log(ex.ToString());
            }
            finally
            {
                SetUiRunning(false);
                UpdateRunButtonState();
            }
        }


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

            Log("Loading input files...");

            // 1) Circulation headers + load
            var circHeaders = _csvLoader.ReadHeaders(circPath);
            var circMissing = CsvHeaderValidator.MissingHeaders(
                circHeaders,
                "User barcode", "Item barcode", "Circ action", "Date", "Description"
            );
            if (circMissing.Count > 0)
                throw new InvalidOperationException("Circulation log is missing required columns: " + string.Join(", ", circMissing));

            var circulation =
                _csvLoader.Load<Freyja.Models.CirculationLogRow, Freyja.Csv.CsvMaps.CirculationLogMap>(circPath);

            var circUniqueUsers = circulation.Select(x => x.UserBarcode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count();
            var circUniqueItems = circulation.Select(x => x.ItemBarcode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count();

            Log($"Circulation log loaded: {circulation.Count} rows ({circUniqueUsers} unique users, {circUniqueItems} unique items).");

            // 2) Users headers + load
            var userHeaders = _csvLoader.ReadHeaders(usersPath);
            var userMissing = CsvHeaderValidator.MissingHeaders(
                userHeaders,
                "groups.group", "users.external_system_id", "users.first_name", "users.last_name"
            );
            if (userMissing.Count > 0)
                throw new InvalidOperationException("Users file is missing required columns: " + string.Join(", ", userMissing));

            var users =
                _csvLoader.Load<Freyja.Models.NewUserRow, Freyja.Csv.CsvMaps.NewUserMap>(usersPath);

            var uniqueUserIds = users.Select(x => x.ExternalSystemId).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count();
            Log($"Users file loaded: {users.Count} rows ({uniqueUserIds} unique user IDs).");

            // 3) Items headers + load
            var itemHeaders = _csvLoader.ReadHeaders(itemsPath);
            var itemMissing = CsvHeaderValidator.MissingHeaders(
                itemHeaders,
                "Barcode", "Instance (Title, Publisher, Publication date)"
            );
            if (itemMissing.Count > 0)
                throw new InvalidOperationException("Items file is missing required columns: " + string.Join(", ", itemMissing));

            var items =
                _csvLoader.Load<Freyja.Models.ItemMatchedRow, Freyja.Csv.CsvMaps.ItemMatchedMap>(itemsPath);

            var uniqueItemBarcodes = items.Select(x => x.Barcode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count();
            Log($"Items file loaded: {items.Count} rows ({uniqueItemBarcodes} unique barcodes).");

            Log("Validating and preparing records...");

            var normalizer = new Freyja.Csv.CsvServices.NormalizationService(Log);
            _phase3Result = normalizer.Normalize(circulation, users, items);

            var missingItemsCount = _phase3Result.Errors.Count(e => e.Type == NormalizationErrorType.MissingItem);
            if (missingItemsCount > 0)
                Log($"Some records reference items not found in the Items file: {missingItemsCount}. See ErrorLog.txt for details.");

            Log("Matching circulation records to users and items...");

            if (_phase3Result == null)
                throw new InvalidOperationException("Normalization results are unavailable. Cannot continue.");

            var joinService = new JoinService();
            var joinResult = joinService.BuildCombinedRecords(_phase3Result);

            Log($"Matching results — Input records: {joinResult.TotalEventsInput}; Matched: {joinResult.EventsJoined}; Grouped records created: {joinResult.Records.Count}.");
            Log($"Lookup gaps — Missing users: {joinResult.MissingUsers}; Missing items: {joinResult.MissingItems}; Matching issues logged: {joinResult.Errors.Count}.");

            var combinedRecords = joinResult.Records;
            _phase4Result = joinResult;

            Log("Applying forgiveness rules...");

            var selectedGroupsText = selectedGroups.Count == 0 ? "None selected" : string.Join(", ", selectedGroups);

            var classifier = new Freyja.Services.ClassificationService();
            _phase5Result = classifier.Classify(combinedRecords, threshold, selectedGroups);

            Log($"Classification summary — Threshold: {threshold:0.00}; Auto-forgive groups: {selectedGroupsText}.");
            Log($"Processed — Records: {_phase5Result.RecordsProcessed}; Charges: {_phase5Result.ChargesProcessed}.");
            Log($"Results — Forgiven line items: {_phase5Result.ForgivenLineItems.Count}; Fine line items: {_phase5Result.FineLineItems.Count}.");

            Log("Generating output files...");

            if (_phase5Result == null) throw new InvalidOperationException("Classification results are unavailable. Cannot continue.");
            if (_phase3Result == null) throw new InvalidOperationException("Normalization results are unavailable. Cannot continue.");

            var outputService = new OutputGenerationService();
            var outResult = outputService.Generate(_phase5Result, _phase3Result, outputFolder);

            Log($"Output saved — Forgiven_List.txt: {outResult.ForgivenLinesWritten} line(s); Fine_List.txt: {outResult.FineLinesWritten} line(s); ErrorLog.txt: {outResult.ErrorLinesWritten} line(s).");
            Log($"Output location: {outputFolder}");
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
            Log("Selections cleared.");

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
