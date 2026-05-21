import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { usersApi } from '../api/services'
import { useNotificationsStore } from './notifications'

export const useAuthStore = defineStore('auth', () => {
  // Восстанавливаем пользователя из localStorage (если есть) на старте
  const storedUser = localStorage.getItem('currentUser')
  const storedToken = localStorage.getItem('authToken')
  const currentUser = ref(storedUser ? JSON.parse(storedUser) : null)
  const isLoading = ref(false)

  const isAuthenticated = computed(() => currentUser.value !== null)
  
  const hasRole = (role) => {
    if (!currentUser.value) return false
    return currentUser.value.roles?.includes(role) || false
  }

  const isNewEmployee = computed(() => hasRole('Новый сотрудник'))
  // Наставник определяется по наличию роли "Наставник"
  const isMentor = computed(() => hasRole('Наставник'))
  const isHR = computed(() => hasRole('HR-специалист'))
  const isManager = computed(() => hasRole('Руководитель подразделения'))
  const isAdmin = computed(() => hasRole('Администратор системы'))
  
  // Начальник отдела - это пользователь с ролью "User" и должностью "DepartmentHead"
  const isDepartmentHead = computed(() => {
    if (!currentUser.value || hasRole('Администратор системы')) return false
    if (!currentUser.value.jobTitle) return false
    return currentUser.value.jobTitle.title === 'Начальник отдела' || 
           currentUser.value.jobTitle.title === 'DepartmentHead'
  })
  
  // Может ли пользователь управлять подопечными в своём отделе
  const canManageMentees = computed(() => {
    return isAdmin.value || isDepartmentHead.value
  })
  
  // Может ли пользователь просматривать отчёты по отделу
  const canViewDepartmentReports = computed(() => {
    return isAdmin.value || isDepartmentHead.value || isHR.value
  })

  // Полный доступ (все подразделения): админ и HR
  const isFullAdmin = computed(() => isAdmin.value || isHR.value)

  // Управление отделом: начальник — свой отдел; админ/HR — все
  const canManageDepartment = computed(() => {
    return isFullAdmin.value || isDepartmentHead.value
  })

  const login = async (email, password) => {
    try {
      isLoading.value = true
      localStorage.removeItem('authToken')
      localStorage.removeItem('currentUser')
      const response = await usersApi.login(email, password)
      
      if (!response.data.success) {
        throw new Error(response.data.errorMessage || 'Ошибка аутентификации')
      }
      
      // Сохраняем токен
      localStorage.setItem('authToken', response.data.token)
      
      // Сохраняем пользователя
      currentUser.value = response.data.user
      localStorage.setItem('currentUser', JSON.stringify(currentUser.value))

      const notificationsStore = useNotificationsStore()
      await notificationsStore.connect()
      
      return currentUser.value
    } catch (error) {
      console.error('Login error:', error)
      
      // Re-throw with more context
      if (error.response) {
        // Server error
        let errorMessage = `Server error: ${error.response.status}`
        
        // Try to extract meaningful error message
        if (error.response.data) {
          if (typeof error.response.data === 'string') {
            errorMessage = error.response.data
          } else if (error.response.data.errorMessage) {
            errorMessage = error.response.data.errorMessage
          } else if (error.response.data.message) {
            errorMessage = error.response.data.message
          } else if (error.response.data.title) {
            errorMessage = error.response.data.title
          } else if (error.response.data.error) {
            errorMessage = error.response.data.error
          }
        }
        
        const serverError = new Error(errorMessage)
        serverError.response = error.response
        throw serverError
      } else if (error.request) {
        // Network error
        throw new Error('Не удалось подключиться к серверу. Проверьте, что бэкенд запущен.')
      } else {
        // Other error
        throw error
      }
    } finally {
      isLoading.value = false
    }
  }

  const logout = async () => {
    const notificationsStore = useNotificationsStore()
    await notificationsStore.disconnect()
    currentUser.value = null
    localStorage.removeItem('currentUser')
    localStorage.removeItem('authToken')
  }

  const refreshUser = async () => {
    if (!currentUser.value) return
    try {
      const response = await usersApi.getById(currentUser.value.userId)
      console.log('🔄 refreshUser - Server response:', response.data)
      console.log('   - bio:', response.data?.bio)
      console.log('   - telegramTag:', response.data?.telegramTag)
      currentUser.value = response.data
      localStorage.setItem('currentUser', JSON.stringify(currentUser.value))
      console.log('✅ User refreshed and saved to localStorage')
    } catch (error) {
      console.error('Refresh user error:', error)
    }
  }

  return {
    currentUser,
    isLoading,
    isAuthenticated,
    isNewEmployee,
    isMentor,
    isHR,
    isManager,
    isAdmin,
    isDepartmentHead,
    canManageMentees,
    canViewDepartmentReports,
    isFullAdmin,
    canManageDepartment,
    hasRole,
    login,
    logout,
    refreshUser
  }
})

