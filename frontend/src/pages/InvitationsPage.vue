<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'
import { useTeamsStore } from '@/stores/teams'

interface Invitation {
  id: string
  email: string
  status: string
  expiresAt: string
  createdAt: string
}

const teamsStore = useTeamsStore()
const invitations = ref<Invitation[]>([])
const loading = ref(false)

const showInviteForm = ref(false)
const inviteLoading = ref(false)
const inviteError = ref('')
const inviteEmail = ref('')
const inviteSuccess = ref('')

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
}

function isExpired(expiresAt: string) {
  return new Date(expiresAt) < new Date()
}

async function fetchInvitations() {
  if (!teamsStore.currentTeam) return
  loading.value = true
  try {
    const { data } = await api.get<Invitation[]>(`/invitations/team/${teamsStore.currentTeam.id}`)
    invitations.value = data
  } catch {
    // ignore — user may not be admin
  } finally {
    loading.value = false
  }
}

async function handleSendInvite() {
  if (!teamsStore.currentTeam) return
  inviteError.value = ''
  inviteSuccess.value = ''
  inviteLoading.value = true
  try {
    await api.post('/invitations', {
      teamId: teamsStore.currentTeam.id,
      email: inviteEmail.value,
    })
    inviteSuccess.value = `Invitation sent to ${inviteEmail.value}.`
    inviteEmail.value = ''
    showInviteForm.value = false
    await fetchInvitations()
  } catch (err: any) {
    inviteError.value = err?.response?.data?.message ?? 'Failed to send invitation.'
  } finally {
    inviteLoading.value = false
  }
}

async function handleCancelInvitation(id: string) {
  if (!confirm('Cancel this invitation?')) return
  try {
    await api.delete(`/invitations/${id}`)
    invitations.value = invitations.value.filter((i) => i.id !== id)
  } catch { /* ignore */ }
}

onMounted(async () => {
  await teamsStore.fetchTeams()
  if (teamsStore.teams.length > 0) {
    teamsStore.currentTeam = teamsStore.teams[0]
    await fetchInvitations()
  }
})
</script>

<template>
  <div class="max-w-3xl mx-auto">
    <!-- Header -->
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Invitations</h1>
        <p class="mt-0.5 text-sm text-gray-500">
          Invite players and staff to join
          <span v-if="teamsStore.currentTeam"> {{ teamsStore.currentTeam.name }}</span>
        </p>
      </div>
      <button
        @click="showInviteForm = !showInviteForm"
        class="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
      >
        <span>＋</span> Invite
      </button>
    </div>

    <!-- Success message -->
    <div v-if="inviteSuccess" class="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg text-sm text-green-700">
      {{ inviteSuccess }}
    </div>

    <!-- Invite Form -->
    <div v-if="showInviteForm" class="mb-6 bg-blue-50 border border-blue-200 rounded-xl p-5">
      <h2 class="text-sm font-semibold text-blue-900 mb-4">Send Invitation</h2>

      <div v-if="inviteError" class="mb-3 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
        {{ inviteError }}
      </div>

      <form @submit.prevent="handleSendInvite" class="flex gap-3">
        <input
          v-model="inviteEmail"
          type="email"
          required
          placeholder="player@example.com"
          class="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <button type="submit" :disabled="inviteLoading"
          class="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors whitespace-nowrap"
        >
          {{ inviteLoading ? 'Sending…' : 'Send Invite' }}
        </button>
        <button type="button" @click="showInviteForm = false"
          class="text-sm text-gray-600 border border-gray-200 px-4 py-2 rounded-lg hover:bg-gray-50 transition-colors"
        >
          Cancel
        </button>
      </form>

      <p class="mt-3 text-xs text-gray-500">
        If the email is already registered they will be added immediately.
        Otherwise a 7-day invitation link will be created.
      </p>
    </div>

    <!-- Invitations List -->
    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="px-5 py-4 border-b border-gray-100">
        <h2 class="text-sm font-semibold text-gray-900">Pending Invitations</h2>
      </div>

      <div v-if="loading" class="px-5 py-8 text-center text-sm text-gray-400">Loading…</div>

      <div v-else-if="invitations.length === 0" class="px-5 py-8 text-center text-sm text-gray-400">
        No pending invitations. Click "Invite" to send one.
      </div>

      <table v-else class="min-w-full divide-y divide-gray-100">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Email</th>
            <th class="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider hidden sm:table-cell">Sent</th>
            <th class="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider hidden sm:table-cell">Expires</th>
            <th class="px-5 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Status</th>
            <th class="px-5 py-3 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="inv in invitations" :key="inv.id" class="hover:bg-gray-50 transition-colors">
            <td class="px-5 py-3.5 text-sm font-medium text-gray-900">{{ inv.email }}</td>
            <td class="px-5 py-3.5 text-sm text-gray-500 hidden sm:table-cell">{{ formatDate(inv.createdAt) }}</td>
            <td class="px-5 py-3.5 text-sm hidden sm:table-cell" :class="isExpired(inv.expiresAt) ? 'text-red-500' : 'text-gray-500'">
              {{ formatDate(inv.expiresAt) }}
              <span v-if="isExpired(inv.expiresAt)" class="ml-1 text-xs">(expired)</span>
            </td>
            <td class="px-5 py-3.5">
              <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-semibold bg-yellow-100 text-yellow-700">
                {{ inv.status }}
              </span>
            </td>
            <td class="px-5 py-3.5 text-right">
              <button @click="handleCancelInvitation(inv.id)"
                class="text-xs text-red-500 hover:text-red-700 font-medium"
              >
                Cancel
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
