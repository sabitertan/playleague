<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'
import { useTeamsStore } from '@/stores/teams'

interface Play {
  id: string
  name: string
  description?: string | null
  isTemplate: boolean
  updatedAt: string
}

interface Session {
  id: string
  title: string
  date: string
  durationMinutes: number
  isShared: boolean
  playCount: number
}

const teamsStore = useTeamsStore()

const sessions = ref<Session[]>([])
const plays = ref<Play[]>([])
const selectedSession = ref<Session | null>(null)
const loadingSessions = ref(false)
const loadingPlays = ref(false)

// Session form
const showCreateSession = ref(false)
const createSessionLoading = ref(false)
const createSessionError = ref('')
const newSession = ref({ title: '', date: '', durationMinutes: 60 })

// Play form
const showCreatePlay = ref(false)
const createPlayLoading = ref(false)
const createPlayError = ref('')
const newPlay = ref({ name: '', description: '' })

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
    const { data } = await api.get<Session[]>(`/practice/sessions/team/${teamsStore.currentTeam.id}`)
    sessions.value = data
  } catch {
    // ignore
  } finally {
    loadingSessions.value = false
  }
}

async function fetchPlays() {
  if (!teamsStore.currentTeam) return
  loadingPlays.value = true
  try {
    const { data } = await api.get<Play[]>(`/practice/plays/team/${teamsStore.currentTeam.id}`)
    plays.value = data
  } catch {
    // ignore
  } finally {
    loadingPlays.value = false
  }
}

function selectSession(session: Session) {
  selectedSession.value = selectedSession.value?.id === session.id ? null : session
}

async function handleCreateSession() {
  if (!teamsStore.currentTeam) return
  createSessionError.value = ''
  createSessionLoading.value = true
  try {
    const isoDate = new Date(newSession.value.date).toISOString()
    const { data: id } = await api.post<string>('/practice/sessions', {
      teamId: teamsStore.currentTeam.id,
      title: newSession.value.title,
      date: isoDate,
      durationMinutes: Number(newSession.value.durationMinutes),
    })
    sessions.value.unshift({
      id,
      title: newSession.value.title,
      date: isoDate,
      durationMinutes: Number(newSession.value.durationMinutes),
      isShared: false,
      playCount: 0,
    })
    newSession.value = { title: '', date: '', durationMinutes: 60 }
    showCreateSession.value = false
  } catch (err: any) {
    createSessionError.value = err?.response?.data?.message ?? 'Failed to create session.'
  } finally {
    createSessionLoading.value = false
  }
}

async function handleDeleteSession(id: string) {
  if (!confirm('Delete this practice session?')) return
  try {
    await api.delete(`/practice/sessions/${id}`)
    sessions.value = sessions.value.filter((s) => s.id !== id)
    if (selectedSession.value?.id === id) selectedSession.value = null
  } catch { /* ignore */ }
}

async function handleCreatePlay() {
  if (!teamsStore.currentTeam) return
  createPlayError.value = ''
  createPlayLoading.value = true
  try {
    const { data: id } = await api.post<string>('/practice/plays', {
      teamId: teamsStore.currentTeam.id,
      name: newPlay.value.name,
      description: newPlay.value.description || null,
      isTemplate: false,
    })
    plays.value.push({
      id,
      name: newPlay.value.name,
      description: newPlay.value.description || null,
      isTemplate: false,
      updatedAt: new Date().toISOString(),
    })
    newPlay.value = { name: '', description: '' }
    showCreatePlay.value = false
  } catch (err: any) {
    createPlayError.value = err?.response?.data?.message ?? 'Failed to create play.'
  } finally {
    createPlayLoading.value = false
  }
}

