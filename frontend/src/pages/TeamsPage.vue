<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useTeamsStore, type CreateTeamData } from '@/stores/teams'
import { useCoachesStore } from '@/stores/coaches'
import { api } from '@/api/client'

const teamsStore = useTeamsStore()
const coachesStore = useCoachesStore()

interface TeamCoach {
  id: string
  name: string
  title?: string | null
}

const expandedCoachesId = ref<string | null>(null)
const teamCoaches = ref<Record<string, TeamCoach[]>>({})
const coachActionId = ref('')
const coachToAssign = ref('')

const loading = ref(false)
const showCreateForm = ref(false)
const createLoading = ref(false)
const createError = ref('')
const newTeam = ref<CreateTeamData>({ name: '', sport: 'HOCKEY', season: '' })

const editingId = ref<string | null>(null)
const editTeam = ref({ name: '', season: '' })
const editLoading = ref(false)
const editError = ref('')

async function fetchTeams() {
  loading.value = true
  try {
    await teamsStore.fetchTeams()
  } finally {
    loading.value = false
  }
}

async function handleCreateTeam() {
  createError.value = ''
  createLoading.value = true
  try {
    await teamsStore.createTeam(newTeam.value)
    newTeam.value = { name: '', sport: 'HOCKEY', season: '' }
    showCreateForm.value = false
  } catch (err: any) {
    createError.value = err?.response?.data?.message ?? 'Failed to create team.'
  } finally {
    createLoading.value = false
  }
}

function startEdit(team: typeof teamsStore.teams[0]) {
  editingId.value = team.id
  editTeam.value = { name: team.name, season: team.season ?? '' }
  editError.value = ''
}

function cancelEdit() {
  editingId.value = null
  editError.value = ''
}

async function handleUpdateTeam(id: string) {
  editError.value = ''
  editLoading.value = true
  try {
    await teamsStore.updateTeam(id, editTeam.value)
    editingId.value = null
  } catch (err: any) {
    editError.value = err?.response?.data?.message ?? 'Failed to update team.'
  } finally {
    editLoading.value = false
  }
}

async function handleDeleteTeam(id: string) {
  if (!confirm('Delete this team? This removes all its data and cannot be undone.')) return
  try {
    await teamsStore.deleteTeam(id)
  } catch { /* ignore */ }
}

// --- Coach assignment ---

async function toggleCoaches(teamId: string) {
  if (expandedCoachesId.value === teamId) {
    expandedCoachesId.value = null
    return
  }
  expandedCoachesId.value = teamId
  coachToAssign.value = ''
  await Promise.all([
    loadTeamCoaches(teamId),
    coachesStore.coaches.length === 0 ? coachesStore.fetchMyCoaches() : Promise.resolve(),
  ])
}

async function loadTeamCoaches(teamId: string) {
  try {
    const { data } = await api.get<TeamCoach[]>(`/teams/${teamId}/coaches`)
    teamCoaches.value[teamId] = data
  } catch {
    teamCoaches.value[teamId] = []
  }
}

function availableCoaches(teamId: string) {
  const assignedIds = new Set((teamCoaches.value[teamId] ?? []).map((c) => c.id))
  return coachesStore.coaches.filter((c) => !assignedIds.has(c.id))
}

async function handleAssignCoach(teamId: string) {
  if (!coachToAssign.value) return
  coachActionId.value = teamId
  try {
    await api.post(`/teams/${teamId}/coaches`, { coachId: coachToAssign.value })
    coachToAssign.value = ''
    await Promise.all([loadTeamCoaches(teamId), coachesStore.fetchMyCoaches()])
  } catch { /* ignore */ } finally {
    coachActionId.value = ''
  }
}

async function handleUnassignCoach(teamId: string, coachId: string) {
  try {
    await api.delete(`/teams/${teamId}/coaches/${coachId}`)
    await Promise.all([loadTeamCoaches(teamId), coachesStore.fetchMyCoaches()])
  } catch { /* ignore */ }
}

onMounted(fetchTeams)
</script>

