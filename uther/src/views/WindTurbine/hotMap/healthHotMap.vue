<template>
  <div
    class="hotmap"
    ref="hotmap"
    id="hotMap"
    :style="{ width: widthSize || '100%', height: isShowX ? '55px' : '34px', marginBottom: '4px' }"
  ></div>
</template>

<script>
let echarts = require('echarts')

export default {
  props: ['hotMapdata', 'isShowX', 'xTime', 'widthSize'],
  data() {
    return {
      id: `hotMap_${this.unique}`,
      option: {},
      mapData: [],
      hotmap: null
      // Xtime: []
    }
  },
  mounted() {
    this.hotMapInit()
    window.addEventListener('resize', () => {
      this.hotmap.resize()
    })
  },
  methods: {
    //数组处理
    dataTranform() {
      // console.log('获取到的数据', this.hotMapdata)
      this.hotMapdata.status.forEach((item, index) => {
        if (item == 'danger' || item == '4') {
          this.mapData.push([index, 0, 4])
        } else if (item == 'warning' || item == '3') {
          this.mapData.push([index, 0, 3])
        } else if (item == 'attention' || item == '2') {
          this.mapData.push([index, 0, 2])
        } else if (item == 'normal') {
          this.mapData.push([index, 0, 1])
        } else {
          this.mapData.push([index, 0, 5])
        }
      })
      // console.log(this.mapData)
      //  this.Xtime = Array.from(this.hotMapdata.time,(item)=>item.)
    },
    hotMapInit() {
      if (this.hotmap) {
        this.hotmap.dispose()
        this.hotmap = null
      }
      this.dataTranform()
      let Ynone = []
      this.hotmap = echarts.init(this.$refs['hotmap'])
      this.hotmap.setOption({
        legend: {
          orient: 'horizontal', //水平放置
          itemGap: '10',
          itemWidth: 8,
          itemHeight: 10,
          itemSymbol: 'circle',
          top: '5%',
          left: '50%',
          pieces: [
            {
              gt: 5,
              label: '无状态',
              color: '#4E6278'
            },
            {
              gt: 4,
              lt: 5,
              label: '危险',
              color: '#FF0F0D'
            },
            {
              gt: 3,
              lt: 4,
              label: '警告',
              color: '#FF6B0E'
            },
            {
              gt: 2,
              lt: 3,
              label: '注意',
              color: '#FFE604'
            },
            {
              gte: 1,
              lt: 2,
              label: '正常',
              color: '#2ED133'
            }
          ]
        },
        tooltip: {
          confine: true,
          formatter: function (item) {
            // console.log(item)
            let hotmaptest = ''
            let status, time, color
            if (item.data[2] == 4) {
              status = '危险'
              color = '#FF0F0D'
            } else if (item.data[2] == 3) {
              color = '#FF6B0E'
              status = '警告'
            } else if (item.data[2] == 2) {
              color = '#FFE604'
              status = '注意'
            } else if (item.data[2] == 1) {
              color = '#2ED133'
              status = '正常'
            } else if (item.data[2] == 5) {
              color = '#4E6278'
              status = '无状态'
            }
            time = item.name.split(' ')[0]
            hotmaptest += time + '<br/>'
            hotmaptest += '<div>'
            hotmaptest +=
              '<span style="margin-right:5px;display:inline-block;width:10px;height:10px;border-radius:5px;background-color:' +
              color +
              ';"></span>'
            hotmaptest += status
            hotmaptest += '</div>'

            return hotmaptest
          }
        },
        grid: {
          height: this.isShowX ? '40%' : '70%',
          top: '20%',
          left: '4%'
        },
        xAxis: {
          type: 'category',
          data: this.xTime,
          splitArea: {
            show: true
          },
          show: this.isShowX,
          axisTick: {
            show: false
          },
          maxInterval: 5,
          axisLabel: {
            color: '#fff',
            margin: 10,
            padding: [3, 4, 5, 25]
          }
          /*  boundaryGap: ['30%', '20%'] */
        },
        yAxis: {
          type: 'category',
          data: Ynone,
          splitArea: {
            show: true
          },
          formatter: (params, index) => {
            return [`{a${index + 1}|} ${params}`].join('\n')
          },
          rich: {
            a1: {
              backgroundColor: { image: '/img/WindTurbine/bladeIcon.png' },
              width: 18,
              height: 18
            },
            a2: {
              backgroundColor: { image: '/img/WindTurbine/mainbearIcon.png' },
              width: 18,
              height: 18
            },
            a3: {
              backgroundColor: { image: '/img/WindTurbine/gearboxIcon.png' },
              width: 18,
              height: 18
            },
            a4: {
              backgroundColor: { image: '/img/WindTurbine/engineIcon.png' },
              width: 18,
              height: 18
            },
            a5: {
              backgroundColor: { image: '/img/WindTurbine/towerIcon.png' },
              width: 18,
              height: 18
            }
          }
        },
        //索引
        visualMap: {
          min: 0,
          max: 6,
          calculable: true,
          orient: 'horizontal', //水平放置
          type: 'piecewise',
          itemWidth: 10,
          itemHeight: 10,
          itemSymbol: 'circle',
          top: '5%',
          left: '50%',
          pieces: [
            {
              gt: 5,
              label: '无状态',
              color: '#4E6278'
            },
            {
              gt: 4,
              lt: 5,
              label: '危险',
              color: '#FF0F0D'
            },
            {
              gt: 3,
              lt: 4,
              label: '警告',
              color: '#FF6B0E'
            },
            {
              gt: 2,
              lt: 3,
              label: '注意',
              color: '#FFE604'
            },
            {
              gte: 1,
              lt: 2,
              label: '正常',
              color: '#2ED133'
            }
          ],
          textStyle: {
            color: '#D6DADE'
          },
          show: false
        },
        series: [
          {
            type: 'heatmap',
            data: this.mapData,
            pointSize: 4,
            label: {
              show: false
            },
            itemStyle: {
              /*   borderWidth: 0.5,
              borderColor: '#DEECE2' */
            },
            emphasis: {
              itemStyle: {
                shadowBlur: 5,
                shadowColor: 'rgba(0, 0, 0, 0.5)'
              }
            }
          }
        ]
      })
    }
  }
}
</script>

<style lang="less" scoped></style>
