<template>
  <el-dialog
    :visible="reacordDialogVisible"
    @update:visible="reacordDialogVisible = $event"
    width="1100px"
    v-dialogDrag
    :modal-append-to-body="false"
    :modal="false"
    class="avue-dialog"
    destroy-on-close
    :close-on-click-modal="false"
  >
    <template #title>
      <i class="el-icon-edit" style="margin-right: 10px"></i>分析记录
      <!-- <span>{{this.dialogTitle}}</span> -->
    </template>

    <div class="dialog_body_layout">
      <div
        style="
          width: 100%;
          height: 400px;
          display: flex;
          flex-direction: row;
          justify-content: space-between;
        "
      >
        <div class="form_picture_uplode">
          <ImageEditorDIa v-if="dialogVisible" ref="ImageEditorDIa" :imgUrl="imgUrl" />
        </div>
        <div style="flex: 1; margin-top: 40px; padding-left: 10px">
          <el-form ref="ruleForm" :rules="rules" :inline="false" :model="ruleForm">
            <el-form-item label="分析描述：" prop="analyzerDescribe">
              <el-input
                type="textarea"
                v-model="ruleForm.analyzerDescribe"
                placeholder="请输入分析描述"
                :rows="14"
              ></el-input>
            </el-form-item>
          </el-form>
        </div>
      </div>
      <div style="width: 100%; padding: 10px 15px 0px 15px">
        <el-collapse v-model="activeNames" @change="handleChange">
          <el-collapse-item title="诊断结论及建议" name="1">
            <el-form
              :rules="rules"
              :inline="true"
              :model="ruleForm"
              ref="ruleForm"
              label-width="80px"
              size="small"
            >
              <div class="complist_div" v-for="(item, i) in turbineCompList" :key="item.name">
                <span class="compname">{{ item }}</span>
                <div style="width: 92%; overflow: hidden">
                  <div
                    style="overflow: hidden"
                    v-for="(item1, index) in ruleForm[item]"
                    :key="item1 + index"
                  >
                    <el-col :span="7">
                      <el-form-item
                        :label="index == 0 ? '诊断结论：' : ''"
                        prop="diagnosisConclusion"
                      >
                        <el-autocomplete
                          clearable
                          :trigger-on-focus="true"
                          style="width: 100%"
                          v-model="ruleForm[item][index].diagnosisConclusion"
                          placeholder="请输入诊断结论"
                          @select="
                            value => {
                              handleSelect(item, index, value)
                            }
                          "
                          :fetch-suggestions="
                            (a, b) => {
                              conclusionQuerySearch(a, b, item)
                            }
                          "
                        />
                      </el-form-item>
                    </el-col>
                    <el-col :span="3">
                      <el-form-item
                        :label="index == 0 ? '等级：' : ''"
                        prop="status"
                        class="fault_level"
                      >
                        <el-select
                          style="width: 100%"
                          :key="item1 + index"
                          v-model="ruleForm[item][index].status"
                          placeholder="请选择报警等级"
                          :style="{
                            color: changeFaultTextColor[ruleForm[item][index].status]
                          }"
                        >
                          <el-option
                            v-for="(ii, index) in faultLevelList"
                            :key="index"
                            :label="ii.label"
                            :value="ii.value"
                          >
                            <span :style="{ color: changeFaultTextColor[ii.value] }">{{
                              ii.label
                            }}</span>
                          </el-option>
                        </el-select>
                      </el-form-item>
                    </el-col>
                    <el-col :span="12">
                      <el-form-item :label="index == 0 ? '维护建议：' : ''" prop="maintainAdvice">
                        <el-autocomplete
                          :trigger-on-focus="true"
                          clearable
                          style="width: 100%"
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
            </el-form>
          </el-collapse-item>
        </el-collapse>
      </div>
    </div>

    <template #footer>
      <el-button type="primary" @click="submitForm('ruleForm')" size="small">保存</el-button>
    </template>
  </el-dialog>
</template>

<script>
import { mapGetters } from 'vuex'
import { eventTypeEnum } from '@/util/constant'
import {
  getDefaultDiagResultApi,
  getDefaultAnalysisRecord,
  saveAnalysisRecord
} from '@/api/analysis/unitMerge'

