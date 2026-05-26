import { describe, it, expect } from 'vitest'

/**
 * Пример: Unit тесты для валидации функций
 * 
 * Эти тесты проверяют:
 * - Валидацию email адресов
 * - Валидацию паролей
 * - Форматирование данных
 */

// Вспомогательные функции для тестирования
export const validateEmail = (email) => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailRegex.test(email)
}

export const validatePassword = (password) => {
  // Минимум 8 символов, буква, цифра, спецсимвол
  return password.length >= 8 && 
         /[a-zA-Z]/.test(password) && 
         /[0-9]/.test(password) &&
         /[!@#$%^&*]/.test(password)
}

export const formatDate = (date) => {
  return new Date(date).toLocaleDateString('ru-RU')
}

// ==== ТЕСТЫ ====

describe('Email Validation', () => {
  it('должен принимать валидный email', () => {
    expect(validateEmail('user@example.com')).toBe(true)
  })

  it('должен отклонять email без @', () => {
    expect(validateEmail('userexample.com')).toBe(false)
  })

  it('должен отклонять email без домена', () => {
    expect(validateEmail('user@')).toBe(false)
  })

  it('должен отклонять пустую строку', () => {
    expect(validateEmail('')).toBe(false)
  })
})

describe('Password Validation', () => {
  it('должен принимать валидный пароль (8+ символов, буквы, цифры, спецсимволы)', () => {
    expect(validatePassword('SecurePass123!')).toBe(true)
  })

  it('должен отклонять короткий пароль', () => {
    expect(validatePassword('Pass1!')).toBe(false)
  })

  it('должен отклонять пароль без цифр', () => {
    expect(validatePassword('SecurePass!')).toBe(false)
  })

  it('должен отклонять пароль без спецсимволов', () => {
    expect(validatePassword('SecurePass123')).toBe(false)
  })

  it('должен отклонять пароль без букв', () => {
    expect(validatePassword('123456789!')).toBe(false)
  })
})

describe('Date Formatting', () => {
  it('должен форматировать дату в русский формат', () => {
    const date = new Date('2026-05-22')
    const result = formatDate(date)
    expect(result).toMatch(/\d{1,2}\.\d{1,2}\.\d{4}/)
  })

  it('должен обрабатывать строковые даты', () => {
    expect(formatDate('2026-05-22')).toBeDefined()
  })
})
