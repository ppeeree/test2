<template>
  <div class="content">
    <template v-if="compList.length">
      <div v-for="item in compList" :key="item.code" class="comp_item">
        <span class="comp_icon">
          <i
            :class="[
              'icon',
              'local',
              `local-${
                !isNaN(parseFloat(item.code.slice(-1))) ? item.code.slice(0, -1) : item.code
              }`
            ]"
            style="color: #abe1ff; font-size: 30px"
          ></i>
          <span>{{ item.type }}</span>
        </span>
        <span class="comp_ring">
          <ring
            ref="doughnut"
            :unique="item.code"
            title=""
            :seriesName="item.type"
            :optDataArr="item.ringData"
            width="167px"
            height="90px"
            centerLeft="45%"
            centerUp="55%"
          ></ring>
        </span>
        <div class="comp_content">
          <p>
            <el-col :span="10">部件数：</el-col>
            <el-col :span="14">
              <span class="import_num">{{ item.total }}</span></el-col
            >
          </p>
          <div class="content_text">
            <el-col :span="10">报警占比：</el-col>
            <el-col :span="14">
              <span class="import_num">{{ item.faultRate }}</span
              >&nbsp; %</el-col
            >
          </div>
        </div>
      </div>
    </template>
    <noData v-else firstText="暂无数据" noteText="" />
  </div>
</template>

<script>
import { getCompFaultLevelCount } from '@/api/screen/leftCardApi.js'
import { entityPartEnum, eventTypeEnum, levelColorEnum } from '@/util/constant'
import popup from './popup.vue'
import PopupWindow from '../mixins/popupWindow'
import ring from '../base/pie-doughnut.vue'

