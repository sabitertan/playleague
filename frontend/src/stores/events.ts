import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/api/client'

export type EventType = 'GAME' | 'PRACTICE' | 'OTHER'
export type RsvpStatus = 'GOING' | 'MAYBE' | 'NOT_GOING'

// Shape returned by the backend (EventDto / EventDetailDto)
interface EventDto {
  id: string
  title: string
  type: EventType
  startAt: string
  endAt?: string | null
  location?: string | null
  opponent?: string | null
  notes?: string | null
  venueId?: string | null
  rsvpGoing: number
  rsvpNotGoing: number
  rsvpMaybe: number
  userRsvp?: RsvpStatus | 'NO_RESPONSE' | null
}

export interface GameEvent {
  id: string
  title: string
  type: EventType
  date: string
  location?: string | null
  teamId?: string
  description?: string | null
  myRsvp?: RsvpStatus | null
  rsvpCounts: { GOING: number; MAYBE: number; NOT_GOING: number }
}

export interface CreateEventData {
  title: string
  type: EventType
  date: string
  location?: string
  teamId: string
  description?: string
}

function mapEvent(dto: EventDto, teamId?: string): GameEvent {
  return {
    id: dto.id,
    title: dto.title,
    type: dto.type,
    date: dto.startAt,
    location: dto.location,
    teamId,
    description: dto.notes,
    myRsvp: dto.userRsvp && dto.userRsvp !== 'NO_RESPONSE' ? dto.userRsvp : null,
    rsvpCounts: { GOING: dto.rsvpGoing, MAYBE: dto.rsvpMaybe, NOT_GOING: dto.rsvpNotGoing },
  }
}

export const useEventsStore = defineStore('events', () => {
  const events = ref<GameEvent[]>([])
  const currentEvent = ref<GameEvent | null>(null)
  const currentTeamId = ref<string | null>(null)

  async function fetchEvents(teamId: string) {
    currentTeamId.value = teamId
    const { data } = await api.get<EventDto[]>(`/events/team/${teamId}`)
    events.value = data.map((e) => mapEvent(e, teamId))
  }

  async function fetchEvent(id: string) {
    const { data } = await api.get<EventDto>(`/events/${id}`)
    currentEvent.value = mapEvent(data)
    return currentEvent.value
  }

  async function createEvent(eventData: CreateEventData) {
    await api.post('/events', {
      teamId: eventData.teamId,
      title: eventData.title,
      type: eventData.type,
      startAt: new Date(eventData.date).toISOString(),
      location: eventData.location || null,
      notes: eventData.description || null,
    })
    // POST returns only the new id; refetch to get the full event with RSVP counts
    await fetchEvents(eventData.teamId)
  }

  async function updateEvent(id: string, eventData: Partial<CreateEventData>) {
    await api.put(`/events/${id}`, {
      title: eventData.title,
      type: eventData.type,
      startAt: eventData.date ? new Date(eventData.date).toISOString() : undefined,
      location: eventData.location || null,
      notes: eventData.description || null,
    })
    if (currentTeamId.value) await fetchEvents(currentTeamId.value)
  }

  async function deleteEvent(id: string) {
    await api.delete(`/events/${id}`)
    events.value = events.value.filter((e) => e.id !== id)
    if (currentEvent.value?.id === id) currentEvent.value = null
  }

  async function updateRsvp(eventId: string, status: RsvpStatus) {
    await api.put(`/events/${eventId}/rsvp`, { status })
    // Update local counts optimistically
    const event = events.value.find((e) => e.id === eventId)
    if (event) {
      if (event.myRsvp) event.rsvpCounts[event.myRsvp]--
      event.rsvpCounts[status]++
      event.myRsvp = status
    }
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
