import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/api/client'

export interface Player {
  id: string
  name: string
  jerseyNumber?: string | null
  position?: string | null
  email?: string | null
  teamId: string
  userId?: string | null
}

export interface PlayerData {
  name: string
  jerseyNumber?: string
  position?: string
  email?: string
}

export const useRosterStore = defineStore('roster', () => {
  const players = ref<Player[]>([])

  async function fetchRoster(teamId: string) {
    const { data } = await api.get<Player[]>(`/teams/${teamId}/roster`)
    players.value = data
  }

  async function addPlayer(teamId: string, playerData: PlayerData) {
    const { data } = await api.post<Player>(`/teams/${teamId}/roster`, playerData)
    players.value.push(data)
    return data
  }

  async function updatePlayer(teamId: string, playerId: string, playerData: Partial<PlayerData>) {
    const { data } = await api.put<Player>(`/teams/${teamId}/roster/${playerId}`, playerData)
    const idx = players.value.findIndex((p) => p.id === playerId)
    if (idx !== -1) players.value[idx] = data
    return data
  }

  async function removePlayer(teamId: string, playerId: string) {
    await api.delete(`/teams/${teamId}/roster/${playerId}`)
    players.value = players.value.filter((p) => p.id !== playerId)
  }

  return { players, fetchRoster, addPlayer, updatePlayer, removePlayer }
})
