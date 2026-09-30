import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const getComputers = () => api.get('/Computers');
export const getSoftware = () => api.get('/Software');
export const getDeployments = () => api.get('/Deployments');
export const approveAgent = (id: string) => api.post(`/Computers/approve-agent/${id}`);

export default api;
