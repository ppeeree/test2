<template>
  <basic-container class="alarm_table">
    <span class="table_title"
      >{{ clickItem.fatherName }}<span v-show="clickItem.fatherName">_</span
      >{{ clickItem.modelName }}</span
    >
    <!-- <span class="table_title" style="position: relative; top: 37px; left: -17%"
      >震动报警设置列表
    </span> -->
    <span style="float: right; margin-top: 10px; margin-bottom: 10px">
      <el-button type="primary " size="small" plain icon="el-icon-plus" @click="submitRole"
        >新 增
      </el-button></span
    >
    <el-table :data="data" border style="width: 100%" height="307">
      <el-table-column prop="compCodeName" label="部件"> </el-table-column>
      <el-table-column prop="sectionNameRe" label="截面"> </el-table-column>
      <el-table-column prop="orientationName" label="方向"> </el-table-column>
      <el-table-column prop="paraUnitName" label="物理量"> </el-table-column>
      <el-table-column prop="evCodeName" label="特征值" width="300px"> </el-table-column>
      <el-table-column prop="power" label="功率"> </el-table-column>
      <el-table-column prop="speet" label="转速"> </el-table-column>
      <el-table-column prop="normal" label="正常"> </el-table-column>
      <el-table-column prop="attention" label="注意"> </el-table-column>
      <el-table-column prop="warning" label="警告"> </el-table-column>
      <el-table-column prop="danger" label="危险"> </el-table-column>
      <el-table-column label="操作" width="200px">
        <template slot-scope="scope">
          <span class="edit">
            <span class="editImg" />
            <span @click="editItem(scope.row)" class="editBtn">编 辑</span></span
          >
          <span class="edit">
            <i class="el-icon-delete"></i>
            <span @click="delteItem(scope.row)" class="editBtn">删 除</span>
          </span>
        </template>
      </el-table-column>
    </el-table>
    <addAlarmCard ref="addCardTree" @onLoad="onLoad"></addAlarmCard>
  </basic-container>
</template>

<script>
import { getList, remove } from '@/api/alarmFault/alarmModel'
import addAlarmCard from './addAlarmCard.vue'
export default {
  components: { addAlarmCard },
  data() {
    return {
      clickItem: {}, //左侧树选中
      FAddBox: false,
      query: {},
      data: [],
      page: {
        total: 0, // 总页数
        currentPage: 1, // 当前页数
        pageSize: 100 // 每页显示多少条
      }
    }
  },
  mounted() {
    this.$bus.$on('selectModelItem', val => {
      this.clickItem = val
      this.onLoad(this.page)
    })
  },
  methods: {
    //表格--加载
    onLoad(page, params = {}) {
      let obj = {
        offset: page.currentPage,
        pageSize: page.pageSize,
        alarmModelId: this.clickItem.id,
        ...Object.assign(params, this.query)
      }
      this.loading = true

      if (this.clickItem.id) {
        getList({ ...obj }).then(res => {
          this.loading = false
          const data = res.data.data.data
          if (res.data.code == 200 && data.length) {
            if (data.length) {
              this.data = this.handlerList(data)
            }
          } else {
            this.data = []
          }
          this.$bus.$emit('tableList', this.data)
        })
      }
    },
    //处理表格数据
    handlerList(list) {
      list.forEach(item => {
        let { evCond, alarmTypeLimits } = item

        item.normal = -1
        item.attention = -1
        item.warning = -1
        item.danger = -1
        alarmTypeLimits.forEach(o => {
          if (o.alarmThresholdLimitB === null) {
            item[o.alarmTypeCode] = '-∞' + ',' + o.alarmThresholdLimitA
          } else if (o.alarmThresholdLimitA === null) {
            item[o.alarmTypeCode] = o.alarmThresholdLimitB + ',' + '+∞'
          } else {
            item[o.alarmTypeCode] = o.alarmThresholdLimitB + ',' + o.alarmThresholdLimitA
          }
        })

        item.power = evCond.wkPower.evCondDown + '-' + evCond.wkPower.evCondUp
        item.speet = evCond.spd.evCondDown + '-' + evCond.spd.evCondUp

        for (let key in item) {
          if (item[key] == '' || item[key] == -1 || item[key] == '-1--1') {
            item[key] = '-'
          }
        }
      })

      return list
    },
    //表格--新增
    submitRole() {
      this.$refs.addCardTree.addBtn('新 增', false, this.clickItem.id)
    },
    //表格--编辑
    editItem(row) {
      if (this.clickItem.isSystem === 0) {
        this.$refs.addCardTree.addBtn('编 辑', false, this.clickItem.id, row)
      } else {
        this.$refs.addCardTree.addBtn('编 辑', true, this.clickItem.id, row) //仅查看
      }
    },
    //表格--删除
    delteItem(val) {
      this.$confirm('确定删除数据?', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
        .then(() => {
          return remove({ ids: val.id })
        })
        .then(() => {
          this.data = []
          this.onLoad(this.page)
          this.$message({
            type: 'success',
            message: '数据删除成功!'
          })
        })
    }
  }
}
</script>

<style lang="less" scoped>
@import url('./style.less');

.alarm_table {
  height: 100%;
  width: calc(100% - 430px);
  float: right;
  .table_title {
    font-size: 14px;
    font-weight: bold;
    color: white;
  }
  ::v-deep .avue-crud {
    height: 330px;
  }
  ::v-deep .avue-crud--card .el-card + .el-card {
    height: 330px;
  }
}
::v-deep .el-table__empty-block {
  height: 39% !important;
  .el-table__empty-text {
    height: 100%;
    .avue-crud__empty {
      padding: 30px 0;
    }
  }
}
::v-deep .avue-crud__left {
  position: relative;
  right: -91%;
}
::v-deep .el-loading-mask {
  height: 310px;
}
::v-deep .el-table__fixed-body-wrapper,
.el-table__body-wrapper {
  height: 265px !important;
}
::v-deep .el-table--border {
  margin-top: 18px;
}

::v-deep .el-table--enable-row-transition .el-table__body td.el-table__cell {
  padding: 8px 0 !important;
}

::v-deep .el-table__row:hover {
  background-color: rgba(30, 56, 140, 0.5) !important;
}
::v-deep .el-table__body tr.hover-row.current-row > td.el-table__cell,
.el-table__body tr.hover-row.el-table__row--striped.current-row > td.el-table__cell,
.el-table__body tr.hover-row.el-table__row--striped > td.el-table__cell,
.el-table__body tr.hover-row > td.el-table__cell {
  background-color: rgba(30, 56, 140, 0.5) !important;
}
::v-deep .el-table__body tr:hover > td {
  background-color: rgba(30, 56, 140, 0.5) !important;
}
</style>
