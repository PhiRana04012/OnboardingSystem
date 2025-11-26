<template>
  <div class="space-y-6">
    <div class="flex justify-between items-center">
      <h1 class="text-3xl font-bold text-gray-900">Администрирование</h1>
    </div>

    <!-- Tabs -->
    <div class="border-b border-gray-200">
      <nav class="-mb-px flex space-x-8">
        <button
          @click="activeTab = 'modules'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'modules' 
            ? 'border-primary-500 text-primary-600' 
            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'"
        >
          Модули
        </button>
        <button
          @click="activeTab = 'users'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'users' 
            ? 'border-primary-500 text-primary-600' 
            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'"
        >
          Пользователи
        </button>
        <button
          @click="activeTab = 'questions'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'questions' 
            ? 'border-primary-500 text-primary-600' 
            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'"
        >
          Вопросы
        </button>
      </nav>
    </div>

    <!-- Modules Tab -->
    <div v-if="activeTab === 'modules'" class="space-y-4">
      <div class="flex justify-between items-center">
        <h2 class="text-xl font-semibold text-gray-900">Управление модулями</h2>
        <button @click="showModuleForm = true" class="btn-primary">
          Создать модуль
        </button>
      </div>

      <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <div
          v-for="module in modules"
          :key="module.moduleId"
          class="card hover:shadow-lg transition-shadow"
        >
          <h3 class="text-lg font-semibold text-gray-900 mb-2">{{ module.title }}</h3>
          <p class="text-sm text-gray-600 mb-4">{{ module.description }}</p>
          <div class="flex items-center space-x-2 mb-4">
            <span
              v-if="module.isMandatory"
              class="px-2 py-1 text-xs font-medium bg-red-100 text-red-800 rounded"
            >
              Обязательный
            </span>
            <span class="px-2 py-1 text-xs font-medium bg-gray-100 text-gray-800 rounded">
              {{ module.questionCount }} вопросов
            </span>
          </div>
          <div class="flex space-x-2">
            <button @click="editModule(module)" class="flex-1 btn-secondary text-sm">
              Редактировать
            </button>
            <button @click="deleteModule(module.moduleId)" class="flex-1 btn-secondary text-sm bg-red-600 hover:bg-red-700 text-white">
              Удалить
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Users Tab -->
    <div v-if="activeTab === 'users'" class="space-y-4">
      <div class="flex justify-between items-center">
        <h2 class="text-xl font-semibold text-gray-900">Управление пользователями</h2>
        <button @click="showUserForm = true" class="btn-primary">
          Создать пользователя
        </button>
      </div>

      <div class="card overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">ФИО</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Email</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Подразделение</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Статус</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Действия</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="user in users" :key="user.userId">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                {{ user.fullName }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ user.email }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ user.departmentName }}
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
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <button @click="editUser(user)" class="text-primary-600 hover:text-primary-900 mr-3">
                  Редактировать
                </button>
                <button @click="deleteUser(user.userId)" class="text-red-600 hover:text-red-900">
                  Удалить
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Questions Tab -->
    <div v-if="activeTab === 'questions'" class="space-y-4">
      <div class="flex justify-between items-center">
        <h2 class="text-xl font-semibold text-gray-900">Управление вопросами</h2>
        <div class="flex space-x-3">
          <select v-model="selectedModuleForQuestions" class="input w-auto">
            <option value="">Выберите модуль</option>
            <option
              v-for="module in modules"
              :key="module.moduleId"
              :value="module.moduleId"
            >
              {{ module.title }}
            </option>
          </select>
          <button
            v-if="selectedModuleForQuestions"
            @click="showQuestionForm = true"
            class="btn-primary"
          >
            Добавить вопрос
          </button>
        </div>
      </div>

      <div v-if="selectedModuleForQuestions && questions.length > 0" class="space-y-4">
        <div
          v-for="question in questions"
          :key="question.questionId"
          class="card"
        >
          <div class="flex justify-between items-start mb-3">
            <h3 class="text-lg font-semibold text-gray-900">{{ question.questionText }}</h3>
            <button @click="deleteQuestion(question.questionId)" class="text-red-600 hover:text-red-900">
              Удалить
            </button>
          </div>
          <div class="space-y-2">
            <div
              v-for="answer in question.answerOptions"
              :key="answer.answerId"
              class="flex items-center space-x-2 p-2 bg-gray-50 rounded"
            >
              <span
                v-if="answer.isCorrect"
                class="px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded"
              >
                Правильный
              </span>
              <span class="text-sm text-gray-700">{{ answer.answerText }}</span>
            </div>
          </div>
        </div>
      </div>
      <div v-else-if="selectedModuleForQuestions" class="card text-center py-12 text-gray-500">
        Нет вопросов в этом модуле
      </div>
      <div v-else class="card text-center py-12 text-gray-500">
        Выберите модуль для просмотра вопросов
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { modulesApi, usersApi, questionsApi } from '../api/services'

const activeTab = ref('modules')
const modules = ref([])
const users = ref([])
const questions = ref([])
const selectedModuleForQuestions = ref('')
const showModuleForm = ref(false)
const showUserForm = ref(false)
const showQuestionForm = ref(false)

onMounted(async () => {
  await loadModules()
  await loadUsers()
})

watch(selectedModuleForQuestions, async (newVal) => {
  if (newVal) {
    await loadQuestions(newVal)
  } else {
    questions.value = []
  }
})

const loadModules = async () => {
  try {
    const response = await modulesApi.getAll()
    modules.value = response.data
  } catch (error) {
    console.error('Failed to load modules:', error)
  }
}

const loadUsers = async () => {
  try {
    const response = await usersApi.getAll()
    users.value = response.data
  } catch (error) {
    console.error('Failed to load users:', error)
  }
}

const loadQuestions = async (moduleId) => {
  try {
    const response = await questionsApi.getByModule(moduleId)
    questions.value = response.data
  } catch (error) {
    console.error('Failed to load questions:', error)
  }
}

const editModule = (module) => {
  // TODO: Implement module editing
  alert('Редактирование модуля будет реализовано')
}

const deleteModule = async (moduleId) => {
  if (!confirm('Вы уверены, что хотите удалить этот модуль?')) return
  try {
    await modulesApi.delete(moduleId)
    await loadModules()
  } catch (error) {
    console.error('Failed to delete module:', error)
    alert('Ошибка при удалении модуля')
  }
}

const editUser = (user) => {
  // TODO: Implement user editing
  alert('Редактирование пользователя будет реализовано')
}

const deleteUser = async (userId) => {
  if (!confirm('Вы уверены, что хотите удалить этого пользователя?')) return
  try {
    await usersApi.delete(userId)
    await loadUsers()
  } catch (error) {
    console.error('Failed to delete user:', error)
    alert('Ошибка при удалении пользователя')
  }
}

const deleteQuestion = async (questionId) => {
  if (!confirm('Вы уверены, что хотите удалить этот вопрос?')) return
  try {
    await questionsApi.delete(questionId)
    if (selectedModuleForQuestions.value) {
      await loadQuestions(selectedModuleForQuestions.value)
    }
  } catch (error) {
    console.error('Failed to delete question:', error)
    alert('Ошибка при удалении вопроса')
  }
}
</script>








