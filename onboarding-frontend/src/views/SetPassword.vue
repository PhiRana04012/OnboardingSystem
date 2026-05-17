<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center px-4">
    <div class="w-full max-w-md">
      <!-- Logo/Header -->
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Система Онбординга</h1>
        <p class="text-gray-600 dark:text-gray-400 mt-2">Установка пароля</p>
      </div>

      <!-- Card -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden">
        <!-- Success State -->
        <div v-if="isSuccess" class="p-8">
          <div class="flex justify-center mb-6">
            <div class="w-16 h-16 bg-green-100 dark:bg-green-900/30 rounded-full flex items-center justify-center">
              <svg class="w-8 h-8 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-center text-gray-900 dark:text-white mb-4">Пароль успешно установлен!</h2>
          <p class="text-gray-600 dark:text-gray-400 text-center mb-6">
            Ваша учетная запись готова к использованию. Теперь вы можете войти в систему.
          </p>
          <router-link to="/login" class="block w-full bg-primary-600 hover:bg-primary-700 text-white font-semibold py-2 px-4 rounded-lg text-center transition-colors">
            Перейти к входу
          </router-link>
        </div>

        <!-- Loading State -->
        <div v-else-if="isLoading" class="p-8">
          <div class="flex justify-center mb-6">
            <div class="relative w-12 h-12">
              <div class="absolute inset-0 bg-primary-600 rounded-full animate-spin opacity-25"></div>
              <div class="absolute inset-1 bg-white dark:bg-gray-800 rounded-full"></div>
            </div>
          </div>
          <p class="text-center text-gray-600 dark:text-gray-400">Загрузка...</p>
        </div>

        <!-- Error State -->
        <div v-else-if="error" class="p-8">
          <div class="flex justify-center mb-6">
            <div class="w-16 h-16 bg-red-100 dark:bg-red-900/30 rounded-full flex items-center justify-center">
              <svg class="w-8 h-8 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-center text-gray-900 dark:text-white mb-4">Ошибка</h2>
          <p class="text-gray-600 dark:text-gray-400 text-center mb-6">
            {{ error }}
          </p>
          <router-link to="/login" class="block w-full bg-primary-600 hover:bg-primary-700 text-white font-semibold py-2 px-4 rounded-lg text-center transition-colors">
            Вернуться на вход
          </router-link>
        </div>

        <!-- Form State -->
        <form v-else @submit.prevent="handleSubmit" class="p-8">
          <div class="space-y-6">
            <!-- Password Field -->
            <div>
              <label for="password" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Новый пароль
              </label>
              <div class="relative">
                <input
                  id="password"
                  v-model="password"
                  :type="showPassword ? 'text' : 'password'"
                  placeholder="Не менее 8 символов"
                  required
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition"
                />
                <button
                  type="button"
                  @click="showPassword = !showPassword"
                  class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300"
                >
                  <svg v-if="!showPassword" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
                  </svg>
                  <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-4.803m5.596-3.856a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0m7.6 13.4l1.414-1.414m2.121-2.121l1.414-1.414M9.172 9.172L7.757 7.757m9.9 9.9l-1.414-1.414"></path>
                  </svg>
                </button>
              </div>
              <p v-if="password" class="text-xs mt-2" :class="password.length >= 8 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'">
                {{ password.length >= 8 ? '✓ Пароль подходит' : '✗ Минимум 8 символов' }}
              </p>
            </div>

            <!-- Confirm Password Field -->
            <div>
              <label for="confirmPassword" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Подтверждение пароля
              </label>
              <div class="relative">
                <input
                  id="confirmPassword"
                  v-model="confirmPassword"
                  :type="showConfirmPassword ? 'text' : 'password'"
                  placeholder="Повторите пароль"
                  required
                  class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition"
                />
                <button
                  type="button"
                  @click="showConfirmPassword = !showConfirmPassword"
                  class="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300"
                >
                  <svg v-if="!showConfirmPassword" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
                  </svg>
                  <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-4.803m5.596-3.856a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0m7.6 13.4l1.414-1.414m2.121-2.121l1.414-1.414M9.172 9.172L7.757 7.757m9.9 9.9l-1.414-1.414"></path>
                  </svg>
                </button>
              </div>
              <p v-if="confirmPassword && password !== confirmPassword" class="text-xs mt-2 text-red-600 dark:text-red-400">
                ✗ Пароли не совпадают
              </p>
              <p v-else-if="confirmPassword && password === confirmPassword" class="text-xs mt-2 text-green-600 dark:text-green-400">
                ✓ Пароли совпадают
              </p>
            </div>

            <!-- Password Requirements -->
            <div class="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
              <p class="text-sm font-semibold text-blue-900 dark:text-blue-300 mb-2">Требования к паролю:</p>
              <ul class="text-sm text-blue-800 dark:text-blue-400 space-y-1">
                <li class="flex items-center">
                  <span :class="password.length >= 8 ? 'text-green-600 dark:text-green-400' : 'text-gray-400'">✓</span>
                  <span class="ml-2">Не менее 8 символов</span>
                </li>
              </ul>
            </div>

            <!-- Error Message -->
            <div v-if="formError" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
              <p class="text-sm text-red-700 dark:text-red-400">{{ formError }}</p>
            </div>

            <!-- Submit Button -->
            <button
              type="submit"
              :disabled="!isFormValid || submitting"
              class="w-full bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 text-white font-semibold py-2 px-4 rounded-lg transition-colors duration-200 flex items-center justify-center space-x-2"
            >
              <span v-if="!submitting">Установить пароль</span>
              <span v-else>Обработка...</span>
            </button>
          </div>

          <!-- Info Text -->
          <p class="text-xs text-gray-600 dark:text-gray-400 text-center mt-6">
            Если вы уже установили пароль, вы можете 
            <router-link to="/login" class="text-primary-600 dark:text-primary-400 hover:underline">войти здесь</router-link>
          </p>
        </form>
      </div>

      <!-- Footer -->
      <p class="text-xs text-gray-600 dark:text-gray-400 text-center mt-8">
        © 2026 Система Онбординга. Все права защищены.
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usersApi } from '../api/services'

