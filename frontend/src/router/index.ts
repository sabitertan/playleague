import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/pages/LoginPage.vue'),
      meta: { public: true },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/pages/RegisterPage.vue'),
      meta: { public: true },
    },
    {
      path: '/',
      component: () => import('@/layouts/DashboardLayout.vue'),
      children: [
        {
          path: '',
          redirect: '/dashboard',
        },
        {
          path: 'dashboard',
          name: 'dashboard',
          component: () => import('@/pages/DashboardPage.vue'),
        },
        {
          path: 'roster',
          name: 'roster',
          component: () => import('@/pages/RosterPage.vue'),
        },
        {
          path: 'events',
          name: 'events',
          component: () => import('@/pages/EventsPage.vue'),
        },
        {
          path: 'schedules',
          name: 'schedules',
          component: () => import('@/pages/SchedulesPage.vue'),
        },
        {
          path: 'venues',
          name: 'venues',
          component: () => import('@/pages/VenuesPage.vue'),
        },
        {
          path: 'practice-planner',
          name: 'practice-planner',
          component: () => import('@/pages/PracticePlannerPage.vue'),
        },
        {
          path: 'league',
          name: 'league',
          component: () => import('@/pages/LeaguePage.vue'),
        },
      ],
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (!to.meta.public && !auth.isAuthenticated) {
    return { name: 'login' }
  }
})

export default router
