const getters = {
  addRouters: state => state.permission.addRouters,
  routers: state => state.permission.routers,
  treeSelectId: state => state.pageInfo.treeSelectId,
  defaultRouter: state => state.permission.defaultRouter
};
export default getters;
