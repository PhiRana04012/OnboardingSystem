<template>
  <div class="max-w-6xl mx-auto space-y-6">
    <div class="flex items-center justify-between">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Мой профиль</h1>
    </div>

    <div v-if="authStore.currentUser">
      <!-- Верхняя карточка: аватар + инфо + статус -->
      <div class="card mb-6">
        <div class="flex flex-col sm:flex-row items-center sm:items-start gap-6">
          <!-- Градиентный аватар -->
          <div class="w-24 h-24 rounded-full flex items-center justify-center text-3xl font-bold text-white shadow-lg flex-shrink-0"
               :style="{ background: getAvatarGradient(authStore.currentUser?.fullName) }">
            {{ initials }}
          </div>
          <!-- Инфо -->
          <div class="flex-1 text-center sm:text-left">
            <h2 class="text-2xl font-bold text-gray-900">{{ authStore.currentUser.fullName }}</h2>
            <p class="text-gray-500 mt-1">{{ jobTitleLabel }}</p>
            <p class="text-gray-400 text-sm">{{ authStore.currentUser.email }}</p>
            <div class="mt-3">
              <span class="inline-block px-4 py-1.5 rounded-full text-xs font-bold uppercase"
                    :class="roleBadgeClass">
                {{ primaryRole }}
              </span>
            </div>
          </div>
          <!-- Статус онбординга -->
          <div class="text-center sm:text-right flex-shrink-0">
            <p class="text-xs text-gray-400 mb-1">Статус онбординга</p>
            <span class="inline-block px-4 py-1.5 rounded-full text-sm font-bold" :class="statusClass">
              {{ authStore.currentUser.onboardingStatus }}
            </span>
          </div>
        </div>
      </div>

      <!-- Рабочая информация + Наставник -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
        <!-- Рабочая информация -->
        <div class="card">
          <h3 class="text-lg font-bold text-gray-900 mb-4 pb-3 border-b border-gray-100">Рабочая информация</h3>
          <div class="space-y-4">
            <div>
              <label class="text-xs text-gray-400 font-medium">Подразделение</label>
              <p class="font-bold text-gray-900 mt-0.5">{{ authStore.currentUser.departmentName }}</p>
            </div>
            <div>
              <label class="text-xs text-gray-400 font-medium">Дата приёма</label>
              <p class="font-bold text-gray-900 mt-0.5">{{ formatDate(authStore.currentUser.hireDate) }}</p>
            </div>
            <div>
              <label class="text-xs text-gray-400 font-medium">ID сотрудника</label>
              <p class="font-bold text-gray-900 mt-0.5">#{{ authStore.currentUser.userId }}</p>
            </div>
          </div>
        </div>

        <!-- Мой наставник -->
        <div class="card">
          <h3 class="text-lg font-bold text-gray-900 mb-4 pb-3 border-b border-gray-100">Мой наставник</h3>
          <div v-if="authStore.currentUser.mentorName" class="flex items-center gap-4">
            <div class="w-14 h-14 rounded-full flex items-center justify-center text-xl font-bold text-white shadow-inner"
                 :style="{ background: getAvatarGradient(authStore.currentUser.mentorName) }">
              {{ mentorInitials }}
            </div>
            <div>
              <p class="font-bold text-gray-900 text-lg">{{ authStore.currentUser.mentorName }}</p>
              <p class="text-sm text-gray-500">Наставник</p>
            </div>
          </div>
          <div v-else class="flex flex-col items-center justify-center py-6 text-gray-400">
            <svg class="w-12 h-12 mb-2 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            <p class="font-medium">Наставник не назначен</p>
          </div>
        </div>
      </div>

      <!-- Блок "Опыт и Достижения" - тёмный -->
      <div class="rounded-xl p-8 mb-6 relative overflow-hidden"
           style="background: linear-gradient(135deg, #0f2027, #203a43, #2c5364);">
        <!-- Декоративная звезда -->
        <div class="absolute top-4 right-8 text-white/10 text-[180px] font-black leading-none pointer-events-none select-none" style="font-family: serif;">★</div>
        
        <h3 class="text-xl font-bold text-white mb-6 flex items-center gap-2">
          Опыт и Достижения <span class="text-2xl">✨</span>
        </h3>

        <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-6 mb-8">
          <!-- Уровень -->
          <div>
            <p class="text-primary-300 text-sm font-medium mb-1">Текущий уровень</p>
            <div class="flex items-center gap-3">
              <span class="text-4xl font-black text-white">{{ gamStore.profile?.level || authStore.currentUser.level || 1 }}</span>
              <span class="px-3 py-1 rounded-full text-sm font-bold flex items-center gap-1"
                    style="background: rgba(255,255,255,0.15); color: #4ade80;">
                🚀 {{ levelTitle }}
              </span>
            </div>
            <!-- XP прогресс-бар -->
            <div class="w-48 bg-white/20 rounded-full h-2.5 mt-3 overflow-hidden">
              <div class="h-full rounded-full transition-all duration-500"
                   style="background: linear-gradient(90deg, #f59e0b, #eab308);"
                   :style="{ width: gamStore.levelProgress + '%' }"></div>
            </div>
          </div>

          <!-- XP -->
          <div class="text-right">
            <p class="text-3xl font-black text-emerald-400">{{ gamStore.profile?.totalXP || authStore.currentUser.totalXP || 0 }} XP</p>
            <p class="text-sm text-gray-400 mt-1">До следующего уровня: {{ xpToNextLevel }} XP</p>
          </div>
        </div>

        <!-- Бейджи / Ачивки -->
        <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-4">
          <div v-for="ach in achievements" :key="ach.achievementId || ach.title"
               class="flex flex-col items-center text-center group">
            <div class="w-14 h-14 rounded-full flex items-center justify-center text-2xl mb-2 transition-transform group-hover:scale-110"
                 :class="ach.isEarned ? 'bg-white/20 shadow-lg shadow-white/10' : 'bg-white/5 opacity-50'">
              {{ getAchievementIcon(ach.iconName || ach.title) }}
            </div>
            <p class="text-xs font-bold text-white leading-tight">{{ ach.title }}</p>
            <p class="text-[10px] text-gray-400 leading-tight mt-0.5">{{ ach.description }}</p>
          </div>
        </div>
      </div>

      <!-- Редактировать профиль -->
      <div class="card">
        <div class="flex justify-between items-center mb-4 pb-3 border-b border-gray-100">
          <h3 class="text-lg font-bold text-gray-900">Мои данные</h3>
          <button 
            v-if="!isEditMode"
            @click="enterEditMode" 
            class="btn-secondary px-6 py-2 text-sm"
          >
            ✏️ Редактировать
          </button>
        </div>

        <!-- Режим просмотра -->
        <div v-if="!isEditMode" class="space-y-4">
          <div>
            <label class="text-xs text-gray-400 font-medium">Telegram Tag</label>
            <p class="font-medium text-gray-900 mt-1">{{ authStore.currentUser.telegramTag || '—' }}</p>
          </div>
          <div>
            <label class="text-xs text-gray-400 font-medium">О себе</label>
            <p class="text-gray-700 mt-1 whitespace-pre-wrap">{{ authStore.currentUser.bio || '—' }}</p>
          </div>
        </div>

        <!-- Режим редактирования -->
        <form v-if="isEditMode" @submit.prevent="saveProfile" class="space-y-4">
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-xs text-gray-500">Telegram Tag</label>
              <div class="relative">
                <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">@</span>
                <input v-model="editForm.telegramTag" type="text" class="input !pl-7" />
              </div>
            </div>
          </div>
          <div class="space-y-1">
            <label class="text-xs text-gray-500">О себе</label>
            <textarea v-model="editForm.bio" rows="3" class="input"></textarea>
          </div>
          <div class="flex justify-end gap-3 pt-4 border-t border-gray-100">
            <button 
              type="button"
              @click="cancelEditMode"
              class="btn-secondary px-6 py-2"
            >
              Отмена
            </button>
            <button 
              type="submit" 
              class="btn-primary px-8 py-2" 
              :disabled="isSaving"
            >
              {{ isSaving ? 'Сохранение...' : '✓ Сохранить' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useGamificationStore } from '../stores/gamification'
import { usersApi } from '../api/services'
import { getAvatarGradient } from '../utils/avatar'
import { formatJobTitle } from '../utils/jobTitle'

const authStore = useAuthStore()
const gamStore = useGamificationStore()
const editForm = ref({ bio: '', telegramTag: '' })
const isSaving = ref(false)
const isEditMode = ref(false)

onMounted(async () => {
  if (authStore.currentUser) {
    console.log('📋 Current user on mount:', authStore.currentUser)
    console.log('   - bio:', authStore.currentUser.bio)
    console.log('   - telegramTag:', authStore.currentUser.telegramTag)
    
    editForm.value.bio = authStore.currentUser.bio || ''
    let tag = authStore.currentUser.telegramTag || ''
    if (tag.startsWith('@')) tag = tag.substring(1)
    editForm.value.telegramTag = tag

    // Загружаем профиль геймификации (XP, уровень, бейджи)
    await gamStore.fetchProfile(authStore.currentUser.userId)
  }
})

const jobTitleLabel = computed(() => {
  return formatJobTitle(authStore.currentUser?.jobTitle) || 'Должность не указана'
})

const initials = computed(() => {
  const name = authStore.currentUser?.fullName || 'U'
  return name.split(' ').slice(0, 2).map(n => n[0]).join('').toUpperCase()
})

const mentorInitials = computed(() => {
  const name = authStore.currentUser?.mentorName || ''
  if (!name) return '?'
  return name.split(' ').slice(0, 2).map(n => n[0]).join('').toUpperCase()
})

const primaryRole = computed(() => {
  const roles = authStore.currentUser?.roles || []
  return roles[0] || 'Пользователь'
})

const roleBadgeClass = computed(() => {
  const role = primaryRole.value
  if (role.includes('Администратор')) return 'bg-primary-100 text-primary-700'
  if (role.includes('HR')) return 'bg-blue-100 text-blue-700'
  if (role.includes('Наставник')) return 'bg-purple-100 text-purple-700'
  if (role.includes('Руководитель')) return 'bg-indigo-100 text-indigo-700'
  return 'bg-green-100 text-green-700'
})

const statusClass = computed(() => {
  const status = authStore.currentUser?.onboardingStatus
  if (status === 'Завершён') return 'bg-green-100 text-green-700'
  if (status === 'В процессе') return 'bg-yellow-100 text-yellow-700'
  return 'bg-gray-100 text-gray-400'
})

// Названия уровней
const levelTitles = ['Новичок', 'Стажёр', 'Специалист', 'Эксперт', 'Мастер']
const levelTitle = computed(() => {
  const lvl = gamStore.profile?.level || authStore.currentUser?.level || 1
  return levelTitles[Math.min(lvl - 1, levelTitles.length - 1)]
})

const xpToNextLevel = computed(() => {
  if (!gamStore.profile) {
    const lvl = authStore.currentUser?.level || 1
    const xp = authStore.currentUser?.totalXP || 0
    return Math.max(0, lvl * 100 - xp)
  }
  return Math.max(0, gamStore.profile.nextLevelXP - gamStore.profile.totalXP)
})

// Бейджи: используем данные с бэкенда, или фоллбэк на статичные
const defaultAchievements = [
  { title: 'Первые шаги', description: 'Пройти первый модуль онбординга.', iconName: 'first_steps', isEarned: false },
  { title: 'Идеальный результат', description: 'Сдать тест на 100%.', iconName: 'perfect_score', isEarned: false },
  { title: 'Знаток безопасности', description: 'Завершить модуль безопасности.', iconName: 'security_expert', isEarned: false },
  { title: 'Полный курс', description: 'Полностью завершить программу онбординга.', iconName: 'full_course', isEarned: false },
  { title: 'Кандидат', description: 'Достичь 2 уровня.', iconName: 'candidate', isEarned: false },
  { title: 'Ветеран', description: 'Достичь 5 уровня.', iconName: 'veteran', isEarned: false }
]

const achievements = computed(() => {
  if (gamStore.profile && gamStore.profile.allAchievements && gamStore.profile.allAchievements.length > 0) {
    return gamStore.profile.allAchievements
  }
  return defaultAchievements
})

const achievementIcons = {
  'first_steps': '🎯',
  'perfect_score': '⭐',
  'security_expert': '🛡️',
  'full_course': '🏆',
  'candidate': '📝',
  'veteran': '🎖️',
  'Первые шаги': '🎯',
  'Идеальный результат': '⭐',
  'Знаток безопасности': '🛡️',
  'Полный курс': '🏆',
  'Кандидат': '📝',
  'Ветеран': '🎖️'
}

const getAchievementIcon = (iconName) => {
  return achievementIcons[iconName] || '🏅'
}

const formatDate = (d) => {
  if (!d) return '—'
  return new Date(d).toLocaleDateString('ru-RU', { year: 'numeric', month: 'long', day: 'numeric' })
}

const enterEditMode = () => {
  // Загружаем актуальные данные перед редактированием
  editForm.value.bio = authStore.currentUser.bio || ''
  let tag = authStore.currentUser.telegramTag || ''
  if (tag.startsWith('@')) tag = tag.substring(1)
  editForm.value.telegramTag = tag
  isEditMode.value = true
}

const cancelEditMode = () => {
  // Восстанавливаем исходные значения
  editForm.value.bio = authStore.currentUser.bio || ''
  let tag = authStore.currentUser.telegramTag || ''
  if (tag.startsWith('@')) tag = tag.substring(1)
  editForm.value.telegramTag = tag
  isEditMode.value = false
}

const saveProfile = async () => {
  try {
    isSaving.value = true
    const tag = editForm.value.telegramTag ? '@' + editForm.value.telegramTag.replace('@', '') : ''
    const payload = { bio: editForm.value.bio, telegramTag: tag }
    
    console.log('1. Starting save with payload:', payload)
    console.log('2. User ID:', authStore.currentUser.userId)
    
    const response = await usersApi.updateProfile(authStore.currentUser.userId, payload)
    console.log('3. Server response:', response)
    console.log('4. Response data:', response.data)
    
    // Обновляем текущего пользователя в store сразу
    authStore.currentUser.bio = editForm.value.bio
    authStore.currentUser.telegramTag = tag
    console.log('5. Updated authStore.currentUser:', authStore.currentUser)
    
    // Сохраняем в localStorage
    localStorage.setItem('currentUser', JSON.stringify(authStore.currentUser))
    console.log('6. Saved to localStorage')
    
    // Также пробуем синхронизировать с бэкендом
    console.log('7. Calling refreshUser...')
    await authStore.refreshUser()
    console.log('8. After refresh, authStore.currentUser:', authStore.currentUser)
    
    isEditMode.value = false
    alert('✓ Профиль успешно сохранён!')
  } catch (err) {
    console.error('❌ Profile save error:', err)
    console.error('Error details:', {
      message: err.message,
      response: err.response?.data,
      status: err.response?.status,
      config: err.config
    })
    alert('❌ Ошибка при сохранении профиля. Проверьте консоль (F12) для деталей.')
  } finally {
    isSaving.value = false
  }
}
</script>
