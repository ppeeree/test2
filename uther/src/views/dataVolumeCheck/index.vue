<template>
  <el-main class="el-main">
    <div class="merge_header">数据量检查</div>
    <div style="width: 100%; height: calc(100% - 33px)" class="card_block">
      <div class="card_block_title">
        <span> 筛选: </span>
        <el-select
          size="small"
          style="width: 300px; margin-right: 10px"
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
        <el-date-picker
          size="small"
          v-model="dataMonth"
          type="month"
          value-format="YYYY-MM"
          placeholder="选择月"
        >
        </el-date-picker>
      </div>
      <el-table
        size="mini"
        :data="tableData"
        border
        ref="myTable"
        style="width: 100%; background-color: #fff"
        height="75%"
      >
        <!-- 第一列：机组 (1#, 2#...) -->
        <!--  <el-table-column prop="deviceName" fixed label="机组" width="50"> </el-table-column>

          <el-table-column prop="compName" fixed label="部件" width="100"> </el-table-column> -->
        <el-table-column
          prop="deviceName"
          align="left"
          label="机组-部件"
          show-overflow-tooltip
          width="140"
        >
          <template #default="scope">
            {{ scope.row.deviceName }}-{{ scope.row.compName }}
          </template>
        </el-table-column>

        <!-- 右侧动态列：1-30 -->
        <el-table-column v-for="(col, index) in columns" :key="index" :label="col" min-width="40">
          <template #default="scope">
            <!-- 这里的 index 对应 columns 的索引，取出对应的数据 -->
            <span
              :style="{
                display: 'inline-block',
                width: '100%',
                height: '100%',
                'background-color': scope.row.counts[index] == 0 ? 'red' : '',
                color: scope.row.counts[index] == 0 ? '#fff' : ''
              }"
              >{{ scope.row.counts[index] }}</span
            >
          </template>
        </el-table-column>
      </el-table>
      <div style="height: calc(25% - 40px); overflow: hidden">
        <h4>机组无数据日期汇总：</h4>
        <div
          style="
            height: calc(100% - 30px);
            overflow: auto;
            padding: 5px;
            border: 1px solid #ccc;
            font-size: 12px;
          "
        >
          <div v-for="(item, index) in tableData" :key="index">
            <template v-if="item.counts.filter(p => p == 0).length > 0">
              {{ item.deviceName }}-{{ item.compName }}：
              <span v-for="(p, m) in item.counts" :key="m">{{
                p == 0 ? columns[m] + '、' : ''
              }}</span>
            </template>
          </div>
        </div>
      </div>
    </div>
  </el-main>
</template>
<script>
import { setTheme } from '@/util/util'
import { mapGetters } from 'vuex'
import { getCheckDataApi } from '@/api/dataManage/index.js'
import dayjs from 'dayjs'
export default {
  components: {},
  data() {
    return {
      dataMonth: dayjs().subtract(1, 'month').format('YYYY-MM'), // 默认选中上个月
      tableData: [],
      columns: [],
      selectedWindparkID: '' // 当前页面显示的风场ID
    }
  },
  computed: {
    ...mapGetters(['userDeptTree'])
  },
  watch: {
    selectedWindparkID: {
      handler() {
        this.initTableData()
      }
    },
    dataMonth: {
      handler() {
        this.initTableData()
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
    /*  objectSpanMethod({ row, rowIndex, columnIndex }) {
      if (columnIndex === 0) {
        return {
          rowspan: this.tableData.filter(item => item.deviceName === row.deviceName).length,
          colspan: 1
        }
      } else {
        return {
          rowspan: 0,
          colspan: 0
        }
      }
    }, */
    initTableData() {
      getCheckDataApi({
        stationID: this.selectedWindparkID,
        time: dayjs(this.dataMonth).format('YYYY-MM')
      }).then(res => {
        const { data, dates } = res.data.data
        this.columns = dates
        this.tableData = data
        // 2. 在 DOM 更新后，强制表格重新计算布局,解决重新渲染columns后,表格宽度高度不适应问题
        this.$nextTick(() => {
          if (this.$refs.myTable) {
            this.$refs.myTable.doLayout()
          }
        })
      })
    }
  }
}
</script>
<style lang="scss" scoped>
:deep(.el-checkbox__label){
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
    padding: 0 10px 10px 10px;
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
  /*  .card_block .card_block_title::before {
    content: '';
    position: absolute;
    left: 10px;
    top: 8px;
    width: 4px;
    height: 25px;
    border-radius: 0px 2px 2px 0px;
    opacity: 1;
    background: linear-gradient(171deg, rgb(0, 129, 255) 0%, #22cce2 101%);
  } */
  :deep(.el-table tr){
    background-color: #fff !important;
  }
}
</style>
