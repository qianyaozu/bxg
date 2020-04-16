export const warnTypeList = [{
  label: '报警类型-全部',
  value: ''
}, {
  label: '正常开门',
  value: '1'
}, {
  label: '密码错误',
  value: '2'
}, {
  label: '正常',
  value: '3'
}, {
  label: '非工作时间开门',
  value: '4'
}, {
  label: '非工作时间关门',
  value: '5'
}, {
  label: '外部电源断开',
  value: '6'
}, {
  label: '备份电源电压低',
  value: '7'
}, {
  label: '未按规定关门',
  value: '8'
}, {
  label: '强烈震动',
  value: '9'
}, {
  label: '网络断开',
  value: '10'
}, {
  label: '修改密码',
  value: '11'
}, {
  label: '设置参数',
  value: '12'
}, {
  label: '上线',
  value: '13'
}, {
  label: '下线',
  value: '14'
}, {
  label: '请求语音',
  value: '15'
}, {
  label: '结束语音',
  value: '16'
}, {
  label: '接受语音',
  value: '18'
}, {
  label: '拒绝语音',
  value: '19'
}, {
  label: '允许开门',
  value: '20'
}, {
  label: '拒绝开门',
  value: '21'
}, {
  label: '申请开门',
  value: '24'
}
];

/**
 * @return {string}
 */
export function GetWarnName (value) {
  if (value) {
    if (value === 0) {
      value = '';
    }
    return warnTypeList.filter(item => {
      return item.value === value.toString();
    })[0].label;
  } else {
    return '';
  }
}

export function GetWarnType (name) {
  return warnTypeList.filter(item => {
    return item.label === name;
  })[0].value;
}
// 密码正则表达式
export const PassWordRegex = /^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{8,20}$/;

export function diff (obj1, obj2) {
  var o1 = obj1 instanceof Object;
  var o2 = obj2 instanceof Object;

  if (!o1 || !o2) {
    return obj1 === obj2;
  }
  if (Object.keys(obj1).length !== Object.keys(obj2).length) {
    return false;
  }

  for (var attr in obj1) {
    var t1 = obj1[attr] instanceof Object;
    var t2 = obj2[attr] instanceof Object;
    if (t1 && t2) {
      return this.diff(obj1[attr], obj2[attr]);
    } else if (obj1[attr] !== obj2[attr]) {
      return false;
    }
  }
  return true;
}
