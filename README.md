
- TOTAL = sum of charges in that line-item
- Fee types are de-duplicated and combined
- One line per user+item per category

---

## Repository Structure (What each file does)

### `Csv/`
CSV parsing, header validation, normalization, and join services.

#### `Csv/CsvMaps/` (CsvHelper ClassMaps)
Maps raw CSV headers into strongly-typed row models.
- `CirculationLogMap.cs` → `CirculationLogRow`
- `NewUserMap.cs` → `NewUserRow`
- `ItemMatchedMap.cs` → `ItemMatchedRow`

#### `Csv/CsvServices/` (Core pipeline services)
- `CsvLoader.cs`
  - Loads CSV with robust settings (BOM, quotes, trimming, blank lines)
  - Also reads headers for validation
- `CsvHeaderValidator.cs`
  - Validates required headers
- `CirculationDescriptionParser.cs`
  - Parses Description → Fee/Fine type + Amount
- `NormalizationService.cs` (**Phase 3**)
  - Builds:
    - `UsersByExternalId`
    - `ItemsByBarcode`
    - `EventsReadyForJoin`
  - Filters invalid rows
  - Collects `NormalizationError`s
- `JoinService.cs` (**Phase 4**)
  - Builds `CombinedRecord` objects
  - Adds one `CombinedCharge` per event
  - Produces `JoinResult`

---

### `Services/`
Business logic services.

- `ClassificationService.cs` (**Phase 5**)
  - Applies rule:
    ```
    (Amount < threshold) OR (PatronGroup selected)
    ```
  - Produces:
    - `ForgivenLineItems`
    - `FineLineItems`
  - Returns `ClassificationResult`

- `OutputGenerationService.cs` (**Phase 6**)
  - Writes:
    - `Forgiven_List.txt`
    - `Fine_List.txt`
    - `ErrorLog.txt`
  - Uses atomic writes (temp → replace)
  - Returns `OutputGenerationResult`

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
- `ClassifiedLineItem.cs`
- `ClassificationResult.cs`

#### `Models/Output/` (Phase 6 output)
- `OutputGenerationResult.cs`

---

### `docs/`
- `workflow.png` — workflow diagram

---

### UI / Entry
- `MainForm.cs`
  - Orchestrates Phase 2 → Phase 6 pipeline
  - Handles UI events and logging
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
4. Choose:
   - Threshold
   - Optional patron groups
5. Click **Run**:
   - Pipeline runs Phase 2 → Phase 6
   - Output files are written to the selected folder

---

## Notes

- Do not commit real CSV files if they contain PII.
- Use synthetic or redacted samples only.
