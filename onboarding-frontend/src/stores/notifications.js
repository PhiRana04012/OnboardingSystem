import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'
import { notificationsApi } from '../api/services'
import { getHubBaseUrl } from '../api/hub'

export const useNotificationsStore = defineStore('notifications', () => {
  const items = ref([])
  const unreadCount = ref(0)
  const isOpen = ref(false)
  const isConnected = ref(false)
  let connection = null

  async function load() {
    const token = localStorage.getItem('authToken')
    if (!token) return

    try {
      const [listRes, countRes] = await Promise.all([
        notificationsApi.getRecent({ limit: 30 }),
        notificationsApi.getUnreadCount()
      ])
      items.value = listRes.data || []
      unreadCount.value = countRes.data?.count ?? 0
    } catch (error) {
      console.error('Failed to load notifications:', error)
    }
  }

  async function connect() {
    const token = localStorage.getItem('authToken')
    if (!token || connection) return

    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${getHubBaseUrl()}/hubs/notifications`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build()

    connection.on('ReceiveNotification', (notification, count) => {
      const exists = items.value.some(n => n.notificationId === notification.notificationId)
      if (!exists) {
        items.value.unshift(notification)
        if (items.value.length > 50) items.value.pop()
      }
      unreadCount.value = count
    })

    connection.on('UnreadCountUpdated', (count) => {
      unreadCount.value = count
      items.value.forEach(n => { n.isRead = true })
    })

    try {
      await connection.start()
      isConnected.value = true
      await load()
    } catch (error) {
      console.error('SignalR connection failed:', error)
      connection = null
      isConnected.value = false
      await load()
    }
  }

  async function disconnect() {
    if (connection) {
      try {
        await connection.stop()
      } catch (_) { /* ignore */ }
      connection = null
    }
    isConnected.value = false
    items.value = []
    unreadCount.value = 0
    isOpen.value = false
  }

  async function markAsRead(notificationId) {
    try {
      await notificationsApi.markAsRead(notificationId)
      const item = items.value.find(n => n.notificationId === notificationId)
      if (item && !item.isRead) {
        item.isRead = true
        unreadCount.value = Math.max(0, unreadCount.value - 1)
      }
    } catch (error) {
      console.error('Failed to mark notification as read:', error)
    }
  }

  async function markAllAsRead() {
    try {
      await notificationsApi.markAllAsRead()
      items.value.forEach(n => { n.isRead = true })
      unreadCount.value = 0
    } catch (error) {
      console.error('Failed to mark all as read:', error)
    }
  }

  function togglePanel() {
    isOpen.value = !isOpen.value
  }

  function closePanel() {
    isOpen.value = false
  }

  return {
    items,
    unreadCount,
    isOpen,
    isConnected,
    load,
    connect,
    disconnect,
    markAsRead,
    markAllAsRead,
    togglePanel,
    closePanel
  }
})
