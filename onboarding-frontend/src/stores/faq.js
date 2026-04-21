import { defineStore } from 'pinia'
import { faqApi } from '../api/services'

export const useFaqStore = defineStore('faq', {
  state: () => ({
    items: [],
    isLoading: false,
    error: null
  }),

  getters: {
    categories: (state) => {
      const cats = Array.from(new Set(state.items.map(item => item.category)))
      return cats.sort()
    },
    getByCategory: (state) => (category) => {
      return state.items.filter(item => item.category === category)
    }
  },

  actions: {
    async fetchFaq() {
      this.isLoading = true
      try {
        const response = await faqApi.getAll()
        this.items = response.data
      } catch (err) {
        this.error = 'Не удалось загрузить справку'
        console.error(err)
      } finally {
        this.isLoading = false
      }
    },

    async addFaq(item) {
      try {
        const response = await faqApi.create(item)
        this.items.push(response.data)
        return response.data
      } catch (err) {
        console.error(err)
        throw err
      }
    },

    async deleteFaq(id) {
      try {
        await faqApi.delete(id)
        this.items = this.items.filter(i => i.id !== id)
      } catch (err) {
        console.error(err)
        throw err
      }
    }
  }
})
