<template>
  <div class="hotmap" style="min-width: 320px; height: 65px; margin-top: -10%">
    <chart :chartData="chartOption" width="320px" height="100%" />
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
      chartOption: {},
      maxFrequency: ''
    }
  },
  watch:{
    chartData:{
      handler(val){
        this.dataInformation(val)
        this.hotMapInit()
      }
    }
  },
  mounted() {
    this.dataInformation(this.chartData)
    this.hotMapInit()
  },
  methods: {
    //数据处理
    dataInformation(list) {
      this.mapData = []
      this.Xtime = []
      let arr = []
      let timeArr = []
      let maxArr = []
      list.forEach((item, index) => {
        arr.push([index, 0, item.frequency])
        maxArr.push(item.frequency)
        timeArr.push(item.time)
      })
      this.mapData = arr
      this.Xtime = timeArr
      this.maxFrequency = this.getMaxFrequency(maxArr)
    },
    getMaxFrequency(list) {
      let data = Math.max.apply(null, list)
      return data
    },
    hotMapInit() {
      this.chartOption = {
        tooltip: {
          textStyle: {
            fontSize: 10
          },
          formatter: function (item) {
            let hotmaptest = ''
            let status, time, color
            status = item.data[2]
            color = item.color
            // if (item.data[2] >= 3) {
            //   status = '3次及以上'
            //   color = 'rgba(0, 93, 255, 1)'
            // } else if (item.data[2] == 2) {
            //   color = 'rgba(76, 141, 255, 1)'
            //   status = '2次'
            // } else if (item.data[2] == 1) {
            //   color = 'rgba(162, 196, 255, 1)'
            //   status = '1次'
            // } else if (item.data[2] == 0) {
            //   color = 'rgba(0, 0, 0, 0.5)'
            //   status = '未发生'
            // }
            time = item.name
            hotmaptest += time + '<br/>'
            hotmaptest += '<div>'
            hotmaptest +=
              '<span style="margin-right:5px;display:inline-block;width:10px;height:10px;border-radius:5px;background-color:' +
              color +
              ';"></span><span style="margin-left:3px;margin-right:3px">发生</span>'
            hotmaptest += status
            hotmaptest += '</span><span style="margin-left:5px">次</span></div>'

            return hotmaptest
          },
          confine: true
        },
        grid: {
          height: '100%',
          top: '65%',
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
          max: this.maxFrequency,
          calculable: true,
          orient: 'horizontal', //水平放置
          itemWidth: 11,
          itemHeight:120,
          left: '50%',
          top: '10%',
          // text: ['高', '低'],
          inRange: {
            color: ['#3546FE', '#5A28A5', '#81218C', '#FF1A6D']
          },
          textStyle: {
            color: '#D6DADE'
          },
          show: false
        },
        // //索引
        // visualMap: {
        //   min: 0,
        //   max: 5,
        //   calculable: true,
        //   orient: 'horizontal', //水平放置
        //   type: 'piecewise',
        //   itemWidth: 10,
        //   itemHeight: 10,
        //   itemSymbol: 'circle',
        //   top: '1%',
        //   left: '50%',
        //   pieces: [
        //     {
        //       gte: 3,
        //       label: '3次及以上',
        //       color: 'rgba(0, 93, 255, 1)'
        //     },
        //     {
        //       gt: 2,
        //       lt: 3,
        //       label: '2次',
        //       color: 'rgba(76, 141, 255, 1)'
        //     },
        //     {
        //       gt: 1,
        //       lt: 2,
        //       label: '1次',
        //       color: 'rgba(162, 196, 255, 1)'
        //     },
        //     {
        //       gt: 0,
        //       lt: 1,
        //       label: '未发生',
        //       color: 'rgba(0, 0, 0, 0.5)'
        //     }
        //   ],
        //   textStyle: {
        //     color: '#D6DADE'
        //   },
        //   show: true
        // },
        series: [
          {
            name: '发生频次',
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
