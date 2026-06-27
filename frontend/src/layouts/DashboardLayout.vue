<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useRouter, RouterLink, RouterView } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

const navLinks = [
  { name: 'Dashboard', path: '/dashboard', icon: '🏠' },
  { name: 'Roster', path: '/roster', icon: '👥' },
  { name: 'Events', path: '/events', icon: '📅' },
  { name: 'Schedules', path: '/schedules', icon: '📋' },
  { name: 'Venues', path: '/venues', icon: '📍' },
  { name: 'Practice Planner', path: '/practice-planner', icon: '🏒' },
  { name: 'League', path: '/league', icon: '🏆' },
]

async function handleLogout() {
  authStore.logout()
  await router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-gray-50 flex">
    <!-- Desktop Sidebar -->
    <aside class="hidden md:flex md:flex-col md:w-64 md:fixed md:inset-y-0 bg-slate-900 text-white">
      <!-- Logo / Brand -->
      <div class="flex items-center h-16 px-6 border-b border-slate-700">
        <span class="text-xl font-bold text-white">⚡ PlayLeague</span>
      </div>

      <!-- Navigation Links -->
      <nav class="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        <RouterLink
          v-for="link in navLinks"
          :key="link.path"
          :to="link.path"
          class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors"
          :class="[
            $route.path === link.path
              ? 'bg-slate-700 text-white'
              : 'text-slate-300 hover:bg-slate-800 hover:text-white',
          ]"
        >
          <span class="text-base leading-none">{{ link.icon }}</span>
          {{ link.name }}
        </RouterLink>
      </nav>

      <!-- User info + logout -->
      <div class="border-t border-slate-700 px-4 py-4">
        <div class="flex items-center gap-3 mb-3">
          <div class="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center text-sm font-bold">
            {{ authStore.user?.name?.charAt(0)?.toUpperCase() ?? '?' }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-white truncate">{{ authStore.user?.name ?? 'User' }}</p>
            <p class="text-xs text-slate-400 truncate">{{ authStore.user?.email }}</p>
          </div>
        </div>
        <button
          @click="handleLogout"
          class="w-full text-left text-sm text-slate-400 hover:text-white transition-colors px-2 py-1 rounded hover:bg-slate-800"
        >
          🚪 Sign out
        </button>
      </div>
    </aside>

    <!-- Main content area -->
    <div class="flex-1 md:ml-64 flex flex-col min-h-screen">
      <!-- Mobile top header -->
      <header class="md:hidden flex items-center justify-between h-14 px-4 bg-white border-b border-gray-200 sticky top-0 z-10">
        <span class="text-lg font-bold text-slate-900">⚡ PlayLeague</span>
        <div class="flex items-center gap-2">
          <div class="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center text-sm font-bold text-white">
            {{ authStore.user?.name?.charAt(0)?.toUpperCase() ?? '?' }}
          </div>
        </div>
      </header>

      <!-- Desktop top bar -->
      <header class="hidden md:flex items-center justify-between h-16 px-6 bg-white border-b border-gray-200 sticky top-0 z-10">
        <h1 class="text-lg font-semibold text-gray-900">{{ $route.name ? String($route.name).charAt(0).toUpperCase() + String($route.name).slice(1).replace(/-/g, ' ') : '' }}</h1>
        <div class="flex items-center gap-3">
          <span class="text-sm text-gray-600">{{ authStore.user?.name }}</span>
          <button
            @click="handleLogout"
            class="text-sm text-gray-500 hover:text-gray-900 border border-gray-200 rounded-md px-3 py-1.5 hover:bg-gray-50 transition-colors"
          >
            Sign out
          </button>
        </div>
      </header>

      <!-- Page content -->
      <main class="flex-1 p-4 md:p-6 pb-20 md:pb-6">
        <RouterView />
      </main>
    </div>

    <!-- Mobile Bottom Navigation -->
    <nav class="md:hidden fixed bottom-0 inset-x-0 bg-white border-t border-gray-200 z-20">
      <div class="grid grid-cols-5 h-16">
        <RouterLink
          v-for="link in navLinks.slice(0, 5)"
          :key="link.path"
          :to="link.path"
          class="flex flex-col items-center justify-center gap-0.5 text-xs font-medium transition-colors"
          :class="[
            $route.path === link.path
              ? 'text-blue-600'
              : 'text-gray-500 hover:text-gray-900',
          ]"
        >
          <span class="text-lg leading-none">{{ link.icon }}</span>
          <span class="leading-none">{{ link.name.split(' ')[0] }}</span>
        </RouterLink>
      </div>
    </nav>
  </div>
</template>
