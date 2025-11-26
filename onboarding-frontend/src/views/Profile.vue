<template>
  <div class="space-y-6">
    <h1 class="text-3xl font-bold text-gray-900">Мой профиль</h1>

    <div v-if="authStore.currentUser" class="card">
      <div class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">ФИО</label>
          <p class="text-gray-900">{{ authStore.currentUser.fullName }}</p>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <p class="text-gray-900">{{ authStore.currentUser.email }}</p>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Подразделение</label>
          <p class="text-gray-900">{{ authStore.currentUser.departmentName }}</p>
        </div>

        <div v-if="authStore.currentUser.jobTitle">
          <label class="block text-sm font-medium text-gray-700 mb-1">Должность</label>
          <p class="text-gray-900">{{ authStore.currentUser.jobTitle }}</p>
        </div>

        <div v-if="authStore.currentUser.mentorName">
          <label class="block text-sm font-medium text-gray-700 mb-1">Наставник</label>
          <p class="text-gray-900">{{ authStore.currentUser.mentorName }}</p>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Дата приёма</label>
          <p class="text-gray-900">{{ formatDate(authStore.currentUser.hireDate) }}</p>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Статус онбординга</label>
          <span
            class="inline-block px-3 py-1 text-sm font-medium rounded-full"
            :class="{
              'bg-green-100 text-green-800': authStore.currentUser.onboardingStatus === 'Завершён',
              'bg-yellow-100 text-yellow-800': authStore.currentUser.onboardingStatus === 'В процессе',
              'bg-gray-100 text-gray-800': authStore.currentUser.onboardingStatus === 'Не начат'
            }"
          >
            {{ authStore.currentUser.onboardingStatus }}
          </span>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Роли</label>
          <div class="flex flex-wrap gap-2 mt-2">
            <span
              v-for="role in authStore.currentUser.roles"
              :key="role"
              class="px-3 py-1 text-sm font-medium bg-primary-100 text-primary-800 rounded-full"
            >
              {{ role }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- PSO Integration Link -->
    <div class="card bg-primary-50 border border-primary-200">
      <h2 class="text-lg font-semibold text-gray-900 mb-2">Запрос доступов</h2>
      <p class="text-sm text-gray-600 mb-4">
        Для получения доступа к корпоративным системам перейдите в ПСО
      </p>
      <a
        href="#"
        @click.prevent="openPSO"
        class="btn-primary inline-block"
      >
        Перейти в ПСО
      </a>
    </div>
  </div>
</template>

<script setup>
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()

const formatDate = (dateString) => {
  if (!dateString) return '—'
  // Handle DateOnly format from backend
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString
  return date.toLocaleDateString('ru-RU', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const openPSO = () => {
  // TODO: Implement PSO integration
  // This would open PSO with pre-filled user data
  const user = authStore.currentUser
  if (user) {
    // Example: window.open(`https://pso.example.com/request?user=${user.fullName}&dept=${user.departmentName}`)
    alert('Интеграция с ПСО будет реализована. Здесь будет ссылка на ПСО с предзаполненными данными.')
  }
}
</script>



