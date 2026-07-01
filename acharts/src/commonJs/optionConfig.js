

export const colorList = [
  '#51B031',
  '#4C86EA',
  '#C1BF3A',
  '#A03AC1',
  '#C13A3A',
  '#3AAFC1',
  '#C17C3A',
  '#3A5AC1',
  '#79D501',
  '#018BD6',
  '#D5D201',
  '#C301D5',
  '#D5013D',
  '#01D5D5',
  '#D59601',
  '#2801D5',
  '#35C492',
  '#2D5CAD',
  '#ACB31C',
  '#7A15A2',
  '#AC2D3E',
  '#4396AD',
  '#AD8F3C',
  '#332DAC',
  '#08A400',
  '#3753E5',
  '#A59500',
  '#7C02B9',
  '#C02B1A',
  '#0096AD',
  '#C1651A',
  '#441AC0',
  '#78D791',
  '#78A4D8',
  '#CFD778',
  '#B178D7',
  '#D77878',
  '#78D4D7',
  '#D7A678',
  '#7888D7'
]

export const lightThemeColor = {
  backgroundColor: "#fff",
  fontColor: '#000',
  borderLineColor: '#EAF0F4',
  axisColor: '#AEB6BC'
}
export const darkThemeColor = {
  backgroundColor: 'rgba(32, 34, 42, 1)',// "srgba(32,34,42,0.8)",
  fontColor: '#fff',
  borderLineColor: 'rgba(255, 255, 255, 0.1)',
  axisColor: 'rgba(68, 68, 68, 0.49)'
}
export const getOptions = (theme) => {
  let themeType = theme || 'light'
  let colorStyle = themeType == 'light' ? lightThemeColor : darkThemeColor
  const { backgroundColor, axisColor, fontColor, borderLineColor } = colorStyle
  return {
    color: colorList,
    backgroundColor: backgroundColor,
    legendStyle: {
      show: true,
      /*  type: 'scroll',  //legend要多，进行分页处理
       orient: 'horizontal', */
      itemHeight: 7,
      itemWidth: 10,
      textStyle: {
        color: fontColor,
        fontSize: 12,
      },
    },
    titleStyle: {
      show: true,
      left: 5,
      top: 0,
      textStyle: {
        color: fontColor,
        fontSize: 14,
        fontWeight: 700
      }
    },
    toolboxStyle: {
      right: 10,
      top: 5,
    },
    toolboxFeature: {
      saveAsImage: {
        show: true,
        title: "保存为图片",
      },
      restore: {
        show: true,
      },
      dataZoom: {
        show: true,
        iconStyle: {
          //不需要图标可以设置隐藏按钮
          /* opacity: 0 */
        },
        brushStyle: {
          color: 'rgba(23,47,214,0.2)'
        }
        // yAxisIndex: 'none'
      }
    },
    gridStyle: {
      show: true,
      backgroundColor: 'transparent',
      borderColor: borderLineColor,
    },
    xAxisStyle: {
      nameTextStyle: {
        fontSize: 12,
        color: fontColor,
        width: 20,
        height: 40,
        padding: [5, 0, 5, 0],
        overflow: 'break',
        verticalAlign: 'middle'
      },
      splitLine: {
        show: true,
        lineStyle: {
          color: borderLineColor,
          width: 1
        }
      },
      axisTick: {
        //去掉刻度
        show: false,
        // inside: true,
        lineStyle: {
          color: borderLineColor,
          width: 2 //这里是坐标轴的宽度
        }
      },
      axisLabel: {
        /* interval: 0, */
        hideOverlap: true,
        interval: 2,
        textStyle: {
          color: fontColor,
          fontSize: 12
        },
      },
      // y轴的颜色和宽度
      axisLine: {
        // onZero: false,
        lineStyle: {
          color: axisColor,
          width: 1 //这里是坐标轴的宽度
        }
      }
    },
    yAxisStyle: {
      nameTextStyle: {
        fontSize: 12,
        color: fontColor,
      },
      splitLine: {
        show: true,
        lineStyle: {
          color: borderLineColor,
          width: 1
        }
      },
      axisTick: {
        //去掉刻度
        show: false,
        // inside: true,
        lineStyle: {
          color: borderLineColor,
          width: 2 //这里是坐标轴的宽度
        }
      },
      axisLabel: {
        /* interval: 0, */
        interval: 2,
        hideOverlap: true,
        textStyle: {
          color: fontColor,
          fontSize: 12
        }
      },
      // y轴的颜色和宽度
      axisLine: {
        onZero: false,
        show: true,
        lineStyle: {
          color: axisColor,
          width: 1 //这里是坐标轴的宽度
        }
      }
    },
  }


}

