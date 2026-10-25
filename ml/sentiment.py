import argparse
import json
import time
import xml.etree.ElementTree as ET
from datetime import datetime, timezone
from pathlib import Path
from urllib.parse import quote_plus

import requests
from vaderSentiment.vaderSentiment import SentimentIntensityAnalyzer

ROOT = Path(__file__).resolve().parent.parent
SNAPSHOT = ROOT / "01_Stock" / "Stock_Analysis.Frontend" / "public" / "snapshot.json"
OUTPUT = Path(__file__).resolve().parent / "output" / "sentiment.json"
FRONTEND_COPY = ROOT / "01_Stock" / "Stock_Analysis.Frontend" / "public" / "sentiment.json"

HEADERS = {"User-Agent": "Mozilla/5.0 StockAnalysisCollegeProject/1.0"}
RSS = "https://news.google.com/rss/search?q={query}&hl=en-IN&gl=IN&ceid=IN:en"

FINANCE_TERMS = {
    "surge": 2.5, "surges": 2.5, "rally": 2.3, "rallies": 2.3, "soar": 2.5, "soars": 2.5, "jump": 1.8, "jumps": 1.8,
    "upgrade": 2.0, "upgraded": 2.0, "outperform": 2.0, "profit": 1.5, "beats": 1.8, "record": 1.2, "gains": 1.5,
    "plunge": -2.8, "plunges": -2.8, "slump": -2.4, "slumps": -2.4, "tumble": -2.4, "tumbles": -2.4, "crash": -3.0,
    "downgrade": -2.0, "downgraded": -2.0, "loss": -1.8, "losses": -1.8, "probe": -1.8, "fraud": -3.0,
    "default": -2.5, "penalty": -1.8, "selloff": -2.2, "bearish": -2.0, "bullish": 2.0,
    "buy": 1.8, "upside": 2.0, "downside": -2.0, "sell": -1.5, "multibagger": 2.0, "best-ever": 2.5,
    "outlook": 0.4, "cuts": -1.2, "hikes": 0.8, "wins": 1.8, "bags": 1.5, "secures": 1.5,
}

EMOTIONS = {
    "optimism": {"surge", "surges", "rally", "rallies", "soar", "soars", "jump", "jumps", "upgrade", "upgraded",
                 "growth", "record", "beats", "gains", "profit", "bullish", "strong", "wins", "order"},
    "fear": {"plunge", "plunges", "slump", "slumps", "tumble", "tumbles", "crash", "selloff", "falls", "drops",
             "bearish", "warning", "warns", "risk", "concern", "concerns"},
    "anger": {"fraud", "probe", "penalty", "scam", "raid", "fine", "lawsuit", "default"},
    "uncertainty": {"may", "could", "might", "uncertain", "volatile", "awaits", "await", "likely"},
}


def fetch_headlines(symbol: str, limit: int):
    query = quote_plus(f"{symbol} share NSE when:14d")
    response = requests.get(RSS.format(query=query), headers=HEADERS, timeout=20)
    response.raise_for_status()
    root = ET.fromstring(response.content)

    items = []
    for item in root.iter("item"):
        title = (item.findtext("title") or "").strip()
        if not title:
            continue
        items.append({
            "title": title,
            "link": item.findtext("link") or "",
            "published": item.findtext("pubDate") or "",
        })
        if len(items) >= limit:
            break
    return items


def label_for(compound: float) -> str:
    if compound >= 0.15:
        return "positive"
    if compound <= -0.15:
        return "negative"
    return "neutral"


def dominant_emotion(titles):
    counts = {name: 0 for name in EMOTIONS}
    for title in titles:
        words = {w.strip(".,:;!?'\"()-").lower() for w in title.split()}
        for name, vocabulary in EMOTIONS.items():
            counts[name] += len(words & vocabulary)
    best = max(counts, key=counts.get)
    return best if counts[best] > 0 else "neutral"


def main():
    parser = argparse.ArgumentParser(description="Headline sentiment for the top ranked stocks.")
    parser.add_argument("--top", type=int, default=20)
    parser.add_argument("--headlines", type=int, default=10)
    args = parser.parse_args()

    symbols = [s["symbol"] for s in json.loads(SNAPSHOT.read_text(encoding="utf-8"))["stocks"][: args.top]]

    analyzer = SentimentIntensityAnalyzer()
    analyzer.lexicon.update(FINANCE_TERMS)

    results = []
    for symbol in symbols:
        try:
            headlines = fetch_headlines(symbol, args.headlines)
        except (requests.RequestException, ET.ParseError) as error:
            print(f"{symbol}: failed ({error})", flush=True)
            continue

        scored = []
        for h in headlines:
            title = h["title"].rsplit(" - ", 1)[0]
            scored.append({**h, "title": title, "compound": round(analyzer.polarity_scores(title)["compound"], 3)})

        if not scored:
            print(f"{symbol}: no headlines", flush=True)
            continue

        average = sum(s["compound"] for s in scored) / len(scored)
        labels = [label_for(s["compound"]) for s in scored]
        results.append({
            "symbol": symbol,
            "headlines": len(scored),
            "avgCompound": round(average, 3),
            "label": label_for(average),
            "positive": labels.count("positive"),
            "neutral": labels.count("neutral"),
            "negative": labels.count("negative"),
            "emotion": dominant_emotion([s["title"] for s in scored]),
            "sample": [{"title": s["title"], "compound": s["compound"], "link": s["link"]} for s in scored[:3]],
        })
        print(f"{symbol}: {len(scored)} headlines, avg {average:+.2f}", flush=True)
        time.sleep(1)

    output = {
        "generatedAt": datetime.now(timezone.utc).isoformat(),
        "source": "Google News RSS headlines from the last 14 days",
        "method": "VADER sentiment with a small finance word list; emotion is a simple keyword count",
        "stocks": results,
        "disclaimer": (
            "Headline sentiment is crude. Searches by ticker can match unrelated news, and the scoring is "
            "not trained on financial text. It is shown separately and does not affect the ranking or the "
            "ML model. Not investment advice."
        ),
    }

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    text = json.dumps(output, indent=2)
    OUTPUT.write_text(text, encoding="utf-8")
    FRONTEND_COPY.write_text(text, encoding="utf-8")
    print(f"Saved {len(results)} stocks")


if __name__ == "__main__":
    main()
