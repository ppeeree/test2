<template>
  <div class="hotmap" style="width: 100%; min-width: 380px; height: 35px">
    <chart :chartData="chartOption" width="380px" height="100%" />
  </div>
</template>

<script>
import chart from '@/components/charts/baseChart'
export default {
  components: {
    chart
  },
  props: ['chartData'],
  data() {
    return {
      mapData: [],
      xTime: [],
      chartOption: {}
      // yNum:[]
    }
  },
  updated() {},
  mounted() {
    this.hotMapInit()
  },
  methods: {
    //数据处理
    dataInformation() {
      this.mapData = []
      this.Xtime = []
      let arr = []
      this.chartData.faultLevel.forEach((item, index) => {
        if (item == 'severe') {
          arr.push([index, 0, 3])
        } else if (item == 'moderate') {
          arr.push([index, 0, 2])
        } else if (item == 'mild') {
          arr.push([index, 0, 1])
        } else {
          arr.push([index, 0, 0])
        }
      })
      this.mapData = arr
      this.Xtime = this.chartData.faultTime
    },
    hotMapInit() {
      this.dataInformation()
      this.chartOption = {
        tooltip: {
          textStyle: {
            fontSize: 10
          },
          formatter: function (item) {
            let hotmaptest = ''
            let status, time, color
            if (item.data[2] == 3) {
              status = '严重'
              color = '#FF0000'
            } else if (item.data[2] == 2) {
              color = '#FF5D5D'
              status = '中等'
            } else if (item.data[2] == 1) {
              color = '#FF9B9B'
              status = '轻微'
            } else if (item.data[2] == 0) {
              color = '#2ed133'
              status = '无故障'
            }
            // time = item.name.split(' ')[0]
            time = item.name
            hotmaptest += time + '<br/>'
            hotmaptest += '<div>'
            hotmaptest +=
              '<span style="margin-right:5px;display:inline-block;width:10px;height:10px;border-radius:5px;background-color:' +
              color +
              ';"></span>'
            hotmaptest += status
            hotmaptest += '</div>'

            return hotmaptest
          },
          confine: true
        },
        grid: {
          height: '70%',
          top: '20%',
          left: '1%',
          right: 10
        },
        xAxis: {
          type: 'category',
          data: this.Xtime,
          label: {
            show: false
          },
          splitArea: {
            show: true
          }
        },
        yAxis: {
          type: 'category',
          splitArea: {
            show: true
          }
        },
        //索引
        visualMap: {
          min: 0,
          max: 5,
          calculable: true,
          orient: 'horizontal', //水平放置
          type: 'piecewise',
          itemWidth: 10,
          itemHeight: 10,
          itemSymbol: 'circle',
          top: '1%',
          left: '50%',
          pieces: [
            {
              gt: 3,
              label: '严重',
              color: '#FF0000'
            },
            {
              gt: 2,
              lt: 3,
              label: '中等',
              color: '#FF5D5D'
            },
            {
              gt: 1,
              lt: 2,
              label: '轻微',
              color: '#FF9B9B'
            },
            {
              gt: 0,
              lt: 1,
              label: '无故障',
              color: '#2ed133'
            }
          ],
          textStyle: {
            color: '#D6DADE'
          },
          show: false
        },
        series: [
          {
            name: '健康状态',
            type: 'heatmap',
            data: this.mapData,
            pointSize: 4,
            label: {
              show: false
            },
            itemStyle: {
              borderWidth: 0,
              borderColor: '#DEECE2'
            },
            emphasis: {
              itemStyle: {
                shadowBlur: 5,
                shadowColor: 'rgba(0, 0, 0, 0.5)'
              }
            }
          }
        ]
      }
    }
  }
}
</script>

<style lang="less" scoped></style>
