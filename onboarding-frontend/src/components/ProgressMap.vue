<template>
  <div class="space-y-10 pb-20">
    <!-- Progress Summary -->
    <div class="card bg-white shadow-sm border border-gray-100 p-8">
      <div class="flex flex-col md:flex-row items-center justify-between gap-6">
        <div class="text-center md:text-left w-full md:w-1/3">
          <h2 class="text-xl font-bold text-gray-900 mb-1">Прогресс онбординга</h2>
          <p class="text-sm text-gray-500">{{ completedCount }} из {{ modules.length }} модулей завершено</p>
        </div>
        
        <div class="w-full md:w-1/3 text-center hidden md:block">
          <div class="w-full bg-gray-100 rounded-full h-3 overflow-hidden">
            <div class="h-full bg-[#0f766e] rounded-full transition-all" :style="{ width: progressPercentage + '%' }"></div>
          </div>
          <div class="flex justify-between items-center mt-2 text-xs">
             <span class="text-gray-400">Обязательных: {{ completedMandatory }} / {{ totalMandatory }}</span>
             <span v-if="allDone" class="font-bold text-[#0f766e]">🎉 Онбординг завершён!</span>
          </div>
        </div>

        <div class="w-full md:w-1/3 text-center md:text-right">
          <div class="text-3xl font-extrabold text-[#0f766e] opacity-90">{{ Math.round(progressPercentage) }}%</div>
        </div>
      </div>
    </div>

    <!-- START BUTTON -->
    <div class="text-center">
      <button class="bg-[#004746] hover:bg-[#003837] text-white font-bold py-3 px-8 rounded-full shadow-lg transition-transform hover:scale-105 inline-flex items-center gap-2 relative z-10">
        🚀 Начало онбординга
      </button>
    </div>

    <!-- Desktop View (Zigzag) -->
    <div class="hidden md:block relative" :style="{ height: mapHeight + 'px' }">
      <svg class="absolute inset-0 w-full h-full pointer-events-none" :viewBox="`0 0 ${VW} ${mapHeight}`" preserveAspectRatio="none">
        <path :d="svgPath" fill="none" stroke="#0f766e" stroke-width="8" stroke-linecap="round" />
      </svg>

      <template v-for="(module, index) in modules" :key="module.moduleId">
        <!-- NODE ICON -->
        <div
          class="absolute z-20 transform -translate-x-1/2 -translate-y-1/2"
          :style="{ left: nodeLeftPct(index), top: nodeTopPx(index) + 'px' }"
        >
          <div v-if="module.status === 'Завершён'" class="w-16 h-16 rounded-full bg-white border-4 border-green-200 flex items-center justify-center cursor-pointer shadow-md transition-transform hover:scale-110" @click="$emit('open-module', module)">
            <div class="w-10 h-10 bg-green-500 rounded-lg flex items-center justify-center text-white">
               <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7" /></svg>
            </div>
          </div>
          <div v-else class="w-16 h-16 rounded-full bg-white border-4 border-gray-200 flex items-center justify-center cursor-pointer shadow-md transition-transform hover:scale-110" @click="$emit('open-module', module)">
            <div class="w-10 h-10 bg-gray-300 rounded-lg flex items-center justify-center text-white">
               <span class="text-lg">🔒</span>
            </div>
          </div>
        </div>

        <!-- CARD -->
        <div
          class="absolute z-10 transform"
          :style="{ 
            left: index % 2 === 0 ? `calc(${nodeLeftPct(index)} + 50px)` : `calc(${nodeLeftPct(index)} - 330px)`, 
            top: (nodeTopPx(index) - 70) + 'px' 
          }"
        >
          <div
            class="card !p-5 w-[280px] cursor-pointer hover:shadow-xl transition-all border-0 shadow-lg relative rounded-2xl"
            @click="$emit('open-module', module)"
          >
            <div class="flex justify-between items-start mb-3">
              <h3 class="font-bold text-gray-900 text-base leading-tight w-2/3">{{ module.moduleTitle || module.title }}</h3>
              <span v-if="module.status === 'Завершён'" class="px-2 py-1 text-[10px] font-bold bg-green-100 text-green-600 rounded-lg whitespace-nowrap">Завершён</span>
            </div>
            <div class="mb-4">
              <span v-if="module.isMandatory" class="px-2 py-1 text-[10px] font-bold bg-red-50 text-red-500 rounded whitespace-nowrap">Обязательный</span>
              <span v-else class="px-2 py-1 text-[10px] font-bold bg-gray-50 text-gray-500 rounded whitespace-nowrap">Дополнительный</span>
            </div>
            <div v-if="module.status === 'Завершён'" class="space-y-1 mb-4 text-xs">
              <p class="text-gray-500">Результат: <span class="font-bold text-green-600">{{ module.bestScore !== null ? Math.round(module.bestScore) : 100 }}%</span></p>
              <p class="text-gray-500">Попыток: {{ module.attemptsCount || 1 }}</p>
            </div>
            <div v-else class="h-10 mb-4"></div>
            
            <button class="w-full py-2 bg-gray-50 hover:bg-gray-100 text-gray-700 font-medium rounded-lg text-sm transition-colors">
              Просмотреть
            </button>
          </div>
        </div>
      </template>

      <!-- END NODE -->
      <div 
        class="absolute z-30 transform -translate-x-1/2" 
        :style="{ left: '50%', top: (endNodeY + 20) + 'px' }"
      >
        <div 
          v-if="allDone"
          class="bg-[#004746] text-white px-8 py-4 rounded-2xl shadow-lg shadow-primary-200/50 font-medium text-lg flex items-center gap-3 transition-transform hover:scale-105"
        >
          🏆 Поздравляем! Онбординг завершен!
        </div>
        <div v-else class="bg-white border border-gray-200 text-gray-400 px-8 py-4 rounded-2xl font-medium tracking-wide shadow-sm">
           Конец маршрута
        </div>
      </div>
    </div>

    <!-- Mobile View -->
    <div class="md:hidden space-y-4 pt-10">
      <div
        v-for="module in modules"
        :key="'mob-' + module.moduleId"
        class="card !p-4 flex items-center gap-4 border-l-4 active:scale-95 transition-transform"
        :class="module.status === 'Завершён' ? 'border-l-green-500' : 'border-l-gray-300'"
        @click="$emit('open-module', module)"
      >
        <div v-if="module.status === 'Завершён'" class="w-12 h-12 rounded-lg bg-green-500 text-white flex items-center justify-center shadow-inner">
           <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7" /></svg>
        </div>
        <div v-else class="w-12 h-12 rounded-lg bg-gray-200 text-gray-500 flex items-center justify-center shadow-inner">
           <span class="text-lg">🔒</span>
        </div>
        <div class="flex-1">
          <h3 class="font-bold text-gray-900 text-sm">{{ module.moduleTitle || module.title }}</h3>
          <p class="text-[10px] font-bold text-gray-400 uppercase mt-1">{{ module.status }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const VW           = 1000   
const NODE_LEFT_X  = 200    
const NODE_RIGHT_X = 800    
const START_Y      = 60     
const NODE_Y_STEP  = 300    

const props = defineProps({
  modules:            { type: Array,  default: () => [] },
  progressPercentage: { type: Number, default: 0 },
  completedMandatory: { type: Number, default: 0 },
  totalMandatory:     { type: Number, default: 0 },
})

defineEmits(['open-module'])

const nodeVX      = (i) => i % 2 === 0 ? NODE_LEFT_X : NODE_RIGHT_X
const nodeVY      = (i) => START_Y + (i + 1) * NODE_Y_STEP
const nodeLeftPct = (i) => `${(nodeVX(i) / VW) * 100}%`
const nodeTopPx   = (i) => nodeVY(i)

const endNodeY  = computed(() => START_Y + (props.modules.length + 1) * NODE_Y_STEP)
const mapHeight = computed(() => endNodeY.value)

const svgPath = computed(() => {
  const pts = [[VW / 2, START_Y - 30], ...props.modules.map((_, i) => [nodeVX(i), nodeVY(i)])]
  if (pts.length < 2) return ''
  let d = `M ${pts[0][0]} ${pts[0][1]}`
  for (let i = 1; i < pts.length; i++) {
    const [x1, y1] = pts[i - 1], [x2, y2] = pts[i]
    const midY = (y1 + y2) / 2
    d += ` C ${x1} ${midY}, ${x2} ${midY}, ${x2} ${y2}`
  }
  return d
})

const completedCount = computed(() => props.modules.filter(m => m.status === 'Завершён').length)
const allDone        = computed(() => props.modules.length > 0 && props.modules.every(m => m.status === 'Завершён'))
</script>
