<template>
  <div class="import_event_card" v-show="showCard" v-if="eventList.length">
    <!-- 上面按钮部分 -->
    <div
      class="event_btn"
      @click="clickEventBtn"
      @mouseenter="enterBtn"
      @mouseleave="leaveBtn"
      :style="{ backgroundImage: 'url(' + this.btnImage + ')', cursor: 'pointer' }"
    ></div>
    <!-- 下方列表内容 -->
    <div class="event_text">
      <el-row style="height: 23px; line-height: 23px; cursor: pointer">
        <!-- 等级 -->
        <el-col :span="2" @click.native="showEventDetailCard(0, firstEventItem)">
          <span>{{ eventCode[firstEventItem.degree] }}</span>
        </el-col>
        <!-- 设备 002机组 -->
        <el-tooltip
          effect="dark"
          :content="firstEventItem.windParkName + '_' + firstEventItem.entityName"
          placement="top"
        >
          <el-col
            @click.native="showEventDetailCard(0, firstEventItem)"
            :span="3"
            class="event_message"
            style="text-align: center"
            >{{ firstEventItem.windParkName }}_{{ firstEventItem.entityName }}</el-col
          >
        </el-tooltip>
        <!-- 事件详情 -->
        <el-col
          @click.native="showEventDetailCard(0, firstEventItem)"
          :span="10"
          class="event_message"
        >
          <img
            style="margin-right: 5px; float: left; margin-top: 3px"
            :src="getEventImg(firstEventItem.type)"
          />
          {{ firstEventItem.message }}
        </el-col>
        <!-- 发生时间 -->
        <el-col @click.native="showEventDetailCard(0, firstEventItem)" :span="4">{{
          firstEventItem.disNowTime
        }}</el-col>
        <!-- 处理状态： -->
        <el-col @click.native="showEventDetailCard(0, firstEventItem)" :span="3">{{
          DoneStatus[firstEventItem.handleStatus]
        }}</el-col>
        <el-col :span="2" style="padding-left: 0px"
          ><img
            style="cursor: pointer; text-align: center; position: relative"
            @click="closeAllEvent = true"
            title="点击删除该条提醒"
            src="/img/eventMethod/noneShowIcon.png"
        /></el-col>
      </el-row>
    </div>
    <!-- 点击后出现的事件列表 -->
    <div
      class="event_list"
      ref="listWrap"
      @scroll="scrollListener"
      :style="{ height: `${contentHeight}px` }"
      v-show="eventListShow"
    >
      <transition name="el-zoom-in-bottom">
        <ul :style="{ height: `${allListHeight}px` }">
          <li
            :style="{
              margin: '10px 0px 5px 5px',
              height: `${itemHeight}px`,
              transform: `translateY(${offsetY}px)`,
              width: '97%'
            }"
            v-for="(allitem, allindex) in showList"
            :key="allindex"
          >
            <el-row
              @mouseenter.native="enterItem(allindex)"
              @mouseleave.native="leaveItem()"
              :style="{
                width: '100%',
                cursor: 'pointer',
                height: '38px',
                lineHeight: '38px',
                border:
                  itemEnterId == allindex
                    ? '2px solid #FF0A35'
                    : itemClickId == allindex
                    ? '2px solid #1FFFFF'
                    : '2px solid transparent'
              }"
            >
              <!-- 等级 -->
              <el-col :span="2" @click.native="showEventDetailCard(allindex, allitem)">
                <span>{{ eventCode[allitem.degree] }}</span>
              </el-col>
              <el-tooltip
                effect="dark"
                :content="allitem.windParkName + '_' + allitem.entityName"
                placement="top"
              >
                <el-col
                  style="text-align: center"
                  :span="3"
                  class="event_message"
                  @click.native="showEventDetailCard(allindex, allitem)"
                  >{{ allitem.windParkName }}_{{ allitem.entityName }}</el-col
                >
              </el-tooltip>
              <el-col
                @click.native="showEventDetailCard(allindex, allitem)"
                :span="10"
                class="event_message"
              >
                <img
                  style="margin-right: 5px; position: relative; top: 3px"
                  :src="getEventImg(allitem.type)"
                />{{ allitem.message }}</el-col
              >

              <el-col :span="4" @click.native="showEventDetailCard(allindex, allitem)">{{
                allitem.disNowTime
              }}</el-col>
              <el-col :span="3" @click.native="showEventDetailCard(allindex, allitem)">{{
                DoneStatus[allitem.handleStatus]
              }}</el-col>
              <el-col :span="2" style="text-align: center">
                <span style="cursor: pointer" @click.stop="deleteEventItem(allindex)">
                  <img title="点击删除该条提醒" src="/img/eventMethod/noneShowIcon.png" />
                </span>
              </el-col>
            </el-row>
          </li>
        </ul>
      </transition>
    </div>

    <!-- 点击删除后的弹框 -->
    <el-dialog v-model="dialogVisible" width="20%" :modal-append-to-body="false">
      <span>
        <span style="float: left; margin-right: 7px"
          ><i class="el-icon-question" style="color: #e6a23c"></i
        ></span>
        <span></span>
        确定将该条事件提醒关闭？
      </span>
        <template #footer>
          <span class="dialog-footer">
        <el-button @click="dialogVisible = false">取 消</el-button>
        <el-button type="primary" @click="clickbtn()">确 定</el-button>
          </span>
        </template>
    </el-dialog>
    <!-- 点击删除后的弹框 -->
    <el-dialog v-model="closeAllEvent" width="20%" :modal-append-to-body="false">
      <span>
        <span style="float: left; margin-right: 7px"
          ><i class="el-icon-question" style="color: #e6a23c"></i
        ></span>
        <span></span>
        确定将所有事件提醒关闭？
      </span>
        <template #footer>
          <span class="dialog-footer">
        <el-button @click="closeAllEvent = false">取 消</el-button>
        <el-button type="primary" @click="clickImportEvent()">确 定</el-button>
          </span>
        </template>
    </el-dialog>
  </div>
