import { describe, it, expect, beforeEach, vi } from 'vitest'
import { nextTick, reactive } from 'vue'
import { mount } from '@vue/test-utils'

const routerPush = vi.fn()
let notificationsStore

vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: routerPush
  })
}))

vi.mock('../../../stores/notifications', () => ({
  useNotificationsStore: () => notificationsStore
}))

import NotificationBell from '../../../components/NotificationBell.vue'

describe('NotificationBell', () => {
  const createMockStore = () => {
    const store = reactive({
      unreadCount: 5,
      isOpen: false,
      items: [
        {
          notificationId: '1',
          title: 'Test item',
          message: 'Notification message',
          type: 'achievement',
          createdAt: new Date(Date.now() - 5 * 60000).toISOString(),
          isRead: false,
          linkUrl: '/target'
        }
      ],
      togglePanel: () => {
        store.isOpen = !store.isOpen
      },
      closePanel: () => {
        store.isOpen = false
      },
      markAsRead: vi.fn(async (id) => {
        const item = store.items.find((item) => item.notificationId === id)
        if (item && !item.isRead) {
          item.isRead = true
          store.unreadCount = Math.max(0, store.unreadCount - 1)
        }
      }),
      markAllAsRead: vi.fn(async () => {
        store.items.forEach((item) => {
          item.isRead = true
        })
        store.unreadCount = 0
      })
    })

    return store
  }

  beforeEach(() => {
    routerPush.mockClear()
    notificationsStore = createMockStore()
  })

  it('renders unread badge and opens panel on toggle', async () => {
    const wrapper = mount(NotificationBell, { attachTo: document.body })

    expect(wrapper.find('span').text()).toBe('5')

    await wrapper.get('button').trigger('click')
    await nextTick()

    expect(notificationsStore.isOpen).toBe(true)
    expect(wrapper.html()).toContain('Уведомления')
    expect(wrapper.html()).toContain('Test item')
  })

  it('displays formatted time and notification icon', async () => {
    const wrapper = mount(NotificationBell, { attachTo: document.body })

    await wrapper.get('button').trigger('click')
    await nextTick()

    expect(wrapper.html()).toContain('🏅')
    expect(wrapper.html()).toContain('5 мин. назад')
  })

  it('calls markAllAsRead when the read-all button is clicked', async () => {
    const wrapper = mount(NotificationBell, { attachTo: document.body })

    await wrapper.get('button').trigger('click')
    await nextTick()

    const buttons = wrapper.findAll('button')
    const markAllButton = buttons[1]

    await markAllButton.trigger('click')
    await nextTick()

    expect(notificationsStore.markAllAsRead).toHaveBeenCalled()
    expect(notificationsStore.unreadCount).toBe(0)
  })

  it('marks a notification as read and navigates when an item is clicked', async () => {
    const wrapper = mount(NotificationBell, { attachTo: document.body })

    await wrapper.get('button').trigger('click')
    await nextTick()

    const buttons = wrapper.findAll('button')
    const itemButton = buttons[2]

    await itemButton.trigger('click')
    await nextTick()

    expect(notificationsStore.markAsRead).toHaveBeenCalledWith('1')
    expect(routerPush).toHaveBeenCalledWith('/target')
    expect(notificationsStore.isOpen).toBe(false)
  })

  it('closes the panel when clicking outside', async () => {
    const wrapper = mount(NotificationBell, { attachTo: document.body })

    await wrapper.get('button').trigger('click')
    await nextTick()
    expect(notificationsStore.isOpen).toBe(true)

    await document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }))
    await nextTick()

    expect(notificationsStore.isOpen).toBe(false)
  })
})
