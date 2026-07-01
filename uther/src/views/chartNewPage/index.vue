<template>
  <div class="bigChartContent" id="bigChartContent">
    <div class="bigChart_left" ref="bigChart_left">
      <emptyChart
        v-if="chartType.length"
        itemId="bigChartUnit"
        tool="isNewPage"
        ref="bigChartUnit"
        :chartInitType="chartType"
        :acqPointerInfoList="acqPointerInfoList"
        :treeDataAndFilterP="treeDataAndFilterP"
      />
    </div>
    <i
      v-if="eigenTypeList.indexOf(chartType) < 0"
      class="el-icon-edit openIcon"
      title="添加诊断记录"
      @click="addRecord"
    ></i>
    <i
      v-if="eigenTypeList.indexOf(chartType) < 0"
      class="el-icon-download downloadIcon"
      title="数据下载"
      @click="downloadData"
    ></i>
    <template>
      <diagnosticRecord
        class="lightTheme"
        :dialogVisible="dialogVisible"
        dialogTitle="诊断记录"
        :componentFromCompleteData="[{ data: waveInfoList }]"
        :imgUrl="imgUrlDialog"
        :key="chartType"
        drawingContainer="drawingContainer"
        @handleDialogVisible="handleDialogVisibleRec"
      />
    </template>
  </div>
</template>
<script>
import { setTheme, downloadTxt } from '@/util/util'
import { downloadData } from '@/api/analysis/index.js'
import uniqWith from 'lodash/uniqWith'
import diagnosticRecord from '@/components/diagnosisRecord/index.vue'
import emptyChart from '@/views/customAnalysisNew/components/emptyChartTemplate.vue'
import Splitter from '@/views/customAnalysisNew/components/splitter.vue'
export default {
  components: {
    emptyChart,
    Splitter,
    diagnosticRecord
  },
  data() {
    return {
      eigenTypeList: ['RCA', 'PCA', 'WCA', 'OA', 'DA'],
      chartType: '',
      acqPointerInfoList: [],
      waveInfoList: [],
      treeDataAndFilterP: [],
      dialogVisible: false,
      imgUrlDialog: null
    }
  },
  created() {
    //获取传过来的对象参数decodedObject
    let decodedObject = JSON.parse(decodeURIComponent(this.$route.query.data))
    //decodedObject 就是之前传过来的参数item
    const { chartType, acqPointerInfoList, treeDataAndFilterP } = decodedObject
    this.chartType = chartType
    this.acqPointerInfoList = acqPointerInfoList
    this.treeDataAndFilterP = treeDataAndFilterP
  },
  mounted() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
    this.$nextTick(() => {
      window.addEventListener('resize', this.handleResize)
    })
  },
  beforeDestroy() {
    window.removeEventListener('resize', this.handleResize)
  },
  methods: {
    downloadData() {
      this.$message.warning('数据获取中，请稍后......')
      let originalWaveformDataRequestList = []
      let uniqList = uniqWith(this.acqPointerInfoList, (arr, acc) => arr === acc)
      uniqList.forEach(ele => {
        originalWaveformDataRequestList.push({
          acqTime: ele.split('&&')[2],
          measlocId: ele.split('&&')[1],
          waveDefId: ele.split('&&')[3] || '',
          waveCategory: this.chartType
        })
      })
      downloadData({ originalWaveformDataRequestList }).then(res => {
        if (res) {
          let temp = res.headers['content-disposition'].split(';')[1].split('=')[1]
          //对文件名乱码转义
          let fileName = decodeURI(temp)
          downloadTxt(res.data, fileName)
          this.$message.success('数据下载成功！')
        } else {
          this.$message.error('数据下载失败！')
        }
      })
    },
    handleResize(param) {
      // 可见区域宽度高度
      const { clientWidth, clientHeight } = document.documentElement
      // const rightDom = this.$ref.bigChart_right
      //let rightWidth = rightDom.clientWidth
      // let chartWidth = clientWidth - rightWidth - 4
      let chartWidth = clientWidth
      this.$refs.bigChart_left.style.width = chartWidth + 'px'
      //   this.$refs.bigChart_left.style.height = newHeight + 'px'
      this.$refs.bigChart_left.style.left = 0
      // document.getElementById('bigChartSplit').style.left = chartWidth + 'px'
      //rightDom.style.left = clientWidth - rightWidth + 'px'
      //rightDom.style.height = clientHeight + 'px'
    },
    handleDialogVisibleRec(val) {
      this.dialogVisible = val
    },
    /*  updateWaveInfo(data) {
      this.waveInfoList = data
    }, */
    addRecord() {
      let reader = new FileReader()
      let data = this.$refs.bigChartUnit.myChartInstance.imageBase64Data(true)
      reader.readAsDataURL(data)
      reader.onload = e => {
        this.imgUrlDialog = e.target.result
        this.dialogVisible = true
      }
    }
  }
}
</script>
<style lang="scss" scoped>
::v-deep .el-dialog .el-dialog__header {
  height: 50px !important;
}
.bigChartContent {
  width: 100%;
  height: 100%;
  position: relative;
  .bigChart_left {
    position: absolute;
    left: 0;
    top: 0;
    width: 100%; // calc(100% - 204px);
    height: 100%;
  }
  .bigChart_right {
    position: absolute;
    left: calc(100% - 204px);
    top: 0;
    width: 200px;
    height: 100%;
  }
  .openIcon {
    position: absolute;
    right: 35px;
    top: 8px;
    font-size: 20px;
    color: #606266;
    cursor: pointer;
  }
  .downloadIcon {
    position: absolute;
    right: 65px;
    top: 8px;
    font-size: 20px;
    color: #606266;
    cursor: pointer;
  }
}
</style>
