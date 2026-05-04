import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import toast from 'react-hot-toast'
import { jwtDecode } from 'jwt-decode'
import * as authApi from '../api/authApi'
import type { AuthUser, Role } from '../types/auth'

interface AuthContextType {
  user: AuthUser | null
  token: string | null
  isAuthenticated: boolean
  login: (username: string, parola: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

function mapRole(role: string): Role {
  if (role === 'Administrator' || role === 'SefInspectorat' || role === 'AgentPolitie') return role
  return 'AgentPolitie'
}

interface JwtPayload {
  userId: string
  username: string
  role?: string
  sectieId?: string
  exp: number
  // ClaimTypes.Role default key
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(localStorage.getItem('bluecommand_token'))
  const [user, setUser] = useState<AuthUser | null>(null)

  useEffect(() => {
    if (!token) {
      setUser(null)
      return
    }
    try {
      const decoded = jwtDecode<JwtPayload>(token)
      if (decoded.exp * 1000 < Date.now()) {
        localStorage.removeItem('bluecommand_token')
        setToken(null)
        setUser(null)
        return
      }
      const role = decoded.role ?? decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? ''
      setUser({
        id: Number(decoded.userId),
        username: decoded.username,
        nume: null,
        prenume: null,
        rol: mapRole(role),
        sectieId: decoded.sectieId ? Number(decoded.sectieId) : null,
      })
    } catch {
      localStorage.removeItem('bluecommand_token')
      setToken(null)
      setUser(null)
    }
  }, [token])

  const login = async (username: string, parola: string) => {
    try {
      const res = await authApi.login(username, parola)
      localStorage.setItem('bluecommand_token', res.token)
      setToken(res.token)
      setUser({
        id: res.utilizator.id,
        username: res.utilizator.username,
        nume: res.utilizator.nume ?? null,
        prenume: res.utilizator.prenume ?? null,
        rol: mapRole(res.utilizator.rol),
        sectieId: res.utilizator.sectieId ?? null,
      })
      toast.success('Autentificare reusita')
    } catch (e: any) {
      const msg = e?.response?.data?.error ?? 'Autentificare esuata'
      toast.error(msg)
      throw e
    }
  }

  const logout = () => {
    localStorage.removeItem('bluecommand_token')
    setToken(null)
    setUser(null)
    toast('Delogat')
    window.location.href = '/login'
  }

  const value = useMemo(
    () => ({ user, token, isAuthenticated: !!token, login, logout }),
    [user, token],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('AuthContext missing')
  return ctx
}