export default {
  components: {
    ring,
    noData: () => import('@/components/noData/index.vue')
  },
  inject: ['parent'],
  data() {
    return {
      entityPartEnum,
      pieDataOpt: [],
      legendData: [],
      tableList: [],
      icoName: sessionStorage.getItem('icoName'),
      total: 0,
      cardFromData: [],
      peiDataIndex: null,
      compList: []
    }
  },
  created() {},
  mounted() {
    //  this.getFaultStatistic()
    window.addEventListener('setItem', () => {
      this.icoName = sessionStorage.getItem('icoName')
    })
  },
  methods: {
    getFaultStatistic() {
      let params = this.parent.isControl
        ? { userID: this.$store.getters.userInfo.user_id }
        : {
            stationId: this.parent.WindFarm.id
          }
      getCompFaultLevelCount(params).then(res => {
        if (res.data.code == 200) {
          let dealList = []
          res.data.data.forEach(item => {
            let {
              compName,
              compCode,
              compTotal,
              faultStatusList,
              faultCompCount,
              changeRate,
              changeCount
            } = item
            let newCount = faultCompCount == '' ? 0 : faultCompCount
            let obj = {
              code: compCode,
              type: compName,
              total: compTotal,
              faultRate: ((newCount / compTotal) * 100).toFixed(2),
              increase: Number(changeRate.slice(0, -1)),
              increaseNum: Number(changeCount),
              ringData: []
            }
            obj.ringData = faultStatusList.map((item, index) => {
              const { name, count } = item
              return {
                value: count,
                name: eventTypeEnum[name],
                itemStyle: {
                  color: levelColorEnum[name]
                },
                label: {
                  show: false // name !== 'normal' && name !== 'noFault' && name !== 'nostate' && index === 0
                }
              }
            })
            dealList.push(obj)
          })
          this.compList = dealList
        }
      })
    },
    passPart() {
      this.peiDataIndex = null
      // this.$refs['doughnut'].clearParams()
      this.setSessionItem('icoName', null)
      this.$bus.$emit('pieDoughnut', {
        all: true,
        id: null
      })
      this.$cesuimData.popupList.forEach((item, index) => {
        // item.domRove()
        item.windowClose()
        this.$cesuimData.popupList.splice(index, 1)
      })
    },
    handlePart(e) {
      this.setSessionItem('icoName', e.name)
      this.setSessionItem('changeEntities', '1')
      const ids = e.data.ids.map(({ windturbineId }) => windturbineId)
      this.$cesuimData.popupList.length > 0 &&
        this.$cesuimData.popupList.forEach(item => {
          // item.domRove()
          item.windowClose()
        })
      this.$cesuimData.popupList = []
      const temp = this.handleInclude(ids, { isAll: true, bool: false })
      // this.$utils.map.initMapStatus()
      if (this.peiDataIndex === e.dataIndex) {
        this.handleInclude(ids, { isAll: true, bool: true }, true)
        this.setSessionItem('changeEntities', '0')
        return this.passPart()
      }

      const partData = e.data.ids.map(({ compStatus, compType, time, windturbineId }) => {
        return {
          code: compType,
          state: compStatus,
          acqTime: time,
          windturbineId
        }
      })
      // console.log('展示框', partData)
      if (partData[0].code && partData[0].state) {
        temp.forEach(item => {
          const { name, id } = item
          const { elevation, latitude, longitude } = item.description._value.position
          const { status } = item.description._value

          // eslint-disable-next-line no-useless-escape
          let reg = /entitie[\-]+?/g
          const part = partData.filter(item => item.windturbineId === id.replace(reg, ''))
          const domVue = new PopupWindow(
            window.viewer,
            window.Cesium.Cartesian3.fromDegrees(+longitude, +latitude, +elevation),
            {
              name,
              part,
              deviceStatus: status,
              width: '60%'
            },
            popup,
            {
              y: -80
            }
          )
          this.$cesuimData.popupList.push(domVue)
        })
      }

      this.peiDataIndex = e.dataIndex
    },
    handleInclude(e, all = { isAll: false, bool: true }, isShow) {
      // console.log('风机隐藏的方法', e, all, isShow)

      const temp = this.$utils.map.getInclude(
        { isAll: isShow || false, bool: true },
        // eslint-disable-next-line no-useless-escape
        e.map(item => 'entitie\-' + item),
        true
      )
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        e.map(item => 'label\-' + item),
        true,
        this.$utils.map.FJLabel
      )
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        e.map(item => 'event\-' + item),
        true,
        this.$utils.map.eventIcon
      )
      return temp
    },
    getCompIcon(type) {
      console.log(type)
      let img = require(`/public/img/screen/blueDevicePart/big${type}.png`)
      if (img) {
        return img
      } else {
        // 图库无此类型数据
        return require(`/public/img/screen/empty/161.png`)
      }
    }
  }
}
</script>

<style lang="less" scoped>
.content {
  position: relative;
  height: 340px;
  padding-top: 8px;
  max-height: 340px;
  overflow: auto;
  width: 100%;
  // display: flex;
  // flex-direction: row;
  // align-items: center;
  // .flex-ico {
  //   display: flex;
  //   flex-direction: column;
  //   div + div {
  //     margin-top: 6px;
  //   }
  // }
  .comp_item {
    background-color: rgba(255, 255, 255, 0.1);
    width: 100%;
    height: 100px;
    border-radius: 5px;
    margin: 0 0 10px 0px;
    display: flex;
    flex-direction: row;
    justify-content: center;
    align-items: center;
    .comp_icon {
      width: 16%;
      font-size: 12px;
      color: white;
      position: relative;
      text-align: center;
      span {
        width: 100%;
        // text-align: center;
        display: inline-block;
        margin-top: 14px;
      }
    }
    .comp_ring {
      height: 100%;
      width: 40%;
    }
    .comp_content {
      height: 100%;
      width: 44%;
      color: white;
      font-size: 12px;
      padding: 15px 8px;
      line-height: 28px;
      display: flex;
      flex-direction: column;
      justify-content: center;
      .el-col {
        margin-bottom: 0;
      }
      .content_text {
        .el-col {
          margin-top: 8px;
        }
      }
      .content_num {
        .el-col {
          margin-top: 8px;
        }
      }
      .import_num {
        font-size: 18px;
        color: #abe1ff;
      }
    }
  }
}
</style>
