# Freyja (WinForms)

Freyja is a Windows Forms application that supports a fine-forgiveness workflow by taking three CSV inputs, extracting and normalizing key fields, joining them into a combined dataset, classifying charges using configurable rules, and (in the next phase) generating output text files for library staff.

## Workflow
![Freyja workflow](docs/workflow.png)

---

## Current Status (Phases 1–5 Complete)

✅ **Phase 1**: UI skeleton + predictable UX (validation, logging, tooltips)  
✅ **Phase 2**: Reliable CSV ingestion using CsvHelper + required-header validation + sanity stats  
✅ **Phase 3**: Normalization + field extraction + structured error collection  
✅ **Phase 4**: Join normalized datasets to build `CombinedRecord` list grouped by `(UserExternalSystemId, ItemBarcode)`  
✅ **Phase 5**: Classification into **Forgiven vs Fine** using `(Amount <= threshold) OR (PatronGroup selected)`

---

## What “Run” does right now

When you click **Run**:

1. **Phase 1** validates required file paths + output folder
2. **Phase 2** loads CSVs (CsvHelper), validates required headers, logs row counts and distinct key counts
3. **Phase 3** normalizes + extracts:
   - filters circulation rows (keeps only user barcodes starting with `E1`)
   - parses `Description` into `FeeFineType` + `Amount`
   - builds lookup dictionaries:
     - users keyed by `users.external_system_id`
     - items keyed by `Barcode`
   - collects normalization errors (e.g., missing items) for later ErrorLog output
4. **Phase 4** joins normalized datasets to build `CombinedRecord` groups:
   - groups by `(UserExternalSystemId, ItemBarcode)`
   - each group contains a list of all charges for that user+item
5. **Phase 5** classifies charges into:
   - **Forgiven** if:
     - `Amount <= threshold` OR
     - `PatronGroup` is in selected auto-forgive groups
   - **Fine** otherwise
   - Important: a single user+item pair can produce:
     - one forgiven line-item
     - and one fine line-item (if charges split)

> Note: Missing items/users are handled at Phase 3 (errors collected and those events excluded from join).  
> That’s why Phase 4 typically reports `Missing items/users: 0`.

---

## Repository Structure (What each file does)

### `Csv/`
CSV parsing, header validation, normalization, and join services.

#### `Csv/CsvMaps/` (CsvHelper ClassMaps)
Maps raw CSV headers into strongly-typed row models.
- `CirculationLogMap.cs`
  - Maps circulation headers → `CirculationLogRow`.
- `NewUserMap.cs`
  - Maps dotted headers (e.g., `users.external_system_id`) → `NewUserRow`.
- `ItemMatchedMap.cs`
  - Maps `Barcode`, `Instance (...)`, `Material type` → `ItemMatchedRow`.

#### `Csv/CsvServices/` (Core pipeline services)
- `CsvLoader.cs`
  - Loads CSV using CsvHelper with robust settings.
  - Also supports reading headers for required-column validation.
- `CsvHeaderValidator.cs`
  - Validates required headers and returns friendly error messages.
- `CirculationDescriptionParser.cs`
  - Parses `Description` field to extract:
    - `Fee/Fine type`
    - `Amount`
- `NormalizationService.cs` (**Phase 3**)
  - Builds:
    - `UsersByExternalId`
    - `ItemsByBarcode`
    - `EventsReadyForJoin`
  - Filters invalid rows and collects `NormalizationError`s.
- `JoinService.cs` (**Phase 4**)
  - Builds `CombinedRecord` objects grouped by `(UserExternalSystemId, ItemBarcode)`
  - Adds one `CombinedCharge` per circulation event
  - Produces `JoinResult` with stats and errors

---

### `Services/`
Business logic services.
- `ClassificationService.cs` (**Phase 5**)
  - Applies forgiveness rules:
    - `(Amount <= threshold) OR (PatronGroup selected)`
  - Splits charges into:
    - `ForgivenLineItems`
    - `FineLineItems`
  - Produces `ClassificationResult` with summary statistics

---

### `Models/`

#### Raw input models (Phase 2)
- `CirculationLogRow.cs`
- `NewUserRow.cs`
- `ItemMatchedRow.cs`

#### `Models/Normalized/` (Phase 3 output)
- `NormalizedUser.cs`
- `NormalizedItem.cs`
- `NormalizedCirculationEvent.cs`
- `NormalizationError.cs`
- `NormalizationResult.cs`

#### `Models/Combined/` (Phase 4 output)
- `CombinedCharge.cs`
- `CombinedRecord.cs`
- `JoinError.cs`
- `JoinResult.cs`

#### `Models/Classification/` (Phase 5 output)
- `ClassifiedCharge.cs`
  - One classified charge (type, amount, date)
- `ClassifiedLineItem.cs`
  - One output line-item (per user+item, per category)
- `ClassificationResult.cs`
  - Holds:
    - `ForgivenLineItems`
    - `FineLineItems`
    - summary counters

---

### `docs/`
- `workflow.png` – workflow diagram

---

### UI / Entry
- `MainForm.cs`
  - Orchestrates Phase 2 → Phase 3 → Phase 4 → Phase 5
  - Handles UI events and logging
- `Program.cs`
  - App entry point

---

## Not Implemented Yet (Next Phase)

- **Phase 6**: Output generation:
  - `Forgiven_List.txt`
  - `Fine_List.txt`
  - `ErrorLog.txt`

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
5. Click Run:
   - Pipeline runs Phase 2 → Phase 5 and logs all stats

---

## Notes
- Do not commit real CSV files if they contain PII. Use synthetic or redacted samples only.
