export const option = {
  grid: {
    top: 40,
    left: 60,
    bottom: 50,
    right: 80
  },
  tooltip: {
    trigger: 'axis'
  },
  legend: {
    show: true,
    /*   top: 20,
      left: 100, */
    itemHeight: 7,
    itemWidth: 10,
    textStyle: {
      color: '#fff',
      fontSize: 12,
    },
  },
  dataZoom: [
    {
      type: 'inside',
      id: 'Xinside',
      xAxisIndex: [0],
    },
    {
      id: 'Xslider',
      type: 'slider',
      xAxisIndex: [0],
      height: 25,
      bottom: 0,
      orient: "horizontal",
    },
  ],
  title: {
    textStyle: {
      show: true,
      left: 5,
      top: 0,
      color: '#fff',
      fontSize: 16,
      fontSize: 14,
      fontWeight: 700
    }
  },
  xAxis: {
    type: 'time',
    nameTextStyle: {
      fontSize: 12,
      color: '#fff',
      width: 20,
      height: 40,
      padding: [5, 0, 5, 0],
      overflow: 'break',
      verticalAlign: 'middle'
    },
    splitLine: {
      show: true,
      lineStyle: {
        color: 'rgba(68, 68, 68, 0.49)',
        width: 1
      }
    },
    axisTick: {
      //去掉刻度
      show: false,
      // inside: true,
      lineStyle: {
        color: 'rgba(68, 68, 68, 0.49)',
        width: 2 //这里是坐标轴的宽度
      }
    },
    axisLabel: {
      /* interval: 0, */
      hideOverlap: true,
      interval: 2,
      textStyle: {
        color: '#fff',
        fontSize: 12
      },
    },
    // y轴的颜色和宽度
    axisLine: {
      // onZero: false,
      lineStyle: {
        color: 'rgba(68, 68, 68, 0.49)',
        width: 1 //这里是坐标轴的宽度
      }
    }
  },
  yAxis: {
    type: 'value',
    nameTextStyle: {
      fontSize: 12,
      color: '#fff',
    },
    splitLine: {
      show: true,
      lineStyle: {
        color: 'rgba(68, 68, 68, 0.49)',
        width: 1
      }
    },
    axisTick: {
      //去掉刻度
      show: false,
      // inside: true,
      lineStyle: {
        color: 'rgba(68, 68, 68, 0.49)',
        width: 2 //这里是坐标轴的宽度
      }
    },
    axisLabel: {
      /* interval: 0, */
      interval: 2,
      hideOverlap: true,
      textStyle: {
        color: '#fff',
        fontSize: 12
      }
    },
    // y轴的颜色和宽度
    axisLine: {
      onZero: false,
      show: true,
      lineStyle: {
        color: 'rgba(68, 68, 68, 0.49)',
        width: 1 //这里是坐标轴的宽度
      }
    }
  },
  series: [
    {
      /*  data: [150, 230, 224, 218, 135, 147, 260],
       type: 'line' */
    }
  ]
}