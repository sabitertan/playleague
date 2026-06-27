import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/api/client'

export type EventType = 'GAME' | 'PRACTICE' | 'OTHER'
export type RsvpStatus = 'GOING' | 'MAYBE' | 'NOT_GOING'

export interface GameEvent {
  id: string
  title: string
  type: EventType
  date: string
  location?: string | null
  teamId: string
  description?: string | null
  myRsvp?: RsvpStatus | null
  rsvpCounts?: { GOING: number; MAYBE: number; NOT_GOING: number }
}

export interface CreateEventData {
  title: string
  type: EventType
  date: string
  location?: string
  teamId: string
  description?: string
}

export const useEventsStore = defineStore('events', () => {
  const events = ref<GameEvent[]>([])
  const currentEvent = ref<GameEvent | null>(null)

  async function fetchEvents(teamId: string) {
    const { data } = await api.get<GameEvent[]>(`/events/team/${teamId}`)
    events.value = data
  }

  async function fetchEvent(id: string) {
    const { data } = await api.get<GameEvent>(`/events/${id}`)
    currentEvent.value = data
    return data
  }

  async function createEvent(eventData: CreateEventData) {
    const { data } = await api.post<GameEvent>('/events', eventData)
    events.value.push(data)
    return data
  }

  async function updateEvent(id: string, eventData: Partial<CreateEventData>) {
    const { data } = await api.put<GameEvent>(`/events/${id}`, eventData)
    const idx = events.value.findIndex((e) => e.id === id)
    if (idx !== -1) events.value[idx] = data
    if (currentEvent.value?.id === id) currentEvent.value = data
    return data
  }

  async function deleteEvent(id: string) {
    await api.delete(`/events/${id}`)
    events.value = events.value.filter((e) => e.id !== id)
    if (currentEvent.value?.id === id) currentEvent.value = null
  }

  async function updateRsvp(eventId: string, status: RsvpStatus) {
    const { data } = await api.put<GameEvent>(`/events/${eventId}/rsvp`, { status })
    const idx = events.value.findIndex((e) => e.id === eventId)
    if (idx !== -1) events.value[idx] = data
    return data
  }

  return {
    events,
    currentEvent,
    fetchEvents,
    fetchEvent,
    createEvent,
    updateEvent,
    deleteEvent,
    updateRsvp,
  }
})
