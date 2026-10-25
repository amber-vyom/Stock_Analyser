import { useEffect, useState } from 'react'

interface ModelResult {
  model: string
  accuracy: number
  precision?: number
  recall?: number
  rocAuc: number
  topPicksAvgReturnPct?: number
  allStocksAvgReturnPct?: number
}

interface MlOutput {
  generatedAt: string
  dataFrom: string
  dataTo: string
  tradingDays: number
  predictionDate: string
  horizonTradingDays: number
  target: string
  universe: string
  split: { trainDates: string; testDates: string; trainRows: number; testRows: number; gapTradingDays: number }
  bestModel: string
  models: ModelResult[]
  baselines: ModelResult[]
  topPicksEdgePct: number
  verdict: string
  picks: { symbol: string; probUp: number; return7dPct: number; price: number }[]
  disclaimer: string
}

interface SentimentItem {
  symbol: string
  headlines: number
  avgCompound: number
  label: 'positive' | 'neutral' | 'negative'
  positive: number
  neutral: number
  negative: number
  emotion: string
  sample: { title: string; compound: number; link: string }[]
}

interface SentimentOutput {
  generatedAt: string
  source: string
  method: string
  stocks: SentimentItem[]
  disclaimer: string
}

async function loadJson<T>(url: string): Promise<T | null> {
  try {
    const res = await fetch(url)
    if (!res.ok) return null
    return (await res.json()) as T
  } catch {
    return null
  }
}

function useJson<T>(url: string, enabled: boolean) {
  const [data, setData] = useState<T | null>(null)
  const [done, setDone] = useState(false)

  useEffect(() => {
    if (!enabled || done) return
    loadJson<T>(url).then((result) => {
      setData(result)
      setDone(true)
    })
  }, [url, enabled, done])

  return { data, done }
}

const pct = (v: number | undefined) => (v === undefined ? '-' : `${v.toFixed(2)}%`)

function MlPanel({ enabled }: { enabled: boolean }) {
  const { data, done } = useJson<MlOutput>('/ml_predictions.json', enabled)

  if (!done) return <div className="state">Loading…</div>
  if (!data) return <div className="state">The ML results have not been generated yet.</div>

  const best = data.models.find((m) => m.model === data.bestModel)

  return (
    <div>
      <p className="muted">
        Trained on {data.tradingDays} trading days ({data.dataFrom} to {data.dataTo}). Goal: {data.target.toLowerCase()}.
        The model learns from the first part of the period and is tested on a later part
        ({data.split.testDates}), with a {data.split.gapTradingDays}-day gap so targets do not overlap.
      </p>

      <p className="banner">{data.verdict}</p>

      <h3>Test results</h3>
      <div className="table-card">
        <table>
          <thead>
            <tr><th>Model</th><th>Accuracy</th><th>ROC-AUC</th><th>Top-20 avg return</th><th>All stocks avg return</th></tr>
          </thead>
          <tbody>
            {data.models.map((m) => (
              <tr key={m.model} className="static-row">
                <td>{m.model}{m.model === data.bestModel ? ' (used for the picks)' : ''}</td>
                <td>{(m.accuracy * 100).toFixed(1)}%</td>
                <td>{m.rocAuc.toFixed(3)}</td>
                <td>{pct(m.topPicksAvgReturnPct)}</td>
                <td>{pct(m.allStocksAvgReturnPct)}</td>
              </tr>
            ))}
            {data.baselines.map((m) => (
              <tr key={m.model} className="static-row muted-row">
                <td>{m.model}</td>
                <td>{(m.accuracy * 100).toFixed(1)}%</td>
                <td>{m.rocAuc.toFixed(3)}</td>
                <td>-</td>
                <td>-</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="muted">
        ROC-AUC of 0.5 means no better than guessing. Top-20 average return is the {data.horizonTradingDays}-day
        return of the model's 20 highest-probability stocks each day{best ? ` (${pct(data.topPicksEdgePct)} versus all stocks)` : ''}.
      </p>

      <h3>Latest picks as of {data.predictionDate}</h3>
      <div className="table-card">
        <table>
          <thead>
            <tr><th>Symbol</th><th>Probability higher in {data.horizonTradingDays} days</th><th>7-day return</th><th>Price</th></tr>
          </thead>
          <tbody>
            {data.picks.slice(0, 10).map((p) => (
              <tr key={p.symbol} className="static-row">
                <td><strong>{p.symbol}</strong></td>
                <td>{(p.probUp * 100).toFixed(1)}%</td>
                <td className={p.return7dPct >= 0 ? 'pos' : 'neg'}>{p.return7dPct.toFixed(2)}%</td>
                <td>{p.price.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="muted">{data.universe}. {data.disclaimer}</p>
    </div>
  )
}

function SentimentPanel({ enabled }: { enabled: boolean }) {
  const { data, done } = useJson<SentimentOutput>('/sentiment.json', enabled)

  if (!done) return <div className="state">Loading…</div>
  if (!data) return <div className="state">The sentiment results have not been generated yet.</div>

  return (
    <div>
      <p className="muted">
        {data.source}. {data.method}. Generated {new Date(data.generatedAt).toLocaleString()}.
      </p>
      <div className="table-card">
        <table>
          <thead>
            <tr><th>Symbol</th><th>Sentiment</th><th>Score</th><th>+ / neutral / -</th><th>Emotion</th><th>Headlines</th></tr>
          </thead>
          <tbody>
            {data.stocks.map((s) => (
              <tr key={s.symbol} className="static-row">
                <td><strong>{s.symbol}</strong></td>
                <td><span className={`tag tag-${s.label}`}>{s.label}</span></td>
                <td>{s.avgCompound.toFixed(2)}</td>
                <td>{s.positive} / {s.neutral} / {s.negative}</td>
                <td>{s.emotion}</td>
                <td>
                  <details>
                    <summary>{s.headlines} headlines</summary>
                    <ul className="headlines">
                      {s.sample.map((h) => (
                        <li key={h.link || h.title}>
                          <a href={h.link} target="_blank" rel="noreferrer">{h.title}</a>
                          <span className="muted"> ({h.compound >= 0 ? '+' : ''}{h.compound.toFixed(2)})</span>
                        </li>
                      ))}
                    </ul>
                  </details>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="muted">{data.disclaimer}</p>
    </div>
  )
}

export default function Experiments() {
  const [tab, setTab] = useState<'ml' | 'sentiment'>('ml')

  return (
    <section id="experiments" className="section">
      <h2>Experiments</h2>
      <p className="section-lead">
        Two extra experiments, kept apart from the ranking above. They do not change any score or rank.
      </p>
      <div className="card">
        <div className="tabs" role="tablist">
          <button role="tab" aria-selected={tab === 'ml'} className={tab === 'ml' ? 'active' : ''} onClick={() => setTab('ml')}>
            ML prediction
          </button>
          <button role="tab" aria-selected={tab === 'sentiment'} className={tab === 'sentiment' ? 'active' : ''} onClick={() => setTab('sentiment')}>
            News sentiment
          </button>
        </div>
        {tab === 'ml' ? <MlPanel enabled /> : <SentimentPanel enabled />}
      </div>
    </section>
  )
}