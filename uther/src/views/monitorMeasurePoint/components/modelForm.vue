<template>
  <div class="right_form">
    <h4>{{ newTitle }} <i class="el-icon-close" @click="cancel"></i></h4>
    <div style="height: 85%; overflow-x: hidden; overflow-y: auto">
      <el-form :model="form" label-width="100px" :rules="rules" ref="addForm">
        <template v-if="showType == 'plan'">
          <el-form-item label="方案名称：" prop="name">
            <el-input v-model="form.name" clearable> </el-input>
          </el-form-item>
          <el-form-item label="机组型号：" prop="turbineType">
            <el-cascader
              ref="turbineType"
              :key="modelCascader"
              v-model="form.turbineType"
              placeholder="请选择机组型号"
              :options="turbineTypeList"
              :props="turbineProps"
              collapse-tags
              clearable
              style="display: inline"
              @change="turbineTypeChange"
            >
            </el-cascader>
          </el-form-item>
          <el-form-item label="监测设备：" prop="deviceList">
            <el-checkbox-group v-model="form.deviceList">
              <el-checkbox
                name="deviceList"
                v-for="item in deviceOptions"
                :key="item.entityName"
                :label="item.entityName"
                :value="item.entityName"
              ></el-checkbox>
            </el-checkbox-group>
          </el-form-item>

          <el-form-item label="默认部件：" prop="firstComp">
            <el-select v-model="form.firstComp" placeholder="请选择默认部件" style="width: 100%">
              <el-option
                v-for="item in firstCompSelectList"
                :key="item.entityName"
                :label="item.entityName"
                :value="item.entityName"
              >
              </el-option>
            </el-select>
          </el-form-item>
        </template>
        <template v-else-if="showType == '整机'">
          <el-form-item label="方案名称：" prop="name">
            <el-input v-model="form.name" :disabled="true"> </el-input>
          </el-form-item>
          <el-form-item label="设备模型：" prop="modelName">
            <el-cascader
              :key="modelCascader"
              v-model="form.modelName"
              placeholder="请选择设备"
              :options="dirveData"
              :props="modelProps"
              collapse-tags
              clearable
              :disabled="true"
              style="display: inline"
            >
            </el-cascader>
          </el-form-item>
          <el-form-item label="详细特征值：" prop="isDetailValue">
            <el-switch v-model="form.isDetailValue"> </el-switch>
          </el-form-item>
          <el-form-item label="位置参数：" prop="isDetailValue">
            <div v-for="(witem, windex) in objPosition" :key="windex" style="margin-bottom: 10px">
              <span class="axle_style">{{ axleIndex[windex] }}：</span>
              <el-input-number
                v-model="objPosition[windex]"
                clearable
                :precision="4"
                :step="0.1"
                :controls="false"
                @change="handlerPositionChange"
              >
              </el-input-number>
            </div>
          </el-form-item>
        </template>
        <template v-else>
          <el-form-item label="方案名称：" prop="name">
            <el-input v-model="form.name" :disabled="true"> </el-input>
          </el-form-item>
          <el-form-item label="设备模型：" prop="modelName">
            <el-cascader
              :key="modelCascader"
              v-model="form.modelName"
              placeholder="请选择设备"
              :options="dirveData"
              :props="modelProps"
              collapse-tags
              clearable
              :disabled="true"
              style="display: inline"
            >
            </el-cascader>
          </el-form-item>
          <!-- <el-form-item label="测量部件：" prop="monitorPointerList">
            <el-cascader
              :key="monitorCompCascader"
              ref="compascader"
              v-model="form.monitorPointerList"
              :options="measureCompList"
              :props="compProps"
              placeholder="请选择测量部件"
              clearable
              collapse-tags
              style="display: inline"
              @change="selectCompChange"
            ></el-cascader>
          </el-form-item> -->
          <el-form-item label="测量位置：" prop="measurePosition">
            <el-cascader
              :key="monitorSpotCascader"
              ref="measureCascader"
              v-model="form.measurePosition"
              :options="compList"
              :props="positionProps"
              placeholder="请选择测量位置"
              clearable
              collapse-tags
              style="display: inline"
            ></el-cascader>
          </el-form-item>
          <el-form-item label="详细特征值：" prop="isDetailValue">
            <el-switch v-model="form.isDetailValue"> </el-switch>
          </el-form-item>
          <el-form-item label="位置参数：" prop="isDetailValue">
            <div v-for="(witem, windex) in objPosition" :key="windex" style="margin-bottom: 10px">
              <span class="axle_style">{{ axleIndex[windex] }}：</span>
              <el-input-number
                v-model="objPosition[windex]"
                clearable
                :precision="4"
                :step="0.1"
                :controls="false"
                @change="handlerPositionChange"
              >
              </el-input-number>
            </div>
          </el-form-item>
          <!-- <div class="template_line" v-show="showSteelNum"></div>
          <el-form-item label="钢索数：" prop="steelCableNum" v-show="showSteelNum">
            <el-input-number
              v-model="form.steelCableNum"
              @change="steelCableNumChange"
              :min="0"
              :max="100"
              :controls="false"
              style="width: 75%"
            ></el-input-number>
          </el-form-item>
          <el-checkbox-group
            v-model="form.checkedSteel"
            v-show="steelNumber !== 0 && this.showSteelNum"
            class="steel_checkbox"
          >
            <el-checkbox v-for="index in steelNumber" :label="index" :key="index" :value="index"
              >钢索{{ index }}</el-checkbox
            >
          </el-checkbox-group> -->
        </template>
      </el-form>
    </div>
    <div class="footer_btn">
      <el-button type="primary" size="small" @click="saveModel">保存</el-button>
      <el-button type="primary" size="small" @click="viewModel()" v-if="showType !== 'plan'"
        >预览</el-button
      >
      <el-button type="primary" size="small" @click="cancel">取消</el-button>
    </div>
  </div>
