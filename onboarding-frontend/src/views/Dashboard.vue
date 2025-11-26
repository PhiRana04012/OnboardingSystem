<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex justify-between items-center">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Мой онбординг</h1>
        <p class="mt-1 text-sm text-gray-600">
          Добро пожаловать, {{ authStore.currentUser?.fullName }}
        </p>
      </div>
      <div v-if="authStore.isMentor || authStore.isHR || authStore.isManager" class="text-right">
        <router-link
          :to="`/reports?userId=${authStore.currentUser?.userId}`"
          class="btn-secondary"
        >
          Просмотр отчётов
        </router-link>
      </div>
    </div>

    <!-- Progress Card -->
    <div v-if="progressStore.userProgress" class="card">
      <div class="mb-4">
        <div class="flex justify-between items-center mb-2">
          <h2 class="text-xl font-semibold text-gray-900">Прогресс онбординга</h2>
          <span class="text-2xl font-bold text-primary-600">
            {{ Math.round(progressStore.userProgress.progressPercentage) }}%
          </span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-3 mb-4">
          <div
            class="bg-primary-600 h-3 rounded-full transition-all duration-500"
            :style="{ width: `${progressStore.userProgress.progressPercentage}%` }"
          ></div>
        </div>
        <div class="flex justify-between text-sm text-gray-600">
          <span>
            Завершено обязательных модулей: {{ progressStore.userProgress.completedMandatoryModules }} / 
            {{ progressStore.userProgress.totalMandatoryModules }}
          </span>
          <span>
            Всего модулей: {{ progressStore.userProgress.completedModules }} / 
            {{ progressStore.userProgress.totalModules }}
          </span>
        </div>
      </div>

      <div class="mt-4 pt-4 border-t border-gray-200">
        <p class="text-sm text-gray-600">
          <span class="font-medium">Статус:</span> {{ progressStore.userProgress.onboardingStatus }}
        </p>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="progressStore.isLoading" class="card text-center py-12">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      <p class="mt-4 text-gray-600">Загрузка прогресса...</p>
    </div>

    <!-- Modules List -->
    <div v-if="progressStore.userProgress" class="space-y-4">
      <h2 class="text-2xl font-semibold text-gray-900">Модули</h2>
      
      <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <div
          v-for="module in progressStore.userProgress.modules"
          :key="module.moduleId"
          class="card hover:shadow-lg transition-shadow cursor-pointer"
          @click="openModule(module)"
        >
          <div class="flex items-start justify-between mb-3">
            <div class="flex-1">
              <h3 class="text-lg font-semibold text-gray-900 mb-1">
                {{ module.moduleTitle }}
              </h3>
              <span
                v-if="module.isMandatory"
                class="inline-block px-2 py-1 text-xs font-medium bg-red-100 text-red-800 rounded"
              >
                Обязательный
              </span>
              <span
                v-else
                class="inline-block px-2 py-1 text-xs font-medium bg-primary-50 text-primary-700 rounded"
              >
                Рекомендуемый
              </span>
            </div>
          </div>

          <div class="mt-4 space-y-2">
            <div class="flex items-center justify-between text-sm">
              <span class="text-gray-600">Статус:</span>
              <span
                :class="{
                  'text-green-600 font-medium': module.status === 'Завершён',
                  'text-yellow-600 font-medium': module.status === 'В процессе',
                  'text-gray-600': module.status === 'Не начат'
                }"
              >
                {{ module.status }}
              </span>
            </div>

            <div v-if="module.attemptsCount > 0" class="flex items-center justify-between text-sm">
              <span class="text-gray-600">Попыток:</span>
              <span class="font-medium">{{ module.attemptsCount }}</span>
            </div>

            <div v-if="module.bestScore !== null" class="flex items-center justify-between text-sm">
              <span class="text-gray-600">Лучший результат:</span>
              <span
                :class="{
                  'text-green-600 font-medium': module.isPassed,
                  'text-red-600 font-medium': !module.isPassed
                }"
              >
                {{ Math.round(module.bestScore) }}%
              </span>
            </div>
          </div>

          <div class="mt-4 pt-4 border-t border-gray-200">
            <button
              v-if="module.status === 'Не начат' || module.status === 'В процессе'"
              @click.stop="openModule(module)"
              class="w-full btn-primary text-sm"
            >
              {{ module.status === 'Не начат' ? 'Начать модуль' : 'Продолжить' }}
            </button>
            <button
              v-else-if="module.status === 'Завершён'"
              @click.stop="openModule(module)"
              class="w-full btn-secondary text-sm"
            >
              Просмотреть
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="!progressStore.isLoading && !progressStore.userProgress" class="card text-center py-12">
      <p class="text-gray-600">Не удалось загрузить прогресс</p>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useProgressStore } from '../stores/progress'

const router = useRouter()
const authStore = useAuthStore()
const progressStore = useProgressStore()

onMounted(async () => {
  if (authStore.currentUser) {
    try {
      await progressStore.fetchUserProgress(authStore.currentUser.userId)
    } catch (error) {
      console.error('Failed to fetch progress:', error)
    }
  }
})

const openModule = (module) => {
  router.push(`/module/${module.moduleId}`)
}
</script>



