import argparse
import io
import time
import zipfile
from datetime import date, timedelta
from pathlib import Path

import requests

URL = "https://nsearchives.nseindia.com/content/cm/BhavCopy_NSE_CM_0_0_0_{stamp}_F_0000.csv.zip"
HEADERS = {"User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) StockAnalysisCollegeProject/1.0"}
DEFAULT_DIR = Path(__file__).resolve().parent.parent / "01_Stock" / "Stock_Data" / "NSE" / "History"


def weekdays(start: date, end: date):
    current = start
    while current <= end:
        if current.weekday() < 5:
            yield current
        current += timedelta(days=1)


def download_day(day: date, out_dir: Path) -> str:
    target = out_dir / f"BhavCopy_NSE_CM_0_0_0_{day:%Y%m%d}_F_0000.csv"
    if target.exists():
        return "exists"

    for attempt in range(3):
        try:
            response = requests.get(URL.format(stamp=f"{day:%Y%m%d}"), headers=HEADERS, timeout=30)
        except requests.RequestException:
            time.sleep(2 * (attempt + 1))
            continue

        if response.status_code == 404:
            return "unavailable"
        if response.status_code != 200:
            time.sleep(2 * (attempt + 1))
            continue

        try:
            with zipfile.ZipFile(io.BytesIO(response.content)) as archive:
                name = next(n for n in archive.namelist() if n.lower().endswith(".csv"))
                target.write_bytes(archive.read(name))
        except (zipfile.BadZipFile, StopIteration):
            return "invalid zip"
        return "downloaded"

    return "failed"


def main():
    parser = argparse.ArgumentParser(description="Download NSE BhavCopy reports for a date range.")
    parser.add_argument("--months", type=int, default=5, help="how many months back to download")
    parser.add_argument("--out", type=Path, default=DEFAULT_DIR)
    args = parser.parse_args()

    end = date.today() - timedelta(days=1)
    start = end - timedelta(days=round(args.months * 30.4))
    args.out.mkdir(parents=True, exist_ok=True)

    counts: dict[str, int] = {}
    for day in weekdays(start, end):
        status = download_day(day, args.out)
        counts[status] = counts.get(status, 0) + 1
        print(f"{day}: {status}", flush=True)
        if status != "exists":
            time.sleep(0.5)

    print("Summary:", counts)


if __name__ == "__main__":
    main()
