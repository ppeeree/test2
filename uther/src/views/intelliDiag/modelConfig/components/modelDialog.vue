<template>
  <el-dialog :title="dialogTitle" v-drag :modal="false" :visible.sync="visible" width="900px" @close="handleClose"
    destroy-on-close>
    <div class="dialog-content">
      <el-form :model="formData" :rules="rules" ref="formRef" label-width="120px" size="small">
        <!-- 1. 基础信息区域 -->
        <el-divider content-position="left"><i class="el-icon-document"></i> {{ modelTypeInfo.name }}</el-divider>
        <el-row :gutter="20">
          <el-col :span="10" v-if="modelTypeInfo.name?.indexOf('发电机') > -1">
            <el-form-item label="诊断模型名称前缀：" prop="modelName1" label-width="150px">
              <el-input disabled v-model="formData.modelName1" placeholder="请输入模型名称"></el-input>
            </el-form-item>
          </el-col>
          <el-col :span="14">
            <el-form-item :label="modelTypeInfo.name?.indexOf('发电机') > -1 ? '主体：' : '诊断模型名称：'" prop="modelName2"
              label-width="120px">
              <el-input v-model="formData.modelName2" placeholder="请输入模型名称"></el-input>
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="模型描述：" prop="description">
          <el-input type="textarea" v-model="formData.description" placeholder="请输入描述" rows="1"></el-input>
        </el-form-item>

        <!-- 2. 模型参数区域 (动态生成) -->
        <el-divider content-position="left"><i class="el-icon-document"></i> 模型参数</el-divider>
        <el-row :gutter="20">
          <el-col :span="8" v-for="(value, key) in formData.params" :key="key">
            <el-form-item :label="key + ':'" label-width="80px" :prop="'params.' + key"
              :rules="[{ required: true, message: '请输入参数,数值>0', trigger: 'blur' }]">
              <template v-if="typeof value === 'number'">
                <!--  :disabled="isEdit" -->
                <el-input-number v-model="formData.params[key]" controls-position="right" style="width: 100%"
                  :min="0"></el-input-number></template>
              <template v-else>
                <el-input :disabled="key === 'p1'" v-model="formData.params[key]" placeholder="请输入参数"></el-input>
              </template>
            </el-form-item>
          </el-col>
        </el-row>

        <!-- 3. 数据配置区域 -->
        <el-divider content-position="left"><i class="el-icon-document"></i> 数据配置</el-divider>
        <!-- ================= 3. 底部：动态数据配置 (支持增删) ================= -->
        <data-config-list ref="configListRef" :list.sync="formData.dataRules" :isMultipleSelection="isMultipleSelection"
          :options="measLocationCodes" />
      </el-form>
    </div>
    <!-- 底部按钮区 -->
    <div slot="footer" class="dialog-footer">
      <el-button size="small" type="primary" style="margin-right: 10px" @click="submitForm" :loading="loading">保
        存</el-button>
      <el-button size="small" @click="handleClose">取 消</el-button>
    </div>
  </el-dialog>
</template>

