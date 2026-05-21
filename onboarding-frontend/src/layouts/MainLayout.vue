<template>
  <div class="min-h-screen bg-gradient-to-br from-gray-50 via-white to-gray-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900 flex flex-col transition-colors duration-300">
    <!-- Navigation -->
    <nav class="bg-white/95 dark:bg-gray-800/95 shadow-sm border-b border-gray-200/50 dark:border-gray-700/50 backdrop-blur-sm sticky top-0 z-50 transition-colors duration-300">
      <div class="max-w-7xl mx-auto px-3 sm:px-4 lg:px-8">
        <div class="flex items-center justify-between gap-3 sm:gap-4 min-h-16 py-2">
          <!-- Logo - Fixed left -->
          <router-link
            to="/"
            class="flex items-center gap-2 sm:gap-3 hover:opacity-80 transition-opacity flex-shrink-0"
          >
            <div class="w-9 h-9 sm:w-11 sm:h-11 rounded-lg overflow-hidden border border-gray-200 dark:border-gray-700 flex items-center justify-center bg-gradient-to-br from-primary-50 to-white dark:from-gray-700 dark:to-gray-800 shadow-md p-0.5">
              <img src="/image6.png" alt="Logo" class="w-full h-full object-contain" />
            </div>
            <span class="text-base sm:text-xl font-bold text-gray-900 dark:text-white hidden sm:inline">
              Онбординг
            </span>
          </router-link>

          <!-- Desktop / tablet landscape: nav - centered -->
          <div class="hidden lg:flex items-center gap-1 xl:gap-2 flex-1 justify-center">
            <!-- Nav links -->
            <div class="flex items-center gap-0.5 xl:gap-1">
              <router-link
                v-for="item in visibleNavItems"
                :key="item.to"
                :to="item.to"
                class="whitespace-nowrap text-gray-600 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 px-3 xl:px-4 py-2.5 rounded-lg text-sm xl:text-base font-medium transition-all duration-200 hover:bg-primary-50 dark:hover:bg-primary-900/20"
                active-class="text-primary-600 dark:text-primary-400 font-bold bg-primary-50 dark:bg-primary-900/30"
              >
                {{ item.name }}
              </router-link>
            </div>
          </div>

          <!-- User actions - Fixed right -->
          <div class="flex items-center gap-1 xl:gap-2 flex-shrink-0">
  <div class="text-right hidden 2xl:block max-w-[140px]">
    <p class="text-sm font-semibold text-gray-900 dark:text-gray-100 truncate">
      {{ authStore.currentUser?.fullName }}
    </p>
    <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
      {{ authStore.currentUser?.departmentName }}
    </p>
  </div>

            <router-link
              to="/profile"
              class="w-9 h-9 xl:w-10 xl:h-10 rounded-full flex-shrink-0 flex items-center justify-center text-white font-bold text-sm border-2 border-white dark:border-gray-700 shadow-md hover:shadow-lg transition-all"
              :style="{ background: getAvatarGradient(authStore.currentUser?.fullName) }"
              title="Профиль"
            >
              {{ authStore.currentUser?.fullName?.charAt(0) }}
            </router-link>

            <NotificationBell />

            <button
              type="button"
              @click="themeStore.toggleTheme()"
              class="flex-shrink-0 p-2.5 text-gray-600 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors hover:bg-gray-100 dark:hover:bg-gray-800 rounded-lg"
              :title="themeStore.isDark ? 'Светлая тема' : 'Тёмная тема'"
            >
              <ThemeIcon :is-dark="themeStore.isDark" />
            </button>

            <button
              type="button"
              @click="handleLogout"
              class="flex-shrink-0 p-2.5 text-gray-600 dark:text-gray-300 hover:text-red-600 dark:hover:text-red-400 transition-colors hover:bg-red-50 dark:hover:bg-red-950 rounded-lg"
              title="Выйти"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
            </button>
          </div>

          <!-- Mobile / small tablet controls -->
          <div class="flex lg:hidden items-center gap-0.5 sm:gap-1 flex-shrink-0">
            <NotificationBell />
            <button
              type="button"
              @click="themeStore.toggleTheme()"
              class="p-2.5 rounded-lg text-gray-600 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              :title="themeStore.isDark ? 'Светлая тема' : 'Тёмная тема'"
            >
              <ThemeIcon :is-dark="themeStore.isDark" size-class="h-6 w-6" />
            </button>
            <button
              type="button"
              @click="isMobileMenuOpen = !isMobileMenuOpen"
              class="p-2.5 rounded-lg text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              aria-label="Меню"
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
      <div
        v-show="isMobileMenuOpen"
        class="lg:hidden bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700 shadow-xl max-h-[calc(100vh-4rem)] overflow-y-auto"
      >
        <div class="px-2 pt-2 pb-1 space-y-0.5">
          <router-link
            v-for="item in visibleNavItems"
            :key="item.to"
            :to="item.to"
            class="block px-4 py-3.5 rounded-lg text-base font-medium text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors"
            active-class="bg-primary-100 dark:bg-primary-900/30 text-primary-600 dark:text-primary-400 font-bold"
            @click="isMobileMenuOpen = false"
          >
            {{ item.name }}
          </router-link>
        </div>

        <div class="px-4 py-4 border-t border-gray-200 dark:border-gray-700 space-y-3">
          <div class="flex items-center gap-3 min-w-0">
            <router-link
              to="/profile"
              class="w-10 h-10 flex-shrink-0 rounded-full flex items-center justify-center text-white font-bold"
              :style="{ background: getAvatarGradient(authStore.currentUser?.fullName) }"
              @click="isMobileMenuOpen = false"
            >
              {{ authStore.currentUser?.fullName?.charAt(0) }}
            </router-link>
            <div class="min-w-0 flex-1">
              <p class="text-sm font-semibold text-gray-900 dark:text-gray-100 truncate">
                {{ authStore.currentUser?.fullName }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
                {{ authStore.currentUser?.departmentName }}
              </p>
            </div>
          </div>

          <div class="flex flex-wrap gap-2">
            <router-link
              to="/profile"
              class="flex-1 min-w-[calc(50%-0.25rem)] text-center px-3 py-2.5 rounded-lg text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors"
              @click="isMobileMenuOpen = false"
            >
              Профиль
            </router-link>
            <button
              type="button"
              @click="handleLogout"
              class="flex-1 min-w-[calc(50%-0.25rem)] px-3 py-2.5 rounded-lg text-sm font-semibold text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/30 hover:bg-red-100 dark:hover:bg-red-900/50 transition-colors"
            >
              Выйти
            </button>
          </div>
        </div>
      </div>
    </nav>

    <main class="flex-grow max-w-7xl mx-auto w-full min-w-0 px-3 sm:px-4 lg:px-8 py-6 sm:py-8 lg:py-10">
      <router-view />
    </main>

    <!-- AI Chat Assistant -->
    <div v-if="!isAiChatOpen" class="fixed bottom-4 right-4 sm:bottom-6 sm:right-6 z-40">
      <button
        type="button"
        @click="isAiChatOpen = true"
        class="w-12 h-12 sm:w-14 sm:h-14 rounded-full bg-primary-600 text-white shadow-lg hover:bg-primary-700 transition-all duration-200 flex items-center justify-center hover:scale-105 hover:shadow-xl"
        title="Открыть AI-помощника"
      >
        <svg class="w-6 h-6 sm:w-7 sm:h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16h6M5 20l1.5-1.5A8 8 0 1119 18.5L20.5 20z" />
        </svg>
      </button>
    </div>

    <div
      v-else
      class="fixed inset-x-3 bottom-4 sm:inset-x-auto sm:bottom-6 sm:right-6 z-40 w-auto sm:w-[min(380px,calc(100vw-2rem))]"
    >
      <AiChatAssistant @close="isAiChatOpen = false" />
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
import NotificationBell from '../components/NotificationBell.vue'
import ThemeIcon from '../components/ThemeIcon.vue'
import { useNotificationsStore } from '../stores/notifications'

const authStore = useAuthStore()
const notificationsStore = useNotificationsStore()
const themeStore = useThemeStore()
const router = useRouter()
const isMobileMenuOpen = ref(false)
const isAiChatOpen = ref(false)

onMounted(async () => {
  themeStore.applyTheme()
  if (authStore.isAuthenticated) {
    await notificationsStore.connect()
  }
})

const navItems = computed(() => [
  { name: 'Дашборд', to: '/', show: true },
  { name: 'Лидеры', to: '/leaderboard', show: true },
  { name: 'Справки', to: '/faq', show: true },
  { name: 'Мои подопечные', to: '/mentor', show: authStore.isMentor || authStore.canManageMentees || authStore.isAdmin },
  { name: 'Отчёты', to: '/reports', show: authStore.canViewDepartmentReports },
  { name: 'Администрирование', to: '/admin', show: authStore.canManageDepartment }
])

const visibleNavItems = computed(() => navItems.value.filter(item => item.show))

const handleLogout = async () => {
  isMobileMenuOpen.value = false
  await notificationsStore.disconnect()
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.scrollbar-thin {
  scrollbar-width: none;
}
.scrollbar-thin::-webkit-scrollbar {
  display: none;
}
</style>