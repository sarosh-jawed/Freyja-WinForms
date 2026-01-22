# Freyja (WinForms)

Freyja is a Windows Forms application that supports a fine forgiveness workflow by taking three CSV inputs, applying configurable rules (threshold and optional patron group selection), and generating output text files.

## Workflow
![Freyja workflow](docs/workflow.png)

## Current Status (Phases 1–2 Complete)
Phase 1 and Phase 2 are complete.

- Phase 1 delivers the full UI flow, validation, logging, and smooth UX.
- Phase 2 adds reliable CSV ingestion using CsvHelper, required-header validation, and basic sanity statistics logging.

No joining, forgiveness classification, or output file generation is implemented yet (that starts in Phase 3/4+).

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

---

## Not Implemented Yet (Next Phases)
- Phase 3: normalization and field extraction (trim/preserve leading zeros, parse instance fields, parse amount)
- Phase 4: joins to build combined dataset + unmatched reporting
- Phase 5: forgiveness rules + classification
- Phase 6: output generation (`Fine_List.txt`, `Forgiven_List.txt`)
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

---

## Notes
- Do not commit real CSV files if they contain PII. Use redacted samples only.
