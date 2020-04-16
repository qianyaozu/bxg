<template xmlns:v-contextmenu="http://www.w3.org/1999/xhtml" xmlns:v-popover="http://www.w3.org/1999/xhtml">
  <section>
    <v-contextmenu ref="contextmenu">
      <!--<v-contextmenu-item @click="cancelAlarm()">解除警音</v-contextmenu-item>-->
      <v-contextmenu-item v-if="detail.Status !== 0 && detail.Status !== 1 && detail.Status !== null" @click="cancelWarn()">解除警告</v-contextmenu-item>
    </v-contextmenu>

    <el-popover ref="popover" placement="bottom" width="300" trigger="hover">
      <table>
        <tr>
          <td>ID：</td><td>{{detail.ID}}</td>
        </tr>
        <tr>
          <td>手动编号：</td><td>{{detail.Code}}</td>
        </tr>
        <tr>
          <td>存放地址：</td><td>{{detail.Address}}</td>
        </tr>
        <tr>
          <td>单位名称：</td><td>{{detail.DepartmentName}}</td>
        </tr>
        <tr>
          <td>IP地址：</td><td>{{detail.IP}}</td>
        </tr>
        <tr>
          <td>责任人1：</td><td>{{detail.FirstContact}}</td>
        </tr>
        <tr>
          <td>电话：</td><td>{{detail.FirstContactPhone}}</td>
        </tr>
        <tr>
          <td>责任人2：</td><td>{{detail.SecondContact}}</td>
        </tr>
        <tr>
          <td>电话：</td><td>{{detail.SecondContactPhone}}</td>
        </tr>
      </table>
    </el-popover>

    <div class="box_wr" v-contextmenu:contextmenu  v-popover:popover @click="showLog" :style="{ pointerEvents: !detail.IsOnline ? 'none' : '' }">
      <div class="header"><p style="padding-top: 1px">{{detail.Name}}</p></div>
      <div class="contain">
        <div id="IMG_F0000D">
          <img v-if="detail.Status !== 1 && detail.Status !== 0 && detail.Status !== null && detail.Status !== 10 && detail.Status !== 3 && detail.IsOnline" src="../assets/img/0xWarning.png"  alt="" border="0"/>
          <img v-else-if="(detail.Status === 0 || detail.Status === null || detail.Status === 1 || detail.Status === 3) && detail.IsOnline" src="../assets/img/0xNormal.png" alt="" border="0">
          <img v-else-if="detail.Status === 10 || !detail.IsOnline" src="../assets/img/0xDisconnection.png" alt="" border="0">
        </div>
      </div>
      <div class="bmgfooter"><div id="INFO_F0000D">{{!detail.IsOnline ? '离线' : getOptionName(detail.Status)}}</div></div>
    </div>

    <!--弹框-->
    <el-dialog title="解除报警" :visible.sync="cancelWarnDialogVisible" width="30%" center>
      <el-row>
        <el-col :span="8">报警类型--</el-col>
        <el-col :span="16">{{getOptionName(detail.Status)}}</el-col>
      </el-row>
      <el-row>
        <el-col :span="8">保密柜ID：</el-col>
        <el-col :span="16">{{detail.ID}}</el-col>
      </el-row>
      <el-row>
        <el-col :span="8">保密柜编号：</el-col>
        <el-col :span="16">{{detail.Code}}</el-col>
      </el-row>
      <el-row>
        <el-col :span="8">存放地址：</el-col>
        <el-col :span="16">{{detail.Address}}</el-col>
      </el-row>
      <div class="line"></div>
      <el-row>
        <el-col :span="8">解除报警说明：</el-col>
        <el-col :span="16"><el-input type="textarea" :rows="3" v-model="cancelAlarm.EventContent"></el-input>
        </el-col>
      </el-row>

      <span slot="footer" class="dialog-footer">
        <el-button @click="submitCancelWarn" size="mini">确定</el-button>
        <el-button @click="cancelWarnDialogVisible = false" type="primary" size="mini">关闭</el-button>
      </span>
    </el-dialog>

    <el-dialog title="保密柜日志" :visible.sync="logTableVisible" width="70%">
      <el-dialog width="30%" title="[处理结果]" :visible.sync="innerVisible" append-to-body>
        <el-row>
          <el-col :span="8">报警类型</el-col>
          <el-col :span="16">{{handleDetail.warnType}}</el-col>
        </el-row>
        <div class="line"></div>
        <el-row>
          <el-col :span="8">保密柜ID：</el-col>
          <el-col :span="16">{{handleDetail.bmgId}}</el-col>
        </el-row>
        <el-row>
          <el-col :span="8">保密柜编号：</el-col>
          <el-col :span="16">{{handleDetail.code}}</el-col>
        </el-row>
        <el-row>
          <el-col :span="8">报警时间：</el-col>
          <el-col :span="16">{{handleDetail.alertTime}}</el-col>
        </el-row>
        <el-row>
          <el-col :span="8">处理时间：</el-col>
          <el-col :span="16">{{handleDetail.dealTime}}</el-col>
        </el-row>
        <el-row>
          <el-col :span="8">处理结果：</el-col>
          <el-col :span="16"><el-input type="textarea" :rows="3" v-model="handleDetail.result"></el-input>
          </el-col>
        </el-row>

        <span slot="footer" class="dialog-footer">
        <el-button @click="handleResulinnerVisibletVisible = false">关闭</el-button>
      </span>
      </el-dialog>

      <el-row>
        <el-col :span="24" style="float: left;margin: 3px 0">
          <span class="searchlabel">操作日期：</span>
          <el-date-picker
            v-model="searchParam.StartTime"
            type="date"
            size="mini"
            placeholder="选择日期">
          </el-date-picker>
          <span class="searchlabel">-</span>
          <el-date-picker
            v-model="searchParam.EndTime"
            type="date"
            size="mini"
            placeholder="选择日期">
          </el-date-picker>
          <el-select size="mini" v-model="searchParam.OperatorType" default-first-option placeholder="报警类型-全部">
            <el-option
              v-for="item in options"
              :key="item.value"
              :label="item.label"
              :value="item.value">
            </el-option>
          </el-select>
          <el-button size="small" @click="search">搜索</el-button>
        </el-col>
      </el-row>
      <el-table :data="tableData" size="mini">
        <el-table-column type="selection" width="55"></el-table-column>
        <el-table-column prop="CabinetID" label="保密柜ID"></el-table-column>
        <el-table-column prop="CabinetCode" label="保密柜编号"></el-table-column>
        <el-table-column label="使用记录">
          <template slot-scope="scope">
              <span>
                {{getOptionName(scope.row.OperationType)}}
                <a v-show="scope.row.DealResult !== null" href="javascript:void(0)" @click="showHandleResult(scope.row.ID)">使用记录</a>
              </span>
          </template>
        </el-table-column>
        <el-table-column prop="DepartmentName" label="使用单位"></el-table-column>
        <el-table-column prop="DealPerson" label="使用人"></el-table-column>
        <el-table-column prop="CabinetIP" label="发生IP"></el-table-column>
        <el-table-column label="服务器接收时间">
          <template slot-scope="scope">
            <span>{{getDate(scope.row.OperateTime)}}</span>
          </template>
        </el-table-column>
      </el-table>
      <div style="float:right">
        <el-pagination
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
          :current-page="currentPage"
          :page-sizes="[10, 20, 30, 40]"
          :page-size="20"
          layout="total, sizes, prev, pager, next, jumper"
          :total="totalNum">
        </el-pagination>
      </div>
    </el-dialog>
  </section>
