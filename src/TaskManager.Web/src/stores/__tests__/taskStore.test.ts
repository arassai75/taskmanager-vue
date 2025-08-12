import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useTaskStore } from '../taskStore'
import type { Task, Category } from '../../types'

// Mock the API service
vi.mock('../../services/api', () => ({
  taskApi: {
    getTasks: vi.fn(),
    getTask: vi.fn(),
    createTask: vi.fn(),
    updateTask: vi.fn(),
    deleteTask: vi.fn(),
    toggleTask: vi.fn(),
    getStatistics: vi.fn(),
  },
  categoriesApi: {
    getCategories: vi.fn(),
  },
  apiUtils: {
    formatErrorMessage: vi.fn((error) => error?.message || 'Unknown error'),
    retryApiCall: vi.fn((apiCall) => apiCall()),
  },
}))

describe('TaskStore', () => {
  let pinia: any

  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)
    vi.clearAllMocks()
  })

  const createMockTask = (overrides: Partial<Task> = {}): Task => ({
    id: 1,
    title: 'Test Task',
    description: 'Test Description',
    priority: 2, // Medium priority (numeric)
    priorityText: 'Medium',
    categoryId: 1,
    categoryName: 'Work',
    categoryColor: '#FF0000',
    isCompleted: false,
    dueDate: undefined,
    estimatedHours: undefined,
    notificationsEnabled: false,
    isOverdue: false,
    isDueSoon: false,
    dueStatus: 'none',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    ...overrides,
  })

  const createMockCategory = (overrides: Partial<Category> = {}): Category => ({
    id: 1,
    name: 'Work',
    description: 'Work tasks',
    color: '#FF0000',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    activeTaskCount: 5,
    completedTaskCount: 3,
    completionPercentage: 37.5,
    ...overrides,
  })

  describe('State', () => {
    it('initializes with default state', () => {
      const store = useTaskStore()

      expect(store.tasks).toEqual([])
      expect(store.statistics).toEqual([])
      expect(store.loading).toEqual({
        isLoading: false,
        error: null
      })
      expect(store.notifications).toEqual([])
      expect(store.filters).toEqual({
        showCompleted: true,
        selectedPriority: undefined,
        selectedCategory: undefined,
        searchTerm: ''
      })
    })
  })

  describe('Computed Properties', () => {
    it('activeTasks returns only incomplete tasks', () => {
      const store = useTaskStore()
      store.tasks = [
        createMockTask({ id: 1, isCompleted: false }),
        createMockTask({ id: 2, isCompleted: true }),
        createMockTask({ id: 3, isCompleted: false }),
      ]

      expect(store.activeTasks).toHaveLength(2)
      expect(store.activeTasks.every(task => !task.isCompleted)).toBe(true)
    })

    it('completedTasks returns only completed tasks', () => {
      const store = useTaskStore()
      store.tasks = [
        createMockTask({ id: 1, isCompleted: false }),
        createMockTask({ id: 2, isCompleted: true }),
        createMockTask({ id: 3, isCompleted: true }),
      ]

      expect(store.completedTasks).toHaveLength(2)
      expect(store.completedTasks.every(task => task.isCompleted)).toBe(true)
    })

    it('taskCounts returns correct counts', () => {
      const store = useTaskStore()
      store.tasks = [
        createMockTask({ id: 1, isCompleted: false }),
        createMockTask({ id: 2, isCompleted: true }),
        createMockTask({ id: 3, isCompleted: false }),
        createMockTask({ id: 4, isCompleted: true }),
      ]

      expect(store.taskCounts).toEqual({
        total: 4,
        active: 2,
        completed: 2,
        highPriority: 0,
      })
    })

    it('filteredTasks applies search filter', () => {
      const store = useTaskStore()
      store.tasks = [
        createMockTask({ id: 1, title: 'Work Task', categoryName: 'Personal' }),
        createMockTask({ id: 2, title: 'Personal Task', categoryName: 'Work' }),
        createMockTask({ id: 3, title: 'Meeting Task', categoryName: 'Other' }),
      ]
      store.filters.searchTerm = 'Personal Task'

      expect(store.filteredTasks).toHaveLength(1)
      expect(store.filteredTasks[0].title).toBe('Personal Task')
    })

    it('filteredTasks applies priority filter', () => {
      const store = useTaskStore()
      store.tasks = [
        createMockTask({ id: 1, priority: 1, priorityText: 'Low' }),
        createMockTask({ id: 2, priority: 2, priorityText: 'Medium' }),
        createMockTask({ id: 3, priority: 3, priorityText: 'High' }),
      ]
      store.filters.selectedPriority = 3

      expect(store.filteredTasks).toHaveLength(1)
      expect(store.filteredTasks[0].priority).toBe(3)
    })

    it('filteredTasks applies category filter', () => {
      const store = useTaskStore()
      store.tasks = [
        createMockTask({ id: 1, categoryId: 1, categoryName: 'Work' }),
        createMockTask({ id: 2, categoryId: 2, categoryName: 'Personal' }),
        createMockTask({ id: 3, categoryId: 1, categoryName: 'Work' }),
      ]
      store.filters.selectedCategory = 1

      expect(store.filteredTasks).toHaveLength(2)
      expect(store.filteredTasks.every(task => task.categoryId === 1)).toBe(true)
    })

    it('filteredTasks applies completion filter', () => {
      const store = useTaskStore()
      store.tasks = [
        createMockTask({ id: 1, isCompleted: false }),
        createMockTask({ id: 2, isCompleted: true }),
        createMockTask({ id: 3, isCompleted: false }),
      ]
      store.filters.showCompleted = false

      expect(store.filteredTasks).toHaveLength(2)
      expect(store.filteredTasks.every(task => !task.isCompleted)).toBe(true)
    })
  })

  describe('Actions', () => {
    it('fetchTasks loads tasks from API', async () => {
      const mockTasks = [createMockTask(), createMockTask({ id: 2 })]
      const { taskApi, apiUtils } = await import('../../services/api')
      vi.mocked(taskApi.getTasks).mockResolvedValue(mockTasks)

      const store = useTaskStore()
      await store.fetchTasks()

      expect(taskApi.getTasks).toHaveBeenCalled()
      expect(store.tasks).toEqual(mockTasks)
    })

    it('createTask adds new task', async () => {
      const newTask = createMockTask({ id: 999, title: 'New Task' })
      const { taskApi } = await import('../../services/api')
      vi.mocked(taskApi.createTask).mockResolvedValue(newTask)

      const store = useTaskStore()
      const createTaskDto = {
        title: 'New Task',
        description: 'New Description',
        priority: 2,
        categoryId: 1,
      }

      const result = await store.createTask(createTaskDto)

      expect(taskApi.createTask).toHaveBeenCalledWith(createTaskDto)
      expect(result).toEqual(newTask)
      expect(store.tasks).toHaveLength(1)
      expect(store.tasks[0]).toEqual(newTask)
    })

    it('updateTask updates existing task', async () => {
      const existingTask = createMockTask()
      const updatedTask = { ...existingTask, title: 'Updated Task' }
      const { taskApi } = await import('../../services/api')
      vi.mocked(taskApi.updateTask).mockResolvedValue(updatedTask)

      const store = useTaskStore()
      store.tasks = [existingTask]

      const updateTaskDto = {
        title: 'Updated Task',
        description: 'Updated Description',
        priority: 3,
        categoryId: 2,
      }

      await store.updateTask(existingTask.id, updateTaskDto)

      expect(taskApi.updateTask).toHaveBeenCalledWith(existingTask.id, updateTaskDto)
      expect(store.tasks[0].title).toBe('Updated Task')
    })

    it('deleteTask removes task', async () => {
      const task = createMockTask()
      const { taskApi } = await import('../../services/api')
      vi.mocked(taskApi.deleteTask).mockResolvedValue()

      const store = useTaskStore()
      store.tasks = [task]

      await store.deleteTask(task.id)

      expect(taskApi.deleteTask).toHaveBeenCalledWith(task.id)
      expect(store.tasks).toHaveLength(0)
    })

    it('toggleTask toggles completion status', async () => {
      const task = createMockTask()
      const toggledTask = { ...task, isCompleted: !task.isCompleted }
      const { taskApi } = await import('../../services/api')
      vi.mocked(taskApi.toggleTask).mockResolvedValue(toggledTask)

      const store = useTaskStore()
      store.tasks = [task]

      await store.toggleTask(task.id)

      expect(taskApi.toggleTask).toHaveBeenCalledWith(task.id)
      expect(store.tasks[0].isCompleted).toBe(toggledTask.isCompleted)
    })

    it('searchTasks updates search filter', () => {
      const store = useTaskStore()
      store.filters.searchTerm = 'test search'

      expect(store.filters.searchTerm).toBe('test search')
    })

    it('fetchStatistics loads statistics from API', async () => {
      const mockStats = [
        {
          category: 'Work',
          totalTasks: 10,
          completedTasks: 6,
          pendingTasks: 4,
          highPriorityPending: 2,
          completionPercentage: 60,
        }
      ]
      const { taskApi } = await import('../../services/api')
      vi.mocked(taskApi.getStatistics).mockResolvedValue(mockStats)

      const store = useTaskStore()
      await store.fetchStatistics()

      expect(taskApi.getStatistics).toHaveBeenCalled()
      expect(store.statistics).toEqual(mockStats)
    })

    it('setLoading updates loading state', () => {
      const store = useTaskStore()
      store.setLoading(true)

      expect(store.loading.isLoading).toBe(true)
    })

    it('setError updates error state', () => {
      const store = useTaskStore()
      store.setError('Test error')

      expect(store.loading.error).toBe('Test error')
    })

    it('clearError clears error state', () => {
      const store = useTaskStore()
      store.setError('Test error')
      store.clearError()

      expect(store.loading.error).toBe(null)
    })

    it('showNotification adds notification', () => {
      const store = useTaskStore()
      store.showNotification('success', 'Test Title', 'Test message')

      expect(store.notifications).toHaveLength(1)
      expect(store.notifications[0].message).toBe('Test message')
      expect(store.notifications[0].type).toBe('success')
    })

    it('removeNotification removes notification', () => {
      const store = useTaskStore()
      store.showNotification('success', 'Test Title', 'Test message')
      const notificationId = store.notifications[0].id

      store.removeNotification(notificationId)

      expect(store.notifications).toHaveLength(0)
    })

    it('updateFilters updates filter state', () => {
      const store = useTaskStore()
      const newFilters = {
        searchTerm: 'test',
        selectedPriority: 3,
        selectedCategory: 1,
        showCompleted: false,
      }

      store.updateFilters(newFilters)

      expect(store.filters.searchTerm).toBe('test')
      expect(store.filters.selectedPriority).toBe(3)
      expect(store.filters.selectedCategory).toBe(1)
      expect(store.filters.showCompleted).toBe(false)
    })

    it('resetFilters resets to default state', () => {
      const store = useTaskStore()
      store.filters.searchTerm = 'test'
      store.filters.selectedPriority = 3

      store.resetFilters()

      expect(store.filters).toEqual({
        showCompleted: true,
        selectedPriority: undefined,
        selectedCategory: undefined,
        searchTerm: ''
      })
    })

    it('getTaskById returns task by id', () => {
      const task = createMockTask()
      const store = useTaskStore()
      store.tasks = [task]

      const foundTask = store.getTaskById(task.id)

      expect(foundTask).toEqual(task)
    })

    it('getTaskById returns undefined for non-existent task', () => {
      const store = useTaskStore()
      store.tasks = [createMockTask()]

      const foundTask = store.getTaskById(999)

      expect(foundTask).toBeUndefined()
    })

    it('sortTasks sorts tasks by field and direction', () => {
      const store = useTaskStore()
      store.tasks = [
        createMockTask({ id: 1, title: 'Zebra' }),
        createMockTask({ id: 2, title: 'Alpha' }),
        createMockTask({ id: 3, title: 'Beta' }),
      ]

      store.sortTasks('title', 'asc')

      expect(store.tasks[0].title).toBe('Alpha')
      expect(store.tasks[1].title).toBe('Beta')
      expect(store.tasks[2].title).toBe('Zebra')
    })

    it('initialize sets up the store', async () => {
      const mockTasks = [createMockTask()]
      const mockStats = [
        {
          category: 'Work',
          totalTasks: 1,
          completedTasks: 0,
          pendingTasks: 1,
          highPriorityPending: 0,
          completionPercentage: 0,
        }
      ]
      const { taskApi } = await import('../../services/api')
      vi.mocked(taskApi.getTasks).mockResolvedValue(mockTasks)
      vi.mocked(taskApi.getStatistics).mockResolvedValue(mockStats)

      const store = useTaskStore()
      await store.initialize()

      expect(taskApi.getTasks).toHaveBeenCalled()
      expect(taskApi.getStatistics).toHaveBeenCalled()
      expect(store.tasks).toEqual(mockTasks)
      expect(store.statistics).toEqual(mockStats)
    })
  })
})
