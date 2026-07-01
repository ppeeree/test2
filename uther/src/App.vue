<template>
  <div id="app">
    <router-view />
  </div>
</template>

<script>
import { mapGetters } from 'vuex'

export default {
  name: 'app',
  data() {},
  watch: {
    'userInfo.role_name': {
      handler(val) {
        // this.website.jurisdiction = val === 'administrator'
      },
      immediate: true
    }
  },
  created() {},
  mounted() {},
  computed: {
    ...mapGetters(['userInfo'])
  },
  methods: {}
}
</script>
<style lang="scss">
#app {
  width: 100%;
  height: 100%;
  overflow: hidden;
  background: url('/img/WindTurbine/background/bg.png');
}
.avue--detail .el-col {
  margin-bottom: 0;
}
</style>
