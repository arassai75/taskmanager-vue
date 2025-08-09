<template>
  <div class="bg-white dark:bg-gray-600 rounded-lg shadow-md p-6 transition-colors duration-300">
    <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-6">
      {{ isEditing ? 'Edit Task' : 'Create New Task' }}
    </h2>

    <form @submit.prevent="handleSubmit" class="space-y-4">
      <!-- Title Field -->
      <div>
        <label for="title" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
          Title <span class="text-red-500">*</span>
        </label>
        <input
          id="title"
          v-model="form.title"
          type="text"
          required
          maxlength="200"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-500 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
          :class="{ 'border-red-500': errors.title }"
          placeholder="Enter task title..."
        />
        <div v-if="errors.title" class="mt-1 text-sm text-red-600">
          {{ errors.title }}
        </div>
        <div class="mt-1 text-xs text-gray-500 dark:text-gray-400">
          {{ form.title.length }}/200 characters
        </div>
      </div>

      <!-- Description Field -->
      <div>
        <label for="description" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
          Description
        </label>
        <textarea
          id="description"
          v-model="form.description"
          rows="3"
          maxlength="1000"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-500 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200 resize-none"
          :class="{ 'border-red-500': errors.description }"
          placeholder="Enter task description (optional)..."
        ></textarea>
        <div v-if="errors.description" class="mt-1 text-sm text-red-600">
          {{ errors.description }}
        </div>
        <div class="mt-1 text-xs text-gray-500 dark:text-gray-400">
          {{ (form.description || '').length }}/1000 characters
        </div>
      </div>

      <!-- Priority and Category Row -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Priority Field -->
        <div>
          <label for="priority" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Priority <span class="text-red-500">*</span>
          </label>
          <select
            id="priority"
            v-model.number="form.priority"
            required
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-500 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
            :class="{ 'border-red-500': errors.priority }"
          >
            <option :value="1">Low Priority</option>
            <option :value="2">Medium Priority</option>
            <option :value="3">High Priority</option>
          </select>
          <div v-if="errors.priority" class="mt-1 text-sm text-red-600">
            {{ errors.priority }}
          </div>
        </div>

        <!-- Category Field -->
        <div>
          <label for="category" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Category
          </label>
          <div class="relative">
            <select
              id="category"
              v-model="form.categoryId"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-500 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
              :class="{ 'border-red-500': errors.categoryId }"
            >
              <option :value="undefined">No Category</option>
              <option 
                v-for="category in categories" 
                :key="category.id" 
                :value="category.id"
              >
                {{ category.name }}
              </option>
            </select>
            <!-- Color indicator for selected category -->
            <div 
              v-if="selectedCategory"
              class="absolute right-10 top-1/2 transform -translate-y-1/2 w-4 h-4 rounded-sm border border-gray-300 dark:border-gray-500"
              :style="{ backgroundColor: selectedCategory.color || '#6B7280' }"
              :title="selectedCategory.name"
            ></div>
          </div>
          <div v-if="errors.categoryId" class="mt-1 text-sm text-red-600">
            {{ errors.categoryId }}
          </div>
          <!-- Debug info -->
          <div v-if="categories.length === 0" class="mt-1 text-xs text-orange-600">
            Loading categories... ({{ categoriesLoading ? 'Loading' : 'No categories found' }})
          </div>
          <div v-else class="mt-1 text-xs text-green-600">
            {{ categories.length }} categories loaded
          </div>
        </div>
      </div>

      <!-- Due Date and Estimate Row -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Due Date Field -->
        <div>
          <label for="dueDate" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Due Date
          </label>
          <input
            id="dueDate"
            v-model="form.dueDate"
            type="datetime-local"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-500 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
            :class="{ 'border-red-500': errors.dueDate }"
          />
          <div v-if="errors.dueDate" class="mt-1 text-sm text-red-600">
            {{ errors.dueDate }}
          </div>
          <div class="mt-1 text-xs text-gray-500 dark:text-gray-400">
            Optional - Set a deadline for this task
          </div>
        </div>

        <!-- Estimated Hours Field -->
        <div>
          <label for="estimatedHours" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Estimated Hours
          </label>
          <input
            id="estimatedHours"
            v-model.number="form.estimatedHours"
            type="number"
            step="0.1"
            placeholder="e.g., 2.5 (optional)"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-500 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-colors duration-200"
            :class="{ 'border-red-500': errors.estimatedHours }"
          />
          <div v-if="errors.estimatedHours" class="mt-1 text-sm text-red-600">
            {{ errors.estimatedHours }}
          </div>
          <div class="mt-1 text-xs text-gray-500 dark:text-gray-400">
            Optional - How long do you think this will take?
          </div>
        </div>
      </div>

      <!-- Notifications Toggle -->
      <div class="flex items-center space-x-3">
        <input
          id="notificationsEnabled"
          v-model="form.notificationsEnabled"
          type="checkbox"
          class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 dark:border-gray-500 rounded"
        />
        <label for="notificationsEnabled" class="text-sm font-medium text-gray-700 dark:text-gray-300">
          🔔 Enable notifications for this task
        </label>
      </div>

      <!-- Form Actions -->
      <div class="flex justify-end gap-3 pt-6 border-t border-gray-200 dark:border-gray-500">
        <button
          type="button"
          @click="$emit('cancel')"
          class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-500 border border-gray-300 dark:border-gray-400 rounded-md hover:bg-gray-200 dark:hover:bg-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors duration-200"
          :disabled="isSubmitting"
        >
          Cancel
        </button>
        
        <button
          type="submit"
          class="px-4 py-2 text-sm font-medium text-white bg-primary-600 border border-transparent rounded-md hover:bg-primary-700 dark:bg-primary-500 dark:hover:bg-primary-600 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
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
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { categoriesApi } from '../services/api'
import type { Task, CreateTaskDto, UpdateTaskDto, Category } from '../types'

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
  categoryId: undefined as number | undefined,
  dueDate: '',
  estimatedHours: undefined as number | undefined,
  notificationsEnabled: true
})

