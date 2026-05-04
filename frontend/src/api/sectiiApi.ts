import { api } from './axios'

export interface Sectie {
  id: number
  nume: string
  adresa?: string | null
  zona?: string | null
  latitudine?: number | null
  longitudine?: number | null
  creatLa: string
}

export async function listSectii(search?: string) {
  const { data } = await api.get<Sectie[]>('/api/sectii', { params: { search } })
  return data
}

export interface CreateSectieRequest {
  nume: string
  adresa?: string
  zona?: string
  latitudine?: number
  longitudine?: number
}

export async function getSectie(id: number) {
  const { data } = await api.get(`/api/sectii/${id}`)
  return data as any
}

export async function createSectie(body: CreateSectieRequest) {
  const { data } = await api.post('/api/sectii', body)
  return data
}

export async function updateSectie(id: number, body: Partial<CreateSectieRequest>) {
  const { data } = await api.put(`/api/sectii/${id}`, body)
  return data
}

export async function deleteSectie(id: number) {
  await api.delete(`/api/sectii/${id}`)
}

export interface IstoricEntry {
  id: number
  campModificat: string
  valoareVeche?: string | null
  valoareNoua?: string | null
  modificatDe: number
  modificatLa: string
}

export async function getSectieIstoric(id: number) {
  const { data } = await api.get<IstoricEntry[]>(`/api/sectii/${id}/istoric`)
  return data
}

export async function getHartaSectii() {
  const { data } = await api.get<Sectie[]>('/api/sectii/harta')
  return data
}
