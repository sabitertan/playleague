<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useEventsStore, type CreateEventData, type RsvpStatus } from '@/stores/events'
import { useTeamsStore } from '@/stores/teams'

const eventsStore = useEventsStore()
const teamsStore = useTeamsStore()

const isAdmin = ref(true)
const showCreateForm = ref(false)
const createLoading = ref(false)
const createError = ref('')

const newEvent = ref<CreateEventData>({
  title: '',
  type: 'PRACTICE',
  date: '',
  location: '',
  teamId: '',
  description: '',
})

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function typeBadgeClass(type: string) {
  return type === 'GAME'
    ? 'bg-blue-100 text-blue-700'
    : type === 'PRACTICE'
    ? 'bg-purple-100 text-purple-700'
    : 'bg-gray-100 text-gray-600'
}

async function handleCreateEvent() {
  createError.value = ''
  createLoading.value = true
  try {
    newEvent.value.teamId = teamsStore.currentTeam?.id ?? teamsStore.teams[0]?.id ?? ''
    await eventsStore.createEvent(newEvent.value)
    resetCreateForm()
  } catch (err: any) {
    createError.value = err?.response?.data?.message ?? 'Failed to create event.'
  } finally {
    createLoading.value = false
  }
}

function resetCreateForm() {
  newEvent.value = { title: '', type: 'PRACTICE', date: '', location: '', teamId: '', description: '' }
  createError.value = ''
  showCreateForm.value = false
}

async function handleRsvp(eventId: string, status: RsvpStatus) {
  await eventsStore.updateRsvp(eventId, status)
}

async function handleDeleteEvent(id: string) {
  if (!confirm('Delete this event?')) return
  await eventsStore.deleteEvent(id)
}

onMounted(async () => {
  await teamsStore.fetchTeams()
  if (teamsStore.teams.length > 0) {
    const team = teamsStore.teams[0]
    teamsStore.currentTeam = team
    await eventsStore.fetchEvents(team.id)
  }
})
</script>

<template>
  <div class="max-w-4xl mx-auto">
    <!-- Header -->
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Events</h1>
        <p class="mt-0.5 text-sm text-gray-500">{{ eventsStore.events.length }} total events</p>
      </div>
      <button
        v-if="isAdmin"
        @click="showCreateForm = !showCreateForm"
        class="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
      >
        <span>＋</span> Create Event
      </button>
    </div>

    <!-- Create Event Form -->
    <div v-if="showCreateForm" class="mb-6 bg-blue-50 border border-blue-200 rounded-xl p-5">
      <h2 class="text-sm font-semibold text-blue-900 mb-4">New Event</h2>

      <div v-if="createError" class="mb-3 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
        {{ createError }}
      </div>

      <form @submit.prevent="handleCreateEvent" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-gray-700 mb-1">Title *</label>
          <input
            v-model="newEvent.title"
            type="text"
            required
            placeholder="e.g. Friday Night Practice"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Type *</label>
          <select
            v-model="newEvent.type"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
          >
            <option value="PRACTICE">Practice</option>
            <option value="GAME">Game</option>
            <option value="OTHER">Other</option>
          </select>
        </div>

        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Date & Time *</label>
          <input
            v-model="newEvent.date"
            type="datetime-local"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-gray-700 mb-1">Location</label>
          <input
            v-model="newEvent.location"
            type="text"
            placeholder="e.g. Main Arena, Rink 2"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-gray-700 mb-1">Description</label>
          <textarea
            v-model="newEvent.description"
            rows="2"
            placeholder="Optional notes…"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
          />
        </div>

        <div class="sm:col-span-2 flex gap-3 pt-1">
          <button
            type="submit"
            :disabled="createLoading"
            class="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
          >
            {{ createLoading ? 'Creating…' : 'Create Event' }}
          </button>
          <button
            type="button"
            @click="resetCreateForm"
            class="text-sm text-gray-600 hover:text-gray-900 px-4 py-2 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>

    <!-- Events List -->
    <div class="space-y-3">
      <div v-if="eventsStore.events.length === 0" class="bg-white rounded-xl border border-gray-200 p-8 text-center text-sm text-gray-400">
        No events yet.
        <span v-if="isAdmin"> Create one to get started.</span>
      </div>

      <div
        v-for="event in eventsStore.events"
        :key="event.id"
        class="bg-white rounded-xl border border-gray-200 shadow-sm p-4 hover:shadow-md transition-shadow"
      >
        <div class="flex items-start gap-3">
          <!-- Type Badge -->
          <span
            class="mt-0.5 inline-flex items-center px-2 py-0.5 rounded text-xs font-semibold flex-shrink-0"
            :class="typeBadgeClass(event.type)"
          >
            {{ event.type }}
          </span>

          <!-- Event Details -->
          <div class="flex-1 min-w-0">
            <div class="flex items-start justify-between gap-2">
              <h3 class="text-sm font-semibold text-gray-900">{{ event.title }}</h3>
              <button v-if="isAdmin" @click="handleDeleteEvent(event.id)"
                class="text-gray-300 hover:text-red-500 transition-colors text-sm flex-shrink-0"
                title="Delete event"
              >✕</button>
            </div>
            <div class="mt-1 flex flex-wrap gap-x-4 gap-y-0.5 text-xs text-gray-500">
              <span>📅 {{ formatDate(event.date) }}</span>
              <span v-if="event.location">📍 {{ event.location }}</span>
            </div>
            <p v-if="event.description" class="mt-1.5 text-xs text-gray-500 line-clamp-2">
              {{ event.description }}
            </p>
          </div>
        </div>

        <!-- RSVP Buttons -->
        <div class="mt-3 pt-3 border-t border-gray-100 flex items-center gap-2">
          <span class="text-xs text-gray-500 mr-1">RSVP:</span>
          <button
            v-for="(label, status) in { GOING: '✅ Going', MAYBE: '🤔 Maybe', NOT_GOING: '❌ Not Going' }"
            :key="status"
            @click="handleRsvp(event.id, status as RsvpStatus)"
            class="text-xs font-medium px-2.5 py-1 rounded-full border transition-colors"
            :class="
              event.myRsvp === status
                ? 'bg-blue-600 border-blue-600 text-white'
                : 'border-gray-200 text-gray-600 hover:bg-gray-50'
            "
          >
            {{ label }}
          </button>

          <!-- RSVP Counts -->
          <div v-if="event.rsvpCounts" class="ml-auto flex gap-3 text-xs text-gray-400">
            <span>✅ {{ event.rsvpCounts.GOING }}</span>
            <span>🤔 {{ event.rsvpCounts.MAYBE }}</span>
            <span>❌ {{ event.rsvpCounts.NOT_GOING }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
