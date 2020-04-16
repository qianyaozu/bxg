<template>
  <section>
    <el-row>
      <el-col :span="24" style="text-align: center;">
        <span> [</span>
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
        <span>中心] 每月报警统计次数</span>
      </el-col>
      <el-col :span="24">
        <div id="highcharts-container"></div>
      </el-col>
    </el-row>
  </section>
</template>

<script>
import request from '../utils/request';
import Highcharts from 'highcharts/highstock';
import Highchartsmore from 'highcharts/highcharts-more';
import HighchartsDrilldown from 'highcharts/modules/drilldown';
import Highcharts3D from 'highcharts/highcharts-3d';
Highchartsmore(Highcharts);
HighchartsDrilldown(Highcharts);
Highcharts3D(Highcharts);

export default {
  name: 'warn-count-per-month-stat',
  data () {
    return {
      orgOptions: [{
        label: '-----不限------',
        value: 0
      }],
      searchParam: {
        From: '',
        To: '',
        DepartmentID: ''
      },
      options: {
        chart: {
          type: 'line'
        },
        title: {
          text: null
        },
        xAxis: {
          categories: ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月']
        },
        yAxis: {
          title: {
            text: '报警次数'
          }
        },
        plotOptions: {
          line: {
            dataLabels: {
              enabled: true
            },
            enableMouseTracking: false
          }
        },
        credits: {
          enabled: false // 不显示LOGO
        },
        series: [],
        responsive: {
          rules: [{
            condition: {
              maxWidth: 500
            },
            chartOptions: {
              legend: {
                layout: 'horizontal',
                align: 'center',
                verticalAlign: 'bottom'
              }
            }
          }]
        }
      },
      charts: ''
    };
  },
  mounted () {
    this.loadDepartment();
    this.chart = new Highcharts.Chart('highcharts-container', this.options);
  },
  methods: {
    reDrawChart (data) {
      this.options.series = [];
      // var temp = []
      data.map(item => {
        this.options.series.push({
          name: item.Year,
          data: item.Data
        });
      });
      // this.options.series.push({
      //   name: '项目开发',
      //   data: [7.0, 6.9, 9.5, 14.5, 18.4, 21.5, 25.2, 26.5, 23.3, 18.3, 13.9, 9.6]
      //   // data: temp
      // })
      this.chart = new Highcharts.Chart('highcharts-container', this.options);
    },
    getChartData () {
      if (!this.searchParam.From || !this.searchParam.To) {
        return;
      }
      request.post('/api/cabinetlog/departMonthAlarmStatistics', this.searchParam).then(res => {
        if (res.data.State === 1) {
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
    }
  },
  watch: {
    searchParam: {
      handler: function () {
        this.getChartData();
      },
      deep: true
    }
  }
};
</script>

<style scoped>

</style>
