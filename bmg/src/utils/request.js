import axios from 'axios';
import router from '../router/index';

const request = axios.create({
  baseURL: '',
  timeout: 60000
});

request.interceptors.request.use(config => {
  return config;
}, error => {
  console.log(error);
  Promise.reject(error);
});

request.interceptors.response.use(response => {
  if (response.data.State === 2) {
    router.replace({
      path: 'login'
    });
  }
  return response;
}, error => {
  return Promise.reject(error);
});

export default request;
