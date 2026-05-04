import axios from 'axios'

const baseURL =
  (window as any).__API_URL__ ??
  import.meta.env.VITE_API_URL ??
  'http://localhost:5001'

export const api = axios.create({
  baseURL,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('bluecommand_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('bluecommand_token')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  },
)
