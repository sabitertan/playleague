<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'

interface LeagueSummary {
  id: string
  name: string
  sport: string
  contactEmail?: string | null
  role: string
}

interface Division {
  id: string
  name: string
  ageGroup?: string | null
  skillLevel?: string | null
  teamCount: number
}

interface LeagueDetail {
  id: string
  name: string
  sport: string
  contactEmail?: string | null
  teamCount: number
  memberCount: number
  divisions: Division[]
}

interface MessageSummary {
  id: string
  subject: string
  messageType: string
  priority: string
  createdAt: string
  senderName: string
  recipientCount: number
}

const league = ref<LeagueDetail | null>(null)
const messages = ref<MessageSummary[]>([])
const loading = ref(false)

// Division form
const showAddDivision = ref(false)
const divisionLoading = ref(false)
const divisionError = ref('')
const newDivision = ref({ name: '', ageGroup: '', skillLevel: '' })

// Message form
const showSendMessage = ref(false)
const messageLoading = ref(false)
const messageError = ref('')
const newMessage = ref({ subject: '', content: '', messageType: 'ANNOUNCEMENT', priority: 'NORMAL', entireLeague: true })

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })
}

function priorityBadgeClass(priority: string) {
  return priority === 'HIGH' || priority === 'URGENT'
    ? 'bg-red-100 text-red-700'
    : priority === 'NORMAL'
    ? 'bg-blue-100 text-blue-700'
    : 'bg-gray-100 text-gray-500'
}

async function fetchLeague() {
  loading.value = true
  try {
    const { data: leagues } = await api.get<LeagueSummary[]>('/leagues')
    if (leagues.length === 0) return
    const { data } = await api.get<LeagueDetail>(`/leagues/${leagues[0].id}`)
    league.value = data
  } catch {
    // not in a league
  } finally {
    loading.value = false
  }
}

async function fetchMessages() {
  if (!league.value) return
  try {
    const { data } = await api.get<{ items: MessageSummary[]; total: number }>(
      `/leagues/${league.value.id}/messages?page=1&pageSize=20`
    )
    messages.value = data.items
  } catch {
    // ignore
  }
}

async function handleAddDivision() {
  if (!league.value) return
  divisionError.value = ''
  divisionLoading.value = true
  try {
    const { data: id } = await api.post<string>(`/leagues/${league.value.id}/divisions`, {
      name: newDivision.value.name,
      ageGroup: newDivision.value.ageGroup || null,
      skillLevel: newDivision.value.skillLevel || null,
    })
    league.value.divisions.push({
      id,
      name: newDivision.value.name,
      ageGroup: newDivision.value.ageGroup || null,
      skillLevel: newDivision.value.skillLevel || null,
      teamCount: 0,
    })
    newDivision.value = { name: '', ageGroup: '', skillLevel: '' }
    showAddDivision.value = false
  } catch (err: any) {
    divisionError.value = err?.response?.data?.message ?? 'Failed to create division.'
  } finally {
    divisionLoading.value = false
  }
}

async function handleDeleteDivision(divisionId: string) {
  if (!league.value) return
  if (!confirm('Delete this division?')) return
  try {
    await api.delete(`/leagues/${league.value.id}/divisions/${divisionId}`)
    league.value.divisions = league.value.divisions.filter((d) => d.id !== divisionId)
  } catch { /* ignore */ }
}

async function handleSendMessage() {
  if (!league.value) return
  messageError.value = ''
  messageLoading.value = true
  try {
    await api.post(`/leagues/${league.value.id}/messages`, {
      subject: newMessage.value.subject,
      content: newMessage.value.content,
      messageType: newMessage.value.messageType,
      priority: newMessage.value.priority,
      entireLeague: newMessage.value.entireLeague,
    })
    newMessage.value = { subject: '', content: '', messageType: 'ANNOUNCEMENT', priority: 'NORMAL', entireLeague: true }
    showSendMessage.value = false
    await fetchMessages()
  } catch (err: any) {
    messageError.value = err?.response?.data?.message ?? 'Failed to send message.'
  } finally {
    messageLoading.value = false
  }
}

