import {
  savePicture,
  outputXlsxFile,
  copyPicture,
  getNewArr,
  getTargetNode
} from '../tool.js'
import { getOptions } from '../../commonJs/optionConfig.js'
export class Toolbox {
  constructor(myChart, theme, optionConfig, hotUpdate, changeStackedType) {
    this.optionConfig = optionConfig ? {
      isYB: false, // 是否显示游标
      isZoom: true, // 是否显示放大类型图标
      isZoomY: true,// 是否支持Y轴放大
      isSetX: false,// 是否支持X轴最大最小值设置
      isSetY: true,// 是否支持Y轴最大最小值设置
      isAddNote: true,// 是否支持添加备注
      isDataDown: true,// 是否支持资源下载
      isFilterWavePointer: false,// 是否支持波形点过滤
      isStacked: false,// 是否支持layout
      isMarkLine: false,// 是否支持添加备注
      ...optionConfig
    } : {
      isYB: false, // 游标是否显示
      isSetX: false, // 可单独设置X轴值
      isSetY: true, // 可单独设置X轴值
      isMarkLine: false,// 是否显示markline
      isStacked: false, // 是否显示堆叠方式
      isFilterWavePointer: false, // 是否过滤波形点
      isZoom: true, // 是否显示放大方式
      isZoomY: true,
      isAddNote: true,
      isDataDown: true,
      YBList: {// 游标配置
        isSingle: false,// 单游标
        isShuang: false,// 双游标
        isBei: false,// 倍频游标
        isBian: false,// 边频游标
        isGap: false,// 间隔游标
        isPeak: false,// 峰值
      },

    }
    //  this.myChart = myChart // chart实例
    this.myChartId = myChart.id
    this.myChartDom = myChart.getDom()
    this.themeType = theme // 主题
    this.stackedType = 'overlay'// 堆叠方式
    this.marklineType = '' // 标记线类型
    // this.toolboxDom = null
    this.noteDom = null // 创建备注的弹出框DOM
    this.menuDom = null// 鼠标右键弹出框信息DOM
    this.YBDom = null // 鼠标划上游标线显示的辅助信息DOM
    this.YBDataDom = null // 所有游标数据展示DOM
    this.YBDetailIcon = null // 游标详情图标DOM
    this.limitDom = null // 设置XY轴最大值最小值弹窗DOM
    this.dataZoomType = '' // 设置放大方式
    this.rightClickedNoteId = ''// 右键菜单选中的备注Id
    this.markindex = 0 // 备注增加，索引标记
    this.hotUpdate = hotUpdate // 更改图谱实例某些属性的方法
    this.changeStackedType = changeStackedType || null // 更改堆叠方式的方法
    this.init(myChart)
  }

