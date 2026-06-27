<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'
import { useTeamsStore } from '@/stores/teams'

const teamsStore = useTeamsStore()

interface IceSurface {
  id: string
  name: string
  surfaceType?: string | null
  capacity?: number | null
  isDefault: boolean
}

interface Venue {
  id: string
  name: string
  address?: string | null
  city?: string | null
  state?: string | null
  surfaces?: IceSurface[]
}

interface VenueFormData {
  name: string
  address: string
  city: string
  state: string
}

const venues = ref<Venue[]>([])
const loading = ref(false)
const showAddForm = ref(false)
const addLoading = ref(false)
const addError = ref('')

const editingId = ref<string | null>(null)
const editLoading = ref(false)
const editError = ref('')

const expandedSurfacesId = ref<string | null>(null)
const addSurfaceForId = ref<string | null>(null)
const addSurfaceLoading = ref(false)
const addSurfaceError = ref('')

const newVenue = ref<VenueFormData>({ name: '', address: '', city: '', state: '' })
const editVenue = ref<VenueFormData>({ name: '', address: '', city: '', state: '' })
const newSurface = ref({ name: '', surfaceType: '', capacity: '' as number | '' })

async function fetchVenues() {
  loading.value = true
  try {
    const { data } = await api.get<Venue[]>('/venues')
    venues.value = data.map((v) => ({ ...v, surfaces: [] }))
  } catch {
    // ignore
  } finally {
    loading.value = false
  }
}

async function fetchSurfaces(venueId: string) {
  const venue = venues.value.find((v) => v.id === venueId)
  if (!venue) return
  try {
    const { data } = await api.get<IceSurface[]>(`/venues/${venueId}/surfaces`)
    venue.surfaces = data
  } catch { /* ignore */ }
}

async function handleAddVenue() {
  addError.value = ''
  addLoading.value = true
  try {
    const { data: id } = await api.post<string>('/venues', {
      ...newVenue.value,
      teamId: teamsStore.currentTeam?.id ?? null,
      visibility: 'TEAM',
    })
    venues.value.push({
      id,
      name: newVenue.value.name,
      address: newVenue.value.address || null,
      city: newVenue.value.city || null,
      state: newVenue.value.state || null,
      surfaces: [],
    })
    newVenue.value = { name: '', address: '', city: '', state: '' }
    showAddForm.value = false
  } catch (err: any) {
    addError.value = err?.response?.data?.message ?? 'Failed to add venue.'
  } finally {
    addLoading.value = false
  }
}

function startEdit(venue: Venue) {
  editingId.value = venue.id
  editVenue.value = {
    name: venue.name,
    address: venue.address ?? '',
    city: venue.city ?? '',
    state: venue.state ?? '',
  }
  editError.value = ''
}

function cancelEdit() {
  editingId.value = null
  editError.value = ''
}

async function handleUpdateVenue(id: string) {
  editError.value = ''
  editLoading.value = true
  try {
    await api.put(`/venues/${id}`, editVenue.value)
    const idx = venues.value.findIndex((v) => v.id === id)
    if (idx !== -1) {
      venues.value[idx] = { ...venues.value[idx], ...editVenue.value }
    }
    editingId.value = null
  } catch (err: any) {
    editError.value = err?.response?.data?.message ?? 'Failed to update venue.'
  } finally {
    editLoading.value = false
  }
}

async function handleDeleteVenue(id: string) {
  if (!confirm('Delete this venue?')) return
  try {
    await api.delete(`/venues/${id}`)
    venues.value = venues.value.filter((v) => v.id !== id)
  } catch { /* ignore */ }
}

async function toggleSurfaces(venueId: string) {
  if (expandedSurfacesId.value === venueId) {
    expandedSurfacesId.value = null
    addSurfaceForId.value = null
    return
  }
  expandedSurfacesId.value = venueId
  await fetchSurfaces(venueId)
}

function openAddSurface(venueId: string) {
  addSurfaceForId.value = venueId
  newSurface.value = { name: '', surfaceType: '', capacity: '' }
  addSurfaceError.value = ''
}

