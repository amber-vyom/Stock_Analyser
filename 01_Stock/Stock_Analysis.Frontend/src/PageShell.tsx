import { useEffect, useState, type ReactNode } from 'react'
import { Background, BackToTop, Logo, MoonIcon, SunIcon } from './ui'

type Page = 'learn' | 'algorithms'

function initialTheme(): 'light' | 'dark' {
  try {
    const saved = localStorage.getItem('theme')
    if (saved === 'light' || saved === 'dark') return saved
  } catch {
    // storage may be unavailable
  }
  return window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark'
}

export default function PageShell({ active, title, children }: { active: Page; title: string; children: ReactNode }) {
  const [theme, setTheme] = useState<'light' | 'dark'>(initialTheme)

  useEffect(() => {
    document.documentElement.dataset.theme = theme
    try {
      localStorage.setItem('theme', theme)
    } catch {
      // ignore
    }
  }, [theme])

  useEffect(() => {
    document.title = `${title} · Stock Analysis System`
    window.scrollTo(0, 0)
    return () => {
      document.title = 'Stock Analysis System'
    }
  }, [title])

  return (
    <>
      <a
        className="skip-link"
        href="#page-main"
        onClick={(e) => {
          e.preventDefault()
          document.getElementById('page-main')?.focus()
        }}
      >
        Skip to content
      </a>
      <Background />

      <nav className="nav">
        <div className="nav-inner">
          <a className="brand" href="#/">
            <Logo /> <span>Stock Analysis</span>
          </a>
          <div className="nav-links" role="navigation" aria-label="Site">
            <a href="#/">Dashboard</a>
            <a href="#/learn" className={active === 'learn' ? 'active' : ''}>Learn</a>
            <a href="#/algorithms" className={active === 'algorithms' ? 'active' : ''}>Algorithms</a>
          </div>
          <div className="nav-actions">
            <button
              className="icon-btn"
              onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
              aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} theme`}
              title="Toggle theme"
            >
              {theme === 'dark' ? <SunIcon /> : <MoonIcon />}
            </button>
          </div>
        </div>
      </nav>

      <main className="page" id="page-main" tabIndex={-1}>
        {children}
      </main>

      <footer className="site-footer">
        <p className="footer-note" style={{ marginTop: 0, borderTop: 'none', paddingTop: 0 }}>
          Educational content only. Markets involve risk, and past performance does not predict future returns.
        </p>
      </footer>

      <BackToTop />
    </>
  )
}
