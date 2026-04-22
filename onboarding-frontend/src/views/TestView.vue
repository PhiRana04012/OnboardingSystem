<template>
  <div class="space-y-6">
    <!-- Loading State -->
    <div v-if="isLoading" class="card text-center py-12">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      <p class="mt-4 text-gray-600 dark:text-gray-400">Загрузка теста...</p>
    </div>

    <!-- Test Form -->
    <div v-else-if="questions.length > 0" class="space-y-6">
      <!-- Header -->
      <div class="card bg-primary-50 dark:bg-primary-900/20 border border-primary-200 dark:border-primary-800">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">Тест по модулю</h1>
            <p class="text-gray-600 dark:text-gray-400">
              Ответьте на все вопросы. Проходной балл: {{ passingScore }}%
            </p>
          </div>
          <div class="text-right">
            <p class="text-sm text-gray-600 dark:text-gray-400">Вопросов: {{ questions.length }}</p>
            <p class="text-sm font-medium" :class="remainingAttempts === 0 ? 'text-red-600 dark:text-red-400' : 'text-gray-600 dark:text-gray-400'">
              Осталось попыток: {{ remainingAttempts }}
            </p>
          </div>
        </div>
      </div>

      <!-- Questions Container -->
      <form @submit.prevent="submitTest" class="space-y-6 relative overflow-hidden">
        <transition name="slide-fade" mode="out-in">
          <div
            v-if="currentQuestion"
            :key="currentQuestion.questionId"
            class="card"
          >
            <div class="mb-4">
              <h3 class="text-lg font-semibold text-gray-900 mb-1">
                Вопрос {{ currentQuestionIndex + 1 }} из {{ questions.length }}
              </h3>
              <p class="text-gray-700">{{ currentQuestion.questionText }}</p>
            </div>

            <div class="space-y-3">
              <label
                v-for="answer in currentQuestion.answerOptions"
                :key="answer.answerId"
                class="flex items-start p-4 border-2 rounded-lg cursor-pointer transition-all"
                :class="{
                  'border-primary-500 bg-primary-50': selectedAnswers[currentQuestion.questionId] === answer.answerId,
                  'border-gray-200 hover:border-gray-300': selectedAnswers[currentQuestion.questionId] !== answer.answerId
                }"
              >
                <input
                  type="radio"
                  :name="`question-${currentQuestion.questionId}`"
                  :value="answer.answerId"
                  v-model="selectedAnswers[currentQuestion.questionId]"
                  :disabled="remainingAttempts === 0"
                  class="mt-1 mr-3 h-4 w-4 text-primary-600 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
                />
                <span class="flex-1 text-gray-700">{{ answer.answerText }}</span>
              </label>
            </div>
            
            <!-- Navigation -->
            <div class="mt-8 flex items-center justify-between border-t pt-4">
               <div>
                  <button type="button" @click="prevQuestion" :disabled="currentQuestionIndex === 0" class="btn-secondary disabled:opacity-50 disabled:cursor-not-allowed">
                     &larr; Назад
                  </button>
               </div>
               <div>
                  <button 
                     v-if="currentQuestionIndex < questions.length - 1" 
                     type="button" 
                     @click="nextQuestion" 
                     :disabled="!selectedAnswers[currentQuestion.questionId]" 
                     class="btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                     Далее &rarr;
                  </button>
                  <button 
                     v-else 
                     type="submit" 
                     :disabled="!allAnswered || isSubmitting || remainingAttempts === 0" 
                     class="btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                     {{ isSubmitting ? 'Отправка...' : 'Завершить тест' }}
                  </button>
               </div>
            </div>
          </div>
        </transition>

        <!-- Attempt Limit Warning -->
        <div v-if="remainingAttempts === 0" class="mt-4 px-4 py-3 bg-red-50 text-red-700 rounded text-center font-medium shadow">
          У вас больше не осталось попыток для сдачи этого теста.
        </div>
      </form>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="card text-center py-12">
      <p class="text-red-600 mb-4">{{ error }}</p>
      <button @click="$router.back()" class="btn-secondary">
        Вернуться к модулю
      </button>
    </div>

    <!-- No Questions -->
    <div v-else class="card text-center py-12">
      <p class="text-gray-600 mb-4">Вопросы не найдены</p>
      <button @click="$router.back()" class="btn-secondary">
        Вернуться к модулю
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { questionsApi, testAttemptsApi, modulesApi } from '../api/services'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const questions = ref([])
const selectedAnswers = ref({})
const currentQuestionIndex = ref(0)
const isLoading = ref(true)
const isSubmitting = ref(false)
const error = ref('')
const moduleInfo = ref(null)
const remainingAttempts = ref(3)

