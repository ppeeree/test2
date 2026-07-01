import { getStationList, getEnitiyTree } from '@/api/screen/index'

let windList = []

//获取风场列表
export function getWindParkList() {
  getStationList('region').then(result => {
    result.data.data[0]?.childNode.forEach(item => {
      const { childNode } = item
      childNode.forEach(i => {
        if (!windList.find(j => j.id == i.id)) {
          windList.push(i)
        }
      })
    })
  })
  return windList
}

//根据风场获取机组
export function getTurbineList(id) {
  let turbineList = []
  getEnitiyTree({ stationId: id }).then(res => {
    if (res.data.code == 200) {
      res.data.data[0].childNode.forEach(item => {
        if (!turbineList.find(j => j.entityId == item.entityId)) {
          turbineList.push(...res.data.data[0].childNode)
        }
      })
    }
  })
  return turbineList
}

//递归拿到名称
export function fuzzyQueryTree(arr, value) {
  if (!Array.isArray(arr) || arr.length === 0) {
    return []
  }
  let result = []
  arr.forEach(item => {
    if (item.name === value) {
      const children = fuzzyQueryTree(item.children, value)
      const obj = { ...item, children }
      result.push(obj)
    } else {
      if (item.children && item.children.length > 0) {
        const children = fuzzyQueryTree(item.children, value)
        const obj = { ...item, children }
        if (children && children.length > 0) {
          result.push(obj)
        }
      }
    }
  })
  return result
}

export function deleteCompChildren(arr) {
  arr.forEach(item => {
    if (item.children && item.children.length !== 0) {
      deleteCompChildren(item.children)
      item.childNode = item.children
    }
  })
}

//根据搜索返回数据，筛选结构
export function getSearchCompList(list, searchList) {
  list &&
    list.forEach(item => {
      if (item.children) {
        getSearchCompList(item.children, searchList)
      } else {
        if (!searchList.find(j => j.value == item.name)) {
          list.splice(list.indexOf(item), 1)
        }
      }
    })
  return list
}

export function handlerCompList(list) {
  list.forEach(item => {
    if (item.children && item.children.length) {
      handlerCompList(item.children)
    } else {
      item.children = handlerMeasureList(item.measureList)
    }
  })
  return list
}

export function handlerMeasureList(list) {
  let Slist = []
  let Olist = []
  list &&
    list.forEach(ele => {
      if (ele.key == 'section') {
        Slist.push({ code: ele.code, key: ele.key, name: ele.name, children: [] })
      } else {
        Olist.push({ code: ele.code, key: ele.key, name: ele.name })
      }
    })

  if (Olist.length) {
    Slist.forEach(item => {
      item.children.push(...Olist)
    })
  }

  return Slist
}

//根据叶子节点code拿到路径
/*** 根据curKey在data中找，data中对应的curKey的键值是code  */
export function getPathByKey(curKey, data, code) {
  let result = [] // 记录路径结果
  let traverse = (curKey, path, data) => {
    if (data.length === 0) {
      return
    }
    for (let item of data) {
      path.push(item[code])
      if (item[code] === curKey) {
        result = JSON.parse(JSON.stringify(path))
        return
      }
      const children = Array.isArray(item.children) ? item.children : []
      traverse(curKey, path, children) // 遍历
      path.pop() // 回溯
    }
  }
  traverse(curKey, [], data)
  return result
}

//根据key在data中找到对应的叶子结点
let res
export function findLeafNode(key, data, code) {
  res = undefined
  for (let i = 0; data && i < data.length; i++) {
    let ele = data[i]

    if (ele[code] == key) {
      res = ele
      return res
    }

    if (ele.children && ele.children.length > 0) {
      res = findLeafNode(key, ele.children, code)
      if (res != undefined) return res
    }
  }
}
