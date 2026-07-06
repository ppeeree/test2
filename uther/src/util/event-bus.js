const listeners = new Map()

function getHandlers(event) {
  if (!listeners.has(event)) {
    listeners.set(event, new Set())
  }
  return listeners.get(event)
}

const eventBus = {
  $on(event, handler) {
    getHandlers(event).add(handler)
  },
  $off(event, handler) {
    if (!event) {
      listeners.clear()
      return
    }
    if (!handler) {
      listeners.delete(event)
      return
    }
    const handlers = listeners.get(event)
    if (handlers) handlers.delete(handler)
  },
  $emit(event, ...args) {
    const handlers = listeners.get(event)
    if (!handlers) return
    handlers.forEach(handler => handler(...args))
  }
}

export default eventBus
