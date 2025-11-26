import api from './index'

// Users API
export const usersApi = {
  getAll: () => api.get('/users'),
  getById: (id) => api.get(`/users/${id}`),
  create: (data) => api.post('/users', data),
  update: (id, data) => api.put(`/users/${id}`, data),
  delete: (id) => api.delete(`/users/${id}`)
}

// Modules API
export const modulesApi = {
  getAll: (params) => api.get('/modules', { params }),
  getById: (id) => api.get(`/modules/${id}`),
  create: (data) => api.post('/modules', data),
  update: (id, data) => api.put(`/modules/${id}`, data),
  delete: (id) => api.delete(`/modules/${id}`)
}

// Progress API
export const progressApi = {
  getUserProgress: (userId) => api.get(`/progress/user/${userId}`),
  getModuleProgress: (userId, moduleId) => api.get(`/progress/user/${userId}/module/${moduleId}`),
  markAsRead: (userId, data) => api.post(`/progress/user/${userId}/mark-read`, data),
  getAll: (params) => api.get('/progress', { params })
}

// Questions API
export const questionsApi = {
  getByModule: (moduleId) => api.get(`/questions/module/${moduleId}`),
  getForTest: (moduleId) => api.get(`/questions/module/${moduleId}/test`),
  getById: (id) => api.get(`/questions/${id}`),
  create: (data) => api.post('/questions', data),
  update: (id, data) => api.put(`/questions/${id}`, data),
  delete: (id) => api.delete(`/questions/${id}`)
}

// Test Attempts API
export const testAttemptsApi = {
  getUserAttempts: (userId) => api.get(`/testattempts/user/${userId}`),
  getModuleAttempts: (moduleId, userId) => api.get(`/testattempts/module/${moduleId}/user/${userId}`),
  submitTest: (data) => api.post('/testattempts/submit', data),
  getAttempt: (attemptId) => api.get(`/testattempts/${attemptId}`)
}

// Reports API
export const reportsApi = {
  getOnboardingProgress: (userId) => api.get(`/reports/onboarding-progress/${userId}`),
  getTestResults: (params) => api.get('/reports/test-results', { params }),
  getDepartmentReport: (departmentId) => api.get(`/reports/department/${departmentId}`)
}

// Departments API
export const departmentsApi = {
  getAll: () => api.get('/departments'),
  getById: (id) => api.get(`/departments/${id}`),
  create: (data) => api.post('/departments', data),
  update: (id, data) => api.put(`/departments/${id}`, data),
  delete: (id) => api.delete(`/departments/${id}`)
}

// Action Logs API
export const actionLogsApi = {
  getAll: (params) => api.get('/actionlogs', { params }),
  getByUser: (userId) => api.get(`/actionlogs/user/${userId}`)
}

// RIMS Sync API
export const rimsSyncApi = {
  syncUserByUid: (rimsUid) => api.post(`/rimssync/sync-user/${rimsUid}`),
  syncUserByEmail: (email) => api.post('/rimssync/sync-by-email', { email })
}






