<template>
  <el-main class="data-download-page">
    <div class="merge_header">数据下载</div>

    <el-tabs type="border-card" @tab-click="changeTab">
      <el-tab-pane label="诊断数据下载">
        <div class="left_download">
          筛选：
          <el-date-picker
            v-model="dayZipTimeRange"
            type="daterange"
            :shortcuts="timePickerShortcuts"
            range-separator="~"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            align="right"
            value-format="YYYY-MM-DD"
            size="small"
            @change="daySelectChange"
          />

          <el-button style="margin-left: 10px" type="primary" size="small" @click="downloadDayZip">
            <el-icon style="margin-right: 7px"><Download /></el-icon>
            批量下载
          </el-button>

          <el-table
            ref="multipleTableRef"
            :data="dayTableList"
            height="95%"
            style="width: 100%; margin-top: 8px"
            border
            :highlight-current-row="true"
            row-key="fileName"
            :header-cell-style="tableHeaderStyle"
            class="tableLimit"
            size="small"
            @selection-change="handleSelectionChange"
          >
            <el-table-column type="selection" width="55" />
            <el-table-column type="index" label="编号" :index="indexMethod" width="60" />
            <el-table-column prop="fileTime" label="数据包时间" />
            <el-table-column prop="fileName" label="文件名称" />
            <el-table-column prop="turbineNum" label="机组数量" />
            <el-table-column prop="fileMemory" label="文件大小" />
          </el-table>
        </div>
      </el-tab-pane>

      <el-tab-pane label="自定义下载">
        <div class="right_download">
          <el-form ref="ruleFormRef" :model="ruleForm" :rules="rules" label-width="120px">
            <el-form-item label="风场：" prop="windparkID" :required="true">
              <el-select v-model="ruleForm.windparkID" placeholder="请选择" @change="windparkSelectChange">
                <el-option
                  v-for="item in windparkList"
                  :key="item.id"
                  :label="item.name"
                  :value="item.id"
                />
              </el-select>
            </el-form-item>

            <el-form-item label="机组：" prop="windturbineIDs" :required="true">
              <el-button-group>
                <el-button size="small" plain @click="handleCheckedAll">全选</el-button>
                <el-button size="small" plain @click="handleCheckedNone">全不选</el-button>
                <el-button size="small" plain @click="handleCheckedReverse">反选</el-button>
              </el-button-group>
              <el-checkbox-group
                v-model="ruleForm.windturbineIDs"
                style="line-height: 24px; width: 60%"
              >
                <el-checkbox
                  v-for="item in turbineList"
                  :key="item.entityId"
                  :label="item.entityId"
                >
                  {{ item.entityName }}
                </el-checkbox>
              </el-checkbox-group>
            </el-form-item>

            <el-form-item label="时间范围：" prop="itemTime" :required="true">
              <el-date-picker
                v-model="ruleForm.itemTime"
                type="datetimerange"
                :shortcuts="timePickerShortcuts"
                range-separator="至"
                start-placeholder="开始日期"
                end-placeholder="结束日期"
                align="right"
                value-format="YYYY-MM-DD HH:mm:ss"
                :default-time="defaultTime"
              />
            </el-form-item>

            <el-form-item label="部件：" prop="measType" :required="true">
              <el-checkbox-group v-model="ruleForm.measType">
                <el-checkbox v-for="item in measTypeList" :key="item.key" :label="item.key">
                  {{ item.value }}
                </el-checkbox>
              </el-checkbox-group>
            </el-form-item>

            <el-form-item label="波形组数：" prop="waveNum">
              <el-input-number
                v-model="ruleForm.waveNum"
                controls-position="right"
                style="margin-right: 5px; width: 120px"
                placeholder=""
              />
            </el-form-item>

            <el-form-item label="数据格式：" prop="waveSaveType">
              <el-select v-model="ruleForm.waveSaveType">
                <el-option
                  v-for="item in downloadSaveTypeList"
                  :key="item.key"
                  :label="item.value"
                  :value="item.key"
                />
              </el-select>
            </el-form-item>
          </el-form>

          <el-button style="margin-left: 10px" size="small" type="primary" @click="addTask">
            数据打包
          </el-button>

          <el-card class="download_task">
            <template #header>
              <div class="clearfix">
                <span>自定义下载日志</span>
              </div>
            </template>
            <div v-loading="loading">
              <p v-for="item in logsList" :key="item">{{ item }}</p>
            </div>
          </el-card>

          <div v-if="filePath.length" class="file_button">
            <el-button @click="downloadFile(filePath)">
              <el-image style="width: 40px; height: 40px" src="/img/zip.png" fit="fill" />
              <p style="float: right; margin-top: 15px; margin-left: 10px">
                {{ extractFileName(filePath) }}
              </p>
            </el-button>
          </div>
        </div>
      </el-tab-pane>
    </el-tabs>
  </el-main>
</template>

