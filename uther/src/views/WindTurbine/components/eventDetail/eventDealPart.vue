<template>
  <div class="event_deal_detail">
    <el-form>
      <el-form-item label="关联事件：">
        <el-tooltip placement="right" effect="light">
          <div slot="content">
            <div style="color: #409eff; margin-bottom: 3px">
              <i class="el-icon-warning"></i> 关联事件处理
            </div>
            对于同一个机组，同一个类型，同一级别事件进行批量统一处理。
          </div>
          <span
            @mouseenter="mouseShow = true"
            @mouseleave="mouseShow = false"
            style="margin-right: 10px"
            ><img src="/img/WindTurbine/icon/linkIcon.png"
          /></span>
        </el-tooltip>
        <el-checkbox v-model="checked">关联事件合并处理</el-checkbox>
      </el-form-item>
      <el-form-item label="处理方式：" prop="handleMethods" class="is-required">
        <el-select v-model="handleMethods" placeholder="请选择" :popper-append-to-body="false">
          <el-option
            v-for="item in dealMethodList"
            :key="item.value"
            :label="item.value"
            :value="item.value"
          >
          </el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="执行时间：" prop="handleTime" class="is-required">
        <el-date-picker
          v-model="handleTime"
          type="datetime"
          placeholder="选择日期时间"
          popper-class="get_event_time"
        >
        </el-date-picker
      ></el-form-item>
      <el-form-item label="故障描述：" prop="fauleInput" class="is-required"
        ><el-input
          v-model="fauleInput"
          placeholder="请输入内容"
          clearable
          style="width: 60%"
        ></el-input>
      </el-form-item>
      <el-form-item label="故障程度：" prop="faultLevel" class="is-required">
        <div class="event_deal_degree">
          <el-select
            v-model="faultLevel"
            placeholder="请选择"
            :popper-append-to-body="false"
            id="faultLevelInput"
            ref="testLevel"
            @change="changeInputColor($event)"
          >
            <el-option
              v-for="item in faultLevelList"
              :key="item.value"
              :label="item.value"
              :value="item.value"
            >
              <span :style="{ color: changeFaultTextColor[item.value] }">{{ item.value }}</span>
            </el-option>
          </el-select>
        </div>
      </el-form-item>
      <el-form-item label="维护建议：">
        <el-input
          type="textarea"
          clearable
          :resize="none"
          placeholder="请输入内容"
          v-model="textarea"
          :autosize="{ minRows: 3, maxRows: 3 }"
          style="max-height: 75px; width: 77%"
        >
        </el-input>
      </el-form-item>
      <el-form-item style="margin-left: 52%">
        <el-button round @click="switchCard('noneAdd')">取消</el-button>
        <el-button type="primary" round @click="passTimeLineData('noneAdd')">确认</el-button>
      </el-form-item>
    </el-form>
    <!-- 缺少必要参数的弹框 -->
    <el-dialog
      style="margin-top: 3vh"
      :visible.sync="dialogVisible"
      :modal-append-to-body="false"
      width="60%"
    >
      <span>
        <span style="float: left; margin-right: 7px"
          ><i class="el-icon-warning" style="color: #e6a23c; font-size: 20px"></i
        ></span>
        <span>缺少必要参数</span>
      </span>
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogVisible = false">取 消</el-button>
        <el-button type="primary" @click="dialogVisible = false">确 定</el-button>
      </span>
    </el-dialog>
    <!-- 没有参数时的弹框 -->
    <el-dialog
      style="margin-top: 3vh"
      :visible.sync="dialog"
      width="60%"
      :modal-append-to-body="false"
    >
      <span>
        <span style="float: left; margin-right: 7px"
          ><i class="el-icon-warning" style="color: #e6a23c; font-size: 20px"></i
        ></span>
        <span>请输入参数</span>
      </span>
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialog = false">取 消</el-button>
        <el-button type="primary" @click="dialog = false">确 定</el-button>
      </span>
    </el-dialog>
    <!-- 确认取消修改弹框 -->
    <el-dialog
      style="margin-top: 3vh"
      :visible.sync="showNoChange"
      width="60%"
      :modal-append-to-body="false"
    >
      <span>
        <span style="float: left; margin-right: 7px"
          ><i class="el-icon-warning" style="color: #e6a23c; font-size: 20px"></i
        ></span>
        <span>确认取消修改</span>
      </span>
      <span slot="footer" class="dialog-footer">
        <el-button @click="showNoChange = false">取 消</el-button>
        <el-button type="primary" @click="closeDetailCard">确 定</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import { postEventApi } from '@/api/WindTurbine/eventAPI.js'
