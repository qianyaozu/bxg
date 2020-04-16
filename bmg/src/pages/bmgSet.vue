<template>
  <section>
    <el-row>
      <el-col :span="24" style="float: left;margin: 3px 0">
        <span class="searchlabel">保密柜名称：</span>
        <el-input
          size="mini"
          v-model.trim="searchParam.CabinetName" style="width: 150px">
        </el-input>
        <span class="searchlabel">手动编号：</span>
        <el-input
          size="mini"
          v-model.trim="searchParam.CabinetCode" style="width: 150px">
        </el-input>
        <span class="searchlabel">单位名称：</span>
        <el-input
          size="mini"
          v-model.trim="searchParam.DepartmentName" style="width: 150px">
        </el-input>
        <el-button size="small" @click="search">搜索</el-button>
      </el-col>
      <el-col :span="24">
        <el-button type="success" size="small" @click="addSetInfo">添加</el-button>
        <el-button type="primary" size="small" @click="edit">编辑</el-button>
        <el-button type="danger" size="small" @click="remove">删除</el-button>
      </el-col>
      <el-col :span="24">
        <el-table :data="tableData" style="width: 100%" @selection-change="handleSelectionChange">
          <el-table-column type="selection" width="55"></el-table-column>
          <el-table-column prop="Name" label="保密柜名称"></el-table-column>
          <!--<el-table-column prop="ID" label="保密柜ID"></el-table-column>-->
          <el-table-column prop="DepartmentName" label="使用单位"></el-table-column>
          <el-table-column prop="Code" label="手动管理编号"></el-table-column>
          <el-table-column prop="AndroidMac" label="安卓mac地址"></el-table-column>
          <el-table-column prop="Address" label="存放地址"></el-table-column>
          <el-table-column prop="IP" label="分配IP"></el-table-column>
          <el-table-column label="创建时间">
            <template slot-scope="scope">
              {{getDate(scope.row.CreateTime)}}
            </template>
          </el-table-column>
          <el-table-column label="开门需要申请">
            <template slot-scope="scope">
              {{scope.row.NeedConfirm ? '是' : '否'}}
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
    </el-row>

    <el-dialog width="30%" title="使用单位" :visible.sync="innerAddVisible" append-to-body>
      <div style="height: 400px;overflow-y: auto">
        <el-tree :data="treeData" @node-click="handleNodeClick" :highlight-current="true"></el-tree>
      </div>
      <span slot="footer" class="dialog-footer">
        <el-button @click="innerAddVisible = false" size="mini">关闭</el-button>
      </span>
    </el-dialog>

    <el-dialog :visible.sync="addSetDial" width="700px" class="self-modal" :before-close="handleAddClose">
      <div slot="title">
        <span style="font-size: 10px">[添加]保密柜-基本信息 说明：出于安全考虑，相关人员密码均不存数据库，需各中心保密人员等级</span>
      </div>
      <div style="height: 600px;overflow-y: auto">
        <el-form :model="addSet" :rules="rules" ref="addSetForm">
        <el-form-item label="使用单位：" :label-width="formLabelWidth"  prop="orgName">
          <el-input v-model="addSet.orgName" size="mini" auto-complete="off" style="width: 250px" :readonly="true"></el-input>
          <i class="el-icon-search" style="cursor: pointer" @click="showOrgList"></i>
        </el-form-item>
        <el-form-item label="保密柜名称:" :label-width="formLabelWidth" prop="Name">
          <el-input v-model="addSet.Name" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
        <el-form-item label="手动管理编号:" :label-width="formLabelWidth" prop="Code">
          <el-input v-model="addSet.Code" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
        <el-form-item label="存放地址:" :label-width="formLabelWidth" prop="Address">
          <el-input v-model="addSet.Address" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
        <el-form-item label="安卓mac地址:" :label-width="formLabelWidth" prop="AndroidMac">
          <el-input v-model="addSet.AndroidMac" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
        <el-form-item label="安卓版本:" :label-width="formLabelWidth" prop="AndroidVersion">
          <el-input v-model="addSet.AndroidVersion" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
        <el-form-item label="分配IP:" :label-width="formLabelWidth" prop="IP">
          <el-input v-model="addSet.IP" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
          <div class="line"></div>
        <el-form-item label="负责人1:" :label-width="formLabelWidth">
          <el-input v-model="addSet.FirstContact" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
        <el-form-item label="电话1:" :label-width="formLabelWidth" prop="FirstContactPhone">
          <el-input v-model="addSet.FirstContactPhone" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
        <el-form-item label="负责人1[密码]:" :label-width="formLabelWidth" prop="FirstContactPassword">
          <el-input v-model="addSet.FirstContactPassword" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
          <div class="line"></div>
        <el-form-item label="负责人2:" :label-width="formLabelWidth">
          <el-input v-model="addSet.SecondContact" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
        <el-form-item label="电话2:" :label-width="formLabelWidth" prop="SecondContactPhone">
          <el-input v-model="addSet.SecondContactPhone" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>

        <el-form-item label="负责人2[密码]:" :label-width="formLabelWidth" prop="SecondContactPassword">
          <el-input v-model="addSet.SecondContactPassword" size="mini" auto-complete="off" style="width: 250px"></el-input>
        </el-form-item>
          <div class="line"></div>
        <el-form-item label="开门需要确认：" :label-width="formLabelWidth" prop="NeedConfirm">
          <el-radio-group v-model="addSet.NeedConfirm">
            <el-radio :label="true">需要</el-radio>
            <el-radio :label="false" >不需要</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="备注：" :label-width="formLabelWidth">
          <el-input type="textarea" :rows="4" v-model="addSet.Remark" style="width: 250px">
          </el-input>
        </el-form-item>
      </el-form>
      </div>
      <div slot="footer" class="dialog-footer">
        <el-button @click="addSetDial = false" size="small">取 消</el-button>
        <el-button type="primary" size="small" @click="submitAddSet">确 定</el-button>
      </div>
    </el-dialog>

    <el-dialog :visible.sync="editSetDial" width="700px"  class="self-modal" :before-close="handleEditClose">
      <div slot="title">
        <span style="font-size: 10px">[添加]保密柜-基本信息 说明：出于安全考虑，相关人员密码均不存数据库，需各中心保密人员等级</span>
      </div>
      <div style="height: 600px;overflow-y: auto">
        <el-form :model="editSet" :rules="rules" ref="editSetForm">
          <el-form-item label="使用单位：" :label-width="formLabelWidth"  prop="orgName">
            <el-input v-model="editSet.orgName" size="mini" auto-complete="off" style="width: 250px" :readonly="true"></el-input>
            <i class="el-icon-search" style="cursor: pointer" @click="showOrgList"></i>
          </el-form-item>
          <el-form-item label="保密柜名称:" :label-width="formLabelWidth" prop="Name">
            <el-input v-model="editSet.Name" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <el-form-item label="手动管理编号:" :label-width="formLabelWidth" prop="Code">
            <el-input v-model="editSet.Code" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <el-form-item label="存放地址:" :label-width="formLabelWidth" prop="Address">
            <el-input v-model="editSet.Address" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <el-form-item label="安卓mac地址:" :label-width="formLabelWidth" prop="AndroidMac">
            <el-input v-model="editSet.AndroidMac" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <el-form-item label="安卓版本:" :label-width="formLabelWidth" prop="AndroidVersion">
            <el-input v-model="editSet.AndroidVersion" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <el-form-item label="分配IP:" :label-width="formLabelWidth" prop="IP">
            <el-input v-model="editSet.IP" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <div class="line"></div>
          <el-form-item label="负责人1:" :label-width="formLabelWidth">
            <el-input v-model="editSet.FirstContact" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <el-form-item label="电话1:" :label-width="formLabelWidth" prop="FirstContactPhone">
            <el-input v-model="editSet.FirstContactPhone" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <el-form-item label="负责人1[密码]:" :label-width="formLabelWidth" prop="FirstContactPassword">
            <el-input v-model="editSet.FirstContactPassword" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <div class="line"></div>
          <el-form-item label="负责人2:" :label-width="formLabelWidth">
            <el-input v-model="editSet.SecondContact" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <el-form-item label="电话2:" :label-width="formLabelWidth" prop="SecondContactPhone">
            <el-input v-model="editSet.SecondContactPhone" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>

          <el-form-item label="负责人2[密码]:" :label-width="formLabelWidth" prop="SecondContactPassword">
            <el-input v-model="editSet.SecondContactPassword" size="mini" auto-complete="off" style="width: 250px"></el-input>
          </el-form-item>
          <div class="line"></div>
          <el-form-item label="开门需要确认：" :label-width="formLabelWidth" prop="NeedConfirm">
            <el-radio-group v-model="editSet.NeedConfirm">
              <el-radio :label="true">需要</el-radio>
              <el-radio :label="false" >不需要</el-radio>
            </el-radio-group>
          </el-form-item>
          <el-form-item label="备注：" :label-width="formLabelWidth">
            <el-input type="textarea" :rows="4" v-model="editSet.Remark" style="width: 250px">
            </el-input>
          </el-form-item>
        </el-form>
      </div>
      <div slot="footer" class="dialog-footer">
        <el-button @click="editSetDial = false" size="small">取 消</el-button>
        <el-button type="primary" size="small" @click="submitEditSet">确 定</el-button>
      </div>
    </el-dialog>

  </section>
