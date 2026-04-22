<template>
  <div class="space-y-6">
    <div class="flex justify-between items-center">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Отчёты</h1>
    </div>

    <!-- Tabs -->
    <div class="border-b border-gray-200 dark:border-gray-700">
      <nav class="-mb-px flex space-x-8">
        <button
          @click="activeTab = 'progress'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'progress' 
            ? 'border-primary-500 text-primary-600 dark:text-primary-400' 
            : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
        >
          Прогресс онбординга
        </button>
        <button
          @click="activeTab = 'tests'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'tests' 
            ? 'border-primary-500 text-primary-600 dark:text-primary-400' 
            : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
        >
          Результаты тестов
        </button>
        <button
          v-if="authStore.isManager || authStore.isHR"
          @click="activeTab = 'department'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'department' 
            ? 'border-primary-500 text-primary-600 dark:text-primary-400' 
            : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
        >
          По подразделению
        </button>
      </nav>
    </div>

    <!-- Progress Report -->
    <div v-if="activeTab === 'progress'" class="space-y-4">
      <div class="card">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white">Прогресс онбординга</h2>
          <div class="flex items-center space-x-3">
            <select
              v-model="selectedUserId"
              @change="loadProgressReport"
              class="input w-auto"
            >
              <option value="">Выберите сотрудника</option>
              <option
                v-for="user in users"
                :key="user.userId"
                :value="user.userId"
              >
                {{ user.fullName }} ({{ user.departmentName }})
              </option>
            </select>
            <div v-if="selectedUserId" class="flex space-x-2">
              <button @click="exportProgress('excel')" class="inline-flex items-center px-3 py-1.5 border border-transparent text-sm font-medium rounded shadow-sm text-white bg-green-600 hover:bg-green-700">Excel</button>
              <button @click="exportProgress('pdf')" class="inline-flex items-center px-3 py-1.5 border border-transparent text-sm font-medium rounded shadow-sm text-white bg-red-600 hover:bg-red-700">PDF</button>
            </div>
          </div>
        </div>

        <div v-if="progressReport" class="space-y-4">
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div class="bg-gray-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">Статус</p>
              <p class="text-lg font-semibold text-gray-900">{{ progressReport.onboardingStatus }}</p>
            </div>
            <div class="bg-gray-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">Прогресс</p>
              <p class="text-lg font-semibold text-primary-600">
                {{ Math.round(progressReport.progressPercentage) }}%
              </p>
            </div>
            <div class="bg-gray-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">Завершено модулей</p>
              <p class="text-lg font-semibold text-gray-900">
                {{ progressReport.completedMandatoryModules }} / {{ progressReport.totalMandatoryModules }}
              </p>
            </div>
            <div class="bg-gray-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">Дата начала</p>
              <p class="text-lg font-semibold text-gray-900">
                {{ formatDate(progressReport.onboardingStartDate) }}
              </p>
            </div>
          </div>

          <div class="mt-6">
            <h3 class="text-lg font-semibold text-gray-900 mb-4">Модули</h3>
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Модуль</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Статус</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Попыток</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Результат</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Дата завершения</th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  <tr v-for="module in progressReport.moduleStatuses" :key="module.moduleId">
                    <td class="px-6 py-4 whitespace-nowrap">
                      <div class="flex items-center">
                        <span class="text-sm font-medium text-gray-900">{{ module.moduleTitle }}</span>
                        <span
                          v-if="module.isMandatory"
                          class="ml-2 px-2 py-1 text-xs font-medium bg-red-100 text-red-800 rounded"
                        >
                          Обязательный
                        </span>
                      </div>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                      <span
                        class="px-2 py-1 text-xs font-medium rounded-full"
                        :class="{
                          'bg-green-100 text-green-800': module.status === 'Завершён',
                          'bg-yellow-100 text-yellow-800': module.status === 'В процессе',
                          'bg-gray-100 text-gray-800': module.status === 'Не начат'
                        }"
                      >
                        {{ module.status }}
                      </span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {{ module.attemptsCount }}
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                      <span
                        v-if="module.bestScore !== null"
                        class="text-sm font-medium"
                        :class="{
                          'text-green-600': module.isPassed,
                          'text-red-600': !module.isPassed
                        }"
                      >
                        {{ Math.round(module.bestScore) }}%
                      </span>
                      <span v-else class="text-sm text-gray-400">—</span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {{ formatDate(module.completionDate) }}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Test Results Report -->
    <div v-if="activeTab === 'tests'" class="space-y-4">
      <div class="card">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-xl font-semibold text-gray-900">Результаты тестов</h2>
          <div class="flex space-x-3">
            <select
              v-model="testFilterUserId"
              @change="loadTestResults"
              class="input w-auto"
            >
              <option value="">Все сотрудники</option>
              <option
                v-for="user in users"
                :key="user.userId"
                :value="user.userId"
              >
                {{ user.fullName }}
              </option>
            </select>
            <div class="flex space-x-2">
              <button @click="exportTests('excel')" class="inline-flex items-center px-3 py-1.5 border border-transparent text-sm font-medium rounded shadow-sm text-white bg-green-600 hover:bg-green-700">Excel</button>
              <button @click="exportTests('pdf')" class="inline-flex items-center px-3 py-1.5 border border-transparent text-sm font-medium rounded shadow-sm text-white bg-red-600 hover:bg-red-700">PDF</button>
            </div>
          </div>
        </div>

        <div v-if="testResults.length > 0" class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Сотрудник</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Модуль</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Дата</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Попытка</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Результат</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Статус</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="result in testResults" :key="result.attemptId">
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  {{ result.fullName }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ result.moduleTitle }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ formatDate(result.attemptDate) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ result.attemptNumber }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  {{ result.correctAnswers }} / {{ result.totalQuestions }} ({{ Math.round(result.score) }}%)
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span
                    class="px-2 py-1 text-xs font-medium rounded-full"
                    :class="{
                      'bg-green-100 text-green-800': result.isPassed,
                      'bg-red-100 text-red-800': !result.isPassed
                    }"
                  >
                    {{ result.isPassed ? 'Сдано' : 'Не сдано' }}
                  </span>
                  
                  <button 
                    v-if="!result.isPassed && (authStore.isAdmin || authStore.isHR)"
                    @click="resetUserAttempts(result.userId, result.moduleId)"
                    class="ml-3 text-red-600 hover:text-red-900 text-xs font-medium underline"
                    title="Сбросить все попытки по этому модулю"
                  >
                    Сбросить
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-else class="text-center py-8 text-gray-500">
          Нет данных для отображения
        </div>
      </div>
    </div>

    <!-- Department Report -->
    <div v-if="activeTab === 'department'" class="space-y-4">
      <div class="card">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-xl font-semibold text-gray-900">Отчёт по подразделению</h2>
          <div class="flex items-center space-x-3">
            <select
              v-model="selectedDepartmentId"
              @change="loadDepartmentReport"
              class="input w-auto"
            >
              <option value="">Выберите подразделение</option>
              <option
                v-for="dept in departments"
                :key="dept.departmentId"
                :value="dept.departmentId"
              >
                {{ dept.name }}
              </option>
            </select>
            <div v-if="selectedDepartmentId" class="flex space-x-2">
              <button @click="exportDepartment('excel')" class="inline-flex items-center px-3 py-1.5 border border-transparent text-sm font-medium rounded shadow-sm text-white bg-green-600 hover:bg-green-700">Excel</button>
              <button @click="exportDepartment('pdf')" class="inline-flex items-center px-3 py-1.5 border border-transparent text-sm font-medium rounded shadow-sm text-white bg-red-600 hover:bg-red-700">PDF</button>
            </div>
          </div>
        </div>

        <div v-if="departmentReport" class="space-y-4">
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div class="bg-gray-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">Всего сотрудников</p>
              <p class="text-lg font-semibold text-gray-900">{{ departmentReport.totalUsers }}</p>
            </div>
            <div class="bg-gray-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">В процессе</p>
              <p class="text-lg font-semibold text-yellow-600">{{ departmentReport.usersInProgress }}</p>
            </div>
            <div class="bg-gray-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">Завершено</p>
              <p class="text-lg font-semibold text-green-600">{{ departmentReport.usersCompleted }}</p>
            </div>
            <div class="bg-gray-50 p-4 rounded-lg">
              <p class="text-sm text-gray-600">Средний прогресс</p>
              <p class="text-lg font-semibold text-primary-600">
                {{ Math.round(departmentReport.averageProgressPercentage) }}%
              </p>
            </div>
          </div>

          <div class="mt-6">
            <h3 class="text-lg font-semibold text-gray-900 mb-4">Сотрудники</h3>
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Сотрудник</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Статус</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Прогресс</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Дата завершения</th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  <tr v-for="user in departmentReport.users" :key="user.userId">
                    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {{ user.fullName }}
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                      <span
                        class="px-2 py-1 text-xs font-medium rounded-full"
                        :class="{
                          'bg-green-100 text-green-800': user.onboardingStatus === 'Завершён',
                          'bg-yellow-100 text-yellow-800': user.onboardingStatus === 'В процессе',
                          'bg-gray-100 text-gray-800': user.onboardingStatus === 'Не начат'
                        }"
                      >
                        {{ user.onboardingStatus }}
                      </span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                      <div class="flex items-center">
                        <div class="w-24 bg-gray-200 rounded-full h-2 mr-2">
                          <div
                            class="bg-primary-600 h-2 rounded-full"
                            :style="{ width: `${user.progressPercentage}%` }"
                          ></div>
                        </div>
                        <span class="text-sm text-gray-600">{{ Math.round(user.progressPercentage) }}%</span>
                      </div>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {{ formatDate(user.completionDate) }}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import { reportsApi, usersApi, departmentsApi, testAttemptsApi } from '../api/services'

