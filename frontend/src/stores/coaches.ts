import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/api/client'

export interface AssignedTeam {
  teamId: string
  teamName: string
}

export interface Coach {
  id: string
  name: string
  title?: string | null
  email?: string | null
  phone?: string | null
  teams: AssignedTeam[]
}

export interface CoachData {
  name: string
  title?: string
  email?: string
  phone?: string
}

function toPayload(data: CoachData) {
  return {
    name: data.name,
    title: data.title || null,
    email: data.email || null,
    phone: data.phone || null,
  }
}

export const useCoachesStore = defineStore('coaches', () => {
  // The current user's coach pool
  const coaches = ref<Coach[]>([])

  async function fetchMyCoaches() {
    const { data } = await api.get<Coach[]>('/coaches')
    coaches.value = data
  }

  async function createCoach(data: CoachData) {
    const payload = toPayload(data)
    const { data: id } = await api.post<string>('/coaches', payload)
    coaches.value.push({ id, ...payload, teams: [] })
  }

  async function updateCoach(coachId: string, data: CoachData) {
    const payload = toPayload(data)
    await api.put(`/coaches/${coachId}`, payload)
    const idx = coaches.value.findIndex((c) => c.id === coachId)
    if (idx !== -1) coaches.value[idx] = { ...coaches.value[idx], ...payload }
  }

  async function deleteCoach(coachId: string) {
    await api.delete(`/coaches/${coachId}`)
    coaches.value = coaches.value.filter((c) => c.id !== coachId)
  }

  return { coaches, fetchMyCoaches, createCoach, updateCoach, deleteCoach }
})
