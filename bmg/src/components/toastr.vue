<template>
<div class="toastr" v-show="isShow">
  <div class="t-header">
    <i class="el-icon-close" @click="closeToastr"></i>
  </div>
  <div class="t-body" style="text-align: center">
    <span>保密柜：{{result.CabinetName}} <br/>
    请求 {{GetTypeName(result.OperationType)}}</span>
  </div>
  <div class="t-foot">
    <el-button type="primary" size="small" @click="confirmFun">确认</el-button>
    <el-button type="error" size="small">取消</el-button>
  </div>
</div>
</template>

<script>
import { diff, GetWarnName } from '../utils/base';
import request from '../utils/request';
export default {
  name: 'toastr',
  props: ['result', 'popLazy'],
  data () {
    return {
      isShow: true,
      myData: null,
      submitLog: {
        CabinetID: 0,
        OperationType: 0
      }
    };
  },
  watch: {
    result: {
      handler: function (val) {
        this.myData = val;
      },
      deep: true,
      immediate: true
    },
    myData: {
      handler: function (val, oldVal) {
        if (!diff(val, oldVal)) {
          if (this.myData.OperationType !== 15 && this.myData.OperationType !== 1 && this.myData.OperationType !== 4) {
            setTimeout(() => {
              this.$snotify.info(`${this.myData.CabinetName}  ${GetWarnName(this.myData.OperationType)}`, {
                timeout: 3000
              });
            }, this.popLazy * 500);
          } else {
            console.log(this.myData);
            let photos = this.myData.Photos;
            let photoUrl = '';
            if (photos != null && photos !== [] && photos.length > 0) {
              photoUrl = `http://47.98.230.218:8080/upload/${photos[0]}`;
            }
            setTimeout(() => {
              // this.$snotify.prompt(`${this.myData.CabinetName}  请求 ${this.GetTypeName(this.myData.OperationType)}`, '消息', {
              //   buttons: [
              //     {text: '确认', action: this.confirmFun, bold: true},
              //     {text: '拒绝', action: this.refuseApply}
              //   ]
              // })
              this.$snotify.simple(`${this.myData.CabinetName}  请求 ${this.GetTypeName(this.myData.OperationType)}`, '消息', {
                timeout: 0,
                showProgressBar: false,
                closeOnClick: true,
                icon: photoUrl, // require('../assets/logo.png'),
                buttons: [
                  {text: '确认', action: this.confirmFun, bold: true},
                  {text: '拒绝', action: this.refuseApply}
                ]
              });
            }, this.popLazy * 500);
          }
        }
      },
      deep: true,
      immediate: true
    }
  },
  methods: {
    closeToastr () {
      this.isShow = false;
    },
    confirmFun (toast) {
      this.submitLog.CabinetID = this.myData.CabinetID;
      if (this.result.OperationType === 15) {
        this.submitLog.OperationType = 18;
      } else if (this.result.OperationType === 1 || this.result.OperationType === 4) {
        this.submitLog.OperationType = 20;
      }
      request.post('/api/cabinetlog/submit', this.submitLog).then(res => {
        if (res.data.State === 1) {
          this.$message({
            type: 'success',
            message: '操作成功'
          });
          if (this.result.OperationType === 15) {
            // let vUrl = `https://47.98.230.218:8081/demos/Audio-Conferencing.html?room=${this.myData.CabinetMac}`
            // let vUrl = `http://47.98.230.218:8081/?room=${this.myData.CabinetMac}`;
            let vUrl = `https://127.0.0.1:9001/demos/Audio-Conferencing.html?room=${this.myData.CabinetMac}`
            window.open(vUrl, '视频窗口', 'height=600,width=800,top=0,left=0,toolbar=no,menubar=no,scrollbars=no,resizable=no,location=no, status=no');
            this.$snotify.remove(toast.id);
          }
          this.$snotify.remove(toast.id);
        } else {
          this.$message({
            type: 'error',
            message: '操作失败'
          });
        }
      }).catch(() => {});
      // if (this.result.OperationType === 15) {
      //   var vUrl = `https://47.98.230.218:8081/demos/Audio-Conferencing.html?room=${this.myData.CabinetMac}`
      //   window.open(vUrl, '视频窗口', 'height=600,width=800,top=0,left=0,toolbar=no,menubar=no,scrollbars=no,resizable=no,location=no, status=no')
      //   this.$snotify.remove(toast.id)
      // } else {
      //   request.post(`/api/cabinet/updatestatus?id=${this.myData.CabinetID}&status=1`).then(res => {
      //     if (res.data.State === 1) {
      //       this.$message({
      //         type: 'success',
      //         message: '同意开门操作成功'
      //       })
      //       this.$snotify.remove(toast.id)
      //     } else {
      //       this.$message({
      //         type: 'error',
      //         message: '操作失败'
      //       })
      //     }
      //   }).catch(() => {})
      // }
    },
    refuseApply (toast) {
      this.submitLog.CabinetID = this.myData.CabinetID;
      if (this.result.OperationType === 15) {
        this.submitLog.OperationType = 19;
      } else if (this.result.OperationType === 1 || this.result.OperationType === 4) {
        this.submitLog.OperationType = 21;
      }
      request.post('/api/cabinetlog/submit', this.submitLog).then(res => {
        if (res.data.State === 1) {
        } else {
        }
      }).catch(() => {});
      this.$snotify.remove(toast.id);
    },
    GetTypeName (val) {
      if (val === 15) {
        return '请求语音';
      } else if (val === 1) {
        return '请求开门';
      } else if (val === 4) {
        return '非正常开门';
      }
    },
    test () {
      alert('123');
    }
  },
  destroyed () {
    this.$snotify.clear();
  }
};
</script>

<style scoped>
.toastr {
  height: 200px;
  width: 380px;
  position: fixed;
  right: 5px;
  bottom: 5px;
  background-color: #ffffff;
  z-index: 20;
  box-shadow: 0 2px 12px 0 rgba(0,0,0,.1);
}
.t-header{
  height: 60px;
}
.t-header > i{
  float: right;
  cursor:pointer;
}
.t-body{
  height: 100px;
}
.t-foot{
  height: 40px;
  text-align: center;
}

</style>
