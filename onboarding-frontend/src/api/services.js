import { identityApi as identityClient, contentApi as contentClient, progressApi as progressClient } from './index'

// Users API - Identity Service
export const usersApi = {
  login: (email, password) => identityClient.post('/users/login', { email, password }),
  getAll: () => identityClient.get('/users'),
  getById: (id) => identityClient.get(`/users/${id}`),
  create: (data) => identityClient.post('/users', data),
  update: (id, data) => identityClient.put(`/users/${id}`, data),
  updateProfile: (id, data) => identityClient.put(`/users/${id}`, data),
  delete: (id) => identityClient.delete(`/users/${id}`),
  getMentees: (mentorId) => identityClient.get(`/users/mentor/${mentorId}/mentees`)
}

// Modules API - Content Service
export const modulesApi = {
  getAll: (params) => contentClient.get('/modules', { params }),
  getById: (id) => contentClient.get(`/modules/${id}`),
  create: (data) => contentClient.post('/modules', data),
  update: (id, data) => contentClient.put(`/modules/${id}`, data),
  delete: (id) => contentClient.delete(`/modules/${id}`)
}

// Progress API - Progress Service
export const progressApi = {
  getUserProgress: (userId) => progressClient.get(`/progress/user/${userId}`),
  getModuleProgress: (userId, moduleId) => progressClient.get(`/progress/user/${userId}/module/${moduleId}`),
  markAsRead: (userId, data) => progressClient.post(`/progress/user/${userId}/mark-read`, data),
  getAll: (params) => progressClient.get('/progress', { params })
}

// Questions API - Content Service
export const questionsApi = {
  getByModule: (moduleId) => contentClient.get(`/questions/module/${moduleId}`),
  getForTest: (moduleId) => contentClient.get(`/questions/module/${moduleId}/test`),
  getById: (id) => contentClient.get(`/questions/${id}`),
  create: (data) => contentClient.post('/questions', data),
  update: (id, data) => contentClient.put(`/questions/${id}`, data),
  delete: (id) => contentClient.delete(`/questions/${id}`)
}

// Test Attempts API - Progress Service
export const testAttemptsApi = {
  getUserAttempts: (userId) => progressClient.get(`/testattempts/user/${userId}`),
  getModuleAttempts: (moduleId, userId) => progressClient.get(`/testattempts/module/${moduleId}/user/${userId}`),
  submitTest: (data) => progressClient.post('/testattempts/submit', data),
  getAttempt: (attemptId) => progressClient.get(`/testattempts/${attemptId}`),
  resetAttempts: (userId, moduleId) => progressClient.delete(`/testattempts/reset/user/${userId}/module/${moduleId}`)
}

// Reports API - Progress Service
export const reportsApi = {
  getOnboardingProgress: (userId) => progressClient.get(`/reports/onboarding-progress/${userId}`),
  getTestResults: (params) => progressClient.get('/reports/test-results', { params }),
  getDepartmentReport: (departmentId) => progressClient.get(`/reports/department/${departmentId}`),
  exportOnboardingProgress: (userId, format) => progressClient.get(`/reports/onboarding-progress/${userId}/export`, { params: { format }, responseType: 'blob' }),
  exportTestResults: (params, format) => progressClient.get('/reports/test-results/export', { params: { ...params, format }, responseType: 'blob' }),
  exportDepartmentReport: (departmentId, format) => progressClient.get(`/reports/department/${departmentId}/export`, { params: { format }, responseType: 'blob' })
}

// Departments API - Identity Service
export const departmentsApi = {
  getAll: () => identityClient.get('/departments'),
  getById: (id) => identityClient.get(`/departments/${id}`),
  create: (data) => identityClient.post('/departments', data),
  update: (id, data) => identityClient.put(`/departments/${id}`, data),
  delete: (id) => identityClient.delete(`/departments/${id}`)
}

// Roles API - Identity Service
export const rolesApi = {
  getAll: () => identityClient.get('/roles')
}

// Action Logs API - Progress Service
export const actionLogsApi = {
  getAll: (params) => progressClient.get('/actionlogs', { params }),
  getByUser: (userId) => progressClient.get(`/actionlogs/user/${userId}`)
}

// RIMS Sync API - Identity Service
export const rimsSyncApi = {
  syncUserByUid: (rimsUid) => identityClient.post(`/rimssync/sync-user/${rimsUid}`),
  syncUserByEmail: (email) => identityClient.post('/rimssync/sync-by-email', { email })
}

// Gamification API - Progress Service
export const gamificationApi = {
  getUserProfile: (userId) => progressClient.get(`/gamification/user/${userId}`)
}

// Checklists API - Progress Service
export const checklistsApi = {
  getModuleChecklist: (moduleId, userId) => progressClient.get(`/checklists/module/${moduleId}/user/${userId}`),
  toggleItem: (data) => progressClient.post('/checklists/toggle', data),
  create: (data) => progressClient.post('/checklists', data),
  update: (id, data) => progressClient.put(`/checklists/${id}`, data),
  delete: (id) => progressClient.delete(`/checklists/${id}`)
}

// FAQ API - Content Service
export const faqApi = {
  getAll: () => contentClient.get('/faq'),
  create: (data) => contentClient.post('/faq', data),
  update: (id, data) => contentClient.put(`/faq/${id}`, data),
  delete: (id) => contentClient.delete(`/faq/${id}`)
}








