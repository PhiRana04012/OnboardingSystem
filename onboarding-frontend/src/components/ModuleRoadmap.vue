<template>
  <div class="mb-8">
    <div class="flex items-center gap-2 md:gap-4">
      <!-- Step 1: Reading -->
      <div class="flex items-center flex-1">
        <div 
          class="w-10 h-10 md:w-12 md:h-12 rounded-full flex items-center justify-center font-bold transition-all flex-shrink-0 relative"
          :class="steps[0].completed 
            ? 'bg-green-500 text-white shadow-lg shadow-green-500/30' 
            : 'bg-gray-300 text-gray-600 dark:bg-gray-600 dark:text-gray-300'"
        >
          <span v-if="steps[0].completed" class="text-lg">✓</span>
          <span v-else class="text-sm">1</span>
        </div>
        <div class="hidden md:block ml-2">
          <p class="text-xs font-semibold text-gray-600 dark:text-gray-400">Этап 1</p>
          <p class="text-sm font-bold text-gray-900 dark:text-gray-100">{{ steps[0].title }}</p>
        </div>
      </div>

      <!-- Line 1 -->
      <div class="flex-1 h-1 md:h-1.5 transition-all"
        :class="steps[0].completed 
          ? 'bg-gradient-to-r from-green-500 to-green-400 shadow-sm shadow-green-500/50' 
          : 'bg-gray-300 dark:bg-gray-600'"
      ></div>

      <!-- Step 2: Practice -->
      <div class="flex items-center flex-1">
        <div 
          class="w-10 h-10 md:w-12 md:h-12 rounded-full flex items-center justify-center font-bold transition-all flex-shrink-0"
          :class="steps[1].completed 
            ? 'bg-green-500 text-white shadow-lg shadow-green-500/30' 
            : 'bg-gray-300 text-gray-600 dark:bg-gray-600 dark:text-gray-300'"
        >
          <span v-if="steps[1].completed" class="text-lg">✓</span>
          <span v-else class="text-sm">2</span>
        </div>
        <div class="hidden md:block ml-2">
          <p class="text-xs font-semibold text-gray-600 dark:text-gray-400">Этап 2</p>
          <p class="text-sm font-bold text-gray-900 dark:text-gray-100">{{ steps[1].title }}</p>
        </div>
      </div>

      <!-- Line 2 -->
      <div v-if="hasTest" class="flex-1 h-1 md:h-1.5 transition-all"
        :class="steps[1].completed 
          ? 'bg-gradient-to-r from-green-500 to-green-400 shadow-sm shadow-green-500/50' 
          : 'bg-gray-300 dark:bg-gray-600'"
      ></div>

      <!-- Step 3: Test (conditional) -->
      <div v-if="hasTest" class="flex items-center flex-1">
        <div 
          class="w-10 h-10 md:w-12 md:h-12 rounded-full flex items-center justify-center font-bold transition-all flex-shrink-0"
          :class="steps[2].completed 
            ? 'bg-green-500 text-white shadow-lg shadow-green-500/30' 
            : 'bg-gray-300 text-gray-600 dark:bg-gray-600 dark:text-gray-300'"
        >
          <span v-if="steps[2].completed" class="text-lg">✓</span>
          <span v-else class="text-sm">3</span>
        </div>
        <div class="hidden md:block ml-2">
          <p class="text-xs font-semibold text-gray-600 dark:text-gray-400">Этап 3</p>
          <p class="text-sm font-bold text-gray-900 dark:text-gray-100">{{ steps[2].title }}</p>
        </div>
      </div>
    </div>

    <!-- Progress Summary -->
    <div class="mt-4 p-3 bg-gradient-to-r from-blue-50 to-cyan-50 dark:from-blue-900/20 dark:to-cyan-900/20 rounded-lg border border-blue-200 dark:border-blue-800">
      <p class="text-xs md:text-sm text-gray-700 dark:text-gray-300">
        <span class="font-bold">Прогресс:</span>
        <span v-if="!hasTest">{{ completedSteps }} из 2 этапов завершено</span>
        <span v-else>{{ completedSteps }} из 3 этапов завершено</span>
        <span class="text-gray-500 dark:text-gray-400"> — {{ currentStepTitle }}</span>
      </p>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  hasTestBefore: {
    type: Boolean,
    default: false
  },
  hasTest: {
    type: Boolean,
    default: false
  },
  isReadyForTest: {
    type: Boolean,
    default: false
  },
  allRequiredChecked: {
    type: Boolean,
    default: false
  },
  moduleStatus: {
    type: String,
    default: 'Не начат'
  }
})

const steps = computed(() => {
  const isModuleCompleted = props.moduleStatus === 'Завершён'
  const hasPassedTestBefore = props.hasTestBefore

  const baseSteps = [
    {
      id: 1,
      title: 'Ознакомление',
      type: 'reading',
      completed: props.hasTest
        ? props.isReadyForTest || hasPassedTestBefore || isModuleCompleted
        : isModuleCompleted
    },
    {
      id: 2,
      title: 'Практические задания',
      type: 'practice',
      completed: props.allRequiredChecked || hasPassedTestBefore || isModuleCompleted
    }
  ]

  if (props.hasTest) {
    baseSteps.push({
      id: 3,
      title: 'Тестирование',
      type: 'test',
      completed: hasPassedTestBefore || isModuleCompleted
    })
  }

  return baseSteps
})

const completedSteps = computed(() => {
  return steps.value.filter(s => s.completed).length
})

const currentStepTitle = computed(() => {
  const currentStep = steps.value.find(s => !s.completed)
  if (!currentStep) {
    return '✨ Все этапы завершены!'
  }
  return `Текущий этап: "${currentStep.title}"`
})
</script>

<style scoped>
/* Smooth transitions for progress indicators */
.rounded-full {
  transition: all 0.3s ease;
}
</style>
