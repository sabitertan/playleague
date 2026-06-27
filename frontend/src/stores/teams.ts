import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/api/client'

export interface Team {
  id: string
  name: string
  sport: string
  leagueId?: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateTeamData {
  name: string
  sport: string
  leagueId?: string
}

export const useTeamsStore = defineStore('teams', () => {
  const teams = ref<Team[]>([])
  const currentTeam = ref<Team | null>(null)

  async function fetchTeams() {
    const { data } = await api.get<Team[]>('/teams')
    teams.value = data
  }

  async function fetchTeam(id: string) {
    const { data } = await api.get<Team>(`/teams/${id}`)
    currentTeam.value = data
    return data
  }

  async function createTeam(teamData: CreateTeamData) {
    const { data } = await api.post<Team>('/teams', teamData)
    teams.value.push(data)
    return data
  }

  async function updateTeam(id: string, teamData: Partial<CreateTeamData>) {
    const { data } = await api.put<Team>(`/teams/${id}`, teamData)
    const idx = teams.value.findIndex((t) => t.id === id)
    if (idx !== -1) teams.value[idx] = data
    if (currentTeam.value?.id === id) currentTeam.value = data
    return data
  }

  async function deleteTeam(id: string) {
    await api.delete(`/teams/${id}`)
    teams.value = teams.value.filter((t) => t.id !== id)
    if (currentTeam.value?.id === id) currentTeam.value = null
  }

  return { teams, currentTeam, fetchTeams, fetchTeam, createTeam, updateTeam, deleteTeam }
})
