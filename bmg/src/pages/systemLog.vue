<template>
  <section>
    <el-row :gutter="20">
      <el-col :span="24" style="float: left;margin: 3px 0">
        <span class="searchlabel">登录账号：</span>
        <el-input
          size="mini"
          v-model="searchParam.UserName" style="width: 150px">
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
        <el-button size="small" @click="searchLog">搜索</el-button>
      </el-col>

      <el-col :span="24">
        <el-table :data="tableData" style="width: 100%">
          <el-table-column prop="UserName" label="账号" width="180">
          </el-table-column>
          <el-table-column prop="RealName" label="用户" width="180">
          </el-table-column>
          <el-table-column prop="DepartmentName" label="所属机构"></el-table-column>
          <el-table-column prop="CreateTime" label="操作时间">
            <template slot-scope="scope">
              {{getDate(scope.row.CreateTime)}}
            </template>
          </el-table-column>
          <el-table-column prop="Action" label="操作类型"></el-table-column>
          <el-table-column prop="LogContent" label="操作内容"></el-table-column>
          <el-table-column prop="ClientIP" label="访问IP"></el-table-column>
        </el-table>
        <div style="margin: 5px 0 0 0;float:right">
          <el-pagination
            @size-change="handleSizeChange"
            @current-change="handleCurrentChange"
            :current-page="currentPage"
            :page-sizes="[10, 20, 30, 40]"
            :page-size="pageSize"
            layout="total, sizes, prev, pager, next, jumper"
            :total="totalNum">
          </el-pagination>
        </div>
      </el-col>

    </el-row>
  </section>
</template>

<script>
import request from '../utils/request';
import moment from 'moment';
export default {
  name: 'system-log',
  data () {
    return {
      searchParam: {
        UserName: '',
        StartTime: '',
        EndTime: '',
        PageIndex: 0,
        PageSize: 0
      },
      tableData: [],
      currentPage: 1,
      pageSize: 20,
      totalNum: 10
    };
  },
  mounted () {
    this.searchLog();
  },
  methods: {
    getDate (time) {
      return moment(time).format('YYYY-MM-DD HH:mm:ss');
    },
    searchLog () {
      if (this.searchParam.StartTime) {
        this.searchParam.StartTime = moment(this.searchParam.StartTime).format('YYYY-MM-DD HH:mm:ss');
      }
      if (this.searchParam.EndTime) {
        this.searchParam.EndTime = moment(this.searchParam.EndTime).format('YYYY-MM-DD HH:mm:ss');
      }
      request.post('/api/systemlog/view', this.searchParam).then(res => {
        console.log(res);
        if (res.data.State === 1) {
          this.tableData = res.data.Data.Items;
          this.currentPage = res.data.Data.CurrentPage;
          this.totalNum = res.data.Data.TotalItems;
        }
      }).catch((err) => {
        console.log(err);
      });
    },
    handleSizeChange (val) {
      this.searchParam.PageSize = val;
    },
    handleCurrentChange (val) {
      this.searchParam.PageIndex = val;
    }
  },
  watch: {
    'searchParam.PageSize': {
      handler (curVal, oldVal) {
        this.searchLog();
      }
    },
    'searchParam.PageIndex': {
      handler (curVal, oldVal) {
        this.searchLog();
      }
    }
  }
};
</script>

<style scoped>
.searchlabel {
  font-size: 10px;
}
</style>
