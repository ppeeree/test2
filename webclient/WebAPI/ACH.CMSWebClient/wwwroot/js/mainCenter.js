
/*
    针对中间趋势图处理JS
*/


// 1.3、根据点击的机组获取波形数据
function getWaveData(item) {
    let endTime = moment().format('YYYY-MM-DD');
    let startTime = moment(new Date()).subtract(3, "months").format('YYYY-MM-DD');

    console.log('获取波形数据的时间', startTime, endTime)

    let chartUrl = "api/state/GetChartByTurbineId/" + item.windTurbineID + "&" + item.dauID + "&" + startTime + "&" + endTime
    fetch(chartUrl)
        .then((response) => response.json())
        .then((data) => handlerChartData(item,data))
        .catch((error) => console.error("Unable to get items.", error));
}

// 1.4、处理返回数据格式
function handlerChartData(clickObj,data) {
    let seriesData = []
    let legendData = []


    if (!data.length) {
       alert('暂无数据！')
    } else {
        let sortData = sortObject(data, 'dcAcquisitionTime')
        let allTimeList = sortData.map(j => { return j.dcAcquisitionTime.toString().split("T").join(" ") })
        let newAllTime = [...new Set(allTimeList)].sort((a, b) => a - b)


        let groupData = groupByIndex(data, 'measLocationID', true)

        groupData.forEach(item => {
            let test =[]
            newAllTime.map(j => {
                let obj = item.children.find(ele => ele.dcAcquisitionTime.toString().split("T").join(" ") == j)
                if (obj) {
                    test.push( obj.dcDataValue)
                } else {
                    test.push( null)
                }
            })

            seriesData.push({
                name: item.children[0].measLocationName,
                type: 'line',
                connectNulls: true, 
                data: test
            })

            legendData.push(item.children[0].measLocationName)
        })
        if (seriesData.length) {
            initChart(clickObj,seriesData, legendData, newAllTime)
        }

    }
    
}

// 1.5、初始化图表
function initChart(clickObj,seriesData, legendData,xData) {
    const myChart = echarts.init(document.getElementById('waveChart'))
    myChart.showLoading()
    myChart.clear()

    const option = {
        tooltip: {
            trigger: 'axis',
            position: function (pt) {
                return [pt[0], '10%'];
            }
        },
        title: {
            text: clickObj.windTurbineName + "-" + clickObj.daUnitName
        },
        legend: {
            orient: 'vertical',
            right: '20',
            bottom: '20',
            data: legendData,
            itemWidth: 9,
            itemHeight: 9,
            textStyle: {
                fontSize: 11
            }
        },
        toolbox: { //内置工具
            feature: {
                dataZoom: {
                    yAxisIndex: 'none'
                },
                restore: {},
                saveAsImage: {}
            }
        },
        dataZoom: [
            {
                type: 'inside',
                filterMode: 'filter',
                start: 0,
                end: 100
            },
            {
                start: 0,
                end: 100,
                filterMode: 'filter'
            }
        ],
        xAxis: {
            type: 'category',
            data: xData.sort((a, b) => a - b),
        },
        yAxis: { type: 'value' },
       
        series: seriesData
    }

    myChart.hideLoading()
    option && myChart.setOption(option,true)
}
