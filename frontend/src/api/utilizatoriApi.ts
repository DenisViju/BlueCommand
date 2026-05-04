import { api } from './axios'

export interface Utilizator {
  id: number
  username: string
  nume?: string | null
  prenume?: string | null
  grad?: string | null
  rol: string
  sectieId?: number | null
  sectieNume?: string | null
  dataCreare: string
  esteActiv: boolean
}

export async function listUtilizatori(params?: { search?: string; rol?: string; page?: number }) {
  const { data } = await api.get('/api/utilizatori', { params })
  return data as { items: Utilizator[]; total: number; page: number; pageSize: number }
}

export async function profil() {
  const { data } = await api.get<Utilizator>('/api/utilizatori/profil')
  return data
}

export interface CreateUtilizatorRequest {
  username: string
  parola: string
  nume: string
  prenume: string
  grad?: string
  rolId: number
  sectieId?: number
}

export interface UpdateUtilizatorRequest {
  nume?: string
  prenume?: string
  grad?: string
  rolId?: number
  sectieId?: number | null
  esteActiv?: boolean
}

export async function getUtilizator(id: number) {
  const { data } = await api.get<Utilizator>(`/api/utilizatori/${id}`)
  return data
}

export async function createUtilizator(body: CreateUtilizatorRequest) {
  const { data } = await api.post<Utilizator>('/api/utilizatori', body)
  return data
}

export async function updateUtilizator(id: number, body: Partial<CreateUtilizatorRequest>) {
  const { data } = await api.put<Utilizator>(`/api/utilizatori/${id}`, body)
  return data
}

export async function updateUtilizatorAdmin(id: number, body: UpdateUtilizatorRequest) {
  const { data } = await api.put<Utilizator>(`/api/utilizatori/${id}`, body)
  return data
}

export async function resetPassword(id: number, parolaNoua: string) {
  await api.put(`/api/utilizatori/${id}/reseteaza-parola`, { parolaNoua })
}

export async function deactivateUser(id: number) {
  await api.delete(`/api/utilizatori/${id}`)
}
