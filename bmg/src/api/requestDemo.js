import request from '@/utils/request';

export function get (q) {
  return request({
    url: '',
    method: 'get21',
    params: q
  });
}

export function post (p) {
  return request({
    url: '',
    method: 'post',
    p
  });
}
