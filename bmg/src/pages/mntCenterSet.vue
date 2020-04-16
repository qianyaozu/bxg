<template>
  <section>
    <el-row>
      <el-col :span="24">
        <span class="searchlabel">登录账号：</span>
        <el-input
          size="mini"
          v-model.trim="searchParam.UserName" style="width: 150px">
        </el-input>
        <el-button size="small" @click="search">搜索</el-button>
      </el-col>
      <!--按钮-->
      <el-col :span="24">
        <el-button type="success" size="small" @click="add">添加</el-button>
        <el-button type="primary" size="small" @click="edit">编辑</el-button>
        <el-button type="danger" size="small" @click="remove">删除</el-button>
        <el-button type="primary" size="small" @click="resetPwd">重置密码</el-button>
        <el-button type="primary" size="small" @click="resetStatus">更新账户状态</el-button>
      </el-col>
      <!--表格-->
      <el-col :span="24">
        <el-table :data="tableData" style="width: 100%" @selection-change="handleSelectionChange">
          <el-table-column type="selection" width="55"></el-table-column>
          <el-table-column prop="Name" label="登录名称"></el-table-column>
          <el-table-column prop="RealName" label="真实名字"></el-table-column>
          <el-table-column prop="DepartmentName" label="所在机构"></el-table-column>
          <el-table-column prop="Phone" label="电话"></el-table-column>
          <el-table-column label="账户状态">
            <template slot-scope="scope">
              {{getStatus(scope.row.Status)}}
            </template>
          </el-table-column>
          <el-table-column prop="CreateTime" label="创建时间">
            <template slot-scope="scope">
              {{getDate(scope.row.CreateTime)}}
            </template>
          </el-table-column>
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

      <el-dialog width="30%" title="选择单位" :visible.sync="innerAddVisible" append-to-body>
        <div style="height: 400px;overflow-y: auto">
          <el-tree :data="treeData" @node-click="handleNodeClick" :highlight-current="true"></el-tree>
        </div>
        <span slot="footer" class="dialog-footer">
        <el-button @click="innerAddVisible = false" size="mini">关闭</el-button>
      </span>
      </el-dialog>

      <!--模态框-->
      <el-dialog title="添加账号"  width="700px"  :visible.sync="addDialogVisible" :before-close="resetAddForm">
        <el-form :model="addUser" :rules="rules" ref="addUserForm" label-width="160px">
          <el-form-item label="下级分中心名称:" prop="DepartmentName">
            <el-input v-model="addUser.DepartmentName" :readonly="true" size="mini" style="width: 350px" placeholder="不可重复"></el-input>
            <i class="el-icon-search" style="cursor: pointer" @click="showOrgList"></i>
          </el-form-item>
          <el-form-item label="账号:" prop="Name">
            <el-input v-model="addUser.Name" size="mini" style="width: 350px" placeholder="不可重复"></el-input>
          </el-form-item>
          <el-form-item label="真实名字:" prop="RealName">
            <el-input v-model="addUser.RealName" size="mini" style="width: 350px"></el-input>
          </el-form-item>
          <el-form-item label="登录密码:" prop="Password">
            <el-input v-model="addUser.Password" size="mini" style="width: 350px" placeholder="8 - 20字符"></el-input>
          </el-form-item>
          <el-form-item label="电话:">
            <el-input v-model="addUser.Phone" size="mini" style="width: 350px"></el-input>
          </el-form-item>
          <el-form-item label="用户邮箱:" prop="email">
            <el-input v-model="addUser.Email" size="mini" style="width: 350px"></el-input>
          </el-form-item>
          <el-form-item label="用户状态:">
            <el-select size="mini" v-model="addUser.Status" placeholder="请选择"  style="width: 350px">
              <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value">
              </el-option>
            </el-select>
          </el-form-item>
        </el-form>
        <div slot="footer" class="dialog-footer">
          <el-button @click="resetAddForm" size="small">取 消</el-button>
          <el-button type="primary" @click="submitAddForm()" size="small">确 定</el-button>
        </div>
      </el-dialog>

      <el-dialog title="修改用户"  width="700px"  :visible.sync="editDialogVisible" :before-close="resetEditForm">
        <el-form :model="editUser" :rules="rules" ref="editUserForm" label-width="160px">
          <el-form-item label="下级分中心名称:" prop="DepartmentName">
            <el-input v-model="editUser.DepartmentName" :readonly="true" size="mini" style="width: 350px" placeholder="不可重复"></el-input>
            <i class="el-icon-search" style="cursor: pointer" @click="showOrgList"></i>
          </el-form-item>
          <el-form-item label="账号:" prop="Name">
            <el-input v-model="editUser.Name" size="mini" style="width: 350px" placeholder="不可重复"></el-input>
          </el-form-item>
          <el-form-item label="真实名字:" prop="RealName">
            <el-input v-model="editUser.RealName" size="mini" style="width: 350px"></el-input>
          </el-form-item>
          <el-form-item label="登录密码:" prop="Password">
            <el-input v-model="editUser.Password" size="mini" style="width: 350px" placeholder="8 - 20字符"></el-input>
          </el-form-item>
          <el-form-item label="电话:">
            <el-input v-model="editUser.Phone" size="mini" style="width: 350px"></el-input>
          </el-form-item>
          <el-form-item label="用户邮箱:" prop="email">
            <el-input v-model="editUser.Email" size="mini" style="width: 350px"></el-input>
          </el-form-item>
          <el-form-item label="用户状态:">
            <el-select size="mini" v-model="editUser.Status" placeholder="请选择"  style="width: 350px">
              <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value">
              </el-option>
            </el-select>
          </el-form-item>
        </el-form>
        <div slot="footer" class="dialog-footer">
          <el-button @click="resetEditForm" size="small">取 消</el-button>
          <el-button type="primary" @click="submitEditForm()" size="small">确 定</el-button>
        </div>
      </el-dialog>
    </el-row>
  </section>
