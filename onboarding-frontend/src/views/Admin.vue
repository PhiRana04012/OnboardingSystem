<template>
  <div class="space-y-6">
    <div class="flex justify-between items-center">
      <h1 class="text-4xl font-bold text-gray-900 dark:text-white">Администрирование</h1>
    </div>

    <!-- Tabs -->
    <div class="border-b border-gray-200 dark:border-gray-700">
      <nav class="-mb-px flex space-x-8">
        <button
          @click="activeTab = 'modules'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'modules' 
            ? 'border-primary-500 text-primary-600 dark:text-primary-400' 
            : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
        >
          Модули
        </button>
        <button
          @click="activeTab = 'users'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'users' 
            ? 'border-primary-500 text-primary-600 dark:text-primary-400' 
            : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
        >
          Пользователи
        </button>
        <button
          @click="activeTab = 'questions'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'questions' 
            ? 'border-primary-500 text-primary-600 dark:text-primary-400' 
            : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
        >
          Вопросы
        </button>
        <button
          @click="activeTab = 'faq'"
          class="py-4 px-1 border-b-2 font-medium text-sm transition-colors"
          :class="activeTab === 'faq' 
            ? 'border-primary-500 text-primary-600 dark:text-primary-400' 
            : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'"
        >
          FAQ
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
        <h2 class="text-xl font-semibold text-gray-900 dark:text-white">Управление пользователями</h2>
        <button @click="showUserForm = true" class="btn-primary">
          Создать пользователя
        </button>
      </div>

      <div class="card overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead class="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">ФИО</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Email</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Подразделение</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Статус</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Действия</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
            <tr v-for="user in users" :key="user.userId" class="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-gray-100">
                {{ user.fullName }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                {{ user.email }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                {{ user.departmentName }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  class="px-2 py-1 text-xs font-medium rounded-full"
                  :class="{
                    'bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-300': user.onboardingStatus === 'Завершён',
                    'bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-300': user.onboardingStatus === 'В процессе',
                    'bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-300': user.onboardingStatus === 'Не начат'
                  }"
                >
                  {{ user.onboardingStatus }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <button @click="editUser(user)" class="text-primary-600 dark:text-primary-400 hover:text-primary-900 dark:hover:text-primary-300 mr-3 transition-colors">
                  Редактировать
                </button>
                <button @click="deleteUser(user.userId)" class="text-red-600 dark:text-red-400 hover:text-red-900 dark:hover:text-red-300 transition-colors">
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
            <div class="flex space-x-3">
              <button @click="editQuestion(question)" class="text-primary-600 hover:text-primary-900">
                Редактировать
              </button>
              <button @click="deleteQuestion(question.questionId)" class="text-red-600 hover:text-red-900">
                Удалить
              </button>
            </div>
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

    <!-- FAQ Tab -->
    <div v-if="activeTab === 'faq'" class="space-y-4">
      <div class="flex justify-between items-center">
        <h2 class="text-xl font-semibold text-gray-900">Управление FAQ</h2>
        <button @click="openFaqForm()" class="btn-primary">Добавить вопрос</button>
      </div>

      <div v-if="faqItems.length === 0 && !showFaqForm" class="card text-center py-12 text-gray-500">
        Пока нет вопросов FAQ
      </div>

      <div v-else class="space-y-4">
        <div v-for="item in faqItems" :key="item.id" class="card">
          <div class="flex justify-between items-start mb-3">
            <div>
               <h3 class="font-bold text-gray-900 mb-1">{{ item.question }}</h3>
               <p class="text-sm text-gray-600 mb-2">{{ item.answer }}</p>
               <span class="px-2 py-0.5 text-xs font-bold bg-blue-50 text-blue-600 rounded">{{ item.category || 'Общее' }}</span>
            </div>
            <div class="flex space-x-3 ml-4">
              <button @click="openFaqForm(item)" class="text-primary-600 hover:text-primary-900">Редактировать</button>
              <button @click="deleteFaqItem(item.id)" class="text-red-600 hover:text-red-900">Удалить</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Модальное окно для Модуля -->
    <div v-if="showModuleForm" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black bg-opacity-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] flex flex-col transition-colors">
        <div class="flex justify-between items-center p-6 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-lg font-medium text-gray-900 dark:text-white">
            {{ editingModule ? 'Редактировать модуль' : 'Создать модуль' }}
          </h3>
          <button @click="closeModuleForm" class="text-gray-400 hover:text-gray-500">
            <span class="sr-only">Закрыть</span>
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <form @submit.prevent="saveModule" class="p-6 space-y-4 overflow-y-auto">
          <div>
            <label class="block text-sm font-medium text-gray-700">Название модуля</label>
            <input v-model="moduleForm.title" type="text" required class="mt-1 input block w-full" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Описание</label>
            <textarea v-model="moduleForm.description" rows="2" class="mt-1 input block w-full"></textarea>
          </div>
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">Контент (HTML или текст)</label>
            <QuillEditor v-model="moduleForm.content" />
          </div>
          <div class="flex items-center">
            <input v-model="moduleForm.isMandatory" type="checkbox" class="h-4 w-4 text-primary-600 border-gray-300 rounded" />
            <label class="ml-2 block text-sm text-gray-900">Обязательный модуль</label>
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700">Проходной балл</label>
              <input v-model="moduleForm.passingScore" type="number" min="0" required class="mt-1 input block w-full" />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Максимум попыток (0 = не ограничено)</label>
              <input v-model="moduleForm.maxAttempts" type="number" min="0" required class="mt-1 input block w-full" />
            </div>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Подразделение (необязательно, если для всех)</label>
            <select v-model="moduleForm.departmentId" class="mt-1 input block w-full">
              <option :value="null">-- Все подразделения --</option>
              <option v-for="dept in departments" :key="dept.departmentId" :value="dept.departmentId">
                {{ dept.name }}
              </option>
            </select>
          </div>
          <div class="pt-4 flex justify-end space-x-3 border-t">
            <button type="button" @click="closeModuleForm" class="btn-secondary">Отмена</button>
            <button type="submit" class="btn-primary">Сохранить</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Модальное окно для Пользователя -->
    <div v-if="showUserForm" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black bg-opacity-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md max-h-[90vh] flex flex-col transition-colors">
        <div class="flex justify-between items-center p-6 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-lg font-medium text-gray-900 dark:text-white">
            {{ editingUser ? 'Редактировать пользователя' : 'Создать пользователя' }}
          </h3>
          <button @click="closeUserForm" class="text-gray-400 hover:text-gray-500">
            <span class="sr-only">Закрыть</span>
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <form @submit.prevent="saveUser" class="p-6 space-y-4 overflow-y-auto">
          <div>
            <label class="block text-sm font-medium text-gray-700">ФИО</label>
            <input v-model="userForm.fullName" type="text" required class="mt-1 input block w-full" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Email (Логин)</label>
            <input v-model="userForm.email" type="email" required class="mt-1 input block w-full" />
          </div>
          <div v-if="!editingUser">
            <label class="block text-sm font-medium text-gray-700">Пароль</label>
            <input v-model="userForm.passwordHash" type="password" required class="mt-1 input block w-full" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Роль</label>
            <select v-model="userForm.role" required class="mt-1 input block w-full">
              <option value="user">Пользователь</option>
              <option value="admin">Администратор</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Подразделение</label>
            <select v-model="userForm.departmentId" class="mt-1 input block w-full" required>
              <option disabled :value="null">Выберите подразделение</option>
              <option v-for="dept in departments" :key="dept.departmentId" :value="dept.departmentId">
                {{ dept.name }}
              </option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700">Наставник (необязательно)</label>
            <div class="mt-1 relative">
              <input
                v-model="mentorSearchQuery"
                type="text"
                placeholder="Поиск по ФИО..."
                class="input block w-full mb-2"
              />
              <select v-model="userForm.mentorId" class="input block w-full">
                <option :value="null">— Не назначен —</option>
                <option v-for="mentor in filteredMentors" :key="mentor.userId" :value="mentor.userId">
                  {{ mentor.fullName }}
                </option>
              </select>
            </div>
          </div>
          <div class="pt-4 flex justify-end space-x-3 border-t">
            <button type="button" @click="closeUserForm" class="btn-secondary">Отмена</button>
            <button type="submit" class="btn-primary">Сохранить</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Модальное окно для Вопроса -->
    <div v-if="showQuestionForm" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black bg-opacity-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] flex flex-col transition-colors">
        <div class="flex justify-between items-center p-6 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-lg font-medium text-gray-900 dark:text-white">
            {{ editingQuestion ? 'Редактировать вопрос' : 'Добавить вопрос' }} (модуль: {{ currentModuleName }})
          </h3>
          <button @click="closeQuestionForm" class="text-gray-400 hover:text-gray-500">
            <span class="sr-only">Закрыть</span>
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <form @submit.prevent="saveQuestion" class="p-6 space-y-6 overflow-y-auto">
          <div>
            <label class="block text-sm font-medium text-gray-700">Текст вопроса</label>
            <textarea v-model="questionForm.questionText" rows="3" required class="mt-1 input block w-full"></textarea>
          </div>
          
          <div class="space-y-4">
            <div class="flex justify-between items-center">
              <label class="block text-sm font-medium text-gray-700">Варианты ответов</label>
              <button type="button" @click="addAnswerOption" class="text-sm text-primary-600 hover:text-primary-800 font-medium">
                + Добавить вариант
              </button>
            </div>
            
            <div v-for="(answer, index) in questionForm.answerOptions" :key="index" class="flex items-center space-x-3 bg-gray-50 p-3 rounded">
              <input type="radio" :name="'correctAnswer'" :checked="answer.isCorrect" @change="setCorrectAnswer(index)" class="h-4 w-4 text-primary-600 border-gray-300 focus:ring-primary-500" />
              <input v-model="answer.answerText" type="text" placeholder="Текст ответа" required class="input flex-1" />
              <button type="button" @click="removeAnswerOption(index)" v-if="questionForm.answerOptions.length > 2" class="text-red-500 hover:text-red-700">
                <svg class="h-5 w-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clip-rule="evenodd" />
                </svg>
              </button>
            </div>
            <p v-if="!hasCorrectAnswer" class="text-sm text-red-600 mt-1">Необходимо выбрать правильный ответ!</p>
          </div>

          <div class="pt-4 flex justify-end space-x-3 border-t">
            <button type="button" @click="closeQuestionForm" class="btn-secondary">Отмена</button>
            <button type="submit" class="btn-primary" :disabled="!hasCorrectAnswer || questionForm.answerOptions.length < 2">Сохранить</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Модальное окно для FAQ -->
    <div v-if="showFaqForm" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black bg-opacity-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] flex flex-col transition-colors">
        <div class="flex justify-between items-center p-6 border-b border-gray-200 dark:border-gray-700">
           <h3 class="text-lg font-medium text-gray-900 dark:text-white">{{ editingFaq ? 'Редактировать FAQ' : 'Добавить FAQ' }}</h3>
           <button @click="closeFaqForm" class="text-gray-400 hover:text-gray-500"><svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg></button>
        </div>
        <form @submit.prevent="saveFaq" class="p-6 space-y-4 overflow-y-auto">
          <div><label class="block text-sm font-medium text-gray-700">Вопрос</label><input v-model="faqForm.question" type="text" required class="mt-1 input block w-full" /></div>
          <div><label class="block text-sm font-medium text-gray-700">Ответ</label><textarea v-model="faqForm.answer" rows="3" required class="mt-1 input block w-full"></textarea></div>
          <div class="grid grid-cols-2 gap-4">
             <div><label class="block text-sm font-medium text-gray-700">Категория</label><input v-model="faqForm.category" type="text" class="mt-1 input block w-full" /></div>
             <div><label class="block text-sm font-medium text-gray-700">Порядок</label><input v-model="faqForm.displayOrder" type="number" class="mt-1 input block w-full" /></div>
          </div>
          <div class="pt-4 flex justify-end space-x-3 border-t"><button type="button" @click="closeFaqForm" class="btn-secondary">Отмена</button><button type="submit" class="btn-primary" :disabled="isSavingFaq">Сохранить</button></div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed } from 'vue'
