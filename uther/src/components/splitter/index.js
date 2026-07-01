/**
 * 流畅拖拽分割线重构版 (完美适配多盒子 Flexbox 布局)
 * @param {PointerEvent} evt - 指针事件对象
 * @param {string} splitDomId - 分割线 DOM ID
 * @param {'vertical' | 'horizontal'} splitType - 分割类型 (vertical: 左右拖动, horizontal: 上下拖动)
 * @param {{ min: number, max: number }} limit - 面板1的最小和最大尺寸限制(像素)
 */
export const mouseEvent = (evt, splitDomId, splitType, limit = { min: 50, max: 1000 }) => {
  const splitter = document.getElementById(splitDomId)
  const pane1 = splitter.previousElementSibling
  const pane2 = splitter.nextElementSibling

  if (!splitter || !pane1 || !pane2) return

  // 1. 状态判断与初始值记录
  const isVertical = splitType === 'vertical'
  const initialMousePos = isVertical ? evt.clientX : evt.clientY

  // ==========================================
  // 【核心修复】：在拖拽开始瞬间，锁定所有盒子的物理像素尺寸
  // ==========================================
  const parentContainer = splitter.parentNode
  const allPanes = Array.from(parentContainer.children).filter(child => child !== splitter && child.nodeType === 1)

  // 将所有盒子（包括 pane1 和 pane2）全部强制冻结为当前的实际像素尺寸
  // 这样可以消除 flex: 1 带来的动态分配误差，让整个容器在 mousedown 瞬间变成绝对的物理布局
  allPanes.forEach(pane => {
    const currentSize = isVertical ? pane.getBoundingClientRect().width : pane.getBoundingClientRect().height
    pane.style.flex = 'none' // 等同于 flex: 0 0 auto，彻底脱离弹性伸缩
    if (isVertical) {
      pane.style.width = `${currentSize}px`
    } else {
      pane.style.height = `${currentSize}px`
    }
  })

  // 冻结完成后，再获取 pane1 和 pane2 的绝对初始尺寸（此时它们已经是固定像素值，极其稳定）
  const initialPane1Size = isVertical ? pane1.getBoundingClientRect().width : pane1.getBoundingClientRect().height
  const initialPane2Size = isVertical ? pane2.getBoundingClientRect().width : pane2.getBoundingClientRect().height

  // 最后，唯独让 pane2 恢复弹性：让它吸收拖拽产生的所有空间变化
  pane2.style.flex = '1 1 0%'
  if (isVertical) {
    pane2.style.width = ''
  } else {
    pane2.style.height = ''
  }
  // ==========================================

  // 2. 拖拽准备：添加全局样式，优化体验
  document.body.style.userSelect = 'none'
  document.body.style.cursor = isVertical ? 'col-resize' : 'row-resize'
  pane1.style.pointerEvents = 'none'
  pane2.style.pointerEvents = 'none'

  let requestedFrame = null
  let deltaPos = 0

  // 3. 拖拽中逻辑
  const onPointerMove = (ev) => {
    deltaPos = (isVertical ? ev.clientX : ev.clientY) - initialMousePos

    if (!requestedFrame) {
      requestedFrame = requestAnimationFrame(() => {
        if (isVertical) {
          splitter.style.transform = `translateX(${deltaPos}px)`
        } else {
          splitter.style.transform = `translateY(${deltaPos}px)`
        }
        requestedFrame = null
      })
    }
  }

  // 4. 拖拽结束逻辑
  const onPointerUp = () => {
    document.removeEventListener('pointermove', onPointerMove)
    document.removeEventListener('pointerup', onPointerUp)
    if (requestedFrame) cancelAnimationFrame(requestedFrame)

    // 清理全局样式
    document.body.style.userSelect = ''
    document.body.style.cursor = ''
    pane1.style.pointerEvents = ''
    pane2.style.pointerEvents = ''

    // 清除分割线的 transform
    splitter.style.transform = ''

    // 5. 计算并应用最终尺寸
    let newPane1Size = initialPane1Size + deltaPos

    // 动态计算最大允许尺寸：不能把 pane2 挤到小于它的最小限制
    const pane2MinSize = limit.min
    const maxAllowedSize = initialPane1Size + initialPane2Size - pane2MinSize
    const effectiveMax = Math.min(limit.max, maxAllowedSize)

    if (newPane1Size < limit.min) newPane1Size = limit.min
    if (newPane1Size > effectiveMax) newPane1Size = effectiveMax

    // 应用新的尺寸给 pane1
    pane1.style.flex = `0 0 ${newPane1Size}px`
    if (isVertical) {
      pane1.style.width = `${newPane1Size}px`
    } else {
      pane1.style.height = `${newPane1Size}px`
    }

    // pane2 保持弹性吸收剩余空间
    pane2.style.flex = '1 1 0%'
    if (isVertical) {
      pane2.style.width = ''
    } else {
      pane2.style.height = ''
    }

    // ==========================================
    // 【核心修复】：将其他不相邻盒子也固化为当前的实际尺寸，不再恢复为 flex: 1
    // ==========================================
    allPanes.forEach(pane => {
      if (pane !== pane1 && pane !== pane2) {
        const currentSize = isVertical ? pane.getBoundingClientRect().width : pane.getBoundingClientRect().height
        // 保持冻结状态，防止被 pane2 的弹性挤压
        pane.style.flex = `0 0 ${currentSize}px`
        if (isVertical) {
          pane.style.width = `${currentSize}px`
        } else {
          pane.style.height = `${currentSize}px`
        }
      }
    })
    // ==========================================
  }

  document.addEventListener('pointermove', onPointerMove)
  document.addEventListener('pointerup', onPointerUp)
}

