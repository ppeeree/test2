export const calcDate = (date1, date2) => {
  let date3 = date2 - date1

  let days = Math.floor(date3 / (24 * 3600 * 1000))

  let leave1 = date3 % (24 * 3600 * 1000) //计算天数后剩余的毫秒数
  let hours = Math.floor(leave1 / (3600 * 1000))

  let leave2 = leave1 % (3600 * 1000) //计算小时数后剩余的毫秒数
  let minutes = Math.floor(leave2 / (60 * 1000))

  let leave3 = leave2 % (60 * 1000) //计算分钟数后剩余的毫秒数
  let seconds = Math.round(date3 / 1000)
  return {
    leave1,
    leave2,
    leave3,
    days: days,
    hours: hours,
    minutes: minutes,
    seconds: seconds
  }
}

/**
 * 日期格式化
 */
export function dateFormat(date, format) {
  format = format || 'yyyy-MM-dd hh:mm:ss'
  if (date !== 'Invalid Date') {
    let o = {
      "M+": date.getMonth() + 1, //month
      "d+": date.getDate(), //day
      "h+": date.getHours(), //hour
      "m+": date.getMinutes(), //minute
      "s+": date.getSeconds(), //second
      "q+": Math.floor((date.getMonth() + 3) / 3), //quarter
      "S": date.getMilliseconds() //millisecond
    }
    if (/(y+)/.test(format)) format = format.replace(RegExp.$1,
      (date.getFullYear() + "").substr(4 - RegExp.$1.length))
    for (let k in o)
      if (new RegExp("(" + k + ")").test(format))
        format = format.replace(RegExp.$1,
          RegExp.$1.length === 1 ? o[k] :
            ("00" + o[k]).substr(("" + o[k]).length))
    return format
  }
  return ''

}

/**
 * 当前时间戳
 */
export function dateNow() {
  return dateFormat(new Date(), "yyyyMMddhhmmss")
}

// 对数组对象按照日期排序
/**
 * @description 2.根据日期时间混合排序
 * @param {Object[]} dataList - 要排序的数组
 * @param {string} property - 传入需要排序的字段
 * @param {boolean} bol - true: 升序；false: 降序；默认为true 升序
 * @return {Object[]} dataList - 返回改变完顺序的数组
 */
export function dateSort(dataList, property, bol = true) {
  dataList.sort(function (a, b) {
    if (bol) {
      // return a[property].localeCompare(b[property]); // 升序
      return Date.parse(a[property]) - Date.parse(b[property])  // 升序
    } else {
      // return b[property].localeCompare(a[property]); // 降序
      return Date.parse(b[property]) - Date.parse(a[property])  // 降序
    }
  })
  return dataList
}