  // 初始化
  init(echart) {
    this.creatToolBox(echart)
    this.createdMenuContent(echart)
    // this.createdInput(echart)
    if (this.optionConfig.isSetX || this.optionConfig.isSetY) {
      this.createdSetValue(echart)
    }
    if (this.optionConfig.isYB) {
      this.createdYBdiag(echart)
      this.createdYBInfoContent(echart)
    }
    /*  echart.getZr().off('contextmenu')
     echart.getZr().on('contextmenu', (param) => {
       this.setStyleMenuContent(param, echart)
     }) */
    echart.off('contextmenu')
    echart.on('contextmenu', (param) => {
      this.setStyleMenuContent(param, echart)
    })
  }
  /**
   * 
   * @param {*} chart echarts实例
   * @param {*} clickedEvent 点击分发事件
   * @param {*} themeName 主题名称
   * @param {*} datazoomType 放大方式
   */
  creatToolBox(echart) {
    let dom = document.getElementById(this.myChartId + 'toolbox')
    if (dom) {
      this.myChartDom.removeChild(dom)
      dom = null
    }
    // } else {
    let dvBlock = document.createElement('ul')
    dvBlock.setAttribute('id', this.myChartId + 'toolbox')
    dvBlock.classList.add('chart_toolbox');
    dvBlock.classList.add('chart_toolbox_' + this.themeType);
    let htmlStr = ` 
       <li title="放大方式" class='selectLi ${this.optionConfig.isZoom ? '' : 'hideLi'}' style="position:relative">
        <i class="icon iconfont acharts-datazoom"></i>
        <div class="diagmodel" style="position:absolute">
          <p class='radioStyle ${this.optionConfig.isZoom ? '' : 'hideLi'}' title="x轴放大"> <i class="icon iconfont acharts-xzoom"></i>X轴放大</p>
          <p class='radioStyle ${this.optionConfig.isZoom && this.optionConfig.isZoomY ? '' : 'hideLi'}' title="y轴放大"><i class="icon iconfont acharts-yzoom"></i>Y轴放大</p>
          <p class='radioStyle ${this.optionConfig.isZoom && this.optionConfig.isZoomY ? '' : 'hideLi'}' title="xy轴放大"><i class="icon iconfont acharts-xyzoom"></i>XY轴放大</p>
        </div>
      </li>
        <li class='back ${this.optionConfig.isZoom ? '' : 'hideLi'}' title="缩放回退" ><i class="icon iconfont acharts-resetzoom"></i></li>
        <li class='${this.optionConfig.isZoom /* && this.optionConfig.isYB  */ ? '' : 'hideLi'}' title="缩放还原" ><i class="icon iconfont acharts-originalsize"></i></li>
       <span class='gap ${this.optionConfig.isZoom ? '' : 'hideLi'}'></span>
       <li title="游标工具" class='selectLi ${this.optionConfig.isYB ? '' : 'hideLi'}' style="position:relative"><i class="icon iconfont acharts-ybtool"></i>
        <div class="diagmodel" style="position:absolute">
          <p class='YB ${this.optionConfig.isYB && this.optionConfig.YBList.isSingle ? '' : 'hideLi'}' title="单游标"><i class="icon iconfont acharts-single"></i>单游标</p>
          <p class='YB ${this.optionConfig.isYB && this.optionConfig.YBList.isShuang ? '' : 'hideLi'}' title="双游标"><i class="icon iconfont acharts-double"></i>双游标</p>
          <p class='YB ${this.optionConfig.isYB && this.optionConfig.YBList.isBei ? '' : 'hideLi'}' title="倍频游标"><i class="icon iconfont acharts-beipin"></i>倍频游标</p>
          <p class='YB ${this.optionConfig.isYB && this.optionConfig.YBList.isBian ? '' : 'hideLi'}' title="边频游标"><i class="icon iconfont acharts-bianpin"></i>边频游标</p>
          <p class='YB ${this.optionConfig.isYB && this.optionConfig.YBList.isGap ? '' : 'hideLi'}' title="间隔游标"><i class="icon iconfont acharts-gap"></i>间隔游标</p>
          <p class='YB ${this.optionConfig.isYB && this.optionConfig.YBList.isPeak ? '' : 'hideLi'}' title="峰值"><i class="icon iconfont acharts-peak"></i>峰值</p>
        </div>
      </li>
      <li class='selectLi ${this.optionConfig.isYB ? '' : 'hideLi'}' title="游标设置" ><i class="icon iconfont acharts-ybset"></i>
       <div class="diagmodel diagContent" style="position:absolute">
          <h3>游标显示设置</h3>
          <p class='${this.optionConfig.isYB && this.optionConfig.YBList.isBei ? '' : 'hideLi'}'>倍频游标倍数：<input value="5" min="1" step="1" name="beipin" type="number"></input></p>
          <p class='${this.optionConfig.isYB && this.optionConfig.YBList.isGap ? '' : 'hideLi'}'>间隔游标倍数：<input value="5" min="1" step="1" name="jiange" type="number"></input></p>
          <p class='${this.optionConfig.isYB && this.optionConfig.YBList.isBian ? '' : 'hideLi'}'>边频游标倍数：<input value="3" min="1" step="1" name="bianpin" type="number"></input>
          <div><button class="ybNumReset">重置</button><button class="ybNumConfirm">确定</button></div>
          </div>
      </li>
      <li class='${this.optionConfig.isYB ? '' : 'hideLi'}' title="清空游标" ><i class="icon iconfont acharts-clearyb"></i></li>
       <span class='gap  ${this.optionConfig.isYB ? '' : 'hideLi'}'></span>
     <li title="堆叠方式" class="selectLi  ${this.optionConfig.isStacked ? '' : 'hideLi'}" style="position:relative"><i class="icon iconfont acharts-stacked"></i>
        <div class="diagmodel" style="position:absolute">
         <p class='radioStyle activeStack' title="overlay"><i class="icon iconfont acharts-byoverlay"></i>叠加</p>
         <p class='radioStyle' title="stacked"><i class="icon iconfont acharts-bystacked"></i>堆叠</p>
         <p class='radioStyle ${!this.optionConfig.isYB ? '' : 'hideLi'}' title="group"><i class="icon iconfont acharts-byunit"></i>以单位分组</p>
        </div>
      </li>
       <li title="标记线" class='selectLi ${this.optionConfig.isMarkLine ? '' : 'hideLi'}' style="position:relative"><i class="icon iconfont acharts-markline"></i>
        <div class="diagmodel" style="position:absolute">
          <p title="markline_min"><i class="icon iconfont acharts-min"></i>最小值</p>
          <p title="markline_max"><i class="icon iconfont acharts-max"></i>最大值</p>
          <p title="markline_avg"><i class="icon iconfont acharts-avg"></i>平均值</p>
          <p title="markline_mid"><i class="icon iconfont acharts-mid"></i>中位数</p>
          <p title="markline_VDI"><i class="icon iconfont acharts-vdi"></i>报警线</p>
        </div>
      </li>
      <li class="${this.optionConfig.isFilterWavePointer ? 'filterWaveLi activedLi' : 'hideLi'}" title="波形点"><i class="icon iconfont acharts-filter"></i></li>
      <li class="setValueLi ${this.optionConfig.isSetX || this.optionConfig.isSetY ? '' : 'hideLi'}" title="设置区间" class=""><i class="icon iconfont acharts-setvalue"></i></li>
       <span class='gap ${this.optionConfig.isSetX || this.optionConfig.isSetY ? '' : 'hideLi'}'></span>
      <li class='addNote ${this.optionConfig.isAddNote ? '' : 'hideLi'}' title="添加备注"><i class="icon iconfont acharts-addnote"></i></li>
      <li class='${this.optionConfig.isAddNote ? '' : 'hideLi'}' title="清空备注"><i class="icon iconfont acharts-clearnote"></i></li>
       <span class='gap ${this.optionConfig.isAddNote ? '' : 'hideLi'}'></span>
       <li title="数据处理" class='selectLi' style="position:relative"><i class="icon iconfont acharts-download"></i>
        <div class="diagmodel" style="position:absolute">
          <p title="复制图片"><i class="icon iconfont acharts-copy"></i>图片复制</p>
          <p title="保存图片"><i class="icon iconfont acharts-downpic"></i>图片下载</p>
          <p title="数据下载" class="${this.optionConfig.isDataDown ? '' : 'hideLi'}"><i class="icon iconfont acharts-datadown"></i>数据下载</p>
          </div>
      </li>
      <li title="重置" class="biggap"><i class="icon iconfont acharts-reset"></i></li>`
    dvBlock.insertAdjacentHTML('afterbegin', htmlStr)
    this.myChartDom.appendChild(dvBlock)
    dvBlock.style.left = (echart.getWidth() - dvBlock.getBoundingClientRect().width) / 2 + 'px'
    // this.toolboxDom = dvBlock
    // 监听点击事件
    let that = this
    dvBlock.onclick = function (e) {
      let liDom = null
      if (e.target.localName == 'i') {
        liDom = e.target.parentNode
      } else if (e.target.localName == 'li') {
        liDom = e.target
      } else if (e.target.localName == 'button') {
        // 游标线数量设置 
        let diagDom = e.target.parentNode.parentNode
        let input = diagDom.getElementsByTagName('input')
        if (e.target.className == 'ybNumConfirm') {
          let obj = {}
          if (input.length) {
            for (let i = 0; i < input.length; i++) {
              if (input[i].name == 'bianpin' || input[i].name == 'beipin' || input[i].name == 'jiange') {
                obj[input[i].name] = input[i].value
              }
            }
          }
          /*    input.length ? input.forEach(i => {
               if (i.name == 'bianpin' || i.name == 'beipin' || i.name == 'jiange') {
                 obj[i.name] = i.value
               }
             }) : null */
          diagDom.style.display = "none"
          return that.optionConfig.YBEvent('setYbNum', obj)
        } else {
          if (input.length) {
            for (let i = 0; i < input.length; i++) {
              if (input[i].name == 'bianpin') {
                input[i].value = 7
              } else if (input[i].name == 'beipin' || input[i].name == 'jiange') {
                input[i].value = 5
              }
            }
          }
          /*  input.length ? input.forEach(i => {
             if (i.name == 'bianpin') {
               i.value = 7
             } else if (i.name == 'beipin' || i.name == 'jiange') {
               i.value = 5
             }
           }) : null */
          diagDom.style.display = "none"
          return that.optionConfig.YBEvent('resetYbNum', {})
        }
      } else if (e.target.localName == 'p') {
        liDom = e.target
        let modelDom = liDom.parentNode
        modelDom.style.display = 'none'
      } else {
        return
      }
      let title = liDom.title
      // 使用echarts自带的restore方法，会把选中的放大方式的手型重置掉，故也同步需要重置放大方式
      /*   if (title == '重置') {
          for (let i = 0; i < nodes.length; i++) {
            nodes[i].classList.remove('activeRadio')
          }
        } else */
      if (liDom.classList.contains('selectLi')) {
        let DOM = liDom.getElementsByClassName('diagmodel')[0]
        DOM.style.display = 'block'
        document.addEventListener('mouseup', (e) => {
          that.mouseupEvent(e, DOM)
        })
        // }
      }
      that.toolboxClickedEvent(title, liDom, echart)
      return false
    }
    dvBlock = null
  }
  // 关闭菜单
  closeMenu() {
    this.menuDom.style.display = 'none'
    document.body.removeEventListener('click', () => { this.closeMenu() })
  }


