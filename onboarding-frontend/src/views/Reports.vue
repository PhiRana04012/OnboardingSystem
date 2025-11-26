<template>
  <div class="space-y-6">
    <div class="flex justify-between items-center">
      <h1 class="text-3xl font-bold text-gray-900">Отчёты</h1>
    </div>

    <!-- Tabs -->
    <div class="border-b border-gray-200">
      <nav class="-mb-px flex space-x-8">
        <button
          @click="activeTab = 'progress'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'progress' 
            ? 'border-primary-500 text-primary-600' 
            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'"
        >
          Прогресс онбординга
        </button>
        <button
          @click="activeTab = 'tests'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'tests' 
            ? 'border-primary-500 text-primary-600' 
            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'"
        >
          Результаты тестов
        </button>
        <button
          v-if="authStore.isManager || authStore.isHR"
          @click="activeTab = 'department'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'department' 
            ? 'border-primary-500 text-primary-600' 
            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'"
        >
          По подразделению
        </button>
      </nav>
    </div>

    <!-- Progress Report -->
    <div v-if="activeTab === 'progress'" class="space-y-4">
      <div class="card">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-xl font-semibold text-gray-900">Прогресс онбординга</h2>
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
import { reportsApi, usersApi, departmentsApi } from '../api/services'

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
</script>

