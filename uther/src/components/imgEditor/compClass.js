import * as C2S from 'canvas2svg'

const translations = {
  zh: {
    select: "选择",
    rectangle: "矩形",
    circle: "圆形",
    line: "直线",
    arrow: "箭头",
    text: "文字",
    freehand: "自由绘制",
    color: "颜色:",
    lineWidth: "线宽:",
    lineDash: "虚线:",
    fillStyle: "填充:",
    solid: "实线",
    dashed: "虚线",
    dotted: "点线",
    none: "无",
    fillSolid: "实心",
    fillTransparent: "半透明",
    undo: "撤销",
    redo: "重做",
    delete: "删除",
    save: "保存",
    textPlaceholder: "输入文字..."
  },
  en: {
    select: "Select",
    rectangle: "Rectangle",
    circle: "Circle",
    line: "Line",
    arrow: "Arrow",
    text: "Text",
    freehand: "Freehand",
    color: "Color:",
    lineWidth: "Width:",
    lineDash: "Dash:",
    fillStyle: "Fill:",
    solid: "Solid",
    dashed: "Dashed",
    dotted: "Dotted",
    none: "None",
    fillSolid: "Solid",
    fillTransparent: "Transparent",
    undo: "Undo",
    redo: "Redo",
    delete: "Delete",
    save: "Save",
    textPlaceholder: "Enter text..."
  }
}

export default class ImageAnnotator {
  constructor(imageId, canvasId) {
    this.image = document.getElementById(imageId)
    this.canvas = document.getElementById(canvasId)
    this.ctx = this.canvas.getContext('2d')
    this.tools = document.querySelectorAll('.tool-btn')
    this.colorPicker = document.getElementById('color-picker')
    this.lineWidth = document.getElementById('line-width')
    this.lineDash = document.getElementById('line-dash')
    this.fillStyle = document.getElementById('fill-style')
    this.textInputContainer = document.querySelector('.text-input-container')
    this.textInput = document.getElementById('text-input')

    this.currentTool = 'select'
    this.isDrawing = false
    this.annotations = []
    this.history = []
    this.historyIndex = -1
    this.selectedAnnotation = null
    this.startX = 0
    this.startY = 0
    this.freehandPoints = []
    this.currentLanguage = 'zh'
    //  this.annotationCounter = 1
    this.init()
  }

  init() {
    // 设置canvas尺寸与图片相同
    this.resizeCanvas()
    window.addEventListener('resize', () => this.resizeCanvas())

    // 初始化工具按钮事件
    this.tools.forEach(tool => {
      if (tool.dataset.tool) {
        tool.addEventListener('click', () => {
          this.textInputContainer.style.display = 'none'
          this.currentTool = tool.dataset.tool
          this.canvas.classList.add('drawing')
          this.canvas.style.cursor = 'crosshair'
          /*  if (this.currentTool === 'select') {
             this.canvas.classList.remove('drawing');
             this.canvas.style.cursor = 'pointer';
           } else {
             this.canvas.classList.add('drawing');
             this.canvas.style.cursor = 'crosshair';
           } */
          this.updateToolButtons()
        })
      } else if (tool.dataset.action === 'delete') {
        tool.addEventListener('click', () => this.deleteSelected())
      } else if (tool.dataset.action === 'save') {
        tool.addEventListener('click', () => this.saveImage())
      } else if (tool.dataset.action === 'undo') {
        tool.addEventListener('click', () => this.undo())
      } else if (tool.dataset.action === 'redo') {
        tool.addEventListener('click', () => this.redo())
      }
    })

    // 画布事件监听
    this.setupCanvasEvents()

    // 样式控制事件
    this.colorPicker.addEventListener('change', () => this.updateSelectedStyle())
    this.lineWidth.addEventListener('input', () => this.updateSelectedStyle())
    this.lineDash.addEventListener('change', () => this.updateSelectedStyle())
    this.fillStyle.addEventListener('change', () => this.updateSelectedStyle())

    // 文字输入事件
    this.textInput.addEventListener('keydown', e => {
      if (e.key === 'Enter') {
        this.addTextAnnotation()
      } else if (e.key === 'Escape') {
        this.cancelTextAnnotation()
      }
    })
  }

