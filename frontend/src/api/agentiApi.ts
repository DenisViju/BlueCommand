import { api } from './axios'

export interface Agent {
  id: number
  username: string
  nume?: string | null
  prenume?: string | null
  grad?: string | null
  sectieId: number
  sectieNume: string
  esteActiv: boolean
}

export async function listAgenti(params?: { search?: string; sectieId?: number; grad?: string }) {
  const { data } = await api.get<Agent[]>('/api/agenti', { params })
  return data
}

export interface CreateAgentRequest {
  username: string
  parola: string
  nume: string
  prenume: string
  grad: string
  sectieId: number
}

export interface UpdateAgentRequest {
  nume?: string
  prenume?: string
  grad?: string
  sectieId?: number
}

export async function getAgent(id: number) {
  const { data } = await api.get(`/api/agenti/${id}`)
  return data as any
}

export async function createAgent(body: CreateAgentRequest) {
  const { data } = await api.post('/api/agenti', body)
  return data
}

export async function updateAgent(id: number, body: UpdateAgentRequest) {
  const { data } = await api.put(`/api/agenti/${id}`, body)
  return data
}

export async function deleteAgent(id: number) {
  await api.delete(`/api/agenti/${id}`)
}

export async function getAgentIstoric(id: number) {
  const { data } = await api.get<
    Array<{
      id: number
      campModificat: string
      valoareVeche?: string | null
      valoareNoua?: string | null
      modificatLa: string
    }>
  >(`/api/agenti/${id}/istoric`)
  return data
}
