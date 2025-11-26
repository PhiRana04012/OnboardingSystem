import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { usersApi } from '../api/services'

export const useAuthStore = defineStore('auth', () => {
  const currentUser = ref(null)
  const isLoading = ref(false)

  const isAuthenticated = computed(() => currentUser.value !== null)
  
  const hasRole = (role) => {
    if (!currentUser.value) return false
    return currentUser.value.roles?.includes(role) || false
  }

  const isNewEmployee = computed(() => hasRole('Новый сотрудник'))
  const isMentor = computed(() => hasRole('Наставник'))
  const isHR = computed(() => hasRole('HR-специалист'))
  const isManager = computed(() => hasRole('Руководитель подразделения'))
  const isAdmin = computed(() => hasRole('Администратор системы'))

  const login = async (userId) => {
    try {
      isLoading.value = true
      const response = await usersApi.getById(userId)
      
      if (!response.data) {
        throw new Error('Пользователь не найден')
      }
      
      currentUser.value = response.data
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

  const logout = () => {
    currentUser.value = null
  }

  const refreshUser = async () => {
    if (!currentUser.value) return
    try {
      const response = await usersApi.getById(currentUser.value.userId)
      currentUser.value = response.data
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
    hasRole,
    login,
    logout,
    refreshUser
  }
})

