/*******
 * @description: 全流程事件跟踪数据处理
 * @return {*}
 */
import { getEventStatistic } from '@/api/screen/rightCardApi'
import omit from 'lodash/omit'
class FetchData {
  #id
  #type
  #startTime = null
  #endTime = null

  constructor(id, type, startTime, endTime) {
    this.#id = id
    this.#type = type
    this.#startTime = startTime
    this.#endTime = endTime
  }

  get getFetchData() {
    return new Promise((resolve, reject) => {
      getEventStatistic({
        id: this.#id,
        type: this.#type,
        startTime: this.#startTime,
        endTime: this.#endTime
      })
        .then(result => {
          resolve(result)
        })
        .catch(err => {
          reject(err)
        })
    })
  }
}

class HandleEventTrackingInfoData {
  static EVENTLEVEL = {
    first: '一级',
    second: '二级',
    third: '三级',
    fourth: '四级'
  }

  static EVENTSTATUS = {
    done: '已处理',
    // doing: '处理中',
    notdone: '未处理'
  }

  static EVENTTYPE = {
    health: '健康',
    inspection: '巡检',
    work: '工作'
  }

  static EVENTTYPENUM = {
    health: 3,
    inspection: 1,
    work: 4
  }

  static EVENTSTATUSNUM = {
    done: 1,
    // doing: 3,
    notdone: 0
  }

  static EVENTLEVELNUM = {
    first: 1,
    second: 2,
    third: 3,
    fourth: 4
  }

  #tableData
  #levelPeiData
  #statusPeiData
  #eventTypePeiData

  #handleTableData(data) {
    return data.map(
      (item, index) => {
        const { degree, entityName, message, handleStatus, time, type } = item
        return {
          eventLevel: HandleEventTrackingInfoData.EVENTLEVELNUM[degree],
          eventType: HandleEventTrackingInfoData.EVENTTYPENUM[type],
          eventSites: entityName,
          eventReason: message,
          eventCreatTime: time,
          eventStatus: HandleEventTrackingInfoData.EVENTSTATUSNUM[handleStatus],
          indexId: index,
          ...omit(item, ['degree', 'entityName', 'message', 'handleStatus', 'time', 'type'])
        }
      }
    )
  }

  // eslint-disable-next-line no-dupe-class-members
  #handleStructure(data, key, icon) {
    return data.map(item => {
      const keyUpperCase = HandleEventTrackingInfoData[key.toLocaleUpperCase()]
      const keyNum = HandleEventTrackingInfoData[(key + 'NUM').toLocaleUpperCase()]
      const obj = {
        name: keyUpperCase[item[0]],
        value: item[1],
        key,
        icon
      }
      obj[key] = keyNum[item[0]]
      return obj
    })
  }

  set levelData(data) {
    this.#levelPeiData = this.#handleStructure(data, 'eventLevel', 'circle')
    return this.#levelPeiData
  }

  set eventType(data) {
    this.#eventTypePeiData = this.#handleStructure(data, 'eventType', 'circle')
    return this.#eventTypePeiData
  }

  set statusData(data) {
    this.#statusPeiData = this.#handleStructure(data, 'eventStatus', 'circle')
    return this.#statusPeiData
  }

  set tableData(data) {
    this.#tableData = this.#handleTableData(data)
    return this.#tableData
  }

  get tableData() {
    return this.#tableData
  }

  get levelData() {
    return this.#levelPeiData
  }

  get eventType() {
    return this.#eventTypePeiData
  }

  get statusData() {
    return this.#statusPeiData
  }

}

export { FetchData, HandleEventTrackingInfoData }
