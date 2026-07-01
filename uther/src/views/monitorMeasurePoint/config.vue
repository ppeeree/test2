<template>
  <div class="config_page">
    <div class="left_part">
      <tree
        :list="treeData"
        @handlerTreeClick="handlerTreeClick"
        @clickedPlan="clickedPlan"
        @initTreeData="initTreeData"
      ></tree>
    </div>
    <div class="main_part">
      <model-canvas ref="model_canvas" :showType="selectedType" style="width: 100%; height: 100%" />
    </div>
    <div class="right_part" v-if="selectedType">
      <modelForm
        ref="model_form"
        :treeData="treeData"
        :showType="selectedType"
        @clickedPlan="clickedPlan"
        :dirveData="dirveData"
        @viewModel="viewModel"
        @initTreeData="initTreeData"
        @saveMark="saveMarkBtn"
      />
    </div>
  </div>
</template>
<script>
import { getList } from '@/api/equipment/model'
import { getPlan } from '@/api/equipment/monitor'
import ModelCanvas from './components/canvas.vue'
import tree from './components/tree.vue'
import modelForm from './components/modelForm'
export default {
  components: {
    tree,
    ModelCanvas,
    modelForm
  },
  data() {
    return {
      treeData: [], // 左侧树数据
      dirveData: [], //设备列表
      currentModelPath: '',
      selectedType: '' // 方案，机舱，塔筒，叶片，整机
    }
  },
  created() {
    this.initDriveData()
    this.initTreeData()
  },
  methods: {
    // 1.1 获取左侧树数据
    initTreeData() {
      getPlan('').then(res => {
        if (res.data.code == 200) {
          this.treeData = res.data.data
          this.treeData.forEach(item => {
            item.name = item.solutionName
            item.children.forEach(ite => {
              ite.name = ite.deviceModelType
            })
          })
        }
      })
    },
    // 1.2获取设备数据
    initDriveData() {
      getList('').then(res => {
        if (res.data.code == 200) {
          let data = res.data.data
          data.forEach(item => {
            item.label = item['compName']
            item.children.forEach(ite => {
              ite.label = ite['manufacturer']
              ite.children.forEach(it => {
                it.label = it['deviceModel']
              })
            })
          })

          this.dirveData = data
          // console.log('设备模型数据', this.dirveData)
        }
      })
    },

    //点击树节点
    clickedPlan(data, clickNode) {
      // if (data == '') {
      this.$refs['model_canvas'].clearModel()
      this.$refs['model_form'] && this.$refs['model_form'].clearData()
      // }

      this.selectedType = data

      if (clickNode) {
        let p = new Promise(resolve => {
          getPlan(clickNode.id, { deviceType: this.selectedType }).then(res => {
            resolve(res.data.data)
          })
        })
        Promise.all([p]).then(result => {
          const data = result[0]
          console.log('根据节点数据调用接口数据', data)
          if (this.$refs['model_form']) {
            this.$refs['model_form'].initData(data[0].children[0])
          }
        })
      }
    },
    handlerTreeClick(data) {
      this.currentModelPath = data.value
    },

    //预览模型
    viewModel(path, position) {
      this.$refs['model_canvas'].clearModel()
      this.$refs['model_canvas'].changeModel(path)
      this.$refs['model_canvas'].addMarkModel(position)
    },

    // 保存模型
    saveMarkBtn(param) {
      this.$refs['model_canvas'].savePrograme(param)
    }
  }
}
</script>
<style lang="scss" scoped>
.config_page {
  width: 100%;
  height: 100%;
  overflow: hidden;
  position: relative;
  .left_part {
    width: 200px;
    height: 98%;
    z-index: 2;
    position: absolute;
    left: 10px;
    top: 1%;
    padding: 10px 0;
    background: rgba(13, 52, 83, 0.5);
    backdrop-filter: blur(6px);
    border-radius: 3px;
  }
  .main_part {
    width: 100%;
    height: 100%;
    overflow: hidden;
    float: left;
  }
  .right_part {
    height: 98%;
    width: 393px;
    z-index: 2;
    position: absolute;
    right: 10px;
    top: 1%;
    border-radius: 6px;
    backdrop-filter: blur(6px);
    background: rgba(4, 17, 33, 0.5);
  }
}
</style>
