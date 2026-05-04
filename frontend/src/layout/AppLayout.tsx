import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

function NavItem({ to, label }: { to: string; label: string }) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        `block rounded px-3 py-2 text-sm ${isActive ? 'bg-white/15 text-white' : 'text-white/90 hover:bg-white/10'}`
      }
    >
      {label}
    </NavLink>
  )
}

export function AppLayout() {
  const { user, logout } = useAuth()

  return (
    <div className="flex h-full">
      <aside className="w-64 shrink-0 bg-navy text-white">
        <div className="p-4 border-b border-white/10">
          <div className="text-lg font-semibold">BlueCommand</div>
          <div className="text-xs text-white/80">Inspectorat Management</div>
        </div>
        <nav className="p-3 space-y-1">
          <NavItem to="/dashboard" label="Dashboard" />
          <NavItem to="/dosare" label="Dosare" />
          {(user?.rol === 'Administrator' || user?.rol === 'SefInspectorat') && (
            <>
              <NavItem to="/agenti" label="Agenti" />
              <NavItem to="/sectii" label="Sectii" />
              <NavItem to="/rapoarte" label="Rapoarte" />
            </>
          )}
          {user?.rol === 'Administrator' && (
            <>
              <NavItem to="/utilizatori" label="Utilizatori" />
              <NavItem to="/audit" label="Audit Log" />
            </>
          )}
          <NavItem to="/profil" label="Profil" />
        </nav>
      </aside>
      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex items-center justify-between border-b bg-white px-6 py-3">
          <div className="text-sm text-slate-600">
            {user ? (
              <>
                <span className="font-medium text-slate-900">
                  {user.nume ?? ''} {user.prenume ?? ''}
                </span>{' '}
                <span className="rounded bg-slate-100 px-2 py-1 text-xs">{user.rol}</span>
              </>
            ) : null}
          </div>
          <button
            onClick={logout}
            className="rounded bg-accent px-3 py-2 text-sm font-medium text-white hover:bg-blue-600"
          >
            Logout
          </button>
        </header>
        <main className="min-w-0 flex-1 overflow-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