  /**
   * 
   * @param {*} chart echarts 实例
   * @param {*} arr 放大操作数据存储
   * @param {*} theme 主题色
   */
  updateBackDomStyle(arr) {
    let dom = document.getElementById(this.myChartId + 'toolbox').getElementsByClassName('back')[0]
    if (arr.length == 0) {
      dom.classList.remove('activedLi')
    } else {
      dom.classList.add('activedLi')
    }
  }
  /**
   * @description  点击事件，对应按钮事件分派
   * @param {*} title 点击的按钮title
   * @param {*} liDom 点击的按钮DOM, 进行样式修改
   */
  toolboxClickedEvent(title, liDom, echart) {
    if (title == '添加备注') {
      this.createdNote(echart)
    } else if (title == '清空备注') {
      this.deleteAllRemarks(echart)
    } else if (title == 'x轴放大') {
      this.changeZoomType('X', liDom, echart)
    } else if (title == 'y轴放大') {
      this.changeZoomType('Y', liDom, echart)
    } else if (title == 'xy轴放大') {
      this.changeZoomType('XY', liDom, echart)
    } else if (title == 'overlay' || title == 'stacked' || title == 'group') {
      if (this.stackedType == title) { return }
      this.stackedType = title
      let nodes = document.getElementById(this.myChartId + 'toolbox').getElementsByClassName('activeStack')
      nodes?.length && nodes[0].classList.remove('activeStack')
      liDom.classList.add('activeStack')
      let relationOp = this.changeStackedType(this.stackedType, this.themeType)
      echart.setOption({
        ...echart.getOption(),
        ...relationOp
      }, true)
    } else if (title == '缩放回退') {
      this.goBack(echart)
    } else if (title == '缩放还原') {
      this.goBack(echart, true)
    } else if (title == '复制图片') {
      // return alert('此功能暂不支持！')
      /*  const { selectedLegend } = this.hotUpdate()
       copyPicture(echart, selectedLegend) */
      const { selectedLegend, echartData } = this.hotUpdate()
      savePicture(echart, echartData.titleText, selectedLegend, true)
    } else if (title == '保存图片') {
      const { selectedLegend, echartData } = this.hotUpdate()
      savePicture(echart, echartData.titleText, selectedLegend);
    } else if (title == '波形点') {
      if (liDom.classList.contains('activedLi')) {
        liDom.classList.remove('activedLi')
        this.optionConfig.showWavePointer(false)
      } else {
        liDom.classList.add('activedLi')
        this.optionConfig.showWavePointer(true)
      }
    } else if (title == '设置区间') {
      this.openSetValueDiag(echart)
    } else if (title == '数据下载') {
      this.downLoadData(echart)
    } else if (title == '重置') {
      if (this.YBDataDom != null) {
        this.YBDataDom.innerHTML = ''
        this.YBDataDom.classList.add('hideVisibel')
        this.YBDetailIcon.classList.add('hideVisibel')
      }
      if (document.getElementById(this.myChartId + 'toolbox').getElementsByClassName('filterWaveLi').length) {
        document.getElementById(this.myChartId + 'toolbox').getElementsByClassName('filterWaveLi')[0].classList.remove('activedLi')
      }
      // 清除选中样式
      /*  let nodes = this.toolboxDom.getElementsByClassName('activeRadio')
       nodes?.length && nodes[0].classList.remove('activeRadio') */
      // 重置缩放状态
      if (this.dataZoomType.length) {
        let nodes = document.getElementById(this.myChartId + 'toolbox').getElementsByClassName('activeRadio')
        nodes?.length && nodes[0].classList.remove('activeRadio')
        this.dataZoomType = ''
        echart.dispatchAction({
          type: 'takeGlobalCursor',
          key: 'dataZoomSelect',
          dataZoomSelectActive: false
        });
      }
      this.updateBackDomStyle([])
      let { title, dataZoom } = echart.getOption()
      if (this.optionConfig.isYB) {
        this.hotUpdate({ dataZoomDataList: [] }, [{ start: 0, end: 100 }])
      } else {
        this.hotUpdate({ dataZoomDataList: [] })
        echart.setOption({
          dataZoom: dataZoom.map(item => {
            return { ...item, start: 0, end: 100 }
          })
        })
      }

      if (title[0].text.indexOf('瀑布图') > -1) {
        echart.dispatchAction({
          type: 'restore'
        })
        return
      }
    } else if (title.indexOf('游标') > -1 || title.indexOf('峰值') > -1) {
      if (title == '游标设置') {
        this.setYBlineNum()
      } else {
        this.optionConfig.YBEvent(title)
      }

    } else if (title.indexOf('markline') > -1) {
      let arr = title.split('_')
      // if (this.marklineType == arr[1]) { return }
      this.marklineType = arr[1]
      this.optionConfig.operaterMarkline(this.marklineType)
    } else {
      return
    }
  }
  // 设置游标数量
  setYBlineNum() {

  }

