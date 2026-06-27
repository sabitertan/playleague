<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'

interface Venue {
  id: string
  name: string
  address?: string | null
  city?: string | null
  state?: string | null
  surfaceCount?: number | null
  createdAt: string
}

interface CreateVenueData {
  name: string
  address: string
  city: string
  state: string
  surfaceCount: number | ''
}

const venues = ref<Venue[]>([])
const loading = ref(false)
const showAddForm = ref(false)
const addLoading = ref(false)
const addError = ref('')
const isAdmin = ref(true)

const newVenue = ref<CreateVenueData>({
  name: '',
  address: '',
  city: '',
  state: '',
  surfaceCount: '',
})

async function fetchVenues() {
  loading.value = true
  try {
    const { data } = await api.get<Venue[]>('/venues')
    venues.value = data
  } catch {
    // ignore
  } finally {
    loading.value = false
  }
}

async function handleAddVenue() {
  addError.value = ''
  addLoading.value = true
  try {
    const { data } = await api.post<Venue>('/venues', {
      ...newVenue.value,
      surfaceCount: newVenue.value.surfaceCount === '' ? undefined : Number(newVenue.value.surfaceCount),
    })
    venues.value.push(data)
    resetForm()
  } catch (err: any) {
    addError.value = err?.response?.data?.message ?? 'Failed to add venue.'
  } finally {
    addLoading.value = false
  }
}

async function handleDeleteVenue(id: string) {
  if (!confirm('Delete this venue?')) return
  try {
    await api.delete(`/venues/${id}`)
    venues.value = venues.value.filter((v) => v.id !== id)
  } catch { /* ignore */ }
}

function resetForm() {
  newVenue.value = { name: '', address: '', city: '', state: '', surfaceCount: '' }
  addError.value = ''
  showAddForm.value = false
}

onMounted(fetchVenues)
</script>

<template>
  <div class="max-w-4xl mx-auto">
    <!-- Header -->
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Venues</h1>
        <p class="mt-0.5 text-sm text-gray-500">{{ venues.length }} venue{{ venues.length !== 1 ? 's' : '' }} registered</p>
      </div>
      <button
        v-if="isAdmin"
        @click="showAddForm = !showAddForm"
        class="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
      >
        <span>＋</span> Add Venue
      </button>
    </div>

    <!-- Add Venue Form -->
    <div v-if="showAddForm" class="mb-6 bg-blue-50 border border-blue-200 rounded-xl p-5">
      <h2 class="text-sm font-semibold text-blue-900 mb-4">New Venue</h2>

      <div v-if="addError" class="mb-3 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
        {{ addError }}
      </div>

      <form @submit.prevent="handleAddVenue" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-gray-700 mb-1">Venue Name *</label>
          <input
            v-model="newVenue.name"
            type="text"
            required
            placeholder="e.g. Downtown Ice Arena"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-gray-700 mb-1">Street Address</label>
          <input
            v-model="newVenue.address"
            type="text"
            placeholder="123 Main St"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">City</label>
          <input
            v-model="newVenue.city"
            type="text"
            placeholder="Minneapolis"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">State</label>
          <input
            v-model="newVenue.state"
            type="text"
            placeholder="MN"
            maxlength="2"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Number of Surfaces / Rinks</label>
          <input
            v-model="newVenue.surfaceCount"
            type="number"
            min="1"
            placeholder="e.g. 3"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div class="sm:col-span-2 flex gap-3 pt-1">
          <button
            type="submit"
            :disabled="addLoading"
            class="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
          >
            {{ addLoading ? 'Saving…' : 'Save Venue' }}
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

    <!-- Venues Grid -->
    <div v-if="loading" class="text-center py-12 text-sm text-gray-400">Loading venues…</div>

    <div v-else-if="venues.length === 0" class="bg-white rounded-xl border border-gray-200 p-8 text-center text-sm text-gray-400">
      No venues registered yet.
      <span v-if="isAdmin"> Click "Add Venue" to get started.</span>
    </div>

    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <div
        v-for="venue in venues"
        :key="venue.id"
        class="bg-white rounded-xl border border-gray-200 shadow-sm p-4 hover:shadow-md transition-shadow"
      >
        <div class="flex items-start justify-between gap-2 mb-3">
          <div class="flex items-center gap-2">
            <span class="text-2xl">📍</span>
            <h3 class="text-sm font-semibold text-gray-900 leading-snug">{{ venue.name }}</h3>
          </div>
          <button
            v-if="isAdmin"
            @click="handleDeleteVenue(venue.id)"
            class="text-gray-300 hover:text-red-500 transition-colors flex-shrink-0 text-sm"
            title="Delete venue"
          >
            ✕
          </button>
        </div>

        <div class="space-y-1 text-xs text-gray-500">
          <p v-if="venue.address">{{ venue.address }}</p>
          <p v-if="venue.city || venue.state">
            {{ [venue.city, venue.state].filter(Boolean).join(', ') }}
          </p>
          <p v-if="venue.surfaceCount">
            🏒 {{ venue.surfaceCount }} surface{{ venue.surfaceCount !== 1 ? 's' : '' }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
