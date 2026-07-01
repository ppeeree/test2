<template>
  <div class="image-annotation-container">
    <div class="toolbar">
      <button class="tool-btn" data-tool="select" title="点击元素，可删除操作">选择</button>
      <button class="tool-btn" data-tool="rectangle" title="点选位置，拖拽画图">矩形</button>
      <!--   <button class="tool-btn" data-tool="circle">圆形</button> -->
      <button class="tool-btn" data-tool="line" title="点选位置，拖拽画图">直线</button>
      <button class="tool-btn" data-tool="arrow" title="点选位置，拖拽画图">箭头</button>
      <button class="tool-btn" data-tool="text" title="输完点击‘enter’，取消输入点击‘esc’">
        文字
      </button>
      <!-- <button class="tool-btn" data-tool="freehand">自由绘制</button> -->

      <div class="style-controls">
        <div class="style-control">
          <label for="color-picker">颜色:</label>
          <input type="color" id="color-picker" value="#000" />
        </div>
        <div class="style-control">
          <label for="line-width">线宽:</label>
          <input type="range" id="line-width" min="1" max="10" value="1" />
        </div>
        <div class="style-control" style="display: none">
          <label for="line-dash">虚线:</label>
          <select id="line-dash">
            <option value="solid">实线</option>
            <option value="dashed">虚线</option>
            <option value="dotted">点线</option>
          </select>
        </div>
        <div class="style-control" style="display: none">
          <label for="fill-style">填充:</label>
          <select id="fill-style">
            <option value="none">无</option>
            <option value="solid">实心</option>
            <option value="transparent">半透明</option>
          </select>
        </div>
      </div>

      <button class="tool-btn" data-action="undo">撤销</button>
      <!-- <button class="tool-btn" data-action="redo">重做</button> -->
      <button class="tool-btn" data-action="delete">删除</button>
      <!--   <button class="tool-btn" data-action="save">保存</button> -->

      <!--  <select class="language-selector" id="language-selector">
        <option value="zh">中文</option>
        <option value="en">English</option>
      </select> -->
    </div>

    <div class="image-wrapper" style="width: 100%; height: auto">
      <img id="defect-image" :src="imgUrl" style="width: 100%" alt="设备缺陷图片" />
      <canvas style="width: 100%; height: 100%" id="annotation-canvas"></canvas>
      <div id="rotation-handle" class="control-handle" style="display: none">↻</div>
      <div class="text-input-container">
        <input type="text" id="text-input" placeholder="输入文字..." />
      </div>
    </div>
  </div>
</template>
<script>
import ImageAnnotator from './compClass.js'
export default {
  name: 'ImageAnnotation',
  props: {
    imgUrl: {
      type: String,
      default: ''
    }
  },
  data() {
    return {
      imgInstance: null
    }
  },
  mounted() {
    // 如果图片未加载，可以添加加载事件
    const img = document.getElementById('defect-image')
    if (!img.complete) {
      let that = this
      img.onload = function () {
        that.imgInstance = new ImageAnnotator('defect-image', 'annotation-canvas')
      }
    } else {
      // 初始化标注工具
      this.imgInstance = new ImageAnnotator('defect-image', 'annotation-canvas')
    }
  },

  unmounted() {
    if (this.imgInstance) {
      this.imgInstance = null
    }
  },
  methods: {
    saveImage() {
      return this.imgInstance.saveImage()
    }
  }
}
</script>
<style scoped>
.image-annotation-container {
  position: relative;
  max-width: 900px;
  margin: 0 auto;
  font-family: Arial, sans-serif;
}

.toolbar {
  background: #f0f0f0;
  padding: 6px;
  border-radius: 5px;
  display: flex;
  gap: 5px;
  flex-wrap: wrap;
  font-size: 12px;
}

.tool-btn {
  padding: 5px 10px;
  cursor: pointer;
  border: 1px solid #ccc;
  background: white;
  border-radius: 3px;
}

.tool-btn.active {
  background: #4caf50;
  color: white;
  border-color: #4caf50;
}

.style-controls {
  display: flex;
  gap: 5px;
  align-items: center;
  margin-left: auto;
}

.style-control {
  display: flex;
  align-items: center;
}

.style-control label {
  margin-right: 5px;
  font-size: 14px;
}

.image-wrapper {
  position: relative;
  touch-action: none;
}

#annotation-canvas {
  position: absolute;
  top: 0;
  left: 0;
  pointer-events: auto;
}

#annotation-canvas.drawing {
  pointer-events: auto;
}

.text-input-container {
  position: absolute;
  display: none;
  z-index: 100;
}

#text-input {
  padding: 5px;
  border: 2px solid #4caf50;
  border-radius: 3px;
  font-size: 14px;
}

.language-selector {
  margin-left: 10px;
}

@media (max-width: 768px) {
  .toolbar {
    flex-direction: column;
  }

  .style-controls {
    margin-left: 0;
    flex-wrap: wrap;
  }
}
</style>
