<template>
  <div class="click_btn_show">
    <img src="/img/WindTurbine/icon/clickBtnIcon.png" />
  </div>
</template>

<style lang="less" scoped>
.click_btn_show {
  position: relative;
  display: flex;
  justify-content: center;
  align-items: center; 
  z-index: 10;
  border: 1px solid transparent;
  background: rgba(36, 106, 143, 0.7);
  border-radius: 6px;
  width: 41px;
  height: 23px;
  cursor: pointer;
  &:hover {
    background: rgba(31, 255, 255, 0.4);
  }
}
</style>
