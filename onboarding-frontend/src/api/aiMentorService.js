import { identityApi } from './index.js'

export const aiMentorService = {
  /**
   * Отправить сообщение к AI помощнику
   * @param {string} message - Сообщение пользователя
   * @returns {Promise<{success: boolean, reply?: string, errorMessage?: string}>}
   */
  async chat(message) {
    try {
      const response = await identityApi.post('/aimentor/chat', {
        message
      })
      return response.data
    } catch (error) {
      console.error('AI Mentor chat error:', error)
      return {
        success: false,
        errorMessage: error.response?.data?.errorMessage || 'Ошибка при обращении к AI помощнику'
      }
    }
  }
}

export default aiMentorService
