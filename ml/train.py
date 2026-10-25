import argparse
import json
from datetime import datetime, timezone
from pathlib import Path

import numpy as np
import pandas as pd


ROOT = Path(__file__).resolve().parent.parent
HISTORY_DIR = ROOT / "01_Stock" / "Stock_Data" / "NSE" / "History"
OUTPUT = Path(__file__).resolve().parent / "output" / "ml_predictions.json"
FRONTEND_COPY = ROOT / "01_Stock" / "Stock_Analysis.Frontend" / "public" / "ml_predictions.json"

WINDOW = 7
HORIZON = 5
TEST_FRACTION = 0.25
TOP_K = 20
MIN_PRICE = 10.0
MIN_AVG_VALUE = 1_000_000.0
CORPORATE_ACTION_JUMP = 0.25

FEATURES = ["ret7", "ret3", "ret1", "close_vs_open", "range7", "vol_consistency", "log_avg_vol", "log_value7"]

COLUMNS = {
    "TradDt": "date", "TckrSymb": "symbol", "SctySrs": "series", "OpnPric": "open", "HghPric": "high",
    "LwPric": "low", "ClsPric": "close", "TtlTradgVol": "volume", "TtlTrfVal": "value",
}


class LogisticModel:
    """Logistic regression fitted with Newton's method (IRLS) and L2 regularisation, using NumPy only."""

    def __init__(self, l2: float = 1.0, iterations: int = 25):
        self.l2 = l2
        self.iterations = iterations

    def fit(self, X, y):
        X = np.asarray(X, dtype=float)
        y = np.asarray(y, dtype=float)
        self.mean = X.mean(axis=0)
        self.std = X.std(axis=0)
        self.std[self.std == 0] = 1.0
        Z = np.hstack([np.ones((len(X), 1)), (X - self.mean) / self.std])
        w = np.zeros(Z.shape[1])
        penalty = self.l2 * np.eye(Z.shape[1])
        penalty[0, 0] = 0.0
        for _ in range(self.iterations):
            p = 1 / (1 + np.exp(-np.clip(Z @ w, -30, 30)))
            gradient = Z.T @ (p - y) + penalty @ w
            hessian = (Z * (p * (1 - p))[:, None]).T @ Z + penalty + 1e-8 * np.eye(Z.shape[1])
            step = np.linalg.solve(hessian, gradient)
            w -= step
            if np.abs(step).max() < 1e-6:
                break
        self.weights = w
        return self

    def predict_proba(self, X):
        Z = np.hstack([np.ones((len(X), 1)), (np.asarray(X, dtype=float) - self.mean) / self.std])
        return 1 / (1 + np.exp(-np.clip(Z @ self.weights, -30, 30)))


def roc_auc(y_true, score) -> float:
    ranks = pd.Series(np.asarray(score)).rank(method="average").to_numpy()
    y = np.asarray(y_true)
    positives = y.sum()
    negatives = len(y) - positives
    if positives == 0 or negatives == 0:
        return 0.5
    return float((ranks[y == 1].sum() - positives * (positives + 1) / 2) / (positives * negatives))


def load_history(folder: Path) -> pd.DataFrame:
    frames = []
    for csv in sorted(folder.glob("*.csv")):
        frame = pd.read_csv(csv, usecols=list(COLUMNS), dtype={"TckrSymb": str, "SctySrs": str})
        frames.append(frame.rename(columns=COLUMNS))
    if not frames:
        raise SystemExit(f"No CSV files found in {folder}. Run download_history.py first.")
    data = pd.concat(frames, ignore_index=True)
    data = data[data["series"] == "EQ"].drop(columns="series")
    data["date"] = pd.to_datetime(data["date"])
    return data.drop_duplicates(["date", "symbol"])


