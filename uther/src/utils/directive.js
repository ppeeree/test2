const dragDirective = {
  beforeMount(el) {
    bindDrag(el)
  },
  mounted(el) {
    bindDrag(el)
  },
  unmounted(el) {
    unbindDrag(el)
  },
  bind(el) {
    bindDrag(el)
  },
  unbind(el) {
    unbindDrag(el)
  }
}

const preventReClickDirective = {
  beforeMount(el, binding) {
    bindPreventReClick(el, binding)
  },
  mounted(el, binding) {
    bindPreventReClick(el, binding)
  },
  unmounted(el) {
    unbindPreventReClick(el)
  },
  inserted(el, binding) {
    bindPreventReClick(el, binding)
  },
  unbind(el) {
    unbindPreventReClick(el)
  }
}

const directives = {
  drag: dragDirective,
  dialogDrag: dragDirective,
  preventReClick: preventReClickDirective
}

function bindDrag(el) {
  if (el.__dragBound) return
  const dialogHeaderEl = el.querySelector('.el-dialog__header') || el.querySelector('.dialog_header')
  if (!dialogHeaderEl) return

  const dragDom = el
  dialogHeaderEl.style.cursor = 'move'

  const onMouseDown = e => {
    const sty = dragDom.currentStyle || window.getComputedStyle(dragDom, null)
    const disX = e.clientX - dialogHeaderEl.offsetLeft
    const disY = e.clientY - dialogHeaderEl.offsetTop
    let styL
    let styT

    if (sty.left.includes('%')) {
      styL = +document.body.clientWidth * (+sty.left.replace(/%/g, '') / 100)
      styT = +document.body.clientHeight * (+sty.top.replace(/%/g, '') / 100)
    } else {
      styL = +sty.left.replace(/\px/g, '')
      styT = +sty.top.replace(/\px/g, '')
    }

    const onMouseMove = moveEvent => {
      const left = moveEvent.clientX - disX
      const top = moveEvent.clientY - disY
      dragDom.style.left = `${left + styL}px`
      dragDom.style.top = `${top + styT}px`
    }

    const onMouseUp = () => {
      document.removeEventListener('mousemove', onMouseMove)
      document.removeEventListener('mouseup', onMouseUp)
    }

    document.addEventListener('mousemove', onMouseMove)
    document.addEventListener('mouseup', onMouseUp)
  }

  dialogHeaderEl.addEventListener('mousedown', onMouseDown)
  el.__dragBound = { dialogHeaderEl, onMouseDown }
}

function unbindDrag(el) {
  const bound = el.__dragBound
  if (!bound) return
  bound.dialogHeaderEl.removeEventListener('mousedown', bound.onMouseDown)
  delete el.__dragBound
}

function bindPreventReClick(el, binding) {
  if (el.__preventReClickBound) return
  const onClick = () => {
    if (!el.disabled) {
      el.disabled = true
      setTimeout(() => {
        el.disabled = false
      }, binding.value || 1000)
    }
  }
  el.addEventListener('click', onClick)
  el.__preventReClickBound = onClick
}

function unbindPreventReClick(el) {
  if (!el.__preventReClickBound) return
  el.removeEventListener('click', el.__preventReClickBound)
  delete el.__preventReClickBound
}

function install(app) {
  Object.keys(directives).forEach(name => {
    app.directive(name, directives[name])
  })
}

export { directives, dragDirective, preventReClickDirective }
export default { install }
