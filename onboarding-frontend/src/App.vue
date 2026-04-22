<template>
  <div id="app" class="min-h-screen bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-50 transition-colors duration-300">
    <router-view />
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useThemeStore } from './stores/theme'

const themeStore = useThemeStore()

onMounted(() => {
  themeStore.applyTheme()
})
</script>








