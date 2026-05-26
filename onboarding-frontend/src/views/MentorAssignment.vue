<template>
  <div class="space-y-6">
    <div class="flex justify-between items-center">
      <div class="flex items-center gap-3">
        <button
          @click="goBackToAdmin"
          class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-gray-700 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white"
          title="Вернуться в администрирование"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <h1 class="text-4xl font-bold text-gray-900 dark:text-white">
          Управление наставниками
        </h1>
      </div>
    </div>

    <!-- Mentor Selection -->
    <div class="card">
      <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-6">Выбрать наставника</h2>

      <div class="space-y-4">
        <div v-if="authStore.isFullAdmin" class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Подразделение
            </label>
            <div class="relative">
              <input
                v-model="searchDepartment"
                type="text"
                placeholder="Поиск подразделения..."
                class="input w-full pr-10"
                @focus="showDepartmentDropdown = true"
                @input="showDepartmentDropdown = true"
              />
              <button
                v-if="selectedDepartmentId || searchDepartment"
                type="button"
                @click="clearDepartmentFilter"
                class="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 px-1"
                title="Сбросить фильтр"
              >
                ✕
              </button>
              <div
                v-if="showDepartmentDropdown"
                class="absolute top-full left-0 right-0 mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-lg z-20 max-h-64 overflow-y-auto"
              >
                <div
                  @click="selectAllDepartments"
                  class="px-4 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer transition-colors border-b border-gray-100 dark:border-gray-700"
                >
                  <div class="font-medium text-gray-900 dark:text-white">Все подразделения</div>
                </div>
                <div
                  v-for="dept in filteredDepartments"
                  :key="dept.departmentId"
                  @click="selectDepartment(dept)"
                  class="px-4 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer transition-colors"
                  :class="{ 'bg-primary-50 dark:bg-primary-900/30': selectedDepartmentId === dept.departmentId }"
                >
                  <div class="font-medium text-gray-900 dark:text-white">{{ dept.name }}</div>
                </div>
                <div
                  v-if="filteredDepartments.length === 0"
                  class="px-4 py-3 text-sm text-gray-500 dark:text-gray-400 text-center"
                >
                  Подразделения не найдены
                </div>
              </div>
            </div>
          </div>
          <div class="flex items-end">
            <p class="text-sm text-gray-600 dark:text-gray-400 pb-2">
              Найдено сотрудников: <strong>{{ mentorCandidates.length }}</strong>
              <span v-if="selectedDepartmentId"> в выбранном подразделении</span>
            </p>
          </div>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Наставник
          </label>
          <div class="relative">
            <input
              v-model="searchMentor"
              type="text"
              placeholder="Начните вводить ФИО наставника..."
              class="input w-full"
              @focus="showMentorDropdown = true"
            />
            <div
              v-if="showMentorDropdown && filteredMentors.length > 0"
              class="absolute top-full left-0 right-0 mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-lg z-10 max-h-64 overflow-y-auto"
            >
              <div
                v-for="mentor in filteredMentors"
                :key="mentor.userId"
                @click="selectMentor(mentor)"
                class="px-4 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer transition-colors"
              >
                <div class="font-medium text-gray-900 dark:text-white">{{ mentor.fullName }}</div>
                <div class="text-sm text-gray-500 dark:text-gray-400">{{ mentor.departmentName }}</div>
              </div>
            </div>
          </div>
        </div>

        <div v-if="selectedMentor" class="bg-blue-50 dark:bg-blue-900/30 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
          <div class="flex justify-between items-start">
            <div>
              <h3 class="font-semibold text-gray-900 dark:text-white">{{ selectedMentor.fullName }}</h3>
              <p class="text-sm text-gray-600 dark:text-gray-400">{{ selectedMentor.departmentName }}</p>
              <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Текущих подопечных: <strong>{{ currentMenteeCount }}</strong>
              </p>
            </div>
            <button
              @click="clearSelection"
              class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
            >
              ✕
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Employees List -->
    <div v-if="selectedMentor" class="card">
      <div class="mb-6">
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
            Сотрудники отдела «{{ selectedMentor.departmentName }}»
          </h2>
          <div class="text-sm text-gray-600 dark:text-gray-400 flex flex-wrap gap-x-3 gap-y-1">
            <span>Показано: <strong>{{ filteredEmployees.length }}</strong> из {{ availableEmployees.length }}</span>
            <span>Выбрано: <strong>{{ selectedMenteeIds.length }}</strong></span>
          </div>
        </div>

        <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-900/40 border border-gray-200 dark:border-gray-700 space-y-3 mb-4">
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-3">
            <div class="sm:col-span-2 xl:col-span-2">
              <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">Поиск</label>
              <input v-model="searchEmployee" type="text" placeholder="ФИО, должность, email..." class="input w-full" />
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">Должность</label>
              <select v-model="filterJobTitle" class="input w-full">
                <option value="all">Все должности</option>
                <option v-for="title in uniqueJobTitles" :key="title" :value="title">{{ title }}</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">Статус онбординга</label>
              <select v-model="filterOnboardingStatus" class="input w-full">
                <option value="all">Все статусы</option>
                <option v-for="st in uniqueOnboardingStatuses" :key="st" :value="st">{{ st }}</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">Наставник</label>
              <select v-model="filterMentorStatus" class="input w-full">
                <option value="all">Все</option>
                <option value="none">Без наставника</option>
                <option value="current">Подопечные этого наставника</option>
                <option value="other">С другим наставником</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">Выбор</label>
              <select v-model="filterSelection" class="input w-full">
                <option value="all">Все</option>
                <option value="selected">Только выбранные</option>
                <option value="unselected">Только не выбранные</option>
              </select>
            </div>
          </div>
          <div class="flex flex-wrap items-center gap-2">
            <button type="button" @click="filterMentorStatus = 'none'" class="text-xs px-2.5 py-1 rounded-full border border-gray-300 dark:border-gray-600 hover:bg-white dark:hover:bg-gray-800 transition-colors" :class="filterMentorStatus === 'none' ? 'bg-primary-100 dark:bg-primary-900/40 border-primary-400 text-primary-700 dark:text-primary-300' : 'text-gray-600 dark:text-gray-400'">Без наставника</button>
            <button type="button" @click="filterMentorStatus = 'current'" class="text-xs px-2.5 py-1 rounded-full border border-gray-300 dark:border-gray-600 hover:bg-white dark:hover:bg-gray-800 transition-colors" :class="filterMentorStatus === 'current' ? 'bg-primary-100 dark:bg-primary-900/40 border-primary-400 text-primary-700 dark:text-primary-300' : 'text-gray-600 dark:text-gray-400'">Текущие подопечные</button>
            <button type="button" @click="filterSelection = 'selected'" class="text-xs px-2.5 py-1 rounded-full border border-gray-300 dark:border-gray-600 hover:bg-white dark:hover:bg-gray-800 transition-colors" :class="filterSelection === 'selected' ? 'bg-primary-100 dark:bg-primary-900/40 border-primary-400 text-primary-700 dark:text-primary-300' : 'text-gray-600 dark:text-gray-400'">Выбранные</button>
            <button v-if="hasActiveEmployeeFilters" type="button" @click="resetEmployeeFilters" class="text-xs px-2.5 py-1 rounded-full text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors sm:ml-auto">Сбросить фильтры</button>
          </div>
        </div>

        <label class="flex items-center gap-3 mb-4 cursor-pointer">
          <input type="checkbox" :checked="isAllFilteredSelected" @change="toggleSelectAllFiltered" class="w-4 h-4 rounded border-gray-300 dark:border-gray-600 cursor-pointer" />
          <span class="text-sm font-medium text-gray-700 dark:text-gray-300">
            {{ isAllFilteredSelected ? 'Снять выбор с отображаемых' : 'Выбрать всех отображаемых' }}
            <span v-if="filteredEmployees.length > 0" class="text-gray-500 dark:text-gray-400 font-normal"> ({{ filteredEmployees.length }})</span>
          </span>
        </label>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="space-y-3">
        <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
        <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
        <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
      </div>

      <!-- Employees Grid -->
      <div v-else class="grid gap-3 max-h-96 overflow-y-auto">
        <div
          v-if="availableEmployees.length === 0"
          class="text-center py-8 text-gray-500 dark:text-gray-400"
        >
          В этом отделе нет других сотрудников
        </div>

        <div
          v-else-if="filteredEmployees.length === 0"
          class="text-center py-8 text-gray-500 dark:text-gray-400"
        >
          Никого не найдено по заданным фильтрам
        </div>

        <label
          v-for="employee in filteredEmployees"
          :key="employee.userId"
          class="flex items-center space-x-3 p-3 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors"
        >
          <input
            type="checkbox"
            :checked="selectedMenteeIds.includes(employee.userId)"
            @change="toggleMentee(employee.userId)"
            class="w-4 h-4 rounded border-gray-300 dark:border-gray-600 cursor-pointer"
          />
          <div class="flex-1">
            <div class="font-medium text-gray-900 dark:text-white">{{ employee.fullName }}</div>
            <div class="text-sm text-gray-600 dark:text-gray-400">{{ employee.jobTitle }}</div>
            <div class="text-xs text-gray-500 dark:text-gray-500 mt-1">
              <span>Найм: {{ formatDate(employee.hireDate) }}</span>
              <span v-if="employee.onboardingStatus" class="ml-2">{{ employee.onboardingStatus }}</span>
            </div>
          </div>
          <div v-if="employee.currentMentorId === selectedMentor?.userId" class="text-xs bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-300 px-2 py-1 rounded whitespace-nowrap flex-shrink-0">
            Подопечный
          </div>
          <div v-else-if="employee.currentMentorId" class="text-xs bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-300 px-2 py-1 rounded whitespace-nowrap flex-shrink-0">
            Другой наставник
          </div>
        </label>
      </div>
    </div>

    <!-- Action Buttons -->
    <div v-if="selectedMentor" class="card bg-gradient-to-r from-primary-50 dark:from-gray-800 to-blue-50 dark:to-gray-800 border border-primary-200 dark:border-gray-700">
      <div class="flex justify-between items-center">
        <div>
          <h3 class="font-semibold text-gray-900 dark:text-white mb-2">Готово к сохранению</h3>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            {{ selectedMenteeIds.length }} сотрудников будут назначены подопечными
          </p>
        </div>
        <div class="flex space-x-3">
          <button
            @click="resetSelection"
            class="btn-secondary"
            :disabled="loading"
          >
            Отмена
          </button>
          <button
            @click="saveAssignment"
            class="btn-primary"
            :disabled="loading || selectedMenteeIds.length === currentMenteeCount"
          >
            <span v-if="loading" class="flex items-center space-x-2">
              <svg class="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              <span>Сохранение...</span>
            </span>
            <span v-else>Сохранить назначение</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Success Message -->
    <div
      v-if="successMessage"
      class="card bg-green-50 dark:bg-green-900/30 border border-green-200 dark:border-green-800"
    >
      <div class="flex items-start">
        <div class="flex-shrink-0">
          <svg class="h-5 w-5 text-green-400" fill="currentColor" viewBox="0 0 20 20">
            <path
              fill-rule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
              clip-rule="evenodd"
            />
          </svg>
        </div>
        <div class="ml-3">
          <p class="text-sm font-medium text-green-800 dark:text-green-200">
            {{ successMessage }}
          </p>
        </div>
      </div>
    </div>

    <!-- Error Message -->
    <div
      v-if="errorMessage"
      class="card bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800"
    >
      <div class="flex items-start">
        <div class="flex-shrink-0">
          <svg class="h-5 w-5 text-red-400" fill="currentColor" viewBox="0 0 20 20">
            <path
              fill-rule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
              clip-rule="evenodd"
            />
          </svg>
        </div>
        <div class="ml-3">
          <p class="text-sm font-medium text-red-800 dark:text-red-200">
            {{ errorMessage }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { usersApi, departmentsApi } from '../api/services'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const selectedMentor = ref(null)
const searchMentor = ref('')
const showMentorDropdown = ref(false)
const allUsers = ref([])
const departments = ref([])
const selectedDepartmentId = ref('')
const searchDepartment = ref('')
const showDepartmentDropdown = ref(false)
const availableEmployees = ref([])
const selectedMenteeIds = ref([])
const loading = ref(false)
const successMessage = ref('')
const errorMessage = ref('')
const currentMenteeCount = ref(0)
const searchEmployee = ref('')
const filterJobTitle = ref('all')
const filterOnboardingStatus = ref('all')
const filterMentorStatus = ref('all')
const filterSelection = ref('all')

const uniqueJobTitles = computed(() => {
  const titles = new Set(availableEmployees.value.map(e => e.jobTitle).filter(Boolean))
  return [...titles].sort((a, b) => a.localeCompare(b, 'ru'))
})

const uniqueOnboardingStatuses = computed(() => {
  const statuses = new Set(availableEmployees.value.map(e => e.onboardingStatus).filter(Boolean))
  return [...statuses].sort((a, b) => a.localeCompare(b, 'ru'))
})

const filteredEmployees = computed(() => {
  let list = availableEmployees.value
  const q = searchEmployee.value.trim().toLowerCase()

  if (q) {
    list = list.filter(e =>
      e.fullName?.toLowerCase().includes(q) ||
      e.jobTitle?.toLowerCase().includes(q) ||
      e.email?.toLowerCase().includes(q)
    )
  }

  if (filterJobTitle.value !== 'all') {
    list = list.filter(e => e.jobTitle === filterJobTitle.value)
  }

  if (filterOnboardingStatus.value !== 'all') {
    list = list.filter(e => e.onboardingStatus === filterOnboardingStatus.value)
  }

  const mentorId = selectedMentor.value?.userId
  if (filterMentorStatus.value === 'none') {
    list = list.filter(e => !e.currentMentorId)
  } else if (filterMentorStatus.value === 'current') {
    list = list.filter(e => e.currentMentorId === mentorId)
  } else if (filterMentorStatus.value === 'other') {
    list = list.filter(e => e.currentMentorId && e.currentMentorId !== mentorId)
  }

  if (filterSelection.value === 'selected') {
    list = list.filter(e => selectedMenteeIds.value.includes(e.userId))
  } else if (filterSelection.value === 'unselected') {
    list = list.filter(e => !selectedMenteeIds.value.includes(e.userId))
  }

  return list
})

const isAllFilteredSelected = computed(() =>
  filteredEmployees.value.length > 0 &&
  filteredEmployees.value.every(e => selectedMenteeIds.value.includes(e.userId))
)

const hasActiveEmployeeFilters = computed(() =>
  !!searchEmployee.value.trim() ||
  filterJobTitle.value !== 'all' ||
  filterOnboardingStatus.value !== 'all' ||
  filterMentorStatus.value !== 'all' ||
  filterSelection.value !== 'all'
)

const filteredDepartments = computed(() => {
  const query = searchDepartment.value.trim().toLowerCase()
  if (!query) return departments.value
  return departments.value.filter(d =>
    (d.name || '').toLowerCase().includes(query)
  )
})

const mentorCandidates = computed(() => {
  if (!authStore.isFullAdmin) {
    return allUsers.value.filter(
      u => (u.hasMentees || u.roles?.includes('Наставник'))
        && u.departmentId === authStore.currentUser?.departmentId
    )
  }
  if (!selectedDepartmentId.value) return allUsers.value
  return allUsers.value.filter(u => u.departmentId === selectedDepartmentId.value)
})

const filteredMentors = computed(() => {
  const list = mentorCandidates.value
  if (!searchMentor.value) return list

  const search = searchMentor.value.toLowerCase()
  return list.filter(m =>
    m.fullName.toLowerCase().includes(search) ||
    (m.departmentName || '').toLowerCase().includes(search)
  )
})

// Methods
const formatDate = (dateString) => {
  if (!dateString) return 'Не указано'
  const date = new Date(dateString)
  return date.toLocaleDateString('ru-RU')
}

const goBackToAdmin = () => {
  router.push({ name: 'Admin' })
}

const loadDepartments = async () => {
  try {
    const response = await departmentsApi.getAll()
    departments.value = response.data || []
  } catch (error) {
    console.error('Ошибка при загрузке подразделений:', error)
  }
}

const loadMentors = async () => {
  try {
    loading.value = true
    const response = await usersApi.getAll()
    allUsers.value = Array.isArray(response) ? response : response.data
  } catch (error) {
    console.error('Ошибка при загрузке наставников:', error)
    const errorMsg = error.response?.data?.message || error.message || 'Ошибка при загрузке наставников'
    errorMessage.value = typeof errorMsg === 'string' ? errorMsg : 'Ошибка при загрузке наставников'
    setTimeout(() => errorMessage.value = '', 5000)
  } finally {
    loading.value = false
  }
}

const onDepartmentFilterChange = () => {
  if (!selectedMentor.value || !selectedDepartmentId.value) return
  if (selectedMentor.value.departmentId !== selectedDepartmentId.value) {
    clearSelection()
  }
}

const selectAllDepartments = () => {
  selectedDepartmentId.value = ''
  searchDepartment.value = ''
  showDepartmentDropdown.value = false
  onDepartmentFilterChange()
}

const selectDepartment = (dept) => {
  selectedDepartmentId.value = dept.departmentId
  searchDepartment.value = dept.name
  showDepartmentDropdown.value = false
  onDepartmentFilterChange()
}

const clearDepartmentFilter = () => {
  selectAllDepartments()
}

const resetEmployeeFilters = () => {
  searchEmployee.value = ''
  filterJobTitle.value = 'all'
  filterOnboardingStatus.value = 'all'
  filterMentorStatus.value = 'all'
  filterSelection.value = 'all'
}

const selectMentor = async (mentor) => {
  selectedMentor.value = mentor
  searchMentor.value = mentor.fullName
  showMentorDropdown.value = false
  selectedMenteeIds.value = []
  resetEmployeeFilters()

  await loadEmployees()
}

const loadEmployees = async () => {
  if (!selectedMentor.value) return
  
  try {
    loading.value = true
    const response = await usersApi.getAll()
    const users = Array.isArray(response) ? response : response.data
    
    // Фильтруем сотрудников того же отдела, кроме самого наставника
    availableEmployees.value = users.filter(u =>
      u.departmentId === selectedMentor.value.departmentId &&
      u.userId !== selectedMentor.value.userId
    ).map(u => ({
      ...u,
      jobTitle: u.jobTitle?.title || 'Не указана',
      currentMentorId: u.mentorId,
      onboardingStatus: u.onboardingStatus
    }))
    
    // Загружаем текущих подопечных
    const menteesResponse = await usersApi.getMentees(selectedMentor.value.userId)
    const currentMentees = Array.isArray(menteesResponse) ? menteesResponse : (menteesResponse.data || [])
    currentMenteeCount.value = currentMentees.length
    selectedMenteeIds.value = currentMentees.map(m => m.userId)
  } catch (error) {
    console.error('Ошибка при загрузке сотрудников:', error)
    const errorMsg = error.response?.data?.message || error.message || 'Ошибка при загрузке списка сотрудников'
    errorMessage.value = typeof errorMsg === 'string' ? errorMsg : 'Ошибка при загрузке списка сотрудников'
    setTimeout(() => errorMessage.value = '', 5000)
  } finally {
    loading.value = false
  }
}

const toggleMentee = (userId) => {
  const index = selectedMenteeIds.value.indexOf(userId)
  if (index > -1) {
    selectedMenteeIds.value.splice(index, 1)
  } else {
    selectedMenteeIds.value.push(userId)
  }
}

const toggleSelectAllFiltered = () => {
  const filteredIds = filteredEmployees.value.map(e => e.userId)
  if (isAllFilteredSelected.value) {
    const remove = new Set(filteredIds)
    selectedMenteeIds.value = selectedMenteeIds.value.filter(id => !remove.has(id))
  } else {
    const ids = new Set([...selectedMenteeIds.value, ...filteredIds])
    selectedMenteeIds.value = [...ids]
  }
}

const clearSelection = () => {
  selectedMentor.value = null
  searchMentor.value = ''
  selectedMenteeIds.value = []
  availableEmployees.value = []
  currentMenteeCount.value = 0
  resetEmployeeFilters()
}

const resetSelection = () => {
  clearSelection()
  successMessage.value = ''
  errorMessage.value = ''
}

const saveAssignment = async () => {
  if (!selectedMentor.value) return
  
  try {
    loading.value = true
    successMessage.value = ''
    errorMessage.value = ''
    
    await usersApi.assignMentees(selectedMentor.value.userId, {
      mentorId: selectedMentor.value.userId,
      menteeIds: selectedMenteeIds.value
    })
    
    successMessage.value = `Успешно! Для наставника "${selectedMentor.value.fullName}" назначено ${selectedMenteeIds.value.length} подопечных`
    
    // Обновляем количество подопечных
    currentMenteeCount.value = selectedMenteeIds.value.length
    
    // Обновляем пользователя в auth store, чтобы обновился статус наставника
    if (authStore.currentUser.userId === selectedMentor.value.userId) {
      await authStore.refreshUser()
    }
    
    setTimeout(() => {
      successMessage.value = ''
    }, 5000)
  } catch (error) {
    console.error('Ошибка при сохранении назначений:', error)
    const errorMsg = error.response?.data?.message || error.response?.data || error.message || 'Ошибка при сохранении назначений'
    errorMessage.value = typeof errorMsg === 'string' ? errorMsg : 'Ошибка при сохранении назначений'
    setTimeout(() => errorMessage.value = '', 5000)
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await loadMentors()
  if (authStore.isFullAdmin) {
    await loadDepartments()
  }
})
</script>

<style scoped>
.btn-primary {
  @apply px-4 py-2 bg-primary-600 dark:bg-primary-500 text-white rounded-lg hover:bg-primary-700 dark:hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium;
}

.btn-secondary {
  @apply px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-900 dark:text-white rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium;
}

.input {
  @apply px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:focus:border-transparent outline-none;
}

.card {
  @apply bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6;
}
</style>