import { modulesApi, usersApi, questionsApi, departmentsApi, faqApi } from '../api/services'
import QuillEditor from '../components/QuillEditor.vue'

const activeTab = ref('modules')
const modules = ref([])
const users = ref([])
const questions = ref([])
const departments = ref([])

const selectedModuleForQuestions = ref('')

// Module Form State
const showModuleForm = ref(false)
const editingModule = ref(null)
const moduleForm = ref({
  title: '',
  description: '',
  content: '',
  isMandatory: false,
  passingScore: 70,
  maxAttempts: 0,
  departmentId: null
})

// User Form State
const showUserForm = ref(false)
const editingUser = ref(null)
const mentors = ref([])
const mentorSearchQuery = ref('')
const userForm = ref({
  fullName: '',
  email: '',
  passwordHash: '',
  role: 'user',
  departmentId: null,
  mentorId: null
})

// Computed property для фильтрации наставников по отделу и поиску
const filteredMentors = computed(() => {
  return mentors.value.filter(mentor => {
    // Фильтруем только наставников из того же отдела
    const sameDepart = mentor.departmentId === userForm.value.departmentId
    
    // Фильтруем по поисковому запросу (ФИО)
    const matchesSearch = mentor.fullName
      .toLowerCase()
      .includes(mentorSearchQuery.value.toLowerCase())
    
    return sameDepart && matchesSearch
  })
})

