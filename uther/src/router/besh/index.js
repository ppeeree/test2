import Vue from 'vue'
import VueRouter from 'vue-router'
import routes from './routes'
//import routessss from '/router/besh/routes'

Vue.use(VueRouter)
/* const routes=[
  
] */
const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

export default router
