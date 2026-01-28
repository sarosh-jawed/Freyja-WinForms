# Freyja (WinForms)

Freyja is a Windows Forms application that supports a fine-forgiveness workflow by taking three CSV inputs, extracting/normalizing key fields, joining them into a combined dataset, and (in later phases) applying configurable forgiveness rules and generating output text files.

## Workflow
![Freyja workflow](docs/workflow.png)

---

## Current Status (Phases 1–4 Complete)

✅ **Phase 1**: UI skeleton + predictable UX (validation, logging, tooltips)  
✅ **Phase 2**: Reliable CSV ingestion using CsvHelper + required-header validation + sanity stats  
✅ **Phase 3**: Normalization + field extraction + structured error collection  
✅ **Phase 4**: Join normalized datasets to build `CombinedRecord` list grouped by `(UserExternalSystemId, ItemBarcode)`

### What “Run” does right now
When you click **Run**:

1. **Phase 1** validates required file paths + output folder
2. **Phase 2** loads CSVs (CsvHelper), validates required headers, logs row counts and distinct key counts
3. **Phase 3** normalizes + extracts:
   - filters circulation rows (keeps only user barcodes starting with `E1`)
   - parses `Description` into `FeeFineType` + `Amount` (and optional owner if present)
   - builds lookup dictionaries:
     - users keyed by `users.external_system_id`
     - items keyed by `Barcode`
   - collects normalization errors (e.g., missing items) for later ErrorLog output
4. **Phase 4** joins normalized datasets to build `CombinedRecord` groups and logs join statistics

> Note: Missing items/users are handled at Phase 3 (errors collected and those events excluded from join).  
> That’s why Phase 4 typically reports `Missing items/users: 0` if Phase 3 already gated the events.

---

## Repository Structure (What each file does)

### `Csv/`
CSV parsing, header validation, normalization, and join services.

#### `Csv/CsvMaps/` (CsvHelper ClassMaps)
Maps raw CSV headers into strongly-typed row models.
- `CirculationLogMap.cs`
  - Maps circulation headers (e.g., `User barcode`, `Item barcode`, `Date`, `Description`) → `CirculationLogRow`.
- `NewUserMap.cs`
  - Maps dotted headers (e.g., `users.external_system_id`, `users.first_name`) → `NewUserRow`.
- `ItemMatchedMap.cs`
  - Maps `Barcode`, `Instance (Title, Publisher, Publication date)`, `Material type` → `ItemMatchedRow`.

#### `Csv/CsvServices/` (Core pipeline services)
- `CsvLoader.cs`
  - Loads CSV using CsvHelper with robust settings (BOM detection, quoted commas, trimmed values, blank line handling).
  - Also supports reading headers for required-column validation.
- `CsvHeaderValidator.cs`
  - Validates that required headers exist in a CSV and returns missing headers for friendly error messaging.
- `CirculationDescriptionParser.cs`
  - Parses the circulation `Description` field to extract structured values:
    - `Fee/Fine type`
    - `Amount`
    - optional `Fee/Fine owner`
  - Produces clear parse failures (used to create normalization errors).
- `NormalizationService.cs`
  - Converts raw Phase 2 rows into Phase 3 normalized models:
    - Builds `UsersByExternalId` lookup (`NormalizedUser`)
    - Builds `ItemsByBarcode` lookup (`NormalizedItem`)
    - Produces `EventsReadyForJoin` (`NormalizedCirculationEvent`) by:
      - trimming/normalizing join keys
      - filtering invalid user barcodes (must start with `E1`)
      - parsing `Description` into `FeeFineType` + `Amount`
      - excluding events with missing user/item (but logging them to `Errors`)
- `JoinService.cs`
  - Phase 4 join: takes `NormalizationResult` and groups events into `CombinedRecord` objects keyed by:
    - `(UserExternalSystemId, ItemBarcode)`
  - Adds one `CombinedCharge` per circulation event under a record.
  - Produces `JoinResult` with:
    - `Records` (combined output dataset)
    - `Errors` (join-level errors, usually zero if Phase 3 gated successfully)
    - counters for log output (`TotalEventsInput`, `EventsJoined`, etc.)

---

### `Models/`
Strongly-typed models used across the application.

#### Raw input models (Phase 2 ingestion)
- `Models/CirculationLogRow.cs`
  - Represents a single row from the circulation log CSV (raw strings).
- `Models/NewUserRow.cs`
  - Represents a single row from the new users export CSV (raw strings).
- `Models/ItemMatchedRow.cs`
  - Represents a single row from the item matched CSV (raw strings).

#### `Models/Normalized/` (Phase 3 output)
Normalized models used for clean joining + later output formatting.
- `NormalizedUser.cs`
  - Cleaned user representation used in lookups:
    - `ExternalSystemId`
    - `PatronGroup`
    - `DisplayName` (First + Last)
- `NormalizedItem.cs`
  - Cleaned item representation used in lookups:
    - `Barcode`
    - `Instance`
    - `MaterialType`
- `NormalizedCirculationEvent.cs`
  - Cleaned event used for joining:
    - `UserBarcode` (external_system_id / E1…)
    - `ItemBarcode`
    - `FeeFineType`, `Amount`, optional owner
    - `BilledAt` (parsed date if available)
    - `RawDescription`
- `NormalizationError.cs`
  - Captures normalization failures:
    - `Type` (e.g., MissingItem, MissingUser, BadDescriptionParse)
    - key context (user barcode / item barcode)
    - friendly message
- `NormalizationResult.cs`
  - Phase 3 output bundle:
    - `UsersByExternalId`
    - `ItemsByBarcode`
    - `EventsReadyForJoin`
    - `Errors`

#### `Models/Combined/` (Phase 4 output)
Join output models used for Phase 5 classification and Phase 6 file generation.
- `CombinedCharge.cs`
  - Represents a single extracted fee/fine charge event (type + amount + optional date).
- `CombinedRecord.cs`
  - Represents one combined user+item record with all charges:
    - user fields + item fields + list of `Charges`
- `JoinError.cs`
  - Captures join failures (missing user/item at join stage).
- `JoinResult.cs`
  - Phase 4 output bundle:
    - `Records`, `Errors`
    - counters for logging and sanity checks

---

### `docs/`
Documentation assets
- `docs/workflow.png`
  - Visual workflow diagram used in this README.

---

### UI / Entry
- `MainForm.cs`
  - WinForms UI event handlers for Browse/Run/Clear/Exit.
  - Orchestrates Phase 2 → Phase 3 → Phase 4 pipeline.
  - Logs status to `txtLog`.
- `MainForm.Designer.cs` / `MainForm.resx`
  - UI layout/resources.
- `Program.cs`
  - App entry point (launches MainForm).

---

## Not Implemented Yet (Next Phases)
- **Phase 5**: forgiveness rules + classification (threshold + patron-group selection)
- **Phase 6**: output generation:
  - `Fine_List.txt`
  - `Forgiven_List.txt`
  - `ErrorLog.txt` (from Phase 3/4 error collections)
- **Phase 7**: robustness (better UX messages, open output folder, save settings, async/background processing)
- **Phase 8**: packaging and handoff

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
   - Loads CSVs, validates headers, normalizes, joins, and logs stats in the Status area.

---

## Notes
- Do not commit real CSV files if they contain PII. Use synthetic or redacted samples only.
