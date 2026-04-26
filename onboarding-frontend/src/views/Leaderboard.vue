<template>
  <div class="space-y-6">
    <section class="rounded-2xl p-6 md:p-8 text-white relative overflow-hidden bg-gradient-to-br from-indigo-600 via-primary-600 to-cyan-500 shadow-lg">
      <div class="absolute -right-8 -top-8 text-white/10 text-[140px] font-black leading-none select-none">★</div>
      <div class="relative z-10 flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
        <div>
          <h1 class="text-3xl md:text-4xl font-black">Таблица лидеров</h1>
          <p class="text-indigo-50 mt-2 text-sm md:text-base">
            Рейтинг сотрудников по опыту, уровню и активности в онбординге.
          </p>
        </div>
        <button
          @click="fetchLeaderboard"
          class="px-4 py-2 rounded-lg text-sm font-semibold bg-white/20 hover:bg-white/30 border border-white/30 transition-colors disabled:opacity-60"
          :disabled="isLoading"
        >
          {{ isLoading ? 'Обновляем...' : 'Обновить' }}
        </button>
      </div>
      <div class="relative z-10 mt-5 flex flex-wrap gap-3 text-xs">
        <span class="px-3 py-1 rounded-full bg-white/20">Всего участников: {{ leaderboard.length }}</span>
        <span class="px-3 py-1 rounded-full bg-white/20">После фильтра: {{ filteredLeaderboard.length }}</span>
      </div>
    </section>

    <div v-if="isLoading" class="card text-center py-10">
      <div class="inline-block animate-spin rounded-full h-10 w-10 border-4 border-gray-200 dark:border-gray-700 border-t-primary-600 dark:border-t-primary-400"></div>
      <p class="mt-4 text-gray-600 dark:text-gray-300">Загружаем рейтинг...</p>
    </div>

    <div v-else-if="error" class="card border-l-4 border-red-500 dark:border-red-400">
      <p class="font-semibold text-red-700 dark:text-red-300">Ошибка загрузки</p>
      <p class="text-sm text-red-600 dark:text-red-400 mt-1">{{ error }}</p>
    </div>

    <div v-else class="space-y-6">
      <div class="card bg-gradient-to-r from-white to-gray-50 dark:from-gray-800 dark:to-gray-900">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="md:col-span-2">
            <label class="block text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400 mb-1">Поиск сотрудника</label>
            <input
              v-model.trim="searchQuery"
              type="text"
              placeholder="Например: Иванов"
              class="input"
            />
          </div>
          <div>
            <label class="block text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400 mb-1">Подразделение</label>
            <select v-model="selectedDepartment" class="input">
              <option value="">Все подразделения</option>
              <option v-for="department in departments" :key="department" :value="department">
                {{ department }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <div v-if="topThree.length" class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <article
          v-for="user in topThree"
          :key="user.userId"
          class="rounded-xl p-5 border shadow-sm transition-transform hover:-translate-y-0.5"
          :class="topCardClass(user.rank)"
        >
          <div class="flex items-center justify-between">
            <span class="text-2xl">{{ getMedal(user.rank) }}</span>
            <span class="text-xs font-black px-2.5 py-1 rounded-full bg-white/70 text-gray-800">
              #{{ user.rank }}
            </span>
          </div>
          <div class="mt-4 flex items-center gap-3">
            <div
              class="w-12 h-12 rounded-full flex items-center justify-center text-white font-bold text-sm shadow"
              :style="{ background: getAvatarGradient(user.fullName) }"
            >
              {{ getInitials(user.fullName) }}
            </div>
            <div>
              <p class="font-bold text-gray-900 dark:text-gray-100">{{ user.fullName }}</p>
              <p class="text-xs text-gray-600 dark:text-gray-300">{{ user.departmentName || 'Не указано' }}</p>
            </div>
          </div>
          <div class="mt-4 flex items-center justify-between text-sm">
            <span class="text-gray-700 dark:text-gray-200">Уровень {{ user.level || 1 }}</span>
            <span class="font-black text-primary-700 dark:text-primary-300">{{ user.totalXP || 0 }} XP</span>
          </div>
        </article>
      </div>

      <div class="card overflow-x-auto">
        <table class="min-w-full text-sm">
          <thead>
            <tr class="border-b border-gray-200 dark:border-gray-700 text-left text-xs uppercase tracking-wide text-gray-500 dark:text-gray-400">
              <th class="py-3 pr-4">Место</th>
              <th class="py-3 pr-4">Сотрудник</th>
              <th class="py-3 pr-4">Подразделение</th>
              <th class="py-3 pr-4">Уровень</th>
              <th class="py-3 text-right">XP</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="user in filteredLeaderboard"
              :key="user.userId"
              class="border-b border-gray-100 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
              :class="{ 'bg-primary-50/70 dark:bg-primary-900/20': isCurrentUser(user) }"
            >
              <td class="py-3 pr-4">
                <span class="inline-flex items-center gap-1 font-bold">
                  <span>{{ getMedal(user.rank) }}</span>
                  <span>#{{ user.rank }}</span>
                </span>
              </td>
              <td class="py-3 pr-4">
                <div class="flex items-center gap-3">
                  <div
                    class="w-9 h-9 rounded-full flex items-center justify-center text-white font-semibold text-xs"
                    :style="{ background: getAvatarGradient(user.fullName) }"
                  >
                    {{ getInitials(user.fullName) }}
                  </div>
                  <div class="flex items-center gap-2">
                    <span class="font-semibold text-gray-900 dark:text-gray-100">{{ user.fullName }}</span>
                    <span
                      v-if="isCurrentUser(user)"
                      class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase bg-primary-100 text-primary-700 dark:bg-primary-900/40 dark:text-primary-300"
                    >
                      Вы
                    </span>
                  </div>
                </div>
              </td>
              <td class="py-3 pr-4 text-gray-600 dark:text-gray-300">{{ user.departmentName || 'Не указано' }}</td>
              <td class="py-3 pr-4">
                <span class="px-2 py-1 rounded-md bg-gray-100 dark:bg-gray-700 font-semibold">
                  {{ user.level || 1 }}
                </span>
              </td>
              <td class="py-3 text-right font-black text-gray-900 dark:text-gray-100">{{ user.totalXP || 0 }}</td>
            </tr>
          </tbody>
        </table>
        <div
          v-if="!filteredLeaderboard.length"
          class="text-center text-sm text-gray-500 dark:text-gray-400 py-6"
        >
          По вашему запросу ничего не найдено.
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { usersApi } from '../api/services'
import { getAvatarGradient } from '../utils/avatar'

const leaderboard = ref([])
const isLoading = ref(false)
const error = ref(null)
const searchQuery = ref('')
const selectedDepartment = ref('')
const authStore = useAuthStore()

const departments = computed(() =>
  Array.from(
    new Set(
      leaderboard.value
        .map((user) => user.departmentName || 'Не указано')
        .filter(Boolean)
    )
  ).sort((a, b) => a.localeCompare(b, 'ru'))
)

const filteredLeaderboard = computed(() => {
  const query = searchQuery.value.toLowerCase()
  return leaderboard.value.filter((user) => {
    const department = user.departmentName || 'Не указано'
    const matchesDepartment = !selectedDepartment.value || department === selectedDepartment.value
    const matchesSearch = !query || (user.fullName || '').toLowerCase().includes(query)
    return matchesDepartment && matchesSearch
  })
})

const topThree = computed(() => filteredLeaderboard.value.slice(0, 3))

const rankUsers = (users) =>
  users
    .slice()
    .sort((a, b) => {
      const xpDiff = (b.totalXP || 0) - (a.totalXP || 0)
      if (xpDiff !== 0) return xpDiff
      const levelDiff = (b.level || 1) - (a.level || 1)
      if (levelDiff !== 0) return levelDiff
      return (a.fullName || '').localeCompare(b.fullName || '', 'ru')
    })
    .map((user, index) => ({
      ...user,
      rank: index + 1
    }))

const fetchLeaderboard = async () => {
  isLoading.value = true
  error.value = null
  try {
    const response = await usersApi.getAll()
    leaderboard.value = rankUsers(response.data || [])
  } catch (err) {
    error.value = err.response?.data?.message || 'Не удалось загрузить таблицу лидеров'
    console.error('Leaderboard fetch error:', err)
  } finally {
    isLoading.value = false
  }
}

const getMedal = (rank) => {
  if (rank === 1) return '🥇'
  if (rank === 2) return '🥈'
  if (rank === 3) return '🥉'
  return '🏅'
}

const getInitials = (fullName) => {
  if (!fullName) return 'U'
  return fullName
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase()
}

const isCurrentUser = (user) => authStore.currentUser?.userId === user.userId

const topCardClass = (rank) => {
  if (rank === 1) return 'bg-gradient-to-br from-yellow-100 to-amber-50 border-yellow-300 dark:from-yellow-900/30 dark:to-amber-900/20 dark:border-yellow-700'
  if (rank === 2) return 'bg-gradient-to-br from-slate-100 to-gray-50 border-slate-300 dark:from-slate-800/60 dark:to-gray-800 dark:border-slate-600'
  if (rank === 3) return 'bg-gradient-to-br from-orange-100 to-amber-50 border-orange-300 dark:from-orange-900/30 dark:to-amber-900/20 dark:border-orange-700'
  return 'bg-white border-gray-200 dark:bg-gray-800 dark:border-gray-700'
}

onMounted(fetchLeaderboard)
</script>