  // 创建备注输入框dom
  createdInput(echart) {
    let dom = document.getElementById(this.myChartId + 'echart_note_input_block')
    if (dom) {
      this.noteDom = dom
    } else {
      let dvBlock = document.createElement('div')
      dvBlock.setAttribute('id', this.myChartId + 'echart_note_input_block')
      dvBlock.classList.add('echart_note_input_block');
      dvBlock.classList.add('echart_note_input_block_' + this.themeType);
      let htmlStr = `<textarea placeholder="请输入备注"></textarea><p>
      <button class="cancel">取消</button>
      <button class="confirm">确定</button></p> `
      dvBlock.insertAdjacentHTML('afterbegin', htmlStr)
      this.myChartDom.appendChild(dvBlock)
      this.noteDom = dvBlock
      dvBlock = null
    }
    let that = this
    this.noteDom.onclick = function (e) {
      that.inputEvent.call(that, echart, e)
      return false;
    }
  }
  // 点击备注调用事件
  createdNote(echart) {
    if (!this.noteDom) {
      this.createdInput(echart)
    }
    this.noteDom.style.display = 'block'
    const { width } = this.myChartDom.getBoundingClientRect()
    this.noteDom.style.top = '50px' //this.menuDom.style.top
    this.noteDom.style.left = width / 2 - 100 + 'px'//(noteWidth + 200 > width ? width - 200 : noteWidth) + 'px'
    document.addEventListener('mouseup', (e) => {
      this.mouseupEvent(e, this.noteDom)
    })
  }
  // 确定提交事件
  inputEvent(echart, e) {
    let className = e.target.className
    if (className == 'confirm') {// 添加备注
      let text = this.noteDom.firstChild.value
      if (text) {
        // this.addRemarks(text, [Number(this.noteDom.style.left.slice(0, -2)) - left, Number(this.noteDom.style.top.slice(0, -2)) - top])
        this.addRemarks(text, [Number(this.noteDom.style.left.slice(0, -2)), Number(this.noteDom.style.top.slice(0, -2))], echart)
        this.noteDom.style.display = 'none'
      }
    } else if (className == 'cancel') {
      this.noteDom.style.display = 'none'
    }
  }

  // 打开设置坐标轴数值弹窗
  openSetValueDiag(echart) {
    const { width } = this.myChartDom.getBoundingClientRect()
    this.limitDom.style.cssText = `display:block;top:50px;left:${width / 2 - 100}px`
    document.addEventListener('mouseup', (e) => {
      this.mouseupEvent(e, this.limitDom)
    })
    // 获取坐标轴最大最小值，对输入框设定默认值
    // 多Y轴多X轴
    let { yAxis, xAxis } = echart.getOption()
    let yMin = '', yMax = '', xMin = '', xMax = '';
    let obj = echart.getModel()
    if (this.optionConfig.isSetY) {
      yAxis.forEach((item, index) => {
        let arr = obj.getComponent("yAxis", index).axis.scale._extent
        yMin = yMin === '' ? arr[0] : Math.min(arr[0], yMin)
        yMax = yMax === '' ? arr[1] : Math.max(arr[1], yMax)
      })
    }
    if (this.optionConfig.isSetX) {
      xAxis.forEach((item, index) => {
        let arr = obj.getComponent("xAxis", index).axis.scale._extent
        xMin = xMin == '' ? arr[0] : Math.min(arr[0], xMin)
        xMax = xMax == '' ? arr[1] : Math.max(arr[1], xMax)
      })
    }
    let nodes = this.limitDom.getElementsByTagName('input')
    for (let i = 0; i < nodes.length; i++) {
      if (nodes[i].type == 'number' /* && nodes[i].disabled */) {
        let name = nodes[i].name
        if (name.indexOf('ymin') > -1) {
          nodes[i].value = yMin;
        } else if (name.indexOf('ymax') > -1) {
          nodes[i].value = yMax;
        } else if (name.indexOf('xmin') > -1) {
          nodes[i].value = xMin;
        } else if (name.indexOf('xmax') > -1) {
          nodes[i].value = xMax;
        }
      }
    }
  }

  // 释放按下的鼠标键，达到鼠标点击其他区域后弹窗关闭的效果
  mouseupEvent(e, dom) {
    if (!getTargetNode(e.target, dom)) {
      dom.style.display = 'none'
      document.removeEventListener('mouseup', (e) => {
        this.mouseupEvent(e, dom)
      })
    }
  }
  // 创建弹窗，显示Y值大小设置
  createdSetValue(echart) {
    let dom = document.getElementById(this.myChartId + 'echart_setvalue')
    if (dom) {
      this.limitDom = dom
    } else {
      let dvBlock = document.createElement('div')
      dvBlock.setAttribute('id', this.myChartId + 'echart_setvalue')
      dvBlock.classList.add('echart_setvalue');
      dvBlock.classList.add('echart_setvalue_' + this.themeType);
      dvBlock.style.background = '#fff'// this.themeType == 'light' ? '#fff' : '#000'
      let htmlStr = `
       <div class="${this.optionConfig.isSetX ? 'setItem' : 'hideLi'}">
      <h3>X轴设置：</h3>
       <div style="width:100%;text-align:left;font-size:14px"><input type="checkbox" value="xautoScale" checked style="margin-right:10px;"/>默认</div>
       <p><span>最小值：</span><input type="number" disabled @change="changevalue" name="${this.myChartId}xmin" placeholder="请输入数值"/></p>
       <p><span>最大值：</span><input type="number" disabled name="${this.myChartId}xmax" placeholder="请输入数值"/></p>
      </div>
       <div class="${this.optionConfig.isSetY ? 'setItem' : 'hideLi'}" style="margin-left:10px;">
      <h3>Y轴设置：</h3>
       <div style="width:100%;text-align:left;font-size:14px"><input type="checkbox" value="yautoScale" checked style="margin-right:10px;"/>默认</div>
       <p><span>最小值：</span><input type="number" disabled name="${this.myChartId}ymin" placeholder="请输入数值"/></p>
       <p><span>最大值：</span><input type="number" disabled name="${this.myChartId}ymax" placeholder="请输入数值"/></p>
      </div>
       <div class="footer"><button class="cancel">取消</button>
        <button class="confirm">确定</button></div>`
      dvBlock.insertAdjacentHTML('afterbegin', htmlStr)
      this.myChartDom.appendChild(dvBlock)
      this.limitDom = dvBlock
      dvBlock = null
    }
    let that = this
    this.limitDom.onclick = function (e) {
      that.setSubmit.call(that, echart, e)
    }

  }

