<template>
  <div class="space-y-6">
    <!-- Loading State -->
    <div v-if="isLoading" class="card text-center py-12">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
      <p class="mt-4 text-gray-600">Загрузка модуля...</p>
    </div>

    <!-- Module Content -->
    <div v-else-if="module" class="space-y-6">
      <!-- Header -->
      <div class="flex items-start justify-between">
        <div>
          <div class="flex items-center space-x-3 mb-2">
            <button
              @click="$router.back()"
              class="text-gray-500 hover:text-gray-700 transition-colors"
            >
              ← Назад
            </button>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-white">{{ module.title }}</h1>
          </div>
          <p v-if="module.description" class="text-gray-600 mt-2">{{ module.description }}</p>
          <div class="flex items-center space-x-2 mt-3">
            <span
              v-if="module.isMandatory"
              class="px-3 py-1 text-sm font-medium bg-red-100 dark:bg-red-900/30 text-red-800 dark:text-red-300 rounded-full"
            >
              Обязательный модуль
            </span>
            <span
              v-else
              class="px-3 py-1 text-sm font-medium bg-primary-50 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300 rounded-full"
            >
              Рекомендуемый модуль
            </span>
            <span class="px-3 py-1 text-sm font-medium bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200 rounded-full">
              Проходной балл: {{ module.passingScore }}%
            </span>
          </div>
        </div>
      </div>

      <!-- Module Roadmap -->
      <ModuleRoadmap
        :hasTestBefore="hasPassedBefore"
        :hasTest="hasTest"
        :isReadyForTest="isReadyForTest"
        :allRequiredChecked="allRequiredChecked"
        :moduleStatus="moduleStatus"
      />

      <!-- Content -->
      <div class="card">
        <div
          class="prose max-w-none"
          v-html="formatContent(module.content)"
        ></div>
      </div>

      <!-- Checklist Section -->
      <div v-if="hasChecklist" class="card border-l-4 border-yellow-400 dark:border-yellow-600 bg-yellow-50/30 dark:bg-yellow-900/10">
        <h3 class="text-lg font-bold text-gray-900 dark:text-gray-100 mb-4 flex items-center gap-2">
          <span class="text-xl">📝</span>
          Практические задания
        </h3>
        <div class="space-y-3">
          <div 
            v-for="item in checklistItems" 
            :key="item.checklistItemId"
            class="flex items-start gap-3 p-3 rounded-xl transition-all cursor-pointer select-none"
            :class="item.isCompleted ? 'bg-green-100/50 dark:bg-green-900/20' : 'bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/80 border border-gray-100 dark:border-gray-600 shadow-sm'"
            @click="toggleChecklistItem(item)"
          >
            <div 
              class="w-6 h-6 rounded-md border-2 flex items-center justify-center flex-shrink-0 transition-all"
              :class="item.isCompleted ? 'bg-green-500 border-green-500 text-white' : 'border-gray-300 dark:border-gray-500 bg-white dark:bg-gray-700'"
            >
              <svg v-if="item.isCompleted" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"></path>
              </svg>
            </div>
            <div class="flex-1">
              <p class="font-medium text-gray-800 dark:text-gray-200" :class="{ 'line-through text-gray-500 dark:text-gray-500': item.isCompleted }">
                {{ item.text }}
              </p>
              <span v-if="item.isRequired" class="text-[10px] uppercase tracking-wider font-bold text-yellow-600 dark:text-yellow-400 bg-yellow-100 dark:bg-yellow-900/30 px-1.5 py-0.5 rounded mt-1 inline-block">Обязательно</span>
              <span v-else class="text-[10px] uppercase tracking-wider font-bold text-gray-400 dark:text-gray-500 mt-1 inline-block">Дополнительно</span>
            </div>
          </div>
        </div>
        <p v-if="!allRequiredChecked" class="mt-4 text-xs text-red-600 dark:text-red-400 font-medium italic">
          * Завершение модуля доступно только после выполнения всех обязательных заданий.
        </p>
      </div>

      <!-- Actions -->
      <div class="card bg-gray-50 dark:bg-gray-700/50 border-t-4" :class="hasPassedBefore ? 'border-primary-500' : 'border-gray-200 dark:border-gray-700'">
        <div class="flex flex-col sm:flex-row items-center justify-between gap-4">
          <div>
            <p v-if="!hasTest" class="text-sm text-gray-600 dark:text-gray-400 font-medium">
              Пожалуйста, подтвердите полное ознакомление с материалом.
            </p>
            <template v-else>
               <p class="text-sm text-gray-800 dark:text-gray-200 font-medium font-bold">Тестирование по модулю</p>
               <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">Обязательно изучите материал выше, прежде чем приступать к проверке знаний.</p>
               <p class="text-xs text-primary-600 dark:text-primary-400 mt-1 flex items-center gap-1 font-semibold">
                 <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                 Всего вопросов в тесте: {{ module.questionCount }} 
               </p>
            </template>
          </div>
          
          <div class="flex flex-col sm:flex-row space-y-2 sm:space-y-0 sm:space-x-3 w-full sm:w-auto">
            <!-- МОДУЛЬ БЕЗ ТЕСТА -->
            <template v-if="!hasTest">
              <button
                v-if="moduleStatus !== 'Завершён'"
                @click="markAsRead"
                :disabled="isMarking || !allRequiredChecked"
                class="btn-primary w-full sm:w-auto disabled:bg-gray-300 disabled:cursor-not-allowed"
              >
                {{ isMarking ? 'Сохранение...' : '✅ Ознакомлен' }}
              </button>
              <button
                v-else
                class="btn-secondary bg-green-50 text-green-700 hover:bg-green-100 transition-all font-bold tracking-wide shadow w-full sm:w-auto"
                disabled
              >
                ✅ Модуль завершён
              </button>
            </template>

            <!-- МОДУЛЬ С ТЕСТОМ -->
            <template v-else>
              <button
                v-if="!isReadyForTest && !hasPassedBefore"
                @click="isReadyForTest = true"
                :disabled="!allRequiredChecked"
                class="btn-secondary w-full sm:w-auto border-gray-300 text-gray-700 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                👀 Я всё изучил
              </button>
              
              <button
                @click="startTest"
                :disabled="(!isReadyForTest && !hasPassedBefore) || !allRequiredChecked"
                class="w-full sm:w-auto transition-all shadow-md"
                :class="(isReadyForTest || hasPassedBefore) && allRequiredChecked ? 'btn-primary' : 'bg-gray-300 text-gray-500 py-2.5 px-4 rounded-xl cursor-not-allowed font-medium'"
              >
                {{ hasPassedBefore ? 'Пройти тест повторно' : '🚀 Начать тест' }}
              </button>
            </template>
          </div>
        </div>
      </div>

      <!-- Previous Attempts -->
      <div v-if="previousAttempts.length > 0" class="card">
        <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100 mb-4">Предыдущие попытки</h2>
        <div class="space-y-3">
          <div
            v-for="attempt in previousAttempts"
            :key="attempt.attemptId"
            class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700/50 rounded-lg transition-colors"
          >
            <div>
              <p class="font-medium text-gray-900 dark:text-gray-100">
                Попытка {{ attempt.attemptNumber }}
              </p>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {{ formatDate(attempt.attemptDate) }}
              </p>
            </div>
            <div class="text-right">
              <p
                :class="{
                  'text-green-600 dark:text-green-400 font-bold': attempt.isPassed,
                  'text-red-600 dark:text-red-400 font-bold': !attempt.isPassed
                }"
              >
                {{ Math.round(attempt.score) }}%
              </p>
              <p
                :class="{
                  'text-green-600 dark:text-green-400 text-sm': attempt.isPassed,
                  'text-red-600 dark:text-red-400 text-sm': !attempt.isPassed
                }"
              >
                {{ attempt.isPassed ? 'Сдано' : 'Не сдано' }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Error State -->
    <div v-else class="card text-center py-12">
      <p class="text-red-600">Модуль не найден</p>
      <button @click="$router.back()" class="mt-4 btn-secondary">
        Вернуться назад
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useProgressStore } from '../stores/progress'
import { modulesApi, testAttemptsApi, checklistsApi } from '../api/services'
import ModuleRoadmap from '../components/ModuleRoadmap.vue'
import confetti from 'canvas-confetti'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const progressStore = useProgressStore()