async function handleDeletePlay(id: string) {
  if (!confirm('Delete this play?')) return
  try {
    await api.delete(`/practice/plays/${id}`)
    plays.value = plays.value.filter((p) => p.id !== id)
  } catch { /* ignore */ }
}

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
          <button
            @click="showCreateSession = !showCreateSession"
            class="text-xs font-semibold text-blue-600 hover:text-blue-800 border border-blue-200 px-2.5 py-1 rounded-full hover:bg-blue-50 transition-colors"
          >
            ＋ Add
          </button>
        </div>

        <!-- Create Session Form -->
        <div v-if="showCreateSession" class="bg-blue-50 border-b border-blue-100 px-4 py-4">
          <div v-if="createSessionError" class="mb-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-700">
            {{ createSessionError }}
          </div>
          <form @submit.prevent="handleCreateSession" class="space-y-2">
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Title *</label>
              <input v-model="newSession.title" type="text" required placeholder="e.g. Monday Power Skating"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div class="grid grid-cols-2 gap-2">
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Date & Time *</label>
                <input v-model="newSession.date" type="datetime-local" required
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Duration (min) *</label>
                <input v-model="newSession.durationMinutes" type="number" min="15" required
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>
            <div class="flex gap-2 pt-1">
              <button type="submit" :disabled="createSessionLoading"
                class="text-xs font-semibold bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white px-3 py-1.5 rounded-lg transition-colors"
              >
                {{ createSessionLoading ? 'Creating…' : 'Create' }}
              </button>
              <button type="button" @click="showCreateSession = false"
                class="text-xs text-gray-600 border border-gray-200 px-3 py-1.5 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>

        <div v-if="loadingSessions" class="p-8 text-center text-sm text-gray-400">Loading sessions…</div>

        <div v-else-if="sessions.length === 0" class="p-8 text-center text-sm text-gray-400">
          No practice sessions yet. Click "+ Add" to create one.
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
                  <span>⏱ {{ formatDuration(session.durationMinutes) }}</span>
                </div>
              </div>
              <div class="flex items-center gap-2 flex-shrink-0" @click.stop>
                <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-purple-100 text-purple-700">
                  {{ session.playCount }} plays
                </span>
                <button @click="handleDeleteSession(session.id)"
                  class="text-gray-300 hover:text-red-500 transition-colors text-sm"
                  title="Delete session"
                >
                  ✕
                </button>
              </div>
            </div>
          </li>
        </ul>
      </div>

      <!-- Right Panel: Play Library -->
      <div class="lg:col-span-2 bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        <div class="px-4 py-3.5 border-b border-gray-100 flex items-center justify-between">
          <h2 class="text-sm font-semibold text-gray-900">Play Library</h2>
          <button
            @click="showCreatePlay = !showCreatePlay"
            class="text-xs font-semibold text-blue-600 hover:text-blue-800 border border-blue-200 px-2.5 py-1 rounded-full hover:bg-blue-50 transition-colors"
          >
            ＋ Add
          </button>
        </div>

        <!-- Create Play Form -->
        <div v-if="showCreatePlay" class="bg-blue-50 border-b border-blue-100 px-4 py-4">
          <div v-if="createPlayError" class="mb-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-700">
            {{ createPlayError }}
          </div>
          <form @submit.prevent="handleCreatePlay" class="space-y-2">
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Play Name *</label>
              <input v-model="newPlay.name" type="text" required placeholder="e.g. 2-on-1 Breakout"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Description</label>
              <textarea v-model="newPlay.description" rows="2" placeholder="Optional description…"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
              />
            </div>
            <div class="flex gap-2 pt-1">
              <button type="submit" :disabled="createPlayLoading"
                class="text-xs font-semibold bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white px-3 py-1.5 rounded-lg transition-colors"
              >
                {{ createPlayLoading ? 'Creating…' : 'Create' }}
              </button>
              <button type="button" @click="showCreatePlay = false"
                class="text-xs text-gray-600 border border-gray-200 px-3 py-1.5 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>

        <div v-if="loadingPlays" class="p-8 text-center text-sm text-gray-400">Loading plays…</div>

        <div v-else-if="plays.length === 0" class="p-8 text-center text-sm text-gray-400">
          No plays yet. Click "+ Add" to create one.
        </div>

        <ul v-else class="divide-y divide-gray-100 max-h-96 lg:max-h-[600px] overflow-y-auto">
          <li v-for="play in plays" :key="play.id" class="px-4 py-3 hover:bg-gray-50 transition-colors">
            <div class="flex items-start gap-2">
              <span class="text-blue-500 text-sm mt-0.5 flex-shrink-0">🏒</span>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2">
                  <p class="text-sm font-medium text-gray-900">{{ play.name }}</p>
                  <span v-if="play.isTemplate" class="text-xs bg-gray-100 text-gray-500 px-1.5 py-0.5 rounded">template</span>
                </div>
                <p v-if="play.description" class="text-xs text-gray-500 line-clamp-2 mt-0.5">
                  {{ play.description }}
                </p>
              </div>
              <button v-if="!play.isTemplate" @click="handleDeletePlay(play.id)"
                class="text-gray-300 hover:text-red-500 transition-colors text-sm flex-shrink-0"
                title="Delete play"
              >
                ✕
              </button>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>
