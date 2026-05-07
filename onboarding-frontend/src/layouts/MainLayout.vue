<template>
  <div class="min-h-screen bg-gradient-to-br from-gray-50 via-white to-gray-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900 flex flex-col transition-colors duration-300">
    <!-- Navigation -->
    <nav class="bg-white/95 dark:bg-gray-800/95 shadow-sm border-b border-gray-200/50 dark:border-gray-700/50 backdrop-blur-sm sticky top-0 z-50 transition-colors duration-300">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-16 items-center">
          <!-- Logo -->
          <div class="flex items-center">
            <router-link to="/" class="flex items-center space-x-3 hover:opacity-80 transition-opacity">
              <div class="w-11 h-11 rounded-lg overflow-hidden border border-gray-200 dark:border-gray-700 flex items-center justify-center bg-gradient-to-br from-primary-50 to-white dark:from-gray-700 dark:to-gray-800 shadow-md p-0.5 hover:shadow-lg transition-shadow">
                <img src="/image6.png" alt="Logo" class="w-full h-full object-contain" />
              </div>
              <span class="text-xl font-bold text-gray-900 dark:text-white hidden sm:block">Онбординг</span>
            </router-link>
          </div>
          
          <!-- Desktop Nav Items -->
          <div class="hidden md:flex items-center space-x-2">
            <router-link
              v-for="item in navItems"
              :key="item.to"
              :to="item.to"
              v-show="item.show"
              class="text-gray-600 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200 hover:bg-primary-50 dark:hover:bg-primary-900/20"
              active-class="text-primary-600 dark:text-primary-400 font-bold bg-primary-50 dark:bg-primary-900/30"
            >
              {{ item.name }}
            </router-link>
            
            <div class="flex items-center space-x-4 pl-6 border-l border-gray-200">
              <div class="text-right hidden sm:block">
                <p class="text-sm font-semibold text-gray-900 dark:text-gray-100">{{ authStore.currentUser?.fullName }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">{{ authStore.currentUser?.departmentName }}</p>
              </div>
              <router-link to="/profile" class="w-10 h-10 rounded-full flex items-center justify-center text-white font-bold text-sm border-2 border-white shadow-md hover:shadow-lg transition-all hover:scale-105"
                           :style="{ background: getAvatarGradient(authStore.currentUser?.fullName) }">
                {{ authStore.currentUser?.fullName?.charAt(0) }}
              </router-link>
              
              <!-- Theme Toggle Button -->
              <button
                @click="themeStore.toggleTheme()"
                class="p-2 text-gray-600 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors hover:bg-gray-100 dark:hover:bg-gray-800 rounded-lg"
                :title="themeStore.isDark ? 'Светлая тема' : 'Тёмная тема'"
              >
                <svg v-if="!themeStore.isDark" class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z" />
                </svg>
                <svg v-else class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 2a1 1 0 011 1v1a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l-2.12-2.12a1 1 0 00-1.414 0l-.707.707a1 1 0 001.414 1.414l2.12 2.12a1 1 0 001.414-1.414l-.707-.707zm2.12-10.607a1 1 0 010 1.414l-.707.707a1 1 0 11-1.414-1.414l.707-.707a1 1 0 011.414 0zM17 11a1 1 0 100-2h-1a1 1 0 100 2h1zm-7 4a1 1 0 011 1v1a1 1 0 11-2 0v-1a1 1 0 011-1zM5.05 6.464A1 1 0 106.464 5.05l-.707-.707a1 1 0 00-1.414 1.414l.707.707zm5.657-9.193a1 1 0 00-1.414 0l-.707.707A1 1 0 005.05 3.536l.707-.707a1 1 0 011.414 0zM5 6a1 1 0 100-2H4a1 1 0 100 2h1z" clip-rule="evenodd" />
                </svg>
              </button>
              
              <button
                @click="handleLogout"
                class="p-2 text-gray-600 dark:text-gray-300 hover:text-red-600 dark:hover:text-red-400 transition-colors hover:bg-red-50 dark:hover:bg-red-950 rounded-lg"
                title="Выйти"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
              </button>
            </div>
          </div>

          <!-- Mobile menu button -->
          <div class="md:hidden flex items-center space-x-2">
            <button
              @click="themeStore.toggleTheme()"
              class="p-2 rounded-md text-gray-600 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            >
              <svg v-if="!themeStore.isDark" class="h-5 w-5" fill="currentColor" viewBox="0 0 20 20">
                <path d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z" />
              </svg>
              <svg v-else class="h-5 w-5" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 2a1 1 0 011 1v1a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l-2.12-2.12a1 1 0 00-1.414 0l-.707.707a1 1 0 001.414 1.414l2.12 2.12a1 1 0 001.414-1.414l-.707-.707zm2.12-10.607a1 1 0 010 1.414l-.707.707a1 1 0 11-1.414-1.414l.707-.707a1 1 0 011.414 0zM17 11a1 1 0 100-2h-1a1 1 0 100 2h1zm-7 4a1 1 0 011 1v1a1 1 0 11-2 0v-1a1 1 0 011-1zM5.05 6.464A1 1 0 106.464 5.05l-.707-.707a1 1 0 00-1.414 1.414l.707.707zm5.657-9.193a1 1 0 00-1.414 0l-.707.707A1 1 0 005.05 3.536l.707-.707a1 1 0 011.414 0zM5 6a1 1 0 100-2H4a1 1 0 100 2h1z" clip-rule="evenodd" />
              </svg>
            </button>
            <button
              @click="isMobileMenuOpen = !isMobileMenuOpen"
              class="p-2 rounded-md text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
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
      <div v-show="isMobileMenuOpen" class="md:hidden bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700 shadow-xl transition-colors duration-300">
        <div class="px-2 pt-2 pb-3 space-y-1">
          <router-link
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            v-show="item.show"
            class="block px-4 py-3 rounded-lg text-base font-medium text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors"
            active-class="bg-primary-100 dark:bg-primary-900/30 text-primary-600 dark:text-primary-400 font-bold"
            @click="isMobileMenuOpen = false"
          >
            {{ item.name }}
          </router-link>
        </div>
        <div class="pt-4 pb-3 border-t border-gray-200 dark:border-gray-700 px-4 flex items-center justify-between">
          <div class="flex items-center space-x-3">
             <div class="w-10 h-10 rounded-full flex items-center justify-center text-white font-bold"
                  :style="{ background: getAvatarGradient(authStore.currentUser?.fullName) }">
                {{ authStore.currentUser?.fullName?.charAt(0) }}
            </div>
            <div>
              <p class="text-sm font-semibold text-gray-900 dark:text-gray-100">{{ authStore.currentUser?.fullName }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400 text-left">{{ authStore.currentUser?.departmentName }}</p>
            </div>
          </div>
          <button @click="handleLogout" class="text-red-600 dark:text-red-400 font-bold text-sm hover:bg-red-50 dark:hover:bg-red-900/30 px-3 py-2 rounded-lg transition-colors">Выйти</button>
        </div>
      </div>
    </nav>

    <main class="flex-grow max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-10">
      <router-view />
    </main>

    <!-- AI Chat Assistant -->
    <div v-if="!isAiChatOpen" class="fixed bottom-6 right-6 z-50">
      <button
        @click="isAiChatOpen = true"
        class="w-14 h-14 rounded-full bg-primary-600 text-white shadow-lg hover:bg-primary-700 transition-all duration-200 flex items-center justify-center hover:scale-110 hover:shadow-xl"
        title="Открыть AI-помощника"
      >
        <svg class="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16h6M5 20l1.5-1.5A8 8 0 1119 18.5L20.5 20z" />
        </svg>
      </button>
    </div>

    <div v-else class="fixed bottom-6 right-6 z-50 w-[340px] sm:w-[380px]">
      <AiChatAssistant
        @close="isAiChatOpen = false"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useThemeStore } from '../stores/theme'
import { useRouter } from 'vue-router'
import { getAvatarGradient } from '../utils/avatar'
import AiChatAssistant from '../components/AiChatAssistant.vue'

const authStore = useAuthStore()
const themeStore = useThemeStore()
const router = useRouter()
const isMobileMenuOpen = ref(false)
const isAiChatOpen = ref(false)

onMounted(() => {
  themeStore.applyTheme()
})

const navItems = computed(() => [
  { name: 'Дашборд', to: '/', show: true },
  { name: 'Лидеры', to: '/leaderboard', show: true },
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
