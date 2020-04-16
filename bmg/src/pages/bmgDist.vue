<template>
  <section>
    <el-row>
      <el-row>
        <!--<el-col :span="12">{{reMsg}}</el-col>-->
        <el-col :span="12" style="float: right;text-align: right;">
          <!--<el-button @click="returnTop">返回上一级</el-button>-->
          <!--<el-button @click="showSelect">获取当前id</el-button>-->
        </el-col>
      </el-row>
      <el-col :span="24">
        <div id="infovis" style="width: 800px;height: 800px;margin: 0 auto;"></div>
      </el-col>
    </el-row>
  </section>
</template>

<script>
import {init, jsonData, clickId} from '../utils/jit/ragraph';
import request from '../utils/request';
export default {
  name: 'bmg-dist',
  data () {
    return {
      selectId: clickId
    };
  },
  computed: {
    reMsg: {
      get: function () {
        return this.$store.getters.treeSelectId;
      },
      set: function (value) {
        this.selectId = value;
      }
    }
  },
  watch: {
    reMsg (curVal, oldVal) {
      console.log(curVal, oldVal);
    }
  },
  mounted () {
    request.get('/api/cabinet/map').then(res => {
      if (res.data.State === 1) {
        this.drawChart(res.data.Data);
      }
    }).catch(err => {
      console.log(err);
    });
  },
  methods: {
    showSelect () {
      // var json = this.getChildTag(clickId)
      // console.log(json)
      // this.drawChart(json)
      alert(this.reMsg + '----' + clickId);
    },
    drawChart (json) {
      var ragraph = init(json);
      setTimeout(function () {
        ragraph.refresh();
      }, 2000);
    },
    returnTop () {
      console.log('return top');
    },
    getChildTag (id) {
      return jsonData.children.filter(e => e.id === id);
    }
  }
};
</script>

<style scoped>

</style>