export default {
  props: {
    dialogVisible: {
      type: Boolean,
      default: false
    },
    dialogTitle: String,
    componentFromCompleteData: Object,
    imgUrl: String,
    chartType: String
  },
  components: {
    ImageEditorDIa: () => import('@/components/imgEditor/index.vue') // () => import('./ImageEditorDIa.vue')
  },
  data() {
    return {
      activeNames: '',
      turbineCompList: [],
      restaurants: [],
      reacordDialogVisible: false,
      rules: {
        analyzerDescribe: [{ required: true, message: '请输入分析描述', trigger: 'blur' }]
        /*   compName: [{ required: true, trigger: 'blur' }],
        faultLevel: [{ required: true, message: '请选择报警等级', trigger: 'change' }],
        conclusion: [{ required: true, message: '请输入故障结论', trigger: 'blur' }]  */
      },
      ruleForm: {
        analyzerDescribe: '' // 分析描述
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
      devicesList: [],
      compNameTemp: '',
      conclusionByCompList: [],
      adviceByCompList: [],
      changeFaultTextColor: {
        normal: '#2ED133',
        attention: '#FFE604',
        warning: '#FF6B0E',
        danger: '#FF0F0D'
      },
      tagColorList: [
        '#FF6666',
        '#FF9951',
        '#80C459',
        '#59A56E',
        '#65ADD4',
        '#3D65A1',
        '#372E7C',
        '#5F216D',
        '#903FBF',
        '#BF2F7E'
      ]
    }
  },
  created() {
    getDefaultDiagResultApi().then(res => {
      this.compMaintainList = res.data.data
    })
  },
  watch: {
    dialogVisible: {
      handler: function (val) {
        this.reacordDialogVisible = val
        if (val) {
          this.initData(this.componentFromCompleteData)
        }
      },
      immediate: true
    },
    reacordDialogVisible: {
      handler: function (val) {
        this.$emit('handleDialogVisible', val)
        this.$emit('update:dialogVisible', val)
      }
    }
  },
  computed: {
    ...mapGetters(['manualDiagnosisListFetch'])
  },
  methods: {
    initData(val) {
      const { windturbineId, measlocId, evId } = val
      getDefaultAnalysisRecord({
        WindturbineId: windturbineId,
        MeaslocId: measlocId || '',
        ImageType: this.chartType,
        EigenValueId: evId || ''
      }).then(res => {
        const { analyzeDesc, diagnosisConclusions } = res.data.data
        this.ruleForm.analyzerDescribe = analyzeDesc
        let turbineCompList = []
        diagnosisConclusions.forEach(item => {
          let { compName, diagnosisConclusion, maintainAdvice, status } = item
          if (turbineCompList.indexOf(compName) == -1) {
            turbineCompList.push(compName)
            this.ruleForm[compName] = []
          }
          this.ruleForm[compName].push({
            diagnosisConclusion,
            maintainAdvice,
            status
          })
        })
        this.turbineCompList = turbineCompList
      })
    },
    submitForm(formName) {
      this.$refs[formName].validate(valid => {
        if (valid) {
          const { windturbineId, measlocId, compName, measlocName, evId, time } =
            this.componentFromCompleteData
          let DiagnosisConclusions = []
          this.turbineCompList.forEach(item => {
            this.ruleForm[item].forEach(ii => {
              DiagnosisConclusions.push({
                compName: item,
                diagnosisConclusion: ii.diagnosisConclusion,
                maintainAdvice: ii.maintainAdvice,
                status: ii.status
              })
            })
          })
          if (!this.ruleForm.analyzerDescribe.length) {
            return this.$message.error('请输入分析描述！')
          }
          saveAnalysisRecord({
            windturbineId,
            measlocId,
            compName,
            measlocName,
            EigenValueId: evId || '',
            AcqTime: time || '',
            Image: this.$refs.ImageEditorDIa.saveImage(), //  this.imgUrl,
            ImageType: this.chartType,
            Description: this.ruleForm.analyzerDescribe,
            DiagnosisConclusions
          }).then(res => {
            // 左侧设备树监听刷新功能先不做 2025-06-06
            //  this.$bus.$emit('submitFormInit')
            this.reacordDialogVisible = false
            if (res.data.success) {
              this.$message.success('保存成功！')
            } else {
              this.$message.error(res.data.message)
            }
          })
        } else {
          console.log('error submit!!')
          return false
        }
      })
    },
    resetForm(formName) {
      this.$refs[formName].resetFields()
    },
    querySearch(queryString, cb) {
      let restaurants = this.restaurants
      let results = queryString ? restaurants.filter(this.createFilter(queryString)) : restaurants
      cb(results)
    },
    // 维护建议
    maintainQuerySearch(queryString, cb, level, conclusion) {
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
      /*  this.ruleForm[code][index].status =
        eventTypeEnum[
          this.compMaintainList.find(i => i.diagnosisConclusion == value.value).warningLevel
        ] */
    },
    createFilter(queryString) {
      return restaurant => {
        return restaurant.value.toLowerCase().indexOf(queryString.toLowerCase()) === 0
      }
    },

    // 动态删除行
    cancel(code, listIndex, formIndex) {
      // this.componentList[listIndex].list.splice(formIndex, 1)
      this.ruleForm[code].splice(formIndex, 1)
    },
    // 动态添加行
    addFaultType(code, listIndex) {
      this.ruleForm[code].push({
        diagnosisConclusion: '',
        status: '',
        maintainAdvice: ''
      })
    }
  }
}
</script>

<style lang="less" scoped>
.avue-dialog {
  // --tagColor: #FF6666;
  overflow-y: hidden !important;
  ::v-deep .el-dialog {
    //background: #303030;
    &::before {
      //  background-color: #303030;
      height: 0px;
    }
    .el-dialog__footer {
      &::before {
        //  background-color: #303030;
      }
      &::after {
        // background-color: #303030;
      }
      .dialog-footer {
        /*   right: auto;
        left: 50%;
        bottom: 8px; */
      }
    }
    .el-dialog__header {
      height: 32px;
      .el-dialog__title {
        font-size: 17px;
        font-weight: bold;
        line-height: 17px;
        letter-spacing: 0em;
        color: #e3e3e3;

        top: 10px;
      }
      .el-dialog__headerbtn {
        font-size: 20px;
        height: 40px;
      }
    }
    .el-dialog__body {
      padding: 30px 20px 20px 20px;
      background: #f6f6f6 !important;
    }
  }
  .dialog_body_layout {
    display: flex;
    flex-direction: column;
    height: calc(100% - 24px);
    width: 100%;
    padding: 0 10px;

    .form_picture_uplode {
      width: 900px;
    }
    .comp_info_label {
      display: flex;
      flex-direction: row;
      flex-wrap: wrap;
      span {
        margin: 5px;
        color: white;
      }
    }
    ::v-deep .el-collapse-item__header {
      font-size: 15px;
      height: 30px;
      font-weight: bolder;
      background: #f6f6f6;
    }
    ::v-deep .el-form-item {
      width: 100%;
      margin-bottom: 6px;
      margin-right: 0px;
    }
    ::v-deep .el-form-item__content {
      width: 95%;
    }
    ::v-deep .el-form-item__label {
      font-size: 12px;
    }
    ::v-deep .el-textarea__inner {
      background: #fff;
    }
    ::v-deep .el-input__inner {
      background: #fff;
    }
  }

  .fault_level {
    ::v-deep .el-input__inner {
      color: inherit;
      // color: var(--inputText);
    }
  }
  ::v-deep .add_advice {
    .el-tag,
    .el-tag.el-tag--info {
      background-color: var(--tagColor);
      border-color: var(--tagColor);
    }
  }
}
/* ::v-deep .el-textarea {
  width: 82%;
} */
.icon-button {
  font-size: 18px;
  cursor: pointer;
  margin: 40px 5px 0 5px;
  &:hover {
    color: #409eff;
  }
}
.complist_div {
  width: 100%;
  height: auto;
  display: flex;
  flex-direction: row;
  justify-content: space-around;
  align-items: center;
  margin: 10px 0;
  .compname {
    font-size: 14px;
    font-weight: bolder;
  }
}
::v-deep .el-dialog__footer {
  text-align: center;
  background: #f6f6f6;
}
::v-deep .el-collapse-item__wrap {
  border-bottom: none;
  background: #f6f6f6;
}
</style>
<style lang="less" scoped>
.avue-dialog {
  .el-collapse-item__header {
    font-size: 15px;
    height: 30px;
  }
  .el-collapse-item__wrap {
    border-bottom: none;
    background: #f6f6f6;
  }
  .el-collapse-item__content {
    padding-bottom: 0px;
  }
}
</style>
