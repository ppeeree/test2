<template>
  <el-form :model="form" label-width="80px">
    <el-form-item label="已有设备：">
      <el-cascader
        ref="deviceModel"
        v-model="deviceSelect"
        placeholder="请选择设备"
        :options="newTreeData"
        :props="deviceProps"
        collapse-tags
        clearable
        @change="deviceChange"
      >
      </el-cascader>
    </el-form-item>
    <el-form-item label="三维模型：">
      <el-cascader
        ref="modelCascader"
        v-model="modelSelect"
        placeholder="请选择设备"
        :options="pathList"
        :props="modelProps"
        collapse-tags
        clearable
        @change="modelChange"
      >
      </el-cascader>
    </el-form-item>
    <div class="space_line"></div>
    <div class="big_title">三维模型参数</div>
    <el-form-item label="选择模型：">
      <el-select
        v-model="selectModel"
        placeholder="请选择模型"
        @change="changeSelectModel"
        multiple
        clearable
        collapse-tags
      >
        <el-option
          v-for="item in allModelList"
          :key="item.path"
          :label="item.name"
          :value="item.path"
        >
        </el-option>
      </el-select>
    </el-form-item>
    <el-form-item label="位置：">
      <span v-for="(pitem, pindex) in modelForm.position" :key="pindex">
        <span class="axle_style">{{ axleIndex[pindex] }}：</span>
        <el-input-number
          v-model="modelForm.position[pindex]"
          clearable
          :precision="2"
          :step="1"
          :controls="false"
          @change="handlerChange"
        >
        </el-input-number>
      </span>
    </el-form-item>
    <el-form-item label="缩放：">
      <span v-for="(sitem, sindex) in modelForm.scale" :key="sindex">
        <span class="axle_style">{{ axleIndex[sindex] }}：</span>
        <el-input-number
          v-model="modelForm.scale[sindex]"
          clearable
          :precision="2"
          :step="0.01"
          :controls="false"
          @change="handlerChange"
        >
        </el-input-number>
      </span>
    </el-form-item>
    <el-form-item label="旋转：">
      <span v-for="(ritem, rindex) in modelForm.rotate" :key="rindex">
        <span class="axle_style">{{ axleIndex[rindex] }}：</span>
        <el-input-number
          v-model="modelForm.rotate[rindex]"
          clearable
          :precision="2"
          :step="1"
          :controls="false"
          @change="handlerChange"
        >
        </el-input-number>
      </span>
    </el-form-item>
    <div class="big_title">相机视角参数</div>
    <el-form-item label="相机位置：">
      <span v-for="(citem, cindex) in modelForm.cameraPosition" :key="cindex">
        <span class="axle_style">{{ axleIndex[cindex] }}：</span>
        <el-input-number
          v-model="modelForm.cameraPosition[cindex]"
          clearable
          :precision="2"
          :step="1"
          :controls="false"
          @change="handlerChangeCamera"
        >
        </el-input-number>
      </span>
    </el-form-item>
    <el-form-item label="整体位置：">
      <span v-for="(witem, windex) in modelForm.wholePosition" :key="windex">
        <span class="axle_style">{{ axleIndex[windex] }}：</span>
        <el-input-number
          v-model="modelForm.wholePosition[windex]"
          clearable
          :precision="2"
          :step="1"
          :controls="false"
          @change="handlerWholeChange"
        >
        </el-input-number>
      </span>
    </el-form-item>

    <el-form-item label="视角不显示">
      <el-select
        v-model="modelForm.noneShowList"
        multiple
        collapse-tags
        clearable
        placeholder="请选择设备"
        @change="visibleChange"
      >
        <el-option
          v-for="item in allModelList"
          :key="item.path"
          :label="item.name"
          :value="item.path"
        >
        </el-option>
      </el-select>
    </el-form-item>
    <el-button type="primary" @click="handlerSave" style="margin-left: 40%">
      <i class="el-icon-circle-check"></i> 保存</el-button
    >
  </el-form>
