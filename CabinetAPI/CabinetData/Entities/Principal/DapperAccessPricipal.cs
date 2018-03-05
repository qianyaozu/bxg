using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using CabinetData.Base;

namespace CabinetData.Entities
{

	public partial class CabinetDap : BaseDap
	{
		public CabinetDap()
		{
		}

		public CabinetDap(IDbConnection connection)
		{
			Connection = connection;
		}

		public CabinetDap(IDbTransaction transaction)
		{
			Transaction = transaction;
			Connection = transaction.Connection;
		}

		public CabinetDap(BaseDap dapProvider)
		{
			Transaction = dapProvider.Transaction;
			Connection = dapProvider.Connection;
		}

		public List<Cabinet> GetTop(int count)
		{
			return Query<Cabinet>(string.Format("SELECT TOP {0} * FROM {1}", count, SqlTableName)).ToList();
		}

		public Cabinet GetByID(Int32 ID)
		{
			return Query<Cabinet>(SqlSelectCommand + " WHERE ID=@ID", new { ID = ID }).FirstOrDefault();
		}

		public void Insert(Cabinet model)
		{
			Execute(SqlInsertCommand, model);
		}

		public void Insert(IEnumerable<Cabinet> models)
		{
			Execute(SqlInsertCommand, models);
		}

		public void Delete(Int32 ID)
		{
			Execute(SqlDeleteCommand, new { ID = ID });
		}

		public void Update(Cabinet model)
		{
			Execute(SqlUpdateCommand, model);
		}

		public void Update(IEnumerable<Cabinet> models)
		{
			Execute(SqlUpdateCommand, models);
		}




		public const string SqlTableName = "Cabinet";
		public const string SqlSelectCommand = "SELECT * FROM " + SqlTableName;
		public const string SqlInsertCommand = "INSERT INTO " + SqlTableName + " (Name , Address , DepartmentID , Code , AndroidMac , IP , IsOnline , LastOnlineTime , Status , NeedConfirm , AndroidVersion , FirstContact , FirstContactPhone , FirstContactPassword , SecondContact , SecondContactPhone , SecondContactPassword , Remark) VALUES (@Name , @Address , @DepartmentID , @Code , @AndroidMac , @IP , @IsOnline , @LastOnlineTime , @Status , @NeedConfirm , @AndroidVersion , @FirstContact , @FirstContactPhone , @FirstContactPassword , @SecondContact , @SecondContactPhone , @SecondContactPassword , @Remark) ";
		public const string SqlUpdateCommand = "UPDATE " + SqlTableName + " SET Name=@Name , Address=@Address , DepartmentID=@DepartmentID , Code=@Code , AndroidMac=@AndroidMac , IP=@IP , IsOnline=@IsOnline , LastOnlineTime=@LastOnlineTime , Status=@Status , NeedConfirm=@NeedConfirm , AndroidVersion=@AndroidVersion , FirstContact=@FirstContact , FirstContactPhone=@FirstContactPhone , FirstContactPassword=@FirstContactPassword , SecondContact=@SecondContact , SecondContactPhone=@SecondContactPhone , SecondContactPassword=@SecondContactPassword , Remark=@Remark WHERE ID=@ID";
		public const string SqlDeleteCommand = "DELETE FROM " + SqlTableName + " WHERE ID=@ID";
		
	}

	public partial class CabinetLogDap : BaseDap
	{
		public CabinetLogDap()
		{
		}

		public CabinetLogDap(IDbConnection connection)
		{
			Connection = connection;
		}

		public CabinetLogDap(IDbTransaction transaction)
		{
			Transaction = transaction;
			Connection = transaction.Connection;
		}

		public CabinetLogDap(BaseDap dapProvider)
		{
			Transaction = dapProvider.Transaction;
			Connection = dapProvider.Connection;
		}

		public List<CabinetLog> GetTop(int count)
		{
			return Query<CabinetLog>(string.Format("SELECT TOP {0} * FROM {1}", count, SqlTableName)).ToList();
		}

		public CabinetLog GetByID(Int32 ID)
		{
			return Query<CabinetLog>(SqlSelectCommand + " WHERE ID=@ID", new { ID = ID }).FirstOrDefault();
		}

		public void Insert(CabinetLog model)
		{
			Execute(SqlInsertCommand, model);
		}

		public void Insert(IEnumerable<CabinetLog> models)
		{
			Execute(SqlInsertCommand, models);
		}

		public void Delete(Int32 ID)
		{
			Execute(SqlDeleteCommand, new { ID = ID });
		}

		public void Update(CabinetLog model)
		{
			Execute(SqlUpdateCommand, model);
		}

		public void Update(IEnumerable<CabinetLog> models)
		{
			Execute(SqlUpdateCommand, models);
		}




