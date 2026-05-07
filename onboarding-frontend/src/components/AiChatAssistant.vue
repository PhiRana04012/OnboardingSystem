<template>
  <div class="flex flex-col h-[520px] w-full bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
    <!-- Header -->
    <div class="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700 bg-gradient-to-r from-primary-50 to-primary-100 dark:from-primary-900/30 dark:to-primary-800/30 flex-shrink-0">
      <div class="flex items-center space-x-3">
        <div class="w-10 h-10 rounded-full bg-primary-600 dark:bg-primary-500 flex items-center justify-center text-white shadow-md flex-shrink-0">
          <svg class="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
            <path d="M2 11a1 1 0 011-1h2a1 1 0 011 1v5a1 1 0 01-1 1H3a1 1 0 01-1-1v-5zM8 7a1 1 0 011-1h2a1 1 0 011 1v9a1 1 0 01-1 1H9a1 1 0 01-1-1V7zM14 4a1 1 0 011-1h2a1 1 0 011 1v12a1 1 0 01-1 1h-2a1 1 0 01-1-1V4z" />
          </svg>
        </div>
        <div class="min-w-0">
          <h3 class="text-base font-bold text-gray-900 dark:text-white truncate">Личный AI-наставник 24/7</h3>
        
        </div>
      </div>
      <button
        @click="$emit('close')"
        class="p-1.5 rounded hover:bg-gray-200 dark:hover:bg-gray-700 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 transition-colors flex-shrink-0 ml-2"
        title="Свернуть"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>

    <!-- Messages Area -->
    <div
      ref="messagesContainer"
      class="flex-1 overflow-y-auto p-3 space-y-3 bg-gray-50 dark:bg-gray-900/30"
    >
      <div v-if="messages.length === 0" class="flex items-center justify-center h-full">
        <div class="text-center text-gray-500 dark:text-gray-400">
          <svg class="w-12 h-12 mx-auto mb-2 opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <p class="font-medium text-sm">Начните разговор</p>
          <p class="text-xs mt-1">Задайте вопрос о системе</p>
        </div>
      </div>

      <!-- Messages -->
      <div
        v-for="(msg, index) in messages"
        :key="index"
        class="flex gap-2"
        :class="msg.role === 'user' ? 'justify-end' : 'justify-start'"
      >
        <!-- AI Message -->
        <div v-if="msg.role === 'assistant'" class="flex gap-2 max-w-[85%]">
          <div class="w-7 h-7 rounded-full bg-primary-600 dark:bg-primary-500 flex-shrink-0 flex items-center justify-center text-white text-xs font-bold flex-none">
            AI
          </div>
          <div class="bg-white dark:bg-gray-700 rounded-lg px-3 py-2 shadow-sm border border-gray-200 dark:border-gray-600">
            <p class="text-gray-900 dark:text-gray-100 text-sm whitespace-pre-wrap break-words">{{ msg.content }}</p>
            <p v-if="msg.error" class="text-red-600 dark:text-red-400 text-xs mt-1.5">{{ msg.error }}</p>
          </div>
        </div>

        <!-- User Message -->
        <div v-else class="flex gap-2 justify-end max-w-[85%]">
          <div class="bg-primary-600 dark:bg-primary-700 rounded-lg px-3 py-2 shadow-sm">
            <p class="text-white text-sm whitespace-pre-wrap break-words">{{ msg.content }}</p>
          </div>
        </div>
      </div>

      <!-- Loading Indicator -->
      <div v-if="isLoading" class="flex gap-2">
        <div class="w-7 h-7 rounded-full bg-primary-600 dark:bg-primary-500 flex-shrink-0 flex items-center justify-center text-white text-xs font-bold flex-none">
          AI
        </div>
        <div class="bg-white dark:bg-gray-700 rounded-lg px-3 py-2 shadow-sm border border-gray-200 dark:border-gray-600">
          <div class="flex space-x-1.5">
            <div class="w-2 h-2 bg-gray-400 dark:bg-gray-500 rounded-full animate-bounce"></div>
            <div class="w-2 h-2 bg-gray-400 dark:bg-gray-500 rounded-full animate-bounce" style="animation-delay: 0.2s;"></div>
            <div class="w-2 h-2 bg-gray-400 dark:bg-gray-500 rounded-full animate-bounce" style="animation-delay: 0.4s;"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Input Area -->
    <div class="border-t border-gray-200 dark:border-gray-700 p-3 bg-white dark:bg-gray-800 flex-shrink-0">
      <form @submit.prevent="sendMessage" class="flex gap-2 items-center">
        <input
          v-model="userMessage"
          :disabled="isLoading"
          type="text"
          placeholder="Спросите про онбординг..."
          class="flex-1 min-w-0 px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors text-sm"
        />
        <button
          type="submit"
          :disabled="!userMessage.trim() || isLoading"
          class="px-3 py-2 bg-primary-600 hover:bg-primary-700 dark:bg-primary-600 dark:hover:bg-primary-700 text-white rounded-lg font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center flex-shrink-0"
          title="Отправить"
        >
          <svg v-if="!isLoading" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
          </svg>
          <svg v-else class="w-5 h-5 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 2a10 10 0 1 0 10 10" />
          </svg>
        </button>
      </form>
    </div>
  </div>
</template>

<script>
import { ref, nextTick } from 'vue'
import aiMentorService from '../api/aiMentorService.js'

export default {
  name: 'AiChatAssistant',
  emits: ['close'],
  setup() {
    const messages = ref([])
    const userMessage = ref('')
    const isLoading = ref(false)
    const messagesContainer = ref(null)

    const scrollToBottom = async () => {
      await nextTick()
      if (messagesContainer.value) {
        messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
      }
    }

    const sendMessage = async () => {
      if (!userMessage.value.trim() || isLoading.value) return

      const message = userMessage.value.trim()
      userMessage.value = ''

      // Add user message to chat
      messages.value.push({
        role: 'user',
        content: message
      })

      await scrollToBottom()

      isLoading.value = true

      try {
        const response = await aiMentorService.chat(message)

        if (response.success && response.reply) {
          messages.value.push({
            role: 'assistant',
            content: response.reply
          })
        } else {
          messages.value.push({
            role: 'assistant',
            content: 'Извините, не удалось получить ответ.',
            error: response.errorMessage
          })
        }
      } catch (error) {
        console.error('Chat error:', error)
        messages.value.push({
          role: 'assistant',
          content: 'Произошла ошибка при отправке сообщения.',
          error: error.message
        })
      } finally {
        isLoading.value = false
        await scrollToBottom()
      }
    }

    return {
      messages,
      userMessage,
      isLoading,
      messagesContainer,
      sendMessage
    }
  }
}
</script>

<style scoped>
/* Custom scrollbar styling */
::-webkit-scrollbar {
  width: 6px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

.dark ::-webkit-scrollbar-thumb {
  background: #475569;
}

.dark ::-webkit-scrollbar-thumb:hover {
  background: #64748b;
}
</style>
