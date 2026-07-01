<template>
  <el-main>
    <div class="merge_header">数据下载</div>
    <el-tabs type="border-card" @tab-click="changeTab">
      <el-tab-pane label="诊断数据下载">
        <!-- 诊断数据下载 -->
        <div class="left_download">
          <!-- 左侧：时间筛选 -->
          筛选：
          <el-date-picker
            v-model="dayZipTimeRange"
            type="daterange"
            :picker-options="timePickerOptions"
            range-separator="~"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            align="right"
            value-format="yyyy-MM-dd"
            size="small"
            @change="daySelectChange"
          ></el-date-picker>

          <!-- 右侧：按钮组 -->
          <el-button style="margin-left: 10px" type="primary" size="small" @click="DownloadDayZIP">
            <i class="el-icon-download" style="padding-right: 7px"></i>批量下载
          </el-button>
          <!--批量删除按钮隐藏-->
          <!--<el-button type="primary" @click="DeleteDayZIP">
             <i class="el-icon-delete" style="padding-right: 7px"></i>批量删除
         </el-button>-->
          <!-- 表格 -->
          <el-table
            ref="multipleTable"
            :data="dayTableList"
            height="95%"
            style="width: 100%; margin-top: 8px"
            border
            :highlight-current-row="true"
            row-key="fileName"
            :header-cell-style="{
              backgroundColor: '#F2F6FC',
              color: '#909399',
              textAlign: 'center'
            }"
            @selection-change="handleSelectionChange"
            class="tableLimit"
            size="mini"
          >
            <el-table-column type="selection" width="55"></el-table-column>
            <el-table-column type="index" label="编号" :index="indexMethod" width="60">
            </el-table-column>
            <el-table-column prop="fileTime" label="数据包时间"></el-table-column>
            <el-table-column prop="fileName" label="文件名称"></el-table-column>
            <el-table-column prop="turbineNum" label="机组数量"></el-table-column>
            <el-table-column prop="fileMemory" label="文件大小"></el-table-column>
          </el-table>
        </div>
      </el-tab-pane>
      <el-tab-pane label="自定义下载">
        <!-- 手动下载 -->
        <div class="right_download">
          <el-form :model="ruleForm" :rules="rules" ref="ruleForm" label-width="120px">
            <el-form-item label="风场：" prop="windparkID" :required="true">
              <el-select
                v-model="ruleForm.windparkID"
                placeholder="请选择"
                @change="windparkSelectChange"
              >
                <el-option
                  v-for="item in windparkList"
                  :key="item.id"
                  :label="item.name"
                  :value="item.id"
                >
                </el-option>
              </el-select>
            </el-form-item>

            <el-form-item label="机组：" prop="windturbineIDs" :required="true">
              <el-button-group>
                <el-button size="small" plain @click="handleCheckedAll">全选</el-button>
                <el-button size="small" plain @click="handleCheckedNone">全不选</el-button>
                <el-button size="small" plain @click="handleCheckedReverse">反选</el-button>
              </el-button-group>
              <el-checkbox-group
                style="line-height: 24px; width: 60%"
                v-model="ruleForm.windturbineIDs"
              >
                <el-checkbox
                  v-for="item in turbineList"
                  :label="item.entityId"
                  :key="item.entityId"
                >
                  {{ item.entityName }}
                </el-checkbox>
              </el-checkbox-group>
            </el-form-item>

            <el-form-item label="时间范围：" prop="itemTime" :required="true">
              <el-date-picker
                v-model="ruleForm.itemTime"
                type="datetimerange"
                :picker-options="timePickerOptions"
                range-separator="至"
                start-placeholder="开始日期"
                end-placeholder="结束日期"
                align="right"
                value-format="yyyy-MM-dd HH:mm:ss"
                :default-time="['00:00:00', '23:59:59']"
              >
              </el-date-picker>
            </el-form-item>

            <el-form-item label="部件：" prop="measType" :required="true">
              <el-checkbox-group v-model="ruleForm.measType">
                <el-checkbox v-for="item in measTypeList" :label="item.key" :key="item.key">
                  {{ item.value }}
                </el-checkbox>
              </el-checkbox-group>
            </el-form-item>
            <el-form-item label="波形组数：" prop="waveNum">
              <el-input-number
                controls-position="right"
                style="margin-right: 5px; width: 120px"
                v-model="ruleForm.waveNum"
                placeholder=""
              ></el-input-number>
            </el-form-item>
            <el-form-item label="数据格式：" prop="waveSaveType">
              <el-select v-model="ruleForm.waveSaveType">
                <el-option
                  v-for="item in downloadSaveTypeList"
                  :key="item.key"
                  :label="item.value"
                  :value="item.key"
                >
                </el-option>
              </el-select>
            </el-form-item>
          </el-form>

          <el-button
            style="margin-left: 10px"
            size="small"
            type="primary"
            @click.native="addTask('ruleForm')"
          >
            数据打包
          </el-button>

          <el-card class="download_task">
            <div slot="header" class="clearfix">
              <span>自定义下载日志</span>
            </div>
            <div v-loading="loading">
              <p v-for="item in logsList" :key="item">{{ item }}</p>
            </div>
          </el-card>

          <div v-if="filePath.length" class="file_button">
            <el-button @click="downloadFile(filePath)">
              <el-image style="width: 40px; height: 40px" src="/img/zip.png" fit="fill"></el-image>
              <p style="float: right; margin-top: 15px; margin-left: 10px">
                {{ this.extractFileName(this.filePath) }}
              </p>
            </el-button>
          </div>
        </div>
      </el-tab-pane>
    </el-tabs>
  </el-main>
