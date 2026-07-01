<template>
  <div>
    <el-dialog
      :visible.sync="dialogVisible"
      :modal-append-to-body="true"
      @close="closeDialog"
      :append-to-body="true"
      :show-close="false"
    >
      <i class="el-icon-warning" style="color: #2ed133; font-size: 18px; margin-right: 10px"></i>
      <span>确认生成</span>
      <p style="font-size: 14px; margin-top: 10px; margin-bottom: 20px">
        {{ reportName }}
      </p>
      <el-input
        type="textarea"
        :rows="3"
        placeholder="请输入备注信息"
        v-model="noteInfo"
        resize="none"
      >
      </el-input>
      <span slot="footer" class="dialog-footer">
        <el-button size="mini" @click="noneStoreReport">取 消</el-button>
        <el-button size="mini" type="primary" @click="storeRepoter">确 定</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
export default {
  props: {
    dialogVisible: {
      type: Boolean
    },
    reportName: {
      type: String
    }
  },
  data() {
    return {
      noteInfo: ''
    }
  },
  methods: {
    closeDialog() {
      this.$emit('closeDialog')
    },
    storeRepoter() {
      this.$emit('dialogStoreRepote', this.noteInfo)
      this.$emit('closeDialog')
    },
    noneStoreReport() {
      this.$emit('closeDialog')
      // this.$emit('noneStoreRepote')
    }
  }
}
</script>

<style lang="less" scoped>
::v-deep .el-dialog {
  width: 360px;
  height: auto;
  .el-dialog__header {
    display: none;
  }
  .el-dialog__body {
    position: relative;
    padding: 15px 25px !important;
    font-size: 16px;
  }
  .el-dialog__footer {
    .el-button {
      margin-right: 23px;
    }
    .el-button--default {
      background: white;
      color: black;
    }
    .el-button--primary {
      background: #1b84ee;
      color: white;
      &:hover {
        background: #0126bc;
      }
    }
  }
}
</style>
