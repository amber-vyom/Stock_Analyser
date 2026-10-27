import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import PageShell from './PageShell'
import { binarySearchSteps, heapSteps, mergeSortSteps, type Item } from './algorithmSteps'

interface SnapshotStock {
  symbol: string
  score: number
}

const FALLBACK: Item[] = [
  { symbol: 'AAA', score: 71.2 }, { symbol: 'BBB', score: 78.4 }, { symbol: 'CCC', score: 64.9 },
  { symbol: 'DDD', score: 80.1 }, { symbol: 'EEE', score: 69.5 }, { symbol: 'FFF', score: 75.3 },
  { symbol: 'GGG', score: 72.8 }, { symbol: 'HHH', score: 66.0 },
]

function useStocks(limit: number): Item[] {
  const [items, setItems] = useState<Item[]>(FALLBACK)

  useEffect(() => {
    let active = true
    fetch(`${import.meta.env.BASE_URL}snapshot.json`)
      .then((r) => (r.ok ? r.json() : null))
      .then((data: { stocks: SnapshotStock[] } | null) => {
        if (!active || !data?.stocks?.length) return
        setItems(data.stocks.slice(0, limit).map((s) => ({ symbol: s.symbol, score: s.score })))
      })
      .catch(() => undefined)
    return () => {
      active = false
    }
  }, [limit])

  return items
}

function shuffled<T>(list: T[], seed: number): T[] {
  const result = [...list]
  let state = seed
  for (let i = result.length - 1; i > 0; i--) {
    state = (state * 1664525 + 1013904223) % 4294967296
    const j = state % (i + 1)
    ;[result[i], result[j]] = [result[j], result[i]]
  }
  return result
}

function Controls({
  index,
  total,
  onChange,
}: {
  index: number
  total: number
  onChange: (next: number) => void
}) {
  const [playing, setPlaying] = useState(false)
  const timer = useRef<number | undefined>(undefined)

  useEffect(() => {
    if (!playing) return
    timer.current = window.setInterval(() => {
      onChange(-1)
    }, 900)
    return () => window.clearInterval(timer.current)
  }, [playing, onChange])

  useEffect(() => {
    if (playing && index >= total - 1) setPlaying(false)
  }, [playing, index, total])

  return (
    <div className="demo-controls">
      <button onClick={() => { setPlaying(false); onChange(Math.max(index - 1, 0)) }} disabled={index <= 0}>Back</button>
      <button className="primary" onClick={() => { setPlaying(false); onChange(Math.min(index + 1, total - 1)) }} disabled={index >= total - 1}>
        Next step
      </button>
      <button onClick={() => setPlaying(!playing)} disabled={index >= total - 1 && !playing}>{playing ? 'Pause' : 'Auto-play'}</button>
      <button onClick={() => { setPlaying(false); onChange(0) }}>Reset</button>
      <span className="muted">Step {index + 1} of {total}</span>
    </div>
  )
}

function useStepper(total: number, resetKey: string = '') {
  const [index, setIndex] = useState(0)
  const change = useCallback(
    (next: number) => setIndex((current) => (next === -1 ? Math.min(current + 1, total - 1) : Math.min(next, total - 1))),
    [total],
  )
  useEffect(() => setIndex(0), [total, resetKey])
  return [index, change] as const
}

function HeapDemo({ stocks }: { stocks: Item[] }) {
  const items = useMemo(() => shuffled(stocks.slice(0, 10), 7), [stocks])
  const steps = useMemo(() => heapSteps(items, 5), [items])
  const [index, change] = useStepper(steps.length)
  const step = steps[Math.min(index, steps.length - 1)]

  const nodes = step.heap.map((item, i) => {
    const level = Math.floor(Math.log2(i + 1))
    const posInLevel = i + 1 - Math.pow(2, level)
    const count = Math.pow(2, level)
    return { item, i, x: ((posInLevel + 0.5) / count) * 100, y: level * 62 + 26 }
  })

  return (
    <div className="card demo">
      <h3>Priority queue (max-heap): picking the top stocks</h3>
      <p className="muted">
        The ranking uses a heap so the best stock is always at the top. Inserting is O(log n); removing the best is
        O(log n). Watch the real top stocks go in and come out best-first.
      </p>
      <Controls index={index} total={steps.length} onChange={change} />
      <p className="demo-note" role="status">{step.note}</p>
      <div className="tree-wrap" style={{ height: 250 }}>
      <svg viewBox="0 0 100 250" preserveAspectRatio="none" className="tree" role="img" aria-label="Binary heap drawn as a tree" style={{ height: 250 }}>
        {nodes.map(({ i, x, y }) => {
          if (i === 0) return null
          const parent = nodes[Math.floor((i - 1) / 2)]
          return <line key={`l${i}`} x1={parent.x} y1={parent.y} x2={x} y2={y} className="tree-edge" vectorEffect="non-scaling-stroke" />
        })}
      </svg>
      <div className="tree-layer" style={{ height: 250 }}>
        {nodes.map(({ item, i, x, y }) => (
          <div
            key={item.symbol}
            className={`tree-node ${step.highlight.includes(i) ? 'hot' : ''}`}
            style={{ left: `${x}%`, top: y }}
          >
            <strong>{item.symbol}</strong>
            <span>{item.score.toFixed(1)}</span>
          </div>
        ))}
      </div>
      </div>
      <div className="array-row" aria-label="Heap as an array">
        {step.heap.map((item, i) => (
          <div key={item.symbol} className={`cell ${step.highlight.includes(i) ? 'hot' : ''}`}>
            <small>{i}</small>
            <strong>{item.symbol}</strong>
            <span>{item.score.toFixed(1)}</span>
          </div>
        ))}
      </div>
      {step.removed.length > 0 && (
        <p className="demo-note">
          Removed so far (best first): {step.removed.map((r) => `${r.symbol} ${r.score.toFixed(1)}`).join(', ')}
        </p>
      )}
    </div>
  )
}

