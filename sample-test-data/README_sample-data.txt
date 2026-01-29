Sample dataset for Freyja (redacted/synthetic)
Files:
- CirculationLog_sample.csv
- New Users_sample.csv
- ItemMatchedFiles_sample.csv

This dataset is synthetic/redacted and safe to commit. It is designed to exercise:
- Amount threshold boundary (10.00 exactly should route to Fine_List)
- Patron group auto-forgive (e.g., Faculty)
- Invalid user barcode drops
- Missing item barcode error logging
