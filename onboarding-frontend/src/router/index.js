import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    component: () => import('../layouts/MainLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'Dashboard',
        component: () => import('../views/Dashboard.vue')
      },
      {
        path: 'module/:id',
        name: 'ModuleView',
        component: () => import('../views/ModuleView.vue')
      },
      {
        path: 'module/:id/test',
        name: 'TestView',
        component: () => import('../views/TestView.vue')
      },
      {
        path: 'test-result/:attemptId',
        name: 'TestResult',
        component: () => import('../views/TestResult.vue')
      },
      {
        path: 'reports',
        name: 'Reports',
        component: () => import('../views/Reports.vue'),
        meta: { roles: ['HR-специалист', 'Руководитель подразделения', 'Наставник', 'Администратор системы'] }
      },
      {
        path: 'admin',
        name: 'Admin',
        component: () => import('../views/Admin.vue'),
        meta: { roles: ['Администратор системы', 'HR-специалист'] }
      },
      {
        path: 'profile',
        name: 'Profile',
        component: () => import('../views/Profile.vue')
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  if (to.meta.requiresAuth === false) {
    next()
    return
  }

  if (!authStore.isAuthenticated) {
    next({ name: 'Login' })
    return
  }

  // Check role-based access
  if (to.meta.roles) {
    const hasAccess = to.meta.roles.some(role => authStore.hasRole(role))
    if (!hasAccess) {
      next({ name: 'Dashboard' })
      return
    }
  }

  next()
})

export default router








