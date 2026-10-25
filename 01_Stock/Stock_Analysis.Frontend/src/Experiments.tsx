import { useEffect, useMemo, useState } from 'react'
import { Info } from './ui'

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
  dailyTest?: { date: string; picksPct: number; marketPct: number }[]
  featureWeights?: { feature: string; weight: number }[]
  dataStats?: { labelledRows: number; stocks: number; upShare: number; trainUpShare: number; testUpShare: number }
  picks: { symbol: string; probUp: number; return7dPct: number; price: number }[]
  disclaimer: string
}

interface Article {
  title: string
  source?: string
  published?: string
  compound: number
  label?: 'positive' | 'neutral' | 'negative'
  positiveWords?: string[]
  negativeWords?: string[]
  description?: string
  link: string
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
  sample: Article[]
}

interface SentimentOutput {
  generatedAt: string
  source: string
  method: string
  stocks: SentimentItem[]
  disclaimer: string
}

const FEATURE_NAMES: Record<string, string> = {
  ret7: '7-day return',
  ret3: '3-day return',
  ret1: '1-day return',
  close_vs_open: 'Close vs open',
  range7: '7-day price range',
  vol_consistency: 'Volume steadiness',
  log_avg_vol: 'Average volume',
  log_value7: 'Traded value',
}

