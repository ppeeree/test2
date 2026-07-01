<template>
  <div
    class="rowbox"
    style="display: flex; width: 100%; height: 100%; margin: 0; padding: 0; background-color: #eee"
  >
    <div class="colbox" style="flex: 0 0 450px; overflow: hidden; height: 100%">
      <div class="card_block" style="height: 100%">
        <div class="card_block_content" style="height: calc(100%)">
          <el-table
            ref="singleTable"
            :data="turbineList"
            height="98%"
            style="width: 100%"
            border
            v-loading="loading1"
            :highlight-current-row="true"
            element-loading-text="加载中"
            :header-cell-style="{
              backgroundColor: '#F2F6FC',
              color: '#909399',
              textAlign: 'center'
            }"
            row-key="monitorID"
            class="tableLimit"
            size="mini"
            @current-change="getTurbineDAUList"
          >
            <el-table-column prop="deviceName" label="机组名称" width="80"></el-table-column>
            <el-table-column prop="monitorIP" label="IP地址"></el-table-column>
            <el-table-column
              width="60"
              prop="monitorStatus"
              label="状态"
              :filters="turbineStatusFilters"
              :filter-method="filterStatus"
            >
              <template slot-scope="scope">
                <span
                  :style="{ color: scope.row.monitorStatus == '正常' ? '#606266' : '#F56C6C' }"
                  >{{ scope.row.monitorStatus }}</span
                >
              </template>
            </el-table-column>
            <el-table-column prop="monitorStatusTime" sortable label="更新时间"></el-table-column>
          </el-table>
        </div>
      </div>
    </div>
    <Splitter splitType="vertical" id="areaSplit" style="flex: 0 0 6px" :limit="{}" />
    <div
      class="colbox"
      :span="16"
      style="flex: 1 1 0%; overflow: hidden; height: 100%; display: flex; flex-direction: column"
    >
      <div
        class="card_block"
        :style="{
          flexGrow: 0,
          flexShrink: 0,
          flexBasis: monitorTypeCode == 'TVM_BFM' ? '240px' : '400px'
        }"
      >
        <div class="card_block_title">{{ currentDeviceName }}机组采集单元通道状态</div>
        <div class="card_block_content">
          <el-table
            ref="multipleTable"
            :data="dauList"
            element-loading-text="加载中"
            v-loading="loading2"
            height="100%"
            style="width: 100%"
            size="mini"
            border
            row-key="measlocID"
            :header-cell-style="{
              backgroundColor: '#F2F6FC',
              color: '#909399',
              textAlign: 'center'
            }"
            @selection-change="handleSelectionChange"
            @row-click="rowClick"
            class="tableLimit"
          >
            <el-table-column type="selection" width="55" :selectable="checkSelectable">
            </el-table-column>
            <el-table-column prop="channelNum" label="通道号">
              <template slot-scope="scope">
                <span>{{ scope.row.channelNum === -1 ? '--' : scope.row.channelNum }}</span>
              </template>
            </el-table-column>
            <el-table-column prop="measlocName" label="测量位置"></el-table-column>
            <el-table-column
              prop="channelStatus"
              label="通道状态"
              :filters="dauStatusFilters"
              :filter-method="filterDauStatus"
            >
              <template slot-scope="scope">
                <span
                  :style="{ color: scope.row.channelStatus == '正常' ? '#606266' : '#F56C6C' }"
                  >{{ scope.row.channelStatus }}</span
                >
              </template>
            </el-table-column>
            <el-table-column prop="value" :label="getTitleName(true)"></el-table-column>
            <el-table-column prop="channelStatusTime" sortable label="更新时间"></el-table-column>
          </el-table>
        </div>
      </div>
      <splitter splitType="horizontal" id="turbine_module_bus_split" style="flex: 0 0 6px" />
      <div
        class="card_block"
        :style="{
          flex: monitorTypeCode == 'TVM_BFM' ? ' 0 0 240px' : ' 1 1 0'
        }"
      >
        <div class="card_block_title">
          特征值趋势
          <p style="float: right; font-size: 12px; font-weight: normal">
            时间：
            <el-date-picker
              v-model="timeValue"
              style="width: 240px"
              type="daterange"
              size="mini"
              align="right"
              unlink-panels
              range-separator="-"
              start-placeholder="开始日期"
              end-placeholder="结束日期"
              :picker-options="pickerOptions"
            >
            </el-date-picker>
            <el-button
              title="查看表格勾选项的趋势"
              type="primary"
              size="mini"
              @click="getTrendChart(false)"
              style="float: right; margin-left: 20px; margin-top: 5px"
              >查看趋势</el-button
            >
          </p>
          <div style="float: right; font-size: 12px; margin-right: 15px; font-weight: normal">
            <el-select
              style="width: 260px"
              size="mini"
              v-model="selectValue"
              multiple
              placeholder="请选择"
            >
              <el-option
                v-for="item in selectOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              >
              </el-option>
            </el-select>
          </div>
        </div>
        <div class="card_block_content">
          <trend-chart
            :dataSourse="trendDataSource"
            theme="light"
            chartType="TA"
            isMultipleCLick="single"
            @clickEvent="trendClickEvent"
            :loading="loading"
            isShowTitle="hide"
          />
        </div>
      </div>
      <splitter
        v-show="monitorTypeCode == 'TVM_BFM'"
        splitType="horizontal"
        id="turbine_module_bus_split1"
        style="flex: 0 0 6px"
      />
      <div
        class="card_block"
        :style="{ flexGrow: 1, flexShrink: 1, flexBasis: 0 }"
        v-show="monitorTypeCode == 'TVM_BFM'"
      >
        <div
          class="card_block_title"
          style="display: flex; justify-content: center; align-items: center; position: relative"
        >
          <span style="position: absolute; left: 15px">回波波形</span>
          <el-alert
            v-show="isShowAlert"
            title="无基准回波波形！"
            type="warning"
            effect="dark"
            center=""
            show-icon
            :style="{ height: '30px', width: '300px' }"
          ></el-alert>
        </div>
        <div class="card_block_content">
          <wave-chart
            ref="TimeDomain"
            chartType="TimeDomain"
            :requestParam="clickedMarkPoint"
            :requestApi="true"
            theme="light"
            isShowTitle="hide"
            :isDataDown="true"
          >
          </wave-chart>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import Splitter from '@/components/splitter/index.vue'
