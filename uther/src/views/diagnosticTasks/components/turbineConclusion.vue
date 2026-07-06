<template>
  <div class="content_block" style="width: 100%">
    <h3>诊断结论及建议</h3>
    <el-form
      :rules="rules"
      :inline="true"
      :model="ruleForm"
      ref="ruleForm"
      label-width="80px"
      size="small"
      style="width: 100%; height: 100%"
    >
      <div class="complist_status">
        <div class="complist_div" v-for="(item, i) in turbineCompList" :key="item.name">
          <div class="complist_div_title">
            <!--  <span class="compname">{{ item.name }}</span> -->
            <el-form-item
              :label="item"
              style="font-weight: bolder; font-size: 14px"
              prop="status"
              class="fault_level"
            >
              <el-select
                disabled
                v-model="ruleForm[item + 'Level']"
                :style="{
                  width: '100%',
                  color: changeFaultTextColor[ruleForm[item + 'Level']]
                }"
              >
                <el-option
                  v-for="(j, index) in faultLevelList"
                  :key="index"
                  :label="j.label"
                  :value="j.value"
                >
                  <span :style="{ color: changeFaultTextColor[j.value] }">{{ j.label }}</span>
                </el-option>
              </el-select>
            </el-form-item>
          </div>
          <div style="width: 100%; padding: 0 15px">
            <div
              style="
                overflow: hidden;
                width: 100%;
                border: 1px solid #eee;
                padding-top: 8px;
                margin-bottom: 5px;
              "
              v-for="(item1, index) in ruleForm[item]"
              :key="item1 + index"
            >
              <el-col :span="22">
                <el-col :span="20">
                  <el-form-item label="诊断结论：" prop="diagnosisConclusion">
                    <el-autocomplete
                      clearable
                      :trigger-on-focus="true"
                      style="width: 100%"
                      v-model="ruleForm[item][index].diagnosisConclusion"
                      :fetch-suggestions="
                        (a, b) => {
                          conclusionQuerySearch(a, b, item)
                        }
                      "
                      placeholder="请输入诊断结论"
                      @select="
                        value => {
                          handleSelect(item, index, value)
                        }
                      "
                    />
                  </el-form-item>
                </el-col>
                <el-col :span="4" class="fault_level_width">
                  <el-form-item label-width="0px" label="" prop="status" class="fault_level">
                    <el-select
                      v-model="ruleForm[item][index].status"
                      placeholder="请选择报警等级"
                      :style="{
                        color: changeFaultTextColor[ruleForm[item][index].status]
                      }"
                      @change="faultLevelChange(item)"
                    >
                      <el-option
                        v-for="(i, index) in faultLevelList"
                        :key="index"
                        :label="i.label"
                        :value="i.value"
                      >
                        <span :style="{ color: changeFaultTextColor[i.value] }">{{ i.label }}</span>
                      </el-option>
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="24">
                  <el-form-item label="维护建议：" prop="maintainAdvice">
                    <el-autocomplete
                      style="width: 100%"
                      :trigger-on-focus="true"
                      clearable
                      v-model="ruleForm[item][index].maintainAdvice"
                      :fetch-suggestions="
                        (a, b) => {
                          maintainQuerySearch(
                            a,
                            b,
                            ruleForm[item][index].status,
                            ruleForm[item][index].diagnosisConclusion
                          )
                        }
                      "
                      placeholder="请输入维护建议"
                    />
                  </el-form-item>
                </el-col>
              </el-col>
              <el-col :span="2">
                <i
                  v-if="ruleForm[item].length > 1"
                  type="primary"
                  @click="cancel(item, i, index)"
                  size="small"
                  title="删除"
                  class="el-icon-remove-outline icon-button"
                  :style="{ marginTop: index == 0 ? '40px' : '5px' }"
                ></i>
                <i
                  v-if="index == ruleForm[item].length - 1"
                  type="primary"
                  @click="addFaultType(item, i)"
                  size="small"
                  title="添加"
                  class="el-icon-circle-plus-outline icon-button"
                  :style="{ marginTop: index == 0 ? '40px' : '5px' }"
                ></i>
              </el-col>
            </div>
          </div>
        </div>
      </div>
      <!--   机组运行状态 -->
      <div class="turbine_status">
        <el-form-item label-width="120px" label="机组健康状态：" prop="status" class="fault_level">
          <el-select
            disabled
            v-model="ruleForm.turbinestate"
            :style="{
              width: '100%',
              color: changeFaultTextColor[ruleForm.turbinestate]
            }"
          >
            <el-option
              v-for="(item, index) in faultLevelList"
              :key="index"
              :label="item.label"
              :value="item.value"
            >
              <span :style="{ color: changeFaultTextColor[item.value] }">{{ item.label }}</span>
            </el-option>
          </el-select>
        </el-form-item>
        <el-form-item label-width="120px" label="运行建议：" prop="runingAdvice">
          <el-autocomplete
            style="width: 100%"
            v-model="ruleForm.runingAdvice"
            :fetch-suggestions="adviceQuerySearch"
            placeholder="请输入运行建议"
            @select="handleRunAdviceSelect"
          />
        </el-form-item>
      </div>
    </el-form>
  </div>
