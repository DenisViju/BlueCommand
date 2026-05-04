import { api } from './axios'

export async function genereazaRaport(body: { tip: string; dataStart: string; dataEnd: string; sectieId?: number }) {
  const { data } = await api.post('/api/rapoarte/genereaza', body)
  return data
}

export async function exportRaport(body: { tip: string; dataStart: string; dataEnd: string; sectieId?: number; format: 'pdf' | 'excel' }) {
  const res = await api.post('/api/rapoarte/export', body, { responseType: 'blob' })
  return res.data as Blob
}

export async function listRapoarte() {
  const { data } = await api.get<any[]>('/api/rapoarte')
  return data
}
