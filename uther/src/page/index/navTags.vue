<template>
  <div>
    <ul class="cssNav">
      <li v-for="(item, index) in navList" :key="index" @click="clickItem(item)">
        <span :style="handleLastSpan(index)">{{ item[nameKey] }}</span>
      </li>
    </ul>
  </div>
</template>

<script>
export default {
  props: {
    navList: {
      type: Array,
      default: () => []
    },
    nameKey: {
      type: String,
      default: 'name'
    }
  },
  methods: {
    handleLastSpan(index) {
      const isLast = index + 1 === this.navList.length
      return isLast
        ? {
            fontWeight: 'bold',
            color: 'rgba(31, 255, 255, 1)'
          }
        : {}
    },
    clickItem(item) {
      if (this.$listeners['clickItem']) {
        this.$emit('clickItem', item)
      }
    }
  }
}
</script>

<style lang="less" scoped>
.cssNav {
  li {
    position: relative;
    display: inline-block;
    padding-left: 5px;
    color: #fff;
    background: #203049;
    line-height: 36px;
    cursor: pointer;
    height: 36px;
    &::after {
      content: '';
      position: absolute;
      right: -15px;
      top: 0;
      z-index: 10;
      display: block;
      border-top: 18px solid transparent;
      border-bottom: 18px solid transparent;
      border-left: 15px solid #203049;
    }
    &::before {
      content: '';
      position: absolute;
      left: -15px;
      top: 0;
      display: block;
      border-top: 18px solid #203049;
      border-bottom: 18px solid #203049;
      border-right: 15px solid transparent;
      transform: rotateY(180deg);
    }
    &:first-child::before {
      display: none;
    }
    span {
      font-size: 18px;
      line-height: 28px;
      letter-spacing: 0em;
      color: #1fffff;
      font-weight: bold;
      margin: 5px;
      // &:hover {
      //   color: #1fffff;
      // }
      &::before {
        content: '';
        display: block;
        height: 2px;
        width: 0%;
        background-color: #1fffff;
        transition: all ease-in-out 250ms;
        position: absolute;
        top: 24px;
        left: 4px;
      }
      &:hover::before {
        width: 95%;
      }
    }
  }
  li + li {
    margin-left: 23px;
  }
}
</style>
