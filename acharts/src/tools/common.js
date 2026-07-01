import { accDiv } from './tool.js'
import { closest } from '../tools/YBLine/ybCommonTools.js'

// 数据稀释
/**
 * 
 * @param {*} echart 实例
 * @param {*} xInterval X轴间隔
 * @param {*} dataZoom 放大状态数据{}
 * @param {*} data 原始数据
 * @param {*} isValue 放大的是比例还是数值
 * @returns 
 */
export function computedShowDataPoint(echart, xInterval, dataZoom, data, isValue) {
  let chartWidth = echart.getWidth()
  let indexArr = []
  if (isValue) {
    indexArr[0] = closest(data, dataZoom[0]).index
    indexArr[1] = closest(data, dataZoom[1]).index
  } else {
    let long = data.length
    indexArr = [long * dataZoom[0] / 100, long * dataZoom[1] / 100]
  }
  let diff = indexArr[1] - indexArr[0]
  // 稀释
  let pointerList = []
  if (diff > chartWidth * 2) {
    let batchSize = Math.floor(diff / chartWidth) // 每个样本内的数据点长度
    for (let i = 0; i < chartWidth; i++) { //样本个数
      let frame = i == chartWidth - 1 ? data.slice(indexArr[0] + batchSize * i, indexArr[1]) : data.slice(indexArr[0] + batchSize * i, indexArr[0] + batchSize * (i + 1))
      let length = i == chartWidth - 1 ? indexArr[1] - indexArr[0] - batchSize * i : batchSize
      let min = [Infinity, Infinity], max = [-Infinity, -Infinity]
      if (!length > 0) {
        continue;
      }
      while (length--) {
        max = frame[length][1] > max[1] ? frame[length] : max;
        min = frame[length][1] < min[1] ? frame[length] : min;
      }
      //（小大）大小，大大之间取最小，多绘制一个点
      if (min[0] > max[0]) {
        //判断上一组样本大小顺序
        let lastTwoPointer = pointerList.slice(-2)
        if (lastTwoPointer.length && lastTwoPointer[0][1] < lastTwoPointer[1][1]) {
          let indexLastMax = accDiv(lastTwoPointer[1][0], xInterval)//data.findIndex(i => i == lastTwoPointer[1])
          let indexCurrentMax = accDiv(max[0], xInterval) //data.findIndex(i => i == max)
          let length = indexCurrentMax - indexLastMax - 1
          if (length) {
            let maxGapFrame = data.slice(indexLastMax + 1, indexCurrentMax)
            let minPointer = [Infinity, Infinity]
            while (length--) {
              minPointer = maxGapFrame[length][1] < minPointer[1] ? maxGapFrame[length] : minPointer;
            }
            pointerList.push(minPointer)
          }
        }
        pointerList.push(max)
        pointerList.push(min)
      } else {
        // （大小）小大，小小之间取最大，多绘制一个点
        //判断上一组样本大小顺序
        let lastTwoPointer = pointerList.slice(-2)
        if (lastTwoPointer.length && lastTwoPointer[0][1] > lastTwoPointer[1][1]) {
          let indexLastMin = accDiv(lastTwoPointer[1][0], xInterval)//data.findIndex(i => i == lastTwoPointer[1])
          let indexCurrentMin = accDiv(min[0], xInterval) //data.findIndex(i => i == min)
          let length = indexCurrentMin - indexLastMin - 1
          if (length) {
            let minGapFrame = data.slice(indexLastMin + 1, indexCurrentMin)
            let maxPointer = [Infinity, Infinity]
            while (length--) {
              maxPointer = minGapFrame[length][1] < maxPointer[1] ? minGapFrame[length] : maxPointer;
            }
            pointerList.push(maxPointer)
          }

        }
        pointerList.push(min)
        pointerList.push(max)
      }
    }
  } else {
    // 不稀释
    pointerList = data.slice(indexArr[0], indexArr[1])
  }
  return pointerList
}