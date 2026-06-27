<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'
import { useTeamsStore } from '@/stores/teams'

export type ScheduleStatus = 'DRAFT' | 'PUBLISHED' | 'ARCHIVED'

interface Schedule {
  id: string
  name: string
  seasonName?: string | null
  status: ScheduleStatus
  gameCount: number
  startDate?: string | null
  endDate?: string | null
}

interface ScheduleFormData {
  name: string
  seasonName: string
}

const teamsStore = useTeamsStore()

const schedules = ref<Schedule[]>([])
const loading = ref(false)
const showCreateForm = ref(false)
const createLoading = ref(false)
const createError = ref('')
const editingId = ref<string | null>(null)
const editError = ref('')
const editLoading = ref(false)

const newSchedule = ref<ScheduleFormData>({ name: '', seasonName: '' })
const editSchedule = ref<ScheduleFormData>({ name: '', seasonName: '' })

function statusBadgeClass(status: ScheduleStatus) {
  const map: Record<ScheduleStatus, string> = {
    DRAFT: 'bg-yellow-100 text-yellow-700',
    PUBLISHED: 'bg-green-100 text-green-700',
    ARCHIVED: 'bg-gray-100 text-gray-500',
  }
  return map[status]
}

async function fetchSchedules() {
  if (!teamsStore.currentTeam) return
  loading.value = true
  try {
    const { data } = await api.get<Schedule[]>(`/schedules?teamId=${teamsStore.currentTeam.id}`)
    schedules.value = data
  } catch {
    // ignore
  } finally {
    loading.value = false
  }
}

async function handleCreateSchedule() {
  if (!teamsStore.currentTeam) return
  createError.value = ''
  createLoading.value = true
  try {
    const { data: id } = await api.post<string>('/schedules', {
      name: newSchedule.value.name,
      seasonName: newSchedule.value.seasonName || null,
      teamId: teamsStore.currentTeam.id,
      roundRobin: false,
    })
    schedules.value.unshift({
      id,
      name: newSchedule.value.name,
      seasonName: newSchedule.value.seasonName || null,
      status: 'DRAFT',
      gameCount: 0,
    })
    newSchedule.value = { name: '', seasonName: '' }
    showCreateForm.value = false
  } catch (err: any) {
    createError.value = err?.response?.data?.message ?? 'Failed to create schedule.'
  } finally {
    createLoading.value = false
  }
}

function startEdit(schedule: Schedule) {
  editingId.value = schedule.id
  editSchedule.value = { name: schedule.name, seasonName: schedule.seasonName ?? '' }
  editError.value = ''
}

function cancelEdit() {
  editingId.value = null
  editError.value = ''
}

async function handleUpdateSchedule(id: string) {
  editError.value = ''
  editLoading.value = true
  try {
    await api.put(`/schedules/${id}`, {
      name: editSchedule.value.name,
      seasonName: editSchedule.value.seasonName || null,
      roundRobin: false,
    })
    const idx = schedules.value.findIndex((s) => s.id === id)
    if (idx !== -1) {
      schedules.value[idx].name = editSchedule.value.name
      schedules.value[idx].seasonName = editSchedule.value.seasonName || null
    }
    editingId.value = null
  } catch (err: any) {
    editError.value = err?.response?.data?.message ?? 'Failed to update schedule.'
  } finally {
    editLoading.value = false
  }
}

async function handlePublish(id: string) {
  try {
    await api.post(`/schedules/${id}/publish`)
    const idx = schedules.value.findIndex((s) => s.id === id)
    if (idx !== -1) schedules.value[idx].status = 'PUBLISHED'
  } catch { /* ignore */ }
}

async function handleDelete(id: string) {
  if (!confirm('Delete this schedule? This cannot be undone.')) return
  try {
    await api.delete(`/schedules/${id}`)
    schedules.value = schedules.value.filter((s) => s.id !== id)
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
          <label class="block text-xs font-medium text-gray-700 mb-1">Season Name</label>
          <input
            v-model="newSchedule.seasonName"
            type="text"
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
          <button type="button" @click="showCreateForm = false"
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
        No schedules yet. Create one to get started.
      </div>

      <template v-else>
        <div v-for="schedule in schedules" :key="schedule.id"
          class="bg-white rounded-xl border border-gray-200 shadow-sm p-4"
        >
          <!-- View mode -->
          <div v-if="editingId !== schedule.id" class="flex items-center gap-3">
            <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-semibold flex-shrink-0"
              :class="statusBadgeClass(schedule.status)"
            >
              {{ schedule.status }}
            </span>
            <div class="flex-1 min-w-0">
              <h3 class="text-sm font-semibold text-gray-900">{{ schedule.name }}</h3>
              <p class="text-xs text-gray-500">
                <span v-if="schedule.seasonName">{{ schedule.seasonName }} · </span>
                {{ schedule.gameCount }} game{{ schedule.gameCount !== 1 ? 's' : '' }}
              </p>
            </div>
            <div class="flex items-center gap-2 flex-shrink-0">
              <button
                v-if="schedule.status === 'DRAFT'"
                @click="handlePublish(schedule.id)"
                class="text-xs font-medium text-green-600 hover:text-green-800 border border-green-200 px-2.5 py-1 rounded-full hover:bg-green-50 transition-colors"
              >
                Publish
              </button>
              <button @click="startEdit(schedule)"
                class="text-xs font-medium text-gray-500 hover:text-gray-700 border border-gray-200 px-2.5 py-1 rounded-full hover:bg-gray-50 transition-colors"
              >
                Edit
              </button>
              <button @click="handleDelete(schedule.id)"
                class="text-xs font-medium text-red-400 hover:text-red-600 border border-red-100 px-2.5 py-1 rounded-full hover:bg-red-50 transition-colors"
              >
                Delete
              </button>
            </div>
          </div>

          <!-- Edit mode -->
          <div v-else>
            <div v-if="editError" class="mb-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-700">
              {{ editError }}
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 mb-2">
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Name *</label>
                <input v-model="editSchedule.name" type="text" required
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Season Name</label>
                <input v-model="editSchedule.seasonName" type="text"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>
            <div class="flex gap-2">
              <button @click="handleUpdateSchedule(schedule.id)" :disabled="editLoading"
                class="text-xs font-medium bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white px-3 py-1.5 rounded-lg transition-colors"
              >
                {{ editLoading ? 'Saving…' : 'Save' }}
              </button>
              <button @click="cancelEdit"
                class="text-xs font-medium text-gray-600 border border-gray-200 px-3 py-1.5 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>
