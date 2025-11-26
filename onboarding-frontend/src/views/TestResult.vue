<template>
  <div class="space-y-6">
    <!-- Loading State -->
    <div v-if="isLoading" class="card text-center py-12">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      <p class="mt-4 text-gray-600">Загрузка результатов...</p>
    </div>

    <!-- Results -->
    <div v-else-if="result" class="space-y-6">
      <!-- Result Summary -->
      <div
        class="card"
        :class="{
          'bg-green-50 border-2 border-green-200': result.isPassed,
          'bg-red-50 border-2 border-red-200': !result.isPassed
        }"
      >
        <div class="text-center">
          <div
            class="inline-flex items-center justify-center w-20 h-20 rounded-full mb-4"
            :class="{
              'bg-green-500': result.isPassed,
              'bg-red-500': !result.isPassed
            }"
          >
            <span class="text-white text-4xl font-bold">
              {{ result.isPassed ? '✓' : '✗' }}
            </span>
          </div>
          <h1
            class="text-3xl font-bold mb-2"
            :class="{
              'text-green-700': result.isPassed,
              'text-red-700': !result.isPassed
            }"
          >
            {{ result.isPassed ? 'Тест сдан!' : 'Тест не сдан' }}
          </h1>
          <p class="text-xl font-semibold text-gray-700 mb-4">
            Ваш результат: {{ Math.round(result.score) }}%
          </p>
          <div class="flex justify-center space-x-6 text-sm text-gray-600">
            <span>Правильных ответов: {{ result.correctAnswers }} / {{ result.totalQuestions }}</span>
            <span>Попытка: {{ result.attemptNumber }}</span>
          </div>
        </div>
      </div>

      <!-- Question Results -->
      <div v-if="result.questionResults && result.questionResults.length > 0" class="card">
        <h2 class="text-xl font-semibold text-gray-900 mb-4">Детали результатов</h2>
        <div class="space-y-4">
          <div
            v-for="(questionResult, index) in result.questionResults"
            :key="questionResult.questionId"
            class="p-4 rounded-lg border-2"
            :class="{
              'bg-green-50 border-green-200': questionResult.isCorrect,
              'bg-red-50 border-red-200': !questionResult.isCorrect
            }"
          >
            <div class="flex items-start justify-between mb-2">
              <h3 class="font-semibold text-gray-900">
                Вопрос {{ index + 1 }}: {{ questionResult.questionText }}
              </h3>
              <span
                class="px-3 py-1 rounded-full text-sm font-medium"
                :class="{
                  'bg-green-200 text-green-800': questionResult.isCorrect,
                  'bg-red-200 text-red-800': !questionResult.isCorrect
                }"
              >
                {{ questionResult.isCorrect ? 'Правильно' : 'Неправильно' }}
              </span>
            </div>
            <div v-if="!questionResult.isCorrect" class="mt-2 text-sm text-gray-600">
              <p class="text-red-600">Ваш ответ был неправильным</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div class="card bg-gray-50">
        <div class="flex items-center justify-between">
          <div>
            <p v-if="result.canRetry" class="text-gray-700 mb-2">
              У вас осталось {{ result.remainingAttempts }} попыток
            </p>
            <p v-else-if="!result.isPassed" class="text-red-600 font-medium">
              Все попытки исчерпаны. Свяжитесь с наставником или HR.
            </p>
            <p v-else class="text-green-600 font-medium">
              Поздравляем! Модуль успешно пройден.
            </p>
          </div>
          <div class="flex space-x-3">
            <button
              v-if="result.canRetry"
              @click="retryTest"
              class="btn-primary"
            >
              Попробовать снова
            </button>
            <button
              @click="goToModule"
              class="btn-secondary"
            >
              Вернуться к модулю
            </button>
            <button
              @click="goToDashboard"
              class="btn-primary"
            >
              На главную
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Error State -->
    <div v-else class="card text-center py-12">
      <p class="text-red-600 mb-4">Не удалось загрузить результаты</p>
      <button @click="goToDashboard" class="btn-secondary">
        На главную
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { testAttemptsApi } from '../api/services'

const route = useRoute()
const router = useRouter()

const result = ref(null)
const isLoading = ref(true)

onMounted(async () => {
  await loadResult()
})

const loadResult = async () => {
  try {
    isLoading.value = true
    
    // Try to get full result from sessionStorage first (from submit)
    const storedResult = sessionStorage.getItem(`testResult_${route.params.attemptId}`)
    if (storedResult) {
      result.value = JSON.parse(storedResult)
      sessionStorage.removeItem(`testResult_${route.params.attemptId}`)
    } else {
      // Fallback: get basic attempt info from API
      const response = await testAttemptsApi.getAttempt(route.params.attemptId)
      result.value = {
        ...response.data,
        questionResults: [] // Question details not available from this endpoint
      }
    }
  } catch (error) {
    console.error('Failed to load result:', error)
  } finally {
    isLoading.value = false
  }
}

const retryTest = () => {
  if (result.value) {
    router.push(`/module/${result.value.moduleId}/test`)
  }
}

const goToModule = () => {
  if (result.value) {
    router.push(`/module/${result.value.moduleId}`)
  } else {
    router.push('/')
  }
}

const goToDashboard = () => {
  router.push('/')
}
</script>

