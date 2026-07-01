import { TimeDomainChart } from './src/compontents/timeDomain/index.js'
import { EvTrendLine } from './src/compontents/trendLineChart/index.js'
import { ScatterChart } from './src/compontents/scatterChart/index.js'
import { HistogramChart } from './src/compontents/histogramChart/index.js'
import { DipAngleChart } from './src/compontents/overturnChart/index.js'
// import { WaterFallChart } from './src/compontents/waterfall/index.js'
import { SpectrumChart } from './src/compontents/spectrum/index.js'
import { WaveFormIndexChart } from './src/compontents/waveformIndex/index.js'
import { RPMWave } from './src/compontents/RPMWave/index.js'
import { AttachResultLineChart } from './src/compontents/attachResultLineChart/index.js'
import { SimpleLineChart } from './src/compontents/simpleLineChart/index.js'
import { TrackChart } from './src/compontents/trackChart/index.js'
import { PolarChart } from './src/compontents/polarChart/index.js'
import './src/themStyle/index.css'
import './src/assets/fonts/iconfont.css'

export {
  TimeDomainChart, // 时域
  EvTrendLine, // 特征值趋势
  ScatterChart, // 相关性分析-散点图
  HistogramChart, // 分布--柱状区间统计图
  DipAngleChart, // 倾覆分析
  // WaterFallChart, // 瀑布图
  SpectrumChart, // 频谱图
  WaveFormIndexChart, // 波形索引图
  RPMWave, // 转速波形
  AttachResultLineChart,
  SimpleLineChart, //
  TrackChart,
  PolarChart
}

