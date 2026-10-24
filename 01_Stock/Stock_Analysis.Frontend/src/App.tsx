import { useCallback, useEffect, useMemo, useState } from 'react'
import './App.css'

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

const STATIC = import.meta.env.VITE_STATIC === 'true'
const REPO_URL = 'https://github.com/amber-vyom/Stock_Analyser'

const num = (v: number, d = 2) => v.toLocaleString('en-IN', { maximumFractionDigits: d })

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

function ScoreHistogram({ stocks }: { stocks: Stock[] }) {
  const buckets = useMemo(() => {
    const lo = Math.floor(Math.min(...stocks.map((s) => s.score)) / 5) * 5
    const hi = Math.ceil(Math.max(...stocks.map((s) => s.score)) / 5) * 5
    const result: { label: string; count: number }[] = []
    for (let start = lo; start < Math.max(hi, lo + 5); start += 5) {
      result.push({
        label: `${start}-${start + 5}`,
        count: stocks.filter((s) => s.score >= start && s.score < start + 5).length,
      })
    }
    return result
  }, [stocks])

  const max = Math.max(...buckets.map((b) => b.count), 1)

  return (
    <div className="histogram" role="img" aria-label="Distribution of scores in 5-point buckets">
      {buckets.map((b) => (
        <div key={b.label} className="hist-col">
          <span className="hist-count">{b.count}</span>
          <div className="hist-bar" style={{ height: `${(b.count / max) * 100}%` }} />
          <span className="hist-label">{b.label}</span>
        </div>
      ))}
    </div>
  )
}

