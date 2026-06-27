<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'

interface Division {
  id: string
  name: string
  teamCount?: number
}

interface LeagueTeam {
  id: string
  name: string
  sport: string
  divisionId?: string | null
}

interface LeagueMessage {
  id: string
  subject: string
  body: string
  createdAt: string
  sender?: { name: string | null }
}

interface League {
  id: string
  name: string
  sport: string
  description?: string | null
  divisions?: Division[]
  teams?: LeagueTeam[]
}

const league = ref<League | null>(null)
const messages = ref<LeagueMessage[]>([])
const loading = ref(false)

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
}

function teamsInDivision(divisionId: string) {
  return league.value?.teams?.filter((t) => t.divisionId === divisionId) ?? []
}

function unassignedTeams() {
  return league.value?.teams?.filter((t) => !t.divisionId) ?? []
}

async function fetchLeague() {
  loading.value = true
  try {
    const { data } = await api.get<League>('/leagues/current')
    league.value = data
  } catch {
    // backend may not be ready or team is not in a league
  } finally {
    loading.value = false
  }
}

async function fetchMessages() {
  try {
    const { data } = await api.get<LeagueMessage[]>('/league/messages')
    messages.value = data
  } catch {
    // ignore
  }
}

onMounted(async () => {
  await Promise.all([fetchLeague(), fetchMessages()])
})
</script>

<template>
  <div class="max-w-5xl mx-auto">
    <!-- Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-gray-900">League</h1>
      <p class="mt-0.5 text-sm text-gray-500">League information, teams, and divisions</p>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="text-center py-12 text-sm text-gray-400">
      Loading league info…
    </div>

    <!-- Not in a league -->
    <div v-else-if="!league" class="bg-white rounded-xl border border-gray-200 p-8 text-center">
      <span class="text-4xl block mb-3">🏆</span>
      <h2 class="text-lg font-semibold text-gray-900 mb-1">Not in a league</h2>
      <p class="text-sm text-gray-500">Your team is operating standalone. Contact a league administrator to join a league.</p>
    </div>

    <template v-else>
      <!-- League Info Card -->
      <div class="bg-white rounded-xl border border-gray-200 shadow-sm p-5 mb-6">
        <div class="flex items-center gap-3 mb-3">
          <span class="text-3xl">🏆</span>
          <div>
            <h2 class="text-lg font-bold text-gray-900">{{ league.name }}</h2>
            <p class="text-sm text-gray-500">{{ league.sport }}</p>
          </div>
        </div>
        <p v-if="league.description" class="text-sm text-gray-600">{{ league.description }}</p>

        <div class="mt-4 flex gap-4 text-sm">
          <div class="text-center">
            <p class="text-2xl font-bold text-gray-900">{{ league.teams?.length ?? 0 }}</p>
            <p class="text-xs text-gray-500">Teams</p>
          </div>
          <div class="text-center">
            <p class="text-2xl font-bold text-gray-900">{{ league.divisions?.length ?? 0 }}</p>
            <p class="text-xs text-gray-500">Divisions</p>
          </div>
        </div>
      </div>

      <!-- Two Column: Divisions + Messages -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Divisions & Teams -->
        <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
          <div class="px-4 py-3.5 border-b border-gray-100">
            <h2 class="text-sm font-semibold text-gray-900">Divisions & Teams</h2>
          </div>

          <div v-if="!league.divisions?.length && !league.teams?.length" class="p-6 text-center text-sm text-gray-400">
            No divisions or teams yet.
          </div>

          <div v-else class="divide-y divide-gray-100">
            <!-- Divisions with their teams -->
            <div
              v-for="division in league.divisions"
              :key="division.id"
              class="px-4 py-3"
            >
              <div class="flex items-center justify-between mb-2">
                <p class="text-xs font-semibold text-gray-500 uppercase tracking-wider">
                  {{ division.name }}
                </p>
                <span class="text-xs text-gray-400">
                  {{ teamsInDivision(division.id).length }} teams
                </span>
              </div>
              <ul class="space-y-1.5">
                <li
                  v-for="team in teamsInDivision(division.id)"
                  :key="team.id"
                  class="flex items-center gap-2 text-sm text-gray-700"
                >
                  <span class="text-gray-300">▪</span>
                  {{ team.name }}
                </li>
                <li v-if="teamsInDivision(division.id).length === 0" class="text-xs text-gray-400 italic">
                  No teams assigned
                </li>
              </ul>
            </div>

            <!-- Unassigned teams -->
            <div v-if="unassignedTeams().length > 0" class="px-4 py-3">
              <p class="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
                Unassigned
              </p>
              <ul class="space-y-1.5">
                <li
                  v-for="team in unassignedTeams()"
                  :key="team.id"
                  class="flex items-center gap-2 text-sm text-gray-700"
                >
                  <span class="text-gray-300">▪</span>
                  {{ team.name }}
                </li>
              </ul>
            </div>
          </div>
        </div>

        <!-- League Messages -->
        <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
          <div class="px-4 py-3.5 border-b border-gray-100 flex items-center justify-between">
            <h2 class="text-sm font-semibold text-gray-900">League Messages</h2>
            <RouterLink
              to="/league/messages"
              class="text-xs text-blue-600 hover:text-blue-500 font-medium"
            >
              View all →
            </RouterLink>
          </div>

          <div v-if="messages.length === 0" class="p-6 text-center text-sm text-gray-400">
            No league messages yet.
          </div>

          <ul v-else class="divide-y divide-gray-100">
            <li
              v-for="message in messages.slice(0, 5)"
              :key="message.id"
              class="px-4 py-3 hover:bg-gray-50 transition-colors"
            >
              <div class="flex items-start justify-between gap-2">
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium text-gray-900 truncate">{{ message.subject }}</p>
                  <p class="text-xs text-gray-400 mt-0.5">
                    {{ message.sender?.name ?? 'League Admin' }} · {{ formatDate(message.createdAt) }}
                  </p>
                  <p class="text-xs text-gray-500 mt-1 line-clamp-2">{{ message.body }}</p>
                </div>
              </div>
            </li>
          </ul>
        </div>
      </div>
    </template>
  </div>
</template>