function SearchDemo({ stocks }: { stocks: Item[] }) {
  const symbols = useMemo(() => [...stocks.map((s) => s.symbol)].sort((a, b) => a.localeCompare(b)), [stocks])
  const [target, setTarget] = useState('')
  const chosen = symbols.includes(target) ? target : symbols[Math.floor(symbols.length * 0.7)] ?? ''
  const steps = useMemo(() => binarySearchSteps(symbols, chosen), [symbols, chosen])
  const [index, change] = useStepper(steps.length, chosen)
  const step = steps[Math.min(index, steps.length - 1)]

  return (
    <div className="card demo">
      <h3>Binary search: finding a symbol</h3>
      <p className="muted">
        On a sorted list, each comparison discards half of what is left, so it takes O(log n) steps instead of O(n).
        This is how the API looks up one stock.
      </p>
      <label className="demo-pick">
        Search for{' '}
        <select value={chosen} onChange={(e) => setTarget(e.target.value)}>
          {symbols.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </label>
      <Controls index={index} total={steps.length} onChange={change} />
      <p className="demo-note" role="status">{step.note}</p>
      <div className="array-row" aria-label="Sorted symbols">
        {symbols.map((s, i) => {
          const inRange = i >= step.low && i <= step.high
          const state = step.found && i === step.mid ? 'found' : i === step.mid ? 'hot' : inRange ? '' : 'dim'
          return (
            <div key={s} className={`cell ${state}`}>
              <small>{i}</small>
              <strong>{s}</strong>
            </div>
          )
        })}
      </div>
      <p className="muted">
        {symbols.length} symbols: at most {Math.ceil(Math.log2(symbols.length + 1))} comparisons with binary search,
        up to {symbols.length} with a plain scan.
      </p>
    </div>
  )
}

function SortDemo({ stocks }: { stocks: Item[] }) {
  const items = useMemo(() => shuffled(stocks.slice(0, 8), 21), [stocks])
  const steps = useMemo(() => mergeSortSteps(items), [items])
  const [index, change] = useStepper(steps.length)
  const step = steps[Math.min(index, steps.length - 1)]
  const max = Math.max(...items.map((i) => i.score), 1)
  const min = Math.min(...items.map((i) => i.score))

  return (
    <div className="card demo">
      <h3>Merge sort: ranking every stock</h3>
      <p className="muted">
        Merge sort splits the list, sorts the halves and merges them back in order. It is stable and always O(n log n).
        The full ranking uses it, and a test checks it matches the heap.
      </p>
      <Controls index={index} total={steps.length} onChange={change} />
      <p className="demo-note" role="status">{step.note} Comparisons so far: {step.comparisons}.</p>
      <div className="bars" aria-label="Scores being sorted">
        {step.values.map((v, i) => {
          const active = step.range && i >= step.range[0] && i <= step.range[1]
          return (
            <div key={v.symbol} className="bar-col">
              <span className="bar-val">{v.score.toFixed(1)}</span>
              <div className={`bar-fill ${active ? 'hot' : ''}`} style={{ height: `${25 + ((v.score - min) / (max - min || 1)) * 75}%` }} />
              <span className="bar-label">{v.symbol}</span>
            </div>
          )
        })}
      </div>
    </div>
  )
}

const TABLE = [
  ['List / array traversal', 'Reading and parsing daily records', 'O(n)', 'NseParser, SevenDayAggregator'],
  ['Dictionary (hash table)', 'Grouping records by stock symbol', 'O(1) per insert, O(n) total', 'SevenDayAggregator'],
  ['Sliding window', 'Keeping only the latest 7 trading days', 'O(f log f) for f files', 'NseReportFiles'],
  ['Priority queue (binary heap)', 'Top-N ranking', 'O(n log n) overall, O(log n) per operation', 'StockRankingService'],
  ['Merge sort', 'Full ranking and sorting by symbol', 'O(n log n) time, O(n) space', 'MergeSorter'],
  ['Binary search', 'Symbol lookup in the API', 'O(log n) on a sorted list', 'SymbolSearch'],
]

export default function Algorithms() {
  const stocks = useStocks(12)

  return (
    <PageShell active="algorithms" title="Algorithms">
      <section className="section hero">
        <p className="eyebrow">Data structures and algorithms</p>
        <h1>How the ranking works, step by step</h1>
        <p className="lead">
          This project is built around a few classic algorithms. Each demo below runs on real scores from the
          dashboard, so you can step through exactly what the code does.
        </p>
      </section>

      <section className="section">
        <h2>Where each one is used</h2>
        <p className="section-lead">Complexity is in terms of n, the number of stocks.</p>
        <div className="card table-card">
          <table>
            <thead>
              <tr><th>Concept</th><th>Used for</th><th>Complexity</th><th>In the code</th></tr>
            </thead>
            <tbody>
              {TABLE.map(([a, b, c, d]) => (
                <tr key={a} className="static-row"><td><strong>{a}</strong></td><td>{b}</td><td>{c}</td><td><code>{d}</code></td></tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className="section">
        <h2>Live demos</h2>
        <p className="section-lead">Use Next step, or Auto-play to watch it run.</p>
        <div className="demos">
          <HeapDemo stocks={stocks} />
          <SortDemo stocks={stocks} />
          <SearchDemo stocks={stocks} />
        </div>
        <p className="back-link"><a href="#/">Back to the dashboard</a></p>
      </section>
    </PageShell>
  )
}