</template>

<script>
import { getCompApi, getDeviceMonitorByFentitycode } from '@/api/equipment/component'
import { getList, getPagecomp } from '@/api/equipment/model'
import { savePlan, getDeviceApi, getInitPosition } from '@/api/equipment/monitor'

export default {
  props: {
    showType: {
      type: String,
      default: ''
    },
    dirveData: {
      type: Array,
      default: () => []
    },
    treeData: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      monitorCompCascader: 0,
      monitorSpotCascader: 0,
      treeClickObj: '', //点击树的节点
      deviceSelect: [], //选择的设备
      compList: [], // 可选择的部件数组
      deviceOptions: [], //监测设备数组
      allPagecomp: [], //所有聚合部件数组
      isCompList: '', // 判断接口是否调用完成
      initPositionList: [],
      axleIndex: {
        0: 'X',
        1: 'Y',
        2: 'Z'
      },
      objPosition: [0, 0, 0],
      form: {
        name: '',
        turbineType: '',
        deviceList: [],
        modelName: [],
        monitorPointerList: [], // 选择的测量部件
        measurePosition: [], //选择的测量位置
        steelCableNum: '',
        checkedSteel: [],
        isDetailValue: true,
        firstComp: '' //最先展示的页面
      },
      turbineProps: {
        // multiple: true,
        // checkStrictly: true,
        expandTrigger: 'hover',
        value: 'label'
      },
      modelProps: {
        multiple: true,
        expandTrigger: 'hover',
        checkStrictly: true,
        value: 'label'
      },
      compProps: {
        value: 'code',
        label: 'name',
        children: 'childNode',
        // expandTrigger: 'hover',
        checkStrictly: true,
        multiple: true
      },
      positionProps: {
        value: 'code',
        label: 'name',
        // expandTrigger: 'hover',
        multiple: true
      },
      rules: {
        name: [{ required: true, message: '请输入方案名称', trigger: 'blur' }],
        turbineType: [{ required: true, message: '请选择机组模型', trigger: 'change' }],
        deviceList: [
          { type: 'array', required: true, message: '请至少选择一个监测设备', trigger: 'change' }
        ],
        modelName: [{ required: true, message: '请选择设备模型', trigger: 'change' }],
        monitorPointerList: [
          { type: 'array', required: true, message: '请至少选择一个测量部件', trigger: 'change' }
        ],
        measurePosition: [
          { type: 'array', required: true, message: '请至少选择一个测量部件', trigger: 'change' }
        ],
        steelCableNum: [{ required: true, message: '请输入钢索数', trigger: 'blur' }]
      }
      /* turbineSpotPosition: [
        {
          index: 0,
          name: '风轮',
          spot: [0.2, 10.7, -2.2],
          card: [Math.random() * 10, Math.random() * 10, Math.random() * 1]
        },
        {
          index: 1,
          name: '机舱',
          spot: [0.4, 10.7, 0],
          card: [Math.random() * 10, Math.random() * 10, Math.random() * 1]
        },
        {
          index: 2,
          name: '塔筒',
          spot: [0.6, -8, 0],
          card: [Math.random() * 10, Math.random() * 10, Math.random() * 1]
        }
      ] */
    }
  },
  /* watch: {
    deviceOptions: {
      handler(val) {
        console.log('监测设备改变', val)
      },
      immediate: true
    }
  }, */
  computed: {
    turbineTypeList() {
      return this.dirveData.find(j => j.label == '整机').children
    },
    newTitle() {
      let title
      if (this.showType == 'plan') {
        title = '新增方案'
      } else {
        title = this.showType
      }
      return title
    },
    /* measureCompList() {
      let data = this.compList.find(j => j.name == this.newTitle)
        ? this.compList.find(j => j.name == this.newTitle)
        : []
      return data
    }, */
    firstCompSelectList() {
      let list = []
      this.deviceOptions.forEach(item => {
        if (this.form.deviceList.find(k => k == item.entityName)) {
          list.push(item)
        }
      })

      return list.sort(function (a, b) {
        return new String(a.sort) - new String(b.sort)
      })
    }
  },
  created() {
    this.getDevice()

    this.getInitSpotPosition()
  },
  mounted() {
    this.$bus.$on('mouseObjPosition', val => {
      this.objPosition = [val.position.x, val.position.y, val.position.z]
    })
  },
  beforeDestroy() {
    this.clearData()
  },
  methods: {
    // 1.1、获取监测设备
    getDevice() {
      getPagecomp().then(res => {
        this.allPagecomp = res.data.data
        this.deviceOptions = res.data.data
        this.deviceOptions.sort(function (a, b) {
          return new String(a.sort) - new String(b.sort)
        })
      })
    },
    // 1.2、获取测量位置
    getCompSpot() {
      this.isCompList = new Promise(resolve => {
        const index = this.deviceOptions.find(j => j.entityName == this.showType)
          ? this.deviceOptions.find(j => j.entityName == this.showType).entityType
          : ''
        getDeviceMonitorByFentitycode({ FentityCode: index }).then(res => {
          if (res.data.code === 200) {
            this.compList = res.data.data
            resolve(res.data.data)
          }
        })
      })
    },
    // 1.3、获取测点的默认值
    getInitSpotPosition() {
      getInitPosition().then(res => {
        this.initPositionList = res.data.data
      })
    },
    // 根据选中节点拿到对象
    handlerPosition(nameArr) {
      let data = this.$refs['measureCascader'].getCheckedNodes()
      let obj
      data.forEach(ele => {
        if (ele.value == nameArr[nameArr.length - 1]) {
          obj = ele.data
        }
      })
      return obj
    },
    // 选中模型位置改变
    handlerPositionChange() {
      this.$bus.$emit('changeObjPosition', this.objPosition)
    },

    // 选中机组设备
    turbineTypeChange(val) {
      if (val.length) {
        let selectTurbine = this.$refs['turbineType'].getCheckedNodes()[0].data.embodyId

        let promiseList = []
        selectTurbine.forEach(item => {
          let p = new Promise(resolve => {
            getList(item).then(res => {
              if (res.data.code === 200) {
                resolve(res.data.data[0])
              }
            })
          })
          promiseList.push(p)
        })

        let list = [
          {
            entityName: '整机',
            entityType: 'windturbine',
            sort: '1',
            children: []
          }
        ]
        Promise.all(promiseList).then(resolt => {
          resolt.forEach(i => {
            let obj = this.allPagecomp.find(j => j.entityName == i.compName)
            if (obj) {
              list.push(obj)
            }
          })
        })
        this.deviceOptions = list.sort(function (a, b) {
          return new String(a.sort) - new String(b.sort)
        })
      } else {
        this.deviceOptions = this.allPagecomp
        this.form.deviceList = []
      }
    },

    // 预览模型
    viewModel() {
      let path = []
      this.deviceSelect.forEach(item => {
        if (item.isShow && !path.find(j => j.path == item.path)) {
          path.push(item)
        }
      })

      let position = []

      if (this.showType !== '整机' && this.$refs['measureCascader']) {
        position = this.viewCompModel()
      } else if (this.showType == '整机') {
        position = this.viewTurbineModel()
      }

      let newPosition = []
      if (this.showType == '机舱') {
        newPosition = position
      } else {
        position.forEach(item => {
          let { fCode } = item
          let obj = newPosition.find(j => j.fCode == fCode)
          if (!obj) {
            newPosition.push(item)
          } else {
            obj.name = obj.name + '&' + item.name
          }
        })
      }

      this.$emit('viewModel', path, newPosition)
    },
    viewTurbineModel() {
      let position = []
      let plan = this.treeData.find(j => j.name == this.treeClickObj.solutionName)

      if (this.treeClickObj.measlocPositionList.length < 1) {
        plan.children.forEach(item => {
          if (item.name !== '整机') {
            let { sort, name } = item
            position.push({
              index: sort,
              fCode: name,
              name,
              card: this.returnRandomPosition(),
              spot: this.returnRandomPosition()
            })
          }
        })
      } else {
        this.treeClickObj.measlocPositionList.forEach((item, index) => {
          position.push({
            fCode: item.measlocCode[0],
            name: item.measlocCode[0],
            index,
            spot: this.judgeListNumber(item.spot),
            card: this.judgeListNumber(item.cardPosition)
          })
        })
      }
      return position
    },
    viewCompModel() {
      let position = []
      let data = this.$refs['measureCascader'].getCheckedNodes(true)

      data.forEach((ele, index) => {
        let codeList = this.getNodeParent(ele, [], 'code').reverse()
        let nameList = this.getNodeParent(ele, [], 'name').reverse()
        let initObj = this.treeClickObj.measlocPositionList.find(
          j => j.measlocCode.join('&') == codeList.join('&')
        )

        const codeIndex = codeList.join('&').split('&').join('')
        const positionObj = this.initPositionList.find(j => j.measlocCode == codeIndex)

        position.push({
          fCode:
            this.showType == '机舱'
              ? codeList.join('&')
              : codeList.slice(0, codeList.length - 1).join('&'),
          name: nameList.join(''),
          index,
          spot: initObj
            ? this.judgeListNumber(initObj.spot)
            : positionObj
            ? this.judgeListNumber(positionObj.measlocPosition.spot)
            : this.returnRandomPosition(),
          card: initObj
            ? this.judgeListNumber(initObj.cardPosition)
            : positionObj
            ? this.judgeListNumber(positionObj.measlocPosition.cardPosition)
            : this.returnRandomPosition()
        })
      })
      return position
    },

    // 保存按钮
    saveModel() {
      this.$refs['addForm'].validate(valid => {
        if (valid) {
          if (this.showType == 'plan') {
            this.handlerSavePlan()
          } else if (this.showType == '整机') {
            this.handlerTurbinePlan()
          } else {
            let param = this.handlerSaveMark()
            this.$emit('saveMark', param)
          }
        }
      })
    },
    // 保存方案
    handlerSavePlan() {
      let data = this.$refs['turbineType'].getCheckedNodes()

      let param = {
        planName: this.form.name,
        modelTypeId: data[0].data.id,
        deviceModel: this.form.deviceList,
        firstComp: this.form.firstComp,
        deviceCode: []
      }
      this.form.deviceList.forEach(item => {
        let obj = this.allPagecomp.find(j => j.entityName == item)
        if (obj) {
          param.deviceCode.push(obj.entityType)
        }
      })
      console.log('保存方案参数', param)
      savePlan({ ...param }).then(res => {
        if (res.data.code === 200) {
          this.$message({
            type: 'success',
            message: '方案保存成功！'
          })
          this.$emit('initTreeData')
        }
      })
    },
    // 保存整机方案
    handlerTurbinePlan() {
      let param = {
        deviceModelType: this.showType,
        planName: this.form.name,
        detailEv: this.form.isDetailValue,
        id: this.treeClickObj.id,
        monitorDevice: ['windturbine'], //测量部件code
        measlocPositionList: [],
        monitorDeviceProperty: [] //钢索配置信息
      }

      let plan = this.treeData.find(j => j.name == this.treeClickObj.solutionName).children

      plan.forEach(item => {
        if (item.deviceModelType !== '整机') {
          param.measlocPositionList.push({
            measlocCode: [item.deviceModelType],
            spot: [],
            card: []
          })
        }
      })

      this.$emit('saveMark', param)
    },
    // 保存部件方案
    handlerSaveMark() {
      let param = {
        deviceModelType: this.showType,
        planName: this.form.name,
        detailEv: this.form.isDetailValue,
        id: this.treeClickObj.id,
        monitorDevice: [], //测量部件code
        measlocPositionList: [], //测点code+测点position
        monitorDeviceProperty: [] //钢索配置信息
      }

      /*let compData = this.$refs['compascader'].getCheckedNodes()
      compData.forEach(item => {
        param.monitorDevice.push(item.value)
      })*/

      let measureData = this.$refs['measureCascader'].getCheckedNodes(true)
      measureData.forEach((item, index) => {
        let code = this.getNodeParent(item, [], 'code').reverse().join('&')

        param.measlocPositionList.push({
          index,
          measlocCode: code.split('&'),
          spot: [0, 0, 0],
          cardPosition: [0, 0, 0]
        })
        if (!param.monitorDevice.find(j => j == code.split('&')[0])) {
          param.monitorDevice.push(code.split('&')[0])
        }
      })

      console.log('确定参数', this.showType, this.deviceOptions)

      /* if (compData.find(j => j.label == '钢索')) {
        param.monitorDeviceProperty.push({
          monitorDevice: 'SSD',
          property: {
            total: this.form.steelCableNum,
            isMonitor: this.form.checkedSteel
          }
        })
      }*/
      console.log('保存测点参数', this.showType, param)
      return param
    },

    // 获取点击节点的所有父节点
    getNodeParent(node, list, index) {
      list.push(node.data[index])
      if (node.parent) {
        this.getNodeParent(node.parent, list, index)
      }
      return list
    },
    // 对坐标中数值大于100的做处理
    judgeListNumber(arr) {
      let newArr = []
      arr.forEach(item => {
        if (item > 100 || item < -100) {
          newArr.push(Math.random() * 5)
        } else {
          newArr.push(item)
        }
      })
      return newArr
    },
    // 返回随机位置
    returnRandomPosition() {
      let index
      if (this.showType == '机舱' || this.showType == '变桨与偏航') {
        index = 1
      } else {
        index = 5
      }
      return [Math.random() * index, Math.random() * index, Math.random() * index]
    },

    // 取消模型配置
    cancel() {
      this.$emit('clickedPlan', '')
    },

    // 查看数据
    initData(clickObj) {
      this.clearData()

      this.getCompSpot()

      this.treeClickObj = clickObj
      this.form.name = clickObj.solutionName // 方案名称

      // 1、选中设备模型
      let modelSelectList = clickObj.deviceModelList // 选中模型ID

      this.deviceSelect = []
      let typeData = this.dirveData.find(j => j.label == this.showType)
      typeData &&
        typeData.children.forEach(item => {
          item.children.forEach(ite => {
            let obj = modelSelectList.find(j => j == ite.id)
            if (obj) {
              this.form.modelName = [[this.showType, item.label, ite.label]]
              this.deviceSelect.push(...ite.propertiesView)
            }
          })
        })

      // 2、选中测量部件
      let compList = []
      let indexList = ['ROT', 'NAC', 'TWW']
      clickObj.measlocPositionList.forEach(item => {
        if (indexList.find(j => j == item.measlocCode[0])) {
          compList.push(item.measlocCode.slice(1))
        } else {
          compList.push(item.measlocCode)
        }
      })
      this.form.measurePosition = compList
      this.monitorSpotCascader = new Date().getTime()

      setTimeout(() => {
        this.viewModel()
      }, 500)
    },
    clearData() {
      this.treeClickObj = ''
      this.form = {
        name: '',
        turbineType: '',
        deviceList: [''],
        modelName: [],
        monitorPointerList: [], // 选择的测量部件
        measurePosition: [], //选择的测量位置
        steelCableNum: '',
        checkedSteel: [],
        isDetailValue: true
      }
    }
  }
}
</script>