const module = ref(null)
const isLoading = ref(true)
const isMarking = ref(false)
const previousAttempts = ref([])
const isReadyForTest = ref(false)
const checklistItems = ref([])

const hasTest = computed(() => {
  if (!module.value) return false
  return module.value.questionCount > 0
})

const hasChecklist = computed(() => checklistItems.value.length > 0)
const allRequiredChecked = computed(() => {
  return checklistItems.value.filter(i => i.isRequired).every(i => i.isCompleted)
})

const hasPassedBefore = computed(() => {
  return previousAttempts.value.some(a => a.isPassed)
})

const moduleStatus = computed(() => {
  const mod = progressStore.modules.find(m => m.moduleId === module.value?.moduleId)
  return mod ? mod.status : 'Не начат'
})

onMounted(async () => {
  await loadModule()  // Load module first
  await loadAttempts()  // Then load attempts
  await loadChecklist()  // Then load checklist (module is now available)
})

const loadModule = async () => {
  try {
    isLoading.value = true
    const response = await modulesApi.getById(route.params.id)
    module.value = response.data
  } catch (error) {
    console.error('Failed to load module:', error)
  } finally {
    isLoading.value = false
  }
}

const loadAttempts = async () => {
  if (!authStore.currentUser) return
  try {
    const response = await testAttemptsApi.getModuleAttempts(
      parseInt(route.params.id),
      authStore.currentUser.userId
    )
    previousAttempts.value = response.data
  } catch (error) {
    console.error('Failed to load attempts:', error)
  }
}

