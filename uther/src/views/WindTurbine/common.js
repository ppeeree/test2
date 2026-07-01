//对钢索变色数据进行聚合
export function mergeSteelList(list) {
  const ChangeValue = {
    attention: '1',
    warning: '2',
    danger: '3'
  }
  const tranformChangeValue = {
    0: 'nostate',
    1: 'attention',
    2: 'warning',
    3: 'danger'
  }
  let newList = [] //聚合后的塔筒数组
  let stlList = [] //存储状态数组
  let obj = {
    datas: {},
    name: '钢索'
  }
  let statusList = ['danger', 'warning', 'attention']
  list.forEach(item => {
    if (item.type == 'SSD') {
      if (!newList.find(j => j.name == '钢索')) {
        newList.push(obj)
      }
      if (statusList.find(j => j == item.datas.entityStatus)) {
        stlList.push(ChangeValue[item.datas.entityStatus])
      }
    } else {
      newList.push(item)
    }
  })

  let steel = newList.find(j => j.name == '钢索')
  if (steel) {
    let stlStatus
    if (stlList.length !== 0) {
      stlStatus = tranformChangeValue[Math.max(...stlList)]
    } else {
      stlStatus = ''
    }
    newList.find(j => j.name == '钢索').datas.entityStatus = stlStatus
  }

  return newList
}

//将字符串中中文转化为数字
export function handlerChineseName(name) {
  let text = ''
  const map = {
    零: 0,
    一: 1,
    壹: 1,
    二: 2,
    贰: 2,
    两: 2,
    三: 3,
    叁: 3,
    四: 4,
    肆: 4,
    五: 5,
    伍: 5,
    六: 6,
    陆: 6,
    七: 7,
    柒: 7,
    八: 8,
    捌: 8,
    九: 9,
    玖: 9,
    十: 10,
    拾: 10
  }

  for (let i = 0; i < name.length; i++) {
    const char = name.charAt(i)
    const value = map[char]
    if (value !== undefined) {
      text += value
      break; // 遇到数字后直接跳出循环
    } else {
      text += char
    }
  }

  return text
}
