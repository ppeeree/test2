<template>
  <el-container class="turbine_card">
    <el-header
      class="card_title"
      :style="{ fontSize: isMini ? sizeWidth * 0.45 + 'px' : sizeWidth * 0.35 + 'px' }"
    >
      <span
        :style="{
          color:
            data.statusLastTime == '0001-01-01 00:00:00'
              ? '#8D8D8D'
              : levelColor[data.windturbineStatus],
          cursor: 'pointer'
        }"
        @click="jumpTurbine"
      >
        <i
          :class="['icon', 'local', 'local-turbine']"
          :style="{ fontSize: sizeWidth * 0.6 + 'px' }"
          title="整机"
        ></i>
        {{ data.windturbineName }}

        <!-- <span
          @click.native="showEquipStatus"
          :style="{
            position: ' relative',
            top: sizeWidth * 0.2 + 'px',
            cursor: 'pointer',
            float: 'right'
          }"
          ><img :style="{ width: sizeWidth * 0.5 + 'px' }" src="/img/windField/noneCollector.png"
        /></span> -->
      </span>
    </el-header>

    <el-main class="card_compList">
      <span
        v-for="item in data.healthStatusEntityVo"
        :key="item.entityType"
        class="compList_item"
        @click="jumpTurbineComp(item)"
        :style="btnStyle(item)"
        :title="item.entityName"
      >
        <span
          class="icon_index"
          :style="indexStyle(item)"
          v-show="!!item.entityType.match(/\d/g)"
          >{{ item.entityType.replace(/[^\d]/g, ' ') }}</span
        >
        <!-- <i
          :class="['icon', 'local', 'local-' + item.entityType.replace(/[0-9]+/g, '')]"
          :style="{
            fontSize: item.entityType == 'MST' ? sizeWidth * 0.36 + 'px' : sizeWidth * 0.42 + 'px'
          }"
        ></i> -->
        <i
          :class="['icon', 'local', 'local-' + item.entityType.replace(/[0-9]+/g, '')]"
          :style="iconStyle(item)"
        ></i>
      </span>
    </el-main>

    <el-footer
      class="card_bottom"
      :style="{ fontSize: isMini ? sizeWidth * 0.4 + 'px' : sizeWidth * 0.3 + 'px' }"
      >{{ data.statusLastTime }}</el-footer
    >
  </el-container>
</template>
<script>
import { levelColor, eventTypeEnum } from '@/util/constant.js'

export default {
  props: {
    data: {
      type: Object,
      default: () => {}
    },
    boxWidth: {
      type: String
    },
    turbineNum: {
      type: Number,
      default: 33
    }
  },
  data() {
    return {
      levelColor,
      eventTypeEnum,
      boxSize: ''
    }
  },
  computed: {
    sizeWidth() {
      return this.boxWidth / 6
    },
    isMini() {
      if (this.turbineNum > 35 && this.turbineNum <= 100) {
        return true
      } else {
        return false
      }
    }
  },
  methods: {
    // 样式 - 部件按钮
    btnStyle(item) {
      return {
        backgroundColor:
          item.statusTime == '0001-01-01 00:00:00' ? '#8D8D8D' : this.levelColor[item.entityStatus],
        width: this.sizeWidth * 0.65 + 'px',
        height: this.sizeWidth * 0.65 + 'px',
        marginRight: this.sizeWidth * 0.2 + 'px',
        marginTop: this.sizeWidth * 0.2 + 'px'
      }
    },
    // 样式 - icon下标
    indexStyle(item) {
      return {
        color:
          item.statusTime == '0001-01-01 00:00:00' ? '#8D8D8D' : this.levelColor[item.entityStatus],
        width: this.sizeWidth * 0.2 + 'px',
        height: this.sizeWidth * 0.2 + 'px',
        fontSize: this.sizeWidth * 0.2 + 'px',
        top: this.isMini ? this.sizeWidth * 0.13 + 'px' : this.sizeWidth * 0.2 + 'px',
        left: this.isMini ? this.sizeWidth * 0.05 + 'px' : this.sizeWidth * 0.05 + 'px'
      }
    },
    // 样式 - icon
    iconStyle(item) {
      return {
        fontSize: this.sizeWidth * 0.42 + 'px',
        marginLeft: item.entityType == 'MST' ? this.sizeWidth * -0.01 + 'px' : '0px'
      }
    },

    showEquipStatus() {
      // console.log('显示监测设备信息')
    },

    jumpTurbine() {
      this.$router.push({
        path: '/WindTurbine',
        query: {
          turbineId: this.data.windturbineId,
          type: 'windturbine',
          // windFarmName: this.data.windturbineName,
          locationcode: this.data.windparkId
        }
      })
    },
    jumpTurbineComp(val) {
      const compIndex = {
        ROT: ['叶片一', '叶片二', '叶片三', '变桨轴承一', '变桨轴承二', '变桨轴承三'],
        NAC: ['齿轮箱', '发电机', '主轴'],
        TWW: ['塔筒', '偏航轴承']
      }

      let jumpComp = ''

      for (let i in compIndex) {
        if (compIndex[i].find(j => j == val.entityName)) {
          jumpComp = i
          break
        }
      }

      this.$router.push({
        path: '/WindTurbine',
        query: {
          turbineId: this.data.windturbineId,
          type: jumpComp,
          // windFarmName: this.data.windturbineName,
          locationcode: this.data.windparkId
        }
      })
    }
  }
}
</script>

<style lang="less" scoped>
.turbine_card {
  background-color: rgba(13, 52, 83, 1);
  backdrop-filter: blur(10px);
  display: inline-block;
  margin: 0 10px 10px 0;

  .card_title {
    height: 28% !important;
    padding-left: 10px;
    border-bottom: rgb(16, 65, 105) 2px solid;
    font-weight: 500;
    margin: 0 6px;
    display: flex;
    justify-content: space-around;
    flex-direction: column;
  }
  .card_compList {
    height: 55% !important;
    padding: 0 5% !important;
    overflow: hidden;
    display: flex;
    flex-wrap: wrap;
    .compList_item {
      flex: 0 0 12.9%;
      // white-space: nowrap;
      cursor: pointer;
      border-radius: 2px;
      // text-align: center;
      display: flex;
      justify-content: center; /* 水平居中 */
      align-items: center;
      .icon_index {
        position: relative;
        border-radius: 50%;
        display: inline-block;
        background: white;
        text-align: center;
      }
    }
  }
  .card_bottom {
    height: 17% !important;
    padding: 0;
    float: right;
    margin-right: 10px;
    display: flex;
    justify-content: space-around;
    flex-direction: column;
  }
}
</style>
