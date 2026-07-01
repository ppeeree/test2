<template>
  <div class="containter_box">
    <div class="merge_header">诊断记录</div>
    <div class="merge_content">
      <left-tree class="leftTree" @clickedNode="getCheckedData" ref="tree"></left-tree>
      <Splitter splitType="vertical" id="areaSplit" style="flex: 0 0 6px" :limit="{}" />
      <div class="content_right">
        <div style="width: 100%; height: calc(100% - 35px); padding: 0 15px">
          <h4>智能诊断记录表</h4>
          <!-- 表格 -->
          <el-table ref="table" :data="tableData" border stripe size="small" style="width: 100%" height="85%"
            v-loading="loading">
            <el-table-column prop="stationName" label="风场名称" width="200" />
            <el-table-column prop="deviceName" label="机组名称" width="100" />
            <el-table-column prop="compName" label="故障部件" width="100" />
            <el-table-column prop="diagnosisConclusion" label="诊断结论" min-width="300" align="left"
              show-overflow-tooltip />
            <!-- 预警等级 - 带筛选 -->
            <el-table-column prop="alarmLevel" label="预警等级" width="180" column-key="alarmLevel">
              <!-- 自定义筛选下拉菜单 -->
              <template slot="header" slot-scope="{ column }">
                <div style="padding: 10px">
                  <span>{{ column.label }}</span>

                  <el-popover placement="bottom-start" width="80" trigger="click">
                    <div class="white-popover">
                      <el-checkbox-group v-model="alarmLevel" @change="handleFilterChange">
                        <el-checkbox v-for="item in alarmFilters" :key="item.value" :label="item.value"
                          style="display: block; margin: 5px 0">
                          <!-- 根据值动态改变文字颜色 -->
                          <span :style="{ color: item.color }">{{ item.text }}</span>
                        </el-checkbox>
                      </el-checkbox-group>
                    </div>
                    <img slot="reference" src="img/filter.png" alt=""
                      style="width: 12px; height: 12px; margin-left: 5px" />
                  </el-popover>
                </div>
              </template>
              <template slot-scope="{ row }">
                <span :style="{ color: levelColor[eventTypeEnum[row.alarmLevel]] }">{{
                  row.alarmLevel
                }}</span>
              </template>
            </el-table-column>

            <!-- 专病类型 - 带筛选 -->
            <el-table-column prop="sdpTypeName" label="专病类型" width="240" column-key="sdpTypeName" />
            <el-table-column prop="acqTime" label="样本时间" width="260" />
          </el-table>

          <!-- 分页 -->
          <el-pagination class="pagination" background :current-page="page.currentPage" :page-sizes="[15, 30, 50, 100]"
            :page-size="page.pageSize" layout="total, sizes, prev, pager, next, jumper" :total="page.total"
            @size-change="sizeChange" @current-change="currentChange" />
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import { eventTypeEnum, levelColor } from '@/util/constant'
import { setTheme } from '@/util/util'
import leftTree from './leftTree.vue'
import dayjs from 'dayjs'
import Splitter from '@/components/splitter/index.vue'
import { getLastIntelliDiagRecordsApi, getIntelliDiagRecordsApi } from '@/api/intelliDiag'
export default {
  components: { leftTree, Splitter },
  data() {
    return {
      eventTypeEnum,
      levelColor,
      tableData: [],
      page: {
        total: 0,
        currentPage: 1,
        pageSize: 15
      },
      loading: false,
      alarmLevel: ['危险', '警告', '注意', '正常'],
      // 筛选选项
      alarmFilters: [
        { text: '危险', value: '危险', color: '#FF0F0D' },
        { text: '警告', value: '警告', color: '#FF6B0E' },
        { text: '注意', value: '注意', color: '#FFE604' },
        { text: '正常', value: '正常', color: '#2ED133' }
      ],
      filterParams: {}
    }
  },

  created() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
  },
  mounted() { },
  methods: {
    getCheckedData(nodeIDs, isLatest, time) {
      this.page.currentPage = 1
      this.getList(nodeIDs, isLatest, time)
    },
    // 获取列表数据
    getList(nodeIDs, isLatest, time) {
      if (nodeIDs) {
        this.filterParams = { nodeIDs, isLatest, timeRange: time }
      }


      const { nodeIDs: deviceIDs, timeRange } = this.filterParams
      const isLatestFlag = this.filterParams.isLatest === '1'
      if (!deviceIDs.length) {
        this.$message.error('请先选择机组！')
        return
      }
      this.loading = true
      // 选择 API
      const api = isLatestFlag ? getLastIntelliDiagRecordsApi : getIntelliDiagRecordsApi

      // 构建请求参数
      const params = {
        pageNum: this.page.currentPage,
        pageSize: this.page.pageSize,
        deviceIDs: deviceIDs || [],
        alarmTypes: this.alarmLevel,
        ...(isLatestFlag
          ? {}
          : {
            startTime: timeRange?.[0]
              ? dayjs(timeRange[0]).startOf('day').format('YYYY-MM-DD HH:mm:ss')
              : '',
            endTime: timeRange?.[1]
              ? dayjs(timeRange[1]).endOf('day').format('YYYY-MM-DD HH:mm:ss')
              : ''
          })
      }

      api(params)
        .then(res => {
          const { success, message } = res.data
          if (success) {
            const { data, totalCount } = res.data.data
            this.tableData = data
            this.page.total = totalCount
          } else {
            this.$message.error(message)
          }
        })
        .finally(() => {
          this.loading = false
        })
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
    handleFilterChange(val) {
      this.alarmLevel = val
      this.page.currentPage = 1
      this.getList()
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

        .el-button {
          float: right;
          right: 20px;
          top: 10px;
          cursor: pointer;
        }
      }
    }
  }

  ::v-deep .avue-crud__menu {
    display: none !important;
  }
}
</style>
<style lang="scss">
.white-popover {
  background-color: #fff;
  padding: 10px;
}
</style>
