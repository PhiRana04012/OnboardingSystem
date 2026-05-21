<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-50 via-white to-primary-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900 transition-colors duration-300">
    <!-- Theme Toggle -->
    <button
      @click="themeStore.toggleTheme()"
      class="absolute top-4 right-4 p-3 rounded-lg bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
      :title="themeStore.isDark ? 'Светлая тема' : 'Тёмная тема'"
    >
      <ThemeIcon :is-dark="themeStore.isDark" size-class="w-5 h-5" />
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
          <label for="email" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Email
          </label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            class="input"
            placeholder="Введите ваш email"
          />
        </div>

        <div>
          <label for="password" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Пароль
          </label>
          <div class="relative">
            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              required
              class="input pr-12"
              placeholder="Введите ваш пароль"
            />
            <button
              type="button"
              @click="showPassword = !showPassword"
              class="absolute inset-y-0 right-0 px-3 flex items-center text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 transition-colors"
              :title="showPassword ? 'Скрыть пароль' : 'Показать пароль'"
              :aria-label="showPassword ? 'Скрыть пароль' : 'Показать пароль'"
            >
              <svg v-if="!showPassword" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5s8.268 2.943 9.542 7c-1.274 4.057-5.065 7-9.542 7S3.732 16.057 2.458 12z" />
              </svg>
              <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.477 0-8.268-2.943-9.542-7a9.964 9.964 0 012.25-3.592M9.88 9.88a3 3 0 104.243 4.243" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6.1 6.1A9.956 9.956 0 0112 5c4.477 0 8.268 2.943 9.542 7a9.97 9.97 0 01-4.132 5.411M3 3l18 18" />
              </svg>
            </button>
          </div>
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
import ThemeIcon from '../components/ThemeIcon.vue'

const router = useRouter()
const authStore = useAuthStore()
const themeStore = useThemeStore()

const email = ref('')
const password = ref('')
const showPassword = ref(false)
const isLoading = ref(false)
const error = ref('')

const handleLogin = async () => {
  if (!email.value || !password.value) {
    error.value = 'Пожалуйста, введите email и пароль'
    return
  }

  try {
    isLoading.value = true
    error.value = ''
    await authStore.login(email.value, password.value)
    router.push('/')
  } catch (err) {
    console.error('Login error:', err)
    
    // More detailed error messages
    if (err.response) {
      const status = err.response.status
      if (status === 401) {
        error.value = 'Неверный email или пароль. Проверьте учётные данные.'
      } else if (status === 500) {
        error.value = 'Ошибка сервера. Попробуйте позже.'
      } else {
        error.value = `Ошибка сервера (${status}). Попробуйте позже.`
      }
    } else if (err.request) {
      error.value = 'Не удалось подключиться к серверу. Убедитесь, что бэкенд запущен.'
    } else {
      error.value = err.message || 'Ошибка входа. Попробуйте снова.'
    }
  } finally {
    isLoading.value = false
  }
}
</script>

