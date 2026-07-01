/******* 
 * @description: Canvas
 * @return {*}
 */
import { eventIndexEnum } from '@/util/constant'
export default class CesuimCanvas {
  constructor(Cesium, viewer) {
    this.Cesium = Cesium
    this.viewer = viewer
  }

  /******* 
   * @description: 事件图标（包含等级、处理、事件等）
   * @param {*} eventTypeNum 事件类型（小）
   * @param {*} eventLevel 事件等级
   * @param {*} newInfoNum 消息数量
   * @param {*} pintBg 小绿点
   * @param {*} iconType 事件类型（大）
   * @return {*}
   */  
  handleEventIconImg(eventTypeNum, eventLevel, newInfoNum, pintBg = 1, iconType = 'eventType') {
    const typeStyle = {
      'eventType': {
        width: 6,
        height: 3.5
      },
      'eventStatus': {
        width: 4,
        height: 4
      },
      'eventLevel1': {
        width: 2.9,
        height: 3.2
      },
      'eventLevel2': {
        width: 3.8,
        height: 3.2
      },
      'eventLevel3': {
        width: 5,
        height: 3.2
      },
      'eventLevel4': {
        width: 3.9,
        height: 2.8
      }
    }
    const typeEventIndex = {
      health: 1,
      inspection: 3,
      work: 4
    }
    let iconPosition = typeStyle[iconType === 'eventType' ? iconType : `${iconType}${eventIndexEnum[eventLevel]}`]
    const GREATER = newInfoNum > 99
    const canvas = document.createElement('canvas')
    const ctx = canvas.getContext('2d')
    canvas.width = 150
    canvas.height = 180
    let image = new Image()
    ctx.fillStyle = 'rgba(255, 255, 255, 0)'
    let imgObj1 = new Image()
    return new Promise((resolve, reject) => {
      try {
        imgObj1.src = `/img/screen/eventIconBg/4x${eventIndexEnum[eventLevel]}.png`
        imgObj1.onload = () => {
          ctx.drawImage(imgObj1, 2, 8)
          let imgObj2 = new Image()
          imgObj2.src = `/img/screen/pintBg/4x${pintBg}.png`
          imgObj2.onload = () => {
            ctx.drawImage(imgObj2, canvas.width/1.6 - 2, 2)
            ctx.font = `bold ${ GREATER ? '25' : '33' }px Microsoft YaHei`
            ctx.fillStyle = '#fff'
            ctx.textAlign = 'center'
            ctx.textBaseline = 'middle'
            1.22
            ctx.fillText(`${ GREATER ? '99+' : newInfoNum }`, canvas.width/( GREATER ? 1.22 : 1.25 ), canvas.height / 5.5)
            if (iconType === 'eventLevel') {
              ctx.font = 'bold 90px Microsoft YaHei'
              ctx.fillStyle  = 'rgba(0, 0, 0, 0.8)'
              ctx.textAlign = 'center'
              ctx.textBaseline = 'middle'
              ctx.fillText(eventIndexEnum[eventLevel], canvas.width / 2.2, canvas.height / 1.8)
              image.src = canvas.toDataURL('image/jpg')
              resolve(image)
            } else {
              let imgObj3 = new Image()
              imgObj3.src = `/img/screen/${iconType}/4x${ typeEventIndex[eventTypeNum] }.png`
              imgObj3.onload = () => {
                ctx.drawImage(
                  imgObj3,
                  canvas.width / iconPosition.width,
                  canvas.height / iconPosition.height
                )
                image.src = canvas.toDataURL('image/jpg')
                resolve(image)
              }
            }
          }
        }
      } catch (error) {
        reject(error)
      }
    })
  }
}
