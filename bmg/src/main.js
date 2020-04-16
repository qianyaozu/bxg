// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue';
import App from './App';
import router from './router';
import ElementUi from 'element-ui';
import vueCookie from 'vue-cookie';
import 'element-ui/lib/theme-chalk/index.css';
import store from './store';
import contentmenu from 'v-contextmenu';
import {GetToken, DeleteAllCookie} from './utils/token';
import Snotify from 'vue-snotify';
import 'vue-snotify/styles/simple.css';
import 'v-contextmenu/dist/index.css';
import './assets/index.css';

Vue.use(contentmenu);
Vue.use(vueCookie);
Vue.use(ElementUi);
Vue.config.productionTip = false;
Vue.use(Snotify);

router.beforeEach((to, from, next) => {
  if (to.path !== '/Login') {
    if (GetToken()) {
      store.commit('SET_DEFAULT_ROUTER', to.path);
      const roles = ['user'];
      store.dispatch('GenerateRoutes', {roles}).then(() => { // 生成可访问的路由表
        router.addRoutes(store.getters.routers);
      }).catch(err => {
        console.log(err);
      });
      next();
    } else {
      next({path: '/Login'});
    }
  } else {
    DeleteAllCookie();
    next();
  }
});

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  store,
  components: { App },
  template: '<App/>'
});
