<template>
  <div class="data-config-container">
    <!-- model 绑定本地副本 localList -->
    <el-form :model="{ list: localList }" ref="configFormRef" label-width="80px">
      <div v-for="(item, index) in localList" :key="index" class="config-row">
        <el-row :gutter="10" type="flex" align="middle">
          <!-- ...省略部分结构代码... -->
          <el-col :span="4">
            <div class="priority-label">优先级: {{ index + 1 }}</div>
          </el-col>

          <el-col :span="11">
            <el-form-item label="测量位置：" :prop="'list.' + index + '.measLocationCode'"
              :rules="[{ required: true, message: '请选择', trigger: 'change' }]">
              <el-select size="small" v-model="item.measLocationCode" placeholder="请选择" style="width: 100%"
                :multiple="isMultipleSelection" @change="handleItemChange">
                <el-option v-for="opt in options" :key="opt.key" :label="opt.value" :value="opt.key"></el-option>
              </el-select>
            </el-form-item>
          </el-col>

          <el-col :span="7">
            <el-form-item label="采样频率：" :prop="'list.' + index + '.sampleRate'"
              :rules="[{ required: true, message: '请输入', trigger: 'blur' }]">
              <el-input-number size="small" v-model="item.sampleRate" controls-position="right" style="width: 100%"
                :min="0" @change="handleItemChange"></el-input-number>
            </el-form-item>
          </el-col>

          <el-col :span="2" style="text-align: right">
            <el-button type="danger" icon="el-icon-delete" circle size="mini" :disabled="localList.length <= 1"
              @click="removeRow(index)"></el-button>
          </el-col>
        </el-row>
      </div>
    </el-form>

    <div class="add-btn-wrapper">
      <el-button type="text" icon="el-icon-plus" @click="addRow">添加数据</el-button>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    list: {
      type: Array,
      default: () => []
    },
    isMultipleSelection: Boolean,
    options: Array
  },
  data() {
    return {
      // 使用本地副本，避免直接修改 props
      localList: []
    }
  },
  watch: {
    // 监听外部 props 的变化，同步到本地副本
    list: {
      handler(newVal) {
        // 深拷贝防止引用污染，仅在初始化或父组件重置时更新
        // 注意：这里不要每次都 deep clone，否则输入框会失去焦点
        if (newVal && newVal.length !== this.localList.length) {
          this.localList = JSON.parse(JSON.stringify(newVal || []))
        } else if (!this.localList.length && newVal) {
          this.localList = JSON.parse(JSON.stringify(newVal))
        }
      },
      immediate: true,
      deep: true
    }
  },
  methods: {
    // 统一处理变更，通知父组件
    handleItemChange() {
      // 这里的 emit 是为了让父组件知道数据变了（用于保存等）
      // 但由于我们用的是 data 里的 localList，这不会触发 computed set 的死循环
      this.$emit('update:list', this.localList)
    },
    addRow() {
      const newItem = {
        modelID: '',
        ...this.list[0],
        measLocationCode: '',
        sampleRate: undefined
      }
      this.localList.push(newItem)
      this.handleItemChange()
    },
    removeRow(index) {
      this.localList.splice(index, 1)
      this.handleItemChange()
    },
    validate() {
      return new Promise((resolve, reject) => {
        this.$refs.configFormRef.validate(valid => {
          valid ? resolve(true) : reject(false)
        })
      })
    },
    clearValidate() {
      this.$refs.configFormRef?.clearValidate()
    }
  }
}
</script>

<style scoped lang="scss">
.config-row {
  padding: 10px;
  border-radius: 4px;

  .el-col {
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: row;
  }
}

.priority-label {
  color: #606266;
  font-size: 13px;
}

.add-btn-wrapper {
  text-align: center;
  overflow: hidden;

  .el-button {
    width: 100%;
    padding: 0;
  }
}

.el-form-item {
  margin-bottom: 0;
}
</style>
