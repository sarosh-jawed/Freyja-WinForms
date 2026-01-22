# Freyja (WinForms)

Freyja is a Windows Forms application that supports a fine forgiveness workflow by taking three CSV inputs, applying configurable rules (threshold and optional patron group selection), and generating output text files.

## Current Status (Phase 1: UI Skeleton)
Phase 1 is complete. The application provides the UI flow, validation, and logging only.
No CSV parsing, joining, or output file generation is implemented yet.

### Implemented in Phase 1
- UI layout using GroupBoxes:
  - Input Files: Circulation Log CSV, New Users CSV, Item Matched CSV
  - Settings: Threshold (NumericUpDown), Patron Groups (CheckedListBox)
  - Output: Output folder picker
  - Status log area
- Browse dialogs:
  - OpenFileDialog configured for CSV selection
  - FolderBrowserDialog for output folder
- Validation + predictable UX:
  - Run button disabled until required paths are provided
  - Friendly validation messages (MessageBox + log)
  - Clear button resets selections and defaults
  - Exit button closes the application
- Logging:
  - Time-stamped log entries
  - Auto-scroll to latest entry
- Tooltips for key controls

### Not Implemented Yet (coming in Phase 2+)
- CSV ingestion/parsing (CsvHelper)
- Column validation / row counts
- Normalization/extraction
- Joins (circulation → users → items)
- Forgiveness rule classification logic
- Output file generation (Fine_List.txt, Forgiven_List.txt)
- Performance hardening and packaging

## How to Run (Phase 1)
1. Open the solution in Visual Studio.
2. Build and run.
3. Select:
   - Circulation Log CSV
   - New Users CSV
   - Item Matched CSV
   - Output folder
4. Choose threshold and (optional) patron groups.
5. Click Run to validate inputs and print a summary in the log.

## Notes
- Do not commit real CSV files if they contain PII. Use redacted samples only.
