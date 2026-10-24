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

const num =(v: number, d = 2) =>
  v.toLocaleString('en-IN', { maximumFractionDigits: d })

async function getJson<T>(url: string): Promise<T> {
  const res = await fetch(url)
  if (!res.ok) {
    const body = await res.json().catch(() => null)
    throw new Error(body?.error ?? `Request failed (${res.status})`)
  }
  return res.json() as Promise<T>
}

export default function App() {
  const [stocks, setStocks] = useState<Stock[]>([])
  const [count, setCount] = useState(10)
  const [search, setSearch] = useState('')
  const [selected, setSelected] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [updating, setUpdating] = useState(false)
  const [updateMsg, setUpdateMsg] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getJson<Stock[]>(`/api/stocks/top?count=${count}`)
      setStocks(data)
      setSelected(data[0]?.symbol ?? null)
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

  const visible = useMemo(
    () => stocks.filter((s) => s.symbol.toLowerCase().includes(search.trim().toLowerCase())),
    [stocks, search],
  )
  const current = stocks.find((s) => s.symbol === selected)

  return (
    <div className="page">
      <header>
        <h1>Stock Analysis System</h1>
        <span>
          <button onClick={updateData} disabled={updating || loading}>
            {updating ? 'Updating data… (can take a few minutes)' : 'Update data from NSE'}
          </button>{' '}
          <button onClick={load} disabled={loading || updating}>Refresh</button>
        </span>
      </header>
      {updateMsg && <p className="note">{updateMsg}</p>}
      <p className="note">
        Rule-based screening and ranking of NSE EQ stocks over the stored 7-day window.
        The score is not a forecast of future returns.
      </p>

      <div className="toolbar">
        <input placeholder="Search symbol" value={search} onChange={(e) => setSearch(e.target.value)} />
        <select value={count} onChange={(e) => setCount(Number(e.target.value))}>
          {[10, 25, 50, 100].map((n) => (
            <option key={n} value={n}>Top {n}</option>
          ))}
        </select>
      </div>

      {loading && <div className="state">Loading…</div>}
      {error && <div className="state err">Could not load stocks: {error}</div>}
      {!loading && !error && stocks.length === 0 && (
        <div className="state">No processed data. Run "Save Processed Data" in the desktop app.</div>
      )}

      {!loading && !error && stocks.length > 0 && (
        <div className="layout">
          <div className="card">
            <table>
              <thead>
                <tr><th>#</th><th>Symbol</th><th>Score</th><th>Filters</th><th>7-day change</th><th>Days</th></tr>
              </thead>
              <tbody>
                {visible.map((s) => (
                  <tr key={s.symbol} className={s.symbol === selected ? 'sel' : ''} onClick={() => setSelected(s.symbol)}>
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
                    <td>
                      {s.metrics.tradingDays}
                      {s.metrics.tradingDays < 7 && <span className="warn"> partial</span>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {current && (
            <div className="card">
              <h2>{current.symbol} — {current.score.toFixed(2)}/100</h2>
              <ul className="filters">
                {current.filters.map((f) => (
                  <li key={f.name} className={f.passed ? 'pos' : 'neg'}>
                    {f.passed ? '✓' : '✗'} {f.name}
                  </li>
                ))}
              </ul>
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
        </div>
      )}
    </div>
  )
}
