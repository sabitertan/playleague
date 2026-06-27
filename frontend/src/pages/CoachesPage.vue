<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useCoachesStore, type CoachData } from '@/stores/coaches'

const coachesStore = useCoachesStore()

const loading = ref(false)

const showAddForm = ref(false)
const addError = ref('')
const addLoading = ref(false)
const newCoach = ref<CoachData>({ name: '', title: '', email: '', phone: '' })

const editingId = ref<string | null>(null)
const editCoach = ref<CoachData>({ name: '', title: '', email: '', phone: '' })
const editError = ref('')
const editLoading = ref(false)

async function handleAddCoach() {
  addError.value = ''
  addLoading.value = true
  try {
    await coachesStore.createCoach(newCoach.value)
    newCoach.value = { name: '', title: '', email: '', phone: '' }
    showAddForm.value = false
  } catch (err: any) {
    addError.value = err?.response?.data?.message ?? 'Failed to add coach.'
  } finally {
    addLoading.value = false
  }
}

function startEdit(coach: typeof coachesStore.coaches[0]) {
  editingId.value = coach.id
  editCoach.value = {
    name: coach.name,
    title: coach.title ?? '',
    email: coach.email ?? '',
    phone: coach.phone ?? '',
  }
  editError.value = ''
}

async function handleUpdateCoach() {
  if (!editingId.value) return
  editError.value = ''
  editLoading.value = true
  try {
    await coachesStore.updateCoach(editingId.value, editCoach.value)
    editingId.value = null
  } catch (err: any) {
    editError.value = err?.response?.data?.message ?? 'Failed to update coach.'
  } finally {
    editLoading.value = false
  }
}

async function handleDeleteCoach(coachId: string) {
  if (!confirm('Delete this coach? They will be removed from all teams.')) return
  await coachesStore.deleteCoach(coachId)
}

onMounted(async () => {
  loading.value = true
  try {
    await coachesStore.fetchMyCoaches()
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="max-w-4xl mx-auto">
    <!-- Header -->
    <div class="mb-6 flex items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Coaches</h1>
        <p class="mt-0.5 text-sm text-gray-500">
          Your coach pool — assign them to teams from the Teams screen
        </p>
      </div>
      <button
        @click="showAddForm = !showAddForm"
        class="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
      >
        <span>＋</span> Add Coach
      </button>
    </div>

    <!-- Add Coach Form -->
    <div v-if="showAddForm" class="mb-6 bg-blue-50 border border-blue-200 rounded-xl p-5">
      <h2 class="text-sm font-semibold text-blue-900 mb-4">New Coach</h2>

      <div v-if="addError" class="mb-3 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700">
        {{ addError }}
      </div>

      <form @submit.prevent="handleAddCoach" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Name *</label>
          <input v-model="newCoach.name" type="text" required placeholder="Pat Morrison"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Title</label>
          <input v-model="newCoach.title" type="text" placeholder="e.g. Head Coach"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Email</label>
          <input v-model="newCoach.email" type="email" placeholder="coach@example.com"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-700 mb-1">Phone</label>
          <input v-model="newCoach.phone" type="tel" placeholder="(555) 123-4567"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div class="sm:col-span-2 flex gap-3 pt-1">
          <button type="submit" :disabled="addLoading"
            class="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
          >
            {{ addLoading ? 'Saving…' : 'Save Coach' }}
          </button>
          <button type="button" @click="showAddForm = false"
            class="text-sm text-gray-600 hover:text-gray-900 px-4 py-2 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>

    <!-- Coaches List -->
    <div v-if="loading" class="text-center py-12 text-sm text-gray-400">Loading coaches…</div>

    <div v-else-if="coachesStore.coaches.length === 0" class="bg-white rounded-xl border border-gray-200 p-8 text-center text-sm text-gray-400">
      No coaches yet. Click "Add Coach" to build your pool, then assign them to teams.
    </div>

    <div v-else class="space-y-3">
      <div v-for="coach in coachesStore.coaches" :key="coach.id"
        class="bg-white rounded-xl border border-gray-200 shadow-sm p-4"
      >
        <!-- View mode -->
        <div v-if="editingId !== coach.id" class="flex items-start gap-3">
          <span class="text-2xl mt-0.5">📋</span>
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 flex-wrap">
              <h3 class="text-sm font-semibold text-gray-900">{{ coach.name }}</h3>
              <span v-if="coach.title" class="text-xs bg-purple-100 text-purple-700 px-1.5 py-0.5 rounded">{{ coach.title }}</span>
            </div>
            <p class="text-xs text-gray-500 mt-0.5">
              <span v-if="coach.email">{{ coach.email }}</span>
              <span v-if="coach.email && coach.phone"> · </span>
              <span v-if="coach.phone">{{ coach.phone }}</span>
              <span v-if="!coach.email && !coach.phone" class="text-gray-400">No contact info</span>
            </p>
            <!-- Assigned teams -->
            <div class="mt-2 flex flex-wrap gap-1.5">
              <span v-if="coach.teams.length === 0" class="text-xs text-gray-400 italic">Not assigned to any team</span>
              <span v-for="team in coach.teams" :key="team.teamId"
                class="inline-flex items-center text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full"
              >
                🏒 {{ team.teamName }}
              </span>
            </div>
          </div>
          <div class="flex items-center gap-2 flex-shrink-0">
            <button @click="startEdit(coach)"
              class="text-xs font-medium text-gray-500 hover:text-gray-700 border border-gray-200 px-2.5 py-1 rounded-full hover:bg-gray-50 transition-colors"
            >
              Edit
            </button>
            <button @click="handleDeleteCoach(coach.id)"
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
              <input v-model="editCoach.name" type="text" required
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Title</label>
              <input v-model="editCoach.title" type="text"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Email</label>
              <input v-model="editCoach.email" type="email"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-700 mb-1">Phone</label>
              <input v-model="editCoach.phone" type="tel"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>
          <div class="flex gap-2">
            <button @click="handleUpdateCoach" :disabled="editLoading"
              class="text-xs font-medium bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white px-3 py-1.5 rounded-lg transition-colors"
            >
              {{ editLoading ? 'Saving…' : 'Save' }}
            </button>
            <button @click="editingId = null"
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