</template>
<script>
import { setTheme } from '@/util/util'
import dayjs from 'dayjs'
import {
  getDayTableListApi,
  downloadCSVFileApi,
  deleteTaskApi,
  getMeasTypeListApi,
  addTaskApi,
  getDownloadLogsApi,
  getDownloadWaveSaveTypesApi
} from '@/api/basicConfig/dataDownLoad'
import { mapGetters } from 'vuex'
export default {
  data() {
    const low = (rule, value, callback) => {
      if (value <= 0) {
        callback(new Error('波形数量必须大于0'))
      } else {
        callback()
      }
    }
    return {
      dayTableList: [],
      dayZipTimeRange: [], // 诊断数据下载时间范围
      selectTaskList: [], // 选中的压缩包
      windparkList: [], //风场列表
      turbineList: [], // 该风场下的机组列表
      measTypeList: [], // 部件类型
      downloadSaveTypeList: [], // 保存格式类型
      downloadLog: [], // 自定义下载日志

      rules: {
        windparkID: [{ required: true, message: '请选择风场', trigger: 'change' }],
        windturbineIDs: [{ required: true, message: '请选择机组', trigger: 'change' }],
        measType: [{ required: true, message: '请选择部件', trigger: 'change' }],
        dataType: [{ required: true, message: '请选择数据类型', trigger: 'change' }],
        itemTime: [{ required: true, message: '请选择时间范围', trigger: 'blur' }],
        waveNum: [
          { required: true, message: '请输入波形数量', trigger: 'blur' },
          { validator: low, trigger: 'blur' }
        ],
        waveSaveType: [{ required: true, message: '请选择下载的数据格式', trigger: 'blur' }]
      },
      ruleForm: {
        windparkID: '',
        windturbineIDs: [],
        startSpeed: '0',
        endSpeed: '2000',
        waveNum: 0,
        waveSaveType: '',
        itemTime: [],
        measType: ['3', '4']
      },
      interTimer: null, // 轮询定时器
      logsList: [],
      filePath: '',
      loading: false,
      timePickerOptions: {
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
      }
    }
  },
  computed: {
    ...mapGetters(['userDeptTree'])
  },
  watch: {
    'ruleForm.windparkID': {
      handler() {
        this.getMeasTypeList()
      }
    }
  },
  created() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
    // 给时间筛选赋初始值
    const start = dayjs().subtract(1, 'month').startOf('day').format('YYYY-MM-DD')
    const end = dayjs().endOf('day').format('YYYY-MM-DD')
    this.dayZipTimeRange = [start, end]
    this.getDownloadWaveSaveTypes()
  },
  mounted() {
    this.windparkList = this.userDeptTree
    // this.allTurbineList = data.data
    this.ruleForm.windparkID = this.windparkList[0].id
    this.turbineList = this.windparkList[0].childNode
    this.getDayTableList()
  },
  destroyed() {
    if (this.interTimer) {
      clearInterval(this.interTimer)
      this.interTimer = null
    }
  },
  methods: {
    // 诊断数据下载：表格编号
    indexMethod(index) {
      return index + 1
    },
    // 页面切换
    changeTab(tab) {
      if (tab.label == '诊断数据下载') {
        if (this.interTimer) {
          clearInterval(this.interTimer)
          this.interTimer = null
        }
      } else {
        // 开始轮询获取打包日志
        this.getDownloadLogs()
        this.setInterValTimer()
      }
    },
    // 诊断下载：表格选中change
    handleSelectionChange(param) {
      this.selectTaskList = param
    },

    // 诊断下载：表格筛选change
    daySelectChange(param) {
      this.getDayTableList()
    },

    // 诊断下载接口：获取诊断数据下载表格
    getDayTableList() {
      getDayTableListApi({ timeString: this.dayZipTimeRange.toString() })
        .then(data => {
          this.dayTableList = data.data.data
          this.$nextTick(() => {
            this.$refs.multipleTable.toggleAllSelection(this.dayTableList)
            this.selectTaskList = this.dayTableList
          })
        })
        .catch(error => console.error('Unable to get items.', error))
    },

    // 诊断数据下载：下载多个诊断数据压缩包
    async DownloadDayZIP() {
      if (this.selectTaskList.length === 0) {
        this.$message({ type: 'warning', message: '请在表格中选中文件！' })
        return
      }

      const batch = 3 // 每批同时 3 个
      for (let i = 0; i < this.selectTaskList.length; i += batch) {
        const slice = this.selectTaskList.slice(i, i + batch)
        slice.forEach(file => {
          const a = document.createElement('a')
          a.href = `/NetApi/DownloadTask/DownloadZipByPath?path=${encodeURIComponent(
            file.filePath
          )}`
          a.download = file.fileName
          a.click()
        })
        if (i + batch < this.selectTaskList.length) await new Promise(r => setTimeout(r, 1000)) // 等 1 秒再下一批
      }

      // 下载时间范围内的数据采集结果
      this.downloadCSVFile()
    },

    // 诊断数据下载: 下载csv文件
    downloadCSVFile() {
      if (this.dayZipTimeRange.length == 0) {
        this.$message({ type: 'warning', message: '请选择时间范围！' })
      } else {
        downloadCSVFileApi({
          timeString: this.dayZipTimeRange.toString()
        })
          .then(data => {
            if (data.data.success) {
              if (data.data.data == '') {
                this.$message({ type: 'warning', message: data.data.message })
              } else {
                this.downloadFile(data.data.data)
              }
            } else {
              //  this.$message({ type: 'danger', message: data.message });
            }
          })
          .catch(error => console.error('Unable to get items.', error))
      }
    },
    // 诊断下载按钮：删除压缩包
    DeleteDayZIP() {
      if (this.selectTaskList.length == 0) {
        this.$message({ type: 'warning', message: '请在表格中选中文件！' })
        return
      }

      this.$confirm('确定将选择压缩包删除?', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        this.deleteTaskApiFun(this.selectTaskList)
      })
    },

    // 诊断下载接口：删除压缩包
    deleteTaskApiFun(zipList) {
      /*  let request = new Request('/DownloadTask/DeleteDayZipList', {
        method: 'post',
        headers: { 'Content-Type': 'application/json;charset=utf-8;' },
        body: JSON.stringify(zipList)
      }) */

      deleteTaskApi(zipList)
        /*  .then(response => {
          if (!response.ok) {
            throw new Error('Network response was not ok')
          }
          return response.json()
        }) */
        .then(data => {
          if (data.data.success) {
            this.$message({ type: 'success', message: data.data.message })
            this.getDayTableList()
          } else {
            this.$message({ type: 'danger', message: data.data.message })
          }
        })

        .catch(error => console.error('Unable to get items.', error))
    },

    // 自定义下载接口：获取部件类型
    getMeasTypeList() {
      getMeasTypeListApi({
        stationID: this.ruleForm.windparkID
      })
        .then(data => {
          this.measTypeList = data.data.data
          this.ruleForm.measType = data.data.data.map(i => i.key)
        })
        .catch(error => console.error('Unable to get items.', error))
    },

    // 自定义下载接口：获取下载数据的保存类型
    getDownloadWaveSaveTypes() {
      getDownloadWaveSaveTypesApi()
        .then(data => {
          this.downloadSaveTypeList = data.data.data
          this.ruleForm.waveSaveType = data.data.data[0].key
        })
        .catch(error => console.error('Unable to get items.', error))
    },

    // 自定义下载：选中风场改变
    windparkSelectChange() {
      var data = this.windparkList.filter(o => o.id == this.ruleForm.windparkID)
      this.turbineList = data[0].childNode
    },
    // 自定义下载：机组全选
    handleCheckedAll() {
      this.ruleForm.windturbineIDs = Array.from(this.turbineList, i => i.entityId)
    },
    // 自定义下载：机组全不选
    handleCheckedNone() {
      this.ruleForm.windturbineIDs = []
    },
    // 自定义下载：机组反选
    handleCheckedReverse() {
      let arrSelected = this.ruleForm.windturbineIDs
      let unselected = this.turbineList.filter(i => !arrSelected.includes(i.entityId))
      this.ruleForm.windturbineIDs = Array.from(unselected, i => i.entityId)
    },

    // 自定义下载按钮：新增下载任务
    addTask() {
      if (
        (this.logsList.length != 0 && this.logsList.length != 0) ||
        (this.logsList.length == 0 && this.logsList.length == 0)
      ) {
        this.$refs['ruleForm'].validate(valid => {
          if (valid) {
            let param = {
              createdTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
              stationName: this.HandlerStationName(this.ruleForm.windparkID),
              windturbineIDs: this.ruleForm.windturbineIDs,
              startTime: this.ruleForm.itemTime[0],
              endTime: this.ruleForm.itemTime[1],
              waveNum: this.ruleForm.waveNum.toString(),
              measType: this.ruleForm.measType,
              waveSaveType: this.ruleForm.waveSaveType
            }
            this.addTaskApiFun(param)
          } else {
            this.$message({ type: 'warning', message: '参数未输入完整或不满足要求！' })
          }
        })
      } else {
        this.$message({ type: 'warning', message: '上次打包未完成，暂不打包！' })
      }
    },

    // 根据风场ID获取风场名称
    HandlerStationName(stationID) {
      var data = this.windparkList.filter(o => o.id == stationID)
      return data[0].name
    },

    // 自定义下载接口：新增自定义下载
    addTaskApiFun(param) {
      /* let request = new Request('/DownloadTask/AddTask', {
        method: 'post',
        headers: { 'Content-Type': 'application/json;charset=utf-8;' },
        body: JSON.stringify(param)
      }) */

      addTaskApi(param)
        .then(data => {
          if (data.data.success) {
            this.$message({ type: 'success', message: data.data.message })
            this.loading = true
            this.getDownloadLogs() // 自定义下载任务成功后，请求日志
          }
        })
        .catch(error => {
          console.error('Unable to get items.', error)
        })
    },

    // 自定义下载接口：轮询调用，展示数据下载实时接口（自定义下载打包完成后需清除定时器）
    getDownloadLogs() {
      getDownloadLogsApi()
        .then(data => {
          let { logs, path } = data.data.data
          this.logsList = logs
          this.filePath = path
          this.loading = false
        })
        .catch(error => {
          console.error('请求失败:', error)
        })
    },

    setInterValTimer() {
      this.interTimer = setInterval(() => {
        this.getDownloadLogs()
      }, 5000)
    },
    // 使用正则表达式匹配文件名
    extractFileName(filePath) {
      const match = filePath.match(/[^\\/]+$/)
      var name = match ? match[0] : filePath // 如果匹配成功，返回文件名，否则返回路径
      return name
    },

    // 自定义下载：单个文件的A链接下载
    async downloadFile(filePath) {
      // 创建A链接
      const a = document.createElement('a')
      // 路径需 URL 编码
      a.href = `/NetApi/DownloadTask/DownloadZipByPath?path=${encodeURIComponent(filePath)}`
      // 指定保存文件名
      a.download = this.extractFileName(filePath)
      a.style.display = 'none'
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
    }
  }
}
</script>
<style lang="scss" scoped>
.el-main {
  height: 100%;
  width: 100%;
  padding: 0;
  color: #000;
  overflow: unset;
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
  .left_download {
    width: 100%;
    padding: 15px 15px 5px 15px;
    height: 100%;
    font-size: 14px;
  }
  ::v-deep .el-tabs--border-card {
    color: #606266;
    .el-tabs__content {
      height: calc(100% - 30px);
    }
  }

  .right_download {
    width: 100%;
    padding: 15px 15px 5px 15px;
    overflow: auto;
    height: 100%;
  }
  .download_task {
    border: #ccc 1px solid;
    margin: 5px;
    height: 200px;
    ::v-deep .el-card__header {
      padding: 10px;
      font-size: 14px;
    }
    ::v-deep .el-card__body {
      width: 100%;
      height: 160px;
      overflow: auto;
      font-size: 12px;
      line-height: 20px;
      padding: 0px 20px;
    }
  }
  .el-tabs {
    width: 100%;
    height: calc(100% - 40px);
    border: none;
    border-top: 1px solid #ccc;
  }
  .el-tabs .el-tabs__content {
    width: 100%;
    height: calc(100% - 40px);
    .el-tab-pane {
      width: 100%;
      height: 100%;
    }
  }
  ::v-deep .el-checkbox__label {
    color: #606266;
  }
  .el-button-group {
    button {
      background: #fff;
      color: #606266;
      &:hover {
        background: #1b84ee;
        color: #fff;
      }
    }
  }
  .el-form-item {
    margin-bottom: 16px;
  }
}
</style>
