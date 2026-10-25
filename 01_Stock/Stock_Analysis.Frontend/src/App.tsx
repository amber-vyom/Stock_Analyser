import { useCallback, useEffect, useMemo, useState } from 'react'
import './App.css'
import Experiments from './Experiments'
import { Background, CountUp, Info, Reveal, ScoreRing } from './ui'

interface Metrics {
  symbol: string
  tradingDays: number
  totalVolume: number
  averageDailyVolume: number
  averageOpenPrice: number
  averageClosePrice: number
  highestPrice: number
  lowestPrice: number
  totalTrades: number
  totalTradedValue: number
  priceChangePercentage: number
  volumeConsistency: number
}

interface Stock {
  rank: number
  symbol: string
  score: number
  passedFilters: number
  filters: { name: string; passed: boolean }[]
  breakdown: { name: string; points: number; max: number }[]
  metrics: Metrics
}

interface UpdateSummary {
  downloads: { date: string; status: string }[]
  csvFilesUsed: number
  eqRecords: number
  rawInserted: number
  rawSkipped: number
  processedSaved: number
}

type SortKey = 'rank' | 'symbol' | 'score' | 'filters' | 'change' | 'volume'
type Theme = 'light' | 'dark'

const STATIC = import.meta.env.VITE_STATIC === 'true'
const REPO_URL = 'https://github.com/amber-vyom/Stock_Analyser'

const SECTIONS = [
  { id: 'overview', label: 'Overview' },
  { id: 'rankings', label: 'Rankings' },
  { id: 'insights', label: 'Insights' },
  { id: 'how', label: 'How it works' },
  { id: 'experiments', label: 'Experiments' },
  { id: 'team', label: 'Team' },
]

const CONTRIBUTORS = [
  { name: 'Vyom Gupta', regNo: '25MIS0494' },
  { name: 'Y Pooja Reddy', regNo: '25MIS0194' },
]

const ACCENTS = [
  { id: 'blue', label: 'Blue' },
  { id: 'emerald', label: 'Emerald' },
  { id: 'rose', label: 'Rose' },
  { id: 'amber', label: 'Amber' },
] as const

type Accent = (typeof ACCENTS)[number]['id']

function initialAccent(): Accent {
  try {
    const saved = localStorage.getItem('accent')
    if (ACCENTS.some((a) => a.id === saved)) return saved as Accent
  } catch {
    // storage may be unavailable
  }
  return 'blue'
}

const FILTER_GUIDE = [
  ['Completeness', 'The stock traded on all 7 days of the window.'],
  ['Positive return', 'Price at the end is higher than at the start.'],
  ['Strong return', 'Price rose at least 3%.'],
  ['Close above open', 'On average the stock closed higher than it opened.'],
  ['Price stability', 'The gap between the highest and lowest price was 15% or less.'],
  ['Volume activity', 'At least 10 lakh shares traded per day on average.'],
  ['Volume consistency', 'Daily trading volume was fairly steady (score of 60 or more out of 100).'],
  ['Strong momentum', 'Price rose at least 5%.'],
  ['High traded value', 'At least ₹1 crore of shares changed hands in total.'],
  ['Overall quality', 'Rose 2%+, steady volume (50+) and at least 5 lakh shares per day.'],
]

const num = (v: number, d = 2) => v.toLocaleString('en-IN', { maximumFractionDigits: d })
const crore = (v: number) => `₹${num(v / 1e7, 0)} crore`

async function getJson<T>(url: string): Promise<T> {
  const res = await fetch(url)
  if (!res.ok) {
    const body = await res.json().catch(() => null)
    throw new Error(body?.error ?? `Request failed (${res.status})`)
  }
  return res.json() as Promise<T>
}

function sortValue(s: Stock, key: SortKey): number | string {
  switch (key) {
    case 'rank': return s.rank
    case 'symbol': return s.symbol
    case 'score': return s.score
    case 'filters': return s.passedFilters
    case 'change': return s.metrics.priceChangePercentage
    case 'volume': return s.metrics.averageDailyVolume
  }
}

