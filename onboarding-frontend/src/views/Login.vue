<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-50 via-white to-primary-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900 transition-colors duration-300">
    <!-- Theme Toggle -->
    <button
      @click="themeStore.toggleTheme()"
      class="absolute top-4 right-4 p-3 rounded-lg bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
      :title="themeStore.isDark ? 'Светлая тема' : 'Тёмная тема'"
    >
      <svg v-if="!themeStore.isDark" class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
        <path d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z" />
      </svg>
      <svg v-else class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
        <path fill-rule="evenodd" d="M10 2a1 1 0 011 1v1a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l-2.12-2.12a1 1 0 00-1.414 0l-.707.707a1 1 0 001.414 1.414l2.12 2.12a1 1 0 001.414-1.414l-.707-.707zm2.12-10.607a1 1 0 010 1.414l-.707.707a1 1 0 11-1.414-1.414l.707-.707a1 1 0 011.414 0zM17 11a1 1 0 100-2h-1a1 1 0 100 2h1zm-7 4a1 1 0 011 1v1a1 1 0 11-2 0v-1a1 1 0 011-1zM5.05 6.464A1 1 0 106.464 5.05l-.707-.707a1 1 0 00-1.414 1.414l.707.707zm5.657-9.193a1 1 0 00-1.414 0l-.707.707A1 1 0 005.05 3.536l.707-.707a1 1 0 011.414 0zM5 6a1 1 0 100-2H4a1 1 0 100 2h1z" clip-rule="evenodd" />
      </svg>
    </button>

    <div class="max-w-md w-full space-y-8 p-8 bg-white dark:bg-gray-800 rounded-xl shadow-lg transition-colors">
      <div class="text-center">
        <div class="mx-auto w-16 h-16 bg-gradient-to-br from-primary-50 to-white dark:from-gray-700 dark:to-gray-800 border border-primary-600 dark:border-primary-500 rounded-xl flex items-center justify-center mb-4 overflow-hidden">
          <img src="/image6.png" alt="Логотип" class="w-full h-full object-contain" />
        </div>
        <h2 class="text-3xl font-bold text-gray-900 dark:text-white">Система онбординга</h2>
        <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">Войдите в систему для продолжения</p>
      </div>

      <form @submit.prevent="handleLogin" class="mt-8 space-y-6">
        <div>
          <label for="userId" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            ID пользователя
          </label>
          <input
            id="userId"
            v-model="userId"
            type="number"
            required
            class="input"
            placeholder="Введите ваш ID"
          />
          <p class="mt-2 text-xs text-gray-500 dark:text-gray-400">
            Для демонстрации введите ID пользователя из базы данных
          </p>
        </div>

        <div>
          <button
            type="submit"
            :disabled="isLoading"
            class="w-full btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <span v-if="!isLoading">Войти</span>
            <span v-else>Вход...</span>
          </button>
        </div>

        <div v-if="error" class="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700 text-red-700 dark:text-red-300 px-4 py-3 rounded-lg text-sm whitespace-pre-line transition-colors">
          {{ error }}
        </div>
      </form>

      <div class="mt-6 text-center">
        <p class="text-xs text-primary-600 dark:text-primary-400">
          В продакшене здесь будет интеграция с SSO
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useThemeStore } from '../stores/theme'

const router = useRouter()
const authStore = useAuthStore()
const themeStore = useThemeStore()

const userId = ref('')
const isLoading = ref(false)
const error = ref('')

const handleLogin = async () => {
  if (!userId.value) {
    error.value = 'Пожалуйста, введите ID пользователя'
    return
  }

  try {
    isLoading.value = true
    error.value = ''
    await authStore.login(parseInt(userId.value))
    router.push('/')
  } catch (err) {
    console.error('Login error:', err)
    
    // More detailed error messages
    if (err.response) {
      const status = err.response.status
      if (status === 404) {
        error.value = 'Пользователь с таким ID не найден. Проверьте правильность ID.'
      } else if (status === 500) {
        error.value = 'Ошибка сервера. Возможные причины:\n' +
          '1. База данных не инициализирована\n' +
          '2. Пользователь не имеет связанного подразделения\n' +
          '3. Проблема с подключением к базе данных\n\n' +
          'Проверьте логи сервера для подробностей.'
      } else {
        error.value = `Ошибка сервера (${status}). Попробуйте позже.`
      }
    } else if (err.request) {
      error.value = 'Не удалось подключиться к серверу. Убедитесь, что бэкенд запущен.'
    } else {
      error.value = 'Ошибка входа. Проверьте ID пользователя и попробуйте снова.'
    }
  } finally {
    isLoading.value = false
  }
}
</script>