import { setTheme } from '@/util/util'
import debounce from 'lodash/debounce'
import dayjs from 'dayjs'
import {
  getHADUStatusListApi,
  getTurbineDAUListApi,
  getChartDataApi
} from '@/api/basicConfig/dau.js'
import trendChart from '@/components/diagnosisChart/trend.vue'
import waveChart from '@/components/diagnosisChart/wave.vue'
import { dealTrendData } from '@/util/transfrom'
export default {
  components: {
    Splitter,
    trendChart,
    waveChart
  },
  props: {
    selectedWindparkID: {
      type: String,
      required: true
    },
    monitorTypeCode: {
      type: String,
      required: true
    },
    keyId: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      selectedDAUType: '', // 左侧展示相关数据的采集器类型

      // 左侧模块数据
      turbineList: [], // 采集类型对应的机组DAU状态列表（左侧）
      turbineStatusFilters: [],
      currentRow: {}, // 当前选中的行
      loading1: true,

      // 右上模块数据
      dauList: [],
      loading2: true,
      currentDeviceName: '', // 标题机组名称
      selectDAUtableRow: [],
      dauStatusFilters: [],
      dauStatus: [],

      timeValue: [],
      pickerOptions: {
        shortcuts: [
          {
            text: '最近一周',
            onClick(picker) {
              const end = new Date()
              const start = new Date()
              start.setTime(start.getTime() - 3600 * 1000 * 24 * 7)
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '最近一个月',
            onClick(picker) {
              const end = new Date()
              const start = new Date()
              start.setTime(start.getTime() - 3600 * 1000 * 24 * 30)
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '最近三个月',
            onClick(picker) {
              const end = new Date()
              const start = new Date()
              start.setTime(start.getTime() - 3600 * 1000 * 24 * 90)
              picker.$emit('pick', [start, end])
            }
          }
        ]
      },
      trendDataSource: {
        data: [],
        xType: 'time',
        yType: 'value',
        titleText: '趋势分析'
      },
      requestParam: {},
      selectValue: [],
      selectOptions: [],
      // 添加当前选中的 Mark 点信息
      clickedMarkPoint: [],

      // 是否展示alert
      isShowAlert: false,

      isHandlingSelection: false // 新增：防止 selection-change 事件循环的标志位
    }
  },
  computed: {},
  watch: {
    keyId: {
      handler(val, old) {
        this.initWindparkPage()
      },
      immediate: true
    }
  },
  created() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
    // 给时间筛选赋初始值
    const start = dayjs().subtract(7, 'day').startOf('day').format('YYYY-MM-DD')
    const end = dayjs().endOf('day').format('YYYY-MM-DD')
    this.timeValue = [start, end]
  },
  mounted() {
    if (this.monitorTypeCode !== 'TVM_BFM') {
      this.selectValue = ['DC']
      this.selectOptions = [
        {
          value: 'DC',
          label: '偏置电压'
        }
      ]
    } else {
      this.selectValue = ['PPK']
      this.selectOptions = [
        {
          value: 'PPK',
          label: '峰峰值'
        },
        {
          value: 'BAF',
          label: '轴力'
        },
        {
          value: 'Temp',
          label: '温度'
        }
      ]
    }
  },

  methods: {
    rowClick(row) {
      this.$refs.multipleTable.toggleRowSelection(row)
    },
    // 风场ID修改
    initWindparkPage: debounce(function () {
      // 清空所有相关数据
      this.currentDeviceName = ''
      this.turbineList = []
      this.dauList = []
      this.clickedMarkPoint = null
      this.turbineStatusFilters = []
      this.dauStatusFilters = []

      // 请求接口
      this.getTurbineTableList()
    }, 300),

    // 测点状态表格勾选change - 通道号为-1时，默认不选中
    handleSelectionChange(val) {
      // 如果是内部代码主动触发的 clearSelection/toggleRowSelection，直接放行，不拦截
      if (this.isHandlingSelection) return

      // 过滤掉不可选的行
      const valid = val.filter(r => r.channelNum !== -1)

      if (this.monitorTypeCode === 'TVM_BFM') {
        // ================= 单选逻辑 (TVM_BFM) =================
        if (valid.length > 1) {
          // 选中超过1个，取最后一次选中的数据
          const lastSelected = valid[valid.length - 1]
          this.isHandlingSelection = true // 开启标志位，防止下方操作触发死循环

          this.$nextTick(() => {
            this.$refs.multipleTable.clearSelection()
            this.$refs.multipleTable.toggleRowSelection(lastSelected, true)
            this.isHandlingSelection = false // 操作完毕，关闭标志位
          })

          this.selectDAUtableRow = [lastSelected]
        } else if (valid.length === 1) {
          // 正好选中1个，正常赋值
          this.selectDAUtableRow = valid
        } else {
          // 全部取消选中
          this.selectDAUtableRow = []
        }
      } else {
        // ================= 多选逻辑 (其他类型) =================
        if (valid.length !== val.length) {
          // 选中的数据中包含了不可选行，需剔除
          this.isHandlingSelection = true
          this.$nextTick(() => {
            this.$refs.multipleTable.clearSelection()
            valid.forEach(r => this.$refs.multipleTable.toggleRowSelection(r, true))
            this.isHandlingSelection = false
          })
        }
        this.selectDAUtableRow = valid
      }
    },

    // 获取采集器状态列表
    getTurbineTableList() {
      getHADUStatusListApi({
        stationID: this.selectedWindparkID,
        monitorType: this.monitorTypeCode
      })
        .then(data => {
          if (data.data.data.length) {
            this.turbineList = Object.freeze(data.data.data)
            let statusList = new Set(Array.from(this.turbineList, item => item.monitorStatus))
            this.turbineStatusFilters = Array.from([...statusList], item => ({
              value: item,
              text: item
            }))
            this.loading1 = false
            this.$nextTick(() => {
              this.$refs.singleTable.setCurrentRow(this.turbineList[0])
            })
          }
        })
        .catch(error => console.error('Unable to get items.', error))
    },

    // 获取选中机组的通道状态列表
    getTurbineDAUList(row) {
      if (!row) return
      this.currentRow = row
      this.currentDeviceName = row.deviceName

      const table = this.$refs.multipleTable
      this.selectDAUtableRow = []
      table?.clearSelection()

      getTurbineDAUListApi({
        monitorID: row.monitorID,
        monitorType: this.monitorTypeCode
      })
        .then(res => {
          var data = res.data.data
          this.loading2 = false

          if (!data?.length) {
            this.dauList = []
            this.dauStatusFilters = []
            return
          }

          this.dauList = data
          this.dauStatusFilters = [...new Set(data.map(item => item.channelStatus))].map(item => ({
            value: item,
            text: item
          }))

          this.$nextTick(() => {
            if (!this.$refs.multipleTable) return

            const isSingle = this.monitorTypeCode === 'TVM_BFM'
            const validRows = this.dauList.filter(r => r.channelNum !== -1)
            const unNormal = validRows.filter(
              i => i.channelStatus !== '正常' && i.channelStatus !== '未知'
            )
            // 默认选中的目标数据
            let targets = []
            if (isSingle) {
              // 单选模式：优先选异常，否则选第一个有效行
              targets = [unNormal[0] || validRows[0]].filter(Boolean)
            } else {
              // 多选模式：优先选异常，如果没有异常选第一个
              targets = unNormal.length ? unNormal : validRows.slice(0, 1)
            }
            // 开启标志位，防止代码触发选中时被 handleSelectionChange 拦截并重置
            this.isHandlingSelection = true
            this.$refs.multipleTable.clearSelection()
            targets.forEach(r => this.$refs.multipleTable.toggleRowSelection(r, true))
            // 手动赋值给选中数组
            this.selectDAUtableRow = targets

            // 下一个事件循环关闭标志位，确保后续用户手动点击不受影响
            this.$nextTick(() => {
              this.isHandlingSelection = false
            })
            // 加载趋势波形
            this.getTrendChart()
          })
        })
        .catch(error => {
          console.error('Unable to get items.', error)
          this.loading2 = false
        })
    },

    // 获取趋势图数据
    getTrendChart() {
      this.loading = true
      if (!this.selectDAUtableRow.length) return this.$message.warning('请先勾选列表中数据！')
      this.clickedMarkPoint = []
      getChartDataApi({
        evCodes: this.selectValue.join(','),
        measlocIDs: this.selectDAUtableRow.map(i => i.measlocID).join(','),
        startTime: dayjs(this.timeValue[0]).format('YYYY-MM-DD'),
        endTime: dayjs(this.timeValue[1]).format('YYYY-MM-DD'),
        monitorType: this.monitorTypeCode
      }).then(res => {
        if (res.data.code == 200 && res.data.data) {
          let arr = dealTrendData(res.data.data)
          this.$set(this.trendDataSource, 'data', arr)
          if (!this.trendDataSource.data.length) {
            this.$message({
              message: '未查询到数据',
              type: 'warning',
              duration: 1000
            })
          }
          this.loading = false
        }
      })
    },
    trendClickEvent(params) {
      this.clickedMarkPoint = params
    },
    // 机组状态表格状态列筛选
    filterStatus(value, row) {
      return row.monitorStatus === value
    },
    // 通道状态表格状态列筛选
    filterDauStatus(value, row) {
      return row.channelStatus === value
    },
    checkSelectable(row) {
      return row.channelNum !== -1
    },

    getTitleName(isShowUnit) {
      let name = this.monitorTypeCode == 'TVM_BFM' ? '峰峰值' : '偏置电压'
      let unit = this.monitorTypeCode == 'TVM_BFM' ? '' : '(V)'
      return isShowUnit ? name + unit : name
    }
  }
}
</script>
<style lang="scss" scoped>
::v-deep .el-checkbox__label {
  color: #909399 !important;
}
::v-deep .el-table--mini .el-table__cell {
  padding: 3px 0;
}
.el-main {
  height: 100%;
  width: 100%;
  padding: 0;
  overflow: hidden;
  color: #000;

  .el-col {
    padding: 0;
  }
  .card_block {
    width: 100%;
    overflow: hidden;
    background: #fff;
    border-radius: 5px;
  }

  .card_block .card_block_title {
    font-size: 15px;
    font-weight: bolder;
    height: 40px;
    line-height: 40px;
    padding: 0 15px;
    border-bottom: 1px solid #eee;
    position: relative;
    color: #000;
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
    left: 0;
    top: 8px;
    width: 4px;
    height: 25px;
    border-radius: 0px 2px 2px 0px;
    opacity: 1;
    background: linear-gradient(171deg, #0081ff 0%, #22cce2 101%);
  }

  .card_block_content {
    width: 100%;
    height: calc(100% - 41px);
    overflow: hidden;
    padding: 5px 10px;
    position: relative;
  }
  .badge {
    margin: 3px 10px;
  }
  .activebadge .el-button {
    background: #0081ff;
    color: #fff;
  }
  ::v-deep .el-table--mini .el-table__cell {
    padding: 3px 0;
  }
}
</style>
