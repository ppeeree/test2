<template>
  <ul class="card_select" ref="closePopover">
    <div class="windField_select" v-if="viewType == 'now'">
      <el-select
        v-model="sortValue"
        placeholder="请选择排序方式"
        @change="val => sortChange(val, this.orderValue)"
      >
        <el-option
          v-for="item in sortOptions"
          :key="item.value"
          :label="item.label"
          :value="item.value"
        >
        </el-option>
      </el-select>
      <span
        class="status_list"
        @click="sortChange(sortValue, 'Order')"
        :style="{
          background: orderValue == 'Order' ? '#1E62CD' : 'transparent',
          width: '32px'
        }"
        title="升序"
      >
        <img src="/img/windField/OrderIcon.png"
      /></span>
      <span
        class="status_list"
        @click="sortChange(sortValue, 'noneOrder')"
        :style="{
          background: orderValue == 'noneOrder' ? '#1E62CD' : 'transparent',
          width: '32px'
        }"
        title="降序"
      >
        <img src="/img/windField/noneOrderIcon.png"
      /></span>
    </div>
  </ul>
</template>

<script>
export default {
  props: {
    viewType: {
      type: String,
      default: 'now'
    },
    sortOptions: {
      type: Array,
      default: () => []
    }
  },
  components: {},
  data() {
    return {
      orderValue: '',
      sortValue: ''
    }
  },

  mounted() {
    this.orderValue = 'Order'
    this.sortValue = this.sortOptions[0].value
  },
  methods: {
    sortChange(val, orderVal) {
      this.orderValue = orderVal
      this.$emit('sortChange', val, orderVal)
    }
  }
}
</script>

<style lang="less" scoped>
.card_select {
  width: auto;
  display: inline-block;
  display: flex;
  position: absolute;
  top: -10px;
  li {
    position: relative;

    flex-wrap: nowrap;
    padding-left: 5px;
    color: #fff;
    background: #102947;
    height: 45px;
    line-height: 45px;
    &::after {
      content: '';
      position: absolute;
      right: -15px;
      top: 0;
      z-index: 10;
      display: block;
      border-top: 23px solid transparent;
      border-bottom: 23px solid transparent;
      border-left: 15px solid #102947;
    }
    &::before {
      content: '';
      position: absolute;
      left: -15px;
      top: 0;
      display: block;
      border-top: 23px solid #102947;
      border-bottom: 23px solid #102947;
      border-right: 15px solid transparent;
      transform: rotateY(180deg);
    }
    &:first-child::before {
      display: none;
    }
    &:last-child::after {
      display: none;
    }
    span {
      font-size: 18px;
      line-height: 28px;
      letter-spacing: 0em;
      color: #1fffff;
      font-weight: bold;
      margin: 5px;
    }
  }
  li + li {
    margin-left: 30px;
  }
}
.wind_farm_icon {
  height: 45px;
  width: 45px;
  background-color: rgba(13, 52, 83, 0.6);
  color: white;
  line-height: 50px;
  text-align: center;
  cursor: pointer;
  display: inline-block;
}

// 第一个li样式
.wind_farm_tag {
  margin-top: 10px;
  // .more_icon {
  //   display: inline-block;
  //   cursor: pointer;
  //   position: relative;
  //   top: 3px;
  // }
}
.more_wind_card {
  width: auto;
  max-height: 220px !important;
  background-color: rgba(9, 39, 76, 0.6) !important;
  backdrop-filter: blur(10px);
  overflow-y: auto;
  padding: 10px;
  .wind_item {
    height: 36px;
    line-height: 36px;
    padding-left: 10px;
    font-size: 14px;
    color: white;
    cursor: pointer;
  }
}
.wind_information_card {
  width: 210px !important;
  max-height: 150px !important;
  background-color: rgba(9, 39, 76, 0.6) !important;
  backdrop-filter: blur(10px);
  overflow-y: auto;
  padding: 10px;
  font-size: 14px;
  color: white;
  .wind_item {
    height: 36px;
    line-height: 36px;
    padding-left: 10px;
  }
}
// 第二个li样式
.windField_select {
  display: flex;
  align-items: center;
  flex-wrap: nowrap;
  white-space: nowrap;
  .wind_inform {
    position: relative;
    left: 3px;
    top: 4px;
    cursor: pointer;
    display: inline-block;
    width: 35px;
    height: 20px;
  }
  .turbine_num {
    margin-left: 15px;
    margin-right: 35px;
    font-size: 15px;
    font-weight: 350;
    span {
      color: white;
      font-size: 18px;
      font-weight: bold;
    }
  }
  .status_list {
    flex: 0 0 32px;
    color: rgba(255, 255, 255, 0.6);
    border: rgba(30, 98, 205, 0.502) 1px solid;
    border-radius: 4px;
    padding: 0 8px;
    height: 31px;
    // display: inline-block;
    display: inline-flex;
    flex-direction: row;
    align-items: center;
    justify-content: center;
    line-height: 28px;
    margin: 0 0 0 8px;
    font-size: 13px;
    font-weight: 350;
    cursor: pointer;
    span {
      color: white;
      font-size: 18px;
      font-weight: bold;
      margin: 0 4px;
    }
    .status_icon {
      width: 6px;
      height: 14px;
      border-radius: 3px;
      display: inline-block;
      // top: 2px;
      // position: relative;
      margin-right: 5px;
    }
    img {
      width: 18px;
      /*  position: relative;
      top: 2px; */
    }
  }
  :deep(.el-select){
    flex: 0 0 210px;
    width: 210px;
    height: 35px;
    .el-input--suffix .el-input__inner {
      height: 35px;
      color: white;
    }
  }
}
</style>
