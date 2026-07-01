export const getDOMLayout = () => {
  let { clientWidth, clientHeight } = document.body
  let leftPartWidthRate = document.getElementById('leftArea').clientWidth / clientWidth.toFixed(10) * 100
  let rightPartWidth = document.getElementById('rightArea').clientWidth
  let rightPartHeight = document.getElementById('rightArea').clientHeight
  let rightPartWidthRate = (rightPartWidth / clientWidth).toFixed(10) * 100
  let rowsContent = []
  let rowDoms = document.getElementById('rightArea').querySelectorAll('.rowbox')
  if (rowDoms.length) {
    for (let m = 0; m < rowDoms.length; m++) {
      let rowUnit = {
        rowHeight: 0,
        charts: []
      }
      rowUnit.rowHeight = (rowDoms[m].clientHeight / rightPartHeight).toFixed(10) * 100
      let colDoms = rowDoms[m].querySelectorAll('.colbox')
      if (colDoms.length) {
        for (let i = 0; i < colDoms.length; i++) {
          rowUnit.charts.push({
            chartType: colDoms[i].getAttribute('data-key'),
            chartWidth: (colDoms[i].clientWidth / rightPartWidth).toFixed(10) * 100
          })
        }
      }
      rowsContent.push({ ...rowUnit })
    }
  }
  return [
    { key: "leftPart", partWidth: leftPartWidthRate },
    {
      key: "rightPart",
      partWidth: rightPartWidthRate,
      rows: rowsContent
    }
  ]
}


// 只获取辅助图布局
export const getDOMDownLayout = () => {
  let rightPartWidth = document.getElementById('rightArea').clientWidth
  let rightPartHeight = document.getElementById('rightArea').clientHeight
  let rowsContent = []
  let rowDoms = document.getElementById('rightArea').querySelectorAll('.rowbox')
  if (rowDoms.length) {
    for (let m = 0; m < rowDoms.length; m++) {
      let rowUnit = {
        rowHeight: 0,
        charts: []
      }
      rowUnit.rowHeight = (rowDoms[m].clientHeight / rightPartHeight).toFixed(10) * 100
      let colDoms = rowDoms[m].querySelectorAll('.colbox')
      if (colDoms.length) {
        for (let i = 0; i < colDoms.length; i++) {
          rowUnit.charts.push({
            chartType: colDoms[i].getAttribute('data-key'),
            chartWidth: (colDoms[i].clientWidth / rightPartWidth).toFixed(10) * 100
          })
        }
      }
      rowsContent.push({ ...rowUnit })
    }
  }
  return rowsContent
}