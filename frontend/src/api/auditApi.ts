import { api } from './axios'

export async function listAudit(params?: { utilizatorId?: number; dataStart?: string; dataEnd?: string; page?: number }) {
  const { data } = await api.get('/api/audit', { params })
  return data as { items: any[]; total: number; page: number; pageSize: number }
}

