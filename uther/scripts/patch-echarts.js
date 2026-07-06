const fs = require('fs')
const path = require('path')

const patches = [
  {
    file: path.resolve(__dirname, '../node_modules/echarts/lib/layout/barPolar.js'),
    from: "if (seriesModel.coordinateSystem.type !== 'polar') {",
    to: "if (!seriesModel.coordinateSystem || seriesModel.coordinateSystem.type !== 'polar') {"
  },
  {
    file: path.resolve(__dirname, '../node_modules/echarts/lib/chart/bar/BarView.js'),
    from: [
      '    var coord = seriesModel.coordinateSystem;',
      '    var baseAxis = coord.getBaseAxis();'
    ].join('\n'),
    to: [
      '    var coord = seriesModel.coordinateSystem;',
      '    if (!coord) {',
      '      group.removeAll();',
      '      this._data = data;',
      '      return;',
      '    }',
      '    var baseAxis = coord.getBaseAxis();'
    ].join('\n')
  },
  {
    file: path.resolve(__dirname, '../node_modules/echarts/lib/processor/dataSample.js'),
    from: [
      "if (count > 10 && coordSys.type === 'cartesian2d' && sampling) {",
      "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling) {"
    ],
    to: "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling && coordSys.getBaseAxis && coordSys.getOtherAxis) {"
  },
  {
    file: path.resolve(__dirname, '../node_modules/echarts/dist/echarts.simple.js'),
    from: [
      "if (count > 10 && coordSys.type === 'cartesian2d' && sampling) {",
      "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling) {"
    ],
    to: "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling && coordSys.getBaseAxis && coordSys.getOtherAxis) {"
  },
  {
    file: path.resolve(__dirname, '../node_modules/echarts/dist/echarts.common.js'),
    from: [
      "if (count > 10 && coordSys.type === 'cartesian2d' && sampling) {",
      "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling) {"
    ],
    to: "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling && coordSys.getBaseAxis && coordSys.getOtherAxis) {"
  },
  {
    file: path.resolve(__dirname, '../node_modules/echarts/dist/echarts.js'),
    from: [
      "if (count > 10 && coordSys.type === 'cartesian2d' && sampling) {",
      "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling) {"
    ],
    to: "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling && coordSys.getBaseAxis && coordSys.getOtherAxis) {"
  },
  {
    file: path.resolve(__dirname, '../node_modules/echarts/dist/echarts.esm.js'),
    from: [
      "if (count > 10 && coordSys.type === 'cartesian2d' && sampling) {",
      "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling) {"
    ],
    to: "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling && coordSys.getBaseAxis && coordSys.getOtherAxis) {"
  },
  {
    file: path.resolve(__dirname, '../node_modules/echarts/dist/echarts.esm.mjs'),
    from: [
      "if (count > 10 && coordSys.type === 'cartesian2d' && sampling) {",
      "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling) {"
    ],
    to: "if (count > 10 && coordSys && coordSys.type === 'cartesian2d' && sampling && coordSys.getBaseAxis && coordSys.getOtherAxis) {"
  }
]

patches.forEach(({ file, from, to }) => {
  if (!fs.existsSync(file)) {
    return
  }
  const source = fs.readFileSync(file, 'utf8')
  if (source.includes(to)) {
    return
  }
  const fromList = Array.isArray(from) ? from : [from]
  const matchedFrom = fromList.find(item => source.includes(item))
  if (!matchedFrom) {
    console.warn(`[patch-echarts] Skip unmatched file: ${file}`)
    return
  }
  fs.writeFileSync(file, source.replace(matchedFrom, to))
  console.log(`[patch-echarts] Patched ${path.relative(process.cwd(), file)}`)
})
