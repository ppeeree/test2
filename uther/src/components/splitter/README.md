### 介绍

Splitter 是一个可交互的分割面板组件，用于将页面区域划分为多个可调整大小的子面板。用户可以通过拖动分割条来动态改变相邻面板的宽度或高度，适用于需要灵活布局的场景，如代码编辑器、文件管理器或可定制的仪表盘。

### 关键特性

- 拖拽调整：支持通过拖拽分割条动态调整面板尺寸。
- 方向支持：支持水平（horizontal）和垂直（vertical）两种分割方向。
- 嵌套使用：支持多级嵌套，实现复杂的网格布局。
- 面板配置：支持设置面板的最小/最大尺寸及初始比例。
- 折叠功能：可配置面板的折叠与展开交互。
- 自定义样式：支持通过 CSS 自定义分割条和面板的样式。

### 使用

```vue
<template>
<div style="display: flex; height: 100vh;">
<div style="flex:0 0 200px"></div>
  <Splitter
  :splitType="direction"
  :id="splitterId"
  :panels="panels"
  :limit="limit"
  style="flex: 0 0 4px" />
<div style="flex:1 1 0"></div>
</div>
</template>
<script>
import Splitter from '@zhuowenli/vue-splitter';

export default {
  components: {
    Splitter,
  },
  data() {
    return {
      direction: 'horizontal', // 分割方向，可选 'horizontal' 或 'vertical'
      limit: { min: 50, max: 1000 }, // 面板1的最小和最大尺寸限制(像素)
      splitterId: 'areaSplit', // 分割条的ID

<style>
两个模块，一个分割线：一个固定宽度，另一个flex:1,大小弹性，自动占满剩余空间
三个模块，两个分割线：两个固定宽度，一个flex:1,大小弹性，自动占满剩余空间，需要平分宽度，那三个模块需要设置flex:1 1 0;
</style>
```
