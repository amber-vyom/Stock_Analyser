import { useEffect, useMemo, useState } from 'react'
import { Background, BackToTop, Logo, MoonIcon, SearchIcon, SunIcon } from './ui'

type Category = 'Basics' | 'Trading' | 'Analysis' | 'Fundamentals' | 'Risk' | 'In this project'

interface Term {
  term: string
  category: Category
  meaning: string
}

const CATEGORIES: Category[] = ['Basics', 'Trading', 'Analysis', 'Fundamentals', 'Risk', 'In this project']

const TERMS: Term[] = [
  { term: 'Stock (share)', category: 'Basics', meaning: 'A unit of ownership in a company. Owning shares makes you a part-owner, entitled to a share of the profits (dividends) and to any rise in the company\'s value.' },
  { term: 'Stock exchange', category: 'Basics', meaning: 'A regulated marketplace where shares are bought and sold. In India the two main exchanges are the NSE and the BSE.' },
  { term: 'NSE', category: 'Basics', meaning: 'National Stock Exchange of India, based in Mumbai. Its best-known index is the Nifty 50. The data in this project comes from NSE.' },
  { term: 'BSE', category: 'Basics', meaning: 'Bombay Stock Exchange, Asia\'s oldest exchange. Its best-known index is the Sensex.' },
  { term: 'SEBI', category: 'Basics', meaning: 'Securities and Exchange Board of India. The regulator that makes the rules for the market and protects investors.' },
  { term: 'Index', category: 'Basics', meaning: 'A basket of stocks used to measure how the market is doing. The Nifty 50 tracks 50 large NSE companies; the Sensex tracks 30 large BSE companies.' },
  { term: 'Demat account', category: 'Basics', meaning: 'An account that holds your shares in electronic form, much like a bank account holds money.' },
  { term: 'Broker', category: 'Basics', meaning: 'A licensed intermediary through which you place buy and sell orders on the exchange. Brokers charge fees called brokerage.' },
  { term: 'IPO', category: 'Basics', meaning: 'Initial Public Offering. The first time a company sells its shares to the public so that they can be traded on an exchange.' },
  { term: 'Dividend', category: 'Basics', meaning: 'A portion of a company\'s profit paid out to its shareholders, usually per share. Not every company pays one.' },
  { term: 'Stock split', category: 'Basics', meaning: 'Dividing each share into several shares (for example 1 into 5). The price per share falls proportionally, but the total value of your holding does not change.' },
  { term: 'Bonus issue', category: 'Basics', meaning: 'Free extra shares given to existing shareholders in a set ratio, for example 1 for every 1 held. The share price adjusts downward afterwards.' },
  { term: 'Market capitalisation', category: 'Basics', meaning: 'Share price multiplied by the number of shares. SEBI groups the 100 largest companies as large-cap, ranks 101 to 250 as mid-cap, and the rest as small-cap.' },
  { term: 'Bull and bear market', category: 'Basics', meaning: 'A bull market is a period of rising prices and optimism. A bear market is a period of falling prices and pessimism, often defined as a fall of 20% or more from a recent peak.' },

  { term: 'Bid and ask', category: 'Trading', meaning: 'The bid is the highest price a buyer is currently willing to pay; the ask is the lowest price a seller will accept. The gap between them is the spread.' },
  { term: 'Market order and limit order', category: 'Trading', meaning: 'A market order buys or sells immediately at the best available price. A limit order buys or sells only at the price you set, or better, and may not be filled.' },
  { term: 'Stop-loss', category: 'Trading', meaning: 'An order that sells automatically if the price falls to a level you choose, to limit how much you can lose on a position.' },
  { term: 'Intraday and delivery', category: 'Trading', meaning: 'Intraday trades are opened and closed on the same day. Delivery trades are held in your demat account beyond the day.' },
  { term: 'Trading hours', category: 'Trading', meaning: 'The regular NSE equity session runs from 9:15 am to 3:30 pm IST, Monday to Friday, except on market holidays. There is a short pre-open session before it.' },
  { term: 'Settlement (T+1)', category: 'Trading', meaning: 'After a trade on day T, the shares and the money change hands one working day later (T+1).' },
  { term: 'Circuit limit (price band)', category: 'Trading', meaning: 'A cap on how much a stock\'s price may rise or fall in a day, such as 5%, 10% or 20%. Trading in the stock is restricted once the limit is hit.' },
  { term: 'Liquidity', category: 'Trading', meaning: 'How easily you can buy or sell without moving the price. Stocks with high trading volume are usually more liquid.' },
  { term: 'Volume', category: 'Trading', meaning: 'The number of shares traded in a period, such as a day.' },
  { term: 'Traded value (turnover)', category: 'Trading', meaning: 'The total money value of the shares traded, roughly volume multiplied by price.' },
  { term: 'Open, high, low, close (OHLC)', category: 'Trading', meaning: 'The four key prices of a trading day: the first trade, the highest, the lowest and the last.' },
  { term: 'Volatility', category: 'Trading', meaning: 'How much and how quickly a price moves. Higher volatility means bigger swings and higher risk.' },
  { term: 'Short selling', category: 'Trading', meaning: 'Selling shares you do not own in the hope of buying them back cheaper. It is advanced and risky, because losses can exceed the money put in.' },

  { term: 'Fundamental analysis', category: 'Analysis', meaning: 'Judging what a company is worth by studying its business: revenue, profit, debt, management and the industry it operates in.' },
  { term: 'Technical analysis', category: 'Analysis', meaning: 'Studying past price and volume patterns, usually on charts, to look for hints about likely future moves.' },
  { term: 'Trend', category: 'Analysis', meaning: 'The general direction of a price over time: up, down or sideways.' },
  { term: 'Momentum', category: 'Analysis', meaning: 'The tendency of a stock that has been rising to keep rising for a while, or of a falling stock to keep falling.' },
  { term: 'Moving average', category: 'Analysis', meaning: 'The average price over the last N days. It smooths out daily noise so the direction of the trend is easier to see.' },
  { term: 'RSI (Relative Strength Index)', category: 'Analysis', meaning: 'An indicator from 0 to 100 comparing recent gains with recent losses. By convention above 70 is called overbought and below 30 oversold.' },
  { term: 'Support and resistance', category: 'Analysis', meaning: 'Price levels where falls have tended to stop (support) or rises have tended to stall (resistance).' },
  { term: 'Candlestick', category: 'Analysis', meaning: 'A chart bar showing the open, high, low and close for a period, coloured by whether the price rose or fell.' },
  { term: '52-week high and low', category: 'Analysis', meaning: 'The highest and lowest prices of a stock in the last year.' },

  { term: 'EPS (earnings per share)', category: 'Fundamentals', meaning: 'A company\'s net profit divided by its number of shares.' },
  { term: 'P/E ratio', category: 'Fundamentals', meaning: 'Share price divided by EPS. It shows how much investors pay for each rupee of profit and is best compared between similar companies.' },
  { term: 'Dividend yield', category: 'Fundamentals', meaning: 'Annual dividend per share divided by the share price, shown as a percentage.' },
  { term: 'ROE (return on equity)', category: 'Fundamentals', meaning: 'Net profit divided by shareholders\' equity. It measures how well a company turns investors\' money into profit.' },
  { term: 'Debt-to-equity ratio', category: 'Fundamentals', meaning: 'Total borrowings divided by shareholders\' equity. A high number means the company relies heavily on debt.' },
  { term: 'Book value', category: 'Fundamentals', meaning: 'What would be left for shareholders if the company sold its assets and paid all its debts, usually quoted per share.' },

  { term: 'Diversification', category: 'Risk', meaning: 'Spreading money across different stocks, sectors and asset types so that one bad outcome does not sink the whole portfolio.' },
  { term: 'Risk and return', category: 'Risk', meaning: 'Higher possible returns almost always come with a higher chance of losing money. No investment is risk-free.' },
  { term: 'Portfolio', category: 'Risk', meaning: 'The full collection of investments that you hold.' },
  { term: 'Mutual fund and ETF', category: 'Risk', meaning: 'Pooled investments managed by professionals. An ETF trades on the exchange like a stock and usually tracks an index.' },
  { term: 'SIP', category: 'Risk', meaning: 'Systematic Investment Plan. Investing a fixed amount at regular intervals, mostly used for mutual funds, which spreads out the buying price over time.' },

  { term: 'BhavCopy', category: 'In this project', meaning: 'The exchange\'s daily report listing every security\'s open, high, low, close, volume and number of trades. It is this project\'s data source.' },
  { term: 'EQ series', category: 'In this project', meaning: 'Normal equity shares traded in regular rolling settlement. Other series, such as bonds or restricted categories, are left out of the analysis.' },
  { term: '7-day change', category: 'In this project', meaning: 'The percentage change from the first day\'s opening price to the last day\'s closing price in the seven-trading-day window.' },
  { term: 'Volume consistency', category: 'In this project', meaning: 'A score from 0 to 100 for how steady daily volume was. 100 means the same volume every day; spiky volume scores lower.' },
  { term: 'Price stability', category: 'In this project', meaning: 'The gap between the highest and lowest price in the window as a percentage of the lowest price. The check passes at 15% or less.' },
  { term: 'Weighted score', category: 'In this project', meaning: 'A score out of 100 built from ten components, each with a maximum number of points. It is separate from the ten pass/fail checks.' },
  { term: 'Corporate action', category: 'In this project', meaning: 'Events such as splits and bonus issues that change the price without changing value. Unadjusted prices can make them look like crashes, so the ML experiment drops windows with huge one-day moves.' },
  { term: 'ROC-AUC', category: 'In this project', meaning: 'A model score: the chance that the model ranks a random "up" case above a random "down" case. 0.5 means guessing and 1.0 is perfect.' },
  { term: 'Baseline', category: 'In this project', meaning: 'A very simple reference method, such as always guessing the most common outcome, that a model has to beat to be worth anything.' },
  { term: 'Sentiment', category: 'In this project', meaning: 'The tone of news headlines about a stock: positive, neutral or negative. Here it is scored by a simple word-based method.' },
]

