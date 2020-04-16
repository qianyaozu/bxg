<template>
  <section>
    <el-row>
      <el-col :span="24" style="text-align: center">
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
        <span>] [ </span>
        <el-select size="mini" v-model="searchParam.DepartmentID">
          <el-option
            v-for="item in orgOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value">
          </el-option>
        </el-select>
        <span>中心] 报警类型统计</span>
      </el-col>
      <el-col :span="24" style="text-align: center">
        <div id="highcharts-container" class="pieCharts"></div>
        <table class="side-table" style="position: absolute;top: 10px;">
          <thead>
            <th><span>报警类型</span></th>
            <th><span>总次数</span></th>
          </thead>
          <tbody>
            <tr v-for="(item, index) in this.chartData" :key="index" :style="{ color: colors[index]}">
              <td><span>{{item[0]}}</span></td>
              <td><span>{{item[1]}}</span></td>
            </tr>
          </tbody>
        </table>
      </el-col>
    </el-row>

    <el-dialog title="保密柜日志"  width="70%"  :visible.sync="dialogTableVisible">
      <el-dialog width="300px" title="[处理结果]" :visible.sync="innerVisible" append-to-body>
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
        <el-table-column prop="DepartmentName" label="使用单位" width="200"></el-table-column>
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
import {GetWarnName, GetWarnType, warnTypeList} from '../utils/base';
import moment from 'moment';
import Highcharts from 'highcharts/highstock';
import Highchartsmore from 'highcharts/highcharts-more';
import HighchartsDrilldown from 'highcharts/modules/drilldown';
import Highcharts3D from 'highcharts/highcharts-3d';
Highchartsmore(Highcharts);
HighchartsDrilldown(Highcharts);
Highcharts3D(Highcharts);

export default {
  name: 'warn-type-stat',
  data () {
    return {
      searchParam: {
        From: '',
        To: '',
        DepartmentID: ''
      },
      options: {
        chart: {
          type: 'pie'
          // options3d: {
          //   enabled: true,
          //   alpha: 45,
          //   beta: 0
          // }
        },
        title: {
          text: null
        },
        colors: ['#7cb5ec', '#434348', '#90ed7d', '#f7a35c', '#8085e9',
          '#f15c80', '#e4d354', '#8085e8', '#8d4653', '#91e8e1'],
        tooltip: {
          pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
        },
        credits: {
          enabled: false // 不显示LOGO
        },
        plotOptions: {
          pie: {
            allowPointSelect: true,
            cursor: 'pointer',
            depth: 35,
            dataLabels: {
              enabled: true,
              format: '{point.name}'
            }
          }
        },
        series: []
      },
      gridData: [],
      dialogTableVisible: false,
      orgOptions: [{
        label: '-----不限------',
        value: 0
      }],
      currentPage: 1,
      totalNum: 0,
      chart: '',
      innerVisible: false,
      logSearchParam: {
        OperatorType: '',
        PageIndex: 0,
        PageSize: 0
      },
      handleDetail: {
        warnType: '',
        bmgId: '',
        code: '',
        alertTime: '',
        dealTime: '',
        result: ''
      },
      colors: ['#7cb5ec', '#434348', '#90ed7d', '#f7a35c', '#8085e9',
        '#f15c80', '#e4d354', '#8085e8', '#8d4653', '#91e8e1'],
      chartData: null
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
      var temp = [];
      data.map(item => {
        temp.push([GetWarnName(item.OperationType), item.Count]);
      });
      this.chartData = temp;
      var series = {
        type: 'pie',
        name: '报警类型统计',
        data: temp,
        events: {
          click: function (e) {
            that.showDetail(e.point.name);
          }
        }
      };
      this.options.series[0] = series;
      this.chart = new Highcharts.Chart('highcharts-container', this.options);
    },
    showDetail (name) {
      // alert(name + GetWarnType(name))
      this.logSearchParam.OperatorType = GetWarnType(name);
      this.searchLog();
      this.dialogTableVisible = true;
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
      if (!this.searchParam.From || !this.searchParam.To) {
        return;
      }
      request.post('/api/cabinetlog/departAlarmTypeStatistics', this.searchParam).then(res => {
        if (res.data.State === 1) {
          this.reDrawChart(res.data.Data);
        }
        console.log(res);
      }).catch(() => {});
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
.pieCharts {
  height: 400px;
  width: 600px;
  margin: 0 auto;
}
.side-table {
}
.side-table tbody tr td{
  width: 0px;
}
.side-table tbody tr td span,
.side-table thead th span
{
  font-size: 10px;
  display:block;
  width:14px;
  line-height:12px;
  height:90px;
}
</style>
