# Freyja (WinForms)

Freyja is a Windows Forms application that supports a fine-forgiveness workflow for library staff. It ingests three CSV inputs, normalizes and joins records into a combined dataset, classifies charges using configurable rules, and generates staff-ready output text files.

## Workflow
![Freyja workflow](docs/workflow.png)

---

## Current Status (Phases 1–7 Complete)

✅ **Phase 1**: UI skeleton + predictable UX (validation, logging, tooltips)  
✅ **Phase 2**: Reliable CSV ingestion (CsvHelper) + required-header validation + sanity stats  
✅ **Phase 3**: Normalization + field extraction + structured error collection  
✅ **Phase 4**: Join normalized datasets to build `CombinedRecord` list grouped by `(UserExternalSystemId, ItemBarcode)`  
✅ **Phase 5**: Classification into **Forgiven vs Fine** using `(Amount < threshold) OR (PatronGroup selected)`  
✅ **Phase 6**: Output generation:
- `Forgiven_List.txt`
- `Fine_List.txt`
- `ErrorLog.txt`

✅ **Phase 7**: Robustness + usability hardening:
- Header validation is **trimmed + case-insensitive**
- CSV reads support Excel-open files via **FileShare.ReadWrite**
- **Thread-safe Log()** (safe for background tasks)
- **Async Run** (pipeline runs off UI thread; no UI freeze)
- UI is gated during runs (Run/Clear/Browse/patron groups/threshold disabled)
- Optional usability improvements (e.g., Open Output Folder button) and settings persistence (if implemented in your branch)
- ErrorLog output cleaned (timestamp/counts/stable ordering/no “.00” barcodes)

---

## What “Run” does

When you click **Run**, the pipeline executes:

1. **Phase 1 – Input validation**
   - Validates required file paths and output folder.
2. **Phase 2 – Load CSVs**
   - Reads headers and validates required columns.
   - Loads each CSV via CsvHelper and logs sanity stats (row counts, distinct keys).
3. **Phase 3 – Normalize + extract**
   - Filters invalid user barcodes (expects user barcodes starting with `E1`).
   - Parses `Description` into `FeeFineType` + `Amount`.
   - Builds lookup dictionaries:
     - users keyed by `users.external_system_id`
     - items keyed by `Barcode`
   - Collects normalization errors (MissingItem, InvalidUserBarcode, etc.).
4. **Phase 4 – Join**
   - Groups by `(UserExternalSystemId, ItemBarcode)`.
   - Each group contains a list of charges for that user+item.
5. **Phase 5 – Classify**
   - **Forgiven** if:
     - `Amount < threshold`, OR
     - the user’s `PatronGroup` is in the selected auto-forgive groups
   - **Fine** otherwise
   - A single user+item pair can generate:
     - one forgiven line-item
     - and one fine line-item (if charges split)
6. **Phase 6 – Generate outputs**
   - Writes `Forgiven_List.txt`, `Fine_List.txt`, `ErrorLog.txt`.

> Note: Missing items/users are handled at Phase 3 (errors collected and those events excluded from join).  
> That’s why Phase 4 typically reports `Missing items/users: 0`.

---

## Output Files

### `Forgiven_List.txt` and `Fine_List.txt`
One line per user+item per category. Charges are grouped by fee type so duplicates collapse:
- Example:
  - `Lost item fee & Lost item processing fee` may appear on one line with a combined total.

### `ErrorLog.txt`
Includes:
- Timestamp header
- Missing Item Barcodes (count + stable ordering)
- Invalid User Barcodes Dropped (count + stable ordering)
- Barcodes are formatted for readability (no `.00` artifacts)

---

## Repository Structure

### `Csv/`
CSV parsing, header validation, normalization, and join services.

#### `Csv/CsvMaps/` (CsvHelper ClassMaps)
Maps raw CSV headers into strongly-typed row models.
- `CirculationLogMap.cs`
- `NewUserMap.cs`
- `ItemMatchedMap.cs`

#### `Csv/CsvServices/` (Core pipeline services)
- `CsvLoader.cs`
  - Loads CSV using CsvHelper with reliability-focused settings.
  - Uses a shared read stream to avoid Excel “file in use” failures.
- `CsvHeaderValidator.cs`
  - Computes missing required headers (trimmed + case-insensitive).
- `CirculationDescriptionParser.cs`
  - Parses `Description` field (fee/fine type and amount).
- `NormalizationService.cs` (**Phase 3**)
- `JoinService.cs` (**Phase 4**)

### `Services/`
- `ClassificationService.cs` (**Phase 5**)
  - Rule: `(Amount < threshold) OR (selected PatronGroup)`
- `OutputGenerationService.cs` (**Phase 6**)
  - Writes the three output files

### `Models/`
- Raw input models (Phase 2)
- `Models/Normalized/` (Phase 3 output)
- `Models/Combined/` (Phase 4 output)
- `Models/Classification/` (Phase 5 output)
- `Models/Output/` (Phase 6 output)

### `docs/`
- `workflow.png` – workflow diagram

### UI / Entry
- `MainForm.cs`
  - Orchestrates Phase 2 → Phase 6
  - Runs pipeline asynchronously (Phase 7)
- `Program.cs`
  - App entry point

---

## How to Run

1. Open solution in Visual Studio
2. Build and run
3. Select:
   - Circulation Log CSV
   - New Users CSV
   - Item Matched CSV
   - Output folder
4. Choose threshold and (optional) patron groups
5. Click **Run**
   - Pipeline runs Phase 2 → Phase 6 and logs all stats

---

## Notes
- Do not commit real CSV files if they contain PII. Use synthetic or redacted samples only.
