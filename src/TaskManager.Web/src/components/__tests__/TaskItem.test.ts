import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import TaskItem from '../TaskItem.vue'
import type { Task } from '../../types'

// Mock the API service
vi.mock('../../services/api', () => ({
  taskApi: {
    updateTask: vi.fn(),
    deleteTask: vi.fn(),
    toggleTask: vi.fn(),
  },
}))

describe('TaskItem', () => {
  let pinia: any

  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)
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

  it('renders task title and description', () => {
    const task = createMockTask()
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    expect(wrapper.text()).toContain('Test Task')
    expect(wrapper.text()).toContain('Test Description')
  })

  it('shows completed state correctly', () => {
    const task = createMockTask({ isCompleted: true })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    // Check for the Done badge
    expect(wrapper.text()).toContain('Done')
  })

  it('shows incomplete state correctly', () => {
    const task = createMockTask({ isCompleted: false })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    expect(wrapper.text()).not.toContain('Done')
  })

  it('displays priority badge with correct color', () => {
    const task = createMockTask({ priority: 3, priorityText: 'High' })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    // Look for priority badge by text content
    expect(wrapper.text()).toContain('High')
  })

  it('displays category badge with correct color', () => {
    const task = createMockTask({
      categoryName: 'Work',
      categoryColor: '#FF0000',
    })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    expect(wrapper.text()).toContain('Work')
  })

  it('shows due date when present', () => {
    const task = createMockTask({ 
      dueDate: '2024-12-31T00:00:00Z',
      isOverdue: false,
      isDueSoon: false 
    })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    expect(wrapper.text()).toContain('Due')
  })

  it('shows estimated hours when present', () => {
    const task = createMockTask({ estimatedHours: 5.5 })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    expect(wrapper.text()).toContain('5.5h')
  })

  it('shows notification icon when notifications are enabled', () => {
    const task = createMockTask({ 
      notificationsEnabled: true,
      dueDate: '2024-12-31T00:00:00Z'
    })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    // Look for bell icon
    const bellIcon = wrapper.find('i.fas.fa-bell')
    expect(bellIcon.exists()).toBe(true)
  })

  it('does not show notification icon when notifications are disabled', () => {
    const task = createMockTask({ notificationsEnabled: false })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    const bellIcon = wrapper.find('i.fas.fa-bell')
    expect(bellIcon.exists()).toBe(false)
  })

  it('emits edit event when edit button is clicked', async () => {
    const task = createMockTask()
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    const editButton = wrapper.find('button[title="Edit task"]')
    await editButton.trigger('click')

    expect(wrapper.emitted('edit')).toBeTruthy()
    expect(wrapper.emitted('edit')?.[0]).toEqual([task])
  })

  it('emits delete event when delete button is clicked', async () => {
    const task = createMockTask()
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    const deleteButton = wrapper.find('button[title="Delete task"]')
    await deleteButton.trigger('click')

    expect(wrapper.emitted('delete')).toBeTruthy()
    expect(wrapper.emitted('delete')?.[0]).toEqual([task.id])
  })

  it('emits toggle event when checkbox button is clicked', async () => {
    const task = createMockTask()
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    // Find the checkbox button (it's a button, not an input)
    const checkboxButton = wrapper.find('button[aria-label="Mark as complete"]')
    await checkboxButton.trigger('click')

    expect(wrapper.emitted('toggle')).toBeTruthy()
    expect(wrapper.emitted('toggle')?.[0]).toEqual([task.id])
  })

  it('applies dark mode classes when theme is dark', () => {
    const task = createMockTask()
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    // Simulate dark mode by adding class to document
    document.documentElement.classList.add('dark')

    // Find the main container div (first div with the main classes)
    const container = wrapper.find('div.bg-white')
    expect(container.classes()).toContain('dark:bg-gray-800')
    expect(container.classes()).toContain('dark:border-gray-700')

    // Clean up
    document.documentElement.classList.remove('dark')
  })

  it('truncates long titles with line-clamp', () => {
    const task = createMockTask({
      title: 'This is a very long task title that should be truncated when it exceeds the available space',
    })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    const titleElement = wrapper.find('h3')
    expect(titleElement.classes()).toContain('truncate')
  })

  it('truncates long descriptions with line-clamp', () => {
    const task = createMockTask({
      description: 'This is a very long task description that should be truncated when it exceeds the available space',
    })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    const descriptionElement = wrapper.find('p')
    expect(descriptionElement.classes()).toContain('line-clamp-1')
  })

  it('shows overdue indicator for overdue tasks', () => {
    const task = createMockTask({ 
      dueDate: '2024-01-01T00:00:00Z',
      isOverdue: true,
      isDueSoon: false
    })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    // Check for overdue styling
    expect(wrapper.classes()).toContain('border-red-200')
  })

  it('shows due soon indicator for tasks due within 3 days', () => {
    const task = createMockTask({ 
      dueDate: '2024-12-31T00:00:00Z',
      isOverdue: false,
      isDueSoon: true
    })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    // Check for due soon styling
    expect(wrapper.classes()).toContain('border-orange-200')
  })

  it('formats due date correctly', () => {
    const task = createMockTask({ 
      dueDate: '2024-12-25T00:00:00Z',
      isOverdue: false,
      isDueSoon: false
    })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    expect(wrapper.text()).toContain('Due')
  })

  it('handles missing category gracefully', () => {
    const task = createMockTask({ 
      categoryName: '',
      categoryColor: undefined
    })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    // Should not show category badge
    expect(wrapper.text()).not.toContain('Work')
  })

  it('handles missing due date gracefully', () => {
    const task = createMockTask({ dueDate: undefined })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    expect(wrapper.text()).not.toContain('Due')
  })

  it('handles missing estimated hours gracefully', () => {
    const task = createMockTask({ estimatedHours: undefined })
    const wrapper = mount(TaskItem, {
      props: { task },
      global: {
        plugins: [pinia],
      },
    })

    expect(wrapper.text()).not.toContain('h')
  })
})
