import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import LearningPath from '../../../components/LearningPath.vue'

const flushPromises = () => new Promise((resolve) => setTimeout(resolve, 0))
const routerPush = vi.fn()
const authState = { currentUser: { userId: 11 } }

vi.mock('@/api/services', () => ({
  analyticsApi: {
    getLearningPath: vi.fn()
  }
}))

vi.mock('@/stores/auth', () => ({
  useAuthStore: () => authState
}))

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: routerPush })
}))

import { analyticsApi } from '@/api/services'

describe('LearningPath', () => {
  beforeEach(() => {
    routerPush.mockClear()
    authState.currentUser = { userId: 11 }
    vi.clearAllMocks()
  })

  it('renders plan and navigates on start button click', async () => {
    analyticsApi.getLearningPath.mockResolvedValue({
      data: {
        plannedOrder: [
          {
            moduleId: 101,
            moduleTitle: 'Основной модуль',
            priority: 9,
            reason: 'Тестовая причина',
            estimatedHours: 4,
            dependsOn: 'Введение',
            isCompleted: false,
            dayNumber: 1
          }
        ],
        strategy: 'balanced',
        aiExplanation: 'План создан',
        estimatedCompletionDays: 5
      }
    })

    const wrapper = mount(LearningPath)
    await flushPromises()

    expect(wrapper.text()).toContain('Ваш персональный путь обучения')
    expect(wrapper.text()).toContain('Основной модуль')
    await wrapper.get('button').trigger('click')
    expect(routerPush).toHaveBeenCalledWith('/module/101')
  })

  it('shows error when no current user exists', async () => {
    authState.currentUser = null

    const wrapper = mount(LearningPath)
    await flushPromises()

    expect(wrapper.text()).toContain('Пользователь не найден')
  })
})
