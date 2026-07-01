import Layout from '@/page/index/'

export default [{
  path: '/login',
  name: '登录页',
  component: () =>
    import(/* webpackChunkName: "view-user-login" */'@/page/login/index'),
  meta: {
    keepAlive: true,
    isTab: false,
    isAuth: false
  }
},
{
  path: '/lock',
  name: '锁屏页',
  component: () =>
    import('@/page/lock/index'),
  meta: {
    keepAlive: true,
    isTab: false,
    isAuth: false
  }
},
{
  path: '/404',
  component: () =>
    import('@/components/error-page/404'),
  name: '404',
  meta: {
    keepAlive: true,
    isTab: false,
    isAuth: false
  }

},
{
  path: '/403',
  component: () =>
    import('@/components/error-page/403'),
  name: '403',
  meta: {
    keepAlive: true,
    isTab: false,
    isAuth: false
  }
},
{
  path: '/500',
  component: () =>
    import('@/components/error-page/500'),
  name: '500',
  meta: {
    keepAlive: true,
    isTab: false,
    isAuth: false
  }
},
/* {
  path: '/wel',
  component: Layout,
  name: '管理场景',
}, */

{
  path: '/myiframe',
  component: Layout,
  redirect: '/myiframe',
  children: [{
    path: ':routerPath',
    name: 'iframe',
    component: () =>
      import('@/components/iframe/main'),
    props: true
  }]
},
{
  path: '/',
  component: Layout,
  redirect: '/screen',
  children: [{
    path: '/screen',
    name: 'screen',
    component: () =>
      import(/* webpackChunkName: "view-user-index" */'@/views/screen/index'),
    meta: {
      keepAlive: false,
      isTab: false,
      isAuth: false
    }
  }, {
    path: '/WindTurbine',
    name: 'WindTurbine',
    component: () =>
      import(/* webpackChunkName: "view-user-index" */'@/views/WindTurbine/index'),
    meta: {
      keepAlive: false,
      isTab: false,
      isAuth: false
    }
  },
  {
    path: '/customAnalysis',
    name: 'customAnalysis',
    meta: {
      keepAlive: false,
      isTab: false,
      isAuth: false
    },
    children: [{
      path: '/customAnalysis',
      //name: '波形分析',
      component: () =>
        import(/* webpackChunkName: "view-user-detail" */'@/views/customAnalysisNew/index'),
    }]
  },
  {
    path: '/customAnalysisNew/index',
    name: 'customAnalysisNew',
    component: () =>
      import(/* webpackChunkName: "view-user-detail" */'@/views/customAnalysisNew/index'),
    meta: {
      keepAlive: false,
      isTab: false,
      isAuth: false
    }
  },
  {
    path: '/diagnosticTasks',
    name: 'diagnosticTasks',
    meta: {
      keepAlive: false,
      isTab: false,
      isAuth: false
    },
    children: [{
      path: '/diagnosticTasks',
      name: 'diagnosticTask',
      component: () =>
        import(/* webpackChunkName: "view-user-detail" */'@/views/diagnosticTasks/index'),
    },
    ]
  },
  {
    path: '/reportManage',
    meta: {
      keepAlive: false,
      isTab: false,
      isAuth: false
    },
    children: [{
      path: '/reportManage',
      component: () =>
        import(/* webpackChunkName: "view-user-detail" */'@/views/reportManage/index'),
    }]
  }
  ]
},

{
  path: '*',
  redirect: '/404'
},
/* {
  path: '/analysis/chart',
  name: 'bigChart',
  component: () =>
    import('@/views/chartNewPage/index'),
  meta: {
    keepAlive: false,
    isTab: false,
    isAuth: false
  }
}, */
{
  path: '/analysis/bigchart',
  name: 'bigChartT',
  component: () =>
    import(/* webpackChunkName: "view-user-detail2" */ '@/views/customAnalysisNew/bigChart/index'),
  meta: {
    keepAlive: false,
    isTab: false,
    isAuth: false
  }
},
]