</template>

<script>
import { getImportEventApi } from '@/api/WindTurbine/eventAPI.js'
import { GeneratorEnum, eventCode } from '@/util/constant.js'
import dayjs from 'dayjs'
import 'dayjs/locale/zh-cn'

export default {
  props: {
    showEventDetail: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      eventList: [], //所有数据
      showList: [], //可视区域内的数据
      contentHeight: 220, //外部box高度
      itemHeight: 38, //每行高度
      showNum: 0, //展示几条数据
      offsetY: '', //动态偏移量
      allListHeight: '', //全部列表的长度
      eventListShow: false, //判断按钮点击和事件列表是否出现的变量
      firstEventItem: '',
      //枚举判断图片
      getImg: {
        health: 'healthEvent',
        inspection: 'patroEvent',
        work: 'workEvent'
      },
      GeneratorEnum,
      eventCode,
      btnImage: '/img/WindTurbine/background/eventBtn.png', //存储图片地址
      itemEnterId: '-1', //判断鼠标移入列表id
      showCard: true, // 控制整个卡片显示
      itemClickId: '-1', // 点击事件列表
      firstShowItem: '0', //最底端显示的事件
      dialogVisible: false, //控制删除框显示的变量
      closeAllEvent: false,
      deleteId: '',
      DoneStatus: {
        done: '已处理',
        notdone: '未处理'
      }
    }
  },
  watch: {
    showEventDetail: {
      handler(val) {
        if (!val) {
          this.itemClickId = '-1'
        }
      }
    },
    eveltListLength: {
      handler() {
        this.firstEventItem = this.eventList[0]
      }
    }
  },
  computed: {
    eveltListLength() {
      return this.eventList?.length || 0
    }
  },
  mounted() {
    this.getImportEventData()
  },
  methods: {
    //获取四级事件数据
    getImportEventData() {
      getImportEventApi({ degree: 4 }).then(res => {
        if (res.data.code === 200) {
          const data = res.data?.data
          if (!data) return
          this.eventList = data
          this.firstEventItem = data[0]
          this.allListHeight = this.eventList.length * this.itemHeight
          this.showNum = Math.ceil(this.contentHeight / this.itemHeight) + 2
          this.showList = this.eventList.slice(0, this.showNum)
        }
      })
    },
    //改变时间方法
    changeTime(oldTime) {
      let newTime = dayjs(oldTime).fromNow()
      return newTime
    },
    deleteEventItem(index) {
      this.dialogVisible = true
      this.deleteId = index
    },
    clickbtn() {
      this.dialogVisible = false
      this.eventList.splice(this.deleteId, 1)
    },
    // 按钮点击事件
    clickEventBtn() {
      this.eventListShow = !this.eventListShow
      let img
      if (this.eventListShow) {
        img = '/img/WindTurbine/background/clickEventBtn.png'
      } else {
        img = '/img/WindTurbine/background/eventBtn.png'
      }
      this.btnImage = img
    },
    //点击事件列表 item
    showEventDetailCard(index, clickitem) {
      this.itemClickId = index
      this.$emit('showEventDetailCard', clickitem)
    },
    //获取事件列表的名称
    getEventImg(name) {
      return require(`/public/img/eventMethod/${this.getImg[name]}.png`)
    },
    enterBtn() {
      this.btnImage = '/img/WindTurbine/background/clickEventBtn.png'
    },
    leaveBtn() {
      if (!this.eventListShow) {
        this.btnImage = '/img/WindTurbine/background/eventBtn.png'
      }
    },
    enterItem(index) {
      this.itemEnterId = index
    },
    leaveItem(index) {
      this.itemEnterId = index
    },
    //监听该DOM的滚动事件
    scrollListener(e) {
      let scrollTop = e.target.scrollTop //滚动的高度
      this.offsetY = scrollTop - (scrollTop % this.itemHeight) //计算向上卷了多少条
      this.showList = this.eventList.slice(
        Math.floor(scrollTop / this.itemHeight),
        Math.floor(scrollTop / this.itemHeight) + this.showNum
      )
    },
    clickImportEvent() {
      this.closeAllEvent = false
      this.showCard = false
    }
  }
}
</script>

