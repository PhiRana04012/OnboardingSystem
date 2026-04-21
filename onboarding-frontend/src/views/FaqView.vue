<template>
  <div class="max-w-4xl mx-auto space-y-6">
    <div class="text-center space-y-2">
      <h1 class="text-3xl font-bold text-gray-900">База знаний и FAQ</h1>
      <p class="text-gray-600">Ответы на часто задаваемые вопросы для новых сотрудников</p>
    </div>

    <!-- Search -->
    <div class="relative">
      <input
        v-model="searchQuery"
        type="text"
        placeholder="Поиск по вопросам..."
        class="input pl-10 h-12 shadow-sm"
      />
      <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
        <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
      </div>
    </div>

    <div v-if="faqStore.isLoading" class="flex justify-center py-12">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-4 gap-6">
      <!-- Categories Sidebar -->
      <aside class="md:col-span-1 space-y-2">
        <h3 class="text-xs font-bold text-gray-400 uppercase tracking-widest mb-2 ml-2">Категории</h3>
        <button
          @click="selectedCategory = null"
          :class="[!selectedCategory ? 'bg-primary-50 text-primary-700 font-bold border-l-4 border-primary-600' : 'text-gray-600 hover:bg-gray-50']"
          class="w-full text-left px-3 py-2.5 rounded-lg text-sm transition-all"
        >
          Все категории
        </button>
        <button
          v-for="cat in faqStore.categories"
          :key="cat"
          @click="selectedCategory = cat"
          :class="[selectedCategory === cat ? 'bg-primary-50 text-primary-700 font-bold border-l-4 border-primary-600' : 'text-gray-600 hover:bg-gray-50']"
          class="w-full text-left px-3 py-2.5 rounded-lg text-sm transition-all"
        >
          {{ cat }}
        </button>
      </aside>

      <!-- Questions List -->
      <div class="md:col-span-3 space-y-4">
        <div 
          v-for="item in filteredFaq" 
          :key="item.id"
          class="card !p-0 overflow-hidden hover:shadow-lg transition-shadow"
        >
          <button 
            @click="toggleItem(item.id)"
            class="w-full px-6 py-4 flex items-center justify-between text-left focus:outline-none group"
          >
            <span class="font-semibold text-gray-900 group-hover:text-primary-600 transition-colors">{{ item.question }}</span>
            <svg 
              class="w-5 h-5 text-gray-400 transition-transform duration-200"
              :class="{ 'rotate-180 text-primary-600': openItems.includes(item.id) }"
              fill="none" stroke="currentColor" viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
            </svg>
          </button>
          
          <div 
            v-show="openItems.includes(item.id)"
            class="px-6 pb-5 text-gray-600 text-sm border-t border-gray-50 pt-4"
          >
            <div class="prose max-w-none" v-html="formatAnswer(item.answer)"></div>
          </div>
        </div>

        <div v-if="filteredFaq.length === 0" class="text-center py-12 text-gray-400 card border-dashed border-2">
           Ничего не найдено по вашему запросу
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useFaqStore } from '../stores/faq'

const faqStore = useFaqStore()
const searchQuery = ref('')
const selectedCategory = ref(null)
const openItems = ref([])

onMounted(() => {
  faqStore.fetchFaq()
})

const toggleItem = (id) => {
  if (openItems.value.includes(id)) {
    openItems.value = openItems.value.filter(i => i !== id)
  } else {
    openItems.value.push(id)
  }
}

const filteredFaq = computed(() => {
  let result = faqStore.items
  if (selectedCategory.value) {
    result = result.filter(i => i.category === selectedCategory.value)
  }
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(i => 
      i.question.toLowerCase().includes(q) || 
      i.answer.toLowerCase().includes(q)
    )
  }
  return result
})

const formatAnswer = (text) => {
  return text.replace(/\n/g, '<br>')
}
</script>
