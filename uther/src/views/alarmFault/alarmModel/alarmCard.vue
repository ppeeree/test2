<template>
  <div class="alarm_card">
    <div class="alarm_title">报警模型</div>
    <div class="determin_tree">
      <el-tree
        :data="determineModelList"
        :props="defaultProps"
        @node-click="handleNodeClick"
        default-expand-all
        :expand-on-click-node="false"
        v-if="determineModelList.length"
        :indent="16"
      >
        <span class="custom-tree-node" slot-scope="{ data }">
          <span
            class="height_light"
            :style="{ background: data.isClick ? '#3ffff9' : 'transparent' }"
          ></span>
          <span>{{ data.modelName }}</span>
        </span>
      </el-tree>
      <noData v-else />
    </div>
    <div class="custom_tree">
      <div class="custom_tree_title">自定义报警模型</div>
      <div class="custom_tree_content">
        <el-tree
          :data="customModelList"
          :props="defaultProps"
          @node-click="handleNodeClick"
          default-expand-all
          :expand-on-click-node="false"
          v-if="customModelList.length"
        >
          <span class="custom-tree-node" slot-scope="{ data }">
            <span
              class="height_light"
              :style="{
                background: data.isClick ? '#3ffff9' : 'transparent',
                left: data.versions !== '' ? '-23px' : '-41px'
              }"
            ></span>
            <span
              >{{ data.modelName }}<span v-show="data.versions !== ''">_</span
              >{{ data.versions }}</span
            >
          </span>
        </el-tree>
        <noData v-else />
      </div>
      <div style="float: right; margin-right: 10px; margin-top: 5px">
        <el-button type="primary" @click="importBox = true">导入报警模型</el-button>
        <el-button type="primary" @click="addBox = true">新建报警模型</el-button>
        <el-button type="primary" style="width: 60px" @click="deleteAlarmModel">删除</el-button>
      </div>
    </div>
    <el-dialog
      title="导入报警模型"
      append-to-body
      :visible.sync="importBox"
      width="450px"
      v-dialogDrag
      ref="ruleForm"
    >
      <div class="dialog_content">
        导入文件：
        <el-upload
          ref="upload"
          action="/api/wtphm-service/alarmDefine/upFileAlarmModel"
          :file-list="fileList"
          :auto-upload="false"
          accept=".xlsx,.xls"
          :multiple="true"
          :http-request="allUpload"
          :on-change="onUploadChange"
          class="upload_file"
        >
          <el-button
            slot="trigger"
            size="small"
            type="primary"
            style="position: relative; width: 60px"
            >导入</el-button
          >
        </el-upload>
      </div>
      <span slot="footer" class="dialog-footer">
        <el-button type="primary" @click="importSubmitRole" :loading="btnLoading"
          ><i class="el-icon-circle-plus-outline"></i>保 存</el-button
        >
        <el-button @click="importBox = false"><i class="el-icon-circle-close"></i>取 消</el-button>
      </span>
    </el-dialog>
    <el-dialog title="新建报警模型" append-to-body :visible.sync="addBox" width="450px">
      <div class="dialog_content">
        <el-form :model="addCardForm" :rules="addRules" ref="addForm" class="add_form">
          <el-form-item label="报警模型类型：" prop="alarmType">
            <el-select
              v-model="addCardForm.alarmType"
              placeholder="请选择/输入报警模型类型"
              filterable
              allow-create
              clearable
              default-first-option
            >
              <el-option
                v-for="item in alarmTypeList"
                :key="item.id"
                :label="item.modelName"
                :value="item.modelName"
              >
              </el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="报警模型名称：" prop="alarmName">
            <el-select
              v-model="addCardForm.alarmName"
              placeholder="请选择/输入报警模型名称"
              filterable
              allow-create
              clearable
              default-first-option
            >
              <el-option
                v-for="item in alarmNameList"
                :key="item.id"
                :label="item.modelName"
                :value="item.modelName"
              >
              </el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="版本号：" prop="version">
            <el-input v-model="addCardForm.version" placeholder="示例:V0.0.0"></el-input>
          </el-form-item>
        </el-form>
      </div>
      <span slot="footer" class="dialog-footer">
        <el-button type="primary" @click="addSubmitRole"
          ><i class="el-icon-circle-plus-outline"></i>保 存</el-button
        >
        <el-button @click="addBox = false"><i class="el-icon-circle-close"></i>取 消</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import debounce from 'lodash/debounce'
