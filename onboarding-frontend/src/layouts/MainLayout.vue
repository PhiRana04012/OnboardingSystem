<template>
  <div class="min-h-screen bg-gray-50">
    <nav class="bg-white shadow-sm border-b border-gray-200">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-16">
          <div class="flex items-center">
            <router-link to="/" class="flex items-center space-x-2">
              <div class="w-8 h-8 bg-white border border-primary-600 rounded-lg flex items-center justify-center overflow-hidden">
                <img src="/image6.png" alt="Логотип" class="w-full h-full object-contain" />
              </div>
              <span class="text-xl font-semibold text-gray-900">Система онбординга</span>
            </router-link>
          </div>
          
          <div class="flex items-center space-x-4">
            <router-link
              v-if="authStore.isNewEmployee || authStore.isHR || authStore.isManager || authStore.isMentor"
              to="/"
              class="text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium transition-colors"
            >
              Дашборд
            </router-link>
            
            <router-link
              v-if="authStore.isHR || authStore.isManager || authStore.isMentor || authStore.isAdmin"
              to="/reports"
              class="text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium transition-colors"
            >
              Отчёты
            </router-link>
            
            <router-link
              v-if="authStore.isAdmin || authStore.isHR"
              to="/admin"
              class="text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium transition-colors"
            >
              Администрирование
            </router-link>
            
            <div class="flex items-center space-x-3 pl-4 border-l border-gray-200">
              <div class="text-right">
                <p class="text-sm font-medium text-gray-900">{{ authStore.currentUser?.fullName }}</p>
                <p class="text-xs text-gray-500">{{ authStore.currentUser?.departmentName }}</p>
              </div>
              <button
                @click="handleLogout"
                class="text-gray-500 hover:text-gray-700 px-3 py-2 rounded-md text-sm font-medium transition-colors"
              >
                Выход
              </button>
            </div>
          </div>
        </div>
      </div>
    </nav>

    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>



