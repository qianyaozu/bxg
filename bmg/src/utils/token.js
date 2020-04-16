import VueCookie from 'vue-cookie';
import { decryptByDES } from './crypto';

export function SetToken (token) {
  var date = new Date();
  date.setDate(date.getSeconds() + 30);
  VueCookie.set('ii', token, { expires: '60m' });
}

export function GetToken () {
  return VueCookie.get('ii');
}

// export function DeleteToken () {
//   VueCookie.delete('token')
// }

export function SetRouter (router) {
  VueCookie.set('defaultroute', router);
}

export function GetRouter () {
  return VueCookie.get('defaultroute');
}

// export function DeleteRouter () {
//   VueCookie.delete('defaultroute')
// }

export function DeleteAllCookie () {
  VueCookie.delete('defaultroute');
  VueCookie.delete('ii');
}

/**
 * @return {null}
 */
export function GetDepartment () {
  var ii = VueCookie.get('ii');
  if (ii) {
    ii = decryptByDES(ii);
    return JSON.parse(ii).DepartmentName;
  } else {
    return null;
  }
}

/**
 * @return {boolean}
 */
export function NeedChangePwd () {
  var ii = VueCookie.get('ii');
  if (ii) {
    ii = decryptByDES(ii);
    return JSON.parse(ii).NeedChangePassword;
  }
  return false;
}
