export type Role = 'Administrator' | 'SefInspectorat' | 'AgentPolitie'

export interface AuthUser {
  id: number
  username: string
  nume?: string | null
  prenume?: string | null
  rol: Role
  sectieId?: number | null
}

export interface LoginResponse {
  token: string
  utilizator: {
    id: number
    username: string
    nume?: string | null
    prenume?: string | null
    rol: Role
    sectieId?: number | null
  }
}
