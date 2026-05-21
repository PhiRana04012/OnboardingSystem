<template>
  <!-- Солнце: включить светлую тему -->
  <svg
    v-if="isDark"
    :class="sizeClass"
    fill="none"
    stroke="currentColor"
    viewBox="0 0 24 24"
    stroke-width="2"
    aria-hidden="true"
  >
    <circle cx="12" cy="12" r="4" stroke-width="2" />
    <path stroke-linecap="round" d="M12 2v2.5M12 19.5V22M4.22 4.22l1.77 1.77M18.01 18.01l1.77 1.77M2 12h2.5M19.5 12H22M4.22 19.78l1.77-1.77M18.01 5.99l1.77-1.77" />
  </svg>
  <!-- Луна: включить тёмную тему -->
  <svg
    v-else
    :class="sizeClass"
    fill="none"
    stroke="currentColor"
    viewBox="0 0 24 24"
    stroke-width="2"
    aria-hidden="true"
  >
    <path
      stroke-linecap="round"
      stroke-linejoin="round"
      d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z"
    />
  </svg>
</template>

<script setup>
defineProps({
  isDark: {
    type: Boolean,
    required: true
  },
  sizeClass: {
    type: String,
    default: 'w-6 h-6'
  }
})
</script>
