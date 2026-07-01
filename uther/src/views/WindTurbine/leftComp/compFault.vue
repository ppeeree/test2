<template>
  <div class="fault_card_border">
    <div class="fault_card">
      <p>
        <img src="/img/WindTurbine/faultCompIcon.png" alt="" />
        诊断故障
      </p>
      <div class="fault_content">
        <div class="fault_list" v-if="isNoFaultData">
          <div
            v-for="item in hasFaultComp"
            :key="item.compName"
            style="border-bottom: 1px solid #255873"
          >
            <div class="item_title">
              <i
                :class="['icon', 'local', 'local-' + handlerCompImg(item.compCode)]"
                :style="{
                  color: levelColor[item.compState],
                  marginRight: '5px',
                  marginLeft: '3px'
                }"
              ></i>
              <span
                class="cornerMark"
                :style="{ background: levelColor[item.compState] }"
                v-if="!isNaN(parseFloat(item.compCode.slice(-1)))"
                >{{ item.compCode.slice(-1) }}</span
              >
              {{ item.compName }}
            </div>
            <div
              class="item_content"
              v-for="faultItem in item.compFaultList"
              :key="faultItem.entityId"
            >
              <span>{{ faultItem.faultValue }}</span>
              {{ faultItem.diagnoseTime }}
            </div>
            <div v-if="!item.compFaultList.length" style="height: 25px"></div>
          </div>
        </div>
        <div v-else class="no_fault_list">部件健康，暂无故障！</div>
      </div>
    </div>
  </div>
</template>

<script>
// import noData from '@/components/noData/index.vue'
import { levelColor } from '@/util/constant'

export default {
  // components:{noData},
  props: {
    faultList: {
      type: Array,
      default: () => []
    },
    isNoFaultData: {
      type: Boolean,
      default: false
    }
  },
  computed: {
    hasFaultComp() {
      let compListArry = this.faultList.filter(item => item.compFaultList.length > 0)
      return compListArry
    }
  },
  data() {
    return {
      levelColor: levelColor
    }
  },
  methods: {
    showCompFault() {
      this.$emit('showCompFault')
    },
    handlerCompImg(compCode) {
      let code = compCode == 'TWW' ? 'TOW' : compCode
      return !isNaN(parseFloat(code.slice(-1))) ? code.slice(0, -1) : code
    }
  }
}
</script>

<style lang="less" scoped>
.fault_card_border {
  height: 251px;
  width: 400px;
  border-radius: 5px;
  border: 2px solid #4d5869;
  backdrop-filter: blur(5px);
  padding: 3px;
  color: white;
  position: absolute;
  top: 67%;
  right: 3px;

  .fault_card {
    background-color: rgba(77, 88, 105, 0.5);
    p {
      font-size: 14px;
      font-weight: bold;
      height: 37px;
      line-height: 37px;
      padding-left: 10px;
      border-bottom: 2px solid #4d5869;
      img {
        position: relative;
        top: 4px;
        margin-right: 4px;
      }
    }
    .fault_content {
      height: 204px;
      width: 100%;
      overflow-y: auto;
      overflow-x: hidden;
      padding: 11px;
      .no_fault_list {
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 18px;
        font-weight: bold;
        letter-spacing: 0.6px;
        opacity: 0.6;
      }
      .fault_list {
        font-size: 12px;
        .item_title {
          font-weight: bold;
          // border-bottom: 1px solid #255873;
          height: 30px;
          line-height: 30px;
          .cornerMark {
            width: 10px;
            height: 10px;
            line-height: 10px;
            color: #fff;
            border-radius: 50%;
            position: relative;
            left: -26px;
            bottom: -3px;
            font-size: 9px;
            /* margin-top: 30px; */
            text-align: center;
            display: inline-block;
          }
        }
        .item_content {
          height: 27px;
          line-height: 27px;
          padding-right: 8px;
          text-align: right;
          margin-bottom: 5px;
          margin-left: 10px;
          width: 97%;
          position: relative;
          left: -1%;
          span {
            text-align: left;
            overflow: hidden;
            white-space: nowrap;
            text-overflow: ellipsis;
            position: absolute;
            left: 20px;
            max-width: 180px;
          }
        }
      }
    }
  }
}
</style>
