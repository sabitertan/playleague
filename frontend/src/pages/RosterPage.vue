<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRosterStore, type PlayerData } from '@/stores/roster'
import { useTeamsStore } from '@/stores/teams'

const rosterStore = useRosterStore()
const teamsStore = useTeamsStore()

const isAdmin = ref(true)

const showAddForm = ref(false)
const addError = ref('')
const addLoading = ref(false)

const newPlayer = ref<PlayerData>({
  name: '',
  jerseyNumber: '',
  position: '',
  email: '',
})

function resetForm() {
  newPlayer.value = { name: '', jerseyNumber: '', position: '', email: '' }
  addError.value = ''
  showAddForm.value = false
}

async function handleAddPlayer() {
  if (!teamsStore.currentTeam) return
  addError.value = ''
  addLoading.value = true
  try {
    await rosterStore.addPlayer(teamsStore.currentTeam.id, newPlayer.value)
    resetForm()
  } catch (err: any) {
    addError.value = err?.response?.data?.message ?? 'Failed to add player.'
  } finally {
    addLoading.value = false
  }
}

async function handleRemovePlayer(playerId: string) {
  if (!teamsStore.currentTeam) return
  if (!confirm('Remove this player from the roster?')) return
  await rosterStore.removePlayer(teamsStore.currentTeam.id, playerId)
}

onMounted(async () => {
  await teamsStore.fetchTeams()
  if (teamsStore.teams.length > 0) {
    const team = teamsStore.teams[0]
    teamsStore.currentTeam = team
    await rosterStore.fetchRoster(team.id)
  }
})
</script>

<template>
  <div class="max-w-5xl mx-auto">
    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Roster</h1>
        <p class="mt-0.5 text-sm text-gray-500">
          {{ rosterStore.players.length }} player{{ rosterStore.players.length !== 1 ? 's' : '' }}
          <span v-if="teamsStore.currentTeam"> — {{ teamsStore.currentTeam.name }}</span>
        </p>
      </div>
      <button
        v-if="isAdmin"
        @click="showAddForm = !showAddForm"
        class="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
      >
        <span>＋</span> Add Player
      </button>
    </div>

    <!-- Add Player Inline Form -->
    <div
      v-if="showAddForm"
      class="mb-6 bg-blue-50 border border-blue-200 rounded-xl p-5"
    >
      <h2 class="text-sm font-semibold text-blue-900 mb-4">New Player</h2>

      <div v-if="addError" class="mb-3 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
        {{ addError }}
      </div>

      <form @submit.prevent="handleAddPlayer" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Name *</label>
          <input
            v-model="newPlayer.name"
            type="text"
            required
            placeholder="Jane Smith"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Jersey #</label>
          <input
            v-model="newPlayer.jerseyNumber"
            type="text"
            placeholder="e.g. 42"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Position</label>
          <input
            v-model="newPlayer.position"
            type="text"
            placeholder="e.g. Forward"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Email</label>
          <input
            v-model="newPlayer.email"
            type="email"
            placeholder="player@example.com"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div class="sm:col-span-2 flex gap-3 pt-1">
          <button
            type="submit"
            :disabled="addLoading"
            class="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
          >
            {{ addLoading ? 'Saving…' : 'Save Player' }}
          </button>
          <button
            type="button"
            @click="resetForm"
            class="text-sm text-gray-600 hover:text-gray-900 px-4 py-2 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>

    <!-- Roster Table -->
    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div v-if="rosterStore.players.length === 0" class="px-5 py-12 text-center text-sm text-gray-400">
        No players on the roster yet.
        <span v-if="isAdmin"> Click "Add Player" to get started.</span>
      </div>

      <table v-else class="min-w-full divide-y divide-gray-100">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">#</th>
            <th class="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Name</th>
            <th class="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider hidden sm:table-cell">Position</th>
            <th class="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider hidden md:table-cell">Email</th>
            <th v-if="isAdmin" class="px-5 py-3 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr
            v-for="player in rosterStore.players"
            :key="player.id"
            class="hover:bg-gray-50 transition-colors"
          >
            <td class="px-5 py-3.5 text-sm text-gray-500">
              {{ player.jerseyNumber ?? '—' }}
            </td>
            <td class="px-5 py-3.5 text-sm font-medium text-gray-900">
              {{ player.name }}
            </td>
            <td class="px-5 py-3.5 text-sm text-gray-500 hidden sm:table-cell">
              {{ player.position ?? '—' }}
            </td>
            <td class="px-5 py-3.5 text-sm text-gray-500 hidden md:table-cell">
              {{ player.email ?? '—' }}
            </td>
            <td v-if="isAdmin" class="px-5 py-3.5 text-right">
              <button
                @click="handleRemovePlayer(player.id)"
                class="text-xs text-red-500 hover:text-red-700 font-medium"
              >
                Remove
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