  // 点击提交或者取消事件
  setSubmit(echart, e) {
    let className = e.target.className
    let nodes = this.limitDom.getElementsByTagName('input')
    let isSet = false
    if (className == 'confirm') {
      let ymin = null, ymax = null, xmin = null, xmax = null
      for (let i = 0; i < nodes.length; i++) {
        if (nodes[i].type == 'number' && !nodes[i].disabled) {
          isSet = true
          let value = nodes[i].value
          let name = nodes[i].name
          if (value || value == 0) {
            if (name.indexOf('ymin') > -1) {
              ymin = value;
            } else if (name.indexOf('ymax') > -1) {
              ymax = value;
            } else if (name.indexOf('xmin') > -1) {
              xmin = value;
            } else if (name.indexOf('xmax') > -1) {
              xmax = value;
            }
          } else {
            return alert('输入框不能为空！')
          }
        } else {
          if (nodes[i].checked) {
            this.setAxisValue(echart, 'auto', nodes[i].value)
            document.getElementById(this.myChartId + 'toolbox').getElementsByClassName('setValueLi')[0].classList.remove('activedLi')
            this.limitDom.style.display = 'none'
          }
        }
      }
      if (isSet) {
        this.setAxisValue(echart, ymin, ymax, xmin, xmax)
        document.getElementById(this.myChartId + 'toolbox').getElementsByClassName('setValueLi')[0].classList.add('activedLi')
        this.limitDom.style.display = 'none'
      }
    } else if (className == 'cancel') {
      this.limitDom.style.display = 'none'
    } else if (e.target.value == 'xautoScale') {
      if (e.target.checked) {
        for (let i = 0; i < nodes.length; i++) {
          if (nodes[i].type == 'number' && (nodes[i].name.indexOf('xmin') > -1 || nodes[i].name.indexOf('xmax') > -1)) {
            nodes[i].disabled = true
          }
        }
      } else {
        for (let i = 0; i < nodes.length; i++) {
          if (nodes[i].type == 'number' && (nodes[i].name.indexOf('xmin') > -1 || nodes[i].name.indexOf('xmax') > -1)) {
            nodes[i].disabled = false
          }
        }
      }
    } else if (e.target.value == 'yautoScale') {
      if (e.target.checked) {
        for (let i = 0; i < nodes.length; i++) {
          if (nodes[i].type == 'number' && (nodes[i].name.indexOf('ymin') > -1 || nodes[i].name.indexOf('ymax') > -1)) {
            nodes[i].disabled = true
          }
        }
      } else {
        for (let i = 0; i < nodes.length; i++) {
          if (nodes[i].type == 'number' && (nodes[i].name.indexOf('ymin') > -1 || nodes[i].name.indexOf('ymax') > -1)) {
            nodes[i].disabled = false
          }
        }
      }
    }
  }

