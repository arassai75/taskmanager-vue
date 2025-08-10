import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { taskApi, apiUtils } from '@/services/api'
import type { 
  Task, 
  CreateTaskDto, 
  UpdateTaskDto, 
  TaskSearchDto, 
  PagedTasksDto, 
  TaskStatistics, 
  FilterOptions,
  NotificationMessage,
  LoadingState
} from '@/types'

export const useTaskStore = defineStore('tasks', () => {
  // State
  const tasks = ref<Task[]>([])
  const statistics = ref<TaskStatistics[]>([])
  const loading = ref<LoadingState>({
    isLoading: false,
    error: null
  })
  const notifications = ref<NotificationMessage[]>([])
  const filters = ref<FilterOptions>({
    showCompleted: true,
    selectedPriority: undefined,
    selectedCategory: undefined,
    searchTerm: ''
  })

  // Computed getters
  const activeTasks = computed(() => 
    tasks.value.filter(task => !task.isCompleted)
  )

  const completedTasks = computed(() => 
    tasks.value.filter(task => task.isCompleted)
  )

  const filteredTasks = computed(() => {
    let filtered = tasks.value

    // Filter by completion status
    if (!filters.value.showCompleted) {
      filtered = filtered.filter(task => !task.isCompleted)
    }

    // Filter by priority
    if (filters.value.selectedPriority) {
      filtered = filtered.filter(task => task.priority === filters.value.selectedPriority)
    }

    // Filter by category
    if (filters.value.selectedCategory) {
      filtered = filtered.filter(task => task.categoryId === filters.value.selectedCategory)
    }

    // Filter by search term
    if (filters.value.searchTerm.trim()) {
      const searchTerm = filters.value.searchTerm.toLowerCase()
      filtered = filtered.filter(task => 
        task.title.toLowerCase().includes(searchTerm) ||
        (task.description && task.description.toLowerCase().includes(searchTerm)) ||
        task.categoryName.toLowerCase().includes(searchTerm)
      )
    }

    return filtered
  })

  const taskCounts = computed(() => ({
    total: tasks.value.length,
    active: activeTasks.value.length,
    completed: completedTasks.value.length,
    highPriority: tasks.value.filter(t => t.priority === 3 && !t.isCompleted).length
  }))

  // Actions
  async function fetchTasks(includeCompleted = true) {
    try {
      setLoading(true)
      const fetchedTasks = await apiUtils.retryApiCall(() => 
        taskApi.getTasks(includeCompleted)
      )
      tasks.value = fetchedTasks
      showNotification('success', 'Tasks loaded', `${fetchedTasks.length} tasks loaded successfully`)
    } catch (error) {
      console.error('Failed to fetch tasks:', error)
      const errorMessage = apiUtils.formatErrorMessage(error)
      setError(errorMessage)
      showNotification('error', 'Failed to load tasks', errorMessage)
    } finally {
      setLoading(false)
    }
  }

  async function createTask(taskDto: CreateTaskDto) {
    try {
      setLoading(true)
      const newTask = await taskApi.createTask(taskDto)
      tasks.value.unshift(newTask) // Add to beginning of array
      showNotification('success', 'Task created', `"${newTask.title}" has been created successfully`)
      return newTask
    } catch (error) {
      const errorMessage = apiUtils.formatErrorMessage(error)
      setError(errorMessage)
      showNotification('error', 'Failed to create task', errorMessage)
      throw error
    } finally {
      setLoading(false)
    }
  }

  async function updateTask(id: number, taskDto: UpdateTaskDto) {
    try {
      setLoading(true)
      const updatedTask = await taskApi.updateTask(id, taskDto)
      const index = tasks.value.findIndex(t => t.id === id)
      if (index !== -1) {
        tasks.value[index] = updatedTask
      }
      showNotification('success', 'Task updated', `"${updatedTask.title}" has been updated successfully`)
      return updatedTask
    } catch (error) {
      const errorMessage = apiUtils.formatErrorMessage(error)
      setError(errorMessage)
      showNotification('error', 'Failed to update task', errorMessage)
      throw error
    } finally {
      setLoading(false)
    }
  }

  async function toggleTask(id: number) {
    try {
      // Optimistic update
      const taskIndex = tasks.value.findIndex(t => t.id === id)
      if (taskIndex === -1) return

      const task = tasks.value[taskIndex]
      const originalCompleted = task.isCompleted
      task.isCompleted = !task.isCompleted

      try {
        const updatedTask = await taskApi.toggleTask(id)
        tasks.value[taskIndex] = updatedTask
        
        const status = updatedTask.isCompleted ? 'completed' : 'reopened'
        showNotification('success', `Task ${status}`, `"${updatedTask.title}" has been ${status}`)
      } catch (error) {
        // Revert optimistic update on error
        task.isCompleted = originalCompleted
        throw error
      }
    } catch (error) {
      const errorMessage = apiUtils.formatErrorMessage(error)
      setError(errorMessage)
      showNotification('error', 'Failed to toggle task', errorMessage)
    }
  }

  async function deleteTask(id: number) {
    try {
      const task = tasks.value.find(t => t.id === id)
      if (!task) return

      // Optimistic update
      const originalTasks = [...tasks.value]
      tasks.value = tasks.value.filter(t => t.id !== id)

      try {
        await taskApi.deleteTask(id)
        showNotification('success', 'Task deleted', `"${task.title}" has been deleted successfully`)
      } catch (error) {
        // Revert optimistic update on error
        tasks.value = originalTasks
        throw error
      }
    } catch (error) {
      const errorMessage = apiUtils.formatErrorMessage(error)
      setError(errorMessage)
      showNotification('error', 'Failed to delete task', errorMessage)
    }
  }

  async function searchTasks(searchDto: TaskSearchDto): Promise<PagedTasksDto> {
    try {
      setLoading(true)
      const results = await taskApi.searchTasks(searchDto)
      // Update tasks if this is the first page
      if (searchDto.page === 1) {
        tasks.value = results.tasks
      }
      return results
    } catch (error) {
      const errorMessage = apiUtils.formatErrorMessage(error)
      setError(errorMessage)
      showNotification('error', 'Search failed', errorMessage)
      throw error
    } finally {
      setLoading(false)
    }
  }

  async function fetchStatistics() {
    try {
      const stats = await taskApi.getStatistics()
      statistics.value = stats
    } catch (error) {
      const errorMessage = apiUtils.formatErrorMessage(error)
      console.error('Failed to fetch statistics:', errorMessage)
      // Don't show notification for statistics errors as they're not critical
    }
  }

  async function bulkUpdateTasks(taskIds: number[], updates: Partial<UpdateTaskDto>) {
    try {
      setLoading(true)
      const result = await taskApi.bulkUpdateTasks(taskIds, updates)
      
      // Update local state
      taskIds.forEach(id => {
        const taskIndex = tasks.value.findIndex(t => t.id === id)
        if (taskIndex !== -1) {
          tasks.value[taskIndex] = { ...tasks.value[taskIndex], ...updates }
        }
      })

      showNotification('success', 'Bulk update completed', 
        `${result.updatedCount} tasks updated successfully`)
      return result
    } catch (error) {
      const errorMessage = apiUtils.formatErrorMessage(error)
      setError(errorMessage)
      showNotification('error', 'Bulk update failed', errorMessage)
      throw error
    } finally {
      setLoading(false)
    }
  }

  async function bulkDeleteTasks(taskIds: number[]) {
    try {
      setLoading(true)
      const result = await taskApi.bulkDeleteTasks(taskIds)
      
      // Update local state
      tasks.value = tasks.value.filter(t => !taskIds.includes(t.id))

      showNotification('success', 'Bulk delete completed', 
        `${result.deletedCount} tasks deleted successfully`)
      return result
    } catch (error) {
      const errorMessage = apiUtils.formatErrorMessage(error)
      setError(errorMessage)
      showNotification('error', 'Bulk delete failed', errorMessage)
      throw error
    } finally {
      setLoading(false)
    }
  }

  // Utility functions
  function setLoading(isLoading: boolean, error: string | null = null) {
    loading.value = { isLoading, error }
  }

  function setError(error: string) {
    loading.value = { isLoading: false, error }
  }

  function clearError() {
    loading.value = { isLoading: loading.value.isLoading, error: null }
  }

  function showNotification(
    type: NotificationMessage['type'], 
    title: string, 
    message: string, 
    duration = 5000
  ) {
    const notification: NotificationMessage = {
      id: Date.now().toString(),
      type,
      title,
      message,
      duration
    }
    
    notifications.value.push(notification)

    // Auto-remove notification after duration
    if (duration > 0) {
      setTimeout(() => {
        removeNotification(notification.id)
      }, duration)
    }
  }

  function removeNotification(id: string) {
    const index = notifications.value.findIndex(n => n.id === id)
    if (index !== -1) {
      notifications.value.splice(index, 1)
    }
  }

  function updateFilters(newFilters: Partial<FilterOptions>) {
    filters.value = { ...filters.value, ...newFilters }
  }

  function resetFilters() {
    filters.value = {
      showCompleted: true,
      selectedPriority: undefined,
      selectedCategory: undefined,
      searchTerm: ''
    }
  }

  function getTaskById(id: number): Task | undefined {
    return tasks.value.find(t => t.id === id)
  }

  function sortTasks(field: keyof Task, direction: 'asc' | 'desc' = 'desc') {
    tasks.value.sort((a, b) => {
      const aValue = a[field]
      const bValue = b[field]
      
      // Handle undefined values
      if (aValue == null && bValue == null) return 0
      if (aValue == null) return direction === 'asc' ? -1 : 1
      if (bValue == null) return direction === 'asc' ? 1 : -1
      
      if (aValue < bValue) return direction === 'asc' ? -1 : 1
      if (aValue > bValue) return direction === 'asc' ? 1 : -1
      return 0
    })
  }

  // Initialize store
  function initialize() {
    fetchTasks()
    fetchStatistics()
  }

  return {
    // State
    tasks,
    statistics,
    loading,
    notifications,
    filters,
    
    // Computed
    activeTasks,
    completedTasks,
    filteredTasks,
    taskCounts,
    
    // Actions
    fetchTasks,
    createTask,
    updateTask,
    toggleTask,
    deleteTask,
    searchTasks,
    fetchStatistics,
    bulkUpdateTasks,
    bulkDeleteTasks,
    
    // Utilities
    setLoading,
    setError,
    clearError,
    showNotification,
    removeNotification,
    updateFilters,
    resetFilters,
    getTaskById,
    sortTasks,
    initialize
  }
})