import noData from '@/components/noData'
import { alarmTree, deleteTree, addTree, upload } from '@/api/alarmFault/alarmModel'

export default {
  components: { noData },
  data() {
    return {
      upLoading: false, //上传文件保存按钮loading
      importBox: false, //导入模型弹框
      addBox: false, //新增模型弹框
      fileList: [], //上传的文件列表
      selectModelItem: {}, //选中的模型
      addCardForm: {
        alarmType: '',
        alarmName: '',
        version: ''
      },
      addRules: {
        alarmType: [{ required: true, message: '请选择/输入报警模板类型', trigger: 'change' }],
        alarmName: [{ required: true, message: '请选择/输入报警模板名称', trigger: 'change' }],
        version: [
          {
            required: true,
            pattern: /^(V)([1-9]\d?(\.([1-9]?\d)){2}$)/,
            message: '示例:Vx.x.x',
            trigger: 'blur'
          }
        ]
      },
      alarmTypeList: [], //新建报警类型数组
      alarmNameList: [],
      determineModelList: [], //确定模型
      customModelList: [] //自定义模型
    }
  },
  computed: {},
  watch: {
    importBox() {
      if (!this.importBox) {
        this.fileList = []
      }
    }
  },
  mounted() {
    this.getTree()
  },
  methods: {
    //获取模型树接口
    getTree() {
      alarmTree().then(res => {
        if (res.data.code == 200) {
          const data = res.data.data
          this.determineModelList = []
          this.customModelList = []
          let dList = []
          let cList = []

          data.forEach(item => {
            if (item.isSystem == 1) {
              dList.push(item)
            } else {
              cList.push(item)
            }
          })

          this.determineModelList = this.handlerTree(dList)
          this.customModelList = this.handlerTree(cList)

          if (this.determineModelList.length) {
            this.handleNodeClick(this.determineModelList[0])
          } else {
            this.handleNodeClick(this.customModelList[0])
          }

          this.getAddList(this.customModelList)
        }
      })
    },
    //树节点结构修改
    handlerTree(list) {
      list.forEach(item => {
        item.isClick = false
        item.children = item.children.map(j => ({
          fatherName: item.modelName,
          isClick: false,
          ...j
        }))
      })
      return list
    },
    getAddList(arr) {
      this.alarmTypeList = arr.map(j => ({ modelName: j.modelName, id: j.id }))
      arr.forEach(item => {
        item.children.forEach(ite => {
          if (!this.alarmNameList.find(j => j.modelName == ite.modelName)) {
            this.alarmNameList.push(ite)
          }
        })
      })
    },
    //点击节点
    handleNodeClick(val) {
      this.selectModelItem = val

      this.determineModelList.forEach(item => {
        item.isClick = false
        item.children.forEach(ite => {
          ite.isClick = false
        })
      })
      this.customModelList.forEach(item => {
        item.isClick = false
        item.children.forEach(ite => {
          ite.isClick = false
        })
      })
      this.$set(val, 'isClick', true)
      this.$forceUpdate()
      this.$bus.$emit('selectModelItem', val)
    },

    //导入模型弹框--保存按钮
    importSubmitRole() {
      this.$refs.upload.submit()
    },
    allUpload(data) {
      this.upLoading = true
      upload()
        .then(result => {
          this.fileList.push({
            name: data.file.name
          })
          this.$message[result.data?.data ? 'success' : 'error'](result.data.msg)
          this.upLoading = false
          this.importBox = false
          return false
        })
        .catch(() => {
          this.$message.error('报警模型添加失败')
          this.upLoading = false
        })
    },
    onUploadChange(file) {
      const isXls = file.name.substring(file.name.lastIndexOf('.') + 1)
      console.log('文件名', file, isXls)
      const isLt10M = file.size / (1024 * 10) < 10
      if (!['xlsx', 'xls'].includes(isXls)) {
        this.onRemoveFile(file, 'upload')
        this.$message.error('上传文件只能是xlsx或xls格式!')
        return
      }
      if (!isLt10M) {
        this.$message.error('上传文件大小不能超过 10MB!')
        this.onRemoveFile(file, 'upload')
        return
      }
    },
    onRemoveFile(file, refKey) {
      const index = this.$refs[refKey].uploadFiles.findIndex(e => e.uid === file.uid)
      this.$refs[refKey].uploadFiles.splice(index, 1)
    },

    //新建模板--保存按钮
    addSubmitRole: debounce(function () {
      this.$refs['addForm'].validate(valid => {
        if (valid) {
          let { alarmType, alarmName, version } = this.addCardForm
          let param = {
            alarmModelParentName: alarmType,
            alarmModelName: alarmName,
            versions: version
          }
          let typeObj = this.alarmTypeList.find(j => j.modelName == this.addCardForm.alarmType)
          if (typeObj) {
            param.alarmModelParentId = typeObj.id
          }
          addTree({ ...param }).then(res => {
            if (res.data.code == 200) {
              this.addBox = false
              this.$message({ type: 'success', message: '报警模型添加成功！' })
              this.getTree()
            }
          })
        }
      })
    }, 700),

    //删除节点
    deleteAlarmModel() {
      if (this.selectModelItem.isSystem === 1) {
        this.$message.warning('该模型不可删除，请选择自定义模型删除！')
        return
      }
      this.$confirm('确定将选择数据删除?', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
        .then(() => {
          return deleteTree({ alarmModelId: this.selectModelItem.id })
        })
        .then(() => {
          this.getTree()
          this.$message({
            type: 'success',
            message: '报警模型删除成功!'
          })
        })
    }
  }
}
</script>

<style lang="less" scoped>
@import url('./style.less');

.alarm_card {
  height: 100%;
  width: 420px;
  //   margin-right: 10px;
  float: left;
  padding: 5px 10px 5px 5px;
  .alarm_title {
    font-size: 14px;
    font-weight: bold;
    color: white;
  }
  .determin_tree {
    background-color: rgba(1, 14, 24, 0.5);
    height: 120px;
    margin-bottom: 5px;
    margin-top: 10px;
    // padding-left: 10px;
    overflow-y: auto;
    &::-webkit-scrollbar-thumb {
        border-radius: 0px;
        background: rgba(13, 52, 83, 1);
    //   border-radius: 3px;
    //   border: 6px solid #131c35;
    //   box-shadow: 8px 0 0 rgba(13, 52, 83, 1) inset;
    //   &:hover {
    //     box-shadow: 8px 0 0 rgba(13, 52, 83, 1) inset;
    //   }
    }
    &::-webkit-scrollbar-track {
      border-radius: 0;
      background: #131c35;
    }
    &::-webkit-scrollbar {
        width: 5px
    //   width: 20px;
    //   height: 8px;
    }
  }
  .custom_tree {
    background-color: rgba(1, 14, 24, 0.5);
    height: calc(100% - 155px);

    .custom_tree_title{
        font-size: 12px;
        color: white;
        padding: 10px 0px 5px 10px;
        // height: ;
    }
    .custom_tree_content{
        overflow-y: auto;
        height: calc(100% - 73px);
        &::-webkit-scrollbar-thumb {
          border-radius: 0px;
          background: rgba(13, 52, 83, 1);
        }
        &::-webkit-scrollbar-track {
          border-radius: 0;
          background: #131c35;
        }
        &::-webkit-scrollbar {
          width: 5px
        }
    }
  }
}
.dialog_content{
    color: white;
    .upload_file{
   display: flex;
  align-items: flex-end;
  flex-flow: row-reverse;
  justify-content: space-between;
  margin-bottom: 40px;
  margin-left: 20px;
  ::v-deep .el-upload {
    margin-left: 10px;
  }
    }
  }
  .add_form{
    ::v-deep .el-form-item__content{
        display: inline-block;
        width: calc(100% - 140px);
    }
    ::v-deep .el-form-item__label{
        width: 115px;
        text-align: right;
    }
    ::v-deep .el-select{
        width: 100%;
    }
  }
  .custom-tree-node{
    font-size: 14px;

        .height_light {
          display: inline-block;
          height: 30px;
          width: 4px;
          background: #2A65AE;
          position: relative;
          float: left;
          left: -42px;
          top: 3px;
        }

  }
</style>
