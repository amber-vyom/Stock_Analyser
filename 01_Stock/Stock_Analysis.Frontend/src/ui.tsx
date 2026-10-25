import { Component, useEffect, useRef, useState, type ErrorInfo, type ReactNode } from 'react'

export const prefersReducedMotion = () =>
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches

export function Background() {
  return (
    <div className="bg" aria-hidden="true">
      <span className="blob blob-a" />
      <span className="blob blob-b" />
      <span className="blob blob-c" />
      <span className="grid" />
    </div>
  )
}

export function CountUp({ value, decimals = 0, suffix = '' }: { value: number; decimals?: number; suffix?: string }) {
  const [shown, setShown] = useState(prefersReducedMotion() ? value : 0)

  useEffect(() => {
    if (prefersReducedMotion()) {
      setShown(value)
      return
    }
    const start = performance.now()
    const duration = 900
    let frame = 0
    const tick = (now: number) => {
      const t = Math.min((now - start) / duration, 1)
      setShown(value * (1 - Math.pow(1 - t, 3)))
      if (t < 1) frame = requestAnimationFrame(tick)
    }
    frame = requestAnimationFrame(tick)
    return () => cancelAnimationFrame(frame)
  }, [value])

  return (
    <>
      {shown.toLocaleString('en-IN', { minimumFractionDigits: decimals, maximumFractionDigits: decimals })}
      {suffix}
    </>
  )
}

export function Reveal({ children, className = '' }: { children: ReactNode; className?: string }) {
  const ref = useRef<HTMLDivElement>(null)
  const [visible, setVisible] = useState(prefersReducedMotion())

  useEffect(() => {
    const node = ref.current
    if (!node || visible) return
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) {
          setVisible(true)
          observer.disconnect()
        }
      },
      { threshold: 0.08 },
    )
    observer.observe(node)
    return () => observer.disconnect()
  }, [visible])

  return (
    <div ref={ref} className={`reveal ${visible ? 'in' : ''} ${className}`}>
      {children}
    </div>
  )
}

export function ScoreRing({ score, size = 84 }: { score: number; size?: number }) {
  const radius = (size - 10) / 2
  const circumference = 2 * Math.PI * radius
  const [progress, setProgress] = useState(prefersReducedMotion() ? score : 0)

  useEffect(() => {
    const id = requestAnimationFrame(() => setProgress(score))
    return () => cancelAnimationFrame(id)
  }, [score])

  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} className="ring" role="img" aria-label={`Score ${score.toFixed(1)} out of 100`}>
      <circle cx={size / 2} cy={size / 2} r={radius} className="ring-track" />
      <circle
        cx={size / 2}
        cy={size / 2}
        r={radius}
        className="ring-value"
        strokeDasharray={circumference}
        strokeDashoffset={circumference * (1 - Math.min(progress, 100) / 100)}
        transform={`rotate(-90 ${size / 2} ${size / 2})`}
      />
      <text x="50%" y="50%" textAnchor="middle" dominantBaseline="central" className="ring-text">
        {score.toFixed(1)}
      </text>
    </svg>
  )
}

const iconProps = {
  width: 18,
  height: 18,
  viewBox: '0 0 24 24',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 2,
  strokeLinecap: 'round' as const,
  strokeLinejoin: 'round' as const,
  'aria-hidden': true,
}

export const SunIcon = () => (
  <svg {...iconProps}><circle cx="12" cy="12" r="4" /><path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4" /></svg>
)

export const MoonIcon = () => (
  <svg {...iconProps}><path d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z" /></svg>
)

export const ArrowUpIcon = () => (
  <svg {...iconProps}><path d="M12 19V5M5 12l7-7 7 7" /></svg>
)

export const DownloadIcon = () => (
  <svg {...iconProps}><path d="M12 3v12M7 10l5 5 5-5M5 21h14" /></svg>
)

export const SearchIcon = () => (
  <svg {...iconProps}><circle cx="11" cy="11" r="7" /><path d="M21 21l-4.3-4.3" /></svg>
)

export function Logo() {
  return (
    <svg width="26" height="26" viewBox="0 0 64 64" aria-hidden="true">
      <defs>
        <linearGradient id="logo-grad" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stopColor="var(--accent)" />
          <stop offset="1" stopColor="var(--accent-2)" />
        </linearGradient>
      </defs>
      <rect width="64" height="64" rx="14" fill="url(#logo-grad)" />
      <polyline points="12,44 24,32 33,38 51,18" fill="none" stroke="#fff" strokeWidth="5" strokeLinecap="round" strokeLinejoin="round" />
      <circle cx="51" cy="18" r="4" fill="#fff" />
    </svg>
  )
}

export function Ticker({ items }: { items: { symbol: string; change: number }[] }) {
  if (items.length === 0) return null
  const row = (suffix: string) =>
    items.map((i) => (
      <span key={`${i.symbol}-${suffix}`} className="ticker-item">
        <strong>{i.symbol}</strong>
        <span className={i.change >= 0 ? 'pos' : 'neg'}>
          {i.change >= 0 ? '▲' : '▼'} {Math.abs(i.change).toFixed(2)}%
        </span>
      </span>
    ))

  return (
    <div className="ticker" role="marquee" aria-label="Seven-day price change of the ranked stocks">
      <div className="ticker-track">
        {row('a')}
        {row('b')}
      </div>
    </div>
  )
}

export function Skeleton({ rows = 5 }: { rows?: number }) {
  return (
    <div className="skeleton-wrap" aria-busy="true" aria-label="Loading">
      <div className="skeleton-stats">
        {[0, 1, 2, 3].map((i) => <div key={i} className="skeleton skeleton-stat" />)}
      </div>
      {Array.from({ length: rows }, (_, i) => <div key={i} className="skeleton skeleton-row" />)}
    </div>
  )
}

export function BackToTop() {
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    const onScroll = () => setVisible(window.scrollY > 700)
    onScroll()
    window.addEventListener('scroll', onScroll, { passive: true })
    return () => window.removeEventListener('scroll', onScroll)
  }, [])

  return (
    <button
      className={`to-top ${visible ? 'show' : ''}`}
      onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
      aria-label="Back to top"
      tabIndex={visible ? 0 : -1}
    >
      <ArrowUpIcon />
    </button>
  )
}

export class ErrorBoundary extends Component<{ children: ReactNode }, { failed: boolean }> {
  state = { failed: false }

  static getDerivedStateFromError() {
    return { failed: true }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error(error, info)
  }

  render() {
    if (this.state.failed) {
      return (
        <div className="state err" role="alert" style={{ padding: '80px 16px' }}>
          <h2>Something went wrong</h2>
          <p>The page hit an unexpected error. Reloading usually fixes it.</p>
          <button onClick={() => window.location.reload()}>Reload the page</button>
        </div>
      )
    }
    return this.props.children
  }
}
