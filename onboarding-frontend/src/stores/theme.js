import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

export const useThemeStore = defineStore('theme', () => {
  const isDark = ref(localStorage.getItem('theme') === 'dark')

  // Применяем тему при загрузке
  const applyTheme = () => {
    const root = document.documentElement
    if (isDark.value) {
      root.classList.add('dark')
      localStorage.setItem('theme', 'dark')
    } else {
      root.classList.remove('dark')
      localStorage.setItem('theme', 'light')
    }
  }

  // Переключение темы
  const toggleTheme = () => {
    isDark.value = !isDark.value
    applyTheme()
  }

  // Установка конкретной темы
  const setTheme = (theme) => {
    isDark.value = theme === 'dark'
    applyTheme()
  }

  // Слушаем изменения и применяем тему
  watch(isDark, applyTheme)

  // Инициализируем тему при загрузке
  applyTheme()

  return {
    isDark,
    toggleTheme,
    setTheme,
    applyTheme
  }
})
