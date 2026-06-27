<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useTeamsStore } from '@/stores/teams'
import { useEventsStore } from '@/stores/events'

const authStore = useAuthStore()
const teamsStore = useTeamsStore()
const eventsStore = useEventsStore()

const upcomingEvents = computed(() => {
  const now = new Date()
  return eventsStore.events
    .filter((e) => new Date(e.date) >= now)
    .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime())
})

const nextEvent = computed(() => upcomingEvents.value[0] ?? null)

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

onMounted(async () => {
  await teamsStore.fetchTeams()
  if (teamsStore.teams.length > 0) {
    const teamId = teamsStore.teams[0].id
    await eventsStore.fetchEvents(teamId)
  }
})
</script>

<template>
  <div class="max-w-5xl mx-auto">
    <!-- Welcome heading -->
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-gray-900">
        Welcome back, {{ authStore.user?.name ?? 'Coach' }} 👋
      </h1>
      <p class="mt-1 text-sm text-gray-500">
        Here's what's going on with your team.
      </p>
    </div>

    <!-- Stat Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
      <!-- Upcoming Events -->
      <div class="bg-white rounded-xl border border-gray-200 shadow-sm p-5">
        <div class="flex items-center justify-between mb-3">
          <p class="text-sm font-medium text-gray-500">Upcoming Events</p>
          <span class="text-2xl">📅</span>
        </div>
        <p class="text-3xl font-bold text-gray-900">{{ upcomingEvents.length }}</p>
        <p class="mt-1 text-xs text-gray-400">in the next 30 days</p>
      </div>

      <!-- Team Count -->
      <div class="bg-white rounded-xl border border-gray-200 shadow-sm p-5">
        <div class="flex items-center justify-between mb-3">
          <p class="text-sm font-medium text-gray-500">Teams</p>
          <span class="text-2xl">🏒</span>
        </div>
        <p class="text-3xl font-bold text-gray-900">{{ teamsStore.teams.length }}</p>
        <p class="mt-1 text-xs text-gray-400">active teams</p>
      </div>

      <!-- Next Event -->
      <div class="bg-white rounded-xl border border-gray-200 shadow-sm p-5">
        <div class="flex items-center justify-between mb-3">
          <p class="text-sm font-medium text-gray-500">Next Event</p>
          <span class="text-2xl">⏰</span>
        </div>
        <p v-if="nextEvent" class="text-sm font-semibold text-gray-900 leading-snug">
          {{ nextEvent.title }}
        </p>
        <p v-else class="text-sm text-gray-400">No upcoming events</p>
        <p v-if="nextEvent" class="mt-1 text-xs text-gray-400">
          {{ formatDate(nextEvent.date) }}
        </p>
      </div>
    </div>

    <!-- Recent Events List -->
    <div class="bg-white rounded-xl border border-gray-200 shadow-sm">
      <div class="px-5 py-4 border-b border-gray-100 flex items-center justify-between">
        <h2 class="text-sm font-semibold text-gray-900">Upcoming Events</h2>
        <RouterLink to="/events" class="text-xs text-blue-600 hover:text-blue-500 font-medium">
          View all →
        </RouterLink>
      </div>

      <div v-if="upcomingEvents.length === 0" class="px-5 py-8 text-center text-sm text-gray-400">
        No upcoming events. <RouterLink to="/events" class="text-blue-600">Create one</RouterLink>
      </div>

      <ul v-else class="divide-y divide-gray-100">
        <li
          v-for="event in upcomingEvents.slice(0, 5)"
          :key="event.id"
          class="px-5 py-3.5 flex items-center gap-4 hover:bg-gray-50 transition-colors"
        >
          <span
            class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
            :class="event.type === 'GAME' ? 'bg-blue-100 text-blue-700' : 'bg-purple-100 text-purple-700'"
          >
            {{ event.type }}
          </span>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 truncate">{{ event.title }}</p>
            <p v-if="event.location" class="text-xs text-gray-400 truncate">📍 {{ event.location }}</p>
          </div>
          <p class="text-xs text-gray-500 whitespace-nowrap">{{ formatDate(event.date) }}</p>
        </li>
      </ul>
    </div>
  </div>
</template>
