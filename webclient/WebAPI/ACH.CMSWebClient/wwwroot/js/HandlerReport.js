
/*
    对生成报告中需要的数据做处理
*/

let turbineStatusIndex = {
    /*3: '正常',
    4: '注意',
    5: '警告',
    6: '危险',*/

    3: '正常',
    4: '通讯异常',
    7: '传感器故障',
    8: '转速异常'
}

// 处理采集器汇总信息
function handlerTurbineSumList(allTurbineList) {
    let turbineList = []
    let data = JSON.parse(JSON.stringify(allTurbineList))
    data.forEach(item => {
        item.children = groupByIndex(item.children, 'alarmDegree', true)
        item.children.forEach(ite => {
            let turbineStatus = turbineStatusIndex.hasOwnProperty(ite.name) ? turbineStatusIndex[ite.name] : '无状态'
            turbineList.push({
                comp: item.name,
                status: turbineStatus,
                num: ite.children.length,
                list: ite.children.map(j => { return j.windTurbineName })
            })
        })
    })
    // console.log('报告中的采集器数据', turbineList)
    return turbineList
}

// 处理采集通道类型汇总信息
function handlerTypeSumList(allFilterDatas) {
    let result = []
    allFilterDatas.forEach(item => {
        let list = groupByIndex(item.data, 'status', true)
        list.forEach(item => {
            let turbineList = groupByIndex(item.children, 'windTurbineName', true)
            item.turbineList = turbineList.map(j => { return j.name })
            item.turbineNum = item.turbineList.length
        })
        
        result.push({ type: item.type, data: list.map(j => { return { status: j.name, num: j.children.length } }) })
    })

    return result
}

// 处理详细章节信息
function hanlderSecondContent(allFilterDatas) {
    let allData = JSON.parse(JSON.stringify(allFilterDatas))

    allData.forEach(item => {
        item.data = groupByIndex(item.data, 'windTurbineName', true)
        item.data.forEach(ite => {
            ite.children = groupByIndex(ite.children, 'daUnitName', true)
        })
    })

    let result = []
    allData.forEach((item) => {
        item.data.forEach(ite => {
            let obj = result.find(j => j.name == ite.name)
            if (obj) {
                ite.children.forEach(e => {
                    let compObj = obj.children.find(j => j.name == e.name)
                    if (!compObj) {
                        obj.children.push(e)
                    } else {
                        compObj.children.push(...e.children)
                    }
                })

            } else {
                result.push(ite)
            }
        })
    })

    result = sortObject(result, 'name')
    result.forEach((item, index) => {
        item.turbineIndex = index + 1
        item.children = sortObject(item.children, 'name')
        item.children.forEach((ite, inde) => {
            ite.compIndex = inde + 1
            ite.num = ite.children.length
            ite.children.forEach(it => {
                if (it.measLocationID != null && it.measLocationID.indexOf("RSPD") != -1) {
                    it.measLocationName = '转速'
                }
            })
        })
    })

    return result
}

// 按照从小到大排序
function sortObject(array, key) {
    return array.sort(function (a, b) {
        let x = a[key]
        let y = b[key]
        return x < y ? -1 : x > y ? 1 : 0
    })
}