  //设置坐标轴最大最小值
  setAxisValue(echart, ymin, ymax, xmin, xmax) {
    const { xAxis, yAxis } = echart.getOption()
    let { dataZoomDataList } = this.hotUpdate()
    let newYAxis = []
    let newXAxis = []
    if (ymin == 'auto') {
      if (ymax == 'yautoScale') {
        yAxis.forEach(item => {
          newYAxis.push({
            ...item,
            min: null,
            max: null
          })
        })
      } else {
        if (dataZoomDataList.length == 0) {
          return;
        }
        this.updateBackDomStyle([])
        let { title, dataZoom } = echart.getOption()
        if (this.optionConfig.isYB) {
          this.hotUpdate({ dataZoomDataList: [] }, [{ start: 0, end: 100 }])
        } else {
          this.hotUpdate({ dataZoomDataList: [] })
          echart.setOption({
            dataZoom: dataZoom.map(item => {
              return { ...item, start: 0, end: 100 }
            })
          })
        }
        /* xAxis.forEach(item => {
          newXAxis.push({
            ...item,
            min: null,
            max: null
          })
        }) */
      }
    } else {
      if (ymin !== null && ymax !== null) {
        if (parseFloat(ymin) > parseFloat(ymax)) {
          return alert('最小值不能大于最大值！')
        }
        yAxis.forEach(item => {
          newYAxis.push({
            ...item,
            min: parseFloat(ymin),
            max: parseFloat(ymax)
          })
        })
      } else if (xmin !== null && xmax !== null) {
        if (parseFloat(xmin) > parseFloat(xmax)) {
          return alert('最小值不能大于最大值！')
        }
        if (dataZoomDataList.length) {
          let { startValue, endValue } = dataZoomDataList[dataZoomDataList.length - 1]
          if (startValue == xmin && endValue == xmax) {
            return
          }
        }
        let datazoom = [{
          startValue: parseFloat(xmin),
          endValue: parseFloat(xmax)
        }]
        this.hotUpdate({
          dataZoomDataList: [...dataZoomDataList, datazoom]
        }, datazoom)
        this.updateBackDomStyle(datazoom)
        /* xAxis.forEach(item => {
          newXAxis.push({
            ...item,
            min: parseFloat(xmin),
            max: parseFloat(xmax)
          })
        }) */
      }
    }
    echart.setOption({
      //  ...options,
      // xAxis: newXAxis.length ? newXAxis : xAxis,
      yAxis: newYAxis.length ? newYAxis : yAxis
    })
  }
  // 创建弹框，显示游标线的信息
  createdYBdiag() {
    let dom = document.getElementById(this.myChartId + 'YBBlock')
    if (dom) {
      this.YBDom = dom
      dom = null
    } else {
      let dvBlock = document.createElement('div')
      dvBlock.setAttribute('id', this.myChartId + 'YBBlock')
      dvBlock.classList.add('chart_YBBlock');
      dvBlock.classList.add('chart_YBBlock_' + this.themeType)
      this.YBDom = dvBlock
      this.myChartDom.appendChild(dvBlock)
      dvBlock = null
    }
  }
  // 创建游标信息展示窗口
  createdYBInfoContent() {
    let dom = document.getElementById(this.myChartId + 'YBDataBlock')
    let detailIcon = document.getElementById(this.myChartId + 'YBDataMark')
    if (dom) {
      this.YBDataDom = dom
      this.YBDetailIcon = detailIcon
    } else {
      let dvBlock = document.createElement('div')
      dvBlock.setAttribute('id', this.myChartId + 'YBDataBlock')
      dvBlock.classList.add('chart_YBDataBlock');
      dvBlock.classList.add('hideVisibel');
      dvBlock.classList.add('chart_YBDataBlock_' + this.themeType);
      this.YBDataDom = dvBlock
      this.myChartDom.appendChild(dvBlock)
      dvBlock = null
      let lock = document.createElement('div')
      lock.setAttribute('id', this.myChartId + 'YBDataMark')
      lock.setAttribute('class', 'chart_YBDataMark hideVisibel')
      lock.innerHTML = `<i class="icon iconfont acharts-detail"></i>`
      this.YBDetailIcon = lock
      this.myChartDom.appendChild(lock)
      lock = null
    }
    this.YBDetailIcon.addEventListener('click', e => {
      this.YBDataDom.classList.remove('hideVisibel')
      this.YBDetailIcon.classList.add('hideVisibel')
    })
  }
  // 根据游标信息实时更新显示内容
  YBDomContent(YBArray, seriesNameList) {
    this.YBDataDom.innerHTML = ''
    if (YBArray.length && this.YBDataDom.classList.contains('hideVisibel')) {
      this.YBDetailIcon.classList.remove('hideVisibel')
    } else if (YBArray.length == 0) {
      this.YBDetailIcon.classList.add('hideVisibel')
      this.YBDataDom.classList.add('hideVisibel')
      return
    }
    let htmlStr = '<div class="closeYBInfo">X</div><div style="width:100%;height:auto">'
    seriesNameList.length && seriesNameList.forEach(item => {
      if (YBArray.find(i => i.seriesName == item)) {
        let { seriesColor, children } = YBArray.find(i => i.seriesName == item)
        htmlStr += `<div class="collapse_block"><h2 class="collapse_title" title=${item}><span style="background:${seriesColor}"></span>${item}</h2>`
        let childStr = '<div>'
        Object.keys(children).forEach(ele => {
          if (children[ele].length) {
            let title = ''
            if (ele.includes('shuang')) {
              title = '双游标'
            } else if (ele.includes('single')) {
              title = '单游标'
            } else if (ele.includes('jiange')) {
              title = '间隔游标'
            } else if (ele.includes('bei')) {
              title = '倍频游标'
            } else if (ele.includes('bian')) {
              title = '边频游标'
            }
            childStr += `<div><h4 class='collapse_YB_title'>${title}</h4><div>`
            children[ele].forEach(child => {
              const { lineColor, name, dataList } = child
              dataList.forEach((data, index) => {
                childStr += `<p class="dataline" style='color:${lineColor}'><span title='${data?.length ? data[0] : 'null'}'>${data?.length ? data[0] : 'null'}</span><span title='${data?.length ? data[1] : 'null'}'>${data?.length ? data[1] : 'null'}</span></p>`
              })
            })
            childStr += '</div></div>'
          }
        })
        childStr += '</div></div>'
        htmlStr += childStr
      }
    })
    htmlStr += '</div >'
    // this.YBDataDom.insertAdjacentHTML('afterbegin', htmlStr)
    this.YBDataDom.innerHTML = htmlStr
    let that = this
    this.YBDataDom.onclick = function (e) {
      if (e.target.className == 'collapse_title' || e.target.className == 'collapse_YB_title') {
        let nextSibling = e.target.nextSibling
        if (nextSibling.style.display == '' || nextSibling.style.display == 'block') {
          nextSibling.style.display = 'none'
        } else {
          nextSibling.style.display = 'block'
        }
      } else if (e.target.className == 'closeYBInfo') {
        that.YBDataDom.classList.add('hideVisibel')
        that.YBDetailIcon.classList.remove('hideVisibel')
      }
      return false
    }
  }
  // 创建右键菜单
  createdMenuContent(echart) {
    let dom = document.getElementById(this.myChartId + 'echart_context_menu_block')
    if (dom) {
      this.menuDom = dom
    } else {
      let dvBlock = document.createElement('ul')
      dvBlock.setAttribute('id', this.myChartId + 'echart_context_menu_block')
      dvBlock.classList.add('echart_context_menu_block');
      dvBlock.classList.add('echart_context_menu_block_' + this.themeType);
      let htmlStr = `<li title="delete" class='deleteNote'>删除此条备注</li>
          <li class='deleteGroup' title="delete" style="display:none"> 删除此组游标</li>
          <li class='changeLineNum' title="change" style="display:none"> 修改游标线数量</li>`
      dvBlock.insertAdjacentHTML('afterbegin', htmlStr)
      this.menuDom = dvBlock
      this.myChartDom.appendChild(dvBlock)
      dvBlock = null
    }
    let that = this
    this.menuDom.onclick = function (e) {
      that.menuEvent(echart, e)
      return false
    }
  }
  // 右侧菜单点击事件
  menuEvent(echart, e) {
    let className = e.target.className
    if (className == 'deleteGroup') { // 删除一组游标线
      this.optionConfig.YBEvent(e.target.title)
    } else if (className == 'changeLineNum') { // 增减游标线
      // this.optionConfig.YBEvent(e.target.title)
      this.creatChangeYBlineDiv()
    } else if (className == 'deleteNote') {
      this.deleteRemark(this.rightClickedNoteId, echart)
      this.rightClickedNoteId = ''
    }
  }
  // 点击事件，控制单条删除显隐
  setStyleMenuContent(param, echart) {
    this.menuDom.querySelectorAll('.deleteGroup')[0].style.display = 'none'
    this.menuDom.querySelectorAll('.deleteNote')[0].style.display = 'none'
    this.menuDom.querySelectorAll('.changeLineNum')[0].style.display = 'none'
    /* let i = 0
    while (i < 20) {
      if (
        param.target &&
        param.target['__ec_inner_' + i] &&
        param.target['__ec_inner_' + i].componentMainType == 'graphic'
      ) {
        let { id } = param.target
        this.hotUpdate({
          rightClickedYBLineId: id
        })
        this.menuDom.querySelectorAll('.deleteGroup')[0].style.display = 'block'
        this.menuDom.onclick = (e) => {
          this.menuEvent(echart, e)
          return false
        }
      }
      i++
    } */
    const echartDomSize = this.myChartDom.getBoundingClientRect()
    let newTop, newLeft
    if (param.event.offsetY + 160 > echartDomSize.height) {
      newTop = echartDomSize.height - 160
    } else {
      newTop = param.event.offsetY
    }
    if (param.offsetX + 150 > echartDomSize.width) {
      newLeft = echartDomSize.width - 150
    } else {
      newLeft = param.event.offsetX
    }

    if (param.componentType == "markLine" && param.data.id) {
      this.hotUpdate({
        rightClickedYBLineId: param.data.id
      })
      this.menuDom.querySelectorAll('.deleteGroup')[0].style.display = 'block'
      if (param.data.id.includes('bei') || param.data.id.includes('bian')) {
        this.menuDom.querySelectorAll('.changeLineNum')[0].style.display = 'block'
      }
    }
    if (param.componentType == "graphic" && param.event.target.parent) {
      let id = param.event.target.parent.id
      if (typeof id == 'string' && id.includes('marks')) {
        this.rightClickedNoteId = param.event.target.parent.id
        this.menuDom.querySelectorAll('.deleteNote')[0].style.display = 'block'
      }
    }

    this.menuDom.style.cssText = `top:${newTop}px;left:${newLeft}px;display:block`
    document.body.addEventListener('click', () => { this.closeMenu() })
  }
  // 修改主题色
  /*   changeTheme() {
      const { dataZoomDataList } = this.hotUpdate()
      this.creatToolBox()
      this.updateBackDomStyle(dataZoomDataList)
      if (this.themeType == 'light') {
        this.noteDom.style.background = '#fff'
        let textDom = this.noteDom.getElementsByTagName('textarea')[0]
        textDom.style.color = '#555555'
        textDom.style.background = 'rgba(186, 186, 186, 0.5)'
      } else {
        this.noteDom.style.background = '#000'
        let textDom = this.noteDom.getElementsByTagName('textarea')[0]
        textDom.style.color = '#9E9E9E'
        textDom.style.background = 'rgba(39, 39, 39, 0.5)'
      }
      const { titleStyle, backgroundColor, gridStyle, legendStyle, xAxisStyle, yAxisStyle } = getOptions(this.themeType)
      let { grid, xAxis, yAxis, title, legend } = this.myChart.getOption()
      this.myChart.setOption({
        backgroundColor,
        grid: getNewArr(grid, gridStyle),
        xAxis: getNewArr(xAxis, xAxisStyle),
        yAxis: getNewArr(yAxis, yAxisStyle),
        title: {
          ...title,
          ...titleStyle
        },
        legend: {
          ...legend,
          ...legendStyle
        }
      })
    } */
  // toolbox：放大方式响应事件
  changeZoomType(value, dom, echart) {
    let nodes = document.getElementById(this.myChartId + 'toolbox').getElementsByClassName('activeRadio')
    nodes?.length && nodes[0].classList.remove('activeRadio')
    this.dataZoomType = this.dataZoomType == value ? '' : value

    if (this.dataZoomType == '') {
      // 关闭
      echart.dispatchAction({
        type: 'takeGlobalCursor',
        key: 'dataZoomSelect',
        dataZoomSelectActive: false
      });
      dom.parentNode.parentNode.classList.remove('activedLi')
      return
    }
    if (!dom.parentNode.parentNode.classList.contains('activedLi')) {
      dom.parentNode.parentNode.classList.add('activedLi')
    }
    dom.classList.add('activeRadio')
    if (value == 'X') {
      echart.setOption({
        toolbox: {
          id: 'toolbox',
          show: true,
          feature: {
            dataZoom: {
              show: true,
              yAxisIndex: false,
              xAxisIndex: [0, 1, 2, 3],
              iconStyle: {
                opacity: 0
              },
              title: {
                zoom: "",
                back: ''
              },
              brushStyle: {
                color: 'rgba(23,47,214,0.2)'
              }
            }
          }
        }
      })
    } else if (value == 'Y') {
      echart.setOption({
        toolbox: {
          id: 'toolbox',
          show: true,
          feature: {
            dataZoom: {
              show: true,
              yAxisIndex: [0, 1, 2, 3],
              xAxisIndex: false,
              title: {
                zoom: "",
                back: ''
              },
              iconStyle: {
                opacity: 0
              },
              brushStyle: {
                color: 'rgba(23,47,214,0.2)'
              }
            }
          }
        },
      })
    } else {
      echart.setOption({
        toolbox: {
          id: 'toolbox',
          show: true,
          feature: {
            dataZoom: {
              show: true,
              xAxisIndex: [0, 1, 2, 3],
              yAxisIndex: [0, 1, 2, 3],
              title: {
                zoom: "",
                back: ''
              },
              iconStyle: {
                opacity: 0
              },
              brushStyle: {
                color: 'rgba(23,47,214,0.2)'
              },
            }
          }
        },
      })
    }
    echart.dispatchAction({
      type: 'takeGlobalCursor',
      key: 'dataZoomSelect',
      dataZoomSelectActive: true
    })

  }

