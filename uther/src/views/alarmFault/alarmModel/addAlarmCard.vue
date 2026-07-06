<template>
  <el-dialog
    :title="dialogTitle"
    append-to-body
    :visible.sync="addBox"
    width="780px"
    custom-class="add_card"
  >
    <el-form
      class="dialog_content"
      :model="addForm"
      label-width="80px"
      :rules="addRules"
      ref="addRuleForm"
      style="padding: 0px"
    >
      <el-form-item label="测量位置：" prop="compCode" style="width: 100%">
        <el-cascader
          placeholder="请选择测量位置"
          v-model="addForm.compCode"
          :options="compData"
          :props="compProps"
          :disabled="isWatch"
          @change="compChange"
          style="width: 100%"
        ></el-cascader>
        <!-- <el-form-item label="部件：" prop="compCode">
        </el-form-item> -->
        <!-- <el-form-item label="截面：">
            <el-select
              v-model="addForm.sectionName"
              placeholder="请选择截面"
              :disabled="isWatch"
              @change="selectValue"
              clearable
              filterable
            >
              <el-option
                v-for="item in Slist"
                :key="item.code"
                :label="item.name"
                :value="item.name"
              >
              </el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="方向：">
            <el-select
              v-model="addForm.orientation"
              placeholder="请选择方向"
              :disabled="isWatch"
              @change="selectValue"
              clearable
              filterable
            >
              <el-option
                v-for="item in Olist"
                :key="item.code"
                :label="item.name"
                :value="item.name"
              >
              </el-option>
            </el-select>
          </el-form-item> -->
        <!-- <el-form-item label="物理量：">
            <el-select
              v-model="addForm.paraUnit"
              placeholder="请选择物理量"
              :disabled="isWatch"
              @change="selectValue"
              clearable
              filterable
            >
              <el-option
                v-for="item in unitList"
                :key="item.code"
                :label="item.name"
                :value="item.name"
              >
              </el-option>
            </el-select>
          </el-form-item> -->
      </el-form-item>
      <el-form-item label="特征值：" prop="evName" style="width: 100%">
        <el-select
          v-model="addForm.evName"
          placeholder="请选择特征值"
          class="value_select"
          :disabled="isWatch"
          clearable
          filterable
        >
          <el-option
            v-for="item in valueList"
            :key="item.evCode"
            :label="item.evName"
            :value="item.evName"
          >
          </el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="评估阈值：" class="threshold_value" prop="thresholdValue">
        <div
          class="form_border form_border_threshold"
          style="padding-left: 10px; display: flex; flex-direction: column"
        >
          <el-checkbox
            v-model="isCheck.isNormal"
            :disabled="isWatch"
            @change="clickCheckbox($event, ['normal'])"
            ><span style="color: #2ed133"> 正常：</span>
            <span style="margin-right: 19%; margin-left: 8%">
              下限：
              <el-autocomplete
                v-model="addForm.normalValueDown"
                class="value_mini"
                :disabled="!isCheck.isNormal"
                :fetch-suggestions="searchGeekIdDown"
                placeholder="请输入下限"
                :style="{
                  background: isCheck.isNormal ? 'rgba(71, 86, 128, 0.5)' : ''
                }"
              />
            </span>
            <span>
              上限：
              <el-autocomplete
                v-model="addForm.normalValueUp"
                class="value_mini"
                :disabled="!isCheck.isNormal"
                :fetch-suggestions="searchGeekIdUp"
                placeholder="请输入上限"
                :style="{
                  background: isCheck.isNormal ? 'rgba(71, 86, 128, 0.5)' : ''
                }"
              />
            </span>
          </el-checkbox>
          <el-checkbox
            v-model="isCheck.isAttention"
            :disabled="isWatch"
            @change="clickCheckbox($event, ['attention'])"
            ><span style="color: #ffe604"> 注意：</span>
            <span style="margin-right: 19%; margin-left: 8%">
              下限：
              <el-autocomplete
                v-model="addForm.attentionValueDown"
                class="value_mini"
                :disabled="!isCheck.isAttention"
                :fetch-suggestions="searchGeekIdDown"
                placeholder="请输入下限"
                :style="{
                  background: isCheck.isAttention ? 'rgba(71, 86, 128, 0.5)' : ''
                }"
              />
            </span>
            <span>
              上限：
              <el-autocomplete
                v-model="addForm.attentionValueUp"
                class="value_mini"
                :disabled="!isCheck.isAttention"
                :fetch-suggestions="searchGeekIdUp"
                placeholder="请输入上限"
                :style="{
                  background: isCheck.isAttention ? 'rgba(71, 86, 128, 0.5)' : ''
                }"
              />
            </span>
          </el-checkbox>
          <el-checkbox
            v-model="isCheck.isWarning"
            :disabled="isWatch"
            @change="clickCheckbox($event, ['warning'])"
            ><span style="color: #ff6b0e"> 警告：</span>
            <span style="margin-right: 19%; margin-left: 8%">
              下限：
              <el-autocomplete
                v-model="addForm.warningValueDown"
                class="value_mini"
                :disabled="!isCheck.isWarning"
                :fetch-suggestions="searchGeekIdDown"
                placeholder="请输入下限"
                :style="{
                  background: isCheck.isWarning ? 'rgba(71, 86, 128, 0.5)' : ''
                }"
              />
            </span>
            <span>
              上限：
              <el-autocomplete
                v-model="addForm.warningValueUp"
                class="value_mini"
                :disabled="!isCheck.isWarning"
                :fetch-suggestions="searchGeekIdUp"
                placeholder="请输入上限"
                :style="{
                  background: isCheck.isWarning ? 'rgba(71, 86, 128, 0.5)' : ''
                }"
              />
            </span>
          </el-checkbox>
          <el-checkbox
            v-model="isCheck.isDanger"
            :disabled="isWatch"
            @change="clickCheckbox($event, ['danger'])"
            ><span style="color: #ff0f0d">危险：</span>
            <span style="margin-right: 19%; margin-left: 8%">
              下限：
              <el-autocomplete
                v-model="addForm.dangerValueDown"
                class="value_mini"
                :disabled="!isCheck.isDanger"
                :fetch-suggestions="searchGeekIdDown"
                placeholder="请输入下限"
                :style="{
                  background: isCheck.isDanger ? 'rgba(71, 86, 128, 0.5)' : ''
                }"
              />
            </span>
            <span>
              上限：
              <el-autocomplete
                v-model="addForm.dangerValueUp"
                class="value_mini"
                :disabled="!isCheck.isDanger"
                :fetch-suggestions="searchGeekIdUp"
                placeholder="请输入上限"
                :style="{
                  background: isCheck.isDanger ? 'rgba(71, 86, 128, 0.5)' : ''
                }"
              />
            </span>
          </el-checkbox>
        </div>
      </el-form-item>
      <el-form-item label="工况：" class="waorking_value">
        <div class="form_border" style="padding-left: 10px">
          <el-checkbox
            v-model="isCheck.isPowerValue"
            :disabled="isWatch"
            @change="clickCheckbox($event, ['powerValueDown', 'powerValueUp'])"
            >功率：
          </el-checkbox>
          <span style="margin-right: 19%; margin-left: 8%">
            下限：
            <el-input-number
              v-model="addForm.powerValueDown"
              class="value_mini"
              :disabled="!isCheck.isPowerValue"
              :min="0"
              :max="9999"
              :controls="false"
              :style="{
                background: isCheck.isPowerValue ? 'rgba(71, 86, 128, 0.5)' : ''
              }"
            ></el-input-number>
          </span>
          <span>
            上限：
            <el-input-number
              v-model="addForm.powerValueUp"
              class="value_mini"
              :disabled="!isCheck.isPowerValue"
              :min="0"
              :max="9999"
              :controls="false"
              :style="{
                background: isCheck.isPowerValue ? 'rgba(71, 86, 128, 0.5)' : ''
              }"
            ></el-input-number>
          </span>
          <el-checkbox
            v-model="isCheck.isSpeedValue"
            :disabled="isWatch"
            @change="clickCheckbox($event, ['speedValueDown', 'speedValueUp'])"
            >转速：
          </el-checkbox>
          <span style="margin-right: 19%; margin-left: 8%">
            下限：
            <el-input-number
              v-model="addForm.speedValueDown"
              class="value_mini"
              :disabled="!isCheck.isSpeedValue"
              :min="0"
              :max="9999"
              :controls="false"
              :style="{
                background: isCheck.isSpeedValue ? 'rgba(71, 86, 128, 0.5)' : ''
              }"
            ></el-input-number>
          </span>

          <span>
            上限：
            <el-input-number
              v-model="addForm.speedValueUp"
              class="value_mini"
              :disabled="!isCheck.isSpeedValue"
              :min="0"
              :max="9999"
              :controls="false"
              :style="{
                background: isCheck.isSpeedValue ? 'rgba(71, 86, 128, 0.5)' : ''
              }"
            ></el-input-number>
          </span>
        </div>
      </el-form-item>
    </el-form>
    <span slot="footer" class="dialog-footer">
      <el-button type="primary" @click="submitRole"
        ><i class="el-icon-circle-plus-outline"></i>保 存</el-button
      >
      <el-button @click="addBox = false"><i class="el-icon-circle-close"></i>取 消</el-button>
    </span>
  </el-dialog>
