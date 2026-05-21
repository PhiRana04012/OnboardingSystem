export function getHubBaseUrl() {
  const apiUrl = import.meta.env.VITE_IDENTITY_API_URL || 'http://localhost:5001/api'
  return apiUrl.replace(/\/api\/?$/, '')
}