  // 获取图表数据,导出下载数据
  getData(echart) {
    const r = {};
    r.fileName = ''
    r.result = [];
    const { echartData } = this.hotUpdate()
    r.sheetName = echartData.legendData
    r.fileName = echartData.titleText
    echartData.data.forEach(item => {
      r.result.push({
        data: item.source,
        columnList: item.dimensions
      })
    })
    return r;
  }

  // 数据下载
  downLoadData(echart) {
    const data = this.getData(echart)
    const chart_data = []
    data.result.forEach((item, i) => {
      const newchart_data = [[...item.columnList]]
      chart_data[i] = newchart_data.concat(item.data)
    })
    outputXlsxFile(chart_data, data.fileName, data.sheetName)
  }
  // 辅助信息拖拽
  /**
   * 
   * @param {*} id 拖动的markid
   * @param {*} position 位置数组x,y
   */
  onTextDragging(id, position, echart) {
    let { graphic } = echart.getOption()
    let result = graphic[0].elements
    result.forEach(i => {
      if (i.id == id) {
        i.left = position[0]
        i.top = position[1]
      }
    })
    echart.setOption({
      graphic: result
    })
  }
  /**
   * toolbox: 添加备注
   * @param {*} text 文本
   * @param {*} position 位置坐标
   */
  // 添加辅助，备注信息
  addRemarks(text, position, echart) {
    let idName = 'marks' + this.markindex
    let info = {
      id: idName,
      left: position[0],
      top: position[1],
      draggable: true,
      $action: 'replace',
      cursor: "move",
      ondrag: (position) => {
        this.onTextDragging(idName, [position.offsetX, position.offsetY], echart);
      },
      type: 'text',
      z: 100,
      style: {
        fill: this.themeType == 'light' ? '#000' : '#fff',
        width: 100,
        overflow: 'break',
        text: text,
        font: '14px Microsoft YaHei'
      }
    }
    let graphicList = []
    let { graphic } = echart.getOption()
    if (!graphic || !graphic.length) {
      graphicList.push(info)
    } else {
      graphicList = [...graphic[0].elements, info]
    }
    echart.setOption({
      graphic: graphicList
    })
    this.markindex++
  }
  // toolbox: 清空备注
  deleteAllRemarks(echart) {
    let options = echart.getOption()
    let { graphic } = options
    if (!graphic || !graphic.length) {
      return
    } else {
      let list = graphic[0].elements.filter(ele => ele.id.includes('marks'))
      if (list.length) {
        echart.setOption({
          ...options,
          graphic: graphic[0].elements.filter(ele => !ele.id.includes('marks'))
        }, true)
      } else {
        return
      }
    }
  }
  /**
    * 右键事件: 删除某个备注
    * @param {*} id
    * @returns 
    */
  deleteRemark(id, echart) {
    let markId = id || this.rightClickedNoteId
    let options = echart.getOption()
    let { graphic } = options
    if (!graphic || !graphic.length) {
      return
    } else {
      let list = graphic[0].elements.filter(ele => ele.id !== markId)
      echart.setOption({
        ...options,
        graphic: list
      }, true)
    }
  }
  //  缩放回退
  goBack(echart, isRestore) {
    const { dataZoomDataList } = this.hotUpdate()
    let newDataZoomList = []
    if (isRestore) {
      if (this.optionConfig.isYB) {
        this.hotUpdate({ dataZoomDataList: [] }, [{ start: 0, end: 100 }])

      } else {
        let options = echart.getOption()
        let dataZoom = options.dataZoom.map(item => {
          return { ...item, start: 0, end: 100 }
        })
        this.hotUpdate({ dataZoomDataList: [] })
        echart.setOption({
          dataZoom: dataZoom
        })
      }
      // 回退样式更新
      this.updateBackDomStyle([])
      return
    }
    if (dataZoomDataList.length) {
      let dataZoom = []
      if (dataZoomDataList.length > 1) {
        dataZoom = dataZoomDataList[dataZoomDataList.length - 2]
        newDataZoomList = dataZoomDataList.slice(0, -1)
      }
      if (this.optionConfig.isYB) {
        if (dataZoomDataList.length == 1) {
          dataZoom = [{ start: 0, end: 100 }]
        }
        this.hotUpdate({ dataZoomDataList: newDataZoomList }, dataZoom)
      } else {
        if (dataZoomDataList.length == 1) {
          let options = echart.getOption()
          dataZoom = options.dataZoom.map(item => {
            return { ...item, start: 0, end: 100 }
          })
        }
        this.hotUpdate({ dataZoomDataList: newDataZoomList })
        echart.setOption({
          dataZoom: dataZoom
        })
      }
      // 回退样式更新
      this.updateBackDomStyle(newDataZoomList)
    }
  }