async function handleDeleteMessage(messageId: string) {
  if (!league.value) return
  if (!confirm('Delete this message?')) return
  try {
    await api.delete(`/leagues/${league.value.id}/messages/${messageId}`)
    messages.value = messages.value.filter((m) => m.id !== messageId)
  } catch { /* ignore */ }
}

onMounted(async () => {
  await fetchLeague()
  if (league.value) await fetchMessages()
})
</script>

<template>
  <div class="max-w-5xl mx-auto">
    <!-- Header -->
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-gray-900">League</h1>
      <p class="mt-0.5 text-sm text-gray-500">League information, divisions, and messages</p>
    </div>

    <div v-if="loading" class="text-center py-12 text-sm text-gray-400">Loading league info…</div>

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
        <p v-if="league.contactEmail" class="text-sm text-gray-500 mb-3">📧 {{ league.contactEmail }}</p>
        <div class="flex gap-6 text-sm">
          <div class="text-center">
            <p class="text-2xl font-bold text-gray-900">{{ league.teamCount }}</p>
            <p class="text-xs text-gray-500">Teams</p>
          </div>
          <div class="text-center">
            <p class="text-2xl font-bold text-gray-900">{{ league.memberCount }}</p>
            <p class="text-xs text-gray-500">Members</p>
          </div>
          <div class="text-center">
            <p class="text-2xl font-bold text-gray-900">{{ league.divisions.length }}</p>
            <p class="text-xs text-gray-500">Divisions</p>
          </div>
        </div>
      </div>

      <!-- Two Column: Divisions + Messages -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Divisions -->
        <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
          <div class="px-4 py-3.5 border-b border-gray-100 flex items-center justify-between">
            <h2 class="text-sm font-semibold text-gray-900">Divisions</h2>
            <button
              @click="showAddDivision = !showAddDivision"
              class="text-xs font-semibold text-blue-600 hover:text-blue-800 border border-blue-200 px-2.5 py-1 rounded-full hover:bg-blue-50 transition-colors"
            >
              ＋ Add
            </button>
          </div>

          <!-- Add Division Form -->
          <div v-if="showAddDivision" class="bg-blue-50 border-b border-blue-100 px-4 py-4">
            <div v-if="divisionError" class="mb-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-700">
              {{ divisionError }}
            </div>
            <form @submit.prevent="handleAddDivision" class="space-y-2">
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Division Name *</label>
                <input v-model="newDivision.name" type="text" required placeholder="e.g. Bantam A"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div class="grid grid-cols-2 gap-2">
                <div>
                  <label class="block text-xs font-medium text-gray-700 mb-1">Age Group</label>
                  <input v-model="newDivision.ageGroup" type="text" placeholder="e.g. U14"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label class="block text-xs font-medium text-gray-700 mb-1">Skill Level</label>
                  <input v-model="newDivision.skillLevel" type="text" placeholder="e.g. A, AA"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>
              <div class="flex gap-2">
                <button type="submit" :disabled="divisionLoading"
                  class="text-xs font-semibold bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white px-3 py-1.5 rounded-lg transition-colors"
                >
                  {{ divisionLoading ? 'Creating…' : 'Create' }}
                </button>
                <button type="button" @click="showAddDivision = false"
                  class="text-xs text-gray-600 border border-gray-200 px-3 py-1.5 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>

          <div v-if="league.divisions.length === 0" class="p-6 text-center text-sm text-gray-400">
            No divisions yet.
          </div>

          <ul v-else class="divide-y divide-gray-100">
            <li v-for="division in league.divisions" :key="division.id"
              class="px-4 py-3 flex items-center justify-between gap-2"
            >
              <div>
                <p class="text-sm font-medium text-gray-900">{{ division.name }}</p>
                <p class="text-xs text-gray-500">
                  {{ division.teamCount }} teams
                  <span v-if="division.ageGroup"> · {{ division.ageGroup }}</span>
                  <span v-if="division.skillLevel"> · {{ division.skillLevel }}</span>
                </p>
              </div>
              <button @click="handleDeleteDivision(division.id)"
                class="text-gray-300 hover:text-red-500 transition-colors text-sm flex-shrink-0"
                title="Delete division"
              >
                ✕
              </button>
            </li>
          </ul>
        </div>

        <!-- League Messages -->
        <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
          <div class="px-4 py-3.5 border-b border-gray-100 flex items-center justify-between">
            <h2 class="text-sm font-semibold text-gray-900">League Messages</h2>
            <button
              @click="showSendMessage = !showSendMessage"
              class="text-xs font-semibold text-blue-600 hover:text-blue-800 border border-blue-200 px-2.5 py-1 rounded-full hover:bg-blue-50 transition-colors"
            >
              ＋ Send
            </button>
          </div>

          <!-- Send Message Form -->
          <div v-if="showSendMessage" class="bg-blue-50 border-b border-blue-100 px-4 py-4">
            <div v-if="messageError" class="mb-2 p-2 bg-red-50 border border-red-200 rounded text-xs text-red-700">
              {{ messageError }}
            </div>
            <form @submit.prevent="handleSendMessage" class="space-y-2">
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Subject *</label>
                <input v-model="newMessage.subject" type="text" required placeholder="Message subject"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Content *</label>
                <textarea v-model="newMessage.content" rows="3" required placeholder="Message body…"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
                />
              </div>
              <div class="grid grid-cols-2 gap-2">
                <div>
                  <label class="block text-xs font-medium text-gray-700 mb-1">Type</label>
                  <select v-model="newMessage.messageType"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                  >
                    <option value="ANNOUNCEMENT">Announcement</option>
                    <option value="REMINDER">Reminder</option>
                    <option value="ALERT">Alert</option>
                    <option value="NEWSLETTER">Newsletter</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-medium text-gray-700 mb-1">Priority</label>
                  <select v-model="newMessage.priority"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                  >
                    <option value="LOW">Low</option>
                    <option value="NORMAL">Normal</option>
                    <option value="HIGH">High</option>
                    <option value="URGENT">Urgent</option>
                  </select>
                </div>
              </div>
              <div class="flex gap-2">
                <button type="submit" :disabled="messageLoading"
                  class="text-xs font-semibold bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white px-3 py-1.5 rounded-lg transition-colors"
                >
                  {{ messageLoading ? 'Sending…' : 'Send Message' }}
                </button>
                <button type="button" @click="showSendMessage = false"
                  class="text-xs text-gray-600 border border-gray-200 px-3 py-1.5 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>

          <div v-if="messages.length === 0" class="p-6 text-center text-sm text-gray-400">
            No league messages yet.
          </div>

          <ul v-else class="divide-y divide-gray-100">
            <li v-for="message in messages" :key="message.id"
              class="px-4 py-3 hover:bg-gray-50 transition-colors"
            >
              <div class="flex items-start justify-between gap-2">
                <div class="flex-1 min-w-0">
                  <div class="flex items-center gap-2 mb-0.5">
                    <p class="text-sm font-medium text-gray-900 truncate">{{ message.subject }}</p>
                    <span class="text-xs px-1.5 py-0.5 rounded flex-shrink-0"
                      :class="priorityBadgeClass(message.priority)"
                    >
                      {{ message.priority }}
                    </span>
                  </div>
                  <p class="text-xs text-gray-400">
                    {{ message.senderName }} · {{ formatDate(message.createdAt) }} · {{ message.recipientCount }} recipients
                  </p>
                </div>
                <button @click="handleDeleteMessage(message.id)"
                  class="text-gray-300 hover:text-red-500 transition-colors text-sm flex-shrink-0"
                  title="Delete message"
                >
                  ✕
                </button>
              </div>
            </li>
          </ul>
        </div>
      </div>
    </template>
  </div>
</template>
