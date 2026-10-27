import { useEffect, useState } from 'react'
import App from './App'
import Algorithms from './Algorithms'
import Learn from './Learn'

export default function Router() {
  const [hash, setHash] = useState(window.location.hash)

  useEffect(() => {
    const onChange = () => setHash(window.location.hash)
    window.addEventListener('hashchange', onChange)
    return () => window.removeEventListener('hashchange', onChange)
  }, [])

  if (hash.startsWith('#/learn')) return <Learn />
  if (hash.startsWith('#/algorithms')) return <Algorithms />
  return <App />
}
