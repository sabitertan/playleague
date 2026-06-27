import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/api/client'

export interface Player {
  id: string
  name: string
  jerseyNumber?: number | null
  position?: string | null
  email?: string | null
  phone?: string | null
  userId?: string | null
}

export interface PlayerData {
  name: string
  jerseyNumber?: string
  position?: string
  email?: string
}

// Map the form's string jersey number to the backend's int? field
function toPayload(playerData: PlayerData) {
  return {
    name: playerData.name,
    email: playerData.email || null,
    position: playerData.position || null,
    jerseyNumber:
      playerData.jerseyNumber && playerData.jerseyNumber.trim() !== ''
        ? Number(playerData.jerseyNumber)
        : null,
  }
}

export const useRosterStore = defineStore('roster', () => {
  const players = ref<Player[]>([])

  async function fetchRoster(teamId: string) {
    const { data } = await api.get<Player[]>(`/teams/${teamId}/roster`)
    players.value = data
  }

  async function addPlayer(teamId: string, playerData: PlayerData) {
    const payload = toPayload(playerData)
    const { data: id } = await api.post<string>(`/teams/${teamId}/roster`, payload)
    players.value.push({ id, ...payload })
  }

  async function updatePlayer(teamId: string, playerId: string, playerData: PlayerData) {
    const payload = toPayload(playerData)
    await api.put(`/teams/${teamId}/roster/${playerId}`, payload)
    const idx = players.value.findIndex((p) => p.id === playerId)
    if (idx !== -1) players.value[idx] = { ...players.value[idx], ...payload }
  }

  async function removePlayer(teamId: string, playerId: string) {
    await api.delete(`/teams/${teamId}/roster/${playerId}`)
    players.value = players.value.filter((p) => p.id !== playerId)
  }

  return { players, fetchRoster, addPlayer, updatePlayer, removePlayer }
})