function summarise(s: Stock): string {
  const m = s.metrics
  const direction = m.priceChangePercentage >= 0 ? 'rose' : 'fell'
  const volume = m.volumeConsistency >= 70 ? 'steady' : m.volumeConsistency >= 50 ? 'fairly steady' : 'uneven'
  return (
    `${s.symbol} passed ${s.passedFilters} of 10 checks. Its price ${direction} ` +
    `${Math.abs(m.priceChangePercentage).toFixed(1)}% over the window, daily trading volume was ${volume}, ` +
    `and about ${crore(m.totalTradedValue)} of shares changed hands.`
  )
}

function bucketOf(score: number) {
  return Math.floor(score / 5) * 5
}

function initialTheme(): Theme {
  try {
    const saved = localStorage.getItem('theme')
    if (saved === 'light' || saved === 'dark') return saved
  } catch {
    // storage may be unavailable
  }
  return window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark'
}

function Histogram({
  stocks,
  active,
  onPick,
}: {
  stocks: Stock[]
  active: number | null
  onPick: (bucket: number | null) => void
}) {
  const buckets = useMemo(() => {
    const lo = bucketOf(Math.min(...stocks.map((s) => s.score)))
    const hi = bucketOf(Math.max(...stocks.map((s) => s.score)))
    const result: { start: number; count: number }[] = []
    for (let start = lo; start <= hi; start += 5) {
      result.push({ start, count: stocks.filter((s) => bucketOf(s.score) === start).length })
    }
    return result
  }, [stocks])

  const max = Math.max(...buckets.map((b) => b.count), 1)

  return (
    <div className="histogram">
      {buckets.map((b) => (
        <button
          key={b.start}
          className={`hist-col ${active === b.start ? 'active' : ''}`}
          onClick={() => onPick(active === b.start ? null : b.start)}
          aria-pressed={active === b.start}
          aria-label={`Scores ${b.start} to ${b.start + 5}: ${b.count} stocks`}
        >
          <span className="hist-count">{b.count}</span>
          <span className="hist-bar" style={{ height: `${(b.count / max) * 100}%` }} />
          <span className="hist-label">{b.start}-{b.start + 5}</span>
        </button>
      ))}
    </div>
  )
}