export default function App() {
  const [stocks, setStocks] = useState<Stock[]>([])
  const [count, setCount] = useState(25)
  const [search, setSearch] = useState('')
  const [minFilters, setMinFilters] = useState(0)
  const [sortKey, setSortKey] = useState<SortKey>('rank')
  const [sortAsc, setSortAsc] = useState(true)
  const [selected, setSelected] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [snapshotDate, setSnapshotDate] = useState<string | null>(null)
  const [updating, setUpdating] = useState(false)
  const [updateMsg, setUpdateMsg] = useState<string | null>(null)
  const [showAbout, setShowAbout] = useState(false)

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

  const visible = useMemo(() => {
    const term = search.trim().toLowerCase()
    const filtered = stocks.filter(
      (s) => s.symbol.toLowerCase().includes(term) && s.passedFilters >= minFilters,
    )
    return [...filtered].sort((a, b) => {
      const x = sortValue(a, sortKey)
      const y = sortValue(b, sortKey)
      const order = typeof x === 'string' ? x.localeCompare(y as string) : (x as number) - (y as number)
      return sortAsc ? order : -order
    })
  }, [stocks, search, minFilters, sortKey, sortAsc])

  const current = stocks.find((s) => s.symbol === selected)
  const avgScore = stocks.length ? stocks.reduce((t, s) => t + s.score, 0) / stocks.length : 0
  const avgChange = stocks.length
    ? stocks.reduce((t, s) => t + s.metrics.priceChangePercentage, 0) / stocks.length
    : 0

  const header = (key: SortKey, label: string) => (
    <th aria-sort={sortKey === key ? (sortAsc ? 'ascending' : 'descending') : 'none'}>
      <button className="sort" onClick={() => changeSort(key)}>
        {label}
        {sortKey === key ? (sortAsc ? ' ▲' : ' ▼') : ''}
      </button>
    </th>
  )

  return (
    <div className="page">
      <header>
        <div>
          <h1>Stock Analysis System</h1>
          <p className="tagline">NSE equity screening and ranking over a rolling 7-trading-day window</p>
        </div>
        <div className="actions">
          <span className={`badge ${STATIC ? 'badge-snapshot' : 'badge-live'}`}>
            {STATIC ? 'Snapshot' : 'Live data'}
          </span>
          {!STATIC && (
            <button onClick={updateData} disabled={updating || loading}>
              {updating ? 'Updating… (can take a few minutes)' : 'Update data from NSE'}
            </button>
          )}
          <button onClick={load} disabled={loading || updating}>Refresh</button>
          <button onClick={() => setShowAbout(!showAbout)} aria-expanded={showAbout}>
            How it works
          </button>
        </div>
      </header>

      {updateMsg && <p className="banner">{updateMsg}</p>}
      {STATIC && snapshotDate && (
        <p className="banner">
          Static snapshot generated {new Date(snapshotDate).toLocaleString()}. Live data and updates are only
          available when the API runs locally.
        </p>
      )}

      {showAbout && (
        <section className="card about">
          <h2>How it works</h2>
          <ol>
            <li>NSE BhavCopy reports for the latest 7 trading days are downloaded; holidays are skipped.</li>
            <li>EQ-series records are grouped by symbol and aggregated into seven-day metrics.</li>
            <li>Ten pass/fail filters are applied to each stock.</li>
            <li>A weighted score out of 100 is computed from the metrics (see the breakdown for any stock).</li>
            <li>
              Stocks are ranked with a priority queue: score first, then filters passed, then symbol.
              Stocks without all 7 trading days are not ranked.
            </li>
          </ol>
          {current && (
            <p className="muted">
              Maximum points per component:{' '}
              {current.breakdown.map((b) => `${b.name} ${b.max}`).join(' · ')}.
            </p>
          )}
          <p className="muted">
            This is a rule-based screening tool for education. The score has not been backtested and is not
            a prediction of future returns or investment advice.{' '}
            <a href={REPO_URL} target="_blank" rel="noreferrer">Source code</a>
          </p>
        </section>
      )}

      {loading && <div className="state">Loading…</div>}
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

      {!loading && !error && stocks.length > 0 && (
        <>
          <div className="stats">
            <div className="stat"><span>Stocks ranked</span><strong>{stocks.length}</strong></div>
            <div className="stat"><span>Best score</span><strong>{stocks[0].score.toFixed(2)}</strong><em>{stocks[0].symbol}</em></div>
            <div className="stat"><span>Average score</span><strong>{avgScore.toFixed(2)}</strong></div>
            <div className="stat">
              <span>Avg 7-day change</span>
              <strong className={avgChange >= 0 ? 'pos' : 'neg'}>{num(avgChange)}%</strong>
            </div>
          </div>

          <div className="toolbar">
            <input
              placeholder="Search symbol"
              aria-label="Search symbol"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
            <select aria-label="Number of stocks" value={count} onChange={(e) => setCount(Number(e.target.value))}>
              {[10, 25, 50, 100, 200].map((n) => (
                <option key={n} value={n}>Top {n}</option>
              ))}
            </select>
            <select aria-label="Minimum filters passed" value={minFilters} onChange={(e) => setMinFilters(Number(e.target.value))}>
              {[0, 5, 7, 8, 9, 10].map((n) => (
                <option key={n} value={n}>{n === 0 ? 'Any filters passed' : `${n}+ filters passed`}</option>
              ))}
            </select>
            <span className="muted">Showing {visible.length} of {stocks.length}</span>
          </div>

          <div className="layout">
            <div className="card table-card">
              <table>
                <thead>
                  <tr>
                    {header('rank', '#')}
                    {header('symbol', 'Symbol')}
                    {header('score', 'Score')}
                    {header('filters', 'Filters')}
                    {header('change', '7-day change')}
                    {header('volume', 'Avg volume')}
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
                        {num(s.metrics.priceChangePercentage)}%
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
                <div className="card">
                  <h2>{current.symbol}</h2>
                  <p className="big-score">{current.score.toFixed(2)}<span> / 100 · rank {current.rank}</span></p>
                  <h3>Filters ({current.passedFilters}/10)</h3>
                  <ul className="filters">
                    {current.filters.map((f) => (
                      <li key={f.name} className={f.passed ? 'pos' : 'neg'}>
                        {f.passed ? '✓' : '✗'} {f.name}
                      </li>
                    ))}
                  </ul>
                  <h3>Score breakdown</h3>
                  <ul className="breakdown">
                    {current.breakdown.map((b) => (
                      <li key={b.name}>
                        <span>{b.name}</span>
                        <span>{b.points.toFixed(1)} / {b.max}</span>
                        <div className="bar"><div style={{ width: `${b.max > 0 ? (b.points / b.max) * 100 : 0}%` }} /></div>
                      </li>
                    ))}
                  </ul>
                  <h3>Seven-day metrics</h3>
                  <dl>
                    <dt>Trading days</dt><dd>{current.metrics.tradingDays}</dd>
                    <dt>Avg open</dt><dd>{num(current.metrics.averageOpenPrice)}</dd>
                    <dt>Avg close</dt><dd>{num(current.metrics.averageClosePrice)}</dd>
                    <dt>High / Low</dt><dd>{num(current.metrics.highestPrice)} / {num(current.metrics.lowestPrice)}</dd>
                    <dt>Avg daily volume</dt><dd>{num(current.metrics.averageDailyVolume, 0)}</dd>
                    <dt>Total trades</dt><dd>{num(current.metrics.totalTrades, 0)}</dd>
                    <dt>Traded value</dt><dd>{num(current.metrics.totalTradedValue, 0)}</dd>
                    <dt>Volume consistency</dt><dd>{num(current.metrics.volumeConsistency)}</dd>
                  </dl>
                </div>
              )}
              <div className="card">
                <h3>Score distribution</h3>
                <ScoreHistogram stocks={stocks} />
              </div>
            </aside>
          </div>
        </>
      )}

      <footer>
        Educational project · Data: NSE BhavCopy · Not investment advice ·{' '}
        <a href={REPO_URL} target="_blank" rel="noreferrer">GitHub</a>
      </footer>
    </div>
  )
}
