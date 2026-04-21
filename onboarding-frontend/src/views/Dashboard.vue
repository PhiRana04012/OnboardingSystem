<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:justify-between md:items-center gap-4">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Мой онбординг</h1>
        <p class="mt-1 text-gray-600">
          Рады видеть тебя снова, <span class="font-bold text-primary-600">{{ authStore.currentUser?.fullName }}</span>!
        </p>
      </div>
      <div v-if="authStore.isMentor || authStore.isHR || authStore.isManager">
        <router-link to="/reports" class="btn-secondary flex items-center gap-2">
          📊 Просмотр отчётов
        </router-link>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="progressStore.isLoading" class="card text-center py-12">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      <p class="mt-4 text-gray-600 font-medium">Загружаем твой прогресс...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="progressStore.error" class="card border-l-4 border-red-500 bg-red-50 text-red-700 p-6">
      {{ progressStore.error }}
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
