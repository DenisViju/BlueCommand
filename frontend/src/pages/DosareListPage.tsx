import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import * as dosareApi from '../api/dosareApi'
import { useAuth } from '../context/AuthContext'

function StatusBadge({ status }: { status: string }) {
  const cls =
    status === 'DESCHIS'
      ? 'bg-green-100 text-green-800'
      : status === 'IN_LUCRU'
        ? 'bg-amber-100 text-amber-800'
        : 'bg-slate-100 text-slate-700'
  return <span className={`rounded px-2 py-1 text-xs ${cls}`}>{status}</span>
}

export function DosareListPage() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<string>('')
  const [data, setData] = useState<dosareApi.PagedResult<dosareApi.DosarListItem> | null>(null)

  useEffect(() => {
    dosareApi.listDosare({ page, search: search || undefined, status: status || undefined }).then(setData)
  }, [page, search, status])

  if (!data) return <div>Se incarca...</div>

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-slate-900">Dosare</h2>
        {user?.rol !== 'Administrator' ? (
          <button className="rounded bg-navy px-3 py-2 text-sm text-white" onClick={() => navigate('/dosare/nou')}>
            Dosar Nou
          </button>
        ) : null}
      </div>

      <div className="flex flex-wrap items-center gap-2">
        <input
          className="rounded border px-3 py-2 text-sm"
          placeholder="Cauta numar sau titlu..."
          value={search}
          onChange={(e) => {
            setPage(1)
            setSearch(e.target.value)
          }}
        />
        <select
          className="rounded border px-3 py-2 text-sm"
          value={status}
          onChange={(e) => {
            setPage(1)
            setStatus(e.target.value)
          }}
        >
          <option value="">Toate</option>
          <option value="DESCHIS">DESCHIS</option>
          <option value="IN_LUCRU">IN_LUCRU</option>
          <option value="INCHIS">INCHIS</option>
        </select>
      </div>
      <div className="rounded border bg-white shadow-sm overflow-auto">
        <table className="w-full text-sm">
          <thead className="bg-slate-50 text-slate-700">
            <tr>
              <th className="p-2 text-left">Numar</th>
              <th className="p-2 text-left">Titlu</th>
              <th className="p-2 text-left">Tip</th>
              <th className="p-2 text-left">Sectie</th>
              <th className="p-2 text-left">Status</th>
            </tr>
          </thead>
          <tbody>
            {data.items.map((d) => (
              <tr key={d.id} className="border-t">
                <td className="p-2">
                  <Link className="text-blue-600 hover:underline" to={`/dosare/${d.id}`}>
                    {d.numarDosar}
                  </Link>
                </td>
                <td className="p-2">{d.titlu ?? '-'}</td>
                <td className="p-2">{d.tipIncident ?? '-'}</td>
                <td className="p-2">{d.sectieNume}</td>
                <td className="p-2">
                  <StatusBadge status={d.status} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <div className="flex items-center gap-2 text-sm">
        <button className="rounded border px-2 py-1 disabled:opacity-50" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
          Prev
        </button>
        <span>
          Pagina {data.page} / {Math.max(1, Math.ceil(data.total / data.pageSize))}
        </span>
        <button
          className="rounded border px-2 py-1 disabled:opacity-50"
          disabled={page >= Math.ceil(data.total / data.pageSize)}
          onClick={() => setPage((p) => p + 1)}
        >
          Next
        </button>
      </div>
    </div>
  )
}