def build_features(data: pd.DataFrame) -> pd.DataFrame:
    wide = {c: data.pivot(index="date", columns="symbol", values=c).sort_index()
            for c in ["open", "high", "low", "close", "volume", "value"]}

    close, open_ = wide["close"], wide["open"]
    avg_volume = wide["volume"].rolling(WINDOW).mean()
    mean_abs_dev = wide["volume"].rolling(WINDOW).apply(lambda x: np.abs(x - x.mean()).mean(), raw=True)
    consistency = (100 - mean_abs_dev / avg_volume.replace(0, np.nan) * 100).clip(0, 100)
    value7 = wide["value"].rolling(WINDOW).sum()

    jump = (close.pct_change().abs() > CORPORATE_ACTION_JUMP).astype(float)
    window_jump = jump.rolling(WINDOW, min_periods=1).max()
    forward_jump = jump.rolling(HORIZON, min_periods=1).max().shift(-HORIZON)

    features = {
        "corporate_action": ((window_jump > 0) | (forward_jump > 0)).astype(float),
        "ret7": close / open_.shift(WINDOW - 1) - 1,
        "ret3": close.pct_change(3),
        "ret1": close.pct_change(1),
        "close_vs_open": close.rolling(WINDOW).mean() / open_.rolling(WINDOW).mean() - 1,
        "range7": wide["high"].rolling(WINDOW).max() / wide["low"].rolling(WINDOW).min() - 1,
        "vol_consistency": consistency / 100,
        "log_avg_vol": np.log1p(avg_volume),
        "log_value7": np.log1p(value7),
        "avg_value": value7 / WINDOW,
        "price": close,
        "fwd_return": close.shift(-HORIZON) / close - 1,
    }

    long = pd.concat({name: frame.stack() for name, frame in features.items()}, axis=1)
    long.index.names = ["date", "symbol"]
    long = long.replace([np.inf, -np.inf], np.nan).reset_index()
    long = long[(long["price"] >= MIN_PRICE) & (long["avg_value"] >= MIN_AVG_VALUE)
                & (long["corporate_action"] == 0)]
    return long.dropna(subset=FEATURES)


