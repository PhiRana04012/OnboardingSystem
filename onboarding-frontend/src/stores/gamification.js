import { defineStore } from 'pinia'
import { gamificationApi } from '../api/services'

export const useGamificationStore = defineStore('gamification', {
  state: () => ({
    profile: null,
    isLoading: false,
    error: null
  }),
  
  getters: {
    levelProgress: (state) => {
      if (!state.profile) return 0
      if (state.profile.levelProgressPercent != null)
        return state.profile.levelProgressPercent
      const currentLevelBaseXP = (state.profile.level - 1) * 100
      const xpInCurrentLevel = state.profile.totalXP - currentLevelBaseXP
      const xpNeeded = state.profile.nextLevelXP - currentLevelBaseXP
      if (xpNeeded <= 0) return 100
      return Math.min(100, Math.max(0, (xpInCurrentLevel / xpNeeded) * 100))
    }
  },

  actions: {
    async fetchProfile(userId) {
      if (!userId) return
      
      this.isLoading = true
      this.error = null
      
      try {
        const response = await gamificationApi.getUserProfile(userId)
        this.profile = response.data
      } catch (err) {
        this.error = err.response?.data?.message || 'Не удалось загрузить профиль геймификации'
        console.error('Gamification fetch error:', err)
      } finally {
        this.isLoading = false
      }
    }
  }
})
