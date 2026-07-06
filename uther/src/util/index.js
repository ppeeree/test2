import { getStore } from '@/util/store'
import { setTheme } from '@/util/util'
import { ElMessage, ElMessageBox } from 'element-plus'
import map from './map'
// import { getCommonApi, /* getUavBrandApi */ } from '../api/common'
// 使用方法：this.$utils.方法名

const UtilData = {
  map: map,
  /**
   * @param {*} title 输出名称
   * @param {*} msg  输出内容
   * @param {*} status  0：紧急 red ,1：严重 yellow ,2：提示 blue
   * @param tip 开启、关闭 顶部提示
   */
  consoleStyle(title, msg, status, tip) {
    if (tip) {
      setTimeout(() => {
        this.$message({
          message: msg,
          type: status === 0 ? 'error' : status === 0 ? 'warning' : 'info'
        })
      }, 1000)
    }
    console.log(
      '%c '.concat('' + msg + '', ' %c ').concat('' + title + '', ' %c'),
      'background:#35495E; padding: 1px; border-radius: 3px 0 0 3px; color: #fff;',
      'background:'.concat(
        // eslint-disable-next-line eqeqeq
        '' + status == 0 ? 'red' : status == 1 ? 'orange' : 'blue' + '',
        '; padding: 1px; border-radius: 0 3px 3px 0;  color: #fff;'
      ),
      'background:transparent'
    )
  },
  /**
   * @function format 日期格式化
   * @params timestamp 必填，需格式化的时间戳或者其他日期格式
   * @return fmt 选填，需返回的时间格式，默认 yyy-MM-dd hh:mm:ss
   * 或者：yyyy年MM月dd日 hh时mm分ss秒
   * 或者<i>ss<i>
   * */
  format(timestamp, fmt) {
    fmt = fmt == undefined ? 'yyyy-MM-dd hh:mm:ss' : fmt
    if (timestamp == undefined) {
      timestamp = new Date().getTime()
    } else if (String(timestamp).length == 10) {
      timestamp = timestamp * 1000
    }
    timestamp = new Date(timestamp)
    if (/(y+)/.test(fmt)) {
      fmt = fmt.replace(RegExp.$1, (timestamp.getFullYear() + '').substr(4 - RegExp.$1.length))
    }
    const o = {
      'M+': timestamp.getMonth() + 1,
      'd+': timestamp.getDate(),
      '[H|h]+': timestamp.getHours(),
      'm+': timestamp.getMinutes(),
      's+': timestamp.getSeconds()
    }
    for (const k in o) {
      if (new RegExp(`(${k})`).test(fmt)) {
        const str = o[k] + ''
        fmt = fmt.replace(RegExp.$1, RegExp.$1.length === 1 ? str : ('00' + str).substr(str.length))
      }
    }
    return fmt
  },
  // 根据当前时间戳
  getNowStamp() {
    return Math.floor(+new Date() / 1000)
  },
  trueOrFalse(num) {
    let _res = '-'
    if (num == '1') {
      _res = '是'
    } else if (num == '0') {
      _res = '否'
    }
    return _res
  },
  strCutByLen(str, len) {
    const strLen = str.length
    let _str = ''
    let _len = 0
    for (let i = 0; i < strLen; i++) {
      const abs = str.charAt(i)
      // 中文计算为2个字符
      _len += escape(abs).length > 4 ? 2 : 1
      _str += abs
      // 达到指定长度在其后添加…返回
      if (_len >= len) {
        _str += '…'
        return _str
      }
    }
    return _str
  },
  /**
   * @description: 确认消息提示弹窗
   * @param {object} options [必填] 接收参数 可为空 {}
   * @param {function} callback [必选] 确认回调方法
   * @param {function} catchFn [可选] 取消回调方法
   */
  confirm(options, callback, catchFn) {
    const { msg = '删除后不可恢复，请确认是否删除？', title = '提示', type = 'warning', distinguishCancelAndClose = false } = options
    ElMessageBox.confirm(msg, title, {
      type,
      dangerouslyUseHTMLString: true,
      distinguishCancelAndClose
    })
      .then(() => {
        callback()
      })
      .catch(action => {
        if (typeof catchFn === 'function') {
          catchFn(action)
        }
      })
  },
  /**
   * @param {*} content 展示信息
   * @param {*} type 展示类型
   */
  showAlert(content, type = 0) {
    const typeList = ['success', 'error', 'warning', 'info']
    ElMessage.closeAll()
    ElMessage[typeList[type]](content)
  },
  /**
   * @description: 弹窗拖拽功能
   */
  dialogDrag(nodeName) {
    const dialogHeaderEl = document.querySelector(`${nodeName} >div.dialog_header`)
    const dragDom = document.querySelector(`${nodeName}`)
    dialogHeaderEl.style.cssText += ';cursor:move;'
    dragDom.style.cssText += ';top:0px;'

    const getStyle = (function () {
      if (window.document.currentStyle) {
        return (dom, attr) => dom.currentStyle[attr]
      } else {
        return (dom, attr) => getComputedStyle(dom, false)[attr]
      }
    })()

    dialogHeaderEl.onmousedown = e => {
      const disX = e.clientX - dialogHeaderEl.offsetLeft
      const disY = e.clientY - dialogHeaderEl.offsetTop

      const dragDomWidth = dragDom.offsetWidth
      const dragDomheight = dragDom.offsetHeight

      const screenWidth = document.body.clientWidth
      const screenHeight = document.body.clientHeight

      const minDragDomLeft = dragDom.offsetLeft
      const maxDragDomLeft = screenWidth - dragDom.offsetLeft - dragDomWidth

      const minDragDomTop = dragDom.offsetTop
      const maxDragDomTop = screenHeight - dragDom.offsetTop - dragDomheight

      let styL = getStyle(dragDom, 'left')
      let styT = getStyle(dragDom, 'top')

      styL = +styL.replace(/\px/g, '')
      styT = +styT.replace(/\px/g, '')

      document.onmousemove = function (e) {
        let left = e.clientX - disX
        let top = e.clientY - disY

        if (-left > minDragDomLeft) {
          left = -minDragDomLeft
        } else if (left > maxDragDomLeft) {
          left = maxDragDomLeft
        }

        if (-top > minDragDomTop) {
          top = -minDragDomTop
        } else if (top > maxDragDomTop) {
          top = maxDragDomTop
        }
        dragDom.style.cssText += `;left:${left + styL}px;top:${top + styT}px;`
      }
      document.onmouseup = function () {
        document.onmousemove = null
        document.onmouseup = null
      }
    }
  },
  /**
   * @param {Function} func
   * @param {number} wait
   * @param {boolean} immediate
   * @return {*}
   */
  debounce(func, wait, immediate) {
    let timeout, args, context, timestamp, result

    const later = function () {
      // 据上一次触发时间间隔
      const last = +new Date() - timestamp

      // 上次被包装函数被调用时间间隔 last 小于设定时间间隔 wait
      if (last < wait && last > 0) {
        timeout = setTimeout(later, wait - last)
      } else {
        timeout = null
        // 如果设定为immediate===true，因为开始边界已经调用过了此处无需调用
        if (!immediate) {
          result = func.apply(context, args)
          if (!timeout) context = args = null
        }
      }
    }

    return function (...args) {
      context = this
      timestamp = +new Date()
      const callNow = immediate && !timeout
      // 如果延时不存在，重新设定延时
      if (!timeout) timeout = setTimeout(later, wait)
      if (callNow) {
        result = func.apply(context, args)
        context = args = null
      }

      return result
    }
  },
  /**
   * uuid
   */
  getUUID() {
    let s = []
    let hexDigits = '0123456789abcdef'
    for (let i = 0; i < 36; i++) {
      s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1)
    }
    s[14] = '4'
    s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1)
    s[8] = s[13] = s[18] = s[23] = '-'

    let uuid = s.join('')
    return uuid
  },
  /**
   *
   * @param {*} params 数据源
   * @param {*} length 取整长度
   */
  formatterAxisLabel(params, length = 4) {
    let newParamsName = '' // 最终拼接成的字符串
    let paramsNameNumber = params.length // 实际标签的个数
    let provideNumber = length // 每行能显示的字的个数
    let rowNumber = Math.ceil(paramsNameNumber / provideNumber) // 换行的话，需要显示几行，向上取整
    /**
     * 判断标签的个数是否大于规定的个数， 如果大于，则进行换行处理 如果不大于，即等于或小于，就返回原标签
     */
    // 条件等同于rowNumber>1
    if (paramsNameNumber > provideNumber) {
      /** 循环每一行,p表示行 */
      for (let p = 0; p < rowNumber; p++) {
        let tempStr = '' // 表示每一次截取的字符串
        let start = p * provideNumber // 开始截取的位置
        let end = start + provideNumber // 结束截取的位置
        // 此处特殊处理最后一行的索引值
        if (p == rowNumber - 1) {
          // 最后一次不换行
          tempStr = params.substring(start, paramsNameNumber)
        } else {
          // 每一次拼接字符串并换行
          tempStr = params.substring(start, end) + '\n'
        }
        newParamsName += tempStr // 最终拼成的字符串
      }
    } else {
      // 将旧标签的值赋给新标签
      newParamsName = params
    }
    //将最终的字符串返回
    return newParamsName
  },
  /**
   * 根据txt删除三维entities
   * @param {*} nameText
   */
  removeEntitiesByName(nameText, viewer) {
    const func = name => {
      viewer.entities.values.forEach(item => {
        if (item.name && item.name.includes(name)) {
          viewer.entities.remove(item)
          func(name)
        }
      })
    }
    func(nameText)
  },
  /**
   * 请求封装
   */
  xmlHttpRequest(url) {
    return new Promise(resolve => {
      let request = new XMLHttpRequest()
      request.open('get', url) /*设置请求方法与路径*/
      request.send(null) /*不发送数据到服务器*/
      request.onload = () => {
        /*XHR对象获取到返回信息后执行*/
        if (request.status == 200) {
          /*返回状态为200，即为数据获取成功*/
          resolve(request.responseText)
        }
      }
    })
  }
  /* globalCodeList(code, call) {
    getCommonApi({ code })
      .then((res) => call && call(res.data.data))
      .catch((err) => console.log(err))
  }, */
  // 主题色页面判断
  /*  confirmStyle(name) {
     debugger
     let themeName = getStore({ name: 'themeName' })
     if (themeName != name) {
       this.$store && this.$store.commit('SET_THEME_NAME', name)
       setTheme(name)
     }
   }, */
  /*   getUavBrandApi (code, call) {
      getUavBrandApi({ code })
        .then((res) => call && call(res.data.data))
        .catch((err) => console.log(err))
    } */
}

const UTIL = {
  install: app => {
    app.config.globalProperties.$utils = UtilData
  }
}

export default UTIL
