//定义路由规则(路由表)
const routes = [
   
  {
    path: '/about',
    name: 'about',
    component: () => import('../views/besh/about.vue'),
      
    children:[
      {
        path: '/user',
        name: 'user',
        component: () => import('../views/besh/user.vue')

      },
      {
        path: '/dns',
        name: 'dns',
        component: () => import('../components/besh/dns.vue')

      },
      {
        path: '/top',
        name: 'top',
        component: () => import('../components/besh/top.vue')

      },
      {
        path: '/home',
        name: 'home',
        component: () => import('../components/besh/home.vue')

      },
    ]
  },
    
]

export default routes