import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import AnalyticsDashboard from '../../../components/AnalyticsDashboard.vue'

const flushPromises = () => new Promise((resolve) => setTimeout(resolve, 0))
const routerPush = vi.fn()

vi.mock('@/api/services', () => ({
  analyticsApi: {
    getDashboard: vi.fn()
  }
}))

vi.mock('@/stores/auth', () => ({
  useAuthStore: () => ({
    currentUser: { userId: 42 }
  })
}))

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: routerPush })
}))

import { analyticsApi } from '@/api/services'

describe('AnalyticsDashboard', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    routerPush.mockClear()
  })

  it('renders dashboard data and navigates to module when button clicked', async () => {
    analyticsApi.getDashboard.mockResolvedValue({
      data: {
        analytics: {
          weakAreas: [],
          strengths: [],
          recommendations: ['Рекомендация 1'],
          estimatedCompletionDays: 7,
          aiInsight: 'Хороший прогресс'
        },
        nextModule: {
          moduleId: 99,
          moduleTitle: 'Следующий модуль',
          priority: 8,
          reason: 'Анализирует важные навыки',
          estimatedHours: 3,
          dependsOn: 'Введение'
        }
      }
    })

    const wrapper = mount(AnalyticsDashboard)
    await flushPromises()

    expect(wrapper.text()).toContain('Рекомендации')
    expect(wrapper.text()).toContain('Следующий модуль')
    await wrapper.get('button').trigger('click')
    expect(routerPush).toHaveBeenCalledWith('/module/99')
  })

  it('shows error message when analytics load fails', async () => {
    analyticsApi.getDashboard.mockRejectedValue(new Error('API error'))

    const wrapper = mount(AnalyticsDashboard)
    await flushPromises()

    expect(wrapper.text()).toContain('Ошибка при загрузке анализа. Попробуйте позже.')
  })
})