<style lang="less" scoped>
.right_form {
  width: 100%;
  height: 100%;
  padding: 15px;
  h4 {
    font-size: 14px;
    font-weight: 600;
    text-align: center;
    color: #fff;
    line-height: 32px;
    margin-bottom: 15px;
    .el-icon-close {
      float: right;
      cursor: pointer;
    }
  }
  .template_line {
    width: 100%;
    height: 1px;
    background: rgba(216, 216, 216, 0.2);
    margin-bottom: 10px;
  }
}

::v-deep .el-input-number {
  width: 90px;
  .el-input__inner {
    width: 85px;
  }
}
.footer_btn {
  width: 100%;
  text-align: center;
  margin-top: 30px;
}
.steel_checkbox {
  left: 12px;
  position: relative;
  ::v-deep .el-checkbox {
    margin: 5px 20px 8px 0;
    .el-checkbox__label {
      width: 54px;
    }
  }
}
.el-form-item {
  margin-bottom: 28px;
}
.el-button--primary {
  width: 75px;
}
::v-deep .el-input-number {
  width: 75%;
  .el-input__inner {
    width: 100%;
  }
}

::v-deep .el-input.is-disabled .el-input__inner {
  background: rgba(71, 86, 128, 0.2);
}
::v-deep .el-checkbox__input.is-disabled + span.el-checkbox__label {
  color: #409eff;
}
::v-deep .el-checkbox__input.is-disabled.is-checked .el-checkbox__inner {
  background-color: #409eff;
  border-color: #409eff;
}
::v-deep .el-checkbox__input.is-disabled.is-checked .el-checkbox__inner::after {
  border-color: white;
}
::v-deep .el-input.is-disabled .el-input__inner {
  border-color: rgba(71, 86, 128, 1);
}
</style>
