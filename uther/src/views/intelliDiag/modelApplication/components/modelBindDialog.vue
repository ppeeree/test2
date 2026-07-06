<template>
  <el-dialog
    title="机组绑定模型"
    v-model="dialogVisible"
    width="1000px"
    @close="handleClose"
    destroy-on-close
    :close-on-click-modal="false"
    custom-class="bind-model-dialog"
  >
    <el-form ref="formRef" style="padding: 20px" :model="formData" label-width="220px" size="small">
      <!-- 循环渲染故障部位 -->
      <el-row>
        <el-col :span="12" v-for="(item, index) in faultList" :key="index">
          <el-form-item :label="item.label">
            <el-select v-model="formData[item.prop]" placeholder="无" clearable style="width: 100%">
              <!-- 循环渲染可选模型 -->
              <el-option
                v-for="model in item.options"
                :key="model.id"
                :label="model.name"
                :value="model.id"
              >
                <!-- 自定义选项内容，模拟截图中的样式 -->
              </el-option>
            </el-select>
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button type="primary" size="small" style="margin-right: 10px" @click="handleSave"
          >保存</el-button
        >
        <el-button size="small" @click="handleCancel">取消</el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script>
export default {
  name: 'BindModelDialog',
  props: {
    // 控制弹窗显示隐藏
    visible: {
      type: Boolean,
      default: false
    },
    // 传入当前行的数据（用于回显）
    rowData: {
      type: Object,
      default: () => ({})
    },
    modelTree: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      dialogVisible: false,
      // 定义表单结构：label为显示名称，prop为字段名
      faultList: [
        { label: '发电机驱动端轴承内圈故障:', prop: 'seletItem1', options: [] },
        { label: '发电机非驱动端轴承内圈故障:', prop: 'seletItem2', options: [] },
        { label: '发电机驱动端轴承外圈故障:', prop: 'seletItem3', options: [] },
        { label: '发电机非驱动端轴承外圈故障:', prop: 'seletItem4', options: [] },
        { label: '发电机驱动端轴承保持架故障:', prop: 'seletItem5', options: [] },
        { label: '发电机非驱动端轴承保持架故障:', prop: 'seletItem6', options: [] },
        { label: '发电机驱动端轴承滚动体故障:', prop: 'seletItem7', options: [] },
        { label: '发电机非驱动端轴承滚动体故障:', prop: 'seletItem8', options: [] },
        { label: '齿轮箱平行级高速轴齿轮故障:', prop: 'seletItem9', options: [] }
      ],
      formData: {}
    }
  },
  watch: {
    visible(val) {
      this.dialogVisible = val
      if (val) {
        this.initForm()
      }
    },
    dialogVisible(val) {
      this.$emit('update:visible', val)
    },
    modelTree(val) {
      this.initOptions()
    }
  },
  mounted() {},
  methods: {
    initOptions() {
      const filterByKeyword = (children, keyword) => {
        const filtered = children.filter(child => child.name.includes(keyword))
        return filtered.length > 0 ? filtered : children
      }
      // 轴承类型关键字与其基础索引(偶数)的映射
      const bearingTypes = {
        内圈: 0,
        外圈: 2,
        保持架: 4,
        滚动体: 6
      }
      this.modelTree.forEach(item => {
        if (item.name.includes('齿轮箱')) {
          this.faultList[8].options = item.children
        }
        if (item.name.includes('发电机轴承')) {
          for (const [keyword, baseIndex] of Object.entries(bearingTypes)) {
            if (item.name.includes(keyword)) {
              this.faultList[baseIndex].options = filterByKeyword(item.children, '发电机驱动端')
              this.faultList[baseIndex + 1].options = filterByKeyword(
                item.children,
                '发电机非驱动端'
              )
            }
          }
        }
      })
    },

    // 初始化表单数据
    initForm() {
      const form = {}
      // 初始化所有字段为空数组
      this.faultList.forEach(item => {
        form[item.prop] = ''
      })
      // 如果有传入 rowData，则进行回显
      if (this.rowData && this.rowData.linkModelIDs && this.rowData.linkModelIDs.length > 0) {
        this.faultList.forEach(item => {
          // 假设 rowData 中存储的是逗号分隔的字符串 "id1,id2" 或者数组
          const val = this.rowData.linkModelIDs.find(i =>
            item.options.some(option => option.id === i)
          )
          form[item.prop] = val || ''
        })
      }
      this.formData = form
    },

    handleSave() {
      // 1. 获取 formData 中所有的属性值，返回一个数组
      let valuesArray = Object.values(this.formData)
      valuesArray = valuesArray.filter(item => item !== '')
      this.$emit('save', valuesArray)
      this.dialogVisible = false
    },

    handleCancel() {
      this.dialogVisible = false
    },

    handleClose() {
      this.$refs.formRef && this.$refs.formRef.resetFields()
    }
  }
}
</script>

<style scoped>
/* 局部样式 */
:deep(.el-select__tags){
  max-width: 95%; /* 防止标签过多撑破布局 */
}
</style>

<style>
/* 全局样式，用于修改下拉菜单内部样式 */
/* .custom-select-dropdown .el-select-dropdown__item {
  height: auto;
  line-height: 20px;
  padding: 8px 20px;
} */

/* 模拟截图中蓝色的文字效果 */
.custom-option-text {
  color: #409eff; /* Element UI 主题蓝 */
  font-size: 13px;
  display: block;
  white-space: normal; /* 允许长文本换行 */
  word-break: break-all;
}

/* 选中状态的高亮背景 */
/* .custom-select-dropdown .el-select-dropdown__item.selected .custom-option-text {
  color: #000; /* 选中时文字变白 
} */
.dialog-footer {
  width: 100%;
  text-align: center;
  position: inherit;
}
.el-dialog__footer .dialog-footer {
  position: inherit;
}

/* 普通未选中项如果是黑色，可以在这里覆盖，但通常 el-select 默认就是黑色 */
/* 如果想让未选中的也是蓝色，保持上面那个类即可 */
</style>
