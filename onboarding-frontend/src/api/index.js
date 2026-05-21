import axios from 'axios'

// Get base URLs from environment variables or use defaults
const IDENTITY_API_URL = import.meta.env.VITE_IDENTITY_API_URL || 'http://localhost:5001/api'
const CONTENT_API_URL = import.meta.env.VITE_CONTENT_API_URL || 'http://localhost:5002/api'
const PROGRESS_API_URL = import.meta.env.VITE_PROGRESS_API_URL || 'http://localhost:5003/api'

// Create axios instances for each microservice
const createApiInstance = (baseURL) => {
  const instance = axios.create({
    baseURL,
    headers: {
      'Content-Type': 'application/json'
    }
  })

  // Request interceptor for adding auth token
  instance.interceptors.request.use(
    (config) => {
      const url = config.url || ''
      const isPublicAuth = url.includes('/users/login') || url.includes('/users/set-password')
      const token = localStorage.getItem('authToken')
      if (token && !isPublicAuth) {
        config.headers.Authorization = `Bearer ${token}`
      }
      return config
    },
    (error) => {
      return Promise.reject(error)
    }
  )

  // Response interceptor for error handling
  instance.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error.response) {
        const status = error.response.status
        const data = error.response.data
        
        if (status === 401) {
          const url = error.config?.url || ''
          const isLogin = url.includes('/users/login')
          if (!isLogin) {
            localStorage.removeItem('authToken')
            localStorage.removeItem('currentUser')
            if (!window.location.pathname.startsWith('/login')) {
              window.location.href = '/login'
            }
          }
          console.error('Unauthorized')
        } else if (status === 404) {
          console.error('Resource not found:', error.config.url)
        } else if (status === 500) {
          console.error('Server error:', data || 'Internal server error')
          console.error('Request URL:', error.config.url)
          console.error('Request method:', error.config.method)
          if (typeof data === 'string') {
            console.error('Error details:', data)
          } else if (data && typeof data === 'object') {
            console.error('Error object:', JSON.stringify(data, null, 2))
          }
        }
      } else if (error.request) {
        console.error('No response received:', error.request)
      } else {
        console.error('Error setting up request:', error.message)
      }
      return Promise.reject(error)
    }
  )

  return instance
}

// Export separate API instances for each microservice
export const identityApi = createApiInstance(IDENTITY_API_URL)
export const contentApi = createApiInstance(CONTENT_API_URL)
export const progressApi = createApiInstance(PROGRESS_API_URL)

// Export default (for backward compatibility, uses Identity service)
export default identityApi
