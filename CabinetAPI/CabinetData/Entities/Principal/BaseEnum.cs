using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CabinetData.Entities.Principal
{
    /// <summary>
    /// 保险柜执行日志类型枚举
    /// </summary>
    public enum OperatorTypeEnum
    {
        正常开门 = 1,
        密码错误 = 2,
        正常关门 = 3,
        非工作时间开门 = 4,
        非工作时间关门 = 5,
        外部电源断开 = 6,
        备份电源电压低 = 7,
        未按规定关门 = 8,
        强烈震动 = 9,
        网络断开 = 10,
        修改密码 = 11,
        设置参数 = 12,
        上线 = 13,
        下线 = 14,
        请求语音 = 15,
        结束语音 = 16,
        心跳 = 17,
        接受语音 = 18,
        拒绝语音 = 19,
        允许开门 = 20,
        拒绝开门 = 21
    }

    /// <summary>
    /// 角色枚举
    /// </summary>
    public enum RoleEnum
    {
        超级管理员 = 1,
        系统管理员 = 2,
        安全管理员 = 3,
        安全审计员 = 4
    }

    public enum ModuleEnum
    {
        首页查询=0,
        保险柜分布=1,
        报警类型统计=2,
        单位报警统计=3,
        每月报警统计=4,
        监控日志管理=5,
        用户中心管理 = 6,
        使用部门管理 = 7,
        保险柜管理 = 8,
        系统日志 = 9,
        保险柜请求 = 10,
        权限管理=11
    }

}