export default function App() {
  const [stocks, setStocks] = useState<Stock[]>([])
  const [count, setCount] = useState(25)
  const [search, setSearch] = useState('')
  const [minFilters, setMinFilters] = useState(0)
  const [bucket, setBucket] = useState<number | null>(null)
  const [sortKey, setSortKey] = useState<SortKey>('rank')
  const [sortAsc, setSortAsc] = useState(true)
  const [selected, setSelected] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [snapshotDate, setSnapshotDate] = useState<string | null>(null)
  const [updating, setUpdating] = useState(false)
  const [updateMsg, setUpdateMsg] = useState<string | null>(null)
  const [theme, setTheme] = useState<Theme>(initialTheme)
  const [activeSection, setActiveSection] = useState('overview')
  const [accent, setAccent] = useState<Accent>(initialAccent)

  useEffect(() => {
    document.documentElement.dataset.accent = accent
    try {
      localStorage.setItem('accent', accent)
    } catch {
      // ignore
    }
  }, [accent])

  useEffect(() => {
    document.documentElement.dataset.theme = theme
    try {
      localStorage.setItem('theme', theme)
    } catch {
      // ignore
    }
  }, [theme])

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      let data: Stock[]
      if (STATIC) {
        const snapshot = await getJson<{ generatedAt: string; stocks: Stock[] }>('/snapshot.json')
        data = snapshot.stocks.slice(0, count)
        setSnapshotDate(snapshot.generatedAt)
      } else {
        data = await getJson<Stock[]>(`/api/stocks/top?count=${count}`)
      }
      setStocks(data)
      setSelected((current) => (current && data.some((s) => s.symbol === current) ? current : data[0]?.symbol ?? null))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unknown error')
    } finally {
      setLoading(false)
    }
  }, [count])

  useEffect(() => {
    load()
  }, [load])

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries.filter((e) => e.isIntersecting).sort((a, b) => b.intersectionRatio - a.intersectionRatio)
        if (visible[0]) setActiveSection(visible[0].target.id)
      },
      { rootMargin: '-30% 0px -55% 0px', threshold: [0, 0.2, 0.5] },
    )
    SECTIONS.forEach((s) => {
      const node = document.getElementById(s.id)
      if (node) observer.observe(node)
    })
    return () => observer.disconnect()
  }, [loading, error, stocks.length])

  const updateData = async () => {
    setUpdating(true)
    setUpdateMsg(null)
    try {
      const res = await fetch('/api/pipeline/run', { method: 'POST' })
      const body = await res.json().catch(() => null)
      if (!res.ok) throw new Error(body?.error ?? `Request failed (${res.status})`)
      const s = body as UpdateSummary
      const ok = s.downloads.filter((d) => d.status === 'Downloaded').length
      setUpdateMsg(
        `Downloaded ${ok} reports, ${s.eqRecords.toLocaleString()} EQ records ` +
          `(${s.rawInserted.toLocaleString()} new), ${s.processedSaved.toLocaleString()} stocks refreshed.`,
      )
      await load()
    } catch (e) {
      setUpdateMsg(`Update failed: ${e instanceof Error ? e.message : 'Unknown error'}`)
    } finally {
      setUpdating(false)
    }
  }

  const changeSort = (key: SortKey) => {
    if (key === sortKey) {
      setSortAsc(!sortAsc)
    } else {
      setSortKey(key)
      setSortAsc(key === 'rank' || key === 'symbol')
    }
  }

  const goTo = (id: string) => document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' })

  const pickStock = (symbol: string, scroll = false) => {
    setSelected(symbol)
    if (scroll) goTo('rankings')
  }

  const visible = useMemo(() => {
    const term = search.trim().toLowerCase()
    const filtered = stocks.filter(
      (s) =>
        s.symbol.toLowerCase().includes(term) &&
        s.passedFilters >= minFilters &&
        (bucket === null || bucketOf(s.score) === bucket),
    )
    return [...filtered].sort((a, b) => {
      const x = sortValue(a, sortKey)
      const y = sortValue(b, sortKey)
      const order = typeof x === 'string' ? x.localeCompare(y as string) : (x as number) - (y as number)
      return sortAsc ? order : -order
    })
  }, [stocks, search, minFilters, bucket, sortKey, sortAsc])

  const current = stocks.find((s) => s.symbol === selected)
  const avgScore = stocks.length ? stocks.reduce((t, s) => t + s.score, 0) / stocks.length : 0
  const avgChange = stocks.length
    ? stocks.reduce((t, s) => t + s.metrics.priceChangePercentage, 0) / stocks.length
    : 0
  const gainers = [...stocks].sort((a, b) => b.metrics.priceChangePercentage - a.metrics.priceChangePercentage).slice(0, 5)
  const mostTraded = [...stocks].sort((a, b) => b.metrics.totalTradedValue - a.metrics.totalTradedValue).slice(0, 5)
  const podium = stocks.slice(0, 3)
  const ready = !loading && !error && stocks.length > 0

  const header = (key: SortKey, label: string, hint?: string) => (
    <th aria-sort={sortKey === key ? (sortAsc ? 'ascending' : 'descending') : 'none'}>
      <button className="sort" onClick={() => changeSort(key)}>
        {label}
        {sortKey === key ? (sortAsc ? ' ▲' : ' ▼') : ''}
      </button>
      {hint && <Info text={hint} />}
    </th>
  )

  return (
    <>
      <Background />

      <nav className="nav">
        <div className="nav-inner">
          <a className="brand" href="#overview" onClick={(e) => { e.preventDefault(); goTo('overview') }}>
            <span className="brand-mark">◈</span> Stock Analysis
          </a>
          <div className="nav-links" role="navigation" aria-label="Sections">
            {SECTIONS.map((s) => (
              <a
                key={s.id}
                href={`#${s.id}`}
                className={activeSection === s.id ? 'active' : ''}
                onClick={(e) => { e.preventDefault(); goTo(s.id) }}
              >
                {s.label}
              </a>
            ))}
          </div>
          <div className="nav-actions">
            <div className="swatches" role="group" aria-label="Accent colour">
              {ACCENTS.map((a) => (
                <button
                  key={a.id}
                  className={`swatch swatch-${a.id} ${accent === a.id ? 'on' : ''}`}
                  onClick={() => setAccent(a.id)}
                  aria-label={`${a.label} accent`}
                  aria-pressed={accent === a.id}
                  title={`${a.label} accent`}
                />
              ))}
            </div>
            <span className={`badge ${STATIC ? 'badge-snapshot' : 'badge-live'}`}>{STATIC ? 'Snapshot' : 'Live'}</span>
            <button
              className="icon-btn"
              onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
              aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} theme`}
              title="Toggle theme"
            >
              {theme === 'dark' ? '☀' : '☾'}
            </button>
          </div>
        </div>
      </nav>

      <main className="page">
        <section id="overview" className="section hero">
          <p className="eyebrow">NSE equity screener · rolling 7-trading-day window</p>
          <h1>Which stocks looked strongest over the last 7 trading days?</h1>
          <p className="lead">
            We download official NSE market data, run every stock through ten simple checks, give each one a score out
            of 100, and rank them. Click any stock to see exactly why it scored the way it did.
          </p>
          <div className="hero-actions">
            <button className="primary" onClick={() => goTo('rankings')}>See the rankings</button>
            <button onClick={() => goTo('how')}>How does it work?</button>
            {!STATIC && (
              <button onClick={updateData} disabled={updating || loading}>
                {updating ? 'Updating… (can take a few minutes)' : 'Update data from NSE'}
              </button>
            )}
          </div>
          {updateMsg && <p className="banner">{updateMsg}</p>}
          {STATIC && snapshotDate && (
            <p className="banner">
              You are viewing a saved snapshot from {new Date(snapshotDate).toLocaleString()}. Live updates only work
              when the API runs on your own computer.
            </p>
          )}

          {loading && <div className="state">Loading data…</div>}
          {error && (
            <div className="state err" role="alert">
              Could not load stocks: {error}
              <div><button onClick={load}>Try again</button></div>
            </div>
          )}
          {!loading && !error && stocks.length === 0 && (
            <div className="state">
              No ranked stocks yet. {STATIC ? 'The snapshot is empty.' : 'Click "Update data from NSE" to load the latest reports.'}
            </div>
          )}

          {ready && (
            <>
              <div className="stats">
                <div className="stat">
                  <span>Stocks ranked<Info text="Stocks that traded on all 7 days of the window." /></span>
                  <strong><CountUp value={stocks.length} /></strong>
                </div>
                <div className="stat">
                  <span>Best score<Info text="Out of 100. Higher means the stock did better on more of the checks." /></span>
                  <strong><CountUp value={stocks[0].score} decimals={2} /></strong>
                  <em>{stocks[0].symbol}</em>
                </div>
                <div className="stat">
                  <span>Average score</span>
                  <strong><CountUp value={avgScore} decimals={2} /></strong>
                </div>
                <div className="stat">
                  <span>Average 7-day change<Info text="How far the price moved from the first day's open to the last day's close, averaged over these stocks." /></span>
                  <strong className={avgChange >= 0 ? 'pos' : 'neg'}><CountUp value={avgChange} decimals={2} suffix="%" /></strong>
                </div>
              </div>

              <h2 className="podium-title">Top 3 right now</h2>
              <div className="podium">
                {podium.map((s, i) => (
                  <button key={s.symbol} className="podium-card" onClick={() => pickStock(s.symbol, true)}>
                    <span className="medal">{['1st', '2nd', '3rd'][i]}</span>
                    <ScoreRing score={s.score} />
                    <strong>{s.symbol}</strong>
                    <span className={s.metrics.priceChangePercentage >= 0 ? 'pos' : 'neg'}>
                      {s.metrics.priceChangePercentage >= 0 ? '▲' : '▼'} {num(Math.abs(s.metrics.priceChangePercentage))}% in 7 days
                    </span>
                    <span className="muted">{s.passedFilters}/10 checks passed</span>
                  </button>
                ))}
              </div>
            </>
          )}
        </section>

        {ready && (
          <>
            <Reveal>
              <section id="rankings" className="section">
                <h2>Rankings</h2>
                <p className="section-lead">
                  Every stock, ranked by score. Click a column title to sort, click a row to see its details on the right.
                </p>

                <div className="toolbar">
                  <input
                    placeholder="Search symbol, e.g. HFCL"
                    aria-label="Search symbol"
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                  />
                  <select aria-label="Number of stocks" value={count} onChange={(e) => setCount(Number(e.target.value))}>
                    {[10, 25, 50, 100, 200].map((n) => (
                      <option key={n} value={n}>Top {n}</option>
                    ))}
                  </select>
                  <select aria-label="Minimum checks passed" value={minFilters} onChange={(e) => setMinFilters(Number(e.target.value))}>
                    {[0, 5, 7, 8, 9, 10].map((n) => (
                      <option key={n} value={n}>{n === 0 ? 'Any number of checks' : `${n}+ checks passed`}</option>
                    ))}
                  </select>
                  {bucket !== null && (
                    <button className="chip" onClick={() => setBucket(null)}>
                      Score {bucket}-{bucket + 5} ✕
                    </button>
                  )}
                  <button onClick={load} disabled={loading || updating}>Refresh</button>
                  <span className="muted">Showing {visible.length} of {stocks.length}</span>
                </div>

                <div className="layout">
                  <div className="card table-card">
                    <table>
                      <thead>
                        <tr>
                          {header('rank', '#')}
                          {header('symbol', 'Symbol')}
                          {header('score', 'Score', 'Score out of 100 from ten weighted components.')}
                          {header('filters', 'Checks', 'How many of the ten pass/fail checks the stock passed.')}
                          {header('change', '7-day change', 'Price change from the first open to the last close.')}
                          {header('volume', 'Avg volume', 'Average number of shares traded per day.')}
                          <th>Days</th>
                        </tr>
                      </thead>
                      <tbody>
                        {visible.map((s) => (
                          <tr
                            key={s.symbol}
                            className={s.symbol === selected ? 'sel' : ''}
                            tabIndex={0}
                            onClick={() => setSelected(s.symbol)}
                            onKeyDown={(e) => {
                              if (e.key === 'Enter' || e.key === ' ') {
                                e.preventDefault()
                                setSelected(s.symbol)
                              }
                            }}
                          >
                            <td>{s.rank}</td>
                            <td><strong>{s.symbol}</strong></td>
                            <td>
                              {s.score.toFixed(2)}
                              <div className="bar"><div style={{ width: `${s.score}%` }} /></div>
                            </td>
                            <td>{s.passedFilters}/10</td>
                            <td className={s.metrics.priceChangePercentage >= 0 ? 'pos' : 'neg'}>
                              {s.metrics.priceChangePercentage >= 0 ? '▲' : '▼'} {num(Math.abs(s.metrics.priceChangePercentage))}%
                            </td>
                            <td>{num(s.metrics.averageDailyVolume, 0)}</td>
                            <td>{s.metrics.tradingDays}</td>
                          </tr>
                        ))}
                        {visible.length === 0 && (
                          <tr><td colSpan={7} className="state">No stocks match these filters.</td></tr>
                        )}
                      </tbody>
                    </table>
                  </div>

                  <aside className="side">
                    {current && (
                      <div className="card detail" key={current.symbol}>
                        <div className="detail-head">
                          <div>
                            <h3 className="detail-title">{current.symbol}</h3>
                            <span className="muted">Rank {current.rank} of {stocks.length}</span>
                          </div>
                          <ScoreRing score={current.score} size={92} />
                        </div>
                        <p className="plain">{summarise(current)}</p>

                        <h4>Checks ({current.passedFilters}/10)</h4>
                        <ul className="filters">
                          {current.filters.map((f) => (
                            <li key={f.name} className={f.passed ? 'pos' : 'neg'}>
                              {f.passed ? '✓' : '✗'} {f.name}
                            </li>
                          ))}
                        </ul>

                        <h4>Where the score comes from</h4>
                        <ul className="breakdown">
                          {current.breakdown.map((b) => (
                            <li key={b.name}>
                              <span>{b.name}</span>
                              <span>{b.points.toFixed(1)} / {b.max}</span>
                              <div className="bar"><div style={{ width: `${b.max > 0 ? (b.points / b.max) * 100 : 0}%` }} /></div>
                            </li>
                          ))}
                        </ul>

                        <h4>Seven-day numbers</h4>
                        <dl>
                          <dt>Trading days</dt><dd>{current.metrics.tradingDays}</dd>
                          <dt>Avg open</dt><dd>{num(current.metrics.averageOpenPrice)}</dd>
                          <dt>Avg close</dt><dd>{num(current.metrics.averageClosePrice)}</dd>
                          <dt>High / Low</dt><dd>{num(current.metrics.highestPrice)} / {num(current.metrics.lowestPrice)}</dd>
                          <dt>Avg daily volume</dt><dd>{num(current.metrics.averageDailyVolume, 0)}</dd>
                          <dt>Total trades</dt><dd>{num(current.metrics.totalTrades, 0)}</dd>
                          <dt>Traded value</dt><dd>{crore(current.metrics.totalTradedValue)}</dd>
                          <dt>Volume steadiness</dt><dd>{num(current.metrics.volumeConsistency, 0)} / 100</dd>
                        </dl>
                      </div>
                    )}
                  </aside>
                </div>
              </section>
            </Reveal>

            <Reveal>
              <section id="insights" className="section">
                <h2>Insights</h2>
                <p className="section-lead">A quick look at the stocks above. Click a score bar to filter the rankings to that range.</p>
                <div className="insights">
                  <div className="card">
                    <h3>Score distribution</h3>
                    <Histogram
                      stocks={stocks}
                      active={bucket}
                      onPick={(b) => {
                        setBucket(b)
                        if (b !== null) goTo('rankings')
                      }}
                    />
                  </div>
                  <div className="card">
                    <h3>Biggest 7-day gainers</h3>
                    <ul className="mini-list">
                      {gainers.map((s) => (
                        <li key={s.symbol}>
                          <button onClick={() => pickStock(s.symbol, true)}>{s.symbol}</button>
                          <span className={s.metrics.priceChangePercentage >= 0 ? 'pos' : 'neg'}>
                            {s.metrics.priceChangePercentage >= 0 ? '+' : ''}{num(s.metrics.priceChangePercentage)}%
                          </span>
                        </li>
                      ))}
                    </ul>
                  </div>
                  <div className="card">
                    <h3>Most traded (by value)</h3>
                    <ul className="mini-list">
                      {mostTraded.map((s) => (
                        <li key={s.symbol}>
                          <button onClick={() => pickStock(s.symbol, true)}>{s.symbol}</button>
                          <span>{crore(s.metrics.totalTradedValue)}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                </div>
              </section>
            </Reveal>
          </>
        )}

        <Reveal>
          <section id="how" className="section">
            <h2>How it works</h2>
            <p className="section-lead">From raw exchange files to a ranked list, in five steps.</p>
            <ol className="steps">
              <li><span>1</span><div><strong>Download</strong><p>The latest 7 trading days of NSE BhavCopy reports. Holidays are skipped automatically.</p></div></li>
              <li><span>2</span><div><strong>Group</strong><p>Only normal equity (EQ) trades are kept and grouped by stock symbol using a hash table.</p></div></li>
              <li><span>3</span><div><strong>Check</strong><p>Each stock faces ten pass/fail checks, listed below.</p></div></li>
              <li><span>4</span><div><strong>Score</strong><p>A weighted score out of 100 is built from the same numbers, so two stocks that pass the same checks can still score differently.</p></div></li>
              <li><span>5</span><div><strong>Rank</strong><p>A priority queue (heap) pulls out the best stocks: highest score first, then most checks passed, then symbol. Stocks missing days are not ranked.</p></div></li>
            </ol>

            <h3 className="guide-title">The ten checks in plain words</h3>
            <div className="guide">
              {FILTER_GUIDE.map(([name, text], i) => (
                <div key={name} className="guide-item">
                  <span className="guide-n">{i + 1}</span>
                  <div><strong>{name}</strong><p>{text}</p></div>
                </div>
              ))}
            </div>
            <p className="muted">
              This is a rule-based screening tool built for learning. The score has not been tested against future
              prices, so it is not a prediction and not investment advice.{' '}
              <a href={REPO_URL} target="_blank" rel="noreferrer">View the source code</a>
            </p>
          </section>
        </Reveal>

        {ready && (
          <Reveal>
            <Experiments />
          </Reveal>
        )}
        <Reveal>
          <section id="team" className="section">
            <h2>Team</h2>
            <p className="section-lead">This project was designed and built by:</p>
            <div className="team">
              {CONTRIBUTORS.map((c) => (
                <div key={c.regNo} className="card member">
                  <span className="avatar" aria-hidden="true">
                    {c.name.split(' ').map((w) => w[0]).slice(0, 2).join('')}
                  </span>
                  <div>
                    <strong>{c.name}</strong>
                    <p className="muted">Reg. no. {c.regNo}</p>
                  </div>
                </div>
              ))}
            </div>
          </section>
        </Reveal>
      </main>

      <footer>
        Built by {CONTRIBUTORS.map((c) => `${c.name} (${c.regNo})`).join(' and ')} · Educational project · Data: NSE BhavCopy · Not investment advice ·{' '}
        <a href={REPO_URL} target="_blank" rel="noreferrer">GitHub</a>
      </footer>
    </>
  )
}