<script setup>
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue'
import { useStore } from 'vuex'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Download } from '@element-plus/icons-vue'
import dayjs from 'dayjs'
import { setTheme } from '@/util/util'
import {
  addTaskApi,
  deleteTaskApi,
  downloadCSVFileApi,
  getDayTableListApi,
  getDownloadLogsApi,
  getDownloadWaveSaveTypesApi,
  getMeasTypeListApi
} from '@/api/basicConfig/dataDownLoad'

const store = useStore()

const tableHeaderStyle = {
  backgroundColor: '#F2F6FC',
  color: '#909399',
  textAlign: 'center'
}

const dayTableList = ref([])
const dayZipTimeRange = ref([])
const selectTaskList = ref([])
const windparkList = ref([])
const turbineList = ref([])
const measTypeList = ref([])
const downloadSaveTypeList = ref([])
const logsList = ref([])
const filePath = ref('')
const loading = ref(false)
const interTimer = ref(null)
const multipleTableRef = ref(null)
const ruleFormRef = ref(null)

const defaultTime = [new Date(2000, 1, 1, 0, 0, 0), new Date(2000, 1, 1, 23, 59, 59)]
const userDeptTree = computed(() => store.getters.userDeptTree || [])

const ruleForm = reactive({
  windparkID: '',
  windturbineIDs: [],
  startSpeed: '0',
  endSpeed: '2000',
  waveNum: 0,
  waveSaveType: '',
  itemTime: [],
  measType: ['3', '4']
})

const positiveNumber = (rule, value, callback) => {
  if (value <= 0) {
    callback(new Error('波形数量必须大于0'))
  } else {
    callback()
  }
}

const rules = {
  windparkID: [{ required: true, message: '请选择风场', trigger: 'change' }],
  windturbineIDs: [{ required: true, type: 'array', message: '请选择机组', trigger: 'change' }],
  measType: [{ required: true, type: 'array', message: '请选择部件', trigger: 'change' }],
  dataType: [{ required: true, message: '请选择数据类型', trigger: 'change' }],
  itemTime: [{ required: true, type: 'array', message: '请选择时间范围', trigger: 'blur' }],
  waveNum: [
    { required: true, message: '请输入波形数量', trigger: 'blur' },
    { validator: positiveNumber, trigger: 'blur' }
  ],
  waveSaveType: [{ required: true, message: '请选择下载的数据格式', trigger: 'blur' }]
}

const timePickerShortcuts = [
  {
    text: '最近一周',
    value: () => {
      const end = new Date()
      const start = new Date()
      start.setTime(start.getTime() - 3600 * 1000 * 24 * 7)
      return [start, end]
    }
  },
  {
    text: '最近一个月',
    value: () => {
      const end = new Date()
      const start = new Date()
      start.setTime(start.getTime() - 3600 * 1000 * 24 * 30)
      return [start, end]
    }
  },
  {
    text: '最近三个月',
    value: () => {
      const end = new Date()
      const start = new Date()
      start.setTime(start.getTime() - 3600 * 1000 * 24 * 90)
      return [start, end]
    }
  }
]

watch(
  () => ruleForm.windparkID,
  () => {
    getMeasTypeList()
  }
)

onMounted(() => {
  store.commit('SET_THEME_NAME', 'theme-white')
  setTheme('theme-white')

  const start = dayjs().subtract(1, 'month').startOf('day').format('YYYY-MM-DD')
  const end = dayjs().endOf('day').format('YYYY-MM-DD')
  dayZipTimeRange.value = [start, end]

  initWindparkData()
  getDownloadWaveSaveTypes()
  getDayTableList()
})

onBeforeUnmount(() => {
  clearLogTimer()
})

function indexMethod(index) {
  return index + 1
}

function initWindparkData() {
  windparkList.value = userDeptTree.value
  const firstWindpark = windparkList.value[0]

  if (!firstWindpark) return

  ruleForm.windparkID = firstWindpark.id
  turbineList.value = firstWindpark.childNode || []
}

function changeTab(tab) {
  const label = tab?.props?.label || tab?.label
  if (label === '诊断数据下载') {
    clearLogTimer()
    return
  }

  getDownloadLogs()
  setInterValTimer()
}

function handleSelectionChange(selection) {
  selectTaskList.value = selection
}

function daySelectChange() {
  getDayTableList()
}

function getDayTableList() {
  getDayTableListApi({ timeString: dayZipTimeRange.value.toString() })
    .then(({ data }) => {
      dayTableList.value = data.data || []
      nextTick(() => {
        multipleTableRef.value?.toggleAllSelection()
        selectTaskList.value = dayTableList.value
      })
    })
    .catch(error => console.error('Unable to get items.', error))
}

async function downloadDayZip() {
  if (!selectTaskList.value.length) {
    ElMessage({ type: 'warning', message: '请在表格中选中文件！' })
    return
  }

  const batchSize = 3
  for (let index = 0; index < selectTaskList.value.length; index += batchSize) {
    const batch = selectTaskList.value.slice(index, index + batchSize)
    batch.forEach(file => {
      triggerZipDownload(file.filePath, file.fileName)
    })

    if (index + batchSize < selectTaskList.value.length) {
      await new Promise(resolve => setTimeout(resolve, 1000))
    }
  }

  downloadCSVFile()
}

