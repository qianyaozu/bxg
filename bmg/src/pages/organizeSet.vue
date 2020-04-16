<template>
  <section>
    <el-row :gutter="20">
      <el-col :span="4">
        <el-tree :data="treeData" @node-click="handleNodeClick" :highlight-current="true" :expand-on-click-node="false" :default-expand-all="true"></el-tree>
      </el-col>
      <el-col :span="20" style=""  v-show="isShowRight">
        <el-col :span="24">
          <span class="searchlabel">部门名称：</span>
          <el-input size="mini" v-model="searchParam.DepartmentName" style="width: 150px"></el-input>

          <el-button size="small" @click="search">搜索</el-button>
        </el-col>
        <el-col :span="24">
          <el-button type="success" size="small" @click="add">添加</el-button>
          <el-button type="primary" size="small" @click="edit">编辑</el-button>
          <el-button type="danger" size="small" @click="remove">删除</el-button>
        </el-col>
        <el-col :span="24">
          <el-table :data="tableData" style="width: 100%" @selection-change="handleSelectionChange">
            <el-table-column type="selection" width="55"></el-table-column>
            <el-table-column prop="SortID" label="排序"></el-table-column>
            <el-table-column prop="Name" label="名称"></el-table-column>
            <el-table-column prop="ParentName" label="根组织"></el-table-column>
            <el-table-column prop="Remark" label="描述"></el-table-column>
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
      </el-col>
    </el-row>

    <el-dialog title="添加部门"  width="700px"  :visible.sync="addDialogFormVisible">
    <el-form :model="addOrg" :rules="rules" ref="addOrgForm" label-width="100px">
      <el-form-item label="上级单位:">
        <el-input v-model="rootOrg" size="mini" :disabled="true" style="width: 350px"></el-input>
      </el-form-item>
      <el-form-item label="名称:" prop="Name">
        <el-input v-model="addOrg.Name" size="mini" style="width: 350px"></el-input>
      </el-form-item>
      <el-form-item label="排序(可调):" prop="SortID">
        <el-input v-model="addOrg.SortID" size="mini" style="width: 350px"></el-input>
      </el-form-item>
      <el-form-item label="单位详细地址:">
        <el-input v-model="addOrg.Address" size="mini" style="width: 350px"></el-input>
      </el-form-item>
      <el-form-item label="描述:">
        <el-input v-model="addOrg.Remark" type="textarea" :rows="3" style="width: 350px"></el-input>
      </el-form-item>
    </el-form>
    <div slot="footer" class="dialog-footer">
      <el-button @click="resetForm()" size="small">取 消</el-button>
      <el-button type="primary" @click="submitForm()" size="small">确 定</el-button>
    </div>
  </el-dialog>

    <el-dialog title="修改部门"  width="700px"  :visible.sync="editDialogFormVisible">
      <el-form :model="editOrg" :rules="rules" ref="editOrgForm" label-width="100px">
        <el-form-item label="上级单位:">
          <el-input v-model="rootOrg" size="mini" :disabled="true" style="width: 350px"></el-input>
        </el-form-item>
        <el-form-item label="名称:" prop="Name">
          <el-input v-model="editOrg.Name" size="mini" style="width: 350px"></el-input>
        </el-form-item>
        <el-form-item label="排序(可调):" prop="SortID">
          <el-input v-model="editOrg.SortID" size="mini" style="width: 350px"></el-input>
        </el-form-item>
        <el-form-item label="单位详细地址:">
          <el-input v-model="editOrg.Address" size="mini" style="width: 350px"></el-input>
        </el-form-item>
        <el-form-item label="描述:">
          <el-input v-model="editOrg.Remark" type="textarea" :rows="3" style="width: 350px"></el-input>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click="resetEditForm()" size="small">取 消</el-button>
        <el-button type="primary" @click="submitEditForm()" size="small">确 定</el-button>
      </div>
    </el-dialog>
  </section>
</template>