		public const string SqlTableName = "CabinetLog";
		public const string SqlSelectCommand = "SELECT * FROM " + SqlTableName;
		public const string SqlInsertCommand = "INSERT INTO " + SqlTableName + " (CabinetID , OperatorName , OperateTime , OperationType , CreateTime , CabinetIP , EventContent , DealPerson , DealTime , DealResult , Remark) VALUES (@CabinetID , @OperatorName , @OperateTime , @OperationType , @CreateTime , @CabinetIP , @EventContent , @DealPerson , @DealTime , @DealResult , @Remark) ";
		public const string SqlUpdateCommand = "UPDATE " + SqlTableName + " SET CabinetID=@CabinetID , OperatorName=@OperatorName , OperateTime=@OperateTime , OperationType=@OperationType , CreateTime=@CreateTime , CabinetIP=@CabinetIP , EventContent=@EventContent , DealPerson=@DealPerson , DealTime=@DealTime , DealResult=@DealResult , Remark=@Remark WHERE ID=@ID";
		public const string SqlDeleteCommand = "DELETE FROM " + SqlTableName + " WHERE ID=@ID";
		
	}

	 

	 

	public partial class Role_ModuleDap : BaseDap
	{
		public Role_ModuleDap()
		{
		}

		public Role_ModuleDap(IDbConnection connection)
		{
			Connection = connection;
		}

		public Role_ModuleDap(IDbTransaction transaction)
		{
			Transaction = transaction;
			Connection = transaction.Connection;
		}

		public Role_ModuleDap(BaseDap dapProvider)
		{
			Transaction = dapProvider.Transaction;
			Connection = dapProvider.Connection;
		}

		public List<Role_Module> GetTop(int count)
		{
			return Query<Role_Module>(string.Format("SELECT TOP {0} * FROM {1}", count, SqlTableName)).ToList();
		}

		public Role_Module GetByID(Int32 ID)
		{
			return Query<Role_Module>(SqlSelectCommand + " WHERE ID=@ID", new { ID = ID }).FirstOrDefault();
		}

		public void Insert(Role_Module model)
		{
			Execute(SqlInsertCommand, model);
		}

		public void Insert(IEnumerable<Role_Module> models)
		{
			Execute(SqlInsertCommand, models);
		}

		public void Delete(Int32 ID)
		{
			Execute(SqlDeleteCommand, new { ID = ID });
		}

		public void Update(Role_Module model)
		{
			Execute(SqlUpdateCommand, model);
		}

		public void Update(IEnumerable<Role_Module> models)
		{
			Execute(SqlUpdateCommand, models);
		}




		public const string SqlTableName = "Role_Module";
		public const string SqlSelectCommand = "SELECT * FROM " + SqlTableName;
		public const string SqlInsertCommand = "INSERT INTO " + SqlTableName + " (RoleID , ModuleID , ModuleName , EnableView , EnableAdd , EnableEdit , EnableDelete) VALUES (@RoleID , @ModuleID , @ModuleName , @EnableView , @EnableAdd , @EnableEdit , @EnableDelete) ";
		public const string SqlUpdateCommand = "UPDATE " + SqlTableName + " SET RoleID=@RoleID , ModuleID=@ModuleID , ModuleName=@ModuleName , EnableView=@EnableView , EnableAdd=@EnableAdd , EnableEdit=@EnableEdit , EnableDelete=@EnableDelete WHERE ID=@ID";
		public const string SqlDeleteCommand = "DELETE FROM " + SqlTableName + " WHERE ID=@ID";
		
	}

	public partial class DepartmentDap : BaseDap
	{
		public DepartmentDap()
		{
		}

		public DepartmentDap(IDbConnection connection)
		{
			Connection = connection;
		}

		public DepartmentDap(IDbTransaction transaction)
		{
			Transaction = transaction;
			Connection = transaction.Connection;
		}

		public DepartmentDap(BaseDap dapProvider)
		{
			Transaction = dapProvider.Transaction;
			Connection = dapProvider.Connection;
		}

		public List<Department> GetTop(int count)
		{
			return Query<Department>(string.Format("SELECT TOP {0} * FROM {1}", count, SqlTableName)).ToList();
		}

		public Department GetByID(Int32 ID)
		{
			return Query<Department>(SqlSelectCommand + " WHERE ID=@ID", new { ID = ID }).FirstOrDefault();
		}

		public void Insert(Department model)
		{
			Execute(SqlInsertCommand, model);
		}

		public void Insert(IEnumerable<Department> models)
		{
			Execute(SqlInsertCommand, models);
		}

		public void Delete(Int32 ID)
		{
			Execute(SqlDeleteCommand, new { ID = ID });
		}

		public void Update(Department model)
		{
			Execute(SqlUpdateCommand, model);
		}

		public void Update(IEnumerable<Department> models)
		{
			Execute(SqlUpdateCommand, models);
		}




		public const string SqlTableName = "Department";
		public const string SqlSelectCommand = "SELECT * FROM " + SqlTableName;
		public const string SqlInsertCommand = "INSERT INTO " + SqlTableName + " (ID , ParentID , Name , SortID , Address , Remark) VALUES (@ID , @ParentID , @Name , @SortID , @Address , @Remark) ";
		public const string SqlUpdateCommand = "UPDATE " + SqlTableName + " SET ID=@ID , ParentID=@ParentID , Name=@Name , SortID=@SortID , Address=@Address , Remark=@Remark WHERE ID=@ID";
		public const string SqlDeleteCommand = "DELETE FROM " + SqlTableName + " WHERE ID=@ID";
		
	}

