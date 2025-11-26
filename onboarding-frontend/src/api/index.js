import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor for adding auth token if needed
api.interceptors.request.use(
  (config) => {
    // TODO: Add SSO token if needed
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      // Server responded with error status
      const status = error.response.status
      const data = error.response.data
      
      if (status === 401) {
        console.error('Unauthorized')
      } else if (status === 404) {
        console.error('Resource not found:', error.config.url)
      } else if (status === 500) {
        console.error('Server error:', data || 'Internal server error')
        // Log more details for debugging
        console.error('Request URL:', error.config.url)
        console.error('Request method:', error.config.method)
        // Try to extract more details from the error response
        if (typeof data === 'string') {
          console.error('Error details:', data)
        } else if (data && typeof data === 'object') {
          console.error('Error object:', JSON.stringify(data, null, 2))
        }
      }
    } else if (error.request) {
      // Request was made but no response received
      console.error('No response received:', error.request)
    } else {
      // Error setting up the request
      console.error('Error setting up request:', error.message)
    }
    return Promise.reject(error)
  }
)

export default api

