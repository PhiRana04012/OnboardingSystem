import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createAppRouter } from '../../router'
import { createMemoryHistory } from 'vue-router'

const authState = {
  isAuthenticated: false,
  canViewDepartmentReports: false,
  canManageDepartment: false,
  isMentor: false,
  canManageMentees: false
}

vi.mock('../../stores/auth', () => ({
  useAuthStore: () => authState
}))

describe('router guard', () => {
  let router

  beforeEach(() => {
    authState.isAuthenticated = false
    authState.canViewDepartmentReports = false
    authState.canManageDepartment = false
    authState.isMentor = false
    authState.canManageMentees = false

    router = createAppRouter(createMemoryHistory())
  })

  it('redirects unauthenticated user to login page', async () => {
    await router.push('/reports')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('Login')
  })

  it('redirects user without reports access to dashboard', async () => {
    authState.isAuthenticated = true
    authState.canViewDepartmentReports = false

    await router.push('/reports')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('Dashboard')
  })

  it('redirects non-mentor user away from mentor dashboard', async () => {
    authState.isAuthenticated = true
    authState.isMentor = false
    authState.canManageMentees = false

    await router.push('/mentor')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('Dashboard')
  })
})
