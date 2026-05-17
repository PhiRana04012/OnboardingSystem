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
    path: '/auth/set-password',
    name: 'SetPassword',
    component: () => import('../views/SetPassword.vue'),
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
        meta: { requiresReportsAccess: true }
      },
      {
        path: 'admin',
        name: 'Admin',
        component: () => import('../views/Admin.vue'),
        meta: { requiresDepartmentManagement: true }
      },
      {
        path: 'profile',
        name: 'Profile',
        component: () => import('../views/Profile.vue')
      },
      {
        path: 'leaderboard',
        name: 'Leaderboard',
        component: () => import('../views/Leaderboard.vue')
      },
      {
        path: 'mentor',
        name: 'MentorDashboard',
        component: () => import('../views/MentorDashboard.vue'),
        meta: { requiresMentor: true }
      },
      {
        path: 'faq',
        name: 'FAQ',
        component: () => import('../views/FaqView.vue')
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

  if (to.meta.requiresReportsAccess && !authStore.canViewDepartmentReports) {
    next({ name: 'Dashboard' })
    return
  }

  if (to.meta.requiresDepartmentManagement && !authStore.canManageDepartment) {
    next({ name: 'Dashboard' })
    return
  }

  // Check mentor specific access
  if (to.meta.requiresMentor && !authStore.isMentor && !authStore.canManageMentees) {
    next({ name: 'Dashboard' })
    return
  }

  next()
})

export default router