function downloadCSVFile() {
  if (!dayZipTimeRange.value.length) {
    ElMessage({ type: 'warning', message: '请选择时间范围！' })
    return
  }

  downloadCSVFileApi({ timeString: dayZipTimeRange.value.toString() })
    .then(({ data }) => {
      if (!data.success) return
      if (!data.data) {
        ElMessage({ type: 'warning', message: data.message })
        return
      }
      downloadFile(data.data)
    })
    .catch(error => console.error('Unable to get items.', error))
}

function deleteDayZip() {
  if (!selectTaskList.value.length) {
    ElMessage({ type: 'warning', message: '请在表格中选中文件！' })
    return
  }

  ElMessageBox.confirm('确定将选择压缩包删除？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(() => {
    deleteTaskApiFun(selectTaskList.value)
  })
}

function deleteTaskApiFun(zipList) {
  deleteTaskApi(zipList)
    .then(({ data }) => {
      if (data.success) {
        ElMessage({ type: 'success', message: data.message })
        getDayTableList()
      } else {
        ElMessage({ type: 'error', message: data.message })
      }
    })
    .catch(error => console.error('Unable to get items.', error))
}

function getMeasTypeList() {
  if (!ruleForm.windparkID) return

  getMeasTypeListApi({ stationID: ruleForm.windparkID })
    .then(({ data }) => {
      measTypeList.value = data.data || []
      ruleForm.measType = measTypeList.value.map(item => item.key)
    })
    .catch(error => console.error('Unable to get items.', error))
}

function getDownloadWaveSaveTypes() {
  getDownloadWaveSaveTypesApi()
    .then(({ data }) => {
      downloadSaveTypeList.value = data.data || []
      ruleForm.waveSaveType = downloadSaveTypeList.value[0]?.key || ''
    })
    .catch(error => console.error('Unable to get items.', error))
}

function windparkSelectChange() {
  const selectedWindpark = windparkList.value.find(item => item.id === ruleForm.windparkID)
  turbineList.value = selectedWindpark?.childNode || []
  ruleForm.windturbineIDs = []
}

function handleCheckedAll() {
  ruleForm.windturbineIDs = turbineList.value.map(item => item.entityId)
}

function handleCheckedNone() {
  ruleForm.windturbineIDs = []
}

function handleCheckedReverse() {
  const selectedIds = ruleForm.windturbineIDs
  ruleForm.windturbineIDs = turbineList.value
    .filter(item => !selectedIds.includes(item.entityId))
    .map(item => item.entityId)
}

function addTask() {
  ruleFormRef.value?.validate(valid => {
    if (!valid) {
      ElMessage({ type: 'warning', message: '参数未输入完整或不满足要求！' })
      return
    }

    const [startTime, endTime] = ruleForm.itemTime
    const param = {
      createdTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
      stationName: handlerStationName(ruleForm.windparkID),
      windturbineIDs: ruleForm.windturbineIDs,
      startTime,
      endTime,
      waveNum: String(ruleForm.waveNum),
      measType: ruleForm.measType,
      waveSaveType: ruleForm.waveSaveType
    }

    addTaskApiFun(param)
  })
}

function handlerStationName(stationID) {
  const selectedWindpark = windparkList.value.find(item => item.id === stationID)
  return selectedWindpark?.name || ''
}

function addTaskApiFun(param) {
  addTaskApi(param)
    .then(({ data }) => {
      if (!data.success) return

      ElMessage({ type: 'success', message: data.message })
      loading.value = true
      getDownloadLogs()
      setInterValTimer()
    })
    .catch(error => {
      console.error('Unable to get items.', error)
    })
}

function getDownloadLogs() {
  getDownloadLogsApi()
    .then(({ data }) => {
      const result = data.data || {}
      logsList.value = result.logs || []
      filePath.value = result.path || ''
      loading.value = false
    })
    .catch(error => {
      console.error('请求失败:', error)
    })
}

function setInterValTimer() {
  clearLogTimer()
  interTimer.value = setInterval(() => {
    getDownloadLogs()
  }, 5000)
}

function clearLogTimer() {
  if (!interTimer.value) return
  clearInterval(interTimer.value)
  interTimer.value = null
}

function extractFileName(path) {
  const match = path.match(/[^\\/]+$/)
  return match ? match[0] : path
}

function triggerZipDownload(path, fileName) {
  const link = document.createElement('a')
  link.href = `/NetApi/DownloadTask/DownloadZipByPath?path=${encodeURIComponent(path)}`
  link.download = fileName || extractFileName(path)
  link.style.display = 'none'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

async function downloadFile(path) {
  triggerZipDownload(path)
}

defineExpose({
  deleteDayZip,
  downloadCSVFile
})
</script>

<style lang="scss" scoped>
.data-download-page {
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

  :deep(.el-tabs--border-card) {
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

    :deep(.el-card__header) {
      padding: 10px;
      font-size: 14px;
    }

    :deep(.el-card__body) {
      width: 100%;
      height: 160px;
      overflow: auto;
      font-size: 12px;
      line-height: 20px;
      padding: 0 20px;
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

  :deep(.el-checkbox__label) {
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