</template>

<script>
import request from '../utils/request';
import moment from 'moment';
export default {
  name: 'bmg-set',
  data () {
    return {
      searchParam: {
        CabinetName: '',
        CabinetCode: '',
        DepartmentName: '',
        PageIndex: 0,
        PageSize: 0
      },
      pageSize: 20,
      totalNum: 0,
      addSet: {
        ID: 0,
        orgName: '',
        DepartmentID: '',
        Name: '',
        Code: '',
        Address: '',
        IP: '',
        FirstContact: '',
        FirstContactPhone: '',
        FirstContactPassword: '',
        SecondContact: '',
        SecondContactPhone: '',
        SecondContactPassword: '',
        NeedConfirm: true,
        Remark: '',
        AndroidMac: '',
        AndroidVersion: ''
      },
      editSet: {
        ID: '',
        orgName: '',
        DepartmentID: '',
        Name: '',
        Code: '',
        Address: '',
        IP: '',
        FirstContact: '',
        FirstContactPhone: '',
        FirstContactPassword: '',
        SecondContact: '',
        SecondContactPhone: '',
        SecondContactPassword: '',
        NeedConfirm: true,
        Remark: '',
        AndroidMac: '',
        AndroidVersion: ''
      },
      rules: {
        orgName: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        AndroidMac: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        AndroidVersion: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        Name: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        ID: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        IP: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        Code: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        Address: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        FirstContact: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        FirstContactPhone: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        FirstContactPassword: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        SecondContact: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        SecondContactPhone: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        SecondContactPassword: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ],
        NeedConfirm: [
          { required: true, message: '必填字段', trigger: 'blur' }
        ]
      },
      addSetDial: false,
      editSetDial: false,
      tableData: [],
      currentPage: 1,
      formLabelWidth: '250px',
      multipleSelection: [],
      treeData: [],
      innerAddVisible: false,
      isAdd: true
    };
  },
  mounted () {
    this.search();
  },
  methods: {
    getDate (time) {
      return moment(time).format('YYYY-MM-DD HH:mm:ss');
    },
    search () {
      request.post('/api/cabinet/view', this.searchParam).then(res => {
        if (res.data.State === 1) {
          this.tableData = res.data.Data.Items;
          this.currentPage = res.data.Data.CurrentPage;
          this.totalNum = res.data.Data.TotalItems;
          console.log(res);
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
    handleSelectionChange (val) {
      this.multipleSelection = val;
    },
    addSetInfo () {
      this.addSetDial = true;
      this.isAdd = true;
    },
    submitAddSet () {
      this.$refs.addSetForm.validate((valid) => {
        if (valid) {
          request.post('/api/cabinet/add', this.addSet).then(res => {
            console.log(res);
            if (res.data.State === 1) {
              this.$message({
                message: '添加成功',
                type: 'success'
              });
              this.addSetDial = false;
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
    submitEditSet () {
      this.$refs.editSetForm.validate((valid) => {
        if (valid) {
          request.post('/api/cabinet/edit', this.editSet).then(res => {
            if (res.data.State === 1) {
              this.$message({
                message: '修改成功',
                type: 'success'
              });
              this.editSetDial = false;
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
    handleAddClose () {
      this.addSetDial = false;
      this.$refs.addSetForm.resetFields();
    },
    handleEditClose () {
      this.editSetDial = false;
      this.$refs.editSetForm.resetFields();
    },
    edit () {
      this.isAdd = false;
      if (this.multipleSelection.length === 0) {
        this.$message({
          message: '请选择一条记录',
          type: 'warning'
        });
        return;
      }
      if (this.multipleSelection.length > 1) {
        this.$message({
          message: '不能同时选择多条记录',
          type: 'warning'
        });
        return;
      }
      var temp = this.tableData.filter(item => {
        return item.ID === this.multipleSelection[0].ID;
      })[0];

      this.editSet.orgName = temp.DepartmentName;

      this.editSet.ID = temp.ID;
      this.editSet.DepartmentID = temp.DepartmentID;
      this.editSet.Name = temp.Name;
      this.editSet.Code = temp.Code;
      this.editSet.Address = temp.Address;
      this.editSet.IP = temp.IP;
      this.editSet.FirstContact = temp.FirstContact;
      this.editSet.FirstContactPhone = temp.FirstContactPhone;
      this.editSet.FirstContactPassword = temp.FirstContactPassword;
      this.editSet.SecondContact = temp.SecondContact;
      this.editSet.SecondContactPhone = temp.SecondContactPhone;
      this.editSet.SecondContactPassword = temp.SecondContactPassword;

      this.editSet.NeedConfirm = temp.NeedConfirm;
      this.editSet.Remark = temp.Remark;
      this.editSet.AndroidMac = temp.AndroidMac;
      this.editSet.AndroidVersion = temp.AndroidVersion;

      this.editSetDial = true;
    },
    getSelectId () {
      var selectId = this.multipleSelection[0].ID;
      var temp = this.tableData.filter(item => {
        return item.ID === selectId;
      })[0];
      return temp.ID;
    },
    remove () {
      if (this.multipleSelection.length === 0) {
        this.$message({
          message: '请选择一条记录',
          type: 'warning'
        });
        return;
      }
      if (this.multipleSelection.length > 1) {
        this.$message({
          message: '不能同时选择多条记录',
          type: 'warning'
        });
        return;
      }
      this.$confirm(`确认要删除选中的数据 ?, 是否继续?`, '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
        center: true
      }).then(() => {
        request.post(`/api/cabinet/delete?cabinetID=${this.getSelectId()}`).then(res => {
          if (res.data.State === 1) {
            this.$message({
              type: 'success',
              message: '删除成功!'
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
      console.log(data);
      if (this.isAdd) {
        this.addSet.orgName = data.label;
        this.addSet.DepartmentID = data.ID;
      } else {
        this.editSet.orgName = data.label;
        this.editSet.DepartmentID = data.ID;
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
