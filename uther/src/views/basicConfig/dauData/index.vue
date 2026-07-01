<template>
  <el-main>
    <div class="merge_header">系统自检</div>
    <el-row :gutter="0" style="width: 100%; height: calc(100% - 33px)">
      <el-col :span="24" class="card_block">
        <div class="card_block_title">
          <span v-if="userDeptTree.length == 1">
            {{ userDeptTree[0].name }}
          </span>
          <el-select
            size="small"
            style="width: 300px"
            v-else
            v-model="selectedWindparkID"
            placeholder="请选择"
          >
            <el-option
              v-for="item in userDeptTree"
              :key="item.id"
              :label="item.name"
              :value="item.id"
            >
            </el-option>
          </el-select>
        </div>
        <el-tabs
          type="border-card"
          v-if="tabArrList.length"
          style="width: 100%; height: 85px; margin-top: -40px"
          v-model="activeName"
        >
          <el-tab-pane
            v-for="item in tabArrList"
            :key="item.key"
            :label="item.label"
            :name="item.key"
            style="width: 100%; height: 100%"
          >
            <p style="padding-top: 5px" v-if="item.monitorTypeList.length">
              <el-badge
                v-for="ii in item.monitorTypeList"
                :key="ii.monitorTypeCode"
                :value="ii.faultMonitorNum == 0 ? '' : ii.faultMonitorNum"
                :class="[
                  'badge',
                  { activebadge: selectMoniterDev.monitorTypeCode == ii.monitorTypeCode }
                ]"
              >
                <el-button
                  :title="ii.faultMonitorNum + '台机组状态异常'"
                  @click="changeSelectedDAUType(ii)"
                  size="small"
                >
                  {{ ii.monitorTypeName }}</el-button
                >
              </el-badge>
            </p>
          </el-tab-pane>
        </el-tabs>
        <template>
          <template v-if="selectMoniterDev && selectMoniterDev.keyId">
            <component
              :keyId="selectMoniterDev.keyId"
              :key="selectMoniterDev.keyId"
              :monitorTypeCode="selectMoniterDev.monitorTypeCode"
              :is="selectMoniterDev.componentName ? selectMoniterDev.componentName : 'HADU'"
              :ref="selectMoniterDev.monitorTypeCode"
              :selectedWindparkID="selectedWindparkID"
              style="height: calc(100% - 90px)"
            />
          </template>
          <no-data v-else style="width: 100%; height: 100%" />
        </template>
      </el-col>
    </el-row>
  </el-main>
</template>
<script>
import { setTheme } from '@/util/util'
import { mapGetters } from 'vuex'
import { getStationMonitorApi } from '@/api/basicConfig/dau.js'
export default {
  components: {
    HADU: () => import('./components/dauTab.vue'),
    ModuleBus: () => import('./components/modulebusTab.vue'),
    noData: () => import('@/components/noData/index.vue')
  },
  data() {
    return {
      selectMoniterDev: {}, // 选中的采集单元设备信息
      activeName: '',
      tabArrList: [],
      selectedWindparkID: '', // 当前页面显示的风场ID
      monitorTypeCodeComponentMap: {
        CVM: 'HADU',
        BVM: 'HADU',
        TVM_STE: 'ModuleBus', // 塔筒结构
        TVM_FLG_GAP: 'ModuleBus', // 法兰间隙
        TVM_CBF: 'HADU', // 塔筒索力
        TVM_BFM: 'HADU' // 超声螺栓
      }
    }
  },
  computed: {
    ...mapGetters(['userDeptTree'])
  },
  watch: {
    selectedWindparkID: {
      handler() {
        this.initWindparkPage()
      }
    },
    activeName: {
      handler() {
        this.selectMoniterDev = this.tabArrList.find(
          item => item.key == this.activeName
        ).monitorTypeList[0]
      }
    }
  },
  created() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
    this.selectedWindparkID = this.userDeptTree[0].id
  },
  mounted() {},
  methods: {
    // 选中的采集单元设备
    changeSelectedDAUType(value) {
      this.selectMoniterDev = value
    },
    initWindparkPage() {
      getStationMonitorApi({ stationID: this.selectedWindparkID }).then(res => {
        if (res.data.data.length) {
          this.tabArrList = res.data.data.map(item => {
            return {
              label: item.monitorCompName,
              key: item.monitorCompName,
              monitorTypeList: Array.from(item.monitorTypeList, ii => ({
                ...ii,
                keyId: this.selectedWindparkID + '_' + ii.monitorTypeCode,
                componentName: this.monitorTypeCodeComponentMap[ii.monitorTypeCode]
              }))
            }
          })
          this.activeName = this.tabArrList[0].key
          this.selectMoniterDev = this.tabArrList[0].monitorTypeList[0]
        } else {
          this.tabArrList = []
          this.activeName = ''
          this.selectMoniterDev = {}
          // 无数据
        }
      })
    }
  }
}
</script>
<style lang="scss" scoped>
::v-deep .el-checkbox__label {
  color: #909399 !important;
}
.el-main {
  height: 100%;
  width: 100%;
  padding: 0;
  overflow: unset;
  color: #000;
  .merge_header {
    height: 33px;
    margin-bottom: 3px;
    font-size: 16px;
    font-weight: bold;
    line-height: 33px;
    padding: 0 17px;
    position: relative;
    border-bottom: 3px solid #ccc;
  }
  .card_block {
    width: 100%;
    height: 100%;
    overflow: hidden;
    background: #fff;
    border-radius: 5px;
  }

  .card_block .card_block_title {
    font-size: 15px;
    font-weight: bolder;
    height: 40px;
    line-height: 40px;
    padding: 0 25px;
    position: relative;
    border-bottom: 2px solid rgba(160, 161, 163, 0.1);
  }

  .card_block_title .el-select .el-input__inner {
    font-weight: bolder;
    height: 30px;
    line-height: 30px;
    border: none;
    font-size: 15px;
    padding: 0;
    padding-left: 15px;
    width: auto;
    color: #000;
  }
  .card_block_title .el-select .el-input__suffix {
    left: -12px;
    right: auto;
  }
  .card_block .card_block_title::before {
    content: '';
    position: absolute;
    left: 10px;
    top: 8px;
    width: 4px;
    height: 25px;
    border-radius: 0px 2px 2px 0px;
    opacity: 1;
    background: linear-gradient(171deg, rgb(0, 129, 255) 0%, #22cce2 101%);
  }
  ::v-deep .el-tabs__content {
    width: 100%;
    height: calc(100% - 40px);
  }
  ::v-deep .el-tabs__header {
    margin: 0 0 5px 20px;
    margin-left: 350px;
    .el-tabs__item {
      height: 40px;
      line-height: 40px;
    }
    .is-active {
      background: #f9f9f9 !important;
      //  border-radius: 30px;
    }
  }
  /*  ::v-deep .el-tabs__nav-wrap::after {
    background-color: rgba(160, 161, 163, 0.1);
  } */
  .badge {
    margin: 3px 10px;
  }
  .activebadge .el-button {
    background: #0081ff;
    color: #fff;
  }
}
</style>