def evaluate(name, y_true, prob, test):
    y_true = np.asarray(y_true)
    pred = (prob >= 0.5).astype(int)
    true_positive = int(((pred == 1) & (y_true == 1)).sum())
    scored = test.assign(prob=prob)
    daily = scored.groupby("date").apply(
        lambda g: g.nlargest(TOP_K, "prob")["fwd_return"].mean(), include_groups=False)
    market = scored.groupby("date")["fwd_return"].mean()
    return {
        "model": name,
        "accuracy": round(float((pred == y_true).mean()), 4),
        "precision": round(true_positive / max(int((pred == 1).sum()), 1), 4),
        "recall": round(true_positive / max(int((y_true == 1).sum()), 1), 4),
        "rocAuc": round(roc_auc(y_true, prob), 4),
        "topPicksAvgReturnPct": round(float(daily.mean() * 100), 3),
        "allStocksAvgReturnPct": round(float(market.mean() * 100), 3),
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--history", type=Path, default=HISTORY_DIR)
    args = parser.parse_args()

    raw = load_history(args.history)
    dates = sorted(raw["date"].unique())
    features = build_features(raw)

    labelled = features.dropna(subset=["fwd_return"]).copy()
    labelled["target"] = (labelled["fwd_return"] > 0).astype(int)

    label_dates = sorted(labelled["date"].unique())
    split = int(len(label_dates) * (1 - TEST_FRACTION))
    train_dates = set(label_dates[: max(split - HORIZON, 1)])
    test_dates = set(label_dates[split:])

    train = labelled[labelled["date"].isin(train_dates)]
    test = labelled[labelled["date"].isin(test_dates)]

    models = {
        "Logistic regression": LogisticModel(l2=1.0),
        "Logistic regression (momentum only)": LogisticModel(l2=1.0),
    }
    columns = {
        "Logistic regression": FEATURES,
        "Logistic regression (momentum only)": ["ret7", "ret3", "ret1"],
    }

    results = []
    for name, model in models.items():
        model.fit(train[columns[name]], train["target"])
        prob = model.predict_proba(test[columns[name]])
        results.append(evaluate(name, test["target"], prob, test))
        if name == "Logistic regression":
            main_prob = prob

    scored_test = test.assign(prob=main_prob)
    picks_daily = scored_test.groupby("date").apply(
        lambda g: g.nlargest(TOP_K, "prob")["fwd_return"].mean(), include_groups=False)
    market_daily = scored_test.groupby("date")["fwd_return"].mean()
    daily_test = [
        {"date": str(pd.Timestamp(d).date()), "picksPct": round(float(picks_daily[d] * 100), 3),
         "marketPct": round(float(market_daily[d] * 100), 3)}
        for d in picks_daily.index
    ]

    y_test = test["target"].to_numpy()
    majority = int(train["target"].mean() >= 0.5)
    baselines = [
        {
            "model": "Baseline: always predict the majority class",
            "accuracy": round(float((y_test == majority).mean()), 4),
            "rocAuc": 0.5,
        },
        {
            "model": "Baseline: 7-day return above zero",
            "accuracy": round(float(((test["ret7"] > 0).astype(int).to_numpy() == y_test).mean()), 4),
            "rocAuc": round(roc_auc(y_test, test["ret7"]), 4),
        },
    ]

    best = results[0]
    final_model = models[best["model"]]
    final_columns = columns[best["model"]]
    final_model.fit(labelled[final_columns], labelled["target"])

    latest_date = features["date"].max()
    latest = features[features["date"] == latest_date].copy()
    latest["probUp"] = final_model.predict_proba(latest[final_columns])
    picks = latest.nlargest(TOP_K, "probUp")

    edge = best["topPicksAvgReturnPct"] - best["allStocksAvgReturnPct"]
    output = {
        "generatedAt": datetime.now(timezone.utc).isoformat(),
        "dataFrom": str(pd.Timestamp(dates[0]).date()),
        "dataTo": str(pd.Timestamp(dates[-1]).date()),
        "tradingDays": len(dates),
        "predictionDate": str(pd.Timestamp(latest_date).date()),
        "horizonTradingDays": HORIZON,
        "target": f"Close in {HORIZON} trading days is higher than today's close",
        "universe": f"EQ stocks with price >= {MIN_PRICE:.0f} and average daily traded value >= {MIN_AVG_VALUE:,.0f}; "
                    f"windows with a one-day price jump over {CORPORATE_ACTION_JUMP:.0%} are dropped because prices are not adjusted for splits or bonuses",
        "split": {
            "trainDates": f"{pd.Timestamp(min(train_dates)).date()} to {pd.Timestamp(max(train_dates)).date()}",
            "testDates": f"{pd.Timestamp(min(test_dates)).date()} to {pd.Timestamp(max(test_dates)).date()}",
            "trainRows": int(len(train)),
            "testRows": int(len(test)),
            "gapTradingDays": HORIZON,
        },
        "features": FEATURES,
        "bestModel": best["model"],
        "models": results,
        "baselines": baselines,
        "topPicksEdgePct": round(edge, 3),
        "dailyTest": daily_test,
        "featureWeights": sorted(
            [{"feature": f, "weight": round(float(w), 4)} for f, w in zip(final_columns, final_model.weights[1:])],
            key=lambda item: -abs(item["weight"]),
        ),
        "dataStats": {
            "labelledRows": int(len(labelled)),
            "stocks": int(labelled["symbol"].nunique()),
            "upShare": round(float(labelled["target"].mean()), 4),
            "trainUpShare": round(float(train["target"].mean()), 4),
            "testUpShare": round(float(test["target"].mean()), 4),
        },
        "verdict": (
            f"ROC-AUC {best['rocAuc']:.3f} is close to 0.5 (guessing), and the model's accuracy "
            f"({best['accuracy']:.1%}) is {'below' if best['accuracy'] < baselines[0]['accuracy'] else 'above'} the "
            f"majority-class baseline ({baselines[0]['accuracy']:.1%}). Its top-20 picks returned "
            f"{best['topPicksAvgReturnPct']:.2f}% versus {best['allStocksAvgReturnPct']:.2f}% for all stocks over the "
            f"test period. Treat this as an experiment with little or no predictive value."
            if best["rocAuc"] < 0.6 or edge <= 0 else
            f"ROC-AUC {best['rocAuc']:.3f} and a positive edge over all stocks in the test period, but the period is "
            f"short, so this is not evidence of real predictive power."
        ),
        "picks": [
            {
                "symbol": row.symbol,
                "probUp": round(float(row.probUp), 4),
                "return7dPct": round(float(row.ret7 * 100), 2),
                "price": round(float(row.price), 2),
            }
            for row in picks.itertuples()
        ],
        "disclaimer": (
            "Experimental model trained on a few months of data. It is evaluated on a later time period "
            "than it was trained on, but the test period is short and results are not evidence that it "
            "predicts future returns. Not investment advice."
        ),
    }

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    text = json.dumps(output, indent=2)
    OUTPUT.write_text(text, encoding="utf-8")
    FRONTEND_COPY.write_text(text, encoding="utf-8")
    print(json.dumps({k: output[k] for k in ["dataFrom", "dataTo", "tradingDays", "split", "bestModel", "models", "baselines", "topPicksEdgePct"]}, indent=2))


if __name__ == "__main__":
    main()
