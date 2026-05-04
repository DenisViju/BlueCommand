import { Navigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import { useAuth } from '../context/AuthContext'
import type { Role } from '../types/auth'

export function ProtectedRoute({
  children,
  roles,
}: {
  children: React.ReactNode
  roles?: Role[]
}) {
  const { isAuthenticated, user } = useAuth()

  if (!isAuthenticated) return <Navigate to="/login" replace />

  if (roles && user && !roles.includes(user.rol)) {
    toast.error('Acces interzis')
    return <Navigate to="/dashboard" replace />
  }

  return <>{children}</>
}

