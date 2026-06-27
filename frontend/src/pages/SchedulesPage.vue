<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'
import { useTeamsStore } from '@/stores/teams'

export type ScheduleStatus = 'DRAFT' | 'PUBLISHED' | 'ARCHIVED'

interface Schedule {
  id: string
  name: string
  season: string
  status: ScheduleStatus
  teamId: string
  gameCount?: number
  createdAt: string
}

interface CreateScheduleData {
  name: string
  season: string
}

const teamsStore = useTeamsStore()

const schedules = ref<Schedule[]>([])
const loading = ref(false)
const showCreateForm = ref(false)
const createLoading = ref(false)
const createError = ref('')
const isAdmin = ref(true)

const newSchedule = ref<CreateScheduleData>({ name: '', season: '' })

function statusBadgeClass(status: ScheduleStatus) {
  const map: Record<ScheduleStatus, string> = {
    DRAFT: 'bg-yellow-100 text-yellow-700',
    PUBLISHED: 'bg-green-100 text-green-700',
    ARCHIVED: 'bg-gray-100 text-gray-500',
  }
  return map[status]
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

async function fetchSchedules() {
  if (!teamsStore.currentTeam) return
  loading.value = true
  try {
    const { data } = await api.get<Schedule[]>(`/schedules/team/${teamsStore.currentTeam.id}`)
    schedules.value = data
  } catch {
    // Ignore — backend may not exist yet
  } finally {
    loading.value = false
  }
}

async function handleCreateSchedule() {
  if (!teamsStore.currentTeam) return
  createError.value = ''
  createLoading.value = true
  try {
    const { data } = await api.post<Schedule>('/schedules', {
      ...newSchedule.value,
      teamId: teamsStore.currentTeam.id,
    })
    schedules.value.push(data)
    newSchedule.value = { name: '', season: '' }
    showCreateForm.value = false
  } catch (err: any) {
    createError.value = err?.response?.data?.message ?? 'Failed to create schedule.'
  } finally {
    createLoading.value = false
  }
}

async function handlePublish(id: string) {
  try {
    const { data } = await api.put<Schedule>(`/schedules/${id}`, { status: 'PUBLISHED' })
    const idx = schedules.value.findIndex((s) => s.id === id)
    if (idx !== -1) schedules.value[idx] = data
  } catch { /* ignore */ }
}

async function handleArchive(id: string) {
  if (!confirm('Archive this schedule?')) return
  try {
    const { data } = await api.put<Schedule>(`/schedules/${id}`, { status: 'ARCHIVED' })
    const idx = schedules.value.findIndex((s) => s.id === id)
    if (idx !== -1) schedules.value[idx] = data
  } catch { /* ignore */ }
}

onMounted(async () => {
  await teamsStore.fetchTeams()
  if (teamsStore.teams.length > 0) {
    teamsStore.currentTeam = teamsStore.teams[0]
    await fetchSchedules()
  }
})
</script>

<template>
  <div class="max-w-4xl mx-auto">
    <!-- Header -->
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Schedules</h1>
        <p class="mt-0.5 text-sm text-gray-500">Manage game schedules by season</p>
      </div>
      <button
        v-if="isAdmin"
        @click="showCreateForm = !showCreateForm"
        class="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
      >
        <span>＋</span> New Schedule
      </button>
    </div>

    <!-- Create Form -->
    <div v-if="showCreateForm" class="mb-6 bg-blue-50 border border-blue-200 rounded-xl p-5">
      <h2 class="text-sm font-semibold text-blue-900 mb-4">New Schedule</h2>

      <div v-if="createError" class="mb-3 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
        {{ createError }}
      </div>

      <form @submit.prevent="handleCreateSchedule" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Schedule Name *</label>
          <input
            v-model="newSchedule.name"
            type="text"
            required
            placeholder="e.g. Fall 2026 Schedule"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Season *</label>
          <input
            v-model="newSchedule.season"
            type="text"
            required
            placeholder="e.g. Fall 2026"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div class="sm:col-span-2 flex gap-3 pt-1">
          <button
            type="submit"
            :disabled="createLoading"
            class="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
          >
            {{ createLoading ? 'Creating…' : 'Create Schedule' }}
          </button>
          <button
            type="button"
            @click="showCreateForm = false"
            class="text-sm text-gray-600 hover:text-gray-900 px-4 py-2 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>

    <!-- Schedules List -->
    <div class="space-y-3">
      <div v-if="loading" class="bg-white rounded-xl border border-gray-200 p-8 text-center text-sm text-gray-400">
        Loading schedules…
      </div>

      <div v-else-if="schedules.length === 0" class="bg-white rounded-xl border border-gray-200 p-8 text-center text-sm text-gray-400">
        No schedules yet.
        <span v-if="isAdmin"> Create one to get started.</span>
      </div>

      <div
        v-for="schedule in schedules"
        :key="schedule.id"
        class="bg-white rounded-xl border border-gray-200 shadow-sm p-4 hover:shadow-md transition-shadow"
      >
        <div class="flex items-center gap-3">
          <span
            class="inline-flex items-center px-2 py-0.5 rounded text-xs font-semibold flex-shrink-0"
            :class="statusBadgeClass(schedule.status)"
          >
            {{ schedule.status }}
          </span>
          <div class="flex-1 min-w-0">
            <h3 class="text-sm font-semibold text-gray-900">{{ schedule.name }}</h3>
            <p class="text-xs text-gray-500">
              Season: {{ schedule.season }}
              <span v-if="schedule.gameCount !== undefined"> · {{ schedule.gameCount }} games</span>
              · Created {{ formatDate(schedule.createdAt) }}
            </p>
          </div>
          <div v-if="isAdmin" class="flex items-center gap-2 flex-shrink-0">
            <button
              v-if="schedule.status === 'DRAFT'"
              @click="handlePublish(schedule.id)"
              class="text-xs font-medium text-green-600 hover:text-green-800 border border-green-200 px-2.5 py-1 rounded-full hover:bg-green-50 transition-colors"
            >
              Publish
            </button>
            <button
              v-if="schedule.status !== 'ARCHIVED'"
              @click="handleArchive(schedule.id)"
              class="text-xs font-medium text-gray-500 hover:text-gray-700 border border-gray-200 px-2.5 py-1 rounded-full hover:bg-gray-50 transition-colors"
            >
              Archive
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
