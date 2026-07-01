<template>
  <tabsPanNav ref="tabs"></tabsPanNav>
</template>

<script>
import { setTheme } from '@/util/util'

export default {
  components: {
    tabsPanNav: () => import('./components/tabsPanNav.vue')
  },
  created() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
  },
  provide() {
    return {
      clickTurbine: this.clickTurbine
    }
  },
  methods: {
    clickTurbine() {
      this.$refs['tabs'].clickTurbine()
    }
  }
}
</script>

<style></style>
