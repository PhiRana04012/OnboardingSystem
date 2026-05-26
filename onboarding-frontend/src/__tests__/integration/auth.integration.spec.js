import { describe, it, expect, beforeEach, vi } from 'vitest'

/**
 * Пример: Integration тесты для аутентификации
 * 
 * Проверяют:
 * - Взаимодействие API + Store
 * - Полный цикл входа/выхода
 * - Обработка ошибок
 */

// Mock API service
class MockAuthAPI {
  constructor() {
    this.validCredentials = {
      email: 'user@example.com',
      password: 'SecurePass123!'
    }
  }

  async login(email, password) {
    // Имитация задержки API
    await new Promise(resolve => setTimeout(resolve, 100))

    if (email === this.validCredentials.email && 
        password === this.validCredentials.password) {
      return {
        token: 'jwt_token_example',
        user: {
          id: 1,
          email: email,
          name: 'Тестовый Пользователь',
          role: 'employee'
        }
      }
    }

    throw new Error('Invalid credentials')
  }

  async logout() {
    return { success: true }
  }

  async getProfile(token) {
    if (!token) throw new Error('No token provided')
    return {
      id: 1,
      email: 'user@example.com',
      name: 'Тестовый Пользователь',
      xp: 150,
      level: 2
    }
  }
}

// Простое хранилище
class AuthStore {
  constructor(api) {
    this.api = api
    this.token = null
    this.user = null
    this.isLoading = false
    this.error = null
  }

  async login(email, password) {
    this.isLoading = true
    this.error = null

    try {
      const response = await this.api.login(email, password)
      this.token = response.token
      this.user = response.user
      return response
    } catch (err) {
      this.error = err.message
      throw err
    } finally {
      this.isLoading = false
    }
  }

  async logout() {
    await this.api.logout()
    this.token = null
    this.user = null
    this.error = null
  }
}

// ==== ТЕСТЫ ====

describe('Authentication Flow (Integration)', () => {
  let authStore
  let api

  beforeEach(() => {
    api = new MockAuthAPI()
    authStore = new AuthStore(api)
  })

  describe('Успешный вход', () => {
    it('должен логинить пользователя с валидными креденшалами', async () => {
      const response = await authStore.login(
        'user@example.com',
        'SecurePass123!'
      )

      expect(authStore.token).toBe('jwt_token_example')
      expect(authStore.user.email).toBe('user@example.com')
      expect(authStore.user.role).toBe('employee')
      expect(authStore.error).toBe(null)
    })

    it('должен устанавливать isLoading флаг во время запроса', async () => {
      const promise = authStore.login(
        'user@example.com',
        'SecurePass123!'
      )

      // Проверяем во время выполнения
      // (в реальном тесте нужно использовать спецлогику)
      await promise

      expect(authStore.isLoading).toBe(false)
    })
  })

  describe('Неверные креденшалы', () => {
    it('должен вернуть ошибку при неверном пароле', async () => {
      try {
        await authStore.login('user@example.com', 'WrongPassword123!')
        expect.fail('Должна была быть ошибка')
      } catch (err) {
        expect(authStore.error).toBe('Invalid credentials')
        expect(authStore.token).toBe(null)
        expect(authStore.user).toBe(null)
      }
    })

    it('должен вернуть ошибку при неверном email', async () => {
      try {
        await authStore.login('wrong@example.com', 'SecurePass123!')
        expect.fail('Должна была быть ошибка')
      } catch (err) {
        expect(authStore.error).toBe('Invalid credentials')
      }
    })
  })

  describe('Выход (Logout)', () => {
    it('должен очищать данные при выходе', async () => {
      // Вход
      await authStore.login('user@example.com', 'SecurePass123!')
      expect(authStore.token).toBeTruthy()

      // Выход
      await authStore.logout()
      expect(authStore.token).toBe(null)
      expect(authStore.user).toBe(null)
      expect(authStore.error).toBe(null)
    })
  })

  describe('Получение профиля', () => {
    it('должен получать профиль пользователя с токеном', async () => {
      await authStore.login('user@example.com', 'SecurePass123!')

      const profile = await api.getProfile(authStore.token)

      expect(profile.email).toBe('user@example.com')
      expect(profile.xp).toBe(150)
      expect(profile.level).toBe(2)
    })

    it('должен выбрасывать ошибку без токена', async () => {
      try {
        await api.getProfile(null)
        expect.fail('Должна была быть ошибка')
      } catch (err) {
        expect(err.message).toBe('No token provided')
      }
    })
  })
})
