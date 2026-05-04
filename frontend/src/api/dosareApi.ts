import { api } from './axios'

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface DosarListItem {
  id: number
  numarDosar: string
  titlu?: string | null
  tipIncident?: string | null
  dataIncident?: string | null
  status: string
  sectieId: number
  sectieNume: string
  creatLa: string
}

export interface DosarDetail {
  id: number
  numarDosar: string
  titlu?: string | null
  descriere?: string | null
  status: string
  tipIncident?: string | null
  dataIncident?: string | null
  sectieId: number
  sectieNume: string
  creatDe: number
  creatLa: string
  actualizatLa?: string | null
  agenti: Array<{ id: number; username: string; nume?: string | null; prenume?: string | null; grad?: string | null }>
  documente: Array<{ id: number; numeFisier: string; caleFisier: string; marimeBytes?: number | null; dataIncarcare: string }>
  istoric: Array<{ id: number; campModificat: string; valoareVeche?: string | null; valoareNoua?: string | null; modificatLa: string }>
}

export async function listDosare(params?: Record<string, any>) {
  const { data } = await api.get<PagedResult<DosarListItem>>('/api/dosare', { params })
  return data
}

export async function getDosar(id: number) {
  const { data } = await api.get<DosarDetail>(`/api/dosare/${id}`)
  return data
}

export async function exportDosar(id: number, format: 'pdf' | 'excel') {
  const res = await api.get(`/api/dosare/${id}/export`, { params: { format }, responseType: 'blob' })
  return res.data as Blob
}

export async function uploadDocument(dosarId: number, file: File) {
  const form = new FormData()
  form.append('file', file)
  const { data } = await api.post(`/api/dosare/${dosarId}/documente`, form, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return data
}

export async function deleteDocument(dosarId: number, documentId: number) {
  await api.delete(`/api/dosare/${dosarId}/documente/${documentId}`)
}

export interface CreateDosarRequest {
  numarDosar: string
  titlu?: string
  descriere?: string
  tipIncident?: string
  dataIncident?: string
  sectieId: number
  agentiIds: number[]
}

export interface UpdateDosarRequest {
  titlu?: string
  descriere?: string
  tipIncident?: string
  dataIncident?: string
  status?: string
}

export async function createDosar(body: CreateDosarRequest) {
  const { data } = await api.post<DosarDetail>('/api/dosare', body)
  return data
}

export async function updateDosar(id: number, body: UpdateDosarRequest) {
  const { data } = await api.put<DosarDetail>(`/api/dosare/${id}`, body)
  return data
}

export async function schimbaStatus(id: number, status: string) {
  const { data } = await api.put<DosarDetail>(`/api/dosare/${id}`, { status })
  return data
}

export async function closeDosar(id: number) {
  const { data } = await api.put(`/api/dosare/${id}/inchide`)
  return data
}

export async function redeschideDosar(id: number) {
  const { data } = await api.put(`/api/dosare/${id}/redeschide`)
  return data
}

export async function updateDosarAgenti(id: number, agentiIds: number[]) {
  const { data } = await api.put(`/api/dosare/${id}/agenti`, { agentiIds })
  return data
}
