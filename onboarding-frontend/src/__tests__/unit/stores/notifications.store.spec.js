import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'

vi.mock('../../../api/services', () => ({
  notificationsApi: {
    markAsRead: vi.fn(() => Promise.resolve()),
    markAllAsRead: vi.fn(() => Promise.resolve())
  }
}))

import { useNotificationsStore } from '../../../stores/notifications'
import { notificationsApi } from '../../../api/services'

describe('Notifications Store', () => {
  let store

  beforeEach(() => {
    setActivePinia(createPinia())
    store = useNotificationsStore()
    store.items = [
      {
        notificationId: '1',
        isRead: false
      },
      {
        notificationId: '2',
        isRead: false
      }
    ]
    store.unreadCount = 2
    vi.clearAllMocks()
  })

  it('toggles panel visibility', () => {
    expect(store.isOpen).toBe(false)
    store.togglePanel()
    expect(store.isOpen).toBe(true)
    store.togglePanel()
    expect(store.isOpen).toBe(false)
  })

  it('closes panel when requested', () => {
    store.isOpen = true
    store.closePanel()
    expect(store.isOpen).toBe(false)
  })

  it('marks a single notification as read and decreases unread count', async () => {
    await store.markAsRead('1')

    expect(notificationsApi.markAsRead).toHaveBeenCalledWith('1')
    expect(store.items.find((item) => item.notificationId === '1').isRead).toBe(true)
    expect(store.unreadCount).toBe(1)
  })

  it('marks all notifications as read and resets unread count', async () => {
    await store.markAllAsRead()

    expect(notificationsApi.markAllAsRead).toHaveBeenCalled()
    expect(store.items.every((item) => item.isRead)).toBe(true)
    expect(store.unreadCount).toBe(0)
  })
})
