<template>
  <section>
    <el-row>
      <el-col :span="24" style="float: left;margin: 3px 0">
        <!--<span class="searchlabel">保密柜ID：</span>-->
        <!--<el-input-->
          <!--size="mini"-->
          <!--v-model="searchParam.CabinetID" style="width: 150px">-->
        <!--</el-input>-->
        <span class="searchlabel">保密柜编号：</span>
        <el-input
          size="mini"
          v-model="searchParam.CabinetName" style="width: 150px">
        </el-input>
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
          <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value"></el-option>
        </el-select>
        <el-button size="small" @click="search">搜索</el-button>
      </el-col>
      <!-- 删除 按钮-->
      <el-col :span="24">
        <el-button type="danger" size="small" @click="removeAll">删除</el-button>
      </el-col>
      <!-- table -->
      <el-col :span="24">
        <el-table :data="tableData" style="width: 100%" @selection-change="handleSelectionChange">
          <el-table-column type="selection" width="55"></el-table-column>
          <el-table-column prop="CabinetName" label="保密柜名称" width="180"></el-table-column>
          <el-table-column prop="CabinetCode" label="保密柜编号" width="180"></el-table-column>
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
        <div style="margin: 5px 0 0 0;float:right">
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
      </el-col>
    </el-row>

    <!--处理结果弹窗-->
    <el-dialog title="处理结果" :visible.sync="handleResultVisible" width="500px"  center>
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
        <el-button @click="handleResultVisible = false">关闭</el-button>
      </span>
    </el-dialog>
  </section>
</template>

<script>
import request from '../utils/request';
import moment from 'moment';
export default {
  name: 'monitor-log-mng',
  data () {
    return {
      searchParam: {
        CabinetID: '',
        CabinetName: '',
        OperatorType: '',
        StartTime: '',
        EndTime: '',
        PageIndex: 0,
        PageSize: 0
      },
      options: [{
        label: '报警类型-全部',
        value: ''
      }, {
        label: '正常开门',
        value: '1'
      }, {
        label: '密码错误',
        value: '2'
      }, {
        label: '正常关门',
        value: '3'
      }, {
        label: '非工作时间开门',
        value: '4'
      }, {
        label: '非工作时间关门',
        value: '5'
      }, {
        label: '外部电源断开',
        value: '6'
      }, {
        label: '备份电源电压低',
        value: '7'
      }, {
        label: '未按规定关门',
        value: '8'
      }, {
        label: '强烈震动',
        value: '9'
      }, {
        label: '网络断开',
        value: '10'
      }, {
        label: '修改密码',
        value: '11'
      }, {
        label: '设置参数',
        value: '12'
      }, {
        label: '上线',
        value: '13'
      }, {
        label: '下线',
        value: '14'
      }, {
        label: '请求语音',
        value: '15'
      }, {
        label: '结束语音',
        value: '16'
      }, {
        label: '接受语音',
        value: '18'
      }, {
        label: '拒绝语音',
        value: '19'
      }, {
        label: '允许开门',
        value: '20'
      }, {
        label: '拒绝开门',
        value: '21'
      }, {
        label: '申请开门',
        value: '24'
      }
      ],
      tableData: [],
      currentPage: 1,
      totalNum: 0,
      multipleSelection: [],
      handleResultVisible: false,
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
  mounted () {
    this.search();
  },
  methods: {
    getOptionName (value) {
      if (value === 0) {
        value = '';
        return '';
      } else {
        return this.options.filter(item => {
          return item.value === value.toString();
        })[0].label;
      }
    },
    getDate (time) {
      return moment(time).format('YYYY-MM-DD HH:mm:ss');
    },
    search () {
      if (this.searchParam.StartTime) {
        this.searchParam.StartTime = moment(this.searchParam.StartTime).format('YYYY-MM-DD HH:mm:ss');
      }
      if (this.searchParam.EndTime) {
        this.searchParam.EndTime = moment(this.searchParam.EndTime).format('YYYY-MM-DD HH:mm:ss');
      }
      request.post('/api/cabinetlog/view', this.searchParam).then(res => {
        if (res.data.State === 1) {
          // console.log(res)
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
    getSelectId () {
      var arr = [];
      this.multipleSelection.map(item => {
        arr.push(item.ID);
      });
      return arr;
    },
    removeAll () {
      if (this.multipleSelection.length === 0) {
        this.$message({
          message: '请选择一条记录',
          type: 'warning'
        });
        return;
      }

      this.$confirm(`确认要删除选中的日志数据, 是否继续?`, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
        center: true
      }).then(() => {
        var data = this.getSelectId();
        request.post(`/api/cabinetlog/delete`, data).then(res => {
          if (res.data.State === 1) {
            this.$message({
              type: 'success',
              message: '删除成功!'
            });
            this.search();
          } else {
            this.$message({
              type: 'error',
              message: '删除失败'
            });
          }
        }).catch(() => {
          this.$message({
            type: 'error',
            message: '删除失败'
          });
        });
      }).catch(() => {
        this.$message({
          type: 'info',
          message: '已取消删除'
        });
      });
    },
    handleSelectionChange (val) {
      this.multipleSelection = val;
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

      this.handleResultVisible = true;
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
.line{
  height: 3px;
  width: 100%;
  background: black;
}
</style>
