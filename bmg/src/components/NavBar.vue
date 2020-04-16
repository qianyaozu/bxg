<template>
  <section>
    <el-row style="height: 80px;background-color: steelblue;">
      <el-col :span="6">
        <div class="grid-content bg-purple-dark" style="text-align: center">
          <h2>{{departMent}}</h2>
        </div>
      </el-col>
      <el-col :span="18">
        <div class="grid-content bg-purple-dark">
          <a href="javascript:void(0)" style="float: right;font-size: 18px;color: white;font-weight: bold;line-height: 80px;" v-on:click="logOut">退出登录</a>
        </div>
      </el-col>
    </el-row>
    <el-menu :default-active="this.$store.getters.defaultRouter" class="el-menu-demo" mode="horizontal"
             style="maring-top:60px" :router="true" @select="handleSelect">
      <el-menu-item v-if="!item.hidden" :index="item.path"
                    v-for="(item,index) in this.$store.getters.addRouters[0].children"
                    v-bind:key="index">
        {{item.name}}
      </el-menu-item>
    </el-menu>
    <!--<transition name="el-zoom-in-bottom" mode="out-in">-->
    <toastr v-for="(item, index) in this.childData" v-show="showToastr"
            :result="item" :key="index" :popLazy="index"/>
    <!--</transition>-->

    <!--修改密码-->
    <changePwd :changePwdVisible="changePwdShow"></changePwd>

    <vue-snotify></vue-snotify>
  </section>
</template>

<script>
import toastr from '../components/toastr';
import changePwd from '../components/ChangePassword';
import { DeleteAllCookie, SetRouter, GetDepartment, NeedChangePwd } from '../utils/token';
import { diff } from '../utils/base';
import request from '../utils/request';
// import moment from 'moment';
export default {
  name: 'navbar',
  data () {
    return {
      showToastr: false,
      childData: null,
      departMent: GetDepartment(),
      changePwdShow: false,
      loopRequest: null,
      nextRequestUnix: null
    };
  },
  methods: {
    logOut () {
      DeleteAllCookie();
      this.$router.replace('/Login');
    },
    handleSelect (key) {
      SetRouter(key);
    },
    requestForCommand () {
      let requestTime;
      if (this.nextRequestUnix) {
        requestTime = this.nextRequestUnix;
      } else {
        requestTime = 0; // moment().unix() * 1000
      }
      request.get(`/api/cabinetlog/command?time=${requestTime}`).then(res => {
        // console.log(res)
        if (res.data.State === 1) {
          // console.log(res)
          this.nextRequestUnix = res.data.Data.time;
          if (res.data.Data.list.length > 0) {
            // this.showToastr = true
            // res.data.Data = res.data.Data.reverse()
            // this.childData = res.data.Data[0]
            if (this.childData) {
              res.data.Data.list.filter(item => {
                this.childData.every(eb => !diff(item, eb));
              });
            }
            this.childData = res.data.Data.list;
          }
        }
      }).catch(() => {});
    }
  },
  mounted () {
    // setInterval(function () {
    //   console.log('loop')
    // }, 1000)
    // 循环请求
    // var that = this
    // this.childData.name = '11111'
    // setTimeout(function () {
    //   that.showToastr = true
    //   that.changePwdShow = true
    // }, 5000)
    // this.changePwdShow = true // 修改密码弹框
    setTimeout(() => {
      if (NeedChangePwd()) {
        this.changePwdShow = true;
      }
    }, 2000);
    if (this.loopRequest) {
      clearInterval(this.loopRequest);
    }
    this.loopRequest = setInterval(() => {
      this.requestForCommand();
    }, 8000);
  },
  components: {
    toastr,
    changePwd
  },
  destroyed () {
    if (this.loopRequest) {
      clearInterval(this.loopRequest);
    }
  }
};
</script>

<style>
  .component-fade-enter-active, .component-fade-leave-active {
    transition: opacity .3s ease;
  }
  .component-fade-enter, .component-fade-leave-to
    /* .component-fade-leave-active for below version 2.1.8 */ {
    opacity: 0;
  }
  .snotifyToast__input {
    display: none;
  }
</style>
