import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../../../stores/auth'

vi.mock('../../../api/services', () => ({
  usersApi: {
    login: vi.fn()
  }
}))

import { usersApi } from '../../../api/services'

describe('Auth Store', () => {
  beforeEach(() => {
    localStorage.clear()
    setActivePinia(createPinia())
    usersApi.login.mockClear()
  })

  it('computes access flags correctly from current user', () => {
    const store = useAuthStore()
    store.currentUser = {
      userId: 1,
      roles: ['HR-специалист'],
      jobTitle: { title: 'Начальник отдела' },
      hasMentees: true
    }

    expect(store.isAuthenticated).toBe(true)
    expect(store.isHR).toBe(true)
    expect(store.isDepartmentHead).toBe(true)
    expect(store.canManageMentees).toBe(true)
    expect(store.canViewDepartmentReports).toBe(true)
    expect(store.canManageDepartment).toBe(true)
  })

  it('login stores token and current user when credentials are valid', async () => {
    const store = useAuthStore()
    usersApi.login.mockResolvedValue({
      data: {
        success: true,
        token: 'valid-token',
        user: {
          userId: 2,
          roles: ['employee'],
          jobTitle: { title: 'Сотрудник' },
          hasMentees: false
        }
      }
    })

    const user = await store.login('test@example.com', 'Password1!')

    expect(user.userId).toBe(2)
    expect(store.isAuthenticated).toBe(true)
    expect(localStorage.getItem('authToken')).toBe('valid-token')
  })

  it('login throws a friendly error message when server returns an error response', async () => {
    const store = useAuthStore()
    usersApi.login.mockRejectedValue({
      response: {
        status: 401,
        data: { errorMessage: 'Неверные учетные данные' }
      }
    })

    await expect(store.login('user@example.com', 'wrong')).rejects.toThrow('Неверные учетные данные')
  })

  it('logout clears auth state and localStorage', () => {
    const store = useAuthStore()
    store.currentUser = { userId: 3 }
    localStorage.setItem('authToken', 'abc')

    store.logout()

    expect(store.currentUser).toBe(null)
    expect(localStorage.getItem('authToken')).toBe(null)
  })
})
