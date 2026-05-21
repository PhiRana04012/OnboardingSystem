<template>
  <div class="relative" ref="rootRef">
    <button
      type="button"
      @click.stop="toggle"
      class="relative p-2 text-gray-600 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors hover:bg-gray-100 dark:hover:bg-gray-800 rounded-lg"
      title="Уведомления"
    >
      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
      </svg>
      <span
        v-if="notificationsStore.unreadCount > 0"
        class="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] px-1 flex items-center justify-center text-[10px] font-bold text-white bg-red-500 rounded-full"
      >
        {{ notificationsStore.unreadCount > 99 ? '99+' : notificationsStore.unreadCount }}
      </span>
    </button>

    <div
      v-if="notificationsStore.isOpen"
      class="fixed left-3 right-3 top-[4.5rem] sm:absolute sm:left-auto sm:right-0 sm:top-full sm:mt-2 sm:w-80 md:w-96 max-w-[calc(100vw-1.5rem)] sm:max-w-none bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl z-[60] overflow-hidden"
    >
      <div class="flex items-center justify-between px-4 py-3 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
        <h3 class="font-semibold text-gray-900 dark:text-white">Уведомления</h3>
        <button
          v-if="notificationsStore.unreadCount > 0"
          type="button"
          @click="notificationsStore.markAllAsRead()"
          class="text-xs text-primary-600 dark:text-primary-400 hover:underline"
        >
          Прочитать все
        </button>
      </div>

      <div class="max-h-80 overflow-y-auto">
        <div
          v-if="notificationsStore.items.length === 0"
          class="px-4 py-8 text-center text-sm text-gray-500 dark:text-gray-400"
        >
          Нет уведомлений
        </div>

        <button
          v-for="item in notificationsStore.items"
          :key="item.notificationId"
          type="button"
          @click="handleClick(item)"
          class="w-full text-left px-4 py-3 border-b border-gray-100 dark:border-gray-700/50 hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
          :class="{ 'bg-primary-50/50 dark:bg-primary-900/20': !item.isRead }"
        >
          <div class="flex gap-3">
            <span class="text-lg flex-shrink-0">{{ iconFor(item.type) }}</span>
            <div class="min-w-0 flex-1">
              <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
                {{ item.title }}
              </p>
              <p class="text-xs text-gray-600 dark:text-gray-400 mt-0.5 line-clamp-2">
                {{ item.message }}
              </p>
              <p class="text-[10px] text-gray-400 dark:text-gray-500 mt-1">
                {{ formatTime(item.createdAt) }}
              </p>
            </div>
            <span
              v-if="!item.isRead"
              class="w-2 h-2 rounded-full bg-primary-500 flex-shrink-0 mt-1.5"
            />
          </div>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useNotificationsStore } from '../stores/notifications'

console.log('[NotificationBell] Script setup starting...')

const notificationsStore = useNotificationsStore()
const router = useRouter()
const rootRef = ref(null)

console.log('[NotificationBell] Variables initialized, store:', notificationsStore)

// Watch for isOpen changes
watch(() => notificationsStore.isOpen, (newVal, oldVal) => {
  console.log('[NotificationBell] WATCH: isOpen changed from', oldVal, 'to', newVal)
}, { immediate: true })

const iconMap = {
  level_up: '⬆️',
  achievement: '🏅',
  xp_earned: '✨',
  module_completed: '📖',
  test_passed: '✅',
  test_failed: '❌',
  onboarding_completed: '🎉',
  mentor_assigned: '👤',
  mentee_assigned: '👥'
}

const iconFor = (type) => iconMap[type] || '🔔'

const formatTime = (dateStr) => {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = now - date
  const diffMin = Math.floor(diffMs / 60000)
  if (diffMin < 1) return 'только что'
  if (diffMin < 60) return `${diffMin} мин. назад`
  const diffH = Math.floor(diffMin / 60)
  if (diffH < 24) return `${diffH} ч. назад`
  return date.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' })
}

const toggle = () => {
  console.log('[NotificationBell] toggle() called, current isOpen:', notificationsStore.isOpen)
  notificationsStore.togglePanel()
  console.log('[NotificationBell] after toggle, isOpen:', notificationsStore.isOpen)
}

const handleClick = async (item) => {
  if (!item.isRead) {
    await notificationsStore.markAsRead(item.notificationId)
  }
  notificationsStore.closePanel()
  if (item.linkUrl) {
    router.push(item.linkUrl)
  }
}

const onClickOutside = (e) => {
  if (notificationsStore.isOpen && rootRef.value && !rootRef.value.contains(e.target)) {
    notificationsStore.closePanel()
  }
}

try {
  onMounted(() => {
    console.log('[NotificationBell] onMounted CALLED!')
    console.log('[NotificationBell] rootRef:', rootRef.value)
    console.log('[NotificationBell] store:', notificationsStore)
    console.log('[NotificationBell] items:', notificationsStore.items)
    console.log('[NotificationBell] isOpen:', notificationsStore.isOpen)
    document.addEventListener('click', onClickOutside)
  })
} catch (e) {
  console.error('[NotificationBell] onMounted ERROR:', e)
}

onUnmounted(() => {
  document.removeEventListener('click', onClickOutside)
})

console.log('[NotificationBell] Script setup complete')
</script>