async function handleAddSurface(venueId: string) {
  addSurfaceError.value = ''
  addSurfaceLoading.value = true
  try {
    const { data: id } = await api.post<string>(`/venues/${venueId}/surfaces`, {
      name: newSurface.value.name,
      surfaceType: newSurface.value.surfaceType || null,
      capacity: newSurface.value.capacity === '' ? null : Number(newSurface.value.capacity),
      isDefault: false,
    })
    const venue = venues.value.find((v) => v.id === venueId)
    if (venue) venue.surfaces = [...(venue.surfaces ?? []), {
      id,
      name: newSurface.value.name,
      surfaceType: newSurface.value.surfaceType || null,
      capacity: newSurface.value.capacity === '' ? null : Number(newSurface.value.capacity),
      isDefault: false,
    }]
    addSurfaceForId.value = null
    newSurface.value = { name: '', surfaceType: '', capacity: '' }
  } catch (err: any) {
    addSurfaceError.value = err?.response?.data?.message ?? 'Failed to add surface.'
  } finally {
    addSurfaceLoading.value = false
  }
}

async function handleDeleteSurface(venueId: string, surfaceId: string) {
  if (!confirm('Delete this surface?')) return
  try {
    await api.delete(`/venues/${venueId}/surfaces/${surfaceId}`)
    const venue = venues.value.find((v) => v.id === venueId)
    if (venue) venue.surfaces = venue.surfaces?.filter((s) => s.id !== surfaceId)
  } catch { /* ignore */ }
}

