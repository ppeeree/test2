<template>
  <el-popover v-model="visible" width="220" height="270" trigger="click" placement="right-end">
    <div class="drone_information">
      <div class="drone_title">
        <i :class="['icon', 'local', 'local-' + this.compCode[currentComp]]"></i>

        <span class="title_text">设备信息</span>
        <!-- <i class="el-icon-minus"></i> -->
      </div>
      <div class="drone_text" v-if="currentComp == 'windturbine'">
        <ul>
          <li v-for="item in unitData" :key="item.key">
            <b>{{ item.name }}</b
            >：{{ item.value }}{{ item.unit || '' }}
          </li>
        </ul>
      </div>
      <div class="basic_content" v-else>
        <div v-for="item in engineRoomData" :key="item.title">
          <div class="text_title">
            <i :class="['icon', 'local', 'local-' + entityPartEnum[item.entityName]]"></i>
            {{ item.entityName }}
          </div>
          <div class="text_content">
            <span v-for="i in item.entityInfo" :key="i.name">
              <b>{{ i.name }} </b>: {{ i.value }}{{ i.unit || '' }}
            </span>
          </div>
        </div>
      </div>
    </div>
    <img slot="reference" class="markimg" src="/img/WindTurbine/icon/deviceInfo.png" />
  </el-popover>
</template>
<script>
import { getUnitApi } from '@/api/WindTurbine/CenterPartAPI.js'
import { compNameEnum, entityPartEnum } from '@/util/constant'

export default {
  components: {},
  props: {
    currentComp: {
      type: String,
      require: false,
      default: 'turbine'
    },
    // entityId: {
    //   type: String,
    //   require: false,
    //   default: ''
    // },
    windTurbineId: {
      type: String,
      require: false,
      default: ''
    }
  },
  data() {
    return {
      compCode: { windturbine: 'turbine', NAC: 'NAC', TWW: 'TOW', ROT: 'ROT' },
      compNameEnum,
      entityPartEnum,
      // compIndex: { turbine: 'WINDTURBINE', engine: 'NAC', tower: 'TWW', blade: 'ROT' },
      // compName: '',
      unitData: [],
      engineRoomData: []
    }
  },
  watch: {
    currentComp: {
      handler(value) {
        if (value) {
          this.getUnit().then(res => {
            this.handlerInfoList(res)
          })
        }
      }
    },
    windTurbineId: {
      handler(val) {
        if (val) {
          this.getUnit().then(res => {
            this.handlerInfoList(res)
          })
        }
      }
    }
  },
  created() {},
  mounted() {
    this.getUnit().then(res => {
      this.handlerInfoList(res)
    })
  },
  methods: {
    async getUnit() {
      let data = []
      await getUnitApi({
        windTurbineId: this.windTurbineId,
        entityType: this.currentComp
      }).then(res => {
        data = res.data.data
      })
      return data
    },

    handlerInfoList(data) {
      // console.log('基础信息数据', data)
      if (data.length) {
        if (this.currentComp == 'windturbine') {
          const { entityName, entityInfo } = data[0]

          this.unitData = entityInfo
          this.$emit('initCompId', entityName)
        } else {
          this.engineRoomData = data
        }
      }

      /* 原来数据处理
        const { entityInfo, entityName } = res.data.data
       if (this.currentComp === 'turbine') {
          let dataArr = []
          dataArr.push(...entityInfo)
          let idArr = []
          entityInfo.forEach(item => {
            if (item.key == 'ROT' || item.key == 'NAC' || item.key == 'TWW') {
              idArr.push(item)
            } else {
              dataArr.push(item)
            }
          })
          dataArr.unshift({ name: '名称', value: entityName })
          this.unitData = dataArr

          this.$emit('initCompId', entityName)
        } else if (this.currentComp == 'NAC' || this.currentComp == 'TWW') {
          this.engineRoomData = res.data.data
        } else {
          this.unitData = entityInfo
          this.compName = entityName
        } */
    }
  }
}
</script>
<style lang="scss" scoped>
.markimg {
  cursor: pointer;
}
.drone_information {
  width: 280px;
  height: 270px;
  background-color: rgba(41, 64, 88, 0.5);
  padding: 0px;
  backdrop-filter: blur(10px);
  color: #fff;
  overflow: auto;
  border-radius: 5px;
  img {
    float: left;
    // margin-top: -3px;
  }
  .drone_title {
    background-color: #274060;
    padding: 5px 10px;
    .title_text {
      margin-left: 10px;
      line-height: 20px;
    }
    .el-icon-minus {
      float: right;
      margin-right: 10px;
      line-height: 27px;
      cursor: pointer;
    }
  }
  .drone_text {
    padding: 5px 10px;
    li {
      line-height: 28px;
      padding: 0 10px;
      font-family: Arial, Helvetica, sans-serif;
      font-size: 14px;
    }
  }
}
.basic_content {
  padding: 0 10px;
  & > div {
    padding: 5px 0 0 0;
  }
  .text_title {
    // border-bottom: 1px solid #fff;
    margin: 3px 0;
    padding-left: 0px;
    img {
      width: 20px;
      margin-right: 5px;
      margin-top: 0;
    }
  }
  .text_content {
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    flex-flow: row wrap;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 5px;
    margin-bottom: 5px;
    span {
      display: inline-block;
      padding: 3px 15px;
    }
  }
}
</style>
