import csv
import json
from pathlib import Path


BASE_DIR = Path(__file__).resolve().parent.parent

CSV_DIR = BASE_DIR / "data" / "csv"
JSON_DIR = BASE_DIR / "data" / "json"

JSON_DIR.mkdir(parents=True, exist_ok=True)


FILE_MAP = {
    "01_rotalar.csv": "routes.json",
    "02_mekanlar.csv": "locations.json",
    "03_rota_sirasi.csv": "route_order.json",
    "04_karakterler.csv": "characters.json",
    "05_karakter_diyaloglari.csv": "dialogues.json",
    "06_gorevler.csv": "quests.json",
    "07_gorev_sorulari.csv": "questions.json",
    "08_oduller_rozetler.csv": "rewards.json",
    "09_ar_icerikler.csv": "ar_contents.json",
    "10_bilgi_kartlari.csv": "info_cards.json",
    "11_medya_dosyalari.csv": "media_files.json",
    "12_teknik_parametreler.csv": "technical_parameters.json",
    "13_riskler_ve_saha_notlari.csv": "field_risks.json",
    "14_saha_testleri.csv": "field_tests.json",
    "15_kullanici_ilerleme_parametreleri.csv": "progress_parameters.json",
}


def clean_value(value: str):
    if value is None:
        return ""

    value = value.strip()

    if value == "":
        return ""

    lower_value = value.lower()

    if lower_value == "evet":
        return True

    if lower_value == "hayır" or lower_value == "hayir":
        return False

    if lower_value == "true":
        return True

    if lower_value == "false":
        return False

    # Integer conversion
    try:
        if value.isdigit() or (value.startswith("-") and value[1:].isdigit()):
            return int(value)
    except Exception:
        pass

    # Float conversion
    try:
        normalized = value.replace(",", ".")
        if "." in normalized:
            return float(normalized)
    except Exception:
        pass

    return value


def read_csv_file(csv_path: Path):
    rows = []

    with csv_path.open("r", encoding="utf-8-sig", newline="") as file:
        reader = csv.DictReader(file)

        for row in reader:
            cleaned_row = {}

            for key, value in row.items():
                if key is None:
                    continue

                clean_key = key.strip()
                cleaned_row[clean_key] = clean_value(value)

            rows.append(cleaned_row)

    return rows


def write_json_file(json_path: Path, data):
    with json_path.open("w", encoding="utf-8") as file:
        json.dump(data, file, ensure_ascii=False, indent=2)


def convert_all():
    print("CSV to JSON conversion started.")
    print(f"CSV directory: {CSV_DIR}")
    print(f"JSON directory: {JSON_DIR}")
    print("-" * 60)

    converted_count = 0
    missing_files = []

    for csv_file_name, json_file_name in FILE_MAP.items():
        csv_path = CSV_DIR / csv_file_name
        json_path = JSON_DIR / json_file_name

        if not csv_path.exists():
            missing_files.append(csv_file_name)
            print(f"[MISSING] {csv_file_name}")
            continue

        data = read_csv_file(csv_path)
        write_json_file(json_path, data)

        converted_count += 1
        print(f"[OK] {csv_file_name} -> {json_file_name} | {len(data)} rows")

    print("-" * 60)
    print(f"Converted files: {converted_count}")

    if missing_files:
        print("Missing files:")
        for file_name in missing_files:
            print(f"- {file_name}")

    print("CSV to JSON conversion finished.")


if __name__ == "__main__":
    convert_all()