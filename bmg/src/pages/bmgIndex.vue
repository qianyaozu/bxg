<template>
  <section>
    <el-row :gutter="20">
      <el-col :span="4">
        <el-tree :data="treeData" @node-click="handleNodeClick" :highlight-current="true"
                 :default-expand-all="true"
                 :expand-on-click-node="false"></el-tree>
      </el-col>
      <el-col :span="20" style="">
        <bmgDetail v-for="(item,index) in tableData" :key="index" :detail="item"
                   @childEvent="realoadRight"></bmgDetail>
      </el-col>
    </el-row>
  </section>
</template>

<script>
import bmgDetail from '../components/bmgDetail';
import request from '../utils/request';
export default {
  name: 'bmg',
  data () {
    return {
      treeData: [],
      tableData: [],
      selectDepartId: '',
      si: ''
    };
  },
  mounted () {
    this.loadTreeData();
  },
  methods: {
    handleNodeClick (data) {
      this.selectDepartId = data.ID;
      this.searchRight();
    },
    loadTreeData () {
      request.get('/api/department/tree').then(res => {
        if (res.data.State === 1) {
          this.treeData = [];
          this.treeData.push(res.data.Data);
        }
      }).catch(err => {
        console.log(err);
      });
    },
    searchRight () {
      request.post(`/api/cabinet/viewbydepart?departID=${this.selectDepartId}`).then(res => {
        if (res.data.State === 1) {
          this.tableData = res.data.Data;
        }
      }).catch(err => {
        console.log(err);
      });
    },
    realoadRight () {
      this.searchRight();
    }
  },
  components: {
    bmgDetail
  },
  watch: {
    selectDepartId: {
      handler (curVal, oldVal) {
        if (curVal) {
          clearInterval(this.si);
          this.si = setInterval(() => {
            this.searchRight();
          }, 30 * 1000);
        }
      }
    }
  },
  destroyed () {
    if (this.si) {
      clearInterval(this.si);
    }
  }
};
</script>

<style scoped>

</style>