const loadChecklist = async () => {
  if (!authStore.currentUser || !module.value) return
  try {
    const response = await checklistsApi.getModuleChecklist(module.value.moduleId, authStore.currentUser.userId)
    checklistItems.value = response.data
  } catch (error) {
    console.error('Failed to load checklist:', error)
  }
}

const toggleChecklistItem = async (item) => {
  if (!authStore.currentUser) return
  item.isCompleted = !item.isCompleted
  try {
    await checklistsApi.toggleItem({
      userId: authStore.currentUser.userId,
      checklistItemId: item.checklistItemId,
      isCompleted: item.isCompleted
    })
  } catch (error) {
    console.error('Failed to toggle checklist item:', error)
    item.isCompleted = !item.isCompleted // Rollback
  }
}

const markAsRead = async () => {
  if (!authStore.currentUser || !module.value) return
  try {
    isMarking.value = true
    await progressStore.markModuleAsRead(
      authStore.currentUser.userId,
      module.value.moduleId
    )
    // Refresh progress
    await progressStore.fetchUserProgress(authStore.currentUser.userId)
    
    const end = Date.now() + 2000;
    const colors = ['#004746', '#117f6b', '#4ade80', '#f59e0b'];

    (function frame() {
      confetti({
        particleCount: 3,
        angle: 60,
        spread: 55,
        origin: { x: 0 },
        colors: colors
      });
      confetti({
        particleCount: 3,
        angle: 120,
        spread: 55,
        origin: { x: 1 },
        colors: colors
      });

      if (Date.now() < end) {
        requestAnimationFrame(frame);
      }
    }());

    setTimeout(() => {
      router.push('/');
    }, 2500);
  } catch (error) {
    console.error('Failed to mark as read:', error)
    alert('Ошибка при сохранении. Попробуйте снова.')
  } finally {
    isMarking.value = false
  }
}

const startTest = () => {
  router.push(`/module/${route.params.id}/test`)
}

const formatContent = (content) => {
  if (!content) return ''
  // If the content comes from Quill, it already has HTML tags like <p>, <h1>, etc.
  if (content.includes('<p>') || content.includes('<h1>') || content.includes('<ul>')) {
    return content
  }
  // Otherwise it's plain text legacy content, add <br> tags.
  return content.replace(/\n/g, '<br>')
}

const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleString('ru-RU', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

</script>