<script>
import DataConfigList from './dataConfigList.vue'
import { getModelParamsApi, saveModelParamsApi } from '@/api/intelliDiag'
export default {
  name: 'ModelDialog',
  components: {
    DataConfigList
  },
  props: {
    // 控制弹窗显示
    show: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      isEdit: false,
      modelTypeInfo: {},
      loading: false,
      // 内部维护的表单数据
      formData: {
        modelType: '',
        modelID: '',
        modelName1: '',
        modelName2: '',
        description: '',
        params: {},
        // 动态的数据配置列表
        dataRules: []
      },
      rules: {
        modelName2: [{ required: true, message: '请输入模型名称', trigger: 'blur' }]
      }
    }
  },
  computed: {
    visible: {
      get() {
        return this.show
      },
      set(val) {
        this.$emit('update:show', val)
      }
    },
    dialogTitle() {
      return this.isEdit ? '编辑模型' : '新增模型'
    }
  },
  watch: {
    'formData.dataRules': {
      handler(val) {
        if (this.modelTypeInfo.name?.indexOf('发电机') > -1 && val.length > 0) {
          const firstCode = val[0].measLocationCode
          // 2. 使用 find 代替 forEach，找到即停止，性能更好
          const matchedItem = this.measLocationCodes.find(item => item.key === firstCode)
          if (matchedItem) {
            const newName = this.getModelName1(matchedItem.value)
            // 3. 直接赋值即可，无需 $set（因为 modelName1 在 data 中已预定义）
            // 如果确实想用 $set，也可以：this.$set(this.formData, 'modelName1', newName);
            this.formData.modelName1 = newName
          }
        }
      },
      deep: true
    }
  },
  mounted() { },
  methods: {
    // 重置表单
    resetForm() {
      this.formData = {
        modelName1: '',
        modelName2: '',
        description: '',
        modelType: '',
        modelID: '',
        // 模拟的固定参数 P1-P15
        params: {},
        // 动态的数据配置列表
        dataRules: []
      }
      this.$nextTick(() => {
        if (this.$refs.formRef) {
          this.$refs.formRef.resetFields()
        }
      })
    },
    // 初始化数据逻辑
    async initData(modelTypeInfo, row) {
      this.modelTypeInfo = modelTypeInfo
      this.isEdit = row ? true : false
      getModelParamsApi({
        modelId: this.isEdit ? row.id : '',
        modelType: this.isEdit ? '' : modelTypeInfo.id
      }).then(res => {
        const { success, message, data } = res.data
        if (success) {
          const { isMultipleSelection, measLocationCodes, modelName, ...others } = data
          Object.assign(this.formData, others)
          let modelName1 = this.getModelName1(modelName)
          this.formData.modelName1 = this.isEdit ? modelName1 : ''
          this.formData.modelName2 = this.isEdit ? modelName.substring(modelName1.length) : ''
          this.isMultipleSelection = isMultipleSelection
          this.measLocationCodes = measLocationCodes
        } else {
          this.$message.error(message)
        }
      })
    },
    getModelName1(name) {
      let modelName1 = ''
      if (name.indexOf('发电机驱动端') > -1) {
        modelName1 = '发电机驱动端'
      } else if (name.indexOf('发电机非驱动端') > -1) {
        modelName1 = '发电机非驱动端'
      }
      return modelName1
    },
    async submitForm() {
      try {
        // 1. 校验主表单
        const mainValid = await this.$refs.formRef.validate()
        // 2. 校验子组件的数据配置表单
        const configValid = await this.$refs.configListRef.validate()

        if (mainValid && configValid) {
          this.loading = true
          // 构造提交给后端的数据结构
          const submitData = {
            ...this.formData,
            modelName: this.formData.modelName1 + this.formData.modelName2,
            dataRules: this.formData.dataRules.map((item, i) => ({
              ...item,
              priority: i + 1
            }))
          }

          const res = await saveModelParamsApi(submitData)
          const { success, message } = res.data
          this.loading = false
          if (success) {
            this.$message.success(message)
            this.$emit('success', submitData)
            this.visible = false
          } else {
            this.$message.error(message)
          }
        }
      } catch (error) {
        // 如果校验失败，会进入 catch，无需额外处理，Element UI 会自动显示红字
        console.log('表单校验未通过')
      }
    },

    handleClose() {
      this.resetForm()
      this.visible = false
    }
  }
}
</script>

<style scoped lang="scss">
.dialog-content {
  width: 100%;
  padding: 0 20px;
  height: 65vh;
  overflow-y: auto;
}

.dialog-content .el-form-item {
  margin-bottom: 18px;
}

.dialog-content .el-form-item__label {
  font-size: 14px;
  color: #606266;
}

.dialog-content .el-form-item__content {}

.dialog-footer {
  text-align: center;
  margin-top: 20px;
}

/* 调整 Radio 组在 FormItem 中的对齐 */
.el-form-item--small.el-form-item {
  margin-bottom: 18px;
}

::v-deep .el-radio__label {
  color: #303133;
}

.el-divider__text {
  font-size: 14px;
  color: #606266;
}

.el-col {
  margin: 0;
}

.dialog-footer {
  width: 100%;
  text-align: center;
  position: inherit;
}

.el-dialog__footer .dialog-footer {
  position: inherit;
}
</style>
