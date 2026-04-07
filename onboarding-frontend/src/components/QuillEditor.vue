<template>
  <div ref="editorNode" class="quill-editor h-64 mb-12"></div>
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
let isUpdating = false // Flag to prevent infinite update loop

onMounted(() => {
  // Initialize Quill instance
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

    // Set initial content
    if (props.modelValue) {
      quill.root.innerHTML = props.modelValue
    }

    // Listen for text changes to update v-model
    quill.on('text-change', () => {
      isUpdating = true
      emit('update:modelValue', quill.root.innerHTML)
      // Reset flag after Vue tick
      setTimeout(() => { isUpdating = false }, 0)
    })
  } else {
    console.error('Quill library is not loaded from CDN')
  }
})

// Watch for external changes to v-model and update Quill
watch(() => props.modelValue, (newValue) => {
  if (quill && !isUpdating) {
    const currentHtml = quill.root.innerHTML
    if (newValue !== currentHtml) {
      quill.root.innerHTML = newValue || ''
    }
  }
})

onBeforeUnmount(() => {
  quill = null
})
</script>

<style>
/* Adjust Quill default height and styles to match our design */
.ql-container {
  font-family: inherit;
  font-size: 1rem;
  border-bottom-left-radius: 0.375rem;
  border-bottom-right-radius: 0.375rem;
  border-color: #d1d5db;
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
