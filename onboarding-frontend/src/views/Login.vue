<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-50 to-primary-100">
    <div class="max-w-md w-full space-y-8 p-8 bg-white rounded-xl shadow-lg">
      <div class="text-center">
        <div class="mx-auto w-16 h-16 bg-white border border-primary-600 rounded-xl flex items-center justify-center mb-4 overflow-hidden">
          <img src="/image6.png" alt="Логотип" class="w-full h-full object-contain" />
        </div>
        <h2 class="text-3xl font-bold text-gray-900">Система онбординга</h2>
        <p class="mt-2 text-sm text-gray-600">Войдите в систему для продолжения</p>
      </div>

      <form @submit.prevent="handleLogin" class="mt-8 space-y-6">
        <div>
          <label for="userId" class="block text-sm font-medium text-gray-700 mb-2">
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
          <p class="mt-2 text-xs text-gray-500">
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

        <div v-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm whitespace-pre-line">
          {{ error }}
        </div>
      </form>

      <div class="mt-6 text-center">
        <p class="text-xs text-primary-600">
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

const router = useRouter()
const authStore = useAuthStore()

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

