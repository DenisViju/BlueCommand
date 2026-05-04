import { api } from './axios'
import type { DashboardStats } from '../types/dashboard'

export async function getStats() {
  const { data } = await api.get<DashboardStats>('/api/dashboard/statistici')
  return data
}

