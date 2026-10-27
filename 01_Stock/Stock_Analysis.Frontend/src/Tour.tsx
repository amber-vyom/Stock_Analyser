import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react'

interface TourStep {
  selector: string
  title: string
  text: string
}

const STEPS: TourStep[] = [
  {
    selector: '.nav-links',
    title: 'Move around the site',
    text: 'Use these links to jump between sections. The Learn page explains market terms, and Algorithms shows live demos of the code behind the ranking.',
  },
  {
    selector: '.stats',
    title: 'The headline numbers',
    text: 'How many stocks were ranked, the best score, the average score and the average 7-day price change.',
  },
  {
    selector: '.podium',
    title: 'Highest ranked',
    text: 'The three top-scoring stocks. Click one to open its full details in the rankings.',
  },
  {
    selector: '#rankings .table-card',
    title: 'The rankings table',
    text: 'Click a column title to sort, use the search box or the filters above it, and click any row to see why the stock scored the way it did.',
  },
  {
    selector: '#rankings .side',
    title: 'Stock details',
    text: 'The ten pass/fail checks, a breakdown of the score, and the underlying numbers. Use "Add to comparison" to put two stocks side by side.',
  },
  {
    selector: '#insights .histogram',
    title: 'Score distribution',
    text: 'Each bar counts stocks in a score range. Click a bar to filter the rankings to that range.',
  },
  {
    selector: '#experiments',
    title: 'Experiments',
    text: 'A machine-learning experiment and a news-sentiment view. They are kept separate and never change the ranking.',
  },
]

export default function Tour({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [index, setIndex] = useState(0)
  const [rect, setRect] = useState<DOMRect | null>(null)
  const [size, setSize] = useState({ w: 0, h: 0 })
  const popover = useRef<HTMLDivElement>(null)
  const nextButton = useRef<HTMLButtonElement>(null)

  const available = STEPS.filter((s) => document.querySelector(s.selector))
  const step = available[Math.min(index, Math.max(available.length - 1, 0))]

  const measure = useCallback(() => {
    if (!step) return
    const el = document.querySelector(step.selector)
    if (!el) return
    const r = el.getBoundingClientRect()
    const vh = window.innerHeight
    if (r.height > vh * 0.6) {
      const top = Math.max(r.top, 100)
      setRect(new DOMRect(r.left, top, r.width, Math.min(r.height - (top - r.top), vh * 0.45)))
    } else {
      setRect(r)
    }
  }, [step])

  useEffect(() => {
    if (!open) return
    setIndex(0)
  }, [open])

  useEffect(() => {
    if (!open || !step) return
    const el = document.querySelector(step.selector)
    const tall = (el?.getBoundingClientRect().height ?? 0) > window.innerHeight * 0.6
    el?.scrollIntoView({ block: tall ? 'start' : 'center', behavior: 'auto' })
    const timer = window.setTimeout(measure, 350)
    measure()
    return () => window.clearTimeout(timer)
  }, [open, step, measure])

  useEffect(() => {
    if (!open) return
    let frame = 0
    const onChange = () => {
      cancelAnimationFrame(frame)
      frame = requestAnimationFrame(measure)
    }
    window.addEventListener('scroll', onChange, { passive: true })
    window.addEventListener('resize', onChange)
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
      if (e.key === 'ArrowRight') setIndex((i) => Math.min(i + 1, available.length - 1))
      if (e.key === 'ArrowLeft') setIndex((i) => Math.max(i - 1, 0))
    }
    window.addEventListener('keydown', onKey)
    return () => {
      cancelAnimationFrame(frame)
      window.removeEventListener('scroll', onChange)
      window.removeEventListener('resize', onChange)
      window.removeEventListener('keydown', onKey)
    }
  }, [open, measure, onClose, available.length])

  useLayoutEffect(() => {
    if (popover.current) setSize({ w: popover.current.offsetWidth, h: popover.current.offsetHeight })
  }, [index, open, rect])

  useEffect(() => {
    if (open) nextButton.current?.focus({ preventScroll: true })
  }, [open, index])

  if (!open || !step || !rect) return null

  const pad = 8
  const viewportW = window.innerWidth
  const viewportH = window.innerHeight
  const spaceBelow = viewportH - rect.bottom
  const spotTall = rect.height >= viewportH * 0.4
  const placeBelow = spaceBelow > size.h + 24 || rect.top < size.h + 24
  let top = spotTall ? viewportH - size.h - 16 : placeBelow ? rect.bottom + pad + 10 : rect.top - pad - 10 - size.h
  top = Math.max(12, Math.min(top, viewportH - size.h - 12))
  const left = Math.max(12, Math.min(rect.left + rect.width / 2 - size.w / 2, viewportW - size.w - 12))
  const last = index >= available.length - 1

  return (
    <div className="tour" role="dialog" aria-modal="true" aria-label="Guided tour">
      <div
        className="tour-spot"
        style={{ top: rect.top - pad, left: rect.left - pad, width: rect.width + pad * 2, height: rect.height + pad * 2 }}
      />
      <div ref={popover} className="tour-pop" style={{ top, left }}>
        <p className="tour-step">Step {index + 1} of {available.length}</p>
        <h3>{step.title}</h3>
        <p>{step.text}</p>
        <div className="tour-actions">
          <button onClick={onClose}>Skip tour</button>
          <span className="tour-spacer" />
          <button onClick={() => setIndex(Math.max(index - 1, 0))} disabled={index === 0}>Back</button>
          <button ref={nextButton} className="primary" onClick={() => (last ? onClose() : setIndex(index + 1))}>
            {last ? 'Finish' : 'Next'}
          </button>
        </div>
      </div>
    </div>
  )
}
