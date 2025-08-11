import axios from 'axios'
import type { AxiosInstance, AxiosResponse, AxiosError } from 'axios'
import type { 
  Task, 
  CreateTaskDto, 
  UpdateTaskDto, 
  TaskSearchDto, 
  PagedTasksDto, 
  TaskStatistics,
  ApiError 
} from '@/types'

// Create axios instance with default configuration
const api: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor for logging and auth (future enhancement)
api.interceptors.request.use(
  (config) => {
    console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`)
    return config
  },
  (error) => {
    console.error('API Request Error:', error)
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
api.interceptors.response.use(
  (response: AxiosResponse) => {
    console.log(`API Response: ${response.status} ${response.config.url}`)
    return response
  },
  (error: AxiosError) => {
    console.error('API Response Error:', error)
    
    // Handle different error scenarios
    if (error.response) {
      // Server responded with error status
      const responseData = error.response.data as any
      const apiError: ApiError = {
        title: responseData?.title || 'API Error',
        detail: responseData?.detail || error.message,
        status: error.response.status,
        type: responseData?.type
      }
      return Promise.reject(apiError)
    } else if (error.request) {
      // Network error
      const networkError: ApiError = {
        title: 'Network Error',
        detail: 'Unable to connect to the server. Please check your internet connection.',
        status: 0
      }
      return Promise.reject(networkError)
    } else {
      // Something else happened
      const unknownError: ApiError = {
        title: 'Unknown Error',
        detail: error.message || 'An unexpected error occurred',
        status: -1
      }
      return Promise.reject(unknownError)
    }
  }
)

// Task API service
export const taskApi = {
  // Get all tasks with optional filtering
  async getTasks(includeCompleted: boolean = true): Promise<Task[]> {
    const response = await api.get<Task[]>('/tasks', {
      params: { includeCompleted }
    })
    return response.data
  },

  // Get a specific task by ID
  async getTask(id: number): Promise<Task> {
    const response = await api.get<Task>(`/tasks/${id}`)
    return response.data
  },

  // Create a new task
  async createTask(taskDto: CreateTaskDto): Promise<Task> {
    const response = await api.post<Task>('/tasks', taskDto)
    return response.data
  },

  // Update an existing task
  async updateTask(id: number, taskDto: UpdateTaskDto): Promise<Task> {
    const response = await api.put<Task>(`/tasks/${id}`, taskDto)
    return response.data
  },

  // Toggle task completion status
  async toggleTask(id: number): Promise<Task> {
    const response = await api.patch<Task>(`/tasks/${id}/toggle`)
    return response.data
  },

  // Delete a task (soft delete)
  async deleteTask(id: number): Promise<void> {
    await api.delete(`/tasks/${id}`)
  },

  // Search tasks with advanced filtering
  async searchTasks(searchDto: TaskSearchDto): Promise<PagedTasksDto> {
    const response = await api.post<PagedTasksDto>('/tasks/search', searchDto)
    return response.data
  },

  // Get task statistics
  async getStatistics(): Promise<TaskStatistics[]> {
    const response = await api.get<TaskStatistics[]>('/tasks/statistics')
    return response.data
  },

  // Get tasks by priority
  async getTasksByPriority(priority: number): Promise<Task[]> {
    const response = await api.get<Task[]>(`/tasks/priority/${priority}`)
    return response.data
  },

  // Bulk update tasks
  async bulkUpdateTasks(taskIds: number[], updates: Partial<UpdateTaskDto>): Promise<{ updatedCount: number }> {
    const response = await api.patch<{ updatedCount: number }>('/tasks/bulk', {
      taskIds,
      ...updates
    })
    return response.data
  },

  // Bulk delete tasks
  async bulkDeleteTasks(taskIds: number[]): Promise<{ deletedCount: number }> {
    const response = await api.delete<{ deletedCount: number }>('/tasks/bulk', {
      data: { taskIds }
    })
    return response.data
  }
}

// Utility functions for API calls
export const apiUtils = {
  // Check if error is an API error
  isApiError(error: unknown): error is ApiError {
    return typeof error === 'object' && 
           error !== null && 
           'title' in error && 
           'detail' in error && 
           'status' in error
  },

  // Format error message for display
  formatErrorMessage(error: unknown): string {
    if (this.isApiError(error)) {
      return `${error.title}: ${error.detail}`
    }
    if (error instanceof Error) {
      return error.message
    }
    return 'An unexpected error occurred'
  },

  // Retry API call with exponential backoff
  async retryApiCall<T>(
    apiCall: () => Promise<T>,
    maxRetries: number = 3,
    baseDelay: number = 1000
  ): Promise<T> {
    let lastError: unknown

    for (let attempt = 1; attempt <= maxRetries; attempt++) {
      try {
        return await apiCall()
      } catch (error) {
        lastError = error
        
        if (attempt === maxRetries) {
          throw error
        }

        // Only retry on network errors or 5xx status codes
        if (this.isApiError(error) && error.status >= 400 && error.status < 500) {
          throw error // Don't retry client errors
        }

        const delay = baseDelay * Math.pow(2, attempt - 1)
        console.log(`Retrying API call in ${delay}ms (attempt ${attempt}/${maxRetries})`)
        await new Promise(resolve => setTimeout(resolve, delay))
      }
    }

    throw lastError
  }
}

// Export the configured axios instance for custom calls
export default api
