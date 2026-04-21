<template>
  <div class="min-h-screen bg-gray-50 flex flex-col">
    <!-- Navigation -->
    <nav class="bg-white shadow-sm border-b border-gray-200 sticky top-0 z-50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-16">
          <!-- Logo -->
          <div class="flex items-center">
            <router-link to="/" class="flex items-center space-x-3">
              <div class="w-11 h-11 rounded-lg overflow-hidden border border-gray-100 flex items-center justify-center bg-white shadow-sm p-0.5">
                <img src="/image6.png" alt="Logo" class="w-full h-full object-contain" />
              </div>
              <span class="text-xl font-bold text-gray-900 hidden sm:block">Система онбординга</span>
            </router-link>
          </div>
          
          <!-- Desktop Nav Items -->
          <div class="hidden md:flex items-center space-x-4">
            <router-link
              v-for="item in navItems"
              :key="item.to"
              :to="item.to"
              v-show="item.show"
              class="text-gray-600 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium transition-colors"
              active-class="text-primary-600 font-bold"
            >
              {{ item.name }}
            </router-link>
            
            <div class="flex items-center space-x-3 pl-4 border-l border-gray-200">
              <div class="text-right">
                <p class="text-sm font-medium text-gray-900">{{ authStore.currentUser?.fullName }}</p>
                <p class="text-xs text-gray-500">{{ authStore.currentUser?.departmentName }}</p>
              </div>
              <router-link to="/profile" class="w-10 h-10 rounded-full flex items-center justify-center text-white font-bold text-sm border-2 border-white shadow-sm"
                           :style="{ background: getAvatarGradient(authStore.currentUser?.fullName) }">
                {{ authStore.currentUser?.fullName?.charAt(0) }}
              </router-link>
              <button
                @click="handleLogout"
                class="p-2 text-gray-400 hover:text-red-600 transition-colors"
                title="Выйти"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
              </button>
            </div>
          </div>

          <!-- Mobile menu button -->
          <div class="md:hidden flex items-center">
            <button
              @click="isMobileMenuOpen = !isMobileMenuOpen"
              class="p-2 rounded-md text-gray-400 hover:text-gray-500 hover:bg-gray-100"
            >
              <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path v-if="!isMobileMenuOpen" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- Mobile menu -->
      <div v-show="isMobileMenuOpen" class="md:hidden bg-white border-t border-gray-200 shadow-lg">
        <div class="px-2 pt-2 pb-3 space-y-1">
          <router-link
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            v-show="item.show"
            class="block px-3 py-2 rounded-md text-base font-medium text-gray-700 hover:text-primary-600 hover:bg-gray-50"
            active-class="bg-primary-50 text-primary-600 font-bold"
            @click="isMobileMenuOpen = false"
          >
            {{ item.name }}
          </router-link>
        </div>
        <div class="pt-4 pb-3 border-t border-gray-200 px-4 flex items-center justify-between">
          <div class="flex items-center space-x-3">
             <div class="w-10 h-10 rounded-full flex items-center justify-center text-white font-bold"
                  :style="{ background: getAvatarGradient(authStore.currentUser?.fullName) }">
                {{ authStore.currentUser?.fullName?.charAt(0) }}
            </div>
            <div>
              <p class="text-sm font-medium text-gray-900">{{ authStore.currentUser?.fullName }}</p>
              <p class="text-xs text-gray-500 text-left">{{ authStore.currentUser?.departmentName }}</p>
            </div>
          </div>
          <button @click="handleLogout" class="text-red-600 font-bold text-sm">Выйти</button>
        </div>
      </div>
    </nav>

    <main class="flex-grow max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-8">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'
import { getAvatarGradient } from '../utils/avatar'

const authStore = useAuthStore()
const router = useRouter()
const isMobileMenuOpen = ref(false)

onMounted(() => {
  // Принудительная очистка следов тёмной темы
  document.documentElement.classList.remove('dark')
  localStorage.removeItem('onboarding-theme')
  localStorage.removeItem('theme')
})

const navItems = computed(() => [
  { name: 'Дашборд', to: '/', show: true },
  { name: 'Справки', to: '/faq', show: true },
  { name: 'Мои подопечные', to: '/mentor', show: authStore.isMentor || authStore.isAdmin },
  { name: 'Отчёты', to: '/reports', show: authStore.isHR || authStore.isManager || authStore.isAdmin },
  { name: 'Администрирование', to: '/admin', show: authStore.isAdmin || authStore.isHR }
])

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>
