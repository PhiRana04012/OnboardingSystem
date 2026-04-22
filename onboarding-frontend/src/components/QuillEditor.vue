<template>
  <div class="quill-editor-wrapper">
    <div ref="editorNode" class="quill-editor h-64 mb-12 rounded-lg overflow-hidden border-2 border-gray-200 dark:border-gray-700"></div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:modelValue'])

const editorNode = ref(null)
let quill = null
let isUpdating = false

onMounted(() => {
  if (window.Quill) {
    quill = new window.Quill(editorNode.value, {
      theme: 'snow',
      modules: {
        toolbar: [
          [{ 'header': [1, 2, 3, false] }],
          ['bold', 'italic', 'underline', 'strike'],
          [{ 'list': 'ordered'}, { 'list': 'bullet' }],
          ['link', 'image', 'video'],
          ['clean']
        ]
      }
    })

    if (props.modelValue) {
      quill.root.innerHTML = props.modelValue
    }

    quill.on('text-change', () => {
      isUpdating = true
      emit('update:modelValue', quill.root.innerHTML)
      setTimeout(() => { isUpdating = false }, 0)
    })
  } else {
    console.error('Quill library is not loaded from CDN')
  }
})

onBeforeUnmount(() => {
  if (quill) {
    quill = null
  }
})

watch(() => props.modelValue, (newValue) => {
  if (!isUpdating && quill && newValue !== quill.root.innerHTML) {
    quill.root.innerHTML = newValue
  }
})
</script>

<style scoped>
:deep(.ql-toolbar) {
  @apply bg-white dark:bg-gray-700 border-b-2 border-gray-200 dark:border-gray-600;
}

:deep(.ql-container) {
  @apply bg-white dark:bg-gray-800;
}

:deep(.ql-editor) {
  @apply text-gray-900 dark:text-gray-100;
  min-height: 200px;
}

:deep(.ql-editor.ql-blank::before) {
  @apply text-gray-400 dark:text-gray-500;
}

:deep(.ql-toolbar button:hover),
:deep(.ql-toolbar button.ql-active),
:deep(.ql-toolbar button:focus),
:deep(.ql-toolbar button:active) {
  @apply text-primary-600 dark:text-primary-400;
}

:deep(.ql-toolbar.ql-snow .ql-picker-label) {
  @apply text-gray-700 dark:text-gray-300;
}

:deep(.ql-toolbar.ql-snow .ql-stroke) {
  @apply stroke-gray-600 dark:stroke-gray-400;
}

:deep(.ql-toolbar.ql-snow .ql-fill) {
  @apply fill-gray-600 dark:fill-gray-400;
}

.ql-toolbar.ql-snow {
  border-top-left-radius: 0.375rem;
  border-top-right-radius: 0.375rem;
  border-color: #d1d5db;
  background-color: #f9fafb;
}
.ql-editor {
  min-height: 200px;
}
</style>
