<template>
  <div>
    <el-form-item label="风场名称：" prop="windparkName">
      <el-select
        v-model="addCompForm.windparkName"
        placeholder="请选择风场名称"
        @change="windparkChange"
      >
        <el-option
          v-for="item in allTurbineList.windList"
          :key="item.id"
          :label="item.name"
          :value="item.id"
        ></el-option>
      </el-select>
    </el-form-item>
    <el-form-item label="机组名称：" prop="turbineName">
      <el-select
        v-model="addCompForm.turbineName"
        placeholder="请选择机组名称"
        :multiple="isMultiple"
        collapse-tags
        @change="turbineChange"
      >
        <el-option
          v-for="item in allTurbineList.turbineList"
          :key="item.entityId"
          :label="item.entityName"
          :value="item.entityId"
        ></el-option>
      </el-select>
    </el-form-item>
  </div>
</template>

<script>
import { getWindParkList, getTurbineList } from './windList'
export default {
  props: {
    addCompForm: {
      type: Object,
      default: () => {}
    },
    addRules: {
      type: Object,
      default: () => {}
    },
    isMultiple: {
      type: Boolean,
      default: true
    },
    allCompList: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      compList: [], //根据机组拿到部件
      allTurbineList: {} //设备树的机组和风场数组
    }
  },
  mounted() {
    this.getWindList()
  },
  methods: {
    getWindList() {
      let param = {
        windList: getWindParkList(),
        turbineList: []
      }
      this.allTurbineList = param
    },
    windparkChange(value) {
      this.addCompForm.turbineName = ''
      this.allTurbineList.turbineList = getTurbineList(value)
    },
    turbineChange(value) {
      let nameList = []
      let list = []
      value.forEach(element => {
        let obj = this.allTurbineList.turbineList.find(j => j.entityId == element)
        if (obj) {
          nameList.push({ name: obj.entityName, ip: '', code: obj.entityCode, id: obj.entityId })
          list.push(obj)
        }
      })
      this.$emit('turbineChange', nameList)
      if (this.allCompList.length) {
        this.compList = this.addSelectCompList(list)
        this.$emit('selectCompList', this.compList)
      }
    },
    addSelectCompList(list) {
      // step1：获取选中机组下的所有部件列表
      const mapCompList = list.map(o => o.childNode)
      const selectAllCompList = []
      mapCompList.forEach(item => {
        selectAllCompList.push(...item)
      })

      // step2：将选中机组的全部部件整合去重
      const allCompNameList = Array.from(
        new Set(
          selectAllCompList.map(item =>
            JSON.stringify({ name: item.entityName, code: item.entityCode })
          )
        )
      ).map(item => JSON.parse(item))

      // console.log('处理该机组下的部件', list, selectAllCompList, allCompNameList)
      /*
            let arr = []
      list.forEach(item1 => {
        item1.childNode.forEach(item2 => {
          let bigCompObj = arr.find(j => j.name == item2.entityName) //最终数组中是否存在部件对象
          let allBigCompObj = this.allCompList.find(j => j.name == item2.entityName) //全部部件中和该部件名称相同的大部件对象

          if (!bigCompObj) {
            //最终数组中没有该部件---给最终数组添加
            let getList = []
            item2.childNode.forEach(ele => {
              let obj = allBigCompObj.children.find(j => j.name == ele.entityName)
              if (obj) {
                getList.push(obj)
              }
            })
            arr.push({
              name: allBigCompObj.name,
              code: allBigCompObj.code,
              children: getList,
              key: allBigCompObj.key
            })
          } else {
            item2.childNode.forEach(ele => {
              let obj = bigCompObj.children.find(j => j.name == ele.entityName)
              if (!obj) {
                //给这个大部件添加小部件
                //添加的是在全部部件中找到的小部件
                let allobj = allBigCompObj.children.find(j => j.name == ele.entityName)
                bigCompObj.children.push(allobj)
              }
            })
          }
        })
      }) */

      return allCompNameList
    }
  }
}
</script>

<style lang="less" scoped>
@import url('./commonStyle.less');
</style>
