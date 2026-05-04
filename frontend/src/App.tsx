import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { AuthProvider } from './context/AuthContext'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AppLayout } from './layout/AppLayout'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { ProfilPage } from './pages/ProfilPage'
import { SectiiPage } from './pages/SectiiPage'
import { SectieDetailPage } from './pages/SectieDetailPage'
import { SectieFormPage } from './pages/SectieFormPage'
import { AgentiPage } from './pages/AgentiPage'
import { AgentDetailPage } from './pages/AgentDetailPage'
import { AgentFormPage } from './pages/AgentFormPage'
import { DosareListPage } from './pages/DosareListPage'
import { DosarFormPage } from './pages/DosarFormPage'
import { DosarDetailPage } from './pages/DosarDetailPage'
import { RapoartePage } from './pages/RapoartePage'
import { UtilizatoriPage } from './pages/UtilizatoriPage'
import { UtilizatorDetailPage } from './pages/UtilizatorDetailPage'
import { UtilizatorFormPage } from './pages/UtilizatorFormPage'
import { AuditPage } from './pages/AuditPage'

const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  { path: '/', element: <Navigate to="/dashboard" replace /> },
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <AppLayout />
      </ProtectedRoute>
    ),
    children: [
      { path: '/dashboard', element: <DashboardPage /> },
      { path: '/profil', element: <ProfilPage /> },
      {
        path: '/sectii',
        element: (
          <ProtectedRoute roles={['Administrator', 'SefInspectorat']}>
            <SectiiPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/sectii/nou',
        element: (
          <ProtectedRoute roles={['Administrator', 'SefInspectorat']}>
            <SectieFormPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/sectii/:id',
        element: (
          <ProtectedRoute roles={['Administrator', 'SefInspectorat']}>
            <SectieDetailPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/sectii/:id/edit',
        element: (
          <ProtectedRoute roles={['Administrator', 'SefInspectorat']}>
            <SectieFormPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/agenti',
        element: (
          <ProtectedRoute roles={['Administrator', 'SefInspectorat']}>
            <AgentiPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/agenti/nou',
        element: (
          <ProtectedRoute roles={['Administrator', 'SefInspectorat']}>
            <AgentFormPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/agenti/:id',
        element: (
          <ProtectedRoute roles={['Administrator', 'SefInspectorat']}>
            <AgentDetailPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/agenti/:id/edit',
        element: (
          <ProtectedRoute roles={['Administrator', 'SefInspectorat']}>
            <AgentFormPage />
          </ProtectedRoute>
        ),
      },
      { path: '/dosare', element: <DosareListPage /> },
      {
        path: '/dosare/nou',
        element: (
          <ProtectedRoute roles={['Administrator', 'SefInspectorat', 'AgentPolitie']}>
            <DosarFormPage />
          </ProtectedRoute>
        ),
      },
      { path: '/dosare/:id', element: <DosarDetailPage /> },
      {
        path: '/rapoarte',
        element: (
          <ProtectedRoute roles={['Administrator', 'SefInspectorat']}>
            <RapoartePage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/utilizatori',
        element: (
          <ProtectedRoute roles={['Administrator']}>
            <UtilizatoriPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/utilizatori/nou',
        element: (
          <ProtectedRoute roles={['Administrator']}>
            <UtilizatorFormPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/utilizatori/:id',
        element: (
          <ProtectedRoute roles={['Administrator']}>
            <UtilizatorDetailPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/audit',
        element: (
          <ProtectedRoute roles={['Administrator']}>
            <AuditPage />
          </ProtectedRoute>
        ),
      },
    ],
  },
])

export default function App() {
  return (
    <AuthProvider>
      <RouterProvider router={router} />
      <Toaster position="top-right" />
    </AuthProvider>
  )
}
