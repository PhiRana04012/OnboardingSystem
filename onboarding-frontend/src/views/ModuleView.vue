<template>
  <div class="space-y-6">
    <!-- Loading State -->
    <div v-if="isLoading" class="card text-center py-12">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      <p class="mt-4 text-gray-600">Загрузка модуля...</p>
    </div>

    <!-- Module Content -->
    <div v-else-if="module" class="space-y-6">
      <!-- Header -->
      <div class="flex items-start justify-between">
        <div>
          <div class="flex items-center space-x-3 mb-2">
            <button
              @click="$router.back()"
              class="text-gray-500 hover:text-gray-700 transition-colors"
            >
              ← Назад
            </button>
            <h1 class="text-3xl font-bold text-gray-900">{{ module.title }}</h1>
          </div>
          <p v-if="module.description" class="text-gray-600 mt-2">{{ module.description }}</p>
          <div class="flex items-center space-x-2 mt-3">
            <span
              v-if="module.isMandatory"
              class="px-3 py-1 text-sm font-medium bg-red-100 text-red-800 rounded-full"
            >
              Обязательный модуль
            </span>
            <span
              v-else
              class="px-3 py-1 text-sm font-medium bg-primary-50 text-primary-700 rounded-full"
            >
              Рекомендуемый модуль
            </span>
            <span class="px-3 py-1 text-sm font-medium bg-gray-100 text-gray-800 rounded-full">
              Проходной балл: {{ module.passingScore }}%
            </span>
          </div>
        </div>
      </div>

      <!-- Content -->
      <div class="card">
        <div
          class="prose max-w-none"
          v-html="formatContent(module.content)"
        ></div>
      </div>

      <!-- Actions -->
      <div class="card bg-gray-50">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">
              После ознакомления с материалом вы можете пройти тест
            </p>
            <p v-if="module.questionCount > 0" class="text-sm text-gray-500 mt-1">
              В тесте будет {{ module.questionCount }} вопросов
            </p>
          </div>
          <div class="flex space-x-3">
            <button
              v-if="!hasTest"
              @click="markAsRead"
              :disabled="isMarking"
              class="btn-primary disabled:opacity-50"
            >
              {{ isMarking ? 'Сохранение...' : 'Ознакомлен' }}
            </button>
            <button
              v-if="module.questionCount > 0"
              @click="startTest"
              class="btn-primary"
            >
              Пройти тест
            </button>
          </div>
        </div>
      </div>

      <!-- Previous Attempts -->
      <div v-if="previousAttempts.length > 0" class="card">
        <h2 class="text-xl font-semibold text-gray-900 mb-4">Предыдущие попытки</h2>
        <div class="space-y-3">
          <div
            v-for="attempt in previousAttempts"
            :key="attempt.attemptId"
            class="flex items-center justify-between p-4 bg-gray-50 rounded-lg"
          >
            <div>
              <p class="font-medium text-gray-900">
                Попытка {{ attempt.attemptNumber }}
              </p>
              <p class="text-sm text-gray-600">
                {{ formatDate(attempt.attemptDate) }}
              </p>
            </div>
            <div class="text-right">
              <p
                :class="{
                  'text-green-600 font-bold': attempt.isPassed,
                  'text-red-600 font-bold': !attempt.isPassed
                }"
              >
                {{ Math.round(attempt.score) }}%
              </p>
              <p
                :class="{
                  'text-green-600 text-sm': attempt.isPassed,
                  'text-red-600 text-sm': !attempt.isPassed
                }"
              >
                {{ attempt.isPassed ? 'Сдано' : 'Не сдано' }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Error State -->
    <div v-else class="card text-center py-12">
      <p class="text-red-600">Модуль не найден</p>
      <button @click="$router.back()" class="mt-4 btn-secondary">
        Вернуться назад
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useProgressStore } from '../stores/progress'
import { modulesApi, testAttemptsApi } from '../api/services'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const progressStore = useProgressStore()

const module = ref(null)
const isLoading = ref(true)
const isMarking = ref(false)
const previousAttempts = ref([])

const hasTest = computed(() => {
  if (!module.value) return false
  return module.value.questionCount > 0
})

onMounted(async () => {
  await loadModule()
  await loadAttempts()
})

const loadModule = async () => {
  try {
    isLoading.value = true
    const response = await modulesApi.getById(route.params.id)
    module.value = response.data
  } catch (error) {
    console.error('Failed to load module:', error)
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
    previousAttempts.value = response.data
  } catch (error) {
    console.error('Failed to load attempts:', error)
  }
}

const markAsRead = async () => {
  if (!authStore.currentUser || !module.value) return
  try {
    isMarking.value = true
    await progressStore.markModuleAsRead(
      authStore.currentUser.userId,
      module.value.moduleId
    )
    // Refresh progress
    await progressStore.fetchUserProgress(authStore.currentUser.userId)
  } catch (error) {
    console.error('Failed to mark as read:', error)
    alert('Ошибка при сохранении. Попробуйте снова.')
  } finally {
    isMarking.value = false
  }
}

const startTest = () => {
  router.push(`/module/${route.params.id}/test`)
}

const formatContent = (content) => {
  if (!content) return ''
  // Simple formatting - in production, use a proper markdown/HTML renderer
  return content.replace(/\n/g, '<br>')
}

const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleString('ru-RU', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>



