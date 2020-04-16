<template>
  <section>
    <el-row>
      <el-col :span="24" style="text-align: center;">
        <span>[</span>
        <span class="searchlabel">日期：</span>
        <el-date-picker
          v-model="searchParam.From"
          type="date"
          size="mini"
          placeholder="选择日期"
          value-format="yyyy-MM-dd">
        </el-date-picker>
        <span class="searchlabel">-</span>
        <el-date-picker
          v-model="searchParam.To"
          type="date"
          size="mini"
          placeholder="选择日期"
          value-format="yyyy-MM-dd">
        </el-date-picker>
        <span>年] [ </span>
        <el-select size="mini" v-model="searchParam.DepartmentID">
          <el-option
            v-for="item in orgOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value">
          </el-option>
        </el-select>
        <span>中心] 产生报警次数最多的前10个单位</span>
      </el-col>
      <el-col :span="24">
        <div id="highcharts-container"></div>
      </el-col>
    </el-row>

    <el-dialog title="保密柜日志" :visible.sync="dialogTableVisible"  width="70%" >
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
      <el-table :data="gridData" size="mini">
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
import request from '../utils/request';
import moment from 'moment';
import {warnTypeList} from '../utils/base';
import Highcharts from 'highcharts/highstock';
import Highchartsmore from 'highcharts/highcharts-more';
import HighchartsDrilldown from 'highcharts/modules/drilldown';
import Highcharts3D from 'highcharts/highcharts-3d';
Highchartsmore(Highcharts);
HighchartsDrilldown(Highcharts);
Highcharts3D(Highcharts);

export default {
  name: 'warn-count-stat',
  data () {
    return {
      searchParam: {
        From: '',
        To: '',
        DepartmentID: ''
      },
      options: {
        chart: {
          type: 'column'
        },
        title: {
          text: null
        },
        xAxis: {
          type: 'category',
          labels: {
            rotation: 0,
            style: {
              fontSize: '13px',
              fontFamily: 'Verdana, sans-serif'
            }
          }
        },
        yAxis: {
          min: 0,
          title: {
            text: '总报警次数'
          }
        },
        legend: {
          enabled: false
        },
        credits: {
          enabled: false // 不显示LOGO
        },
        tooltip: {
          pointFormat: '报警: <b>{point.y} 次</b>'
        },
        series: []
      },
      chart: '',
      dialogTableVisible: false,
      innerVisible: false,
      gridData: [],
      currentPage: 1,
      orgOptions: [{
        label: '-----不限------',
        value: 0
      }],
      chartData: [],
      totalNum: 0,
      handleDetail: {
        warnType: '',
        bmgId: '',
        code: '',
        alertTime: '',
        dealTime: '',
        result: ''
      },
      logSearchParam: {
        DepartmentID: '',
        PageIndex: 0,
        PageSize: 0
      }
    };
  },
  mounted () {
    this.loadDepartment();
    this.chart = new Highcharts.Chart('highcharts-container', this.options);
  },
  methods: {
    getOptionName (value) {
      if (value) {
        return warnTypeList.filter(item => {
          return item.value === value.toString();
        })[0].label;
      } else {
        return '正常';
      }
    },
    getDate (time) {
      return moment(time).format('YYYY-MM-DD HH:mm:ss');
    },
    reDrawChart (data) {
      var that = this;
      if (data) {
        var temp = [];
        data.map(item => {
          temp.push([item.DepartmentName, item.Count]);
        });
        var series = {
          name: '次数',
          data: temp,
          dataLabels: {
            enabled: true,
            rotation: 0,
            color: '#000000',
            align: 'center',
            format: '{point.y:.1f}', // one decimal
            y: 10, // 10 pixels down from the top
            style: {
              fontSize: '13px',
              fontFamily: 'Verdana, sans-serif'
            }
          },
          events: {
            click: function (e) {
              console.log(e);
              that.showWarningLog(e.point.name);
            }
          }
        };
        this.options.series[0] = series;
        this.chart = new Highcharts.Chart('highcharts-container', this.options);
      }
    },
    showWarningLog (name) {
      console.log(this.chartData);
      var temp = this.chartData.filter(item => {
        return item.DepartmentName === null; // name
      })[0];
      this.logSearchParam.DepartmentID = temp.DepartmentID;
      this.dialogTableVisible = true;
    },
    resetEditForm () {
      this.$refs.editOrgForm.resetFields();
      this.editDialogFormVisible = false;
    },
    handleSelectionChange (val) {
      this.multipleSelection = val;
      console.log(val);
    },
    showInnerResult () {
      this.innerVisible = true;
    },
    handleSizeChange (val) {
      this.logSearchParam.PageSize = val;
    },
    handleCurrentChange (val) {
      this.logSearchParam.PageIndex = val;
    },
    loadChartData () {
      request.post('/api/cabinetlog/departAlarmStatistics', this.searchParam).then(res => {
        if (res.data.State === 1) {
          console.log(res);
          this.chartData = res.data.Data;
          this.reDrawChart(res.data.Data);
        }
      }).catch(err => {
        console.log(err);
      });
    },
    loadDepartment () {
      request.get('/api/department/lowerdepart').then(res => {
        if (res.data.State === 1) {
          this.orgOptions = this.orgOptions.concat(res.data.Data);
        }
      }).catch(err => {
        console.log(err);
      });
    },
    searchLog () {
      if (!this.searchParam.From || !this.searchParam.To) {
        return;
      }
      request.post('/api/cabinetlog/view', this.logSearchParam).then(res => {
        if (res.data.State === 1) {
          this.gridData = res.data.Data.Items;
          this.currentPage = res.data.Data.CurrentPage;
          this.totalNum = res.data.Data.TotalItems;
        }
      }).catch(err => {
        console.log(err);
      });
    },
    showHandleResult (id) {
      var temp = this.gridData.filter(item => {
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
    searchParam: {
      handler: function () {
        this.loadChartData();
      },
      deep: true
    },
    'logSearchParam.PageSize': {
      handler () {
        this.searchLog();
      }
    },
    'logSearchParam.PageIndex': {
      handler () {
        this.searchLog();
      }
    }
  }
};
</script>

<style scoped>

</style>