const route = useRoute()
const router = useRouter()

const password = ref('')
const confirmPassword = ref('')
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const isLoading = ref(true)
const isSuccess = ref(false)
const error = ref('')
const formError = ref('')
const submitting = ref(false)

const token = computed(() => route.query.token || '')

const isFormValid = computed(() => {
  return password.value.length >= 8 && 
         confirmPassword.value.length >= 8 &&
         password.value === confirmPassword.value
})

onMounted(async () => {
  // Проверяем наличие токена
  if (!token.value) {
    error.value = 'Токен не предоставлен. Проверьте ссылку из письма.'
    isLoading.value = false
    return
  }

  // Можно добавить проверку токена на валидность здесь
  // Но обычно проверка происходит при отправке формы
  isLoading.value = false
})

const handleSubmit = async () => {
  if (!isFormValid.value) {
    formError.value = 'Проверьте корректность заполнения полей'
    return
  }

  submitting.value = true
  formError.value = ''

  try {
    const response = await usersApi.setPassword({
      token: token.value,
      password: password.value,
      confirmPassword: confirmPassword.value
    })

    if (response.data.success) {
      isSuccess.value = true
    } else {
      error.value = response.data.errorMessage || 'Ошибка при установке пароля'
    }
  } catch (err) {
    console.error('SetPassword error:', err)
    
    let errorMessage = 'Ошибка при установке пароля. Попробуйте снова.'
    
    if (err.response?.data?.errorMessage) {
      errorMessage = err.response.data.errorMessage
    } else if (err.response?.data?.message) {
      errorMessage = err.response.data.message
    } else if (err.response?.status === 401) {
      errorMessage = 'Токен недействителен или истек. Запросите новую ссылку.'
    } else if (err.response?.status === 400) {
      errorMessage = 'Пароль не соответствует требованиям безопасности.'
    }
    
    error.value = errorMessage
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
/* Animations */
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.animate-spin {
  animation: spin 1s linear infinite;
}
</style>
