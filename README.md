# Freyja (WinForms)

Freyja is a Windows Forms application that supports a fine forgiveness workflow by taking three CSV inputs, applying configurable rules (threshold and optional patron group selection), and generating output text files.

## Workflow
![Freyja workflow](docs/workflow.png)

## Current Status (Phases 1–3 Complete)
Phases 1–3 are complete.

- Phase 1 delivers the full UI flow, validation, logging, and smooth UX.
- Phase 2 adds reliable CSV ingestion using CsvHelper, required-header validation, and basic sanity statistics logging.
- Phase 3 adds normalization + field extraction:
  - filters invalid circulation rows (only user barcodes starting with `E1` are kept)
  - extracts structured fields from the circulation description (fee/fine type, amount)
  - normalizes users/items into lookup dictionaries for joining in Phase 4
  - collects normalization errors (ex: missing item barcodes) for later error log output

Joins, forgiveness classification, and output file generation begin in Phase 4+.

---

## Implemented

### Phase 1: UI Skeleton + Validation
- UI layout using GroupBoxes:
  - Input Files: Circulation Log CSV, New Users CSV, Item Matched CSV
  - Settings: Threshold (NumericUpDown), Patron Groups (CheckedListBox)
  - Output: Output folder picker
  - Status log area
- Browse dialogs:
  - OpenFileDialog configured for CSV selection
  - FolderBrowserDialog for output folder
- Validation + predictable UX:
  - Run gated until required file paths + output folder are provided
  - Friendly validation messages (MessageBox + log)
  - Clear resets selections and defaults
  - Exit closes the application
- Logging:
  - Time-stamped log entries + auto-scroll
- Tooltips for key controls

### Phase 2: CSV Ingestion Layer (CsvHelper)
- CsvHelper-based parsing with robust configuration:
  - BOM detection, quoted commas, trimmed values, blank line handling
- Strongly-typed row models + ClassMaps to support dotted headers (e.g., `users.external_system_id`)
- Required header validation per file with human-friendly error messages
- Run action loads all 3 CSVs (after Phase 1 validation) and logs:
  - total row counts
  - distinct key counts (e.g., barcodes, external_system_id)
  - basic sanity stats to help with later join/debugging

### Phase 3: Normalization + Field Extraction
- Normalizes Circulation Log into “events ready for join”
  - drops invalid/missing user barcodes (keeps only values starting with `E1`)
  - trims and standardizes key fields (user barcode, item barcode)
- Extracts structured fields from the circulation `Description` text:
  - Fee/Fine type
  - Amount
- Normalizes New Users into a lookup dictionary keyed by `users.external_system_id`
- Normalizes ItemMatched into a lookup dictionary keyed by `Barcode`
- Collects normalization issues (ex: item barcode not found in ItemMatched) for later ErrorLog generation

---

## Repository Structure

### `Csv/`
CSV parsing, mapping, validation, and normalization services.

- `Csv/CsvMaps/`
  - CsvHelper ClassMaps for each CSV input.
  - Maps CSV headers (including dotted headers) into strongly-typed row models.

- `Csv/CsvServices/`
  - `CsvLoader.cs`: reads CSV files using CsvHelper and returns typed row lists.
  - `CsvHeaderValidator.cs`: validates required headers exist and produces friendly errors.
  - `CirculationDescriptionParser.cs`: parses the Circulation Log `Description` field to extract fee type and amount.
  - `NormalizationService.cs`: converts raw rows into normalized models and “events ready for join”.

### `Models/`
Strongly-typed models used across the application.

- Root models:
  - `CirculationLogRow.cs`
  - `NewUserRow.cs`
  - `ItemMatchedRow.cs`

- `Models/Normalized/`
  - Models representing cleaned/normalized data used for joining and output logic in later phases:
    - `NormalizedUser.cs`
    - `NormalizedItem.cs`
    - `NormalizedCirculationEvent.cs`
    - `NormalizationResult.cs`
    - `NormalizationError.cs`

### `docs/`
Documentation assets (images, diagrams).
- `docs/workflow.png`: workflow diagram used in this README.

### UI / Entry
- `MainForm.cs` + `MainForm.Designer.cs` + `MainForm.resx`: WinForms UI logic and layout.
- `Program.cs`: application entry point.

---

## Not Implemented Yet (Next Phases)
- Phase 4: joins to build combined dataset + unmatched reporting
- Phase 5: forgiveness rules + classification (threshold + patron-group auto-forgive logic)
- Phase 6: output generation (`Fine_List.txt`, `Forgiven_List.txt`, and `ErrorLog.txt`)
- Phase 7: robustness (better UX messages, open output folder, save settings, async/background processing)
- Phase 8: packaging and handoff

---

## How to Run (Current)
1. Open the solution in Visual Studio.
2. Build and run.
3. Select:
   - Circulation Log CSV
   - New Users CSV
   - Item Matched CSV
   - Output folder
4. Choose threshold and (optional) patron groups.
5. Click Run:
   - Phase 1 validates inputs
   - Phase 2 loads CSVs, validates headers, and logs counts/stats in the Status area
   - Phase 3 normalizes data and extracts fields needed for joining in Phase 4

---

## Notes
- Do not commit real CSV files if they contain PII. Use synthetic or redacted samples only.
