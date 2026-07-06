<template>
  <el-main>
    <el-row :gutter="8" style="width: 100%; height: 100%; margin: 0; padding: 10px 5px">
      <el-col :span="8" style="height: 100%; border-right: 5px solid #000">
        <div class="card_block" style="height: 100%">
          <div class="card_block_title">
            <span v-if="userDeptTree.length == 1">
              {{ userDeptTree[0].name }}
            </span>
            <el-select v-else v-model="selectedWindparkID" placeholder="请选择">
              <el-option
                v-for="item in userDeptTree"
                :key="item.id"
                :label="item.name"
                :value="item.id"
              >
              </el-option>
            </el-select>
          </div>
          <div class="card_block_content">
            <p>
              <el-button
                type="primary"
                v-for="item in allTurbineDAUList"
                :key="item.deviceType"
                @click="changeSelectedDAUType(item.deviceType)"
                size="small"
                :class="[{ activebadge: selectedDAUType == item.deviceType }]"
              >
                {{ item.deviceTypeName }}采集单元</el-button
              >
            </p>
            <avue-crud
              ref="singleTable"
              :option="turbineOption"
              :data="turbineList"
              @row-update="addUpdate"
              @current-row-change="getTurbineDAUList"
              class="custom-table"
            >
            </avue-crud>
          </div>
        </div>
      </el-col>
      <el-col :span="16" style="height: 100%">
        <div
          class="card_block"
          :style="{ height: selectedDAUType == 'CVM' ? '69%' : '100%', maginBottom: '8px' }"
        >
          <div class="card_block_title">{{ currentDeviceName }}机组采集器信息</div>
          <div class="card_block_content">
            <avue-crud
              ref="multipleTable"
              :option="dauOption"
              :data="dauList"
              @row-update="addUpdate1"
              class="custom-table"
            ></avue-crud>
          </div>
        </div>
        <div v-if="selectedDAUType == 'CVM'" class="card_block" style="height: 30%">
          <div class="card_block_title">{{ currentDeviceName }}机组倾角仪（串口服务器）</div>
          <div class="card_block_content">
            <avue-crud
              ref="multipleTable"
              :option="serialServerOption"
              :data="serialServerList"
              @row-update="addUpdate2"
              class="custom-table"
            ></avue-crud>
          </div>
        </div>
      </el-col>
    </el-row>
  </el-main>