</template>

<script>
import { saveDiagnosticRecordList } from '@/api/analysis/index'
import { eventTypeEnum } from '@/util/constant'
import { getDefaultDiagResultApi, getDefaultRuningAdviceApi } from '@/api/analysis/unitMerge'

export default {
  computed: {},
  data() {
    return {
      rules: {
        //runingAdvice: [{ required: true, trigger: 'blur' }]
        // compName: [{ required: true, trigger: 'blur' }],
        // faultLevel: [{ required: true, message: '请选择报警等级', trigger: 'change' }],
        //  conclusion: [{ required: true, message: '请输入故障结论', trigger: 'blur' }]
      },
      turbineCompList: [],
      ruleForm: {
        runingAdvice: '', // 运行建议
        turbinestate: 'normal'
      },
      faultLevelList: [
        {
          label: '正常',
          value: 'normal'
        },
        {
          label: '注意',
          value: 'attention'
        },
        {
          label: '警告',
          value: 'warning'
        },
        {
          label: '危险',
          value: 'danger'
        }
      ],
      changeFaultTextColor: {
        normal: '#2ED133',
        attention: '#FFE604',
        warning: '#FF6B0E',
        danger: '#FF0F0D'
      },
      turbinerunDict: {}
    }
  },
  created() {
    getDefaultDiagResultApi().then(res => {
      this.compMaintainList = res.data.data
    })
    getDefaultRuningAdviceApi().then(res => {
      if (res.data.data && res.data.data.length > 0) {
        let turbineDict = {}
        res.data.data.forEach(item => {
          let { runingAdvice, warningLevel } = item
          if (warningLevel in turbineDict) {
            turbineDict[warningLevel].push(runingAdvice)
          } else {
            turbineDict[warningLevel] = [runingAdvice]
          }
        })
        this.turbinerunDict = turbineDict
      }
    })
  },
  methods: {
    faultLevelChange(compName) {
      let faultLevelList = this.ruleForm[compName].map(item => item.status)
      this.ruleForm[compName + 'Level'] = this.getLevel(faultLevelList)
      let compLevelList = []
      Object.keys(this.ruleForm).forEach(key => {
        if (key.includes('Level')) {
          compLevelList.push(this.ruleForm[key])
        }
      })
      this.ruleForm.turbinestate = this.getLevel(compLevelList)
      this.turbineStatusChange(this.ruleForm.turbinestate)
    },
    // 取报警等级最高级
    getLevel(list) {
      if (list.includes('danger')) {
        return 'danger'
      } else if (list.includes('warning')) {
        return 'warning'
      } else if (list.includes('attention')) {
        return 'attention'
      } else {
        return 'normal'
      }
    },
    turbineStatusChange(val) {
      this.ruleForm.runingAdvice = this.turbinerunDict[eventTypeEnum[val]][0]
    },
    initData(data) {
      this.ruleForm = {}
      const { conclusions, runingAdvice, status } = data
      this.ruleForm.runingAdvice = runingAdvice
      this.ruleForm.turbinestate = status
      let turbineCompList = []
      conclusions.forEach(item => {
        let { compStatus, name, children } = item
        turbineCompList.push(name)
        this.ruleForm[name + 'Level'] = compStatus
        this.ruleForm[name] = []
        children.forEach(ii => {
          let { compName, diagnosisConclusion, maintainAdvice, status } = ii
          this.ruleForm[compName].push({
            diagnosisConclusion,
            maintainAdvice,
            status
          })
        })
      })
      this.turbineCompList = turbineCompList
    },
    // 添加故障类型
    addFaultType(code, listIndex) {
      this.ruleForm[code].push({
        diagnosisConclusion: '',
        status: '',
        maintainAdvice: ''
      })
    },
    // 删除故障类型
    cancel(code, listIndex, formIndex) {
      this.ruleForm[code].splice(formIndex, 1)
      this.faultLevelChange(code)
    },
    // 机组运行建议
    adviceQuerySearch(queryString, cb) {
      let adviceByCompList = Array.from(
        this.turbinerunDict[eventTypeEnum[this.ruleForm.turbinestate]],
        item => ({
          label: item,
          value: item
        })
      )
      let results = queryString
        ? adviceByCompList.filter(this.createFilter(queryString))
        : adviceByCompList
      cb(results)
    },
    // 诊断结论
    conclusionQuerySearch(queryString, cb, compName) {
      let conclusionByCompList = Array.from(
        this.compMaintainList.filter(i => i.compName === compName),
        item => ({ label: item.diagnosisConclusion, value: item.diagnosisConclusion })
      )
      let results = queryString
        ? conclusionByCompList.filter(this.createFilter(queryString))
        : conclusionByCompList
      cb(results)
    },
    handleSelect(code, index, value) {
      this.ruleForm[code][index].status =
        eventTypeEnum[
          this.compMaintainList.find(i => i.diagnosisConclusion == value.value).warningLevel
        ]
      this.faultLevelChange(code)
    },
    handleRunAdviceSelect(val) {
      // console.log(val)
      // console.log(this.ruleForm.runingAdvice)
    },

    // 维护建议
    maintainQuerySearch(queryString, cb, level, conclusion) {
      // console.log(eventTypeEnum[level])
      let maintainByCompList = Array.from(
        this.compMaintainList.filter(
          i => i.warningLevel === eventTypeEnum[level] && i.diagnosisConclusion === conclusion
        ),
        item => ({ label: item.maintainAdvice, value: item.maintainAdvice })
      )
      let results = queryString
        ? maintainByCompList.filter(this.createFilter(queryString))
        : maintainByCompList
      cb(results)
    },
    createFilter(queryString) {
      return restaurant => {
        return restaurant.value.toLowerCase().indexOf(queryString.toLowerCase()) > -1
      }
    },
    hanldeRestaurants(val, key = 'restaurants', opt) {
      if (val.length) {
        const restaurantsTemp = val.map(item => {
          return {
            value: item[opt],
            label: item[opt]
          }
        })
        this[key] = Array.from(new Set(restaurantsTemp.map(item => item.value))).map(item => {
          return {
            value: item,
            label: item
          }
        })
      }
    },
    submitForm(formName) {
      this.$refs[formName].validate(valid => {
        if (valid) {
          this.ruleForm.compName = this.ruleForm.compName
          this.ruleForm.imagesBytes = this.$refs.ImageEditorDIa.saveImage() //this.$refs.ImageEditorDIa.handleSaveBlob()
          this.ruleForm.tags = this.ruleForm.tagsTemp.join(',')
          saveDiagnosticRecordList(this.ruleForm).then(() => {
            // this.$bus.$emit('submitFormInit')
            this.reacordDialogVisible = false
            this.$message({
              message: '保存成功！',
              type: 'success'
            })
          })
        } else {
          console.log('error submit!!')
          return false
        }
      })
    },
    resetForm(formName) {
      this.$refs[formName].resetFields()
    }
  }
}
</script>