// Validation errors
const errors = ref<Record<string, string>>({})

// Categories state
const categories = ref<Category[]>([])
const categoriesLoading = ref(false)

// Selected category computed property
const selectedCategory = computed(() => {
  if (!form.categoryId) return null
  return categories.value.find(cat => cat.id === form.categoryId) || null
})

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
      form.dueDate = newTask.dueDate ? new Date(newTask.dueDate).toISOString().slice(0, 16) : ''
      form.estimatedHours = newTask.estimatedHours
      form.notificationsEnabled = newTask.notificationsEnabled ?? true
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

  // Estimated hours validation (optional field)
  if (form.estimatedHours !== undefined && form.estimatedHours !== null && !isNaN(form.estimatedHours)) {
    if (form.estimatedHours < 0.1 || form.estimatedHours > 999.99) {
      errors.value.estimatedHours = 'Estimated hours must be between 0.1 and 999.99'
    }
  }

  // Due date validation (optional: warn if in the past)
  if (form.dueDate) {
    const dueDate = new Date(form.dueDate)
    if (dueDate < new Date()) {
      errors.value.dueDate = 'Due date is in the past. Are you sure?'
    }
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
    categoryId: form.categoryId,
    dueDate: form.dueDate || undefined,
    estimatedHours: (form.estimatedHours && form.estimatedHours > 0) ? form.estimatedHours : undefined,
    notificationsEnabled: form.notificationsEnabled
  }

  emit('submit', formData)
}

function resetForm() {
  form.title = ''
  form.description = ''
  form.priority = 2
  form.categoryId = undefined
  form.dueDate = ''
  form.estimatedHours = undefined
  form.notificationsEnabled = true
  errors.value = {}
}

// Load categories
async function loadCategories() {
  try {
    categoriesLoading.value = true
    console.log('Loading categories from API...')
    categories.value = await categoriesApi.getCategories()
    console.log('Categories loaded:', categories.value)
  } catch (error) {
    console.error('Failed to load categories:', error)
    // Fallback to empty array if API fails
    categories.value = []
  } finally {
    categoriesLoading.value = false
  }
}

// Initialize categories on component mount
onMounted(() => {
  loadCategories()
})

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