  setupCanvasEvents() {
    // 桌面端事件
    this.canvas.addEventListener('mousedown', this.handleStart.bind(this)) // 添加鼠标按下事件监听
    this.canvas.addEventListener('mousemove', this.handleMove.bind(this))
    this.canvas.addEventListener('mouseup', this.handleEnd.bind(this))
    this.canvas.addEventListener('mouseleave', this.handleEnd.bind(this))
  }
  // 处理开始事件
  handleStart(e) {
    // 获取canvas的边界矩形
    const rect = this.canvas.getBoundingClientRect()
    // 获取鼠标点击的起始坐标
    this.startX = e.clientX - rect.left
    this.startY = e.clientY - rect.top
    if (this.currentTool === 'select') {
      this.selectedAnnotation = this.getAnnotationAtPosition(this.startX, this.startY)
      if (this.selectedAnnotation) {
        this.updateStyleControls()
      }
      this.redrawCanvas()
    } else if (this.currentTool === 'text') {
      this.startTextAnnotation(this.startX, this.startY)
    } else if (this.currentTool === 'freehand') {
      this.isDrawing = true
      this.freehandPoints = [{ x: this.startX, y: this.startY }]
    } else {
      this.isDrawing = true
    }
  }

  handleMove(e) {
    if (!this.isDrawing) return

    const rect = this.canvas.getBoundingClientRect()
    const currentX = e.clientX - rect.left
    const currentY = e.clientY - rect.top

    // 清除画布并重绘所有标注
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height)
    this.redrawAnnotations()

    // 设置当前样式
    this.setDrawingStyle()

