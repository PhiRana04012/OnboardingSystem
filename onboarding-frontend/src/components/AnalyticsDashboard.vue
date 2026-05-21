<template>
  <div class="space-y-6 p-6">
    <!-- Loading State -->
    <div v-if="isLoading" class="flex items-center justify-center h-96">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto mb-4"></div>
        <p class="text-gray-600 dark:text-gray-400">Анализируем ваш прогресс...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4 text-red-700 dark:text-red-400">
      <p>{{ error }}</p>
    </div>

    <!-- Main Content -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Next Recommended Module -->
      <div class="lg:col-span-2 bg-gradient-to-r from-blue-50 to-cyan-50 dark:from-blue-900/30 dark:to-cyan-900/30 rounded-lg shadow p-6 border-l-4 border-blue-500">
        <div class="flex items-start justify-between">
          <div class="flex-1">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">🎯 Рекомендованный следующий модуль</h3>
            <p v-if="nextModule" class="text-gray-700 dark:text-gray-300">
              <span class="font-semibold">{{ nextModule.moduleTitle }}</span>
              <span class="text-sm text-gray-600 dark:text-gray-400"> (Приоритет: {{ nextModule.priority }}/10)</span>
            </p>
            <p v-else class="text-gray-700 dark:text-gray-300">Все модули пройдены! Поздравляем! 🎉</p>
            <p v-if="nextModule" class="text-sm text-gray-600 dark:text-gray-400 mt-2">
              {{ nextModule.reason }}
            </p>
          </div>
          <button
            v-if="nextModule"
            @click="startModule(nextModule.moduleId)"
            class="ml-4 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors flex-shrink-0"
          >
            Начать →
          </button>
        </div>
      </div>

      <!-- Weak Areas -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
        <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-4 flex items-center">
          <span class="text-2xl mr-2">⚠️</span>
          Области для улучшения
        </h3>

        <div v-if="analytics.weakAreas.length === 0" class="text-center py-8">
          <p class="text-gray-500 dark:text-gray-400">Нет слабых сторон! Вы молодец! 🌟</p>
        </div>

        <div v-else class="space-y-4">
          <div v-for="area in analytics.weakAreas" :key="area.moduleId" class="flex flex-col gap-2">
            <div class="flex justify-between items-start">
              <span class="text-sm font-medium text-gray-900 dark:text-white">{{ area.moduleTitle }}</span>
              <span class="text-sm font-bold text-red-600 dark:text-red-400">{{ area.averageScore.toFixed(0) }}%</span>
            </div>
            <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
              <div
                class="bg-gradient-to-r from-red-500 to-orange-500 h-2 rounded-full transition-all"
                :style="{ width: area.averageScore + '%' }"
              ></div>
            </div>
            <p class="text-xs text-gray-500 dark:text-gray-400">
              {{ area.failedAttempts }} ошибок из {{ area.totalAttempts }} попыток
            </p>
          </div>
        </div>
      </div>

      <!-- Strong Areas -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
        <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-4 flex items-center">
          <span class="text-2xl mr-2">✅</span>
          Сильные стороны
        </h3>

        <div v-if="analytics.strengths.length === 0" class="text-center py-8">
          <p class="text-gray-500 dark:text-gray-400">Пока нет отличных результатов</p>
        </div>

        <div v-else class="space-y-4">
          <div v-for="strength in analytics.strengths" :key="strength.moduleId" class="flex flex-col gap-2">
            <div class="flex justify-between items-start">
              <span class="text-sm font-medium text-gray-900 dark:text-white">{{ strength.moduleTitle }}</span>
              <span class="text-sm font-bold text-green-600 dark:text-green-400">{{ strength.averageScore.toFixed(0) }}%</span>
            </div>
            <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
              <div
                class="bg-gradient-to-r from-green-500 to-emerald-500 h-2 rounded-full"
                :style="{ width: strength.averageScore + '%' }"
              ></div>
            </div>
            <p class="text-xs text-gray-500 dark:text-gray-400">
              {{ strength.successfulAttempts }} успешных попыток
            </p>
          </div>
        </div>
      </div>

      <!-- AI Insights -->
      <div class="lg:col-span-2 bg-gradient-to-r from-purple-50 to-pink-50 dark:from-purple-900/30 dark:to-pink-900/30 rounded-lg shadow p-6 border-l-4 border-purple-500">
        <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-4 flex items-center">
          <span class="text-2xl mr-2">💡</span>
          AI-анализ вашего прогресса
        </h3>

        <p class="text-gray-700 dark:text-gray-300 leading-relaxed text-sm">
          {{ analytics.aiInsight }}
        </p>

        <div class="mt-6 grid grid-cols-1 md:grid-cols-3 gap-4 pt-6 border-t border-purple-200 dark:border-purple-700">
          <div class="text-center">
            <p class="text-2xl font-bold text-purple-600 dark:text-purple-400">
              {{ analytics.estimatedCompletionDays.toFixed(0) }}
            </p>
            <p class="text-xs text-gray-600 dark:text-gray-400">дней до завершения</p>
          </div>
          <div class="text-center">
            <p class="text-2xl font-bold text-blue-600 dark:text-blue-400">
              {{ analytics.weakAreas.length }}
            </p>
            <p class="text-xs text-gray-600 dark:text-gray-400">областей для улучшения</p>
          </div>
          <div class="text-center">
            <p class="text-2xl font-bold text-green-600 dark:text-green-400">
              {{ analytics.strengths.length }}
            </p>
            <p class="text-xs text-gray-600 dark:text-gray-400">сильных сторон</p>
          </div>
        </div>
      </div>

      <!-- Recommendations -->
      <div class="lg:col-span-2 bg-white dark:bg-gray-800 rounded-lg shadow p-6">
        <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-4 flex items-center">
          <span class="text-2xl mr-2">📋</span>
          Рекомендации
        </h3>

        <div class="space-y-3">
          <div v-for="(rec, idx) in analytics.recommendations" :key="idx" class="flex items-start gap-3">
            <div class="w-6 h-6 rounded-full bg-primary-100 dark:bg-primary-900 flex items-center justify-center flex-shrink-0 mt-0.5">
              <span class="text-primary-600 dark:text-primary-400 text-sm font-bold">{{ idx + 1 }}</span>
            </div>
            <p class="text-gray-700 dark:text-gray-300 text-sm">{{ rec }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { analyticsApi } from '@/api/services'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const isLoading = ref(true)
const error = ref(null)
const analytics = ref({
  weakAreas: [],
  strengths: [],
  recommendations: [],
  estimatedCompletionDays: 0,
  aiInsight: ''
})
const nextModule = ref(null)

onMounted(async () => {
  try {
    isLoading.value = true
    error.value = null

    const userId = authStore.currentUser?.userId
    if (!userId) {
      error.value = 'Пользователь не найден'
      return
    }

    const dashboardRes = await analyticsApi.getDashboard(userId)
    analytics.value = dashboardRes.data.analytics
    nextModule.value = dashboardRes.data.nextModule
  } catch (err) {
    console.error('Error loading analytics:', err)
    error.value = 'Ошибка при загрузке анализа. Попробуйте позже.'
  } finally {
    isLoading.value = false
  }
})

const startModule = (moduleId) => {
  router.push(`/module/${moduleId}`)
}
</script>