<script>
import request from '../utils/request';
export default {
  name: 'organize-set',
  data () {
    return {
      treeData: [],
      searchParam: {
        DepartmentName: '',
        ParentID: '',
        PageIndex: 0,
        PageSize: 0
      },
      pageSize: 20,
      totalNum: 0,
      addOrg: {
        Name: '',
        SortID: 1,
        Address: '',
        Remark: '',
        ParentID: this.parentId
      },
      editOrg: {
        ID: '',
        Name: '',
        SortID: 1,
        Address: '',
        Remark: '',
        ParentID: this.parentId
      },
      addDialogFormVisible: false,
      editDialogFormVisible: false,
      rules: {
        Name: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        SortID: [
          { required: true, message: '必填字段', trigger: 'change' }
        ]
      },
      formLabelWidth: '120px',
      isShowRight: false,
      tableData: [],
      multipleSelection: [],
      rootOrg: '',
      parentId: '',
      currentPage: 1
    };
  },
  mounted () {
    this.loadTreeData();
  },
  methods: {
    loadTreeData () {
      request.get('/api/department/tree').then(res => {
        // console.log(res.data.Data)
        if (res.data.State === 1) {
          this.treeData = [];
          this.treeData.push(res.data.Data);
        }
      }).catch(err => {
        console.log(err);
      });
    },
    handleNodeClick (data) {
      this.rootOrg = data.label;
      this.searchParam.ParentID = data.ID;
      this.addOrg.ParentID = data.ID;
      this.editOrg.ParentID = data.ID;
      this.parentId = data.ID;
      this.isShowRight = true;
      this.search();
    },
    add () {
      this.addDialogFormVisible = true;
    },
    submitForm () {
      this.$refs.addOrgForm.validate((valid) => {
        if (valid) {
          // console.log(this.addOrg)
          request.post('/api/department/add', this.addOrg).then(res => {
            if (res.data.State === 1) {
              this.$message({
                type: 'success',
                message: '添加成功!'
              });
              // this.addDialogFormVisible = false
              // this.loadTreeData()
              // this.isShowRight = false
              setTimeout(() => {
                window.location.reload();
              }, 1000);
            }
            console.log(res);
          }).catch(err => {
            console.log(err);
          });
        } else {
          return false;
        }
      });
    },
    resetForm () {
      this.$refs.addOrgForm.resetFields();
      this.addDialogFormVisible = false;
    },
    submitEditForm () {
      this.$refs.editOrgForm.validate((valid) => {
        if (valid) {
          request.post('/api/department/edit', this.editOrg).then(res => {
            // console.log(res)
            if (res.data.State === 1) {
              this.$message({
                type: 'success',
                message: '修改信息成功!'
              });
              // this.editDialogFormVisible = false
              // this.loadTreeData()
              setTimeout(() => {
                window.location.reload();
              }, 1000);
            }
          }).catch(err => {
            console.log(err);
          });
        } else {
          return false;
        }
      });
    },
    resetEditForm () {
      this.$refs.editOrgForm.resetFields();
      this.editDialogFormVisible = false;
    },
    handleSelectionChange (val) {
      this.multipleSelection = val;
    },
    getSelectId () {
      var selectId = this.multipleSelection[0].ID;
      var temp = this.tableData.filter(item => {
        return item.ID === selectId;
      })[0];
      return temp.ID;
    },
    edit () {
      if (this.multipleSelection.length === 0) {
        this.$message({
          message: '请选择一个部门',
          type: 'warning'
        });
        return;
      }
      if (this.multipleSelection.length > 1) {
        this.$message({
          message: '不能同时选择多个部门',
          type: 'warning'
        });
        return;
      }
      var temp = this.tableData.filter(item => {
        return item.ID === this.multipleSelection[0].ID;
      })[0];
      console.log(temp);
      this.editOrg.Address = temp.Address;
      this.editOrg.Remark = temp.Remark;
      this.editOrg.SortID = temp.SortID;
      this.editOrg.ID = temp.ID;
      this.editOrg.Name = temp.Name;
      this.editDialogFormVisible = true;
    },
    remove () {
      if (this.multipleSelection.length === 0) {
        this.$message({
          message: '请选择一个部门',
          type: 'warning'
        });
        return;
      }
      if (this.multipleSelection.length > 1) {
        this.$message({
          message: '不能同时选择多个部门',
          type: 'warning'
        });
        return;
      }
      var deleteName = this.multipleSelection[0].Name;
      this.$confirm(`确认要删除该组织 ${deleteName}, 是否继续?`, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
        center: true
      }).then(() => {
        request.post(`/api/department/delete?departID=${this.getSelectId()}`).then(res => {
          if (res.data.State === 1) {
            this.$message({
              type: 'success',
              message: '删除成功!'
            });
            // this.loadTreeData()
            // this.isShowRight = false
            setTimeout(() => {
              window.location.reload();
            }, 1000);
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
    search () {
      // console.log(this.searchParam)
      request.post('/api/department/view', this.searchParam).then(res => {
        console.log(res);
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
.searchlabel {
  font-size: 10px;
}
</style>
