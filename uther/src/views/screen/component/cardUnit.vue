<template>
  <div class="my_popup" :style="windparkInfo.style" @click="clickEvent">
    <h3 class="my_popup_title">{{ windparkInfo.stationName }}</h3>
    <div class="my_popup_content">
      <p class="my_popup_content_up">
        <span>监测机组：</span><b>{{ windparkInfo.deviceNum }}</b
        >台
      </p>
      <div style="width: 100%">
        <div class="my_popup_content_down">
          <p v-for="item in levelConfig" :key="item.code">
            <template v-if="windparkInfo[item.key]">
              <span>{{ item.name }}：</span
              ><b :style="{ color: levelColorEnum[item.code] }">{{ windparkInfo[item.key] }}</b>
            </template>
          </p>
        </div>
        <div class="percent_bar">
          <b
            v-if="windparkInfo.dangerDeviceNum"
            :style="{
              width: (windparkInfo.dangerDeviceNum / windparkInfo.deviceNum) * 100 + '%',
              backgroundColor: levelColorEnum.danger
            }"
          ></b>
          <b
            v-if="windparkInfo.warningDeviceNum"
            :style="{
              width: (windparkInfo.warningDeviceNum / windparkInfo.deviceNum) * 100 + '%',
              backgroundColor: levelColorEnum.warning,
              left: (windparkInfo.dangerDeviceNum / windparkInfo.deviceNum) * 100 + '%'
            }"
          ></b>
          <b
            v-if="windparkInfo.attentionDeviceNum"
            :style="{
              width: (windparkInfo.attentionDeviceNum / windparkInfo.deviceNum) * 100 + '%',
              backgroundColor: levelColorEnum.attention,
              left:
                ((windparkInfo.dangerDeviceNum + windparkInfo.warningDeviceNum) /
                  windparkInfo.deviceNum) *
                  100 +
                '%'
            }"
          ></b>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import { levelColorEnum } from '@/util/constant.js'

export default {
  props: {
    windparkInfo: {
      type: Object,
      default: () => ({})
    }
  },
  data() {
    return {
      levelConfig: [
        /*  {
          name: '危险',
          code: 'danger',
          key: 'dangerDeviceNum'
        }, */
        {
          name: '警告',
          code: 'warning',
          key: 'warningDeviceNum'
        },
        {
          name: '注意',
          code: 'attention',
          key: 'attentionDeviceNum'
        },
        {
          name: '正常',
          code: 'normal',
          key: 'normalDeviceNum'
        }
      ],
      levelColorEnum
    }
  },
  methods: {
    clickEvent() {
      this.$emit('clickevent', this.windparkInfo)
    }
  }
}
</script>
<style lang="scss" scoped>
.my_popup::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.3), transparent);
  transition: left 0.5s;
}

.my_popup:hover::before {
  left: 100%;
}

.my_popup:hover {
  transform: scale(1.05);
  z-index: 10;
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.4);
}

.my_popup:active {
  transform: scale(0.95);
}
.my_popup {
  background: rgba(4, 17, 33, 0.5);
  border-radius: 8px;
  font-size: calc(var(--block-width) * 0.08);
  font-weight: bold;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
  transition: all 0.3s ease;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  user-select: none;
  .my_popup_title {
    width: 100%;
    white-space: nowrap; /* 防止文本换行 */
    overflow: hidden; /* 隐藏溢出的内容 */
    text-overflow: ellipsis; /* 显示省略号来代表被修剪的文本 */
    padding-left: calc(var(--block-width) * 0.05);
    height: calc(var(--block-height) * 0.22);
    line-height: calc(var(--block-height) * 0.28);
    font-size: calc(var(--block-width) * 0.07);
    color: #fff;
    position: relative;
    border-bottom: 1px solid #1c8c93;
    &::before {
      content: '';
      position: absolute;
      width: calc(var(--block-width) * 0.03);
      height: calc(var(--block-height) * 0.08);
      background: #1fffff;
      left: calc(var(--block-width) * 0.01);
      top: calc(var(--block-height) * 0.1);
    }
    &::after {
      content: '';
      position: absolute;
      width: calc(var(--block-width) * 0.08);
      height: calc(var(--block-height) * 0.02);
      background: #1fffff;
      bottom: 0;
      right: 0;
    }
  }
  .my_popup_content {
    width: 100%;
    height: calc(var(--block-height) * 0.78);
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    align-items: center;
    .my_popup_content_up {
      display: flex;
      flex-direction: row;
      font-size: calc(var(--block-width) * 0.06);
      color: rgba(255, 255, 255, 0.8);
      align-items: baseline;
      height: auto; //calc(var(--block-height) * 0.28);
      overflow: hidden;
      width: 100%;
      padding-left: calc(var(--block-width) * 0.05);
      b {
        font-weight: bolder;
        margin: 0 5px;
        font-family: 'DS-DIGIB-2';
        font-size: calc(var(--block-width) * 0.13);
        color: #fff;
        span {
          font-size: calc(var(--block-width) * 0.06);
          color: rgba(255, 255, 255, 0.85);
          margin-left: calc(var(--block-width) * 0.03);
        }
      }
    }
    .my_popup_content_down {
      width: 100%;
      height: auto; //calc(var(--block-height) * 0.4);
      display: flex;
      flex-direction: row;
      align-items: baseline;
      justify-content: space-around;
      justify-items: center;
      color: #fff;
      p {
        display: flex;
        flex-direction: row;
        align-items: baseline;
        justify-content: center;
        span {
          font-size: calc(var(--block-width) * 0.06);
          color: rgba(255, 255, 255, 0.8);
        }
        b {
          font-family: 'DS-DIGIB-2';
          font-size: calc(var(--block-width) * 0.18);
        }
      }
    }
    .percent_bar {
      width: 100%;
      height: calc(var(--block-height) * 0.03);
      border-radius: 5px;
      background: #2ed133;
      position: relative;
      b {
        position: absolute;
        left: 0;
        top: 0;
        height: calc(var(--block-height) * 0.03);
      }
      b:first-child {
        border-top-left-radius: 5px;
        border-bottom-left-radius: 5px;
      }
    }
  }
}
</style>
