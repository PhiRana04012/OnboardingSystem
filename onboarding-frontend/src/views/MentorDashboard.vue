<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between">
      <h1 class="text-3xl font-bold text-gray-900">Мои подопечные</h1>
      <button @click="loadMentees" class="btn-secondary" :disabled="isLoading">
        Обновить
      </button>
    </div>

    <!-- Общая статистика -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">
      <div class="card bg-gradient-to-br from-primary-50 to-white">
        <h3 class="text-sm font-medium text-gray-500">Всего подопечных</h3>
        <p class="text-3xl font-bold text-primary-700 mt-2">{{ mentees.length }}</p>
      </div>
      <div class="card bg-gradient-to-br from-yellow-50 to-white">
        <h3 class="text-sm font-medium text-gray-500">В процессе</h3>
        <p class="text-3xl font-bold text-yellow-600 mt-2">{{ inProcessCount }}</p>
      </div>
      <div class="card bg-gradient-to-br from-green-50 to-white">
        <h3 class="text-sm font-medium text-gray-500">Завершили</h3>
        <p class="text-3xl font-bold text-green-600 mt-2">{{ completedCount }}</p>
      </div>
    </div>

    <div v-if="isLoading" class="text-center py-12">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      <p class="mt-4 text-gray-600">Загрузка информации...</p>
    </div>

    <div v-else-if="mentees.length === 0" class="card text-center py-12 bg-white">
      <div class="text-6xl mb-4">👥</div>
      <h3 class="text-lg font-medium text-gray-900">У вас пока нет подопечных</h3>
      <p class="text-gray-500 mt-2">Как только новичок будет прикреплен к вам, он появится здесь.</p>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="mentee in mentees" :key="mentee.userId" class="card hover:shadow-lg transition-shadow relative overflow-hidden flex flex-col">
        <!-- Badge Status -->
        <div class="absolute top-0 right-0 px-3 py-1 rounded-bl-lg text-xs font-bold"
             :class="{
               'bg-green-100 text-green-800': mentee.onboardingStatus === 'Завершён',
               'bg-yellow-100 text-yellow-800': mentee.onboardingStatus === 'В процессе',
               'bg-gray-100 text-gray-800': mentee.onboardingStatus === 'Не начат'
             }">
          {{ mentee.onboardingStatus }}
        </div>

        <div class="flex items-center gap-4 mb-4 mt-2">
          <div class="w-12 h-12 rounded-full flex items-center justify-center font-bold text-lg text-white shadow-sm"
               :style="{ background: getAvatarGradient(mentee.fullName) }">
            {{ getInitials(mentee.fullName) }}
          </div>
          <div>
            <h3 class="font-bold text-gray-900 line-clamp-1">{{ mentee.fullName }}</h3>
            <p class="text-xs text-gray-500 line-clamp-1">{{ mentee.jobTitle || 'Должность не указана' }}</p>
          </div>
        </div>

        <div class="flex-1 space-y-3 mb-4 text-sm text-gray-600">
          <div class="flex items-center gap-2">
            <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"></path></svg>
            <span class="truncate">{{ mentee.departmentName }}</span>
          </div>
          <div class="flex items-center gap-2">
            <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path></svg>
            <span>Нанят: {{ formatDate(mentee.hireDate) }}</span>
          </div>
          <div v-if="mentee.telegramTag" class="flex items-center gap-2">
            <svg class="w-4 h-4 text-blue-400" fill="currentColor" viewBox="0 0 24 24"><path d="M11.944 0A12 12 0 000 12a12 12 0 0012 12 12 12 0 0012-12A12 12 0 0012 0a12 12 0 00-.056 0zm4.962 7.224c.1-.002.321.023.465.14a.506.506 0 01.171.325c.016.093.036.306.02.472-.18 1.898-.962 6.502-1.36 8.627-.168.9-.499 1.201-.82 1.23-.696.065-1.225-.46-1.9-.902-1.056-.693-1.653-1.124-2.678-1.8-1.185-.78-.417-1.21.258-1.91.177-.184 3.247-2.977 3.307-3.23.007-.032.014-.15-.056-.212s-.174-.041-.249-.024c-.106.024-1.793 1.14-5.061 3.345-.48.33-.913.49-1.302.48-.428-.008-1.252-.241-1.865-.44-.752-.245-1.349-.374-1.297-.789.027-.216.325-.437.892-.664 3.498-1.524 5.83-2.529 6.998-3.014 3.332-1.386 4.025-1.627 4.476-1.635z"/></svg>
            <a :href="'https://t.me/' + mentee.telegramTag" target="_blank" class="text-blue-600 hover:underline">@{{ mentee.telegramTag }}</a>
          </div>
        </div>

        <!-- Gamification stats view -->
        <div class="mt-auto pt-4 border-t flex justify-between items-center bg-gray-50 -mx-6 -mb-6 p-4 rounded-b-xl">
           <div class="flex flex-col">
             <span class="text-xs text-gray-500 font-medium">Уровень</span>
             <span class="font-black text-primary-700 text-lg">{{ mentee.level }}</span>
           </div>
           <div class="flex flex-col text-right">
             <span class="text-xs text-gray-500 font-medium">Опыт</span>
             <span class="font-bold text-gray-800">{{ mentee.totalXP }} XP</span>
           </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { usersApi } from '../api/services'
import { useAuthStore } from '../stores/auth'
import { getAvatarGradient } from '../utils/avatar'

const authStore = useAuthStore()
const mentees = ref([])
const isLoading = ref(true)

const inProcessCount = computed(() => mentees.value.filter(m => m.onboardingStatus === 'В процессе').length)
const completedCount = computed(() => mentees.value.filter(m => m.onboardingStatus === 'Завершён').length)

onMounted(async () => {
  await loadMentees()
})

const loadMentees = async () => {
  if (!authStore.currentUser) return
  try {
    isLoading.value = true
    // Here we request the mentees using user's mentor ID
    const res = await usersApi.getMentees(authStore.currentUser.userId)
    mentees.value = res.data
  } catch (error) {
    console.error('Failed to load mentees:', error)
  } finally {
    isLoading.value = false
  }
}

const getInitials = (name) => {
  if (!name) return '?'
  return name.split(' ').slice(0, 2).map(n => n[0]).join('').toUpperCase()
}

const formatDate = (dateString) => {
  if (!dateString) return '—'
  const date = new Date(dateString)
  return date.toLocaleDateString('ru-RU', { year: 'numeric', month: 'long', day: 'numeric' })
}
</script>
