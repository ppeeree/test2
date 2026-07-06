<template>
  <div class="containter_box">
    <div class="merge_header">诊断结果</div>
    <div class="merge_content" style="display: flex">
      <left-tree class="leftTree" @checkData="checkData" ref="leftTree"></left-tree>
      <Splitter splitType="vertical" id="areaSplit" style="flex: 0 0 6px" :limit="{
        left: 40,
        right: 40,
        top: 0,
        bottom: 0
      }" />
      <div class="content_right">
        <div class="right_up">
          <el-row :gutter="10">
            <el-col :span="6">
              <div class="card_title">监测机组</div>
              <div class="card_content">
                <card-item titleText="诊断机组数量" :value="summaryInfo.diagDeviceCount" icon="local-turbine"
                  bgColor="#409EFF" fontColor="#909399">
                </card-item>
                <card-item titleText="预警机组数量" :value="summaryInfo.alarmDeviceCount" icon="local-turbine"
                  bgColor="#F56C6C" fontColor="#909399">
                </card-item>
              </div>
            </el-col>
            <el-col :span="6" style="border-left: 2px solid #ccc; border-right: 2px solid #ccc">
              <div class="card_title">运维要求</div>
              <div class="card_content">
                <card-item @click="selectedItem('危险')" titleText="48小时内检查" :value="summaryInfo.dangerDeviceCount"
                  icon="local-fix" bgColor="#FF0F0D" fontColor="#FF0F0D" :class="[
                    'active_card_item',
                    { active_card_item_active: selectedlevel === '危险' }
                  ]">
                </card-item>
                <card-item @click="selectedItem('警告')" titleText="一周内检查" :value="summaryInfo.warningDeviceCount"
                  icon="local-fix" bgColor="#F37628" fontColor="#F37628" :class="[
                    'active_card_item',
                    { active_card_item_active: selectedlevel === '警告' }
                  ]">
                </card-item>
              </div>
            </el-col>
            <el-col :span="12">
              <div class="card_title">专病类型等级分布</div>
              <div class="card_content">
                <fault-chart ref="chart" @selectedItem="selectedItem" :chartData="chartData" />
              </div>
            </el-col>
          </el-row>
        </div>
        <splitter splitType="horizontal" id="line_split" style="flex: 0 0 6px" />
        <div class="right_bottom" ref="right_bottom">
          <h4>
            智能诊断结果表
            <el-button size="small" plain type="primary" icon="el-icon-download" @click="handleExport">
              导出
            </el-button>
          </h4>
          <avue-crud ref="crud" :page="page" :data="tableData" :option="tableOption" v-loading="loading"
            class="my-avue-crud" @selection-change="handleSelectionChange" @current-change="currentChange"
            @size-change="sizeChange">
            <!-- 自定义操作列 -->
            <template #menu="scope">
              <el-button @click="goToPath(scope.row)"> 诊断详情 </el-button>
            </template>
            <template #alarmLevel="scope">
              <span :style="{ color: levelColor[eventTypeEnum[scope.row.alarmLevel]] }">{{
                scope.row.alarmLevel
              }}</span>
            </template>
            <template #maintenanceRequirement="scope">
              <span :style="{ color: levelColor[eventTypeEnum[scope.row.alarmLevel]] }">{{
                scope.row.maintenanceRequirement
              }}</span>
            </template>
          </avue-crud>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import Splitter from '@/components/splitter/index.vue'
