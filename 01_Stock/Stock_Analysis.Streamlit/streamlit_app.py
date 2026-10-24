import json
from pathlib import Path

import pandas as pd
import streamlit as st

SNAPSHOT = Path(__file__).parent / "snapshot.json"

st.set_page_config(page_title="Stock Analysis System", page_icon=":material/monitoring:", layout="wide")


@st.cache_data(ttl=3600)
def load_snapshot():
    with SNAPSHOT.open(encoding="utf-8") as f:
        data = json.load(f)

    rows = []
    for s in data["stocks"]:
        m = s["metrics"]
        rows.append(
            {
                "Rank": s["rank"],
                "Symbol": s["symbol"],
                "Score": s["score"],
                "Filters passed": s["passedFilters"],
                "7-day change %": m["priceChangePercentage"],
                "Trading days": m["tradingDays"],
                "Avg daily volume": m["averageDailyVolume"],
                "Traded value": m["totalTradedValue"],
            }
        )
    return data["generatedAt"], pd.DataFrame(rows), {s["symbol"]: s for s in data["stocks"]}


st.title("Stock Analysis System")
st.caption(
    "Rule-based screening and ranking of NSE EQ stocks over a 7-trading-day window. "
    "The score is not a forecast of future returns."
)

if not SNAPSHOT.exists():
    st.error("snapshot.json not found. Generate it with tools/export-snapshot.ps1.")
    st.stop()

generated_at, df, details = load_snapshot()
st.caption(f"Snapshot generated at {generated_at} (UTC)")

with st.sidebar:
    st.header("Filters")
    top_n = st.slider("Show top", 5, len(df), min(25, len(df)))
    min_filters = st.slider("Minimum filters passed", 0, 10, 0)
    search = st.text_input("Search symbol")

view = df[df["Filters passed"] >= min_filters]
if search.strip():
    view = view[view["Symbol"].str.contains(search.strip(), case=False)]
view = view.head(top_n)

with st.container(horizontal=True):
    st.metric("Stocks shown", len(view), border=True)
    st.metric("Best score", f"{df['Score'].max():.2f}", border=True)
    st.metric("Average score", f"{df['Score'].mean():.2f}", border=True)

left, right = st.columns([3, 2])

with left:
    event = st.dataframe(
        view,
        hide_index=True,
        on_select="rerun",
        selection_mode="single-row",
        column_config={
            "Score": st.column_config.ProgressColumn("Score", min_value=0, max_value=100, format="%.2f"),
            "7-day change %": st.column_config.NumberColumn(format="%.2f"),
            "Avg daily volume": st.column_config.NumberColumn(format="%d"),
            "Traded value": st.column_config.NumberColumn(format="%d"),
        },
        alt="Ranked stocks with score, filters passed and 7-day change",
    )

with right:
    selected = event.selection.rows
    if selected:
        symbol = view.iloc[selected[0]]["Symbol"]
    elif len(view):
        symbol = view.iloc[0]["Symbol"]
    else:
        symbol = None

    if symbol:
        stock = details[symbol]
        m = stock["metrics"]
        with st.container(border=True):
            st.subheader(f"{symbol} - {stock['score']:.2f}/100")
            for f in stock["filters"]:
                st.markdown(f"{':green[:material/check:]' if f['passed'] else ':red[:material/close:]'} {f['name']}")
            st.divider()
            st.write(f"Average open: {m['averageOpenPrice']:.2f}")
            st.write(f"Average close: {m['averageClosePrice']:.2f}")
            st.write(f"High / low: {m['highestPrice']:.2f} / {m['lowestPrice']:.2f}")
            st.write(f"Volume consistency: {m['volumeConsistency']:.2f}")
    else:
        st.info("No stocks match the current filters.")
