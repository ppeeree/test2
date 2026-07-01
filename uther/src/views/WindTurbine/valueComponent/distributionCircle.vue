<template>
  <div style="overflow: hidden">
    <div class="circle_wrapper" v-show="isShowMore">
      <div
        class="circle_background"
        :style="{ width: size + 'px', height: size + 'px' }"
        ref="circle_background"
      ></div>
      <div class="circle_canvas" :style="{ left: `calc(50% - ${size / 2}px)` }">
        <canvas
          ref="renderCanvas"
          :width="size"
          :height="size"
          @mousemove="onMove"
          @mouseup="onUp"
          @mouseleave="onUp"
        />
        <!-- 仿 title 的浮层 -->
        <div v-if="tipShow" class="canvas-tip" :style="tipPos">
          <span>{{ tipText }}</span
          >{{ dataList.length > 0 ? dataList[0].evUnit : '' }}
        </div>
      </div>
    </div>
    <span class="fold_button" @click="clickFoldBtn" :title="isShowMore ? '收起' : '展开'">
      <i :class="isShowMore ? 'el-icon-caret-top' : 'el-icon-caret-bottom'"></i>
    </span>
  </div>
</template>

<script>
import debounce from 'lodash/debounce'
import { levelColor } from '@/util/constant.js'