import { eventTypeEnum, levelColor } from '@/util/constant'
import { setTheme } from '@/util/util'
import XLSX from 'xlsx'
import dayjs from 'dayjs'
import leftTree from './compontents/leftTree.vue'
import { getDiagResultListApi, getSummaryInfoApi } from '@/api/intelliDiag'
import FaultChart from './compontents/faultChart.vue'
import CardItem from './compontents/cardItem.vue'
export default {
  components: { leftTree, FaultChart, CardItem, Splitter },
  data() {
    return {
      summaryInfo: {
        diagDeviceCount: 0,
        alarmDeviceCount: 0,
        dangerDeviceCount: 0,
        warningDeviceCount: 0
      },
      chartData: [],
      selectedlevel: '',
      eventTypeEnum,
      levelColor,
      treeNode: [],
      tableData: [],
      tableOption: {
        height: 'calc(100vh - 480px)',
        rowKey: 'faultId',
        tip: false,
        menu: false,
        border: false, // 显示边框
        index: false, // 是否显示索引列
        selection: true, // 显示多选框
        stripe: false, // 是否显示斑马纹
        headerAlign: 'center', // 表头对齐方式
        addBtn: false, // 隐藏新增按钮
        searchShow: false, // 隐藏搜索栏
        columnBtn: false, // 隐藏列显示/隐藏按钮
        refreshBtn: false, // 隐藏刷新按钮
        editBtn: false, // 显示编辑按钮 (对应图中的“诊断详情”)
        delBtn: false, // 隐藏删除按钮
        menuWidth: 200, // 操作栏宽度
        // 列定义
        column: [
          {
            label: '风场名称',
            prop: 'stationName',
            minWidth: 120
          },
          {
            label: '机组名称',
            prop: 'deviceName',
            width: 100
          },
          {
            label: '故障部件',
            prop: 'compName',
            width: 100
          },
          {
            label: '诊断结论',
            prop: 'diagnosisConclusion',
            align: 'left',
            minWidth: 220,
            showOverflowTooltip: true // 内容过长隐藏并显示 tooltip
          },
          {
            label: '预警等级',
            prop: 'alarmLevel',
            width: 100,
            slot: true // 开启插槽以自定义颜色
          },
          {
            label: '专病类型',
            prop: 'sdpTypeName',
            minWidth: 130
          },
          {
            label: '维护建议',
            prop: 'maintenanceSuggestion',
            align: 'left',
            minWidth: 240,
            showOverflowTooltip: true // 内容过长隐藏并显示 tooltip
          },
          {
            label: '运维要求',
            prop: 'maintenanceRequirement',
            align: 'center',
            width: 160,
            slot: true
          },
          {
            label: '样本时间',
            prop: 'acqTime',
            align: 'center',
            width: 160
          },
          {
            label: '操作',
            prop: 'menu', // 操作列固定 prop 为 menu
            width: 120,
            slot: true // 开启插槽以自定义按钮
          }
        ]
      },
      page: {
        total: 0,
        currentPage: 1,
        pageSize: 10
      },
      // 3. 【核心】全局维护的选中行集合
      selectedRows: [],
      isPageChanging: false, // 翻页标记
      loading: false
    }
  },
  created() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
  },
  watch: {},
  mounted() {
    this.initResizeObserver() // 初始化resizeObserver
  },
  beforeUnmount() {
    // 组件销毁前的清理工作，防止内存泄漏
    if (this.resizeObserver) {
      this.resizeObserver.disconnect()
    }
    if (this.resizeTimer) {
      clearTimeout(this.resizeTimer)
    }
  },
  methods: {
    // 初始化resizeObserver
    initResizeObserver() {
      this.resizeObserver = new ResizeObserver(entries => {
        // 防抖处理：避免高频触发导致页面卡顿
        if (this.resizeTimer) clearTimeout(this.resizeTimer)

        this.resizeTimer = setTimeout(() => {
          // 获取父级容器的最新高度
          const parentHeight = entries[0].contentRect.height
          this.tableOption.height = parentHeight - 120 + 'px'
          this.$nextTick(() => {
            setTimeout(() => {
              if (this.$refs.crud) {
                this.$refs.crud.doLayout()
              }
            }, 100)
          })
        }, 100) // 100ms 防抖延迟
      })
      // 开始监听父级容器
      if (this.$refs.right_bottom) {
        this.resizeObserver.observe(this.$refs.right_bottom)
      }
    },
    // 点击数据查询按钮进行数据查询
    checkData(node, timeRange) {
      this.treeNode = node
      this.timeRange = timeRange
      this.page.currentPage = 1
      // 右侧数据初始化
      if (!this.treeNode.length) {
        this.$message.error('请先选择风场')
        return
      }
      this.getSummaryInfo()
      this.getList()
    },
    getSummaryInfo() {
      getSummaryInfoApi({
        stationIDs: this.treeNode?.join(','),
        startTime:
          this.timeRange?.[0]
            ? dayjs(this.timeRange[0]).startOf('day').format('YYYY-MM-DD HH:mm:ss')
            : '',
        endTime: this.timeRange?.[1]
          ? dayjs(this.timeRange[1]).endOf('day').format('YYYY-MM-DD HH:mm:ss')
          : ''
      }).then(res => {
        if (res.data.success) {
          const {
            alarmDeviceCount,
            dangerDeviceCount,
            warningDeviceCount,
            diagDeviceCount,
            faultTypeDegrees
          } = res.data.data
          this.summaryInfo = {
            alarmDeviceCount,
            dangerDeviceCount,
            warningDeviceCount,
            diagDeviceCount
          }
          let dataList = [[], [], []] // 注意、警告、危险serise数组
          faultTypeDegrees.forEach(item => {
            const { attentionDeviceCount, warningDeviceCount, dangerDeviceCount } = item
            dataList[0].push(attentionDeviceCount)
            dataList[1].push(warningDeviceCount)
            dataList[2].push(dangerDeviceCount)
          })
          this.chartData = dataList
        } else {
          this.$message.error(res.data.message)
        }
      })
    },
    // 获取列表数据
    getList() {

      this.loading = true
      getDiagResultListApi({
        pageNum: this.page.currentPage,
        pageSize: this.page.pageSize,
        stationIDs: this.treeNode?.join(','),
        startTime:
          this.timeRange?.[0]
            ? dayjs(this.timeRange[0]).startOf('day').format('YYYY-MM-DD HH:mm:ss')
            : '',
        endTime: this.timeRange?.[1]
          ? dayjs(this.timeRange[1]).endOf('day').format('YYYY-MM-DD HH:mm:ss')
          : '',
        faultType:
          this.selectedlevel.length && this.selectedlevel.split('_').length > 1
            ? this.selectedlevel.split('_')[0]
            : '',
        alarmLevel:
          this.selectedlevel.length && this.selectedlevel.split('_').length > 1
            ? this.eventTypeEnum[this.selectedlevel.split('_')[1]]
            : this.eventTypeEnum[this.selectedlevel]
      })
        .then(res => {
          if (res.data.success) {
            const { data, totalCount } = res.data.data
            this.tableData = data
            this.page.total = totalCount
            this.$nextTick(() => {
              this.restoreSelection()
              // 恢复完成后，解锁标记（稍微延迟一点，确保事件触发完）
              this.isPageChanging = false
            })
          } else {
            this.$message.error(res.data.message)
          }
        })
        .finally(() => {
          this.loading = false
        })
    },
    // 选中项
    selectedItem(level) {
      if (
        this.selectedlevel.length &&
        this.selectedlevel.split('_').length > 1 &&
        level.length &&
        level.split('_').length == 1
      ) {
        this.$refs.chart.clearSelected()
      }
      this.selectedlevel = this.selectedlevel === level ? '' : level
      this.page.currentPage = 1
      this.getList()
    },
    /**
     * 核心：恢复当前页的勾选状态
     * 当翻页回来时，表格是空的，需要根据 selectedRows 重新勾选
     */
    restoreSelection() {
      const crudRef = this.$refs.crud
      if (!crudRef) return

      // 1. 找出当前页中，哪些行在全局选中池里
      const rowsToSelect = this.tableData.filter(item =>
        this.selectedRows.some(selected => selected.faultId === item.faultId)
      )

      // 2. 先清空当前表格的选中状态（防止残留）
      crudRef.toggleSelection([])

      // 3. 重新勾选那些应该被选中的行
      rowsToSelect.forEach(row => {
        crudRef.toggleSelection([row])
      })
    },
    handleSelectionChange(selection) {
      // 【核心】如果是翻页过程中触发的清空事件，直接忽略
      if (this.isPageChanging && selection.length === 0) {
        return
      }
      // 1. 获取当前页所有 ID
      const currentPageIds = this.tableData.map(item => item.faultId)

      // 2. 从全局池中，移除所有属于当前页的旧数据
      // 为什么？因为用户可能取消了某些勾选，我们需要用最新的 selection 覆盖当前页的状态
      this.selectedRows = this.selectedRows.filter(row => !currentPageIds.includes(row.faultId))

      // 3. 将当前页新选中的数据加入全局池
      this.selectedRows.push(...selection)

      // 调试用
      // console.log('当前全局选中行数:', this.selectedRows.length)
    },
    // 导出功能
    handleExport() {
      if (this.selectedRows.length == 0) {
        return this.$message.warning('请先选择要导出的数据')
      }
      // 1. 定义表头映射 (key: 对应prop, title: Excel表头名称)
      const headers = [
        { key: 'stationName', title: '风场名称', width: 15 },
        { key: 'deviceName', title: '机组名称', width: 15 },
        { key: 'compName', title: '故障部件', width: 15 },
        { key: 'diagnosisConclusion', title: '诊断结论', width: 60 },
        { key: 'alarmLevel', title: '预警等级', width: 15 },
        { key: 'sdpTypeName', title: '专病类型', width: 30 },
        { key: 'maintenanceSuggestion', title: '维护建议', width: 60 },
        { key: 'maintenanceRequirement', title: '运维要求', width: 15 },
        { key: 'acqTime', title: '样本时间', width: 20 }
      ]

      // 2. 格式化数据，将中文表头映射到数据上
      const data = this.selectedRows.map(item => {
        let row = {}
        headers.forEach(h => {
          row[h.title] = item[h.key]
        })
        return row
      })

      // 3. 创建工作簿
      const ws = XLSX.utils.json_to_sheet(data)
      // 只能设置列宽！原版 xlsx 设置了 border 也不会生效
      ws['!cols'] = headers.map(h => ({ wch: h.width }))
      const wb = XLSX.utils.book_new()
      XLSX.utils.book_append_sheet(wb, ws, '诊断记录')

      // 4. 生成文件并下载
      let checkedNames = this.$refs.leftTree.getCheckedNodesFirstLevelNames()
      const fileName = `${checkedNames.length > 1 ? checkedNames.join('&') : checkedNames[0]
        }_智能诊断结果表_${dayjs().format('YYYYMMDDHHmmss')}.xlsx`
      // const fileName = `智能诊断结果表_${dayjs().format('YYYYMMDDHHmmss')}.xlsx`
      XLSX.writeFile(wb, fileName)
    },
    // 分页变化
    currentChange(currentPage) {
      this.isPageChanging = true // 标记开始翻页
      this.page.currentPage = currentPage
      this.getList()
    },

    sizeChange(pageSize) {
      this.page.currentPage = 1
      this.page.pageSize = pageSize
      this.getList()
    },

    goToPath(row) {
      console.log(row)
      return
      this.$router.push({
        path: '/customAnalysisNew/index'
        // query: { id: row.measlocId, acqTime: row.acqTime }
      })
      this.setSessionItem('checkData', JSON.stringify(row))
    }
  }
}
</script>
<style scoped lang="scss">
.containter_box {
  width: 100%;
  height: 100%;

  .merge_header {
    height: 33px;
    margin-bottom: 3px;
    font-size: 16px;
    font-weight: bold;
    line-height: 33px;
    padding: 0 17px;
    position: relative;

    &::after {
      content: '';
      width: 100%;
      height: 3px;
      background: #ccc;
      position: absolute;
      left: 0;
      bottom: 0;
    }
  }

  .merge_content {
    width: 100%;
    height: calc(100% - 33px);
    display: flex;

    .leftTree {
      flex: 0 0 280px;
      overflow: hidden;
      height: 100%;
    }

    .content_right {
      flex: 1 1 0%;
      overflow: hidden;
      height: 100%;
      display: flex;
      flex-direction: column;
      min-height: 0;

      h3 {
        width: 100%;
        height: 30px;
        line-height: 30px;
        font-size: 14px;
        padding: 0 15px;
        position: relative;
        font-weight: normal;

        &::after {
          content: '';
          width: 100%;
          height: 2px;
          background: #ccc;
          position: absolute;
          left: 0;
          bottom: 0;
        }
      }

      h4 {
        width: 100%;
        height: 30px;
        line-height: 30px;
        font-size: 16px;
        margin: 10px 0;
        padding: 0 15px;
        color: #333b69;
        flex: 0 0 auto;

        .el-button {
          float: right;
          right: 20px;
          top: 10px;
          cursor: pointer;
        }
      }

      .right_up {
        width: 100%;
        flex: 0 0 240px;
        overflow: hidden;

        .el-row {
          width: 100%;
          height: 100%;

          .el-col {
            height: 100%;

            .card_title {
              width: 100%;
              line-height: 30px;
              font-size: 16px;
              padding: 0 15px;
              position: relative;
              font-weight: normal;
            }

            .card_content {
              width: 100%;
              height: calc(100% - 30px);
              display: flex;
              flex-direction: row;
              justify-content: space-around;
              align-items: center;
            }

            .active_card_item {
              cursor: pointer;
              border: 2px solid transparent;

              &:hover {
                background: #e4f4ff;
                border: 2px solid #59a9ff;
              }
            }

            .active_card_item_active {
              background: #e4f4ff;
              border: 2px solid #59a9ff;
            }
          }
        }
      }

      .right_bottom {
        width: 100%;
        flex: 1 1 0%;
        overflow: hidden;
        min-height: 0;

        :deep(.my-avue-crud){
          flex: 1;
          height: calc(100% - 50px);
        }

        :deep(.avue-crud__menu){
          display: none !important;
        }

        /*     :deep(.my-avue-crud){
          flex: 1;
          height: calc(100% - 50px);
          display: flex;
          flex-direction: column;
          overflow: hidden;
          .el-card:nth-child(2) {
            height: calc(100% - 100px);
            .el-card__body {
              height: 100%;
              .el-form {
                height: 100%;
              }
            }
          }
        }

        :deep(.my-avue-crud .el-table__header-wrapper){
          flex: 0 0 auto;
        }
        :deep(.my-avue-crud .el-table){
          flex: 1;
          height: 100%;
          min-height: 0;
          display: flex;
          flex-direction: column;
          overflow: hidden;
        }

        :deep(.my-avue-crud .el-table__body-wrapper){
          flex: 1;
          height: 100%;
          overflow-y: auto !important;
        }
      }
    }*/
      }
    }
  }
}
</style>
