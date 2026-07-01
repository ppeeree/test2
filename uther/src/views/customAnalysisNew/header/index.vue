<template>
  <div class="header-content">
    <div class="header_down">
      <label>诊断分析</label>
      <span
        :title="isFullScreen ? '退出沉浸模式' : '进入沉浸模式'"
        style="float: right; margin-top: -3px; margin-right: 0px; cursor: pointer"
        @click="changeSize"
      >
        <i
          :class="isFullScreen ? 'el-icon-copy-document' : 'el-icon-full-screen'"
          style="font-size: 24px"
        ></i>
      </span>
      <el-dropdown trigger="click" style="float: right; margin-top: -7px">
        <span class="el-dropdown-link">
          <img class="menu_img" :src="require('/public/img/menu.png')" title="tools" />
        </span>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item icon="el-icon-plus" @click="addRow">增加行</el-dropdown-item>
          <el-dropdown-item>
            <el-dropdown placement="right-start" class="personalization" trigger="hover">
              <span class="el-dropdown-link personalization">增加图谱 </span>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item
                    v-for="item in menuList"
                    :key="item.value"
                    :index="item.key + '/' + item.value"
                    @click="addChart(item.key)"
                    >{{ item.value }}
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </el-dropdown-item>
            <el-dropdown-item @click="openSaveDiag">保存当前布局</el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
      <!--  <el-select
        :style="{ top: isFullScreen ? '20px' : '66px' }"
        v-model="selectedValue"
        @change="layoutChange"
        placeholder="请选择"
      >
        <el-option
          v-for="item in options"
          :key="item.layoutName"
          :label="item.layoutName"
          :value="item.layoutName"
          @mouseenter="mouseEnter($event)"
          @mouseleave="mouseLeave($event)"
        >
          <span style="float: left">{{ item.layoutName }}</span>
          <span
            @click.stop="removeLayout(item.layoutName)"
            class="removeIcon"
            title="删除"
            style="float: right; color: #8492a6; font-size: 13px"
            ><i class="el-icon-circle-close"></i
          ></span>
        </el-option>
      </el-select> -->
    </div>
    <el-dialog
      title="保存当前布局"
      :modal="false"
      :visible="dialogVisible"
      width="20%"
      @update:visible="dialogVisible = $event"
    >
      <!-- <el-row>
        <el-col :span="6"><p style="line-height: 40px; text-align: right">布局名称：</p></el-col>
        <el-col :span="12">
          <el-input v-model="layoutNameInput" placeholder="请输入布局名称"
        /></el-col>
      </el-row>
      <el-checkbox v-model="checked">设置为默认布局</el-checkbox> -->
      <div>确定保存当前分析方案的布局吗？</div>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogVisible = false">取 消</el-button>
          <el-button type="primary" @click="saveLayout">确 定</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>