</template>
<script>
import { mapGetters } from 'vuex'
import {
  getTurbineTableListApi,
  getTurbineDAUListApi,
  updateHADUChannelMapperIPApi,
  updateHADUChannelNumApi,
  updateHADUModbusApi,
  getModbusListApi
} from '@/api/basicConfig/dau.js'
export default {
  data() {
    return {
      selectedWindparkID: '', // 当前页面显示的风场ID
      selectedDAUType: '', // 左侧展示相关数据的采集器类型

      // 左侧模块数据
      // userDeptTree: [], //风场列表
      allTurbineDAUList: [], //全部机组采集状态列表
      turbineList: [], // 采集类型对应的机组DAU状态列表（左侧）
      turbineOption: {
        rowKey: 'deviceID',
        height: 'auto',
        addBtn: false,
        addRowBtn: false,
        calcHeight: 90,
        cellBtn: true,
        delBtn: false,
        highlightCurrentRow: true,
        menuWidth: 200,
        column: [
          {
            label: '机组名称',
            prop: 'deviceName',
            width: 100,
            cell: false
          },
          {
            label: 'IP地址',
            prop: 'ip',
            cell: true,
            rules: [
              {
                required: true,
                message: '请输入IP地址',
                trigger: 'blur'
              },
              {
                validator: (rule, value, callback) => {
                  if (!this.validateIP(value)) {
                    callback(new Error('请输入正确的IP地址格式'))
                  } else {
                    callback()
                  }
                },
                trigger: 'blur'
              }
            ]
          }
        ]
      },
      // 右上模块数据
      dauList: [],
      currentDeviceName: '', // 标题机组名称
      dauOption: {
        rowKey: 'measlocID',
        height: '100%',
        calcHeight: 300,
        addBtn: false,
        addRowBtn: false,
        cellBtn: true,
        delBtn: false,
        menuWidth: 300,
        column: [
          {
            label: '通道号',
            prop: 'channelNumber',
            width: 300,
            cell: true,
            type: 'select',
            dicData: []
          },
          {
            label: '测量位置',
            prop: 'measlocName',
            cell: false
          }
        ]
      },
      // 右下模块数据
      serialServerList: [],
      serialServerOption: {
        rowKey: 'haduid',
        height: '30vh',
        calcHeight: 30,
        addBtn: false,
        addRowBtn: false,
        cellBtn: true,
        delBtn: false,
        menuWidth: 300,
        column: [
          {
            label: '采集单元名称',
            prop: 'haduid',
            width: 300,
            cell: false
          },
          {
            label: 'Modbus地址',
            prop: 'modbusAddress',
            cell: false
          },
          {
            label: 'ModbusIP',
            prop: 'modbusTCPIP',
            cell: true,
            rules: [
              {
                required: true,
                message: '请输入ModbusIP',
                trigger: 'blur'
              },
              {
                validator: (rule, value, callback) => {
                  if (!this.validateIP(value)) {
                    callback(new Error('请输入正确的IP地址格式'))
                  } else {
                    callback()
                  }
                },
                trigger: 'blur'
              }
            ]
          }
        ]
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
    }
  },
  created() {},
  mounted() {
    this.selectedWindparkID = this.userDeptTree[0].id
  },
  methods: {
    validateIP(ip) {
      const ipRegex =
        /^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/
      return ipRegex.test(ip)
    },
    initWindparkPage() {
      // 清空所有相关数据
      this.currentDeviceName = ''
      this.allTurbineDAUList = []
      this.turbineList = []
      this.dauList = []
      this.serialServerList = []
      // 请求接口
      this.getTurbineTableList()
    },

    // 切换采集类型，左侧列表数据变更
    changeSelectedDAUType(val) {
      if (this.selectedDAUType == val) {
        return
      }
      this.selectedDAUType = val
      this.turbineList = this.allTurbineDAUList.find(i => i.deviceType == val).deviceStatusList
      this.$nextTick(() => {
        this.$refs.singleTable.setCurrentRow(this.turbineList[0])
      })
    },

    // 切换左侧机组，右侧数据变更
    changeCurrentRow(row) {
      this.getTurbineDAUList(row)
    },

    // 机组所有采集数据
    getTurbineTableList() {
      getTurbineTableListApi({
        stationID: this.selectedWindparkID
      })
        .then(data => {
          if (data.data.data.length) {
            this.allTurbineDAUList = data.data.data
            this.selectedDAUType = data.data.data[0].deviceType
            this.turbineList = data.data.data[0].deviceStatusList
            this.$nextTick(() => {
              this.$refs.singleTable.setCurrentRow(this.turbineList[0])
            })
          }
        })
        .catch(error => console.error('Unable to get items.', error))
    },

    // 获取单个机组通道状态列表
    getTurbineDAUList(row) {
      if (!row) return
      this.currentDeviceName = row.deviceName
      getTurbineDAUListApi({
        deviceID: row.deviceID,
        deviceType: this.selectedDAUType
      })
        .then(data => {
          this.dauList = data.data.data
          const list = this.findObject(this.dauOption.column, 'channelNumber')
          let codeOption = []
          if (this.selectedDAUType == 'CVM') {
            codeOption = Array.from({ length: 20 }, (_, index) => index + 1)
          } else {
            codeOption = Array.from({ length: 8 }, (_, index) => index + 1)
          }
          this.dauList.forEach(i => {
            let index = codeOption.indexOf(i.channelNumber)
            if (index > -1) {
              codeOption.splice(index, 1)
            }
          })
          list.dicData = codeOption.map(i => {
            return {
              label: i,
              value: i
            }
          })
        })
        .catch(error => console.error('Unable to get items.', error))
      if (this.selectedDAUType == 'CVM') {
        this.getModbusList(row)
      }
    },
    getModbusList(row) {
      getModbusListApi({
        deviceID: row.deviceID
      }).then(data => {
        this.serialServerList = data.data.data
      })
    },
    // 编辑机组IP地址
    addUpdate(form, index, done, loading) {
      updateHADUChannelMapperIPApi({ ...form }).then(res => {
        if (res.data.success) {
          this.$message.success(res.data.message)
          done()
        } else {
          this.$message.error(res.data.message)
        }
      })
    },
    // 编辑采集单元
    addUpdate1(form, index, done, loading) {
      updateHADUChannelNumApi({ ...form }).then(res => {
        if (res.data.success) {
          this.$message.success(res.data.message)
          done()
        } else {
          this.$message.error(res.data.message)
        }
      })
    },
    // 编辑倾角仪
    addUpdate2(form, index, done, loading) {
      updateHADUModbusApi({ ...form }).then(res => {
        if (res.data.success) {
          this.$message.success(res.data.message)
          done()
        } else {
          this.$message.error(res.data.message)
        }
      })
    }
  }
}
</script>
<style lang="scss" scoped>
:deep(.el-checkbox__label){
  color: #fff !important;
}
:deep(.avue-crud__menu){
  min-height: 0;
}
.el-main {
  height: 100%;
  width: 100%;
  padding: 0;
  overflow: unset;
  color: #fff;
  .custom-table {
    height: 95%;
  }
  .merge_header {
    height: 33px;
    margin-bottom: 3px;
    font-size: 16px;
    font-weight: bold;
    line-height: 33px;
    padding: 0 17px;
    position: relative;
  }
  .card_block {
    width: 100%;
    overflow: hidden;
    // background: #fff;
    border-radius: 5px;
  }

  .card_block .card_block_title {
    font-size: 15px;
    font-weight: bolder;
    height: 40px;
    line-height: 40px;
    padding: 0 15px;
    border-bottom: 1px solid #000;
    position: relative;
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
    color: #fff;
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
    height: calc(100% - 40px);
    overflow: hidden;
    padding: 5px 10px;
  }
  .badge {
    margin: 3px 10px;
  }
  .activebadge {
    background: #0081ff !important;
    color: #fff !important;
    margin-bottom: 10px;
  }
  :deep(.el-table__row:hover){
    background-color: rgba(30, 56, 140, 0.5) !important;
  }
  :deep(.el-table__body tr.hover-row.current-row > td.el-table__cell),
  .el-table__body tr.hover-row.el-table__row--striped.current-row > td.el-table__cell,
  .el-table__body tr.hover-row.el-table__row--striped > td.el-table__cell,
  .el-table__body tr.hover-row > td.el-table__cell {
    background-color: rgba(30, 56, 140, 0.5) !important;
  }
  :deep(.el-table__body tr:hover > td){
    background-color: rgba(30, 56, 140, 0.5) !important;
  }
  :deep(.el-table__body tr.current-row > td.el-table__cell){
    background-color: rgba(30, 56, 140, 0.5) !important;
  }
  :deep(.el-table::before){
    background-color: transparent !important;
  }
}
</style>