<template>
  <div class="max-w-4xl mx-auto">
    <!-- Header -->
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Teams</h1>
        <p class="mt-0.5 text-sm text-gray-500">
          {{ teamsStore.teams.length }} team{{ teamsStore.teams.length !== 1 ? 's' : '' }} you belong to
        </p>
      </div>
      <button
        @click="showCreateForm = !showCreateForm"
        class="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
      >
        <span>＋</span> Create Team
      </button>
    </div>

    <!-- Create Form -->
    <div v-if="showCreateForm" class="mb-6 bg-blue-50 border border-blue-200 rounded-xl p-5">
      <h2 class="text-sm font-semibold text-blue-900 mb-1">New Team</h2>
      <p class="text-xs text-blue-700 mb-4">You'll be added as the team admin automatically.</p>

      <div v-if="createError" class="mb-3 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
        {{ createError }}
      </div>

      <form @submit.prevent="handleCreateTeam" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-gray-700 mb-1">Team Name *</label>
          <input v-model="newTeam.name" type="text" required placeholder="e.g. Northside Ice Hawks"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Sport *</label>
          <select v-model="newTeam.sport" required
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
          >
            <option value="HOCKEY">Hockey</option>
            <option value="SOCCER">Soccer</option>
            <option value="BASKETBALL">Basketball</option>
            <option value="BASEBALL">Baseball</option>
            <option value="OTHER">Other</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Season</label>
          <input v-model="newTeam.season" type="text" placeholder="e.g. Winter 2026-27"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div class="sm:col-span-2 flex gap-3 pt-1">
          <button type="submit" :disabled="createLoading"
            class="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
          >
            {{ createLoading ? 'Creating…' : 'Create Team' }}
          </button>
          <button type="button" @click="showCreateForm = false"
            class="text-sm text-gray-600 hover:text-gray-900 px-4 py-2 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>

    <!-- Teams List -->
    <div v-if="loading" class="text-center py-12 text-sm text-gray-400">Loading teams…</div>

    <div v-else-if="teamsStore.teams.length === 0" class="bg-white rounded-xl border border-gray-200 p-8 text-center text-sm text-gray-400">
      You're not on any teams yet. Click "Create Team" to start one and become its admin.
    </div>

    <div v-else class="space-y-3">
      <div v-for="team in teamsStore.teams" :key="team.id"
        class="bg-white rounded-xl border border-gray-200 shadow-sm p-4"
      >
        <!-- View mode -->
        <div v-if="editingId !== team.id" class="flex items-center gap-3">
          <span class="text-2xl">🏒</span>
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2">
              <h3 class="text-sm font-semibold text-gray-900">{{ team.name }}</h3>
              <span v-if="team.role === 'ADMIN'" class="text-xs bg-blue-100 text-blue-700 px-1.5 py-0.5 rounded">admin</span>
              <span v-else class="text-xs bg-gray-100 text-gray-500 px-1.5 py-0.5 rounded">member</span>
            </div>
            <p class="text-xs text-gray-500">
              {{ team.sport }}
              <span v-if="team.season"> · {{ team.season }}</span>
              <span v-if="team.playerCount !== undefined"> · {{ team.playerCount }} players</span>
              <span v-if="team.leagueName"> · {{ team.leagueName }}</span>
            </p>
          </div>
          <div class="flex items-center gap-2 flex-shrink-0">
            <button @click="toggleCoaches(team.id)"
              class="text-xs font-medium text-purple-600 hover:text-purple-800 border border-purple-200 px-2.5 py-1 rounded-full hover:bg-purple-50 transition-colors"
            >
              📋 Coaches
            </button>
            <template v-if="team.role === 'ADMIN'">
              <button @click="startEdit(team)"
                class="text-xs font-medium text-gray-500 hover:text-gray-700 border border-gray-200 px-2.5 py-1 rounded-full hover:bg-gray-50 transition-colors"
              >
                Edit
              </button>
              <button @click="handleDeleteTeam(team.id)"
                class="text-xs font-medium text-red-400 hover:text-red-600 border border-red-100 px-2.5 py-1 rounded-full hover:bg-red-50 transition-colors"
              >
                Delete
              </button>
            </template>
          </div>
        </div>

        <!-- Coaches Panel -->
        <div v-if="expandedCoachesId === team.id" class="mt-3 pt-3 border-t border-gray-100">
          <div class="flex items-center justify-between mb-2">
            <h4 class="text-xs font-semibold text-gray-700 uppercase tracking-wider">Assigned Coaches</h4>
          </div>

          <!-- Assign control (admins only) -->
          <div v-if="team.role === 'ADMIN'" class="flex gap-2 mb-3">
            <select v-model="coachToAssign"
              class="flex-1 px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
            >
              <option value="">Assign a coach from your pool…</option>
              <option v-for="coach in availableCoaches(team.id)" :key="coach.id" :value="coach.id">
                {{ coach.name }}{{ coach.title ? ` — ${coach.title}` : '' }}
              </option>
            </select>
            <button @click="handleAssignCoach(team.id)"
              :disabled="!coachToAssign || coachActionId === team.id"
              class="text-xs font-semibold bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white px-3 py-1.5 rounded-lg transition-colors whitespace-nowrap"
            >
              Assign
            </button>
          </div>
          <p v-if="team.role === 'ADMIN' && availableCoaches(team.id).length === 0 && !teamCoaches[team.id]?.length" class="text-xs text-gray-400 mb-2">
            Your coach pool is empty — add coaches on the Coaches screen first.
          </p>

          <!-- Assigned list -->
          <div v-if="!teamCoaches[team.id]?.length" class="text-xs text-gray-400 italic">
            No coaches assigned to this team yet.
          </div>
          <ul v-else class="space-y-1.5">
            <li v-for="coach in teamCoaches[team.id]" :key="coach.id"
              class="flex items-center justify-between bg-gray-50 border border-gray-200 rounded-lg px-3 py-2"
            >
              <div class="flex items-center gap-2 text-sm">
                <span class="font-medium text-gray-900">{{ coach.name }}</span>
                <span v-if="coach.title" class="text-xs bg-purple-100 text-purple-700 px-1.5 py-0.5 rounded">{{ coach.title }}</span>
              </div>
              <button v-if="team.role === 'ADMIN'" @click="handleUnassignCoach(team.id, coach.id)"
                class="text-gray-300 hover:text-red-500 transition-colors text-sm"
                title="Unassign coach"
              >
                ✕
              </button>
            </li>
          </ul>
        </div>

        <!-- Edit mode -->
        <div v-else>
          <div v-if="editError" class="mb-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-700">
            {{ editError }}
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 mb-2">
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Team Name *</label>
              <input v-model="editTeam.name" type="text" required
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Season</label>
              <input v-model="editTeam.season" type="text"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>
          <div class="flex gap-2">
            <button @click="handleUpdateTeam(team.id)" :disabled="editLoading"
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
    </div>
  </div>
</template>
