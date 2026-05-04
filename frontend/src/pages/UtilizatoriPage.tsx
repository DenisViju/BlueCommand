import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import * as utilizatoriApi from '../api/utilizatoriApi'

function RolBadge({ rol }: { rol: string }) {
  const cls =
    rol === 'Administrator'
      ? 'bg-purple-100 text-purple-800'
      : rol === 'SefInspectorat'
        ? 'bg-blue-100 text-blue-800'
        : 'bg-slate-100 text-slate-700'
  return <span className={`rounded px-2 py-1 text-xs ${cls}`}>{rol}</span>
}

export function UtilizatoriPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [rol, setRol] = useState('')
  const [page, setPage] = useState(1)
  const [data, setData] = useState<{ items: utilizatoriApi.Utilizator[]; total: number; page: number; pageSize: number } | null>(null)
  const [loading, setLoading] = useState(false)

  const load = async () => {
    setLoading(true)
    try {
      const res = await utilizatoriApi.listUtilizatori({ search: search || undefined, rol: rol || undefined, page })
      setData(res)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load().catch(() => toast.error('Nu pot incarca utilizatorii'))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page])

  const deactivate = async (id: number) => {
    if (!confirm('Dezactivezi utilizatorul?')) return
    try {
      await utilizatoriApi.deactivateUser(id)
      toast.success('Dezactivat')
      await load()
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare')
    }
  }

  if (!data) return <div>Se incarca...</div>

  const pages = Math.max(1, Math.ceil(data.total / data.pageSize))

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-slate-900">Utilizatori</h2>
        <button className="rounded bg-navy px-3 py-2 text-sm text-white" onClick={() => navigate('/utilizatori/nou')}>
          Adauga Utilizator
        </button>
      </div>

      <div className="flex flex-wrap items-center gap-2">
        <input
          className="rounded border px-3 py-2 text-sm"
          placeholder="Cauta username/nume..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && load()}
        />
        <select className="rounded border px-3 py-2 text-sm" value={rol} onChange={(e) => setRol(e.target.value)}>
          <option value="">Toate rolurile</option>
          <option value="Administrator">Administrator</option>
          <option value="SefInspectorat">SefInspectorat</option>
          <option value="AgentPolitie">AgentPolitie</option>
        </select>
        <button className="rounded border px-3 py-2 text-sm" onClick={() => { setPage(1); load() }} disabled={loading}>
          Filtreaza
        </button>
      </div>

      <div className="rounded border bg-white shadow-sm overflow-auto">
        <table className="w-full text-sm">
          <thead className="bg-slate-50 text-slate-700">
            <tr>
              <th className="p-2 text-left">Username</th>
              <th className="p-2 text-left">Nume complet</th>
              <th className="p-2 text-left">Rol</th>
              <th className="p-2 text-left">Sectie</th>
              <th className="p-2 text-left">Status</th>
              <th className="p-2 text-left">Data creare</th>
              <th className="p-2 text-left">Actiuni</th>
            </tr>
          </thead>
          <tbody>
            {data.items.map((u) => (
              <tr key={u.id} className="border-t">
                <td className="p-2">{u.username}</td>
                <td className="p-2">{`${u.nume ?? ''} ${u.prenume ?? ''}`.trim()}</td>
                <td className="p-2">
                  <RolBadge rol={u.rol} />
                </td>
                <td className="p-2">{u.sectieNume ?? '-'}</td>
                <td className="p-2">{u.esteActiv ? 'Activ' : 'Inactiv'}</td>
                <td className="p-2">{u.dataCreare}</td>
                <td className="p-2">
                  <div className="flex gap-2">
                    <button className="rounded border px-2 py-1 text-xs" onClick={() => navigate(`/utilizatori/${u.id}`)}>
                      Detalii
                    </button>
                    <button className="rounded border px-2 py-1 text-xs" onClick={() => deactivate(u.id)}>
                      Dezactiveaza
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {data.items.length === 0 ? (
              <tr>
                <td colSpan={7} className="p-3 text-sm text-slate-600">
                  Niciun utilizator.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>

      <div className="flex items-center gap-2 text-sm">
        <button className="rounded border px-2 py-1 disabled:opacity-50" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
          Prev
        </button>
        <span>
          Pagina {page} / {pages}
        </span>
        <button className="rounded border px-2 py-1 disabled:opacity-50" disabled={page >= pages} onClick={() => setPage((p) => p + 1)}>
          Next
        </button>
      </div>
    </div>
  )
}