    // 绘制当前正在创建的标注
    if (this.currentTool === 'freehand') {
      this.freehandPoints.push({ x: currentX, y: currentY })
      this.drawFreehand()
    } else {
      switch (this.currentTool) {
        case 'rectangle':
          this.ctx.beginPath()
          this.ctx.rect(
            this.startX,
            this.startY,
            currentX - this.startX,
            currentY - this.startY
          )
          this.strokeAndFill()
          break

        case 'circle':
          const radius = Math.sqrt(
            Math.pow(currentX - this.startX, 2) + Math.pow(currentY - this.startY, 2)
          )
          this.ctx.beginPath()
          this.ctx.arc(this.startX, this.startY, radius, 0, Math.PI * 2)
          this.strokeAndFill()
          break

        case 'line':
          this.ctx.beginPath()
          this.ctx.moveTo(this.startX, this.startY)
          this.ctx.lineTo(currentX, currentY)
          this.ctx.stroke()
          break

        case 'arrow':
          this.drawArrow(this.startX, this.startY, currentX, currentY)
          break
      }
    }
  }

  handleEnd(e) {
    if (!this.isDrawing && this.currentTool !== 'text') return
    const rect = this.canvas.getBoundingClientRect()
    const endX = e.clientX - rect.left
    const endY = e.clientY - rect.top

    if (this.currentTool === 'freehand') {
      if (this.freehandPoints.length > 1) {
        this.saveAnnotation({
          tool: 'freehand',
          points: this.freehandPoints,
          id: this.generateId(),
          // number: this.annotationCounter++
        })
      }
      this.freehandPoints = []
      this.isDrawing = false
      this.redrawCanvas()
      return
    }

    if (Math.abs(endX - this.startX) < 5 && Math.abs(endY - this.startY) < 5) {
      // 忽略太小的绘制
      this.isDrawing = false
      this.redrawCanvas()
      return
    }

    // 创建新标注
    const annotation = {
      tool: this.currentTool,
      points: [this.startX, this.startY, endX, endY],
      id: this.generateId(),
      // number: this.annotationCounter++
    }

    this.saveAnnotation(annotation)
    this.isDrawing = false
  }

  saveAnnotation(annotation) {
    if (!annotation.text && this.currentTool == 'text') return
    // 保存当前样式到标注
    annotation.color = this.colorPicker.value
    annotation.lineWidth = parseInt(this.lineWidth.value)
    annotation.lineDash = this.lineDash.value
    annotation.fillStyle = this.fillStyle.value

    // 添加到历史记录
    this.addToHistory()
    this.annotations.push(annotation)
    this.selectedAnnotation = annotation
    this.redrawCanvas()
  }

  addToHistory() {
    // 截断历史记录中当前位置之后的部分
    this.history = this.history.slice(0, this.historyIndex + 1)
    // 添加新状态
    this.history.push(JSON.stringify(this.annotations))
    this.historyIndex++
  }

  undo() {
    if (this.historyIndex > 0) {
      this.historyIndex--
      this.annotations = JSON.parse(this.history[this.historyIndex])
      this.selectedAnnotation = null
      this.redrawCanvas()
    }
  }

  redo() {
    if (this.historyIndex < this.history.length - 1) {
      this.historyIndex++
      this.annotations = JSON.parse(this.history[this.historyIndex])
      this.selectedAnnotation = null
      this.redrawCanvas()
    }
  }

  setDrawingStyle() {
    this.ctx.strokeStyle = this.colorPicker.value
    this.ctx.lineWidth = parseInt(this.lineWidth.value)
    this.ctx.fillStyle =
      this.colorPicker.value + (this.fillStyle.value === 'transparent' ? '40' : '')

    switch (this.lineDash.value) {
      case 'dashed':
        this.ctx.setLineDash([10, 5])
        break
      case 'dotted':
        this.ctx.setLineDash([2, 3])
        break
      default:
        this.ctx.setLineDash([])
    }
  }

  strokeAndFill() {
    this.ctx.stroke()
    if (this.fillStyle.value !== 'none') {
      this.ctx.fill()
    }
  }

  drawArrow(fromX, fromY, toX, toY, ctx) {
    ctx = ctx || this.ctx
    const headLength = 15
    const angle = Math.atan2(toY - fromY, toX - fromX)

    // 绘制线条
    ctx.beginPath()
    ctx.moveTo(fromX, fromY)
    ctx.lineTo(toX, toY)
    ctx.stroke()

    // 绘制箭头
    ctx.beginPath()
    ctx.moveTo(toX, toY)
    ctx.lineTo(
      toX - headLength * Math.cos(angle - Math.PI / 6),
      toY - headLength * Math.sin(angle - Math.PI / 6)
    )
    ctx.moveTo(toX, toY)
    ctx.lineTo(
      toX - headLength * Math.cos(angle + Math.PI / 6),
      toY - headLength * Math.sin(angle + Math.PI / 6)
    )
    ctx.stroke()
  }

  drawFreehand() {
    if (this.freehandPoints.length < 2) return

    this.ctx.beginPath()
    this.ctx.moveTo(this.freehandPoints[0].x, this.freehandPoints[0].y)

    for (let i = 1; i < this.freehandPoints.length; i++) {
      this.ctx.lineTo(this.freehandPoints[i].x, this.freehandPoints[i].y)
    }

    this.ctx.stroke()
  }

  startTextAnnotation(x, y) {
    this.textInput.placeholder = translations[this.currentLanguage].textPlaceholder
    this.textInput.value = ''
    this.textInputContainer.style.display = 'block'
    this.textInputContainer.style.left = `${x}px`
    this.textInputContainer.style.top = `${y}px`
    this.textInput.focus()
  }

  addTextAnnotation() {
    const text = this.textInput.value.trim()
    if (text) {
      // const rect = this.textInputContainer.getBoundingClientRect()
      let { offsetLeft, offsetTop } = this.textInputContainer
      const annotation = {
        tool: 'text',
        text: text,
        points: [offsetLeft, offsetTop + 20],
        id: this.generateId(),
        //  number: this.annotationCounter++,
        color: this.colorPicker.value,
        lineWidth: parseInt(this.lineWidth.value),
        font: '14px Arial'// `${this.lineWidth.value * 13}px Arial`
      }

      this.addToHistory()
      this.annotations.push(annotation)
      this.selectedAnnotation = annotation
    }

    this.cancelTextAnnotation()
    this.redrawCanvas()
  }

  cancelTextAnnotation() {
    this.textInputContainer.style.display = 'none'
  }

  redrawCanvas() {
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height)
    this.redrawAnnotations()

    // 高亮选中的标注
    if (this.selectedAnnotation) {
      const { tool, points } = this.selectedAnnotation
      this.ctx.strokeStyle = '#00ff00'
      this.ctx.lineWidth = 2
      this.ctx.setLineDash([5, 5])

      if (tool === 'rectangle') {
        this.ctx.strokeRect(
          points[0],
          points[1],
          points[2] - points[0],
          points[3] - points[1]
        )
      } else if (tool === 'circle') {
        const radius = Math.sqrt(
          Math.pow(points[2] - points[0], 2) + Math.pow(points[3] - points[1], 2)
        )
        this.ctx.beginPath()
        this.ctx.arc(points[0], points[1], radius, 0, Math.PI * 2)
        this.ctx.stroke()
      } else if (tool === 'line' || tool === 'arrow') {
        this.drawArrow(points[0], points[1], points[2], points[3])
      } else if (tool === 'freehand') {
        if (points.length > 1) {
          this.ctx.beginPath()
          this.ctx.moveTo(points[0].x, points[0].y)
          for (let i = 1; i < points.length; i++) {
            this.ctx.lineTo(points[i].x, points[i].y)
          }
          this.ctx.stroke()
        }
      } else if (tool === 'text') {
        this.ctx.strokeRect(points[0], points[1] - 20, 150, 28)
      }

      this.ctx.setLineDash([])
    }
  }

  redrawAnnotations(data) {
    let ctx = data ? data.ctx : this.ctx
    this.annotations.forEach(annotation => {
      const { tool, color, points, lineWidth, lineDash, fillStyle, text, font, number } = annotation
      ctx.strokeStyle = color
      ctx.lineWidth = lineWidth
      ctx.fillStyle =
        fillStyle !== 'none' ? color + (fillStyle === 'transparent' ? '40' : '') : ''

      // svg 不支持，svg导出时，注释掉
      /*   switch (lineDash) {
          case 'dashed':
            ctx.setLineDash([10, 5])
            break
          case 'dotted':
            ctx.setLineDash([2, 3])
            break
          default:
            ctx.setLineDash([])
        } */
      // ==end
      if (tool === 'rectangle') {
        ctx.beginPath()
        ctx.rect(points[0], points[1], points[2] - points[0], points[3] - points[1])
        ctx.stroke()
        //  if (fillStyle !== 'none') ctx.fill()
      } else if (tool === 'circle') {
        const radius = Math.sqrt(
          Math.pow(points[2] - points[0], 2) + Math.pow(points[3] - points[1], 2)
        )
        ctx.beginPath()
        ctx.arc(points[0], points[1], radius, 0, Math.PI * 2)
        ctx.stroke()
        if (fillStyle !== 'none') ctx.fill()
      } else if (tool === 'line') {
        ctx.beginPath()
        ctx.moveTo(points[0], points[1])
        ctx.lineTo(points[2], points[3])
        ctx.stroke()
      } else if (tool === 'arrow') {
        this.drawArrow(points[0], points[1], points[2], points[3], ctx)
      } else if (tool === 'freehand') {
        if (points.length > 1) {
          ctx.beginPath()
          ctx.moveTo(points[0].x, points[0].y)
          for (let i = 1; i < points.length; i++) {
            ctx.lineTo(points[i].x, points[i].y)
          }
          ctx.stroke()
        }
      } else if (tool === 'text') {
        ctx.font = font || '16px Arial'
        /*  ctx.strokeStyle = color;
         ctx.strokeText(text, points[0], points[1]);
         console.log(text) */
        ctx.fillStyle = color
        ctx.textDecoration = 'none'
        ctx.fontWeight = '500'
        ctx.fillText(text, points[0], points[1])
      }

      // 绘制编号
      /* if (number && tool !== 'text') {
        ctx.font = 'bold 14px Arial'
        ctx.fillStyle = '#000000'
        ctx.strokeStyle = '#ffffff'
        ctx.lineWidth = 3

        let x, y
        if (tool === 'rectangle') {
          x = points[0] + 5
          y = points[1] + 18
        } else if (tool === 'circle') {
          x = points[0] - 5
          y = points[1] - 5
        } else if (tool === 'line' || tool === 'arrow') {
          x = (points[0] + points[2]) / 2
          y = (points[1] + points[3]) / 2
        } else if (tool === 'freehand') {
          x = points[0].x
          y = points[0].y
        }

        ctx.strokeText(number.toString(), x, y)
        ctx.fillText(number.toString(), x, y)
      } */

      // ctx.setLineDash([])
    })
  }

  getAnnotationAtPosition(x, y) {
    // 从后往前检查，这样最后添加的标注会优先被选中
    for (let i = this.annotations.length - 1; i >= 0; i--) {
      const annotation = this.annotations[i]

      if (annotation.tool === 'rectangle') {
        const [x1, y1, x2, y2] = annotation.points
        const minX = Math.min(x1, x2)
        const maxX = Math.max(x1, x2)
        const minY = Math.min(y1, y2)
        const maxY = Math.max(y1, y2)

        if (x >= minX && x <= maxX && y >= minY && y <= maxY) {
          return annotation
        }
      } else if (annotation.tool === 'circle') {
        const [x1, y1, x2, y2] = annotation.points
        const radius = Math.sqrt(Math.pow(x2 - x1, 2) + Math.pow(y2 - y1, 2))
        const distance = Math.sqrt(Math.pow(x - x1, 2) + Math.pow(y - y1, 2))

        if (distance <= radius) {
          return annotation
        }
      } else if (annotation.tool === 'line' || annotation.tool === 'arrow') {
        const [x1, y1, x2, y2] = annotation.points
        if (this.isPointNearLine(x, y, x1, y1, x2, y2, 5)) {
          return annotation
        }
      } else if (annotation.tool === 'freehand') {
        for (let i = 0; i < annotation.points.length; i++) {
          const point = annotation.points[i]
          const distance = Math.sqrt(Math.pow(x - point.x, 2) + Math.pow(y - point.y, 2))
          if (distance <= 5) {
            return annotation
          }
        }
      } else if (annotation.tool === 'text') {
        this.ctx.font = annotation.font || '16px Arial'
        const textWidth = this.ctx.measureText(annotation.text).width
        if (
          x >= annotation.points[0] &&
          x <= annotation.points[0] + textWidth &&
          y >= annotation.points[1] - 16 &&
          y <= annotation.points[1]
        ) {
          return annotation
        }
      }
    }
    return null
  }

  isPointNearLine(px, py, x1, y1, x2, y2, threshold) {
    // 计算点到线段的距离
    const A = px - x1
    const B = py - y1
    const C = x2 - x1
    const D = y2 - y1

    const dot = A * C + B * D
    const len_sq = C * C + D * D
    let param = -1
    if (len_sq !== 0) param = dot / len_sq

    let xx, yy

    if (param < 0) {
      xx = x1
      yy = y1
    } else if (param > 1) {
      xx = x2
      yy = y2
    } else {
      xx = x1 + param * C
      yy = y1 + param * D
    }

    const dx = px - xx
    const dy = py - yy
    return Math.sqrt(dx * dx + dy * dy) < threshold
  }

  updateSelectedStyle() {
    if (!this.selectedAnnotation) return

    this.selectedAnnotation.color = this.colorPicker.value
    this.selectedAnnotation.lineWidth = parseInt(this.lineWidth.value)
    this.selectedAnnotation.lineDash = this.lineDash.value
    this.selectedAnnotation.fillStyle = this.fillStyle.value

    this.redrawCanvas()
  }

  updateStyleControls() {
    if (!this.selectedAnnotation) return

    const { color, lineWidth, lineDash, fillStyle } = this.selectedAnnotation
    this.colorPicker.value = color
    this.lineWidth.value = lineWidth
    this.lineDash.value = lineDash
    this.fillStyle.value = fillStyle
  }

  deleteSelected() {
    if (!this.selectedAnnotation) return

    this.addToHistory()

    const index = this.annotations.findIndex(a => a.id === this.selectedAnnotation.id)
    if (index !== -1) {
      this.annotations.splice(index, 1)
      this.selectedAnnotation = null
      this.redrawCanvas()
    }
  }

  updateToolButtons() {
    for (let i = 0; i < this.tools.length; i++) {
      const tool = this.tools[i]
      if (tool.dataset.tool === this.currentTool) {
        tool.classList.toggle('active')
        if (tool.classList.contains('active')) {
          this.currentTool = tool.dataset.tool
        } else {
          this.currentTool = ''
        }
      } else {
        tool.classList.remove('active')
      }
    }
  }

  resizeCanvas() {
    this.canvas.width = this.image.offsetWidth
    this.canvas.height = this.image.offsetHeight
    this.redrawCanvas()
  }

  saveImage() {
    // 创建一个临时canvas合并图片和标注
    // 导出png
    /* const tempCanvas = document.createElement('canvas')
    tempCanvas.width = this.canvas.width
    tempCanvas.height = this.canvas.height
    const tempCtx = tempCanvas.getContext('2d')

    // 先绘制图片
    tempCtx.drawImage(this.image, 0, 0, tempCanvas.width, tempCanvas.height)

    // 再绘制标注
    this.redrawAnnotations.call({ ctx: tempCtx, annotations: this.annotations, drawArrow: this.drawArrow })
    // 也可以保存标注数据到服务器
    return tempCanvas.toDataURL('image/png') */

    // 导出SVG
    const tempCanvas = new C2S(this.canvas.width, this.canvas.height)

    // 先绘制图片
    tempCanvas.drawImage(this.image, 0, 0, tempCanvas.width, tempCanvas.height)

    // 再绘制标注
    this.redrawAnnotations.call({ ctx: tempCanvas, annotations: this.annotations, drawArrow: this.drawArrow })
    const xml = new XMLSerializer().serializeToString(tempCanvas.getSvg())
    const encoded = encodeURIComponent(xml)
    return `data:image/svg+xml;charset=utf-8,${encoded}`
    // 导出为图片
    /* const link = document.createElement('a')
    link.download = 'defect-annotation-' + new Date().toISOString().slice(0, 10) + '.png'
    link.href = tempCanvas.toDataURL('image/png')
    link.click() */
  }

  saveAnnotationsToServer() {
    // 这里添加将标注数据保存到服务器的代码
    const annotationData = {
      image: this.image.src,
      annotations: this.annotations,
      timestamp: new Date().toISOString()
    }

    //  console.log('Saving annotations:', annotationData)
    // 实际项目中可以使用fetch或axios发送数据到服务器
    /*
  fetch('/api/save-annotations', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(annotationData)
  })
  .then(response => response.json())
  .then(data => {
    console.log('Success:', data);
  })
  .catch((error) => {
    console.error('Error:', error);
  });
  */
  }

  generateId() {
    return Date.now().toString(36) + Math.random().toString(36).substr(2)
  }

}
