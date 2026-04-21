<template>
  <div class="space-y-8">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row md:justify-between md:items-start gap-6">
      <div class="flex-1">
        <h1 class="text-4xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent mb-2">Мой онбординг</h1>
        <p class="text-lg text-gray-600 leading-relaxed">
          Рады видеть тебя снова, <span class="font-semibold text-primary-600">{{ authStore.currentUser?.fullName }}</span>! 👋
        </p>
      </div>
      <div v-if="authStore.isMentor || authStore.isHR || authStore.isManager" class="flex-shrink-0">
        <router-link to="/reports" class="btn-secondary inline-flex items-center gap-2 whitespace-nowrap">
          <span>📊</span>
          <span>Просмотр отчётов</span>
        </router-link>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="progressStore.isLoading" class="card text-center py-16">
      <div class="inline-block animate-spin rounded-full h-12 w-12 border-4 border-gray-200 border-t-primary-600"></div>
      <p class="mt-6 text-gray-600 font-medium text-lg">Загружаем твой прогресс...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="progressStore.error" class="card border-l-4 border-red-500 bg-gradient-to-r from-red-50 to-orange-50 text-red-700 p-6">
      <div class="flex gap-3 items-start">
        <span class="text-2xl flex-shrink-0">⚠️</span>
        <div>
          <p class="font-semibold">Ошибка при загрузке</p>
          <p class="text-sm mt-1">{{ progressStore.error }}</p>
        </div>
      </div>
    </div>

    <!-- Progress Map (Zigzag Content) -->
    <div v-if="progressStore.userProgress">
      <ProgressMap
        :modules="progressStore.userProgress.modules"
        :progress-percentage="progressStore.userProgress.progressPercentage"
        :completed-mandatory="progressStore.userProgress.completedMandatoryModules"
        :total-mandatory="progressStore.userProgress.totalMandatoryModules"
        @open-module="openModule"
      />
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useProgressStore } from '../stores/progress'
import ProgressMap from '../components/ProgressMap.vue'

const router = useRouter()
const authStore = useAuthStore()
const progressStore = useProgressStore()

onMounted(async () => {
  if (authStore.currentUser) {
    await progressStore.fetchUserProgress(authStore.currentUser.userId)
  }
})

const openModule = (module) => {
  router.push(`/module/${module.moduleId}`)
}
</script>
