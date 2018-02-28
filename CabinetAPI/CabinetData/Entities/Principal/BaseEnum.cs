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
        下线 = 14
    }
}
