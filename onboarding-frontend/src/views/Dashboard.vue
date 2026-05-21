<template>
  <div class="space-y-8">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row md:justify-between md:items-start gap-6">
      <div class="flex-1">
        <h1 class="text-4xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 dark:from-gray-50 dark:to-gray-300 bg-clip-text text-transparent mb-2">Мой онбординг</h1>
        <p class="text-lg text-gray-600 dark:text-gray-400 leading-relaxed">
          Рады видеть тебя снова, <span class="font-semibold text-primary-600 dark:text-primary-400">{{ authStore.currentUser?.fullName }}</span>! 👋
        </p>
      </div>
      <div v-if="authStore.isMentor || authStore.isHR || authStore.isManager" class="flex-shrink-0">
        <router-link to="/reports" class="btn-secondary inline-flex items-center gap-2 whitespace-nowrap">
          <span>📊</span>
          <span>Просмотр отчётов</span>
        </router-link>
      </div>
    </div>

    <!-- Tabs -->
    <div class="flex gap-2 border-b border-gray-200 dark:border-gray-700">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        @click="activeTab = tab.id"
        :class="[
          'px-4 py-3 font-medium text-sm border-b-2 transition-colors whitespace-nowrap',
          activeTab === tab.id
            ? 'border-primary-600 text-primary-600 dark:text-primary-400'
            : 'border-transparent text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-300'
        ]"
      >
        {{ tab.label }}
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="progressStore.isLoading && activeTab === 'progress'" class="card text-center py-16">
      <div class="inline-block animate-spin rounded-full h-12 w-12 border-4 border-gray-200 dark:border-gray-700 border-t-primary-600 dark:border-t-primary-400"></div>
      <p class="mt-6 text-gray-600 dark:text-gray-400 font-medium text-lg">Загружаем твой прогресс...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="progressStore.error && activeTab === 'progress'" class="card border-l-4 border-red-500 dark:border-red-400 bg-gradient-to-r from-red-50 dark:from-red-900/20 to-orange-50 dark:to-orange-900/20 text-red-700 dark:text-red-300 p-6">
      <div class="flex gap-3 items-start">
        <span class="text-2xl flex-shrink-0">⚠️</span>
        <div>
          <p class="font-semibold">Ошибка при загрузке</p>
          <p class="text-sm mt-1">{{ progressStore.error }}</p>
        </div>
      </div>
    </div>

    <!-- Progress Map (Zigzag Content) -->
    <div v-if="progressStore.userProgress && activeTab === 'progress'">
      <ProgressMap
        :modules="progressStore.userProgress.modules"
        :progress-percentage="progressStore.userProgress.progressPercentage"
        :completed-mandatory="progressStore.userProgress.completedMandatoryModules"
        :total-mandatory="progressStore.userProgress.totalMandatoryModules"
        @open-module="openModule"
      />
    </div>

    <!-- Analytics Tab -->
    <div v-if="activeTab === 'analytics'">
      <AnalyticsDashboard />
    </div>

    <!-- Learning Path Tab -->
    <div v-if="activeTab === 'learning-path'">
      <LearningPath />
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useProgressStore } from '../stores/progress'
import ProgressMap from '../components/ProgressMap.vue'
import AnalyticsDashboard from '../components/AnalyticsDashboard.vue'
import LearningPath from '../components/LearningPath.vue'

const router = useRouter()
const authStore = useAuthStore()
const progressStore = useProgressStore()

const activeTab = ref('progress')

const tabs = [
  { id: 'progress', label: '📊 Мое обучение' },
  { id: 'analytics', label: '🔍 Аналитика' },
  { id: 'learning-path', label: '🗺️ План обучения' }
]

onMounted(async () => {
  if (authStore.currentUser) {
    await progressStore.fetchUserProgress(authStore.currentUser.userId)
  }
})

const openModule = (module) => {
  router.push(`/module/${module.moduleId}`)
}
</script>