import { eventStatusEnum } from '@/util/constant.js'
export default {
  inject: ['getEventAllData'],
  props: {
    //点击列表拿到的row
    clickEventItem: {
      type: Object,
      require: true,
      default() {
        return {}
      }
    },
    //后续跟踪数据
    trackdData: {
      type: Object,
      require: true,
      default() {
        return {}
      }
    }
  },
  data() {
    return {
      dealMethodList: [
        {
          value: '运维检修'
        },
        {
          value: '无人机自主巡检'
        }
      ],
      faultLevelList: [
        {
          value: '严重'
        },
        {
          value: '中等'
        },
        {
          value: '轻微'
        }
      ],
      //已处理的变量
      checked: false, //关联事件变量
      handleMethods: '', //处理方式变量
      handleTime: '', //执行时间变量
      fauleInput: '', //故障描述变量
      faultLevel: '', //故障等级变量
      textarea: '', //维护建议变量
      //弹框显示变量
      dialogVisible: false,
      dialog: false,
      showNoChange: false,
      mouseShow: false, //鼠标移入链接后出现弹框变量
      eventItem: '', //点击的事件内容
      eventTrack: '', //事件跟踪数据
      changeFaultLevel: {
        严重: 'severe',
        中等: 'moderate',
        轻微: 'mild'
      },
      changeDegree: {
        first: 1,
        second: 2,
        third: 3,
        fourth: 4
      },
      changeType: {
        health: 1,
        inspection: 2,
        work: 3
      },
      changeFaultName: {
        mild: '轻微',
        moderate: '中等',
        textareasevere: '严重'
      },
      changeFaultTextColor: {
        轻微: '#FF9B9B',
        中等: '#FF5D5D',
        严重: '#FF0000'
      },
      changeChecked: {
        true: '1',
        false: '2'
      },
      eventStatusEnum,
      fauleHandleStatus: '' //事件处理状态
    }
  },
  watch: {
    clickEventItem: {
      handler(val) {
        this.eventItem = val
        this.handleMethods = ''
        this.handleTime = ''
        this.textarea = ''
        this.faultLevel = this.changeFaultName[val.severity]
        this.fauleInput = val.faultValue
        this.fauleHandleStatus = val.handleStatus
        this.changeInputColor(this.faultLevel)
        //每次切换风机，事件处理卡片都不显示
        this.$emit('switchCard', 'noneAdd')
      }
    },
    //从后续跟踪返回的数组
    trackdData: {
      handler() {
        this.init(this.fauleHandleStatus, this.trackdData)
      }
    }
  },
  destroyed() {},
  mounted() {
    this.faultLevel = this.changeFaultName[this.clickEventItem.severity]
    this.fauleInput = this.clickEventItem.faultValue
    this.changeInputColor(this.faultLevel)
    this.eventItem = this.clickEventItem //对点击的事件进行初始化
  },
  methods: {
    //数据已处理初始化
    init(val, list) {
      if (val == '已处理') {
        let lastItem = list.slice(-1)[0]
        this.checked = this.changeChecked[lastItem.relevanceEvent] //关联事件
        this.handleMethods = lastItem.disposalWay //处理方式
        this.handleTime = lastItem.disposalTime //时间
        this.fauleInput = lastItem.faultDesc //故障描述
        this.faultLevel = this.changeFaultName[lastItem.severity] //故障程度
        this.textarea = lastItem.maintainAdvice //维护建议
        this.changeInputColor(this.faultLevel) //改变故障颜色
      } else {
        return
      }
    },
    // 打开添加处理方式卡片
    switchCard(name) {
      this.$emit('changeSwitchCard', name)
      this.$emit('switchCard', name)
      if (this.fauleHandleStatus == '已处理') {
        this.showNoChange = true
      }
    },
    //关闭已处理卡片
    closeDetailCard() {
      this.$emit('closeDetailCard')
      this.showNoChange = false
    },
    //点击确定添加处理方式的方法
    passTimeLineData(name) {
      let that = this
      if (that.handleMethods && that.handleTime) {
        that.$emit('switchCard', name)
        const { degree, type, id, windTurbineId, message } = that.eventItem
        let newTime = that.timeChange(that.handleTime)
        // 调用post方法
        postEventApi({
          degree: that.changeDegree[degree],
          disposalTime: newTime,
          disposalWay: that.handleMethods,
          eventId: id,
          faultDesc: that.fauleInput,
          maintainAdvice: that.textarea,
          relevanceEvent: that.changeChecked[that.checked],
          type: that.changeType[type],
          windturbineId: windTurbineId,
          message: message,
          severity: that.changeFaultLevel[that.faultLevel]
        }).then(res => {
          if (res.data.code == 200) {
            this.$message({ type: 'success', message: '事件处理成功！' })
            this.getEventAllData(this.eventItem)
            this.$emit('dealEventOver')
          }
        })
      } else if (that.handleMethods || that.handleTime) {
        that.dialogVisible = true
      } else {
        that.dialog = true
      }
    },
    // 改变时间格式
    timeChange(oldTime) {
      let data = new Date(oldTime)
      let y = data.getFullYear()
      let m = data.getMonth() + 1
      m = m < 10 ? '0' + m : m
      let d = data.getDate()
      d = d < 10 ? '0' + d : d
      let h = data.getHours()
      let minute = data.getMinutes()
      minute = minute < 10 ? '0' + minute : minute
      let second = data.getSeconds()
      second = second < 10 ? '0' + second : second
      let nowTime = y + '-' + m + '-' + d + ' ' + h + ':' + minute + ':' + second
      return nowTime
    },
    //根据内容修改文字颜色
    changeInputColor(event) {
      let color = this.changeFaultTextColor[event]
      let dom = document.getElementById('faultLevelInput')
      dom.style.setProperty('--inputText', color)
    }
  }
}
</script>

