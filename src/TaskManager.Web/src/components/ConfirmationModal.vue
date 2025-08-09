<template>
  <Teleport to="body">
    <!-- Backdrop -->
    <div
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click="$emit('cancel')"
    >
      <!-- Modal -->
      <div
        class="bg-white rounded-lg shadow-xl max-w-md w-full transform transition-all duration-200"
        @click.stop
      >
        <!-- Header -->
        <div class="px-6 py-4 border-b border-gray-200">
          <div class="flex items-center gap-3">
            <div
              class="w-10 h-10 rounded-full flex items-center justify-center"
              :class="iconClasses"
            >
              <i :class="iconName" class="text-white"></i>
            </div>
            <h3 class="text-lg font-medium text-gray-900">
              {{ title }}
            </h3>
          </div>
        </div>

        <!-- Content -->
        <div class="px-6 py-4">
          <p class="text-gray-600">
            {{ message }}
          </p>
        </div>

        <!-- Actions -->
        <div class="px-6 py-4 bg-gray-50 rounded-b-lg flex justify-end gap-3">
          <button
            @click="$emit('cancel')"
            class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors duration-200"
          >
            {{ cancelText }}
          </button>
          
          <button
            @click="$emit('confirm')"
            class="px-4 py-2 text-sm font-medium rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors duration-200"
            :class="confirmButtonClasses"
          >
            {{ confirmText }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'

// Props
interface Props {
  title: string
  message: string
  confirmText?: string
  cancelText?: string
  confirmVariant?: 'primary' | 'danger' | 'warning' | 'success'
}

const props = withDefaults(defineProps<Props>(), {
  confirmText: 'Confirm',
  cancelText: 'Cancel',
  confirmVariant: 'primary'
})

// Emits
interface Emits {
  confirm: []
  cancel: []
}

defineEmits<Emits>()

// Computed styles
const iconClasses = computed(() => {
  switch (props.confirmVariant) {
    case 'danger':
      return 'bg-red-100'
    case 'warning':
      return 'bg-yellow-100'
    case 'success':
      return 'bg-green-100'
    default:
      return 'bg-blue-100'
  }
})

const iconName = computed(() => {
  switch (props.confirmVariant) {
    case 'danger':
      return 'fas fa-exclamation-triangle text-red-600'
    case 'warning':
      return 'fas fa-exclamation text-yellow-600'
    case 'success':
      return 'fas fa-check text-green-600'
    default:
      return 'fas fa-question text-blue-600'
  }
})

const confirmButtonClasses = computed(() => {
  const baseClasses = 'text-white border border-transparent'
  
  switch (props.confirmVariant) {
    case 'danger':
      return `${baseClasses} bg-red-600 hover:bg-red-700 focus:ring-red-500`
    case 'warning':
      return `${baseClasses} bg-yellow-600 hover:bg-yellow-700 focus:ring-yellow-500`
    case 'success':
      return `${baseClasses} bg-green-600 hover:bg-green-700 focus:ring-green-500`
    default:
      return `${baseClasses} bg-primary-600 hover:bg-primary-700 focus:ring-primary-500`
  }
})
</script>

