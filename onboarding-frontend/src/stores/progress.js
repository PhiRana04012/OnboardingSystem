import { defineStore } from 'pinia'
import { ref } from 'vue'
import { progressApi } from '../api/services'

export const useProgressStore = defineStore('progress', () => {
  const userProgress = ref(null)
  const isLoading = ref(false)
  const error = ref(null)

  const fetchUserProgress = async (userId) => {
    try {
      isLoading.value = true
      error.value = null
      const response = await progressApi.getUserProgress(userId)
      userProgress.value = response.data
      return userProgress.value
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const markModuleAsRead = async (userId, moduleId) => {
    try {
      isLoading.value = true
      error.value = null
      const response = await progressApi.markAsRead(userId, { moduleId })
      // Refresh progress after marking as read
      await fetchUserProgress(userId)
      return response.data
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      isLoading.value = false
    }
  }

  return {
    userProgress,
    isLoading,
    error,
    fetchUserProgress,
    markModuleAsRead
  }
})








