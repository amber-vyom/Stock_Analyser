export interface Item {
  symbol: string
  score: number
}

export interface HeapStep {
  heap: Item[]
  highlight: number[]
  note: string
  removed: Item[]
}

export function heapSteps(items: Item[], extractCount: number): HeapStep[] {
  const heap: Item[] = []
  const removed: Item[] = []
  const steps: HeapStep[] = []
  const snap = (note: string, highlight: number[] = []) =>
    steps.push({ heap: heap.map((h) => ({ ...h })), highlight, note, removed: [...removed] })

  snap('Start with an empty max-heap. The best score will always sit at the top (index 0).')

  for (const item of items) {
    heap.push(item)
    let i = heap.length - 1
    snap(`Insert ${item.symbol} (${item.score.toFixed(2)}) at the end of the array.`, [i])
    while (i > 0) {
      const parent = Math.floor((i - 1) / 2)
      if (heap[parent].score >= heap[i].score) break
      ;[heap[parent], heap[i]] = [heap[i], heap[parent]]
      snap(`${heap[parent].symbol} beats its parent, so they swap places (sift up).`, [parent, i])
      i = parent
    }
  }

  snap('All stocks are inserted. Now remove the best stock repeatedly (extract-max).', [0])

  for (let n = 0; n < extractCount && heap.length > 0; n++) {
    const top = heap[0]
    const last = heap.pop() as Item
    removed.push(top)
    if (heap.length > 0) {
      heap[0] = last
      snap(`Remove ${top.symbol} (rank ${n + 1}). Move the last item, ${last.symbol}, to the top.`, [0])
      let i = 0
      for (;;) {
        const left = 2 * i + 1
        const right = left + 1
        let largest = i
        if (left < heap.length && heap[left].score > heap[largest].score) largest = left
        if (right < heap.length && heap[right].score > heap[largest].score) largest = right
        if (largest === i) break
        ;[heap[i], heap[largest]] = [heap[largest], heap[i]]
        snap(`${heap[i].symbol} is larger than its child, so they swap (sift down).`, [i, largest])
        i = largest
      }
    } else {
      snap(`Remove ${top.symbol} (rank ${n + 1}). The heap is now empty.`)
    }
  }

  return steps
}

export interface SearchStep {
  low: number
  high: number
  mid: number
  note: string
  found: boolean
}

export function binarySearchSteps(sorted: string[], target: string): SearchStep[] {
  const steps: SearchStep[] = []
  let low = 0
  let high = sorted.length - 1

  while (low <= high) {
    const mid = Math.floor((low + high) / 2)
    const cmp = sorted[mid].localeCompare(target)
    if (cmp === 0) {
      steps.push({ low, high, mid, found: true, note: `Middle is ${sorted[mid]}. That is the symbol we want. Found it.` })
      return steps
    }
    if (cmp < 0) {
      steps.push({ low, high, mid, found: false, note: `Middle is ${sorted[mid]}, which comes before ${target}. Discard the left half.` })
      low = mid + 1
    } else {
      steps.push({ low, high, mid, found: false, note: `Middle is ${sorted[mid]}, which comes after ${target}. Discard the right half.` })
      high = mid - 1
    }
  }

  steps.push({ low, high, mid: -1, found: false, note: `Nothing left to search. ${target} is not in the list.` })
  return steps
}

export interface SortStep {
  values: Item[]
  range: [number, number] | null
  note: string
  comparisons: number
}

export function mergeSortSteps(items: Item[]): SortStep[] {
  const work = items.map((i) => ({ ...i }))
  const buffer = work.map((i) => ({ ...i }))
  const steps: SortStep[] = []
  let comparisons = 0

  const snap = (note: string, range: [number, number] | null) =>
    steps.push({ values: work.map((w) => ({ ...w })), range, note, comparisons })

  snap('Unsorted list. Merge sort splits it in half, sorts each half, then merges them.', null)

  const sort = (left: number, right: number) => {
    if (left >= right) return
    const mid = Math.floor((left + right) / 2)
    sort(left, mid)
    sort(mid + 1, right)

    let i = left
    let j = mid + 1
    let k = left
    while (i <= mid && j <= right) {
      comparisons++
      if (work[i].score >= work[j].score) buffer[k++] = work[i++]
      else buffer[k++] = work[j++]
    }
    while (i <= mid) buffer[k++] = work[i++]
    while (j <= right) buffer[k++] = work[j++]
    for (let n = left; n <= right; n++) work[n] = buffer[n]

    snap(`Merge positions ${left + 1} to ${right + 1}: take the larger front item each time.`, [left, right])
  }

  sort(0, work.length - 1)
  snap('Sorted from best to worst score.', null)
  return steps
}
