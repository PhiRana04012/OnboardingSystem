<template>
  <div class="space-y-6 p-6">
    <!-- Header -->
    <div class="bg-gradient-to-r from-primary-50 to-blue-50 dark:from-primary-900/30 dark:to-blue-900/30 rounded-lg p-6 border-l-4 border-primary-500">
      <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">🗺️ Ваш персональный путь обучения</h2>
      <p class="text-gray-700 dark:text-gray-300 mb-4">
        Система создала оптимальный план на основе анализа ИИ вашего прогресса и должности
      </p>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <p class="text-sm text-gray-600 dark:text-gray-400">Стратегия</p>
          <p class="text-lg font-bold text-gray-900 dark:text-white">
            {{ strategyLabel(learningPath.strategy) }}
          </p>
        </div>
        <div>
          <p class="text-sm text-gray-600 dark:text-gray-400">Осталось модулей</p>
          <p class="text-lg font-bold text-gray-900 dark:text-white">{{ learningPath.plannedOrder.length }}</p>
        </div>
        <div>
          <p class="text-sm text-gray-600 dark:text-gray-400">Прогноз завершения</p>
          <p class="text-lg font-bold text-primary-600 dark:text-primary-400">
            ~{{ learningPath.estimatedCompletionDays.toFixed(0) }} дней
          </p>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex items-center justify-center h-96">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto mb-4"></div>
        <p class="text-gray-600 dark:text-gray-400">Создаём ваш план обучения...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4 text-red-700 dark:text-red-400">
      <p>{{ error }}</p>
    </div>

    <!-- Modules List -->
    <div v-else class="space-y-4">
      <div v-for="(module, idx) in learningPath.plannedOrder" :key="module.moduleId" class="bg-white dark:bg-gray-800 rounded-lg shadow hover:shadow-lg transition-shadow overflow-hidden">
        <!-- Module Header -->
        <div class="flex items-start gap-4 p-6">
          <!-- Day Number Badge -->
          <div class="w-12 h-12 rounded-lg bg-gradient-to-br from-primary-500 to-primary-600 flex items-center justify-center flex-shrink-0">
            <span class="text-white font-bold">День {{ module.dayNumber }}</span>
          </div>

          <!-- Module Info -->
          <div class="flex-1">
            <div class="flex items-center gap-3 mb-2">
              <h3 class="text-lg font-bold text-gray-900 dark:text-white">{{ module.moduleTitle }}</h3>
              <span class="px-3 py-1 bg-primary-100 dark:bg-primary-900 text-primary-700 dark:text-primary-300 rounded-full text-xs font-semibold">
                Приоритет {{ module.priority }}/10
              </span>
            </div>

            <p class="text-gray-600 dark:text-gray-400 text-sm mb-3">{{ module.reason }}</p>

            <!-- Module Stats -->
            <div class="flex items-center gap-4 text-xs text-gray-500 dark:text-gray-400">
              <span>⏱️ {{ module.estimatedHours }} часов</span>
              <span v-if="module.dependsOn">
                📚 Зависит от: {{ module.dependsOn }}
              </span>
            </div>
          </div>

          <!-- Priority Bar -->
          <div class="flex-shrink-0">
            <div class="w-1 h-12 rounded bg-gradient-to-b from-primary-500 to-primary-600 opacity-75"></div>
          </div>

          <!-- Action Button -->
          <div class="flex flex-col gap-2 flex-shrink-0">
            <button
              v-if="!module.isCompleted"
              @click="startModule(module.moduleId)"
              class="px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg font-medium transition-colors text-sm whitespace-nowrap"
            >
              Начать
            </button>
            <button
              v-else
              disabled
              class="px-4 py-2 bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400 rounded-lg font-medium text-sm whitespace-nowrap cursor-default"
            >
              ✓ Завершен
            </button>
          </div>
        </div>

        <!-- Expand Details -->
        <div v-if="expandedModules[module.moduleId]" class="bg-gray-50 dark:bg-gray-700/50 px-6 py-4 border-t border-gray-200 dark:border-gray-700">
          <p class="text-gray-700 dark:text-gray-300 text-sm leading-relaxed">
            {{ getModuleDetails(module.moduleId) }}
          </p>
        </div>
      </div>
    </div>

    <!-- AI Explanation -->
    <div v-if="!isLoading && !error" class="bg-gradient-to-r from-violet-50 to-purple-50 dark:from-violet-900/30 dark:to-purple-900/30 rounded-lg shadow p-6 border-l-4 border-violet-500">
      <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-3 flex items-center">
        <span class="text-2xl mr-2">🤖</span>
        Почему именно такой порядок?
      </h3>
      <p class="text-gray-700 dark:text-gray-300 text-sm leading-relaxed">
        {{ learningPath.aiExplanation }}
      </p>
    </div>

    <!-- Strategy Guide -->
    <div v-if="!isLoading && !error" class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-4">
        <h4 class="font-semibold text-gray-900 dark:text-white mb-2">⚡ Быстрый трек (Fast-track)</h4>
        <p class="text-sm text-gray-600 dark:text-gray-400">Только критичные модули для вашей роли. Минимум времени.</p>
      </div>
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-4">
        <h4 class="font-semibold text-gray-900 dark:text-white mb-2">⚖️ Сбалансированный (Balanced)</h4>
        <p class="text-sm text-gray-600 dark:text-gray-400">Критичное + остальное. Оптимальное время и полнота.</p>
      </div>
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-4">
        <h4 class="font-semibold text-gray-900 dark:text-white mb-2">📚 Углубленный (Deep-learning)</h4>
        <p class="text-sm text-gray-600 dark:text-gray-400">Все модули подробно. Глубокие знания всех аспектов.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { analyticsApi } from '@/api/services'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const isLoading = ref(true)
const error = ref(null)
const expandedModules = reactive({})

const learningPath = ref({
  plannedOrder: [],
  strategy: 'balanced',
  aiExplanation: '',
  estimatedCompletionDays: 0
})

onMounted(async () => {
  try {
    isLoading.value = true
    error.value = null

    const userId = authStore.currentUser?.userId
    if (!userId) {
      error.value = 'Пользователь не найден'
      return
    }

    const res = await analyticsApi.getLearningPath(userId, 'balanced')
    learningPath.value = res.data
  } catch (err) {
    console.error('Error loading learning path:', err)
    error.value = 'Ошибка при загрузке плана обучения. Попробуйте позже.'
  } finally {
    isLoading.value = false
  }
})

const startModule = (moduleId) => {
  router.push(`/module/${moduleId}`)
}

const strategyLabel = (strategy) => {
  const labels = {
    'fast-track': '⚡ Быстрый трек',
    'balanced': '⚖️ Сбалансированный',
    'deep-learning': '📚 Углубленный'
  }
  return labels[strategy] || strategy
}

const getModuleDetails = (moduleId) => {
  return 'Детальная информация о модуле можно увидеть при открытии модуля.'
}

const toggleExpand = (moduleId) => {
  expandedModules[moduleId] = !expandedModules[moduleId]
}
</script>