// Question Form State
const showQuestionForm = ref(false)
const editingQuestion = ref(null)
const questionForm = ref({
  questionText: '',
  answerOptions: [
    { answerText: '', isCorrect: true },
    { answerText: '', isCorrect: false }
  ]
})

const currentModuleName = computed(() => {
  const mod = modules.value.find(m => m.moduleId === selectedModuleForQuestions.value)
  return mod ? mod.title : ''
})
const hasCorrectAnswer = computed(() => {
  return questionForm.value.answerOptions.some(a => a.isCorrect)
})

// FAQ Form State
const faqItems = ref([])
const showFaqForm = ref(false)
const editingFaq = ref(null)
const isSavingFaq = ref(false)
const faqForm = ref({ question: '', answer: '', category: 'Общее', displayOrder: 0 })

onMounted(async () => {
  await Promise.all([
    loadModules(),
    loadUsers(),
    loadDepartments(),
    loadFaq()
  ])
  // Загружаем список наставников (отфильтровываем пользователей с ролью наставника)
  await loadPotentialMentors()
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

const loadDepartments = async () => {
  try {
    const response = await departmentsApi.getAll()
    departments.value = response.data
  } catch (error) {
    console.error('Failed to load departments:', error)
  }
}

const loadPotentialMentors = async () => {
  try {
    const response = await usersApi.getAll()
    // Загружаем всех пользователей как потенциальных наставников
    // Фильтрация по отделу происходит в компоненте выбора
    mentors.value = response.data
  } catch (error) {
    console.error('Failed to load mentors:', error)
  }
}

const closeModuleForm = () => {
  showModuleForm.value = false
  editingModule.value = null
  moduleForm.value = {
    title: '',
    description: '',
    content: '',
    isMandatory: false,
    passingScore: 70,
    maxAttempts: 0,
    departmentId: null
  }
}

const saveModule = async () => {
  try {
    const payload = { ...moduleForm.value }
    if (editingModule.value) {
      await modulesApi.update(editingModule.value.moduleId, payload)
    } else {
      await modulesApi.create(payload)
    }
    await loadModules()
    closeModuleForm()
  } catch (error) {
    console.error('Failed to save module:', error)
    alert('Ошибка при сохранении модуля')
  }
}

const editModule = (module) => {
  editingModule.value = module
  moduleForm.value = {
    title: module.title || '',
    description: module.description || '',
    content: module.content || '',
    isMandatory: !!module.isMandatory,
    passingScore: module.passingScore !== undefined ? module.passingScore : 70,
    maxAttempts: module.maxAttempts || 0,
    departmentId: module.departmentId || null
  }
  showModuleForm.value = true
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

const closeUserForm = () => {
  showUserForm.value = false
  editingUser.value = null
  mentorSearchQuery.value = ''
  userForm.value = {
    fullName: '',
    email: '',
    passwordHash: '',
    role: 'user',
    departmentId: null,
    mentorId: null
  }
}

const saveUser = async () => {
  try {
    // Получаем ID роли на основе выбранной роли
    let roleIds = []
    if (userForm.value.role === 'admin') {
      // ID роли администратора может быть 1, но нужно проверить реальные ID
      roleIds = [1] // Нужно получить реальный ID роли администратора
    }
    // для обычного пользователя roleIds остается пустым или получаем ID роли пользователя
    
    const payload = {
      fullName: userForm.value.fullName,
      email: userForm.value.email,
      departmentId: userForm.value.departmentId,
      mentorId: userForm.value.mentorId || null,
      roleIds: roleIds,
      hireDate: new Date().toISOString().split('T')[0] // Текущая дата в формате YYYY-MM-DD
    }
    
    if (!editingUser.value && userForm.value.passwordHash) {
      payload.passwordHash = userForm.value.passwordHash
    }
    
    if (editingUser.value) {
      await usersApi.update(editingUser.value.userId, payload)
    } else {
      await usersApi.create(payload)
    }
    await loadUsers()
    closeUserForm()
  } catch (error) {
    console.error('Failed to save user:', error)
    alert('Ошибка при сохранении пользователя: ' + (error.response?.data?.message || error.message))
  }
}

const editUser = (user) => {
  editingUser.value = user
  mentorSearchQuery.value = ''
  userForm.value = {
    fullName: user.fullName || '',
    email: user.email || '',
    passwordHash: '', // Keep blank on edit
    role: user.role || 'user',
    departmentId: user.departmentId || null,
    mentorId: user.mentorId || null
  }
  showUserForm.value = true
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

const addAnswerOption = () => {
  questionForm.value.answerOptions.push({ answerText: '', isCorrect: false })
}

const removeAnswerOption = (index) => {
  if (questionForm.value.answerOptions.length > 2) {
    const wasCorrect = questionForm.value.answerOptions[index].isCorrect
    questionForm.value.answerOptions.splice(index, 1)
    if (wasCorrect && questionForm.value.answerOptions.length > 0) {
      questionForm.value.answerOptions[0].isCorrect = true
    }
  }
}

const setCorrectAnswer = (index) => {
  questionForm.value.answerOptions.forEach((ans, i) => {
    ans.isCorrect = (i === index)
  })
}

const closeQuestionForm = () => {
  showQuestionForm.value = false
  editingQuestion.value = null
  questionForm.value = {
    questionText: '',
    answerOptions: [
      { answerText: '', isCorrect: true },
      { answerText: '', isCorrect: false }
    ]
  }
}

const editQuestion = (question) => {
  editingQuestion.value = question
  questionForm.value = {
    questionText: question.questionText || '',
    // Clone answer options dynamically to break reactivity from the original array
    answerOptions: question.answerOptions && question.answerOptions.length > 0 
      ? JSON.parse(JSON.stringify(question.answerOptions))
      : [
          { answerText: '', isCorrect: true },
          { answerText: '', isCorrect: false }
        ]
  }
  showQuestionForm.value = true
}

const saveQuestion = async () => {
  if (!hasCorrectAnswer.value) {
    alert('Выберите правильный вариант ответа!')
    return
  }
  
  try {
    const payload = {
      moduleId: selectedModuleForQuestions.value,
      questionText: questionForm.value.questionText,
      points: editingQuestion.value?.points || 1, // Defaulting to 1 point
      answerOptions: questionForm.value.answerOptions
    }
    
    if (editingQuestion.value) {
      await questionsApi.update(editingQuestion.value.questionId, payload)
    } else {
      await questionsApi.create(payload)
    }
    
    await loadQuestions(selectedModuleForQuestions.value)
    closeQuestionForm()
  } catch (error) {
    console.error('Failed to save question:', error)
    alert('Ошибка при сохранении вопроса')
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

// FAQ Logic
const loadFaq = async () => {
  try {
    const res = await faqApi.getAll()
    faqItems.value = res.data
  } catch (err) {
    console.error('Failed to load FAQ:', err)
  }
}

const openFaqForm = (item = null) => {
  if (item) {
    editingFaq.value = item
    faqForm.value = { question: item.question, answer: item.answer, category: item.category || 'Общее', displayOrder: item.displayOrder || 0 }
  } else {
    editingFaq.value = null
    faqForm.value = { question: '', answer: '', category: 'Общее', displayOrder: 0 }
  }
  showFaqForm.value = true
}

const closeFaqForm = () => { showFaqForm.value = false; editingFaq.value = null; }

const saveFaq = async () => {
  try {
    isSavingFaq.value = true
    if (editingFaq.value) await faqApi.update(editingFaq.value.id, faqForm.value)
    else await faqApi.create(faqForm.value)
    closeFaqForm()
    await loadFaq()
  } catch (err) { alert('Ошибка при сохранении FAQ') }
  finally { isSavingFaq.value = false }
}

const deleteFaqItem = async (id) => {
  if (!confirm('Удалить этот вопрос?')) return
  try { await faqApi.delete(id); await loadFaq() } 
  catch (err) { alert('Ошибка при удалении') }
}
</script>