const authStore = useAuthStore()

const activeTab = ref('progress')
const users = ref([])
const departments = ref([])
const selectedUserId = ref('')
const testFilterUserId = ref('')
const selectedDepartmentId = ref('')
const progressReport = ref(null)
const testResults = ref([])
const departmentReport = ref(null)

onMounted(async () => {
  await loadUsers()
  await loadDepartments()
  
  // Set default user for mentors (find users with this mentor)
  if (authStore.isMentor) {
    const mentees = users.value.filter(u => u.mentorId === authStore.currentUser?.userId)
    if (mentees.length > 0) {
      selectedUserId.value = mentees[0].userId.toString()
      await loadProgressReport()
    }
  }
  
  // Set default department for managers
  if (authStore.isManager && authStore.currentUser?.departmentId) {
    selectedDepartmentId.value = authStore.currentUser.departmentId.toString()
    await loadDepartmentReport()
  }
  
  await loadTestResults()
})

const loadUsers = async () => {
  try {
    const response = await usersApi.getAll()
    users.value = response.data
  } catch (error) {
    console.error('Failed to load users:', error)
  }
}

const loadDepartments = async () => {
  try {
    const response = await departmentsApi.getAll()
    departments.value = response.data
  } catch (error) {
    console.error('Failed to load departments:', error)
  }
}

