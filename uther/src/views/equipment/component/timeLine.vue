<template>
  <!-- 步骤条组件 -->
  <el-steps :active="isActive" align-center class="step_line">
    <el-step
      v-for="item in stepList"
      :key="item.code"
      :title="item.title"
      @click.native="clickStep(item)"
      :status="item.activeIndex == isActive ? 'error' : 'success'"
    ></el-step>
  </el-steps>
</template>

<script>
import { mapGetters } from 'vuex'
export default {
  props: {
    isActive: {
      type: String,
      default: '1'
    }
  },
  computed: {
    ...mapGetters(['menuAll'])
  },
  data() {
    return {
      stepList: [
        {
          title: '风场配置',
          code: 'wind',
          activeIndex: 0,
          path: '/system/dept'
        },
        {
          title: '机组配置',
          code: 'turbine',
          activeIndex: 1,
          path: '/equipment/turbine/index'
        },
        {
          title: '部件配置',
          code: 'comp',
          activeIndex: 2,
          path: '/equipment/component/index'
        },
        {
          title: '测点配置',
          code: 'spot',
          activeIndex: 3,
          path: '/spotManage/spot/index'
        },
        {
          title: '采集单元配置',
          code: 'unit',
          activeIndex: 4,
          path: '/equipment/unit/index'
        }
      ]
    }
  },
  mounted() {
    let menuAll = this.menuAll.flatMap(item => item.children)
    this.stepList.forEach(item => {
      let index = menuAll.findIndex(i => i.path == item.path)
      if (index > -1) {
        item.isPermission = true
      } else {
        item.isPermission = false
      }
    })
  },
  methods: {
    clickStep(item) {
      if (item.isPermission) {
        this.$router.push(item.path)
      } else {
        this.$message.warning('暂无权限')
      }
    }
  }
}
</script>

<style lang="less" scoped>
.step_line {
  border-bottom: 4px solid rgba(0, 0, 0, 0.35);
  padding-bottom: 16px;
}
.el-steps {
  width: 112%;
  margin-left: -7%;
  // margin-bottom: 18px;
  margin-top: 10px;
  // top: -9px;
  position: relative;
  ::v-deep .el-step__title {
    font-size: 14px;
    line-height: 22px;
    color: white;
  }
  ::v-deep .el-step__title.is-success {
    color: #c0c4cc;
  }
  ::v-deep .el-step__line {
    background-color: #11508b;
  }
  ::v-deep .el-step__head.is-success {
    color: #11508b;
    border-color: #11508b;
    .el-step__icon {
      background: #11508b;
    }
  }
  ::v-deep .el-step__head.is-error {
    color: #1f94ff;
    border-color: #1f94ff;
    .el-step__icon {
      background: #1f94ff;
    }
  }
  ::v-deep .el-step__icon {
    height: 16px;
    width: 16px;
    top: 4px;
    cursor: pointer;
  }
  ::v-deep .el-step__icon-inner {
    display: none;
  }
}
</style>