const STUDY_PATH = [
  { title: 'Learn the basics', text: 'What a share is, what exchanges and regulators do, and how companies list through an IPO.' },
  { title: 'See how trading works', text: 'Orders, trading hours, settlement and price bands: what actually happens when you press buy.' },
  { title: 'Read price and volume', text: 'Open, high, low, close, volume and simple charts. Learn to describe what a stock did before explaining why.' },
  { title: 'Know two ways to analyse', text: 'Fundamental analysis asks what a business is worth. Technical analysis studies price behaviour. Most people use both.' },
  { title: 'Manage risk first', text: 'Diversify, size positions small, and only invest money you can afford to lose. Practise with paper trades before using real money.' },
]

const KEY_FACTS = [
  ['Main exchanges', 'NSE and BSE'],
  ['Regulator', 'SEBI'],
  ['Regular session', '9:15 am to 3:30 pm IST, Monday to Friday'],
  ['Settlement', 'T+1 (next working day)'],
  ['Headline indices', 'Nifty 50 (NSE), Sensex (BSE)'],
  ['Where shares are held', 'In a demat account, through a broker'],
]

const MISTAKES = [
  'Buying on tips or social media hype without checking the company.',
  'Putting all your money into one stock or one sector.',
  'Confusing a low share price with a cheap company. Price per share says little without the number of shares and the earnings.',
  'Trading without a plan for when to sell, or without a stop-loss.',
  'Trading too often. Brokerage, taxes and other charges eat into returns.',
  'Expecting a recent winner to keep winning. Past performance does not guarantee future results.',
]

