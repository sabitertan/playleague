import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/api/client'

export interface Team {
  id: string
  name: string
  sport: string
  season?: string | null
  role?: string
  leagueName?: string | null
  divisionName?: string | null
  playerCount?: number
}

export interface CreateTeamData {
  name: string
  sport: string
  season?: string
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
    // POST returns only the new id; the creator becomes a team ADMIN
    const { data: id } = await api.post<string>('/teams', teamData)
    const team: Team = {
      id,
      name: teamData.name,
      sport: teamData.sport,
      season: teamData.season || null,
      role: 'ADMIN',
      playerCount: 0,
    }
    teams.value.push(team)
    return team
  }

  async function updateTeam(id: string, teamData: { name: string; season?: string | null }) {
    await api.put(`/teams/${id}`, { name: teamData.name, season: teamData.season || null })
    const idx = teams.value.findIndex((t) => t.id === id)
    if (idx !== -1) teams.value[idx] = { ...teams.value[idx], ...teamData }
    if (currentTeam.value?.id === id) currentTeam.value = { ...currentTeam.value, ...teamData }
  }

  async function deleteTeam(id: string) {
    await api.delete(`/teams/${id}`)
    teams.value = teams.value.filter((t) => t.id !== id)
    if (currentTeam.value?.id === id) currentTeam.value = null
  }

  return { teams, currentTeam, fetchTeams, fetchTeam, createTeam, updateTeam, deleteTeam }
})