<style lang="less" scoped>
.import_event_card {
  width: 510px;
  height: 55px;
  color: white;
  background-image: url('/img/WindTurbine/background/importEventImg.png');
  .event_btn {
    height: 17.5px;
    width: 113px;
    position: absolute;
    // top: 10px;
    left: 39%;
  }
  .event_text {
    position: absolute;
    top: 24px;
    width: 510px;
    margin-left: 10px;
  }
  .event_list {
    width: 510px;
    background: rgba(220, 16, 52, 0.6);
    backdrop-filter: blur(7px);
    position: relative;
    bottom: 222px;
    overflow-y: auto;
    overflow-x: hidden;
    z-index: 1;
    box-sizing: border-box;
    &::-webkit-scrollbar {
      width: 5px;
    }
    &::-webkit-scrollbar-track {
      background-color: rgba(220, 16, 52, 0.6);
    }
    &::-webkit-scrollbar-thumb {
      box-shadow: inset 0 0 6px #d2e5f1;
    }
    .el-col {
      margin-bottom: 5px;
      // text-align: center;
    }
  }
  //弹框样式修改
  // :deep(.el-dialog){
  //   // margin-top: 3vh;
  //   border-radius: 10px;
  //   .el-dialog__header {
  //     height: 0px;
  //     .el-dialog__close {
  //       color: black;
  //     }
  //   }
  //   .el-dialog__footer,
  //   .el-dialog__body {
  //     background-color: white;
  //     border-radius: 10px;
  //   }
  //   .el-dialog__footer {
  //     bottom: 11px;
  //     right: 42px;
  //     .el-button {
  //       height: 20px;
  //       line-height: 3px;
  //     }
  //   }
  // }
  .event_message {
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
  }
}
</style>
