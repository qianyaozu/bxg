import { asynacRouterMap, constantRoutermap } from '../router';

function hasPermission (roles, route) {
  if (route.meta && route.meta.roles) {
    return roles.some(role => route.meta.roles.indexOf(role) >= 0);
  } else {
    return true;
  }
}

const permission = {
  state: {
    routers: constantRoutermap,
    addRouters: [],
    defaultRouter: ''
  },
  mutations: {
    SET_ROUTERS: (state, routers) => {
      state.addRouters = routers;
      state.routers = constantRoutermap.concat(routers);
    },
    SET_DEFAULT_ROUTER: (state, router) => {
      state.defaultRouter = router;
    }
  },
  actions: {
    GenerateRoutes ({ commit }, data) {
      return new Promise(resolve => {
        const { roles } = data;
        const accessedRouters = asynacRouterMap.filter(v => {
          if (roles.indexOf('admin') >= 0) {
            return true;
          }
          if (hasPermission(roles, v)) {
            if (v.children && v.children.length > 0) {
              v.children = v.children.filter(child => {
                if (hasPermission(roles, child)) {
                  return child;
                } else {
                  return false;
                }
              });
              return v;
            } else {
              return v;
            }
          }
          return false;
        });
        commit('SET_ROUTERS', accessedRouters);
        resolve();
      });
    }
  }
};

export default permission;