<script>
import { removeLayout, storeLayout, getLayoutNameList } from '@/api/analysis/index.js'
import { getDOMLayout } from './tools'
import { mapState } from 'vuex'
import { getStore, setStore } from '@/util/store.js'
import { rowResize } from '../components/toolsComponent/tools.js'
export default {
  props: {
    rowTotal: {
      type: Number,
      default: 2,
      require: true
    },
    activeTabName: {
      type: String,
      default: 'Trend',
      require: true
    }
  },
  watch: {
    rowTotal(val) {
      this.getMenuList(val)
    }
    /*  selectedValue(val) {
      this.getLayoutContent(val)
    } */
  },
  data() {
    return {
      isFullScreen: false,
      menuList: [],
      dialogVisible: false,
      layoutNameInput: '',
      selectedValue: '',
      checked: true,
      options: []
    }
  },
  computed: {
    ...mapState({
      userInfo: state => state.user.userInfo
    })
  },
  created() {},
  mounted() {
    this.getMenuList(this.rowTotal)
    /*  this.getLayoutNameListFunc(() => {
      let layoutName = getStore({ name: 'layout' })
      if (layoutName) {
        if (this.options.find(i => i.layoutName == layoutName)) {
          this.selectedValue = layoutName
          this.getLayoutContent()
        }
      } else {
      }
    }) */
  },
  methods: {
    changeSize() {
      this.isFullScreen = !this.isFullScreen
      let element = document.documentElement
      if (this.isFullScreen) {
        document.getElementsByClassName('header')[0].style.display = 'none'
        document.getElementsByClassName('avue-main')[0].style.height = '100%'
        document.getElementsByClassName('avue-main')[0].style.padding = '0'
        document.getElementsByClassName('avue-main')[0].style.margin = '0'
        document.getElementById('avue-view').style.height = '100%'
        //进入全屏
        if (element.requestFullscreen) {
          element.requestFullscreen()
        } else if (element.webkitRequestFullScreen) {
          element.webkitRequestFullScreen()
        } else if (element.mozRequestFullScreen) {
          element.mozRequestFullScreen()
        } else if (element.msRequestFullscreen) {
          // IE11
          element.msRequestFullscreen()
        }
      } else {
        document.getElementsByClassName('header')[0].style.display = 'block'
        document.getElementsByClassName('avue-main')[0].style.paddingBottom = '0'
        document.getElementsByClassName('avue-main')[0].style.marginTop = '-20px'
        document.getElementsByClassName('avue-main')[0].style.height = 'calc(100% - 48px)'
        document.getElementById('avue-view').style.height = '100%'
        // 退出全屏
        if (document.exitFullscreen) {
          document.exitFullscreen()
        } else if (document.webkitCancelFullScreen) {
          document.webkitCancelFullScreen()
        } else if (document.mozCancelFullScreen) {
          document.mozCancelFullScreen()
        } else if (document.msExitFullscreen) {
          document.msExitFullscreen()
        }
      }
      setTimeout(() => {
        rowResize('rightArea')
      }, 500)
    },

    openSaveDiag() {
      this.dialogVisible = true
      this.layoutNameInput = this.selectedValue
    },
    /*layoutChange() {
      this.getLayoutContent()
    },
       async getLayoutNameListFunc(callback) {
      getLayoutNameList({ userName: this.userInfo.user_name }).then(res => {
        if (res.data.data.length) {
          this.options = res.data.data[0].children
          let layoutName = getStore({ name: 'layout' })
          if (this.selectedValue == '' && !layoutName) {
            this.selectedValue = this.options[0].layoutName
            this.getLayoutContent()
          }
          if (callback) callback()
        }
      })
    }, */
    getLayoutContent() {
      getLayoutNameList({ userName: this.userInfo.user_name, layoutName: this.selectedValue }).then(
        res => {
          if (res.data.data?.length) {
            this.$emit('initLayout', this.selectedValue, res.data.data[0].children[0].layoutData)
          }
        }
      )
    },
    removeLayout(name) {
      if (this.selectedValue == name) {
        return this.$message({
          type: 'error',
          message: '正在使用，禁止删除！'
        })
      }
      this.$alert('确定删除此布局方式？', '', {
        confirmButtonText: '确定',
        callback: res => {
          if (res == 'confirm') {
            removeLayout({ userName: this.userInfo.user_name, layoutName: name }).then(res => {
              this.$message({
                type: res.data.data.success ? 'success' : 'error',
                message: res.data.data.msg
              })
              this.getLayoutNameListFunc()
            })
          }
        }
      })
    },
    saveLayout() {
      /*   if (this.layoutNameInput == '') {
        return alert('布局名称不能为空！')
      } */
      this.dialogVisible = false
      let layoutJson = getDOMLayout()
      let param = {
        userName: this.userInfo.user_name,
        layoutName: this.activeTabName, // this.layoutNameInput,
        layoutContent: JSON.stringify(layoutJson)
      }
      storeLayout(param).then(res => {
        if (res.data.data.success) {
          /*   if (this.checked) {
            setStore({ name: 'layout', content: this.layoutNameInput })
          } */
          /*  this.getLayoutNameListFunc(() => {
            this.selectedValue = this.layoutNameInput
          }) */
        }
        this.$message({
          type: res.data.data.success ? 'success' : 'error',
          message: res.data.data.msg
        })
      })
    },
    getMenuList(val) {
      let list = []
      for (let i = 1; i < val; i++) {
        list.push({
          key: i,
          value: '第 ' + (i + 1) + ' 行'
        })
      }
      this.menuList = list
    },
    addRow() {
      this.$emit('addRow')
    },
    addChart(rowId) {
      this.$emit('addChart', rowId)
    },
    mouseEnter(e) {
      e.target.getElementsByClassName('removeIcon')[0].style.display = 'block'
    },
    mouseLeave(e) {
      e.target.getElementsByClassName('removeIcon')[0].style.display = 'none'
    }
  }
}
</script>

<style scoped lang="scss">
.header-content {
  height: 33px;
  width: 100%;
  text-align: center;
  background: #fff;

  /*  .header_top {
    width: 100%;
    height: 48px;
  } */
  .header_down {
    width: 100%;
    height: 33px;
    background: #fff;
    position: relative;
    border-bottom: 3px solid #eee;
    label {
      font-size: 16px;
      font-weight: bold;
      float: left;
      margin-left: 20px;
      line-height: 12px;
    }
    .menu_img {
      width: 20px;
      height: 20px;
      cursor: pointer;
      margin: 6px 10px 0 10px;
    }
    .el-select {
      height: 100%;
      margin-right: 10px;
      position: fixed;
      right: 65px;
      ::v-deep input {
        height: 28px;
        color: #000;
        line-height: 28px;
        border: 1px solid #aaa;
      }
      ::v-deep .el-input__icon {
        line-height: 28px;
      }
    }
  }
  ::v-deep .el-dialog {
    height: 160px !important;
    .el-row {
      padding: 20px;
    }
    input {
      border: 1px solid #ddd;
      color: #000;
    }
    input:hover {
      background: #fff;
    }
    .el-checkbox {
      color: #606266;
    }
    .el-dialog__header {
      background: #d1eaff;
      color: #000;
    }
    .el-button {
      background: #fff;
      color: #606266;
      margin: 0 10px;
      border: 1px solid #606266;
    }
    .el-button--primary {
      background: #0861aa;
      color: #fff;
    }
  }
}
</style>
