'use strict';
// 获取图表配置
let colorList=['#1C76FD','#106c11','#C805b5','#a5770b','#7400c1','#2235b4','#0fb435','#0faeb4','#590003','#0faeb4']
function getChartOption(dataArray, vdi) {
    let unit='V'
    // 如果vdiArr没有传入，则默认为[0.05, 0.1202]
    let vdiArr = vdi || []
    let dataArr = []
    let legendData = []
    // 遍历生成series
    let newSeries = []
    dataArray.forEach((i,index) => {
        let { data, measlocName } = i
            dataArr = [...data, ...dataArr]
            legendData.push(measlocName)
            newSeries.push({
                name: measlocName,
                id: measlocName,
                type: 'line',
                color: colorList[index % 10],
                // smooth: true,
                symbolSize: data.length == 1 ? 5 : 0.0001,
                emphasis: {
                    itemStyle: {
                        borderWidth: 2,
                        shadowColor: 'rgba(1, 119, 251, 0.5)',
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowOffsetY: 0,
                    },
                },
                data,
            })
    })
    let dataList = dataArr.map(i => { return Number(i[1]) })
    let dataMin = Math.min(...dataList)
    let dataMax = Math.max(...dataList)
    // 正常、注意、危险
    let markLineData = []
    // 如果vdiArr有值，则遍历vdiArr，生成markLineData
    if (vdiArr.length) {
        dataMin = Math.min(dataMin, vdi[0])
        dataMax = Math.max(dataMax, vdi[1])
        vdiArr.forEach((item, index) => {
            markLineData.push({
                name: index == 0 ? '低压' : '高压',
                id: `偏置电压_${index + 1}`,
                yAxis: item,
                lineStyle: {
                    color: '#F56C6C',
                    type: 'solid'
                },
                label: {
                    show: false,
                },
                emphasis: {
                    label: {
                        show: true,
                        color: '#000',
                        fontSize: 12,
                    }
                }
            },
            )
        })
    }
   
    // 返回图表配置
    return {
        legend: {
            show: true,
            icon: 'rect',
            type: 'scroll',
            top: 10,
           // left: 50,
           //  right:50,
            itemWidth: 10,
            itemHeight:10,
            data: legendData,
        },
        title: {
            show: false,
        },
        grid: {
            top: 50,
            bottom: 35,
            left: 40,
            right: 20,
        },
        dataZoom: [
            {
                type: 'inside',
                xAxisIndex: [0],
            }, {
                type: 'inside',
                yAxisIndex: [0],
            },
        ],
        tooltip: {
            confine:true,
            trigger: 'axis',
            textStyle: {
                fontSize: 12,
            },
            axisPointer: {
                type: 'line',
                snap: true,
            },
            borderColor: 'transparent',
            axisPointer: {
                animation: false
            }
        },
        xAxis: {
            type: 'time',
          
            splitLine: {
                show: false
            },
            axisLabel: {
             
                width: 100,
                overflow: 'break',
                formatter: (data) => {
                    let date = new Date(data);
                    let year = date.getFullYear();
                    let month = (date.getMonth() + 1).toString().padStart(2, '0'); // 月份是从0开始的，所以要加1，并确保是两位数
                    let day = date.getDate().toString().padStart(2, '0'); // 确保是两位数
                    return `${year}-${month}-${day}`
                },
            },
        },
        yAxis: {
            type: 'value',
             name: unit|| 'v',
               nameLocation: 'middle',
               nameGap: 25,
               nameRotate: 0,
               nameTextStyle: {
                 color: '#000',
                 fontSize: 12,
               },
            min: Math.floor(dataMin * 0.9),
            max: Math.ceil(dataMax * 1.1),
            boundaryGap: [0, '100%'],
            splitLine: {
                show: true,
                lineStyle: {
                    type: 'dashed',
                    color: 'rgba(165, 180, 203, 0.7)',
                }
            },
           
        },
        series: [
            ...newSeries,
            {
                id: 'markline_level',
                name: "markline_level",
                type: 'line',
                data: [],
                tooltip: {
                    show: false
                },
                markLine: {
                    precision: 8, // 精度，小数点后几位，默认2，如果画0.003，画出来就是0
                    symbol: 'none',
                    data: markLineData
                }
            }
        ]
    }
}
