// 屏幕缩放
export function screenSize(editorDom, defWidth = 1920, defHeight = 1100) {
  let screenWidth = document.body.clientWidth || document.documentElement.clientWidth
  let screenHeight = document.body.clientHeight || document.documentElement.clientHeight
  let xScale = screenWidth / defWidth
  let yScale = screenHeight / defHeight
  editorDom.style.cssText += ';transform: scale(' + xScale + ',' + yScale + ')'

  window.addEventListener('resize', () => {
    let screenWidth = document.body.clientWidth || document.documentElement.clientWidth
    let screenHeight = document.body.clientHeight || document.documentElement.clientHeight
    xScale = screenWidth / defWidth
    yScale = screenHeight / defHeight
    editorDom.style.cssText += ';transform: scale(' + xScale + ',' + yScale + ')'
  })
}

export function dealTrendData(data) {
  const { evdataList } = data
  let lineDatas = []
  if (evdataList && evdataList.length) {
    evdataList.forEach((element, index) => {
      const {
        windturbineName,
        windParkName,
        sampleRate,
        evName,
        measlocName,
        vdiMax,
        vdiMin,
        evdata,
        evId,
        unitX,
        unitY
      } = element
      let name = `${evName}_${sampleRate < 0 ? '' : sampleRate + 'Hz_'}${measlocName}_${windturbineName}_${windParkName}`
      lineDatas.push({
        source: evdata,
        name: name,
        sourceName: name,
        VDI: vdiMin === null ? [] : [vdiMin, vdiMax],
        id: evId + (sampleRate < 0 ? '' : ('~' + sampleRate)),
        dimensions: [unitX, unitY, '转速(rpm)', '是否存在波形'],
        other: {
          ...element,
          evdata: []
        }
      })
    })
    return lineDatas
  } else {
    return []
  }
}