import Vue from 'vue';
import Vuex from 'vuex';
import permission from '@/store/permission';
import user from '@/store/user';
import getters from '@/store/getters';
import pageInfo from '@/store/pageInfo';

Vue.use(Vuex);

const store = new Vuex.Store({
  modules: {
    permission,
    user,
    pageInfo
  },
  getters
});
export default store;
