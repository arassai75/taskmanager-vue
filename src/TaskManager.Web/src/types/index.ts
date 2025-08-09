// Task-related types matching the backend DTOs

export interface Task {
  id: number
  title: string
  description?: string
  isCompleted: boolean
  priority: number
  priorityText: string
  categoryId?: number
  categoryName: string
  categoryColor?: string
  createdAt: string
  updatedAt: string
}

export interface CreateTaskDto {
  title: string
  description?: string
  priority: number
  categoryId?: number
}

export interface UpdateTaskDto {
  title: string
  description?: string
  priority: number
  categoryId?: number
}

export interface TaskSearchDto {
  searchTerm?: string
  isCompleted?: boolean
  priority?: number
  categoryId?: number
  createdAfter?: string
  createdBefore?: string
  pageSize: number
  page: number
}

export interface PagedTasksDto {
  tasks: Task[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface TaskStatistics {
  category: string
  totalTasks: number
  completedTasks: number
  pendingTasks: number
  highPriorityPending: number
  completionPercentage: number
}

export interface Category {
  id: number
  name: string
  description?: string
  color?: string
  isActive: boolean
  createdAt: string
  activeTaskCount: number
  completedTaskCount: number
  completionPercentage: number
}

// UI-specific types
export interface FilterOptions {
  showCompleted: boolean
  selectedPriority?: number
  selectedCategory?: number
  searchTerm: string
}

export interface NotificationMessage {
  id: string
  type: 'success' | 'error' | 'warning' | 'info'
  title: string
  message: string
  duration?: number
}

// API response types
export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
}

export interface ApiError {
  title: string
  detail: string
  status: number
  type?: string
}

// Priority enumeration for better type safety
export enum TaskPriority {
  Low = 1,
  Medium = 2,
  High = 3
}

// Priority display configuration
export const PRIORITY_CONFIG = {
  [TaskPriority.Low]: {
    label: 'Low',
    color: 'text-gray-600',
    bgColor: 'bg-gray-100',
    badgeColor: 'bg-gray-200',
    icon: 'fa-arrow-down'
  },
  [TaskPriority.Medium]: {
    label: 'Medium',
    color: 'text-yellow-600',
    bgColor: 'bg-yellow-100',
    badgeColor: 'bg-yellow-200',
    icon: 'fa-minus'
  },
  [TaskPriority.High]: {
    label: 'High',
    color: 'text-red-600',
    bgColor: 'bg-red-100',
    badgeColor: 'bg-red-200',
    icon: 'fa-arrow-up'
  }
} as const

// Loading states
export interface LoadingState {
  isLoading: boolean
  error: string | null
}

// Form validation types
export interface ValidationErrors {
  [key: string]: string[]
}

export interface FormState<T> {
  data: T
  errors: ValidationErrors
  isSubmitting: boolean
  isDirty: boolean
}

// Sorting configuration
export interface SortConfig {
  field: keyof Task
  direction: 'asc' | 'desc'
}

export const SORT_OPTIONS = [
  { value: 'createdAt:desc', label: 'Newest First' },
  { value: 'createdAt:asc', label: 'Oldest First' },
  { value: 'priority:desc', label: 'High Priority First' },
  { value: 'priority:asc', label: 'Low Priority First' },
  { value: 'title:asc', label: 'Title A-Z' },
  { value: 'title:desc', label: 'Title Z-A' }
] as const