const cap = (text: string) => (text ? text.charAt(0).toUpperCase() + text.slice(1) : text)
const pct = (v: number | undefined, d = 2) => (v === undefined ? '-' : `${v.toFixed(d)}%`)
const fmtDate = (iso?: string) => {
  if (!iso) return ''
  const d = new Date(iso)
  return Number.isNaN(d.getTime()) ? '' : d.toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' })
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

function useJson<T>(url: string) {
  const [data, setData] = useState<T | null>(null)
  const [done, setDone] = useState(false)

  useEffect(() => {
    let active = true
    loadJson<T>(url).then((result) => {
      if (active) {
        setData(result)
        setDone(true)
      }
    })
    return () => {
      active = false
    }
  }, [url])

  return { data, done }
}

function Gauge({ value, min, max, mark, label }: { value: number; min: number; max: number; mark?: number; label?: string }) {
  const position = (v: number) => `${Math.min(100, Math.max(0, ((v - min) / (max - min)) * 100))}%`
  return (
    <div className="gauge" role="img" aria-label={label}>
      <div className="gauge-fill" style={{ width: position(value) }} />
      {mark !== undefined && <div className="gauge-mark" style={{ left: position(mark) }} />}
    </div>
  )
}

function LineChart({ data }: { data: { date: string; picksPct: number; marketPct: number }[] }) {
  const width = 620
  const height = 200
  const pad = { l: 40, r: 12, t: 12, b: 24 }
  const values = data.flatMap((d) => [d.picksPct, d.marketPct, 0])
  const lo = Math.min(...values)
  const hi = Math.max(...values)
  const span = hi - lo || 1
  const x = (i: number) => pad.l + (i / Math.max(data.length - 1, 1)) * (width - pad.l - pad.r)
  const y = (v: number) => pad.t + (1 - (v - lo) / span) * (height - pad.t - pad.b)
  const line = (key: 'picksPct' | 'marketPct') => data.map((d, i) => `${x(i)},${y(d[key])}`).join(' ')

  return (
    <svg viewBox={`0 0 ${width} ${height}`} className="linechart" role="img" aria-label="Average 5-day return of the model's top picks versus all stocks, by test day">
      <line x1={pad.l} x2={width - pad.r} y1={y(0)} y2={y(0)} className="zero-line" />
      <text x={pad.l - 6} y={y(hi) + 4} textAnchor="end" className="axis">{hi.toFixed(1)}%</text>
      <text x={pad.l - 6} y={y(0) + 4} textAnchor="end" className="axis">0</text>
      <text x={pad.l - 6} y={y(lo) + 4} textAnchor="end" className="axis">{lo.toFixed(1)}%</text>
      <polyline points={line('marketPct')} className="series-market" />
      <polyline points={line('picksPct')} className="series-picks" />
      <text x={pad.l} y={height - 6} className="axis">{data[0]?.date}</text>
      <text x={width - pad.r} y={height - 6} textAnchor="end" className="axis">{data[data.length - 1]?.date}</text>
    </svg>
  )
}

function MlPanel() {
  const { data, done } = useJson<MlOutput>('/ml_predictions.json')
  const [showAll, setShowAll] = useState(false)

  if (!done) return <div className="state">Loading…</div>
  if (!data) return <div className="state">The ML results have not been generated yet.</div>

  const best = data.models.find((m) => m.model === data.bestModel) ?? data.models[0]
  const majority = data.baselines[0]
  const weights = data.featureWeights ?? []
  const maxWeight = Math.max(...weights.map((w) => Math.abs(w.weight)), 0.001)
  const picks = showAll ? data.picks : data.picks.slice(0, 10)
  const allResults = [...data.models, ...data.baselines]

  return (
    <div className="exp-body">
      <p className="verdict" role="status"><strong>Honest result:</strong> {data.verdict}</p>

      <div className="mini-stats">
        <div className="mini-stat">
          <span>ROC-AUC <Info text="0.5 means no better than guessing; 1.0 is perfect. Anything near 0.5 is basically random." /></span>
          <strong>{best.rocAuc.toFixed(3)}</strong>
          <Gauge value={best.rocAuc} min={0.4} max={0.8} mark={0.5} label="ROC-AUC gauge with a marker at 0.5" />
        </div>
        <div className="mini-stat">
          <span>Accuracy <Info text="Share of test cases where the up/down call was right. Compare with the 'always guess the majority' baseline." /></span>
          <strong>{(best.accuracy * 100).toFixed(1)}%</strong>
          <Gauge value={best.accuracy} min={0.4} max={0.8} mark={majority.accuracy} label="Accuracy gauge with the baseline marked" />
          <em>Baseline {(majority.accuracy * 100).toFixed(1)}%</em>
        </div>
        <div className="mini-stat">
          <span>Top-20 average return <Info text={`Average ${data.horizonTradingDays}-day return of the 20 stocks the model liked most each day, versus all stocks.`} /></span>
          <strong className={(best.topPicksAvgReturnPct ?? 0) >= 0 ? 'pos' : 'neg'}>{pct(best.topPicksAvgReturnPct)}</strong>
          <em>All stocks {pct(best.allStocksAvgReturnPct)}</em>
        </div>
        <div className="mini-stat">
          <span>Data used</span>
          <strong>{data.tradingDays} days</strong>
          <em>{data.dataFrom} to {data.dataTo}{data.dataStats ? ` · ${data.dataStats.stocks.toLocaleString()} stocks` : ''}</em>
        </div>
      </div>

      <div className="exp-grid">
        <div className="exp-card">
          <h4>Models and baselines</h4>
          <ul className="compare">
            {allResults.map((m) => (
              <li key={m.model}>
                <div className="compare-head">
                  <span>{cap(m.model.replace('Baseline: ', ''))}{m.model === data.bestModel ? ' ★' : ''}</span>
                  <span>{(m.accuracy * 100).toFixed(1)}% · AUC {m.rocAuc.toFixed(2)}</span>
                </div>
                <Gauge value={m.accuracy} min={0.4} max={0.8} mark={majority.accuracy} label={`${m.model} accuracy`} />
              </li>
            ))}
          </ul>
          <p className="muted">★ used for the picks. The marker shows the majority-class baseline.</p>
        </div>

        {weights.length > 0 && (
          <div className="exp-card">
            <h4>What the model looks at <Info text="Standardised logistic-regression weights. Positive pushes the prediction up, negative pushes it down. Bigger bars matter more to the model." /></h4>
            <ul className="weights">
              {weights.map((w) => (
                <li key={w.feature}>
                  <span>{FEATURE_NAMES[w.feature] ?? w.feature}</span>
                  <div className="weight-track">
                    <div
                      className={`weight-bar ${w.weight >= 0 ? 'up' : 'down'}`}
                      style={{ width: `${(Math.abs(w.weight) / maxWeight) * 50}%`, [w.weight >= 0 ? 'left' : 'right']: '50%' }}
                    />
                  </div>
                  <span className={w.weight >= 0 ? 'pos' : 'neg'}>{w.weight > 0 ? '+' : ''}{w.weight.toFixed(2)}</span>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {data.dailyTest && data.dailyTest.length > 1 && (
        <div className="exp-card">
          <h4>Test period: model's top picks vs all stocks <Info text="For each test day: the average 5-day return of the top-20 picks (bright line) against the average of every stock (dim line)." /></h4>
          <LineChart data={data.dailyTest} />
          <div className="legend"><span className="dot picks" /> Top-20 picks <span className="dot market" /> All stocks</div>
        </div>
      )}

      <div className="exp-card">
        <h4>Latest picks as of {data.predictionDate}</h4>
        <div className="table-card">
          <table>
            <thead>
              <tr><th>Symbol</th><th>Chance of being higher in {data.horizonTradingDays} days</th><th>7-day return</th><th>Price</th></tr>
            </thead>
            <tbody>
              {picks.map((p) => (
                <tr key={p.symbol} className="static-row">
                  <td><strong>{p.symbol}</strong></td>
                  <td>
                    {(p.probUp * 100).toFixed(1)}%
                    <div className="bar"><div style={{ width: `${p.probUp * 100}%` }} /></div>
                  </td>
                  <td className={p.return7dPct >= 0 ? 'pos' : 'neg'}>{p.return7dPct.toFixed(2)}%</td>
                  <td>{p.price.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {data.picks.length > 10 && (
          <button className="link-btn" onClick={() => setShowAll(!showAll)}>
            {showAll ? 'Show fewer' : `Show all ${data.picks.length}`}
          </button>
        )}
      </div>

      <details className="method">
        <summary>How this experiment was done</summary>
        <ol>
          <li>About five months of NSE reports were downloaded ({data.tradingDays} trading days).</li>
          <li>For each stock and day, the same seven-day numbers used in the ranking became inputs.</li>
          <li>Goal: {data.target.toLowerCase()}.</li>
          <li>
            The model learned from {data.split.trainDates} and was tested on {data.split.testDates}, with a
            {' '}{data.split.gapTradingDays}-day gap so the two periods do not overlap.
          </li>
          <li>{data.universe}.</li>
          <li>Results are compared with simple baselines. The model is a logistic regression written in NumPy.</li>
        </ol>
      </details>
      <p className="muted">{data.disclaimer}</p>
    </div>
  )
}

type Filter = 'all' | 'positive' | 'neutral' | 'negative'
type SortBy = 'score' | 'headlines' | 'symbol'

function SentimentPanel() {
  const { data, done } = useJson<SentimentOutput>('/sentiment.json')
  const [filter, setFilter] = useState<Filter>('all')
  const [sortBy, setSortBy] = useState<SortBy>('score')

  const stocks = useMemo(() => data?.stocks ?? [], [data])
  const summary = useMemo(() => {
    const totals = { positive: 0, neutral: 0, negative: 0 }
    const emotions: Record<string, number> = {}
    stocks.forEach((s) => {
      totals.positive += s.positive
      totals.neutral += s.neutral
      totals.negative += s.negative
      emotions[s.emotion] = (emotions[s.emotion] ?? 0) + 1
    })
    const all = totals.positive + totals.neutral + totals.negative || 1
    const avg = stocks.length ? stocks.reduce((t, s) => t + s.avgCompound, 0) / stocks.length : 0
    const topEmotion = Object.entries(emotions).sort((a, b) => b[1] - a[1])[0]?.[0] ?? 'neutral'
    return { totals, all, avg, topEmotion }
  }, [stocks])

  const shown = useMemo(() => {
    const list = stocks.filter((s) => filter === 'all' || s.label === filter)
    return [...list].sort((a, b) => {
      if (sortBy === 'symbol') return a.symbol.localeCompare(b.symbol)
      if (sortBy === 'headlines') return b.headlines - a.headlines
      return b.avgCompound - a.avgCompound
    })
  }, [stocks, filter, sortBy])

  if (!done) return <div className="state">Loading…</div>
  if (!data) return <div className="state">The sentiment results have not been generated yet.</div>

  return (
    <div className="exp-body">
      <p className="muted">
        {data.source}. {data.method}. Generated {new Date(data.generatedAt).toLocaleString()}.
      </p>

      <div className="mini-stats">
        <div className="mini-stat">
          <span>Stocks analysed</span>
          <strong>{stocks.length}</strong>
          <em>{summary.all} headlines read</em>
        </div>
        <div className="mini-stat">
          <span>Average sentiment <Info text="From -1 (very negative) to +1 (very positive), averaged over the stocks." /></span>
          <strong className={summary.avg >= 0 ? 'pos' : 'neg'}>{summary.avg >= 0 ? '+' : ''}{summary.avg.toFixed(2)}</strong>
          <Gauge value={summary.avg} min={-1} max={1} mark={0} label="Average sentiment gauge" />
        </div>
        <div className="mini-stat">
          <span>Most common emotion <Info text="A crude keyword count: optimism, fear, anger or uncertainty." /></span>
          <strong>{cap(summary.topEmotion)}</strong>
        </div>
        <div className="mini-stat wide">
          <span>Headline mix</span>
          <div className="stack" role="img" aria-label={`${summary.totals.positive} positive, ${summary.totals.neutral} neutral, ${summary.totals.negative} negative headlines`}>
            <div className="seg positive" style={{ width: `${(summary.totals.positive / summary.all) * 100}%` }} />
            <div className="seg neutral" style={{ width: `${(summary.totals.neutral / summary.all) * 100}%` }} />
            <div className="seg negative" style={{ width: `${(summary.totals.negative / summary.all) * 100}%` }} />
          </div>
          <em>
            Positive {summary.totals.positive} · Neutral {summary.totals.neutral} · Negative {summary.totals.negative}
          </em>
        </div>
      </div>

      <div className="toolbar">
        {(['all', 'positive', 'neutral', 'negative'] as Filter[]).map((f) => (
          <button key={f} className={`chip-btn ${filter === f ? 'active' : ''}`} onClick={() => setFilter(f)} aria-pressed={filter === f}>
            {cap(f)}
          </button>
        ))}
        <select aria-label="Sort stocks by" value={sortBy} onChange={(e) => setSortBy(e.target.value as SortBy)}>
          <option value="score">Sort: Sentiment score</option>
          <option value="headlines">Sort: Number of articles</option>
          <option value="symbol">Sort: Symbol A-Z</option>
        </select>
        <span className="muted">Showing {shown.length} of {stocks.length}</span>
      </div>

      <div className="news-grid">
        {shown.map((s) => (
          <article key={s.symbol} className="news-card">
            <header>
              <strong>{s.symbol}</strong>
              <span className={`tag tag-${s.label}`}>{cap(s.label)}</span>
            </header>
            <Gauge value={s.avgCompound} min={-1} max={1} mark={0} label={`${s.symbol} sentiment ${s.avgCompound.toFixed(2)}`} />
            <div className="news-meta">
              <span>Score {s.avgCompound >= 0 ? '+' : ''}{s.avgCompound.toFixed(2)}</span>
              <span>Emotion: <strong>{cap(s.emotion)}</strong></span>
            </div>
            <div className="news-meta muted">
              <span>{s.positive} positive · {s.neutral} neutral · {s.negative} negative</span>
            </div>
            <details>
              <summary>Articles ({s.headlines})</summary>
              <ul className="articles">
                {s.sample.map((a) => (
                  <li key={a.link || a.title}>
                    <a href={a.link} target="_blank" rel="noreferrer">{a.title}</a>
                    <div className="article-meta">
                      {a.source && <span>{a.source}</span>}
                      {fmtDate(a.published) && <span>{fmtDate(a.published)}</span>}
                      <span className={`tag tag-${a.label ?? 'neutral'}`}>
                        {cap(a.label ?? 'neutral')} {a.compound >= 0 ? '+' : ''}{a.compound.toFixed(2)}
                      </span>
                    </div>
                    {a.description && <p className="article-desc">{a.description}</p>}
                  </li>
                ))}
              </ul>
              {s.headlines > s.sample.length && (
                <p className="muted">Showing {s.sample.length} of {s.headlines} articles.</p>
              )}
            </details>
          </article>
        ))}
        {shown.length === 0 && <div className="state">No stocks match this filter.</div>}
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
        {tab === 'ml' ? <MlPanel /> : <SentimentPanel />}
      </div>
    </section>
  )
}
