import Vue from 'vue';
import Router from 'vue-router';
// import HelloWorld from '@/components/HelloWorld'
import Login from '@/pages/Login';
import index from '@/pages/index';
import bmg from '@/pages/bmgIndex';
import bmgdistribution from '@/pages/bmgDist';
import warnTypeStat from '@/pages/warnTypeStat';
import warnCountStat from '@/pages/warnCountStat';
import warnCountPerMonthStat from '@/pages/warnCountPerMonthStat';
import monitorLogMng from '@/pages/monitorLogMng';
import mntCenterSet from '@/pages/mntCenterSet';
import organizeSet from '@/pages/organizeSet';
import bmgSet from '@/pages/bmgSet';
import systemLog from '@/pages/systemLog';

Vue.use(Router);

// 所有权限通用路由表
export const constantRoutermap = [
  {
    path: '/Login',
    name: 'Login',
    component: Login,
    hidden: true
  }, {
    path: '/',
    redirect: {
      name: 'Login'
    },
    hidden: true
  }
];

export default new Router({
  routes: constantRoutermap
});

// 异步挂载的路由

export const asynacRouterMap = [
  {
    path: '/index',
    name: 'index',
    component: index,
    meta: { roles: ['user', 'admin'] },
    children: [
      {
        path: '/bmg',
        name: '首页',
        component: bmg,
        meta: { roles: ['user'] }
      }, {
        path: '/bmgDist',
        name: '保密柜分布图',
        component: bmgdistribution,
        meta: { roles: ['user'] }
      }, {
        path: '/warnTypeStat',
        name: '报警类型统计',
        component: warnTypeStat
      }, {
        path: '/warnCountStat',
        name: '单位报警统计',
        component: warnCountStat
      }, {
        path: '/warnCountMonStat',
        name: '每月报警统计',
        component: warnCountPerMonthStat
      }, {
        path: '/mntLogStat',
        name: '监控日志管理',
        component: monitorLogMng
      }, {
        path: '/mntSet',
        name: '用户中心配置',
        component: mntCenterSet
      }, {
        path: '/organizeSet',
        name: '使用单位配置',
        component: organizeSet
      }, {
        path: '/bmgSet',
        name: '保密柜配置',
        component: bmgSet
      }, {
        path: '/systemLog',
        name: '系统日志',
        component: systemLog
      }
    ]
  }
];
//
// {
//   path: '/HelloWorld',
//     name: 'HelloWorld',
//   component: HelloWorld,
//   meta: { roles: ['admin'] }
// },