</template>

<script>
import debounce from 'lodash/debounce'
import { add, update, getAllComp, getUnits, getSelectValue } from '@/api/alarmFault/alarmModel'
import { getPathByKey } from '../../equipment/component/windList'

export default {
  props: {},
  data() {
    return {
      addBox: false,
      dialogTitle: '',
      isWatch: '',
      clickItem: '',
      showManage: {},
      selectObj: {}, //级联选择器选中部件
      compData: [],
      Slist: [],
      Olist: [],
      unitList: [],
      valueList: [], //特征值列表
      tableList: [], // 表格数据
      compProps: {
        value: 'name',
        label: 'name',
        expandTrigger: 'hover',
        checkStrictly: true
        // checkStrictly: true
      },
      isCheck: {
        isNormal: false,
        isAttention: false,
        isWarning: false,
        isDanger: false,
        isPowerValue: false,
        isSpeedValue: false
      },
      addForm: {
        compCode: '',
        sectionName: '',
        orientation: '',
        paraUnit: '',
        evName: '',
        /*attention: undefined,
        warning: undefined,
        danger: undefined,*/
        normalValueDown: null,
        normalValueUp: null,
        attentionValueDown: null,
        attentionValueUp: null,
        warningValueDown: null,
        warningValueUp: null,
        dangerValueDown: null,
        dangerValueUp: null,
        powerValueDown: undefined,
        powerValueUp: undefined,
        speedValueDown: undefined,
        speedValueUp: undefined
      },
      addRules: {
        compCode: [{ required: true, message: '请选择测量位置', trigger: 'change' }],
        evName: [{ required: true, message: '请选择特征值', trigger: 'change' }],
        thresholdValue: [
          { required: true, validator: this.thresholdValidatePass, trigger: 'change' }
        ]
      }
    }
  },
  mounted() {
    this.getComp()
    // this.getUnit()
    this.$bus.$on('tableList', val => {
      this.tableList = val
    })
  },
  methods: {
    //1.1、获取物理量
    getUnit() {
      getUnits().then(res => {
        if (res.data.code === 200) {
          this.unitList = res.data.data
        }
      })
    },

    //1.2、获取全部部件
    getComp() {
      getAllComp().then(res => {
        let allCompArr = []
        res.data.data.forEach(item => {
          allCompArr.push(...item.children)
        })
        this.compData = allCompArr

        console.log('所有部件', this.compData)
      })
    },

    thresholdValidatePass(rule, value, callback) {
      let objKeyStr = []
      for (const isCheckKey in this.isCheck) {
        if (this.isCheck[isCheckKey] && !isCheckKey.includes('Value')) {
          objKeyStr.push(isCheckKey.replace('is', '').toLocaleLowerCase())
        }
      }
      for (let i = 0; i <= objKeyStr.length - 1; i++) {
        let valueDownStatus = this.addForm[objKeyStr[i] + 'ValueDown']
        let valueUpStatus = this.addForm[objKeyStr[i] + 'ValueUp']
        if (
          valueDownStatus === '' ||
          valueDownStatus === null ||
          valueUpStatus === '' ||
          valueUpStatus === null
        ) {
          return callback(new Error('已勾选不能为空值'))
        }
        if (isNaN(Number(valueDownStatus)) && valueDownStatus !== '-∞') {
          return callback(new Error('请输入数字'))
        } else if (isNaN(Number(valueUpStatus)) && valueUpStatus !== '+∞') {
          return callback(new Error('请输入数字'))
        }
      }
      return callback()
    },
    addBtn(title, isWatch, id, val) {
      this.addBox = true
      this.dialogTitle = title
      this.isWatch = isWatch
      this.clickItem = id
      this.showManage = val

      if (title == '编 辑') {
        let {
          compCodeName,
          sectionNameRe,
          evCodeName,
          orientationName,
          paraUnitName,
          /*scopeEvaluationAttention,
          scopeEvaluationWarning,
          scopeEvaluationDanger,*/
          normal,
          attention,
          warning,
          danger,
          evCond,
          power,
          speet
        } = val

        // let compCodeList = getPathByKey(compCodeName, this.compData, 'name')
        let compCodeArr = [compCodeName]
        if (sectionNameRe !== '-') {
          compCodeArr.push(sectionNameRe)
        }
        if (orientationName !== '-') {
          compCodeArr.push(orientationName)
        }

        this.addForm = {
          compCode: compCodeArr,
          sectionName: '',
          orientation: '',
          paraUnit: paraUnitName,
          evName: evCodeName,
          normalValueDown:
            normal === '-'
              ? null
              : normal.split(',').length > 2
              ? ',' + normal.split(',')[1]
              : normal.split(',')[0],
          normalValueUp:
            normal === '-'
              ? null
              : normal.split(',').length > 2
              ? normal.split(',')[2]
              : normal.split(',')[1],
          attentionValueDown:
            attention === '-'
              ? null
              : attention.split(',').length > 2
              ? ',' + attention.split(',')[1]
              : attention.split(',')[0],
          attentionValueUp:
            attention === '-'
              ? null
              : attention.split(',').length > 2
              ? attention.split(',')[2]
              : attention.split(',')[1],
          warningValueDown:
            warning === '-'
              ? null
              : warning.split(',').length > 2
              ? ',' + warning.split(',')[1]
              : warning.split(',')[0],
          warningValueUp:
            warning === '-'
              ? null
              : warning.split(',').length > 2
              ? warning.split(',')[2]
              : warning.split(',')[1],
          dangerValueDown:
            danger === '-'
              ? null
              : danger.split(',').length > 2
              ? ',' + danger.split(',')[1]
              : danger.split(',')[0],
          dangerValueUp:
            danger === '-'
              ? null
              : danger.split(',').length > 2
              ? danger.split(',')[2]
              : danger.split(',')[1],
          powerValueDown: evCond.wkPower.evCondDown === -1 ? undefined : evCond.wkPower.evCondDown,
          powerValueUp: evCond.wkPower.evCondUp === -1 ? undefined : evCond.wkPower.evCondUp,
          speedValueDown: evCond.spd.evCondDown === -1 ? undefined : evCond.spd.evCondDown,
          speedValueUp: evCond.spd.evCondUp === -1 ? undefined : evCond.spd.evCondUp
        }

        this.isCheck = {
          isNormal: normal !== '-',
          isAttention: attention !== '-',
          isWarning: warning !== '-',
          isDanger: danger !== '-',
          isPowerValue: power.split('-')[0] == '' && power.split('-')[1] == '' ? false : true,
          isSpeedValue: speet.split('-')[0] == '' && speet.split('-')[1] == '' ? false : true
        }
        // this.compChange(compCodeList)
      } else if (title == '新 增') {
        this.addForm = {
          compCode: '',
          sectionName: '',
          orientation: '',
          paraUnit: '',
          evName: '',
          attention: undefined,
          warning: undefined,
          danger: undefined,
          powerValueDown: undefined,
          powerValueUp: undefined,
          speedValueDown: undefined,
          speedValueUp: undefined
        }
        this.isCheck = {
          isNormal: false,
          isAttention: false,
          isWarning: false,
          isDanger: false,
          isPowerValue: false,
          isSpeedValue: false
        }
      }
    },

    submitRole: debounce(function () {
      let isValueValid = this.valueValidate()
      let repeatValue = this.repaetValidate()

      if (isValueValid && repeatValue) {
        this.$refs['addRuleForm'].validate(valid => {
          if (valid) {
            let page = {
              total: 0, // 总页数
              currentPage: 1, // 当前页数
              pageSize: 100 // 每页显示多少条
            }
            const param = this.handleAddParam()

            if (this.dialogTitle == '新 增') {
              add({ ...param }).then(res => {
                if (res.data.code == 200) {
                  this.addBox = false
                  this.$message({ type: 'success', message: '部件添加成功！' })
                  this.$emit('onLoad', page)
                }
              })
            } else {
              if (!this.isWatch) {
                param.id = this.showManage.id
                update({ ...param }).then(res => {
                  if (res.data.code == 200) {
                    this.addBox = false
                    this.$message({ type: 'success', message: '部件修改成功！' })
                    this.$emit('onLoad', page)
                  }
                })
              }
            }
          }
        })
      }
    }, 700),
    //保存参数修改
    handleAddParam() {
      let {
        compCode,
        sectionName,
        orientation,
        paraUnit,
        evName,
        powerValueUp,
        powerValueDown,
        speedValueUp,
        speedValueDown,
        normalValueDown,
        normalValueUp,
        attentionValueDown,
        attentionValueUp,
        warningValueDown,
        warningValueUp,
        dangerValueDown,
        dangerValueUp
      } = this.addForm

      let compObj = this.compData.find(j => j.name == compCode[0])
      let sValue = compObj.children.find(j => j.name == compCode[1])
      let oValue = sValue.children.find(j => j.name == compCode[2])

      let valueObj = this.valueList.find(j => j.evName == evName)

      console.log('测量位置选中值', compCode, sValue, oValue)
      // console.log(this.valueList, 'this.valueList')
      // console.log(evName, 'evName')

      let param = {
        alarmModelId: this.clickItem,
        compCode: compObj.code,
        orientation: oValue ? oValue.code : '',
        sectionName: sValue ? sValue.code : '',
        paraUnit: paraUnit,
        evCode: valueObj.evCode,
        /*scopeEvaluationAttention: attention,
        scopeEvaluationWarning: warning,
        scopeEvaluationDanger: danger,*/
        alarmTypeLimits: [
          {
            alarmTypeCode: 'normal',
            alarmThresholdLimitA: normalValueUp === '+∞' ? null : Number(normalValueUp),
            alarmThresholdLimitB: normalValueDown === '-∞' ? null : Number(normalValueDown)
          },
          {
            alarmTypeCode: 'attention',
            alarmThresholdLimitA: attentionValueUp === '+∞' ? null : Number(attentionValueUp),
            alarmThresholdLimitB: attentionValueDown === '-∞' ? null : Number(attentionValueDown)
          },
          {
            alarmTypeCode: 'warning',
            alarmThresholdLimitA: warningValueUp === '+∞' ? null : Number(warningValueUp),
            alarmThresholdLimitB: warningValueDown === '-∞' ? null : Number(warningValueDown)
          },
          {
            alarmTypeCode: 'danger',
            alarmThresholdLimitA: dangerValueUp === '+∞' ? null : Number(dangerValueUp),
            alarmThresholdLimitB: dangerValueDown === '-∞' ? null : Number(dangerValueDown)
          }
        ],
        evCond: {
          wkPower: { evCondUp: powerValueUp, evCondDown: powerValueDown },
          spd: { evCondUp: speedValueUp, evCondDown: speedValueDown }
        }
      }

      function titleCase(str) {
        return str.slice(0, 1).toUpperCase() + str.slice(1).toLowerCase()
      }

      for (let key in param) {
        if (param[key] == '' || param[key] == '-') {
          delete param[key]
        }
        if (key === 'alarmTypeLimits') {
          let objKeyStr = []
          for (const isCheckKey in this.isCheck)
            !this.isCheck[isCheckKey] && objKeyStr.push(isCheckKey)
          param[key] = param[key].filter(
            item => !objKeyStr.includes('is' + titleCase(item.alarmTypeCode))
          )
        }
      }

      return param
    },

    /*/对级联选中的部件找到其径向和轴向
    handlerComp(compArr, nameArr) {
      console.log('根据选中的节点拿到')
      let obj0 = compArr.find(j => j.name == nameArr[0])
      let obj1 = obj0.children.find(j => j.name == nameArr[1])
      if (nameArr.length > 2) {
        let obj2 = obj1.children.find(j => j.name == nameArr[2])
        return obj2
      } else {
        return obj1
      }
    },*/
    //测量位置选中
    compChange(val) {
      /*let compObj = this.handlerComp(this.compData, val)
      this.Slist = []
      this.Olist = []
      if (compObj && compObj.measureList) {
        compObj.measureList.forEach(item => {
          if (item.key == 'section') {
            this.Slist.push(item)
          }
          if (item.key == 'orientation') {
            this.Olist.push(item)
          }
        })
      }
      this.selectValue()*/
      console.log('选中的测量位置', val)
      let param = {
        compName: this.addForm.compCode[0],
        sectionName: this.addForm.compCode[1],
        orientationName: this.addForm.compCode[2]
        // standardUnits: this.addForm.paraUnit
      }

      // for (var key in param) {
      //   if (param[key] == '-' || param[key] == '') {
      //     delete param[key]
      //   }
      // }
      getSelectValue({ ...param }).then(res => {
        if (res.data.code === 200) {
          this.valueList = this.deWeight(res.data.data)
        }
      })
    },
    deWeight(arr) {
      for (let i = 0; i < arr.length - 1; i++) {
        for (let j = i + 1; j < arr.length; j++) {
          if (arr[i].evName == arr[j].evName) {
            arr.splice(j, 1)
            j--
          }
        }
      }
      return arr
    },

    //评估阈值校验
    valueValidate() {
      let auth = true

      /*if (!this.isCheck.isAttention || !this.isCheck.isDanger) {
        auth = false
        this.$message.error('请输入注意、危险阈值！')
      }
      if (
        (this.isCheck.isAttention && this.addForm.attention == undefined) ||
        (this.isCheck.isWarning && this.addForm.warning == undefined) ||
        (this.isCheck.isDanger && this.addForm.danger == undefined)
      ) {
        auth = false
        this.$message.error('请填写勾选阈值的数值！')
      }*/
      if (
        (this.isCheck.isSpeedValue &&
          (this.addForm.speedValueDown == undefined || this.addForm.speedValueUp == undefined)) ||
        (this.isCheck.isPowerValue &&
          (this.addForm.powerValueDown == undefined || this.addForm.powerValueUp == undefined))
      ) {
        auth = false
        this.$message.error('请完成上限/下限！')
      }
      return auth
    },
    //重复校验
    repaetValidate() {
      let auth = true

      let a = this.tableList.find(j => j.compCodeName == this.addForm.compCode)
      let b = this.tableList.find(j => j.orientationName == this.addForm.orientation)
      let c = this.tableList.find(j => j.sectionNameRe == this.addForm.sectionName)
      let d = this.tableList.find(j => j.paraUnitName == this.addForm.paraUnit)
      let e = this.tableList.find(j => j.evCodeName == this.addForm.evName)

      let speet = this.addForm.speedValueDown + '-' + this.addForm.speedValueUp
      let power = this.addForm.powerValueDown + '-' + this.addForm.powerValueUp

      let p = this.tableList.find(j => j.power == power)
      let s = this.tableList.find(j => j.speet == speet)

      if (a && b && c && d && e && p && s) {
        auth = false
        this.$message.error('数据重复，不可保存！')
      }

      return auth
    },

    //多选框是否选中
    clickCheckbox(event, val) {
      if (!event) {
        val.forEach(item => {
          if (['normal', 'warning', 'danger', 'attention'].includes(item)) {
            this.addForm[item + 'ValueDown'] = null
            this.addForm[item + 'ValueUp'] = null
          } else {
            this.addForm[item] = null
          }
        })
      }
    },

    searchGeekIdDown(queryString, callback) {
      callback([
        {
          value: '-∞'
        }
      ])
    },
    searchGeekIdUp(queryString, callback) {
      callback([
        {
          value: '+∞'
        }
      ])
    }
  }
}
</script>

<style lang="scss" scoped>
.form_border {
  border: 1px #283754 solid;
  border-radius: 5px;
  padding: 10px 5px 10px 0;
}
.form_border_threshold {
  label + label {
    margin-top: 10px;
  }
}
:deep(.el-form){
  width: 98% !important;
  .el-form-item {
    margin: 10px 10px 10px 0 !important;
  }
}
:deep(.el-form-item__label){
  text-align: right;
}
.value_select {
  width: 100%;
}
.threshold_value {
  .el-checkbox {
    width: 29%;
    .value_mini {
      width: 70%;
      :deep(.el-input__inner){
        background-color: rgba(71, 86, 128, 0.2) !important;
        color: white;
        height: 38px !important;
      }
    }
  }
}

.waorking_value {
  :deep(.el-checkbox){
    // width: 29%;
    margin: 0px;
    .el-checkbox__label {
      margin-bottom: 10px;
      margin-top: 10px;
      color: white;
    }
  }
  .value_mini {
    width: 18%;
    margin-bottom: 10px;
    :deep(.el-input__inner){
      background-color: rgba(71, 86, 128, 0.2) !important;
      color: white;
      height: 38px !important;
    }
  }
}
</style>