<style lang="less" scoped>
.event_deal_detail {
  color: #ffffff;
  width: 100%;
}
.get_event_time {
  ::v-deep .el-input--small .el-input__inner {
    background: white !important;
    box-shadow: none;
    border-color: transparent;
    color: black !important;
    border: 1px solid #dcdfe6 !important;
    border-radius: 4px;
  }
}

//弹框样式
::v-deep .el-dialog__body {
  padding-top: 10px;
  padding-left: 35px;
  color: white;
}
::v-deep .el-button {
  height: 27px;
  padding: 0px 17px;
}

//时间处理弹框
::v-deep .el-form {
  margin-left: 19px;
  background: rgba(255, 255, 255, 0.1);
  padding-left: 7px;
  .el-form-item {
    margin-bottom: 0px;
    .el-form-item__label {
      font-size: 12px;
      width: 69px;
    }
  }
  .el-input__inner {
    height: 32px;
    background: rgba(0, 9, 14, 0.3);
    border-color: transparent;
  }
  .el-select {
    width: 60%;
  }
  .el-date-editor.el-input {
    width: 60%;
  }
  .el-textarea__inner {
    background: rgba(0, 9, 14, 0.3);
    border-color: transparent;
  }
}

.event_deal_degree {
  ::v-deep .el-input__inner {
    color: var(--inputText);
  }
}
</style>
