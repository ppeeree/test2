<template>
  <div class="tree_card">
    <div class="tree_title">设备树</div>
    <div class="turbine_tree">
      <el-tree
        :data="turbineList"
        :props="defaultProps"
        @node-click="handleNodeClick"
        default-expand-all
        :expand-on-click-node="false"
        v-if="turbineList.length"
      >
        <template #default="{ data }">
          <span class="custom-tree-node">
          <span
            class="height_light"
            :style="{
              background: data.isClick ? '#3ffff9' : 'transparent',
              left: data.children ? '-23px' : '-41px'
            }"
          ></span>
          <span>{{ data.windturbineName }}</span>
          <el-tooltip
            class="item"
            effect="dark"
            content="机组已绑定模型"
            placement="right"
            v-show="!data.children && data.alarmModelId !== -1"
          >
            <i class="el-icon-link"></i>
          </el-tooltip>
          </span>
        </template>
      </el-tree>
      <noData v-else />
    </div>
  </div>
</template>

<script>
import { setStore } from '@/util/store'
import noData from '@/components/noData'
import { getList } from '@/api/alarmFault/alarmTree'
import { alarmTree } from '@/api/alarmFault/alarmModel'
export default {
  components: { noData },
  data() {
    return {
      allModelList: [], //所有模型数组
      turbineList: [], //树的数据
      selectTurbineItem: {}, //选中的机组、风场
      page: {
        total: 0, // 总页数
        currentPage: 1, // 当前页数
        pageSize: 100 // 每页显示多少条
      }
    }
  },
  computed: {},
  beforeCreate() {},
  mounted() {
    this.getTree()
  },
  methods: {
    //获取模型树接口
    getTree() {
      let nameArr = []
      alarmTree().then(res => {
        if (res.data.code == 200) {
          res.data.data.forEach(item => {
            nameArr.push(item)
            item.children.forEach(ite => {
              nameArr.push(ite)
            })
          })
          this.allModelList = JSON.parse(JSON.stringify(nameArr))

          setStore({
            name: 'allModelList',
            content: JSON.stringify(res.data.data)
          })
        }
      })
      setTimeout(() => {
        this.getTreeList()
      }, 300)
    },
    //获取列表接口
    getTreeList() {
      let obj = {
        offset: this.page.currentPage,
        pageSize: this.page.pageSize
      }
      getList({ ...obj }).then(res => {
        if (res.data.code == 200) {
          const data = res.data.data.data
          if (data.length) {
            this.turbineList = this.handlerList(data)
            this.turbineList[0].isClick = true
            this.$emit('clickTreeItem', this.turbineList[0])
          }
        }
      })
    },
    handlerList(arr) {
      let dataArr = []
      arr.map(mapItem => {
        let index = dataArr.findIndex(j => j.windparkId == mapItem.windparkId)
        if (index === -1) {
          dataArr.push({
            windparkId: mapItem.windparkId,
            windturbineName: mapItem.windparkName,
            isClick: false,
            children: [mapItem]
          })
        } else {
          dataArr[index].children.push(mapItem)
        }
      })

      return dataArr
    },
    //点击节点
    handleNodeClick(val) {
      this.selectTurbineItem = val

      this.turbineList.forEach(item => {
        item.isClick = false
        item.children.forEach(ite => {
          ite.isClick = false
        })
      })

      val.isClick = true
      this.$emit('clickTreeItem', val)
    }
  }
}
</script>

<style lang="less" scoped>
@import url('./style.less');
.tree_card {
  height: 100%;
  width: 420px;
  padding: 5px;
  float: left;
  .tree_title {
    font-size: 14px;
    font-weight: bold;
    color: white;
  }
  .turbine_tree {
    background-color: rgba(1, 14, 24, 0.5);
    height: 91%;
    margin: 10px 5px 5px 0;
    font-size: 14px;
    overflow-y: auto;
    &::-webkit-scrollbar-thumb {
      border-radius: 0px;
      background: rgba(13, 52, 83, 1);
    }
    &::-webkit-scrollbar-track {
      border-radius: 0;
      background: #131c35;
    }
    &::-webkit-scrollbar {
      width: 5px;
    }
  }
}

.custom-tree-node {
  font-size: 14px;
  .height_light {
    display: inline-block;
    height: 30px;
    width: 4px;
    background: #2a65ae;
    position: relative;
    float: left;
    left: -42px;
    top: 3px;
  }
  .el-icon-link {
    margin-left: 20px;
    color: #6388f6;
  }
}
</style>