// 初始化行高度，定位或者新增行，删减行可用
export const rowResize = (fatherId) => {
  const father = document.getElementById(fatherId)
  if (!father) return

  // 1. 强制父容器为垂直 Flex 布局，并占满高度
  father.style.display = 'flex'
  father.style.flexDirection = 'column'
  father.style.height = '100%' // 或者根据实际业务保留原有的高度设定

  const items = father.querySelectorAll('.rowbox')
  const splitItems = father.querySelectorAll('.x-splitter-horizontal')

  // 2. 重置所有 rowbox：均分剩余空间，清除写死的定位和尺寸
  items.forEach(item => {
    item.style.flex = '1 1 0%'      // 核心：自动伸缩，平分高度
    item.style.height = ''          // 清除拖拽时可能写死的 height
    item.style.minHeight = '0'      // 防止内容撑开容器导致无法收缩
    item.style.overflow = ''  // 防止内容溢出

    // 清除旧代码遗留的绝对定位属性
    item.style.position = ''
    item.style.top = ''
  })

  // 3. 重置所有拖拽条：固定高度，不参与弹性伸缩
  splitItems.forEach(split => {
    split.style.flex = '0 0 4px'    // 核心代码：4px 为分割线高度，不放大不缩小
    split.style.height = ''         // 清除可能写死的 height
    split.style.width = '100%'      // 宽度占满
    split.style.cursor = 'row-resize'

    // 清除旧代码遗留的绝对定位属性
    split.style.position = ''
    split.style.top = ''
    split.style.left = ''
  })
}

// 初始化行内的列宽度，定位 or 响应新增列，删减列可用
export const colResize = (fatherId) => {
  const father = document.getElementById(fatherId)
  if (!father) return

  // 查找所有行容器，如果没有行容器，说明传入的就是行容器本身
  const rowDoms = father.querySelectorAll('.rowbox')
  const targetRows = rowDoms.length > 0 ? rowDoms : [father]

  // 遍历每一行，重置其内部的列布局
  targetRows.forEach(row => {
    // 1. 强制行容器为水平 Flex 布局
    row.style.display = 'flex'
    row.style.flexDirection = 'row'
    row.style.width = '100%'
    row.style.height = '100%' // 确保占满行高度

    const items = row.querySelectorAll('.colbox')
    const splitItems = row.querySelectorAll('.splitter')

    // 2. 重置所有 colbox：均分剩余空间，清除写死的定位和尺寸
    items.forEach(item => {
      item.style.flex = '1 1 0%'      // 核心：自动伸缩，平分宽度
      item.style.width = ''           // 清除拖拽时可能写死的 width
      item.style.minWidth = '0'       // 防止内容撑开容器导致无法收缩
      item.style.overflow = ''  // 防止内容溢出

      // 清除旧代码遗留的绝对定位属性
      item.style.position = ''
      item.style.left = ''
    })

    // 3. 重置所有拖拽条：固定宽度，不参与弹性伸缩
    splitItems.forEach(split => {
      split.style.flex = '0 0 4px'    // 核心代码：4px 为分割线宽度，不放大不缩小
      split.style.width = ''          // 清除可能写死的 width
      split.style.height = '100%'     // 高度占满行高
      split.style.cursor = 'col-resize'

      // 清除旧代码遗留的绝对定位属性
      split.style.position = ''
      split.style.top = ''
      split.style.left = ''
    })
  })
}