	public partial class UserInfoDap : BaseDap
	{
		public UserInfoDap()
		{
		}

		public UserInfoDap(IDbConnection connection)
		{
			Connection = connection;
		}

		public UserInfoDap(IDbTransaction transaction)
		{
			Transaction = transaction;
			Connection = transaction.Connection;
		}

		public UserInfoDap(BaseDap dapProvider)
		{
			Transaction = dapProvider.Transaction;
			Connection = dapProvider.Connection;
		}

		public List<UserInfo> GetTop(int count)
		{
			return Query<UserInfo>(string.Format("SELECT TOP {0} * FROM {1}", count, SqlTableName)).ToList();
		}

		public UserInfo GetByID(Int32 ID)
		{
			return Query<UserInfo>(SqlSelectCommand + " WHERE ID=@ID", new { ID = ID }).FirstOrDefault();
		}

		public void Insert(UserInfo model)
		{
			Execute(SqlInsertCommand, model);
		}

		public void Insert(IEnumerable<UserInfo> models)
		{
			Execute(SqlInsertCommand, models);
		}

		public void Delete(Int32 ID)
		{
			Execute(SqlDeleteCommand, new { ID = ID });
		}

		public void Update(UserInfo model)
		{
			Execute(SqlUpdateCommand, model);
		}

		public void Update(IEnumerable<UserInfo> models)
		{
			Execute(SqlUpdateCommand, models);
		}




		public const string SqlTableName = "UserInfo";
		public const string SqlSelectCommand = "SELECT * FROM " + SqlTableName;
		public const string SqlInsertCommand = "INSERT INTO " + SqlTableName + " (Name , Password , DepartmentID , RoleID , LastLoginIP , LastLoginTime , RealName , Phone , Email , CreateTime , Status) VALUES (@Name , @Password , @DepartmentID , @RoleID , @LastLoginIP , @LastLoginTime , @RealName , @Phone , @Email , @CreateTime , @Status) ";
		public const string SqlUpdateCommand = "UPDATE " + SqlTableName + " SET Name=@Name , Password=@Password , DepartmentID=@DepartmentID , RoleID=@RoleID , LastLoginIP=@LastLoginIP , LastLoginTime=@LastLoginTime , RealName=@RealName , Phone=@Phone , Email=@Email , CreateTime=@CreateTime , Status=@Status WHERE ID=@ID";
		public const string SqlDeleteCommand = "DELETE FROM " + SqlTableName + " WHERE ID=@ID";
		
	}

	public partial class SystemLogDap : BaseDap
	{
		public SystemLogDap()
		{
		}

		public SystemLogDap(IDbConnection connection)
		{
			Connection = connection;
		}

		public SystemLogDap(IDbTransaction transaction)
		{
			Transaction = transaction;
			Connection = transaction.Connection;
		}

		public SystemLogDap(BaseDap dapProvider)
		{
			Transaction = dapProvider.Transaction;
			Connection = dapProvider.Connection;
		}

		public List<SystemLog> GetTop(int count)
		{
			return Query<SystemLog>(string.Format("SELECT TOP {0} * FROM {1}", count, SqlTableName)).ToList();
		}

		public SystemLog GetByID(Int32 ID)
		{
			return Query<SystemLog>(SqlSelectCommand + " WHERE ID=@ID", new { ID = ID }).FirstOrDefault();
		}

		public void Insert(SystemLog model)
		{
			Execute(SqlInsertCommand, model);
		}

		public void Insert(IEnumerable<SystemLog> models)
		{
			Execute(SqlInsertCommand, models);
		}

		public void Delete(Int32 ID)
		{
			Execute(SqlDeleteCommand, new { ID = ID });
		}

		public void Update(SystemLog model)
		{
			Execute(SqlUpdateCommand, model);
		}

		public void Update(IEnumerable<SystemLog> models)
		{
			Execute(SqlUpdateCommand, models);
		}




		public const string SqlTableName = "SystemLog";
		public const string SqlSelectCommand = "SELECT * FROM " + SqlTableName;
		public const string SqlInsertCommand = "INSERT INTO " + SqlTableName + " (Action , LogContent , CreateTime , UserID , UserName , RealName , DepartmentName , RoleName , ClientIP) VALUES (@Action , @LogContent , @CreateTime , @UserID , @UserName , @RealName , @DepartmentName , @RoleName , @ClientIP) ";
		public const string SqlUpdateCommand = "UPDATE " + SqlTableName + " SET Action=@Action , LogContent=@LogContent , CreateTime=@CreateTime , UserID=@UserID , UserName=@UserName , RealName=@RealName , DepartmentName=@DepartmentName , RoleName=@RoleName , ClientIP=@ClientIP WHERE ID=@ID";
		public const string SqlDeleteCommand = "DELETE FROM " + SqlTableName + " WHERE ID=@ID";
		
	}

}
