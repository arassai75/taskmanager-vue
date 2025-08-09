<template>
  <svg 
    viewBox="0 0 12 12" 
    fill="none" 
    xmlns="http://www.w3.org/2000/svg"
    class="w-full h-full"
  >
    <!-- Low Priority: Three downward chevrons stacked vertically (Blue) -->
    <template v-if="priority === 1">
      <path d="M3 2 L6 4 L9 2" stroke="#3B82F6" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"/>
      <path d="M3 5 L6 7 L9 5" stroke="#3B82F6" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"/>
      <path d="M3 8 L6 10 L9 8" stroke="#3B82F6" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"/>
    </template>
    
    <!-- Medium Priority: Three horizontal dashes stacked vertically (Orange) -->
    <template v-else-if="priority === 2">
      <line x1="2" y1="3" x2="10" y2="3" stroke="#F59E0B" stroke-width="1.5" stroke-linecap="round"/>
      <line x1="2" y1="6" x2="10" y2="6" stroke="#F59E0B" stroke-width="1.5" stroke-linecap="round"/>
      <line x1="2" y1="9" x2="10" y2="9" stroke="#F59E0B" stroke-width="1.5" stroke-linecap="round"/>
    </template>
    
    <!-- High Priority: Three upward chevrons stacked vertically (Red) -->
    <template v-else-if="priority === 3">
      <path d="M3 4 L6 2 L9 4" stroke="#EF4444" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"/>
      <path d="M3 7 L6 5 L9 7" stroke="#EF4444" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"/>
      <path d="M3 10 L6 8 L9 10" stroke="#EF4444" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"/>
    </template>
  </svg>
</template>

<script setup lang="ts">
// Props
interface Props {
  priority: number
}

defineProps<Props>()
</script>