</template>

<script>
import { getPath, getList } from '@/api/equipment/model'
import { getPathByKey, findLeafNode } from '../../component/windList'

export default {
  props: {
    treeData: {
      type: Array,
      default: () => []
    },
    selectModelList: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      pathList: [], //接口返回模型数组
      allModelNameList: [], //将所有地址放在一个数组中

      deviceSelect: [], //设备model-选择的设备
      addDeviceList: [], //选择的型号

      modelSelect: [], //模型model-选中模型列表
      addModelList: [], //选中的模型

      deviceProps: {
        multiple: true,
        expandTrigger: 'click',
        value: 'label'
      },
      modelProps: {
        multiple: true,
        expandTrigger: 'click',
        label: 'name',
        value: 'name'
      },
      selectProps: {
        multiple: true,
        expandTrigger: 'click',
        label: 'name',
        value: 'path'
      },
      axleIndex: {
        0: 'X',
        1: 'Y',
        2: 'Z'
      },

      selectModel: [], //下拉选择模型

      modelForm: {
        position: [0, 0, 0],
        scale: [1, 1, 1],
        rotate: [0, 0, 0],
        cameraPosition: [10, 10, 10],
        wholePosition: [0, 0, 0],
        noneShowList: [],
        embodyId: []
      }
    }
  },
  computed: {
    newTreeData() {
      let list = JSON.parse(JSON.stringify(this.treeData))
      return list
    },
    allModelList() {
      let list = [...this.addModelList]
      this.addDeviceList.forEach(ele => {
        let newPath = ele.path.replace(new RegExp('amp;'), '')
        if (!list.find(j => j.path == newPath)) {
          list.push(ele)
        }
      })

      list.forEach(item => {
        item.path = item.path.replace(new RegExp('amp;'), '')
      })

      return list
    }
  },
  watch: {
    selectModelList: {
      handler(val) {
        this.selectModel = []
        val.forEach(item => {
          this.selectModel.push(item.path)
        })
      },
      immediate: true
    }
  },
  mounted() {
    this.$emit('cameraPosition', this.modelForm.cameraPosition)
    this.$emit('wholePosition', this.modelForm.wholePosition)

    this.getPathList()
  },
  methods: {
    getPathList() {
      getPath().then(res => {
        if (res.data.code == 200) {
          this.pathList = res.data.data[0].children
          this.handlerPathList(this.pathList)
        }
      })
    },
    // 处理名称数据
    handlerPathList(data) {
      data.forEach(item => {
        if (item.children) {
          this.handlerPathList(item.children)
        } else {
          let dindex = item.name.indexOf('&')
          if (dindex !== 0) {
            item.name = item.name.slice(0, dindex)
          }
          this.allModelNameList.push(item)
        }
      })
    },

    handlerChange() {
      this.$emit('selectPosition', this.modelForm)
    },
    handlerWholeChange() {
      this.$emit('wholePosition', this.modelForm.wholePosition)
    },
    handlerChangeCamera() {
      this.$emit('cameraPosition', this.modelForm.cameraPosition)
    },

    //1、选择型号
    deviceChange(value) {
      this.addDeviceList = []

      let data = this.$refs['deviceModel'].getCheckedNodes(true)
      // console.log('选中的型号', value, this.newTreeData, data)

      let list = []
      data.forEach(item => {
        list.push(item.data.id)
        item.data.propertiesView.forEach(ele => {
          if (ele.isShow && !this.addDeviceList.find(j => j.path == ele.path)) {
            this.addDeviceList.push({
              path: ele.path.replace(new RegExp('amp;'), ''),
              name: ele.name
            })
          }
        })
      })
      this.modelForm.embodyId = list

      this.$emit('addModel', this.allModelList, true)
    },
    //2、选择模型
    modelChange(val) {
      this.addModelList = []

      let data = this.$refs['modelCascader'].getCheckedNodes(true)
      // console.log('选中三维模型', data)

      data.forEach(item => {
        let obj = item.data
        this.addModelList.push({
          path: obj.value.replace(new RegExp('amp;'), ''),
          name: obj.name,
          key: obj.key
        })
      })
      // console.log('选中的三维模型', this.allModelList)
      this.$emit('addModel', this.allModelList)
    },
    //3、选择视角
    visibleChange(val) {
      this.$emit('handlerVisible', val)
    },

    initData(obj, arr) {
      this.allModelList.length = 0

      this.modelForm = {
        position: [0, 0, 0],
        scale: [1, 1, 1],
        rotate: [0, 0, 0],
        cameraPosition: arr[0].cameraPosition,
        wholePosition: arr[0].wholePosition,
        noneShowList: []
      }

      arr.forEach(item => {
        let { path, ...other } = item
        this.allModelList.push({ ...other, path: path.replace(new RegExp('amp;'), '') })
      })

      this.initDriveData(obj.embodyId, arr)

      this.$emit('cameraPosition', this.modelForm.cameraPosition)
      this.$emit('wholePosition', this.modelForm.wholePosition)
    },

    initDriveData(idArr, modelArr) {
      let idPath = []

      idArr.forEach(item => {
        //获取选中已有设备的路径
        let selectObj = findLeafNode(item, this.newTreeData, 'id')
        let test = getPathByKey(selectObj.label, this.newTreeData, 'label')
        idPath.push(test)

        // 确定选中的三维模型数组
        selectObj.propertiesView.forEach(j => {
          if (j.isShow) {
            modelArr.forEach((ele, index) => {
              if (
                ele.path.replace(new RegExp('amp;'), '') == j.path.replace(new RegExp('amp;'), '')
              ) {
                modelArr.splice(index, 1)
              }
            })
          }
        })
      })
      this.deviceSelect = idPath
      // console.log('选中已有设备', this.deviceSelect)

      this.initModelData(JSON.parse(JSON.stringify(modelArr)))
    },

    initModelData(arr) {
      let showlist = [],
        modelPath = []

      arr.forEach(ele => {
        let newPath = ele.path.replace(new RegExp('amp;'), '')
        if (!ele.isVisible) {
          this.modelForm.noneShowList.push(ele.name)
          showlist.push(newPath)
        }

        let testPath = ele.path.split('/').slice(1)

        testPath[testPath.length - 1] = testPath[testPath.length - 1].slice(
          0,
          testPath[testPath.length - 1].indexOf('&')
        )

        modelPath.push(testPath)
      })
      this.modelSelect = modelPath

      setTimeout(() => {
        this.$emit('handlerVisible', showlist)
      }, 1000)
    },

    clearData(index) {
      if (index) {
        this.modelForm.position = [0, 0, 0]
        this.modelForm.scale = [1, 1, 1]
        this.modelForm.rotate = [0, 0, 0]
      } else {
        this.modelForm = {
          position: [0, 0, 0],
          scale: [1, 1, 1],
          rotate: [0, 0, 0],
          cameraPosition: [10, 10, 10],
          wholePosition: [0, 0, 0],
          noneShowList: [],
          embodyId: []
        }
        this.deviceSelect = []
        this.modelSelect = []
        this.selectModel = []
      }
    },

    handlerSave() {
      this.$emit('handlerModelSave')
    },

    changeSelectModel(value) {
      this.$emit('changeSelectModel', value)
    }
  }
}
</script>

<style lang="less" scoped>
:deep(.el-input-number){
  width: 90px;
  .el-input__inner {
    width: 85px;
  }
}
.axle_style {
}
.big_title {
  font-size: 14px;
  font-weight: 600;
  height: 30px;
  line-height: 30px;
  margin: 10px 0;
}
.el-button--primary {
  margin-top: 35px;
}
.el-cascader {
  width: 100%;
}
.el-select {
  width: 100%;
}
.space_line {
  width: 100%;
  height: 1px;
  background: #283754;
}
</style>
