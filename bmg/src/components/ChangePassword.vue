<template>
<section>
  <el-dialog title="修改密码"  width="500px"  :visible.sync="ShowChagePwd" :before-close="resetForm">
    <el-form :model="pass" :rules="rules" ref="changePwdForm" label-width="120px">
      <el-form-item label="密码:" prop="password">
        <el-input v-model.trim ="pass.password" size="mini" style="width: 250px" placeholder="8 - 20字符"></el-input>
      </el-form-item>
    </el-form>
    <div slot="footer" class="dialog-footer">
      <el-button @click="resetForm" size="small">取 消</el-button>
      <el-button type="primary" @click="submitPwd()" size="small">确 定</el-button>
    </div>
  </el-dialog>
</section>
</template>

<script>
import { PassWordRegex } from '../utils/base';
import request from '../utils/request';
export default {
  name: 'change-password',
  props: ['changePwdVisible'],
  data () {
    var CheckPassword = (rule, value, callback) => {
      if (!PassWordRegex.test(value)) {
        callback(new Error('密码必须包含英文和数字，长度8-20位'));
      } else {
        callback();
      }
    };
    return {
      pass: {
        id: '',
        password: ''
      },
      rules: {
        password: [
          { required: true, message: '必填字段', trigger: 'blur' },
          { min: 8, max: 20, message: '长度在 8 到 20 个字符', trigger: 'blur' },
          { validator: CheckPassword, trigger: 'blur' }
        ]
      },
      ShowChagePwd: this.changePwdVisible
    };
  },
  methods: {
    resetForm () {
      this.ShowChagePwd = false;
      this.$refs.changePwdForm.resetFields();
    },
    submitPwd () {
      this.$refs.changePwdForm.validate((valid) => {
        if (valid) {
          request.post(`/api/user/changepassword?password=${this.pass.password}`).then(res => {
            console.log(res);
            if (res.data.State === 1) {
              this.$message({
                type: 'success',
                message: '密码修改成功，稍后将跳转至登录页面，请重新登录'
              });
              setTimeout(() => {
                this.$router.replace('/Login');
              }, 2000);
            } else {
              this.$message({
                type: 'error',
                message: `${res.data.Message}`
              });
            }
          }).catch(() => {});
        } else {
          return false;
        }
      });
    }
  },
  watch: {
    changePwdVisible (value) {
      this.ShowChagePwd = value;
    }
  }
};
</script>

<style scoped>

</style>