const RESOURCES = [
  { name: 'NSE India', url: 'https://www.nseindia.com', note: 'Official exchange site: market data, reports and investor education.' },
  { name: 'SEBI Investor', url: 'https://investor.sebi.gov.in', note: 'The regulator\'s investor awareness portal.' },
  { name: 'BSE India', url: 'https://www.bseindia.com', note: 'The other major exchange, with its own data and guides.' },
  { name: 'Zerodha Varsity', url: 'https://zerodha.com/varsity', note: 'Free, beginner-friendly modules on markets, analysis and risk.' },
]

const TOC = [
  ['intro', 'Overview'],
  ['path', 'Study path'],
  ['facts', 'Key facts'],
  ['glossary', 'Glossary'],
  ['mistakes', 'Common mistakes'],
  ['resources', 'Resources'],
]

function initialTheme(): 'light' | 'dark' {
  try {
    const saved = localStorage.getItem('theme')
    if (saved === 'light' || saved === 'dark') return saved
  } catch {
    // storage may be unavailable
  }
  return window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark'
}

export default function Learn() {
  const [theme, setTheme] = useState<'light' | 'dark'>(initialTheme)
  const [query, setQuery] = useState('')
  const [category, setCategory] = useState<Category | 'All'>('All')

  useEffect(() => {
    document.documentElement.dataset.theme = theme
    try {
      localStorage.setItem('theme', theme)
    } catch {
      // ignore
    }
  }, [theme])

  useEffect(() => {
    document.title = 'Learn the stock market · Stock Analysis System'
    window.scrollTo(0, 0)
    return () => {
      document.title = 'Stock Analysis System'
    }
  }, [])

  const shown = useMemo(() => {
    const q = query.trim().toLowerCase()
    return TERMS.filter(
      (t) =>
        (category === 'All' || t.category === category) &&
        (!q || t.term.toLowerCase().includes(q) || t.meaning.toLowerCase().includes(q)),
    ).sort((a, b) => a.term.localeCompare(b.term))
  }, [query, category])

  const goTo = (id: string) => document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' })

  return (
    <>
      <a
        className="skip-link"
        href="#learn-main"
        onClick={(e) => {
          e.preventDefault()
          document.getElementById('learn-main')?.focus()
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
            <a href="#/learn" className="active">Learn</a>
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

      <main className="page" id="learn-main" tabIndex={-1}>
        <section id="intro" className="section hero">
          <p className="eyebrow">Study guide</p>
          <h1>Understanding the stock market</h1>
          <p className="lead">
            A plain-language introduction to how stock markets work, how to study them, and what the important terms
            mean. It is written for beginners and uses Indian markets (NSE and BSE) as the example.
          </p>
          <div className="toc" role="navigation" aria-label="On this page">
            {TOC.map(([id, label]) => (
              <button key={id} onClick={() => goTo(id)}>{label}</button>
            ))}
          </div>
          <p className="banner">
            This page is for education only. It is not investment advice, and nothing here recommends buying or selling
            any security.
          </p>
        </section>

        <section id="path" className="section">
          <h2>A simple study path</h2>
          <p className="section-lead">Work through these in order. Each step builds on the one before it.</p>
          <ol className="steps">
            {STUDY_PATH.map((s, i) => (
              <li key={s.title}>
                <span>{i + 1}</span>
                <div><strong>{s.title}</strong><p>{s.text}</p></div>
              </li>
            ))}
          </ol>
        </section>

        <section id="facts" className="section">
          <h2>Key facts about Indian markets</h2>
          <p className="section-lead">The essentials to remember before you read anything else.</p>
          <div className="card">
            <dl className="facts">
              {KEY_FACTS.map(([label, value]) => (
                <div key={label}><dt>{label}</dt><dd>{value}</dd></div>
              ))}
            </dl>
          </div>
        </section>

        <section id="glossary" className="section">
          <h2>Glossary of important terms</h2>
          <p className="section-lead">
            {TERMS.length} terms, grouped by topic. The "In this project" group explains the words used on the
            dashboard.
          </p>
          <div className="toolbar">
            <label className="search">
              <SearchIcon />
              <input
                placeholder="Search a term, e.g. P/E"
                aria-label="Search the glossary"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
              />
            </label>
            {(['All', ...CATEGORIES] as (Category | 'All')[]).map((c) => (
              <button
                key={c}
                className={`chip-btn ${category === c ? 'active' : ''}`}
                onClick={() => setCategory(c)}
                aria-pressed={category === c}
              >
                {c}
              </button>
            ))}
            <span className="muted">Showing {shown.length} of {TERMS.length}</span>
          </div>

          <dl className="glossary">
            {shown.map((t) => (
              <div key={t.term} className="gloss-item">
                <dt>
                  {t.term}
                  <span className="gloss-cat">{t.category}</span>
                </dt>
                <dd>{t.meaning}</dd>
              </div>
            ))}
          </dl>
          {shown.length === 0 && <div className="state">No terms match your search.</div>}
        </section>

        <section id="mistakes" className="section">
          <h2>Common beginner mistakes</h2>
          <p className="section-lead">Most early losses come from habits rather than bad luck.</p>
          <ul className="mistakes">
            {MISTAKES.map((m) => <li key={m}>{m}</li>)}
          </ul>
        </section>

        <section id="resources" className="section">
          <h2>Where to learn more</h2>
          <p className="section-lead">Reliable sources to continue from here.</p>
          <div className="resources">
            {RESOURCES.map((r) => (
              <a key={r.url} className="card resource" href={r.url} target="_blank" rel="noreferrer">
                <strong>{r.name}</strong>
                <span className="muted">{r.note}</span>
              </a>
            ))}
          </div>
          <p className="back-link"><a href="#/">Back to the dashboard</a></p>
        </section>
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