  // 增减游标线数量输入弹窗
  creatChangeYBlineDiv() {
    let dom = document.getElementById(this.myChartId + 'echart_Ybline_add')
    if (dom) {
      dom.style.display = 'block'
    } else {
      dom = document.createElement('div')
      dom.setAttribute('id', this.myChartId + 'echart_Ybline_add')
      dom.classList.add('echart_Ybline_add');
      dom.classList.add('echart_Ybline_add_' + this.themeType);
      let htmlStr = `<p>变化量：<input type="number" value="1"/>倍游标 (增：>0, 减：<0)</p>
      <p><button class="cancel">取消</button>
      <button class="confirm">确定</button></p> `
      dom.insertAdjacentHTML('afterbegin', htmlStr)
      this.myChartDom.appendChild(dom)
    }
    // let { left, top } = this.menuDom.style
    //  dom.style.cssText = `top:${top};left:${left};display:block`
    let that = this
    dom.onclick = null;
    dom.onclick = function (e) {
      that.addYBBlockEvent(e, dom)
      return false
    }
  }
  addYBBlockEvent(e, dom) {
    let className = e.target.className
    if (className == 'cancel') { // 删除一组游标线
      dom.style.display = 'none'
    } else if (className == 'confirm') { // 增减游标线
      let value = dom.getElementsByTagName('input')[0].value
      this.optionConfig.YBEvent('changeLineNum', value)
      dom.style.display = 'none'
    }
  }

}
// '#bddeff'
/*  <li title="暗" class="blackTheme"><img src='${require('../../assets/black.png')}' alt='pic' /></li>
     <li title="亮" class="whiteTheme"><img src='${require('../../assets/white.png')}' alt='pic' /></li> */
{/* <li class='radioStyle ${this.optionConfig.isZoom ? '' : 'hideLi'} zoomx' title = "x轴放大" > <img src='${require(' /src / component / assets / ' + theme + ' / x.png')}' alt = 'pic' /></li >
  <li class='radioStyle ${this.optionConfig.isZoom ? '' : 'hideLi'} zoomy' title = "y轴放大" > <img src='${require(' /src / component / assets / ' + theme + ' / y.png')}' alt = 'pic' /></li >
    <li class='radioStyle ${this.optionConfig.isZoom ? '' : 'hideLi'} zoomy' title = "xy轴放大" > <img src='${require(' /src / component / assets / ' + theme + ' / xy.png')}' alt = 'pic' /></li >
      <li class="back ${this.optionConfig.isZoom ? '' : 'hideLi'}" title="缩放回退"><img src='${require(' ../../assets/' + theme + '/resetzoom.png')}' alt='pic' /></li>
    
    /* <p class='YB ${this.optionConfig.isYB && this.optionConfig.YBList.isPeak ? '' : 'hideLi'}' title="峰值"><i class="icon iconfont acharts-peak"></i>峰值</p> */
}