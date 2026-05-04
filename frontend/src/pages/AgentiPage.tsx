import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import * as agentiApi from '../api/agentiApi'
import * as sectiiApi from '../api/sectiiApi'

function StatusBadge({ active }: { active: boolean }) {
  return (
    <span className={`rounded px-2 py-1 text-xs ${active ? 'bg-green-100 text-green-800' : 'bg-slate-100 text-slate-700'}`}>
      {active ? 'Activ' : 'Inactiv'}
    </span>
  )
}

export function AgentiPage() {
  const navigate = useNavigate()
  const [items, setItems] = useState<agentiApi.Agent[]>([])
  const [sectii, setSectii] = useState<sectiiApi.Sectie[]>([])
  const [search, setSearch] = useState('')
  const [sectieId, setSectieId] = useState<number | ''>('')
  const [loading, setLoading] = useState(false)

  const load = async () => {
    setLoading(true)
    try {
      const data = await agentiApi.listAgenti({
        search: search || undefined,
        sectieId: typeof sectieId === 'number' ? sectieId : undefined,
      })
      setItems(data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    sectiiApi.listSectii().then(setSectii)
    load().catch(() => toast.error('Nu pot incarca agentii'))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const onDelete = async (id: number) => {
    if (!confirm('Stergi agentul?')) return
    try {
      await agentiApi.deleteAgent(id)
      toast.success('Sters')
      await load()
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare la stergere')
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-slate-900">Agenti</h2>
        <button className="rounded bg-navy px-3 py-2 text-sm text-white" onClick={() => navigate('/agenti/nou')}>
          Adauga Agent
        </button>
      </div>

      <div className="flex flex-wrap items-center gap-2">
        <input
          className="rounded border px-3 py-2 text-sm"
          placeholder="Cauta dupa nume/username..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && load()}
        />
        <select className="rounded border px-3 py-2 text-sm" value={sectieId} onChange={(e) => setSectieId(e.target.value ? Number(e.target.value) : '')}>
          <option value="">Toate sectiile</option>
          {sectii.map((s) => (
            <option key={s.id} value={s.id}>
              {s.nume}
            </option>
          ))}
        </select>
        <button className="rounded border px-3 py-2 text-sm" onClick={load} disabled={loading}>
          Filtreaza
        </button>
      </div>

      <div className="rounded border bg-white shadow-sm overflow-auto">
        <table className="w-full text-sm">
          <thead className="bg-slate-50 text-slate-700">
            <tr>
              <th className="p-2 text-left">Nume complet</th>
              <th className="p-2 text-left">Grad</th>
              <th className="p-2 text-left">Sectie</th>
              <th className="p-2 text-left">Username</th>
              <th className="p-2 text-left">Status</th>
              <th className="p-2 text-left">Actiuni</th>
            </tr>
          </thead>
          <tbody>
            {items.map((a) => (
              <tr key={a.id} className="border-t">
                <td className="p-2">{`${a.prenume ?? ''} ${a.nume ?? ''}`.trim()}</td>
                <td className="p-2">{a.grad ?? '-'}</td>
                <td className="p-2">{a.sectieNume}</td>
                <td className="p-2">{a.username}</td>
                <td className="p-2">
                  <StatusBadge active={a.esteActiv} />
                </td>
                <td className="p-2">
                  <div className="flex gap-2">
                    <button className="rounded border px-2 py-1 text-xs" onClick={() => navigate(`/agenti/${a.id}`)}>
                      Detalii
                    </button>
                    <button className="rounded border px-2 py-1 text-xs" onClick={() => onDelete(a.id)}>
                      Sterge
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {items.length === 0 ? (
              <tr>
                <td colSpan={6} className="p-3 text-sm text-slate-600">
                  Niciun agent.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </div>
  )
}