<style lang="scss" scoped>
.content_block {
  width: 100%;
  height: 100%;
  overflow: hidden;
  padding: 10px 15px;
  .complist_status {
    width: 100%;
    height: calc(100% - 150px);
    overflow-x: hidden;
    overflow-y: auto;
    margin-bottom: 10px;
    box-shadow: inset 0 0 6px #eee;
  }
  .turbine_status {
    width: 100%;
    height: 150px;
    overflow: hidden;
    :deep(.el-form-item){
      width: 100%;
      margin-bottom: 6px;
      margin-right: 0px;
    }
    :deep(.el-form-item__content){
      width: calc(100% - 120px);
    }
    :deep(.el-form-item__label){
      font-size: 14px;
      font-weight: bolder;
    }
    .fault_level {
      :deep(.el-form-item__content){
        width: auto;
      }
    }
  }
  h3 {
    width: 100%;
    font-size: 14px;
  }

  .complist_div {
    width: 100%;
    height: auto;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    align-items: flex-start;
    margin: 10px 0;
    .complist_div_title {
      width: 100%;
      display: flex;
      flex-direction: row;
      align-items: flex-start;
      justify-content: flex-start;
      :deep(.el-form-item){
        width: 30%;
        margin-bottom: 6px;
        margin-right: 0px;
      }
    }
    .compname {
      font-size: 14px;
      font-weight: bolder;
      margin-right: 10px;
      display: block;
    }
    .icon-button {
      font-size: 18px;
      cursor: pointer;
      margin: 40px 5px 0 5px;
      &:hover {
        color: #409eff;
      }
    }
    :deep(.el-form-item){
      width: 100%;
      margin-bottom: 6px;
      margin-right: 0px;
    }
    :deep(.el-form-item__content){
      width: calc(100% - 80px);
    }
    :deep(.el-form-item__label){
      font-size: 12px;
    }
    .fault_level_width {
      :deep(.el-form-item__content){
        width: 100%;
      }
    }
  }
  .fault_level {
    :deep(.el-input__inner){
      color: inherit;
      // color: var(--inputText);
    }
  }
  .el-col {
    margin-bottom: 5px;
  }
}
</style>
