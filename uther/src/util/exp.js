import cloneDeep from 'lodash/cloneDeep'
import fromPairs from 'lodash/fromPairs'
import isFinite from 'lodash/isFinite'
import toPairs from 'lodash/toPairs'
import merge from 'lodash/merge'
import mergeWith from 'lodash/mergeWith'
import pick from 'lodash/pick'
import isArray from 'lodash/isArray'
/**
 * 定义目标对象原型。
 * 返回定义后的对象。
 * @param {object} obj 目标对象
 * @param {object} proto 原型属性
 * @param {boolean} clear 定义时是否清空原型属性
 */
export function defineProto(obj, proto, clear = true) {
  Object.setPrototypeOf(obj, clear ? proto : merge(Object.getPrototypeOf(obj), proto))
  return obj
}

/**
 * 创建枚举对象。
 * @param {object} obj 枚举基对象
 *
 * @example
 * const WhetherEnum = createEnum({ 0: '否', 1: '是' })
 * WhetherEnum.0 === '是'
 * WhetherEnum.是 === 0
 */
export function createEnum(obj = {}) {
  const base = toPairs(cloneDeep(obj))
  const reverse = cloneDeep(base).map(([k, v]) => [v, isFinite(Number(k)) ? Number(k) : k])
  return defineProto(fromPairs([...base, ...reverse]), { $_base: obj })
}

/**
 * 定义目标与检索数组。
 * 返回目标数组相同检索元素。
 * @param {Array} arr1 目标对象
 * @param {Array} arr2 检索对象
 */
export function getInclude(arr1, arr2) {
  return arr1.filter(item => {
    return arr2.includes(item)
  })
}

/**
 * 更新对象值。
 * 只会更新目标对象存在的属性值，且会修目标对象
 * @param {object} target 目标对象
 * @param  {...object[]} sources 待合并资源对象集
 */
export function upObjVal(target, ...sources) {
  const onlySource = merge({}, ...sources)
  return mergeWith(target, pick(onlySource, Object.keys(target)), (objVal, srcVal) => {
    if (isArray(objVal)) {
      // 给数组属性重新赋值，触发数组数据派发更新
      return merge([], objVal, srcVal)
    }
  })
}

/**
 * 获取tree级联关系链
 * @param {Number} id 目标id
 * @param {String} key 目标id
 * @param  {Array} list 对象集
 */
export function getRouterPath(id, key, list, opt) {
  if (!Array.isArray(list) || list.length === 0) {
    return []
  }
  const path = []
  const treeFindPath = (list, id, path) => {
    for (const item of list) {
      path.push(item[opt || key])
      if (item[key] === id) {
        return path
      }
      if (item.children) {
        const findChildren = treeFindPath(item.children, id, path)
        if (findChildren.length) {
          return findChildren
        }
      }
      path.pop()
    }
    return []
  }
  return treeFindPath(list, id, path)
}

/**
 * 模糊查询树
 * @param {Number} value 目标value
 * @param {String} key 目标key
 * @param  {Array} arr 对象集
 */
const fuzzyQueryTree = (arr, key, value) => {
  if (!Array.isArray(arr) || arr.length === 0) {
    return []
  }
  let result = []
  arr.forEach(item => {
    if (item[key].indexOf(value) > -1) {
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

/**
 * 数字转成汉字
 * @params num === 要转换的数字
 * @return 汉字
 * */
export function toChinesNum(num) {
  let changeNum = ['零', '一', '二', '三', '四', '五', '六', '七', '八', '九']
  let unit = ['', '十', '百', '千', '万']
  num = parseInt(num)
  let getWan = temp => {
    let strArr = temp.toString().split('').reverse()
    let newNum = ''
    let newArr = []
    strArr.forEach((item, index) => {
      newArr.unshift(item === '0' ? changeNum[item] : changeNum[item] + unit[index])
    })
    let numArr = []
    newArr.forEach((m, n) => {
      if (m !== '零') numArr.push(n)
    })
    if (newArr.length > 1) {
      newArr.forEach((m, n) => {
        if (newArr[newArr.length - 1] === '零') {
          if (n <= numArr[numArr.length - 1]) {
            newNum += m
          }
        } else {
          newNum += m
        }
      })
    } else {
      newNum = newArr[0]
    }
    if (newNum === '一十') {
      newNum = '十'
    }
    return newNum
  }
  let overWan = Math.floor(num / 10000)
  let noWan = num % 10000
  if (noWan.toString().length < 4) {
    noWan = '0' + noWan
  }
  return overWan ? getWan(overWan) + '万' + getWan(noWan) : getWan(num)
}
