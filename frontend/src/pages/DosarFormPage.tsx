import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import * as dosareApi from '../api/dosareApi'
import * as sectiiApi from '../api/sectiiApi'
import * as agentiApi from '../api/agentiApi'

const tipuri = ['Furt', 'Tâlhărie', 'Vandalism', 'Agresiune', 'Altele'] as const

export function DosarFormPage() {
  const navigate = useNavigate()
  const [sectii, setSectii] = useState<sectiiApi.Sectie[]>([])
  const [agenti, setAgenti] = useState<agentiApi.Agent[]>([])
  const [loading, setLoading] = useState(false)
  const [touched, setTouched] = useState(false)

  const [numarDosar, setNumarDosar] = useState('')
  const [titlu, setTitlu] = useState('')
  const [tipIncident, setTipIncident] = useState('')
  const [dataIncident, setDataIncident] = useState('')
  const [sectieId, setSectieId] = useState<number | ''>('')
  const [descriere, setDescriere] = useState('')
  const [selectedAgenti, setSelectedAgenti] = useState<number[]>([])

  useEffect(() => {
    sectiiApi.listSectii().then(setSectii)
  }, [])

  useEffect(() => {
    if (!sectieId) {
      setAgenti([])
      setSelectedAgenti([])
      return
    }
    agentiApi
      .listAgenti({ sectieId: Number(sectieId) })
      .then(setAgenti)
      .catch(() => toast.error('Nu pot incarca agentii'))
  }, [sectieId])

  const valid = useMemo(() => {
    return !!numarDosar.trim() && !!titlu.trim() && !!sectieId && selectedAgenti.length > 0
  }, [numarDosar, titlu, sectieId, selectedAgenti])

  const save = async () => {
    setTouched(true)
    if (!valid) return
    setLoading(true)
    try {
      const created = await dosareApi.createDosar({
        numarDosar: numarDosar.trim(),
        titlu: titlu.trim(),
        descriere: descriere.trim() || undefined,
        tipIncident: tipIncident.trim() || undefined,
        dataIncident: dataIncident || undefined,
        sectieId: Number(sectieId),
        agentiIds: selectedAgenti,
      })
      toast.success('Dosar creat')
      navigate(`/dosare/${created.id}`)
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-slate-900">Dosar nou</h2>
        <div className="flex gap-2">
          <button className="rounded border px-3 py-2 text-sm" onClick={() => navigate('/dosare')}>
            Anuleaza
          </button>
          <button disabled={loading} className="rounded bg-navy px-3 py-2 text-sm text-white disabled:opacity-60" onClick={save}>
            Salveaza
          </button>
        </div>
      </div>

      <div className="rounded border bg-white p-4 shadow-sm grid gap-3 md:grid-cols-2">
        <div>
          <label className="block text-sm font-medium text-slate-700">Numar Dosar *</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={numarDosar} onChange={(e) => setNumarDosar(e.target.value)} />
          {touched && !numarDosar.trim() ? <div className="mt-1 text-xs text-red-600">Obligatoriu</div> : null}
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700">Titlu *</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={titlu} onChange={(e) => setTitlu(e.target.value)} />
          {touched && !titlu.trim() ? <div className="mt-1 text-xs text-red-600">Obligatoriu</div> : null}
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700">Tip Incident</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" list="tipuri" value={tipIncident} onChange={(e) => setTipIncident(e.target.value)} />
          <datalist id="tipuri">
            {tipuri.map((t) => (
              <option key={t} value={t} />
            ))}
          </datalist>
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700">Data Incident</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" type="date" value={dataIncident} onChange={(e) => setDataIncident(e.target.value)} />
        </div>
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-slate-700">Sectie *</label>
          <select className="mt-1 w-full rounded border px-3 py-2 text-sm" value={sectieId} onChange={(e) => setSectieId(e.target.value ? Number(e.target.value) : '')}>
            <option value="">Selecteaza...</option>
            {sectii.map((s) => (
              <option key={s.id} value={s.id}>
                {s.nume}
              </option>
            ))}
          </select>
          {touched && !sectieId ? <div className="mt-1 text-xs text-red-600">Obligatoriu</div> : null}
        </div>
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-slate-700">Descriere</label>
          <textarea className="mt-1 w-full rounded border px-3 py-2 text-sm" rows={4} value={descriere} onChange={(e) => setDescriere(e.target.value)} />
        </div>
      </div>

      <div className="rounded border bg-white p-4 shadow-sm">
        <div className="text-sm font-medium text-slate-700">Asignare agenti *</div>
        {agenti.length === 0 ? <div className="mt-2 text-sm text-slate-600">Selecteaza o sectie pentru a incarca agentii.</div> : null}
        <div className="mt-3 grid gap-2 md:grid-cols-2">
          {agenti.map((a) => {
            const checked = selectedAgenti.includes(a.id)
            return (
              <label key={a.id} className="flex items-center gap-2 rounded border px-3 py-2 text-sm">
                <input
                  type="checkbox"
                  checked={checked}
                  onChange={(e) => {
                    setSelectedAgenti((prev) => (e.target.checked ? [...prev, a.id] : prev.filter((x) => x !== a.id)))
                  }}
                />
                <span>
                  {(a.nume ?? '') + ' ' + (a.prenume ?? '')} ({a.username})
                </span>
              </label>
            )
          })}
        </div>
        {touched && selectedAgenti.length === 0 ? <div className="mt-2 text-xs text-red-600">Selecteaza cel putin un agent</div> : null}
      </div>
    </div>
  )
}