</template>

<script>
import request from '../utils/request';
import moment from 'moment';
import {PassWordRegex} from '../utils/base';

export default {
  name: 'mnt-center-set',
  data () {
    var CheckPassword = (rule, value, callback) => {
      if (!PassWordRegex.test(value)) {
        callback(new Error('密码必须包含英文和数字，长度8-20位'));
      } else {
        callback();
      }
    };
    return {
      options: [{
        label: '禁用',
        value: 0
      }, {
        label: '启用',
        value: 1
      }],
      searchParam: {
        UserName: '',
        DepartmentID: '',
        RoleID: '',
        PageIndex: 0,
        PageSize: 0
      },
      tableData: [],
      multipleSelection: [],
      addUser: {
        Name: '',
        RealName: '',
        Password: '',
        Phone: '',
        Email: '',
        Status: 1, // 1 启用， 0 禁用
        DepartmentID: '',
        DepartmentName: '',
        RoleID: 2
      },
      editUser: {
        ID: '',
        Name: '',
        RealName: '',
        Password: '',
        Phone: '',
        Email: '',
        Status: 1,
        DepartmentID: '',
        DepartmentName: '',
        RoleID: 2
      },
      addDialogVisible: false,
      editDialogVisible: false,
      innerAddVisible: false,
      treeData: [],
      isAdd: true,
      currentPage: 1,
      pageSize: 20,
      totalNum: 10,
      rules: {
        Name: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        DepartmentName: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        RealName: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        realName: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        Password: [
          { required: true, message: '必填字段', trigger: 'blur' },
          { min: 8, max: 20, message: '长度在 8 到 20 个字符', trigger: 'blur' },
          { validator: CheckPassword, trigger: 'blur' }
        ],
        Email: [
          { type: 'email', message: '请输入正确的邮箱地址', trigger: 'blur,change' }
        ]
      }
    };
  },
  mounted () {
    this.search();
  },
  methods: {
    getDate (time) {
      return moment(time).format('YYYY-MM-DD HH:mm:ss');
    },
    getStatus (value) {
      return this.options.filter(item => {
        return item.value === value;
      })[0].label;
    },
    search () {
      request.post('/api/user/view', this.searchParam).then(res => {
        if (res.data.State === 1) {
          this.tableData = res.data.Data.Items;
          this.currentPage = res.data.Data.CurrentPage;
          this.totalNum = res.data.Data.TotalItems;
        }
        // console.log(res)
      }).catch(() => {});
    },
    add () {
      this.isAdd = true;
      this.addDialogVisible = true;
    },
    submitAddForm () {
      this.$refs.addUserForm.validate((valid) => {
        if (valid) {
          request.post('/api/user/add', this.addUser).then(res => {
            if (res.data.State === 1) {
              this.$message({
                message: '添加成功',
                type: 'success'
              });
              this.addDialogVisible = false;
              this.search();
            } else {
              this.$message({
                message: res.data.Message,
                type: 'error'
              });
            }
          }).catch(() => {
            this.$message({
              message: '添加失败',
              type: 'error'
            });
          });
        } else {
          return false;
        }
      });
    },
    resetAddForm () {
      this.addDialogVisible = false;
      this.$refs.addUserForm.resetFields();
    },
    edit () {
      this.isAdd = false;
      if (this.multipleSelection.length === 0) {
        this.$message({
          message: '请选择一个用户',
          type: 'warning'
        });
        return;
      }
      if (this.multipleSelection.length > 1) {
        this.$message({
          message: '不能同时选择多个用户',
          type: 'warning'
        });
        return;
      }

      var temp = this.tableData.filter(item => {
        return item.ID === this.multipleSelection[0].ID;
      })[0];

      this.editUser.ID = temp.ID;
      this.editUser.DepartmentID = temp.DepartmentID;
      this.editUser.DepartmentName = temp.DepartmentName;
      this.editUser.Name = temp.Name;
      this.editUser.RealName = temp.RealName;
      this.editUser.Password = temp.Password;
      this.editUser.Email = temp.Email;
      this.editUser.Status = temp.Status;
      this.editUser.Phone = temp.Phone;

      this.editDialogVisible = true;
    },
    submitEditForm () {
      this.$refs.editUserForm.validate((valid) => {
        if (valid) {
          request.post('/api/user/edit', this.editUser).then(res => {
            if (res.data.State === 1) {
              this.$message({
                message: '修改成功',
                type: 'success'
              });
              this.editDialogVisible = false;
              this.search();
            } else {
              this.$message({
                message: res.data.Message,
                type: 'error'
              });
            }
          }).catch(() => {
            this.$message({
              message: '修改失败',
              type: 'error'
            });
          });
        } else {
          return false;
        }
      });
    },
    resetEditForm () {
      this.editDialogVisible = false;
      this.$refs.editUserForm.resetFields();
    },
    remove () {
      if (this.multipleSelection.length === 0) {
        this.$message({
          message: '请选择一个用户',
          type: 'warning'
        });
        return;
      }
      if (this.multipleSelection.length > 1) {
        this.$message({
          message: '不能同时选择多个用户',
          type: 'warning'
        });
        return;
      }

      this.$confirm(`确认要删除选中用户?`, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
        center: true
      }).then(() => {
        request.post(`/api/user/delete?UserID=${this.multipleSelection[0].ID}`, this.editUser).then(res => {
          if (res.data.State === 1) {
            this.$message({
              message: '删除成功',
              type: 'success'
            });
            this.search();
          } else {
            this.$message({
              message: res.data.Message,
              type: 'error'
            });
          }
        }).catch(() => {
          this.$message({
            message: '删除失败',
            type: 'error'
          });
        });
      }).catch(() => {
        this.$message({
          type: 'info',
          message: '已取消删除'
        });
      });
    },
    resetPwd () {
      if (this.multipleSelection.length === 0) {
        this.$message({
          message: '请选择一个用户',
          type: 'warning'
        });
        return;
      }

      if (this.multipleSelection.length > 1) {
        this.$message({
          message: '不能同时选择多个用户',
          type: 'warning'
        });
        return;
      }

      this.$confirm(`确认要重置选中用户的密码, 是否继续?`, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
        center: true
      }).then(() => {
        request.post(`/api/user/resetpassword?id=${this.multipleSelection[0].ID}`, this.editUser).then(res => {
          if (res.data.State === 1) {
            this.$message({
              message: '重置密码成功',
              type: 'success'
            });
            this.search();
          } else {
            this.$message({
              message: res.data.Message,
              type: 'error'
            });
          }
        }).catch(() => {
          this.$message({
            message: '重置密码失败',
            type: 'error'
          });
        });
      }).catch(() => {
        this.$message({
          type: 'info',
          message: '已取消'
        });
      });
    },
    resetStatus () {
      if (this.multipleSelection.length === 0) {
        this.$message({
          message: '请选择一个用户',
          type: 'warning'
        });
        return;
      }

      if (this.multipleSelection.length > 1) {
        this.$message({
          message: '不能同时选择多个用户',
          type: 'warning'
        });
        return;
      }

      this.$confirm(`确认要更新选中用户的状态, 是否继续?`, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
        center: true
      }).then(() => {
        request.post(`/api/user/updatestatus?ID=${this.multipleSelection[0].ID}`, this.editUser).then(res => {
          if (res.data.State === 1) {
            this.$message({
              message: '删除成功',
              type: 'success'
            });
            this.search();
          } else {
            this.$message({
              message: res.data.Message,
              type: 'error'
            });
          }
        }).catch(() => {
          this.$message({
            message: '删除失败',
            type: 'error'
          });
        });
      }).catch(() => {
        this.$message({
          type: 'info',
          message: '已取消'
        });
      });
    },
    handleSizeChange (val) {
      this.searchParam.PageSize = val;
    },
    handleCurrentChange (val) {
      this.searchParam.PageIndex = val;
    },
    handleSelectionChange (val) {
      this.multipleSelection = val;
      // console.log(val)
    },
    showOrgList () {
      request.get('/api/department/tree').then(res => {
        if (res.data.State === 1) {
          this.treeData = [];
          this.treeData.push(res.data.Data);
          this.innerAddVisible = true;
        }
      }).catch(() => {
        this.$message({
          type: 'error',
          message: '获取组织数据失败'
        });
      });
    },
    handleNodeClick (data) {
      if (this.isAdd) {
        this.addUser.DepartmentName = data.label;
        this.addUser.DepartmentID = data.ID;
      } else {
        this.editUser.DepartmentName = data.label;
        this.editUser.DepartmentID = data.ID;
      }
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

</style>