const loadProgressReport = async () => {
  if (!selectedUserId.value) {
    progressReport.value = null
    return
  }
  try {
    const response = await reportsApi.getOnboardingProgress(parseInt(selectedUserId.value))
    progressReport.value = response.data
  } catch (error) {
    console.error('Failed to load progress report:', error)
  }
}

const loadTestResults = async () => {
  try {
    const params = {}
    if (testFilterUserId.value) {
      params.userId = parseInt(testFilterUserId.value)
    }
    const response = await reportsApi.getTestResults(params)
    testResults.value = response.data
  } catch (error) {
    console.error('Failed to load test results:', error)
  }
}

const resetUserAttempts = async (userId, moduleId) => {
  if (!confirm('Вы уверены, что хотите обнулить все попытки сотрудника по этому модулю? Сотрудник сможет пройти тест заново.')) return
  
  try {
    await testAttemptsApi.resetAttempts(userId, moduleId)
    await loadTestResults() // Reload table
  } catch (error) {
    console.error('Failed to reset attempts:', error)
    alert('Произошла ошибка при сбросе попыток')
  }
}

const loadDepartmentReport = async () => {
  if (!selectedDepartmentId.value) {
    departmentReport.value = null
    return
  }
  try {
    const response = await reportsApi.getDepartmentReport(parseInt(selectedDepartmentId.value))
    departmentReport.value = response.data
  } catch (error) {
    console.error('Failed to load department report:', error)
  }
}

const formatDate = (dateString) => {
  if (!dateString) return '—'
  const date = new Date(dateString)
  return date.toLocaleDateString('ru-RU')
}

const downloadFile = (response, defaultFilename) => {
  const url = window.URL.createObjectURL(new Blob([response.data]))
  const link = document.createElement('a')
  link.href = url
  let filename = defaultFilename
  const contentDisposition = response.headers['content-disposition']
  if (contentDisposition) {
    const filenameMatch = contentDisposition.match(/filename="?([^"]+)"?/)
    if (filenameMatch && filenameMatch.length === 2) filename = filenameMatch[1]
  }
  link.setAttribute('download', filename)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

const exportProgress = async (format) => {
  if (!selectedUserId.value) return
  try {
    const response = await reportsApi.exportOnboardingProgress(parseInt(selectedUserId.value), format)
    downloadFile(response, `Progress_${format.toUpperCase()}.` + (format === 'excel' ? 'xlsx' : 'pdf'))
  } catch (error) {
    console.error('Failed to export:', error)
  }
}

const exportTests = async (format) => {
  try {
    const params = {}
    if (testFilterUserId.value) params.userId = parseInt(testFilterUserId.value)
    const response = await reportsApi.exportTestResults(params, format)
    downloadFile(response, `TestResults_${format.toUpperCase()}.` + (format === 'excel' ? 'xlsx' : 'pdf'))
  } catch (error) {
    console.error('Failed to export:', error)
  }
}

const exportDepartment = async (format) => {
  if (!selectedDepartmentId.value) return
  try {
    const response = await reportsApi.exportDepartmentReport(parseInt(selectedDepartmentId.value), format)
    downloadFile(response, `Department_${format.toUpperCase()}.` + (format === 'excel' ? 'xlsx' : 'pdf'))
  } catch (error) {
    console.error('Failed to export:', error)
  }
}

</script>

