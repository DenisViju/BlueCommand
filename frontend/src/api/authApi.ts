import { api } from './axios'
import type { LoginResponse } from '../types/auth'

export async function login(username: string, parola: string) {
  const { data } = await api.post<LoginResponse>('/api/auth/login', { username, parola })
  return data
}

export async function logout() {
  await api.post('/api/auth/logout')
}

export async function changePassword(parolaActuala: string, parolaNoua: string) {
  await api.put('/api/auth/schimba-parola', { parolaActuala, parolaNoua })
}

