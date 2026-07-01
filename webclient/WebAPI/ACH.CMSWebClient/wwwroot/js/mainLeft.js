
/*
    针对左侧测点状态汇总处理JS
*/

function handlerAlarmStatus(index, data) {
    let shockIndex = {
        5: '正常',
        4: '采集振动值偏小',
        3: '直流分量异常',
        2: '跳变',
        1: '断路',
        0: '短路'
    }
    let speedIndex = {
        3: '未采集到数据',
        2: '故障',
        1: '正常',
        0: '未知'
    }

    let res = data.map(item => {
        return {
            status: index == 0 ? speedIndex[item.alarmState] : shockIndex[item.alarmState],
            ...item
        }
    })
    return res
}
// 3.1、处理圆环的数据：按照异常名称对数据分组
function handlerRingData(arr) {
    let groupList = []
    arr.map(mapItem => {
        let index = groupList.findIndex(j => j.alarmState == mapItem.alarmState)
        if (index === -1) {
            groupList.push({
                alarmState: mapItem.alarmState,
                name: mapItem.status,
                children: [{ ...mapItem, name: mapItem.status }]
            })
        } else {
            groupList[index].children.push({ ...mapItem, name: mapItem.status })
        }
    })
    return groupList
}

function handlerRingOption(arr, allNum) {

    let optionData = []
    arr.forEach(item => {
        optionData.push({
            name: item.name,
            value: item.children.length
        })
    })

    const option = {
        title: {
            text: '共 ' + allNum + '个通道',
            left: 'center',
            textStyle: {
                fontSize: 14
            },
        },
        tooltip: {
            trigger: 'item'
        },
        legend: {
            orient: 'horizontal',
            right: '10',
            bottom: '10',
            itemWidth: 9,
            itemHeight: 9,
            textStyle: {
                fontSize: 11
            }
        },
        series: [
            {
                type: 'pie',
                radius: '50%',
                data: optionData,
                emphasis: {
                    itemStyle: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }
        ]
    }

    return option
}


// 6、处理表格的标题
function handlerTableTitle(tableData) {
    let dom = document.getElementById("leftTitle")
    let text = '共：' + tableData.length +'\n\n'
   

    // 根据采集器分
    let compList = groupByIndex(tableData, 2)
    compList.forEach(j => {
        let newText = j.name + '有：' + j.children.length + '\n\n'
        text = text + newText

    })

    dom.textContent = text

    // console.log('处理标题', compList)
}

// 根据sum对数据分组
function groupByIndex(arr,sum,isNoneList) {
    let groupList = []
    arr.map(mapItem => {
        let index = groupList.findIndex(j => j.name == mapItem[sum])
        if (index === -1) {
            groupList.push({
                name: mapItem[sum],
                children: !isNoneList ? [[...mapItem]] : [{ ...mapItem}]
            })
        } else {
            if (!isNoneList) {
                groupList[index].children.push([...mapItem])
            } else {
                groupList[index].children.push({...mapItem})
            }
        }
    })
    return groupList
}