</template>

<script>
import {warnTypeList} from '../utils/base';
import request from '../utils/request';
import moment from 'moment';

export default {
  props: ['detail'],
  name: 'bmg-detail',
  data () {
    return {
      rightMenu: false,
      cancelWarnMemo: '',
      cancelWarnDialogVisible: false,
      logTableVisible: false,
      innerVisible: false,
      tableData: [],
      searchParam: {
        CabinetID: 0,
        CabinetName: '',
        OperatorType: '',
        StartTime: '',
        EndTime: '',
        PageIndex: 0,
        PageSize: 0
      },
      options: warnTypeList,
      currentPage: 1,
      totalNum: 0,
      cancelAlarm: {
        CabinetID: 0,
        OperationType: 0,
        EventContent: '',
        Remark: ''
      },
      handleDetail: {
        warnType: '',
        bmgId: '',
        code: '',
        alertTime: '',
        dealTime: '',
        result: ''
      }
    };
  },
  methods: {
    getDate (time) {
      return moment(time).format('YYYY-MM-DD HH:mm:ss');
    },
    getOptionName (value) {
      if (value) {
        return this.options.filter(item => {
          return item.value === value.toString();
        })[0].label;
      } else {
        return '正常';
      }
    },
    // cancelAlarm () {
    //   alert('cancel alarm')
    // },
    cancelWarn () {
      this.cancelWarnDialogVisible = true;
    },
    showLog () {
      this.logTableVisible = true;
      this.search();
    },
    submitCancelWarn () {
      if (!this.cancelAlarm.EventContent || this.cancelAlarm.EventContent === '') {
        return;
      }
      this.cancelAlarm.CabinetID = this.detail.ID;
      this.cancelAlarm.OperationType = 23; // this.detail.Status

      request.post('/api/cabinetlog/submit', this.cancelAlarm).then(res => {
        if (res.data.State === 1) {
          this.$message({
            type: 'success',
            message: '操作成功!'
          });
          this.cancelWarnDialogVisible = false;
          this.$emit('childEvent');
        } else {
          this.$message({
            type: 'error',
            message: res.data.Message
          });
        }
      }).catch(() => {
        this.$message({
          type: 'error',
          message: '操作失败'
        });
      });
    },
    showInnerResult () {
      this.innerVisible = true;
    },
    search () {
      this.searchParam.CabinetID = this.detail.ID;
      this.searchParam.CabinetName = this.detail.Name;
      if (this.searchParam.StartTime) {
        this.searchParam.StartTime = moment(this.searchParam.StartTime).format('YYYY-MM-DD HH:mm:ss');
      }
      if (this.searchParam.EndTime) {
        this.searchParam.EndTime = moment(this.searchParam.EndTime).format('YYYY-MM-DD HH:mm:ss');
      }
      request.post('/api/cabinetlog/view', this.searchParam).then(res => {
        if (res.data.State === 1) {
          this.tableData = res.data.Data.Items;
          this.currentPage = res.data.Data.CurrentPage;
          this.totalNum = res.data.Data.TotalItems;
        }
      }).catch(err => {
        console.log(err);
      });
    },
    handleSizeChange (val) {
      this.searchParam.PageSize = val;
    },
    handleCurrentChange (val) {
      this.searchParam.PageIndex = val;
    },
    showHandleResult (id) {
      var temp = this.tableData.filter(item => {
        return item.ID === id;
      })[0];
      if (!temp) {
        return;
      }

      this.handleDetail.warnType = this.getOptionName(temp.OperationType);
      this.handleDetail.bmgId = temp.ID;
      this.handleDetail.code = temp.CabinetCode;
      this.handleDetail.alertTime = '';
      this.handleDetail.dealTime = '';
      this.handleDetail.result = temp.DealResult;

      this.innerVisible = true;
    }
  },
  watch: {
    'searchParam.PageSize': {
      handler (curVal, oldVal) {
        this.search();
      }
    },
    'searchParam.PageIndex': {
      handler (curVal, oldVal) {
        this.search();
      }
    }
  }
};
</script>

<style scoped>
  .box_wr {
    margin: 3px;
    padding: 0;
    float: left;
    width: 198px;
    height: 173px;
    list-style: none;
    background-image: url(../assets/img/beijing_03.png);
    position: relative;
    display: inline-block;
    font-size: 12px;
  }
  .header {
    margin-right: auto;
    margin-left: auto;
    width: 100px;
    height: 20px;
    text-align: center;
    margin-top: 5px;
    font-weight: bold;
  }
  .contain {
    margin-left: auto;
    margin-right: auto;
    width: 113px;
  }
  .bmgfooter {
    clear: both;
    margin: 0px auto;
    padding: 0px;
    width: 100px;
    height: 20px;
    text-align: center;
  }
</style>
