<template>
  <div class="bg-white rounded-lg shadow-md p-6">
    <h2 class="text-xl font-semibold text-gray-900 mb-6">
      {{ isEditing ? 'Edit Task' : 'Create New Task' }}
    </h2>

    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- Title Field -->
      <div>
        <label for="title" class="block text-sm font-medium text-gray-700 mb-1">
          Title <span class="text-red-500">*</span>
        </label>
        <input
          id="title"
          v-model="form.title"
          type="text"
          required
          maxlength="200"
          class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
          :class="{ 'border-red-500': errors.title }"
          placeholder="Enter task title..."
        />
        <div v-if="errors.title" class="mt-1 text-sm text-red-600">
          {{ errors.title }}
        </div>
        <div class="mt-1 text-xs text-gray-500">
          {{ form.title.length }}/200 characters
        </div>
      </div>

      <!-- Description Field -->
      <div>
        <label for="description" class="block text-sm font-medium text-gray-700 mb-1">
          Description
        </label>
        <textarea
          id="description"
          v-model="form.description"
          rows="3"
          maxlength="1000"
          class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200 resize-none"
          :class="{ 'border-red-500': errors.description }"
          placeholder="Enter task description (optional)..."
        ></textarea>
        <div v-if="errors.description" class="mt-1 text-sm text-red-600">
          {{ errors.description }}
        </div>
        <div class="mt-1 text-xs text-gray-500">
          {{ (form.description || '').length }}/1000 characters
        </div>
      </div>

      <!-- Priority and Category Row -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Priority Field -->
        <div>
          <label for="priority" class="block text-sm font-medium text-gray-700 mb-1">
            Priority <span class="text-red-500">*</span>
          </label>
          <select
            id="priority"
            v-model.number="form.priority"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
            :class="{ 'border-red-500': errors.priority }"
          >
            <option :value="1">🔽 Low Priority</option>
            <option :value="2">➖ Medium Priority</option>
            <option :value="3">🔺 High Priority</option>
          </select>
          <div v-if="errors.priority" class="mt-1 text-sm text-red-600">
            {{ errors.priority }}
          </div>
        </div>

        <!-- Category Field (Future Enhancement) -->
        <div>
          <label for="category" class="block text-sm font-medium text-gray-700 mb-1">
            Category
          </label>
          <select
            id="category"
            v-model="form.categoryId"
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
            :class="{ 'border-red-500': errors.categoryId }"
          >
            <option :value="undefined">No Category</option>
            <option :value="1">📋 General</option>
            <option :value="2">💼 Work</option>
            <option :value="3">👤 Personal</option>
            <option :value="4">🚨 Urgent</option>
          </select>
          <div v-if="errors.categoryId" class="mt-1 text-sm text-red-600">
            {{ errors.categoryId }}
          </div>
        </div>
      </div>

      <!-- Form Actions -->
      <div class="flex justify-end gap-3 pt-6 border-t border-gray-200">
        <button
          type="button"
          @click="$emit('cancel')"
          class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 border border-gray-300 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors duration-200"
          :disabled="isSubmitting"
        >
          Cancel
        </button>
        
        <button
          type="submit"
          class="px-4 py-2 text-sm font-medium text-white bg-primary-600 border border-transparent rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
          :disabled="isSubmitting || !isFormValid"

        >
          <i v-if="isSubmitting" class="fas fa-spinner fa-spin mr-2"></i>
          {{ isSubmitting ? 'Saving...' : (isEditing ? 'Update Task' : 'Create Task') }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import type { Task, CreateTaskDto, UpdateTaskDto } from '@/types'

// Props
interface Props {
  task?: Task
  isSubmitting?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isSubmitting: false
})

// Emits
interface Emits {
  submit: [data: CreateTaskDto | UpdateTaskDto]
  cancel: []
}

const emit = defineEmits<Emits>()

// Computed
const isEditing = computed(() => !!props.task)

// Form state
const form = reactive({
  title: '',
  description: '',
  priority: 2 as number, // Default to Medium
  categoryId: undefined as number | undefined
})

// Validation errors
const errors = ref<Record<string, string>>({})

// Form validation
const isFormValid = computed(() => {
  const titleValid = form.title.trim().length > 0 && form.title.length <= 200
  const descriptionValid = (form.description?.length || 0) <= 1000
  const priorityValid = [1, 2, 3].includes(form.priority)
  
  return titleValid && descriptionValid && priorityValid
})

// Watch for task prop changes (when editing)
watch(
  () => props.task,
  (newTask) => {
    if (newTask) {
      form.title = newTask.title
      form.description = newTask.description || ''
      form.priority = newTask.priority
      form.categoryId = newTask.categoryId
    } else {
      resetForm()
    }
  },
  { immediate: true }
)

// Methods
function validateForm(): boolean {
  errors.value = {}

  // Title validation
  if (!form.title.trim()) {
    errors.value.title = 'Title is required'
  } else if (form.title.length > 200) {
    errors.value.title = 'Title must be 200 characters or less'
  }

  // Description validation
  if (form.description && form.description.length > 1000) {
    errors.value.description = 'Description must be 1000 characters or less'
  }

  // Priority validation
  if (![1, 2, 3].includes(form.priority)) {
    errors.value.priority = 'Please select a valid priority'
  }

  return Object.keys(errors.value).length === 0
}

function handleSubmit() {
  if (!validateForm()) {
    return
  }

  const formData = {
    title: form.title.trim(),
    description: form.description?.trim() || undefined,
    priority: form.priority,
    categoryId: form.categoryId
  }

  emit('submit', formData)
}

function resetForm() {
  form.title = ''
  form.description = ''
  form.priority = 2
  form.categoryId = undefined
  errors.value = {}
}

// Real-time validation
watch(() => form.title, () => {
  if (errors.value.title) {
    if (form.title.trim() && form.title.length <= 200) {
      delete errors.value.title
    }
  }
})

watch(() => form.description, () => {
  if (errors.value.description) {
    if (!form.description || form.description.length <= 1000) {
      delete errors.value.description
    }
  }
})

// Expose methods for parent component
defineExpose({
  resetForm,
  validateForm
})
</script>

