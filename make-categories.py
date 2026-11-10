from pathlib import Path
import csv
root=Path(__file__).resolve().parent
rows=list(csv.DictReader((root/'moves-source.csv').open(encoding='utf-8')))
rows=[r for r in rows if int(r['id'])<10000]
values=bytearray([255])*(max(int(r['id']) for r in rows)+1)
for r in rows:
    values[int(r['id'])]=int(r['damage_class_id'])-1
(root/'Engine/move-categories.bin').write_bytes(values)
print('Move category rows:',len(rows))