export default {
  name: 'PolarCanvas',
  props: {
    towerCurrentComp: String,
    size: {
      // 画布宽高
      type: Number,
      default: 240
    },
    warningValue: {
      type: Number,
      default: 0
    },
    dataList: {
      type: Array,
      default: () => []
    },
    isShowRadar: {
      type: Boolean,
      default: true
    }
  },
  provide() {
    return {
      parent: this
    }
  },
  watch: {
    watch: {
      data: {
        // 数据变化自动重绘
        handler() {
          this.draw()
        },
        deep: true
      }
    }
  },
  computed: {},
  data() {
    return {
      isShowMore: true,
      //
      ctx: null, // 画布上下文
      rot: -0.5 * Math.PI, // 当前旋转角（弧度）
      dragging: false, // 是否正在拖拽
      lastX: 0, // 上次鼠标位置
      rayLineColor: 'rgba(135, 135, 135, 0.3)', // 射线颜色
      areaColor: 'rgba(86, 227, 249, 0.3)', // 区域颜色
      circleColor: '#fff', // 雷达图最外层圆颜色
      warningLineColor: '#0081FF', // 报警线颜色
      pointColor: '#56E3F9', // 数据点颜色
      legendColor: '#fff', // legend字体颜色
      legendFont: '10px sans-serif', // legend字体样式
      legendGap: 1.15, // 半径放大系数
      legendRadius: 10, // 编号圆半径
      scaleNum: 0.7, // 半径放大系数，最外环在示意图内的比例
      tipShow: false,
      tipText: '',
      tipPos: { left: '0px', top: '0px' }
    }
  },
  created() {},
  mounted() {
    this.$refs[
      'circle_background'
    ].style.backgroundImage = `url(/img/valueBg/${this.towerCurrentComp}Bg.png`
    if (this.towerCurrentComp == 'flang') {
      this.scaleNum = 0.8
    }
    this.ctx = this.$refs.renderCanvas.getContext('2d')
    this.draw()
  },
  methods: {
    // 绘制
    draw() {
      const {
        ctx,
        size,
        dataList,
        rot,
        rayLineColor,
        areaColor,
        circleColor,
        warningLineColor,
        pointColor,
        legendColor,
        legendFont,
        legendGap,
        legendRadius,
        scaleNum
      } = this
      ctx.clearRect(0, 0, size, size)
      const cx = size / 2
      const cy = size / 2
      const data = dataList.filter(i => i.evID != null)
      const N = data.length
      if (N == 0) return
      const maxR = Math.max(...data.map(i => i.evValue), this.warningValue)
      const scale = (scaleNum * cx) / maxR
      // 5. 画文字
      ctx.font = legendFont
      ctx.fillStyle = legendColor
      ctx.textAlign = 'center'
      ctx.textBaseline = 'middle'

      const rText = maxR * scale * legendGap // 圆外半径
      dataList.forEach((r, i) => {
        const ang = (r.angleDegree * Math.PI) / 180 + rot
        const x = cx + rText * Math.cos(ang)
        const y = cy + rText * Math.sin(ang)
        // 画圆背景
        ctx.fillStyle = levelColor[r.evStatus] //legendBgColor
        ctx.beginPath()
        ctx.arc(x, y, legendRadius, 0, Math.PI * 2)
        ctx.fill()

        // 写字（居中）
        ctx.fillStyle = '#fff'
        ctx.fillText(`${r.circleName}`, x, y)
      })

      if (!this.isShowRadar) {
        // 不画雷达图，只画示意图上的编码
        ctx.restore()
        return
      }

      // 1. 网格：同心圆 + 射线
      // 报警线并且报警线是最大值，只画报警线
      this.drawCircles(ctx, cx, cy, maxR, scale, this.warningValue, warningLineColor, circleColor)

      // 射线
      ctx.strokeStyle = rayLineColor
      for (let i = 0; i < N; i++) {
        const ang = (data[i].angleDegree * Math.PI) / 180 + rot
        ctx.beginPath()
        ctx.moveTo(cx, cy)
        ctx.lineTo(cx + Math.cos(ang) * maxR * scale, cy + Math.sin(ang) * maxR * scale)
        ctx.stroke()
      }

      // 2. 计算点位
      const pts = data.map((r, i) => {
        const ang = (r.angleDegree * Math.PI) / 180 + rot
        return {
          x: cx + r.evValue * scale * Math.cos(ang),
          y: cy + r.evValue * scale * Math.sin(ang)
        }
      })

      // 3. 连线（闭合）
      ctx.strokeStyle = areaColor
      ctx.lineWidth = 2
      ctx.beginPath()
      ctx.moveTo(pts[0].x, pts[0].y)
      for (let i = 1; i < N; i++) ctx.lineTo(pts[i].x, pts[i].y)
      ctx.closePath()
      // 填充
      ctx.fillStyle = areaColor
      ctx.fill()
      // 描边
      ctx.stroke()

      // 4. 画点
      ctx.fillStyle = pointColor
      pts.forEach(p => {
        ctx.beginPath()
        ctx.arc(p.x, p.y, 2, 0, Math.PI * 2)
        ctx.fill()
      })

      ctx.restore()
    },
    // 画圆环
    drawCircles(ctx, cx, cy, maxR, scale, warningValue, warningLineColor, circleColor) {
      // 如果有报警值且等于最大半径，只绘制报警线
      // console.log(warningValue * scale)
      // console.log(maxR * scale)
      if (warningValue && warningValue === maxR) {
        this.drawCircle(ctx, cx, cy, maxR * scale, warningLineColor)
        return
      }

      // 绘制报警线（如果有）
      if (warningValue) {
        this.drawCircle(ctx, cx, cy, warningValue * scale, warningLineColor)
      }

      // 绘制同心圆
      this.drawCircle(ctx, cx, cy, maxR * scale, circleColor)
    },
    // 绘制圆环的工具函数
    drawCircle(ctx, x, y, radius, color, lineWidth = 1) {
      ctx.strokeStyle = color
      ctx.lineWidth = lineWidth
      ctx.beginPath()
      ctx.arc(x, y, radius, 0, Math.PI * 2)
      ctx.stroke()
    },
    /* ---- 拖动旋转 ---- */
    onDown(e) {
      this.dragging = true
      this.lastX = e.clientX
    },
    onMove: debounce(function (e) {
      if (this.dragging) {
        const dx = e.clientX - this.lastX
        this.rot += dx * 0.01
        this.lastX = e.clientX
        this.draw()
      }
      this.detectLegendHover(e)
    }, 100),
    onUp() {
      this.dragging = false
      this.$refs.renderCanvas.style.cursor = 'default' // 还原鼠标样式
      this.hideTip()
    },
    /* 检测是否悬停到 legend 圆 */
    detectLegendHover(e) {
      const rect = this.$refs.renderCanvas.getBoundingClientRect()
      const x = e.clientX - rect.left
      const y = e.clientY - rect.top
      const { dataList, size, rot, legendGap, legendRadius, scaleNum } = this
      const cx = size / 2
      const cy = size / 2
      const rText = scaleNum * cx * legendGap

      let hit = false
      for (let i = 0; i < dataList.length; i++) {
        const ang = (dataList[i].angleDegree * Math.PI) / 180 + rot
        const lx = cx + rText * Math.cos(ang)
        const ly = cy + rText * Math.sin(ang)
        const dist = Math.hypot(x - lx, y - ly)
        if (dist <= legendRadius) {
          // 圆内命中
          // 关键：设置指针样式
          this.$refs.renderCanvas.style.cursor = 'pointer'
          this.showTip(e.offsetX, e.offsetY, dataList[i].evValue)
          hit = true
          break
        } else {
          this.$refs.renderCanvas.style.cursor = 'default'
        }
      }
      if (!hit) this.hideTip()
    },

    /* 显示提示 */
    showTip(offsetX, offsetY, text) {
      this.tipText = text
      this.tipPos = {
        left: offsetX < this.size / 2 ? `${offsetX + 12}px` : `${offsetX - 70}px`,
        top: offsetY < this.size / 2 ? `${offsetY + 12}px` : `${offsetY - 40}px`
      }
      this.tipShow = true
    },

    /* 隐藏提示 */
    hideTip() {
      this.tipShow = false
    },
    // 点击按钮展示详细特征值
    clickFoldBtn() {
      this.isShowMore = !this.isShowMore
    }
  }
}
</script>

<style lang="less" scoped>
.fold_button {
  font-size: 12px;
  position: absolute;
  left: 50%;
  bottom: -5px;
  color: #fff;
  cursor: pointer;
  &:hover {
    color: #2e4de7;
  }
}
.canvas-tip {
  position: absolute;
  width: 80px;
  padding: 5px 5px;
  text-align: center;
  height: auto;
  color: #fff;
  border-radius: 5px;
  background: rgba(77, 88, 105, 0.9);
  z-index: 99;
  font-size: 12px;
  span {
    font-size: 14px;
    font-weight: bolder;
    margin: 0 5px;
  }
}
.circle_wrapper {
  position: relative;
  width: 100%;
  height: 100%;
  text-align: center;
  .circle_background {
    display: inline-block;
    background-repeat: no-repeat;
    background-size: 100% 100%;
  }
  .circle_canvas {
    display: inline-block;
    position: absolute;
    top: 0px; // -285px;
    z-index: 10;
    overflow: hidden;
    /* cursor: default;
    &:hover {
      cursor: pointer;
    } */
  }
}

.border_color {
  border-color: rgb(11, 255, 231) !important;
}
</style>
