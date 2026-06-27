<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { api } from '@/api/client'
import { useTeamsStore } from '@/stores/teams'

interface Play {
  id: string
  name: string
  description?: string | null
  category?: string | null
  duration?: number | null
}

interface PracticeSession {
  id: string
  title: string
  date: string
  duration: number
  teamId: string
  plays?: Play[]
  playCount?: number
}

const teamsStore = useTeamsStore()

const sessions = ref<PracticeSession[]>([])
const plays = ref<Play[]>([])
const selectedSession = ref<PracticeSession | null>(null)
const loadingSessions = ref(false)
const loadingPlays = ref(false)

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function formatDuration(minutes: number) {
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  if (h === 0) return `${m}m`
  return m === 0 ? `${h}h` : `${h}h ${m}m`
}

async function fetchSessions() {
  if (!teamsStore.currentTeam) return
  loadingSessions.value = true
  try {
    const { data } = await api.get<PracticeSession[]>(`/practice-sessions/team/${teamsStore.currentTeam.id}`)
    sessions.value = data
  } catch {
    // backend may not be ready
  } finally {
    loadingSessions.value = false
  }
}

async function fetchPlays() {
  loadingPlays.value = true
  try {
    const { data } = await api.get<Play[]>('/plays')
    plays.value = data
  } catch {
    // backend may not be ready
  } finally {
    loadingPlays.value = false
  }
}

function selectSession(session: PracticeSession) {
  selectedSession.value = selectedSession.value?.id === session.id ? null : session
}

const sessionPlays = computed(() => {
  if (!selectedSession.value) return []
  return selectedSession.value.plays ?? []
})

onMounted(async () => {
  await teamsStore.fetchTeams()
  if (teamsStore.teams.length > 0) {
    teamsStore.currentTeam = teamsStore.teams[0]
  }
  await Promise.all([fetchSessions(), fetchPlays()])
})
</script>

<template>
  <div class="max-w-6xl mx-auto">
    <!-- Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-gray-900">Practice Planner</h1>
      <p class="mt-0.5 text-sm text-gray-500">Plan sessions and manage your play library</p>
    </div>

    <!-- Two-panel layout -->
    <div class="grid grid-cols-1 lg:grid-cols-5 gap-4">
      <!-- Left Panel: Sessions List -->
      <div class="lg:col-span-3 bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        <div class="px-4 py-3.5 border-b border-gray-100 flex items-center justify-between">
          <h2 class="text-sm font-semibold text-gray-900">Practice Sessions</h2>
          <span class="text-xs text-gray-400">{{ sessions.length }} sessions</span>
        </div>

        <div v-if="loadingSessions" class="p-8 text-center text-sm text-gray-400">
          Loading sessions…
        </div>

        <div v-else-if="sessions.length === 0" class="p-8 text-center text-sm text-gray-400">
          No practice sessions yet.
        </div>

        <ul v-else class="divide-y divide-gray-100">
          <li
            v-for="session in sessions"
            :key="session.id"
            class="px-4 py-3.5 hover:bg-gray-50 cursor-pointer transition-colors"
            :class="selectedSession?.id === session.id ? 'bg-blue-50 border-l-2 border-l-blue-500' : ''"
            @click="selectSession(session)"
          >
            <div class="flex items-start justify-between gap-2">
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900">{{ session.title }}</p>
                <div class="mt-0.5 flex gap-3 text-xs text-gray-500">
                  <span>📅 {{ formatDate(session.date) }}</span>
                  <span>⏱ {{ formatDuration(session.duration) }}</span>
                </div>
              </div>
              <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-purple-100 text-purple-700 flex-shrink-0">
                {{ session.playCount ?? (session.plays?.length ?? 0) }} plays
              </span>
            </div>

            <!-- Expanded plays for selected session -->
            <div v-if="selectedSession?.id === session.id && sessionPlays.length > 0" class="mt-3 space-y-1.5">
              <div
                v-for="play in sessionPlays"
                :key="play.id"
                class="flex items-center gap-2 text-xs text-gray-600 bg-white rounded-lg border border-gray-100 px-2.5 py-1.5"
              >
                <span class="text-blue-500">▶</span>
                <span class="font-medium">{{ play.name }}</span>
                <span v-if="play.duration" class="ml-auto text-gray-400">{{ play.duration }}m</span>
              </div>
            </div>
          </li>
        </ul>
      </div>

      <!-- Right Panel: Play Library -->
      <div class="lg:col-span-2 bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        <div class="px-4 py-3.5 border-b border-gray-100 flex items-center justify-between">
          <h2 class="text-sm font-semibold text-gray-900">Play Library</h2>
          <span class="text-xs text-gray-400">{{ plays.length }} plays</span>
        </div>

        <div v-if="loadingPlays" class="p-8 text-center text-sm text-gray-400">
          Loading plays…
        </div>

        <div v-else-if="plays.length === 0" class="p-8 text-center text-sm text-gray-400">
          No plays in the library yet.
        </div>

        <ul v-else class="divide-y divide-gray-100 max-h-96 lg:max-h-[600px] overflow-y-auto">
          <li
            v-for="play in plays"
            :key="play.id"
            class="px-4 py-3 hover:bg-gray-50 transition-colors"
          >
            <div class="flex items-start gap-2">
              <span class="text-blue-500 text-sm mt-0.5">🏒</span>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900">{{ play.name }}</p>
                <p v-if="play.description" class="text-xs text-gray-500 line-clamp-2 mt-0.5">
                  {{ play.description }}
                </p>
                <div class="mt-1 flex gap-2">
                  <span v-if="play.category" class="text-xs bg-gray-100 text-gray-600 px-1.5 py-0.5 rounded">
                    {{ play.category }}
                  </span>
                  <span v-if="play.duration" class="text-xs text-gray-400">
                    ⏱ {{ play.duration }}m
                  </span>
                </div>
              </div>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>
