<template>
<section style="width: 100%;height: 100vh;background-color: rgb(11, 206, 206)">
  <el-form :model="login" ref="loginForm" :rules="rules" label-width="80px" class="login" style="text-align: center;">
    <h1>保密柜在线监控平台</h1>
    <el-form-item label="用户名" prop="UserName">
      <el-input v-model="login.UserName" placeholder="用户名"></el-input>
    </el-form-item>
    <el-form-item label="密码" prop="Password">
      <el-input type="password" v-model="login.Password" placeholder="密码"></el-input>
    </el-form-item>
    <el-button :loading="loading" class="el-button--primary" @click="loginSubmit('loginForm')" style="width: 180px">登录</el-button>
  </el-form>
  <span class="version">0.0.0.2</span>
</section>
</template>

<script>
import { SetToken } from '../utils/token';
import {encryptByDES} from '../utils/crypto';
import request from '../utils/request';
export default {
  name: 'login',
  data () {
    return {
      login: {
        UserName: '',
        Password: ''
      },
      rules: {
        UserName: [
          { required: true, message: '请输入用户名', trigger: 'blur' }
        ],
        Password: [
          { required: true, message: '请输入密码', trigger: 'blur' }
        ]
      },
      loading: false
    };
  },
  methods: {
    loginSubmit (formName) {
      this.$refs[formName].validate((valid) => {
        if (valid) {
          this.loading = true;
          request.post('/api/user/login', this.login).then(res => {
            console.log(res);
            this.loading = false;
            if (res.data.State === 1) {
              let result = res.data.Data;
              let uData = {
                DepartmentName: result.DepartmentName,
                NeedChangePassword: result.NeedChangePassword,
                RealName: result.RealName,
                RoleName: result.RoleName,
                UserID: result.UserID
              };
              SetToken(encryptByDES(JSON.stringify(uData)));
              this.$router.push('/index');
            } else {
              this.$message({
                message: res.data.Message,
                type: 'error'
              });
            }
          }).catch((err) => {
            console.log(err);
            this.loading = false;
            this.$message({
              message: '登录失败',
              type: 'error'
            });
          });
        } else {
          return false;
        }
      });
    }
  }
};
</script>

<style scoped>
.login {
  height: 255px;
  width: 500px;
  position: fixed;
  top: 40%;
  left: 50%;
  transform: translate(-50%,-60%);
}
.version {
  position: fixed;
  bottom: 0;
  right: 0;
  color: darkgrey;
}
</style>