onMounted(async () => {
  await loadModuleInfo()
  await loadQuestions()
  await loadAttempts()
})

const loadModuleInfo = async () => {
  try {
    const response = await modulesApi.getById(route.params.id)
    moduleInfo.value = response.data
  } catch (err) {
    console.error('Failed to load module info:', err)
  }
}

const loadQuestions = async () => {
  try {
    isLoading.value = true
    error.value = ''
    const response = await questionsApi.getForTest(route.params.id)
    questions.value = response.data
    
    // Initialize selected answers
    questions.value.forEach(q => {
      selectedAnswers.value[q.questionId] = null
    })
  } catch (err) {
    error.value = 'Не удалось загрузить вопросы теста'
    console.error('Failed to load questions:', err)
  } finally {
    isLoading.value = false
  }
}

const loadAttempts = async () => {
  if (!authStore.currentUser) return
  try {
    const response = await testAttemptsApi.getModuleAttempts(
      parseInt(route.params.id),
      authStore.currentUser.userId
    )
    const attempts = response.data
    if (moduleInfo.value) {
      remainingAttempts.value = Math.max(0, moduleInfo.value.maxAttempts - attempts.length)
    }
  } catch (err) {
    console.error('Failed to load attempts:', err)
  }
}

const passingScore = computed(() => {
  return moduleInfo.value?.passingScore || 80
})

const currentQuestion = computed(() => {
  if (questions.value.length === 0) return null
  return questions.value[currentQuestionIndex.value]
})

const allAnswered = computed(() => {
  return questions.value.every(q => {
    const ans = selectedAnswers.value[q.questionId]
    return ans !== null && ans !== undefined
  })
})

const prevQuestion = () => {
  if (currentQuestionIndex.value > 0) {
    currentQuestionIndex.value--
  }
}

const nextQuestion = () => {
  if (currentQuestionIndex.value < questions.value.length - 1) {
    currentQuestionIndex.value++
  }
}

const submitTest = async () => {
  if (!authStore.currentUser) {
    router.push('/login')
    return
  }

  // Check if all questions have a selected answer
  console.log('Validating answers...');
  console.log('Selected Answers:', JSON.parse(JSON.stringify(selectedAnswers.value)));

  if (!allAnswered.value) {
    console.warn('Validation failed: not all questions answered');
    alert('Пожалуйста, ответьте на все вопросы')
    return
  }

  try {
    isSubmitting.value = true
    error.value = ''

    const answers = Object.entries(selectedAnswers.value).map(([questionId, answerId]) => ({
      questionId: parseInt(questionId),
      answerId: parseInt(answerId)
    }))

    const response = await testAttemptsApi.submitTest({
      userId: authStore.currentUser.userId,
      moduleId: parseInt(route.params.id),
      answers
    })

    // Store result in sessionStorage for TestResult page
    sessionStorage.setItem(`testResult_${response.data.attemptId}`, JSON.stringify(response.data))

    // Navigate to results page
    router.push(`/test-result/${response.data.attemptId}`)
  } catch (err) {
    if (err.response?.status === 400) {
      error.value = err.response.data || 'Ошибка при отправке теста'
    } else {
      error.value = 'Не удалось отправить тест. Попробуйте снова.'
    }
    console.error('Failed to submit test:', err)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
.slide-fade-enter-active {
  transition: all 0.3s ease-out;
}
.slide-fade-leave-active {
  transition: all 0.2s cubic-bezier(1, 0.5, 0.8, 1);
}
.slide-fade-enter-from {
  transform: translateX(20px);
  opacity: 0;
}
.slide-fade-leave-to {
  transform: translateX(-20px);
  opacity: 0;
}
</style>