onMounted(async () => {
  await teamsStore.fetchTeams()
  if (teamsStore.teams.length > 0) teamsStore.currentTeam = teamsStore.teams[0]
  await fetchVenues()
})
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
          <input v-model="newVenue.name" type="text" required placeholder="e.g. Downtown Ice Arena"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-gray-700 mb-1">Street Address</label>
          <input v-model="newVenue.address" type="text" placeholder="123 Main St"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">City</label>
          <input v-model="newVenue.city" type="text" placeholder="Minneapolis"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">State</label>
          <input v-model="newVenue.state" type="text" placeholder="MN" maxlength="2"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div class="sm:col-span-2 flex gap-3 pt-1">
          <button type="submit" :disabled="addLoading"
            class="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
          >
            {{ addLoading ? 'Saving…' : 'Save Venue' }}
          </button>
          <button type="button" @click="showAddForm = false"
            class="text-sm text-gray-600 hover:text-gray-900 px-4 py-2 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>

    <!-- Venues List -->
    <div v-if="loading" class="text-center py-12 text-sm text-gray-400">Loading venues…</div>

    <div v-else-if="venues.length === 0" class="bg-white rounded-xl border border-gray-200 p-8 text-center text-sm text-gray-400">
      No venues registered yet. Click "Add Venue" to get started.
    </div>

    <div v-else class="space-y-4">
      <div v-for="venue in venues" :key="venue.id"
        class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden"
      >
        <!-- Venue card -->
        <div class="p-4">
          <!-- View mode -->
          <div v-if="editingId !== venue.id" class="flex items-start justify-between gap-3">
            <div class="flex items-start gap-3">
              <span class="text-2xl mt-0.5">📍</span>
              <div>
                <h3 class="text-sm font-semibold text-gray-900">{{ venue.name }}</h3>
                <div class="space-y-0.5 text-xs text-gray-500">
                  <p v-if="venue.address">{{ venue.address }}</p>
                  <p v-if="venue.city || venue.state">{{ [venue.city, venue.state].filter(Boolean).join(', ') }}</p>
                </div>
              </div>
            </div>
            <div class="flex items-center gap-2 flex-shrink-0">
              <button @click="toggleSurfaces(venue.id)"
                class="text-xs font-medium text-purple-600 hover:text-purple-800 border border-purple-200 px-2.5 py-1 rounded-full hover:bg-purple-50 transition-colors"
              >
                🏒 Surfaces
              </button>
              <button @click="startEdit(venue)"
                class="text-xs font-medium text-gray-500 hover:text-gray-700 border border-gray-200 px-2.5 py-1 rounded-full hover:bg-gray-50 transition-colors"
              >
                Edit
              </button>
              <button @click="handleDeleteVenue(venue.id)"
                class="text-gray-300 hover:text-red-500 transition-colors text-sm"
                title="Delete venue"
              >
                ✕
              </button>
            </div>
          </div>

          <!-- Edit mode -->
          <div v-else>
            <div v-if="editError" class="mb-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-700">
              {{ editError }}
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 mb-2">
              <div class="sm:col-span-2">
                <label class="block text-xs font-medium text-gray-700 mb-1">Venue Name *</label>
                <input v-model="editVenue.name" type="text" required
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div class="sm:col-span-2">
                <label class="block text-xs font-medium text-gray-700 mb-1">Address</label>
                <input v-model="editVenue.address" type="text"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">City</label>
                <input v-model="editVenue.city" type="text"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">State</label>
                <input v-model="editVenue.state" type="text" maxlength="2"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>
            <div class="flex gap-2">
              <button @click="handleUpdateVenue(venue.id)" :disabled="editLoading"
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

        <!-- Surfaces Panel -->
        <div v-if="expandedSurfacesId === venue.id" class="border-t border-gray-100 bg-gray-50 px-4 py-3">
          <div class="flex items-center justify-between mb-3">
            <h4 class="text-xs font-semibold text-gray-700 uppercase tracking-wider">Ice Surfaces / Rinks</h4>
            <button @click="openAddSurface(venue.id)"
              class="text-xs font-semibold text-blue-600 hover:text-blue-800 border border-blue-200 px-2 py-0.5 rounded-full hover:bg-blue-50 transition-colors"
            >
              ＋ Add Surface
            </button>
          </div>

          <!-- Add Surface Form -->
          <div v-if="addSurfaceForId === venue.id" class="mb-3 bg-white border border-blue-200 rounded-lg p-3">
            <div v-if="addSurfaceError" class="mb-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-700">
              {{ addSurfaceError }}
            </div>
            <form @submit.prevent="handleAddSurface(venue.id)" class="space-y-2">
              <div class="grid grid-cols-3 gap-2">
                <div>
                  <label class="block text-xs font-medium text-gray-700 mb-1">Name *</label>
                  <input v-model="newSurface.name" type="text" required placeholder="e.g. Rink A"
                    class="w-full px-2 py-1.5 border border-gray-300 rounded text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label class="block text-xs font-medium text-gray-700 mb-1">Type</label>
                  <input v-model="newSurface.surfaceType" type="text" placeholder="e.g. Ice"
                    class="w-full px-2 py-1.5 border border-gray-300 rounded text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label class="block text-xs font-medium text-gray-700 mb-1">Capacity</label>
                  <input v-model="newSurface.capacity" type="number" min="1" placeholder="e.g. 500"
                    class="w-full px-2 py-1.5 border border-gray-300 rounded text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </div>
              </div>
              <div class="flex gap-2">
                <button type="submit" :disabled="addSurfaceLoading"
                  class="text-xs font-semibold bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white px-3 py-1 rounded transition-colors"
                >
                  {{ addSurfaceLoading ? 'Adding…' : 'Add' }}
                </button>
                <button type="button" @click="addSurfaceForId = null"
                  class="text-xs text-gray-600 border border-gray-200 px-3 py-1 rounded hover:bg-gray-100 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>

          <div v-if="!venue.surfaces?.length" class="text-xs text-gray-400 text-center py-2">
            No surfaces added yet.
          </div>
          <ul v-else class="space-y-1.5">
            <li v-for="surface in venue.surfaces" :key="surface.id"
              class="flex items-center justify-between bg-white border border-gray-200 rounded-lg px-3 py-2"
            >
              <div class="flex items-center gap-2 text-xs">
                <span class="font-medium text-gray-900">{{ surface.name }}</span>
                <span v-if="surface.isDefault" class="bg-blue-100 text-blue-700 px-1.5 py-0.5 rounded">default</span>
                <span v-if="surface.surfaceType" class="text-gray-500">{{ surface.surfaceType }}</span>
                <span v-if="surface.capacity" class="text-gray-400">· {{ surface.capacity }} cap.</span>
              </div>
              <button @click="handleDeleteSurface(venue.id, surface.id)"
                class="text-gray-300 hover:text-red-500 transition-colors text-sm"
                title="Delete surface"
              >
                ✕
              </button>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>
