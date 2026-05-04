import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import toast from 'react-hot-toast'
import * as agentiApi from '../api/agentiApi'
import * as sectiiApi from '../api/sectiiApi'

export function AgentFormPage() {
  const { id } = useParams()
  const editId = id ? Number(id) : null
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [sectii, setSectii] = useState<sectiiApi.Sectie[]>([])

  const [nume, setNume] = useState('')
  const [prenume, setPrenume] = useState('')
  const [username, setUsername] = useState('')
  const [parola, setParola] = useState('')
  const [grad, setGrad] = useState('')
  const [sectieId, setSectieId] = useState<number | ''>('')
  const [touched, setTouched] = useState(false)

  useEffect(() => {
    sectiiApi.listSectii().then(setSectii)
  }, [])

  useEffect(() => {
    if (!editId) return
    setLoading(true)
    agentiApi
      .getAgent(editId)
      .then((res) => {
        const a = res.agent ?? res
        setNume(a.nume ?? '')
        setPrenume(a.prenume ?? '')
        setUsername(a.username ?? '')
        setGrad(a.grad ?? '')
        setSectieId(a.sectieId ?? '')
      })
      .catch(() => toast.error('Nu pot incarca agentul'))
      .finally(() => setLoading(false))
  }, [editId])

  const save = async () => {
    setTouched(true)
    if (!nume.trim() || !prenume.trim() || !grad.trim() || !sectieId) return
    if (!editId && (!username.trim() || !parola)) return

    setLoading(true)
    try {
      if (editId) {
        await agentiApi.updateAgent(editId, {
          nume: nume.trim(),
          prenume: prenume.trim(),
          grad: grad.trim(),
          sectieId: typeof sectieId === 'number' ? sectieId : undefined,
        })
        toast.success('Agent actualizat')
        navigate(`/agenti/${editId}`)
      } else {
        const res = await agentiApi.createAgent({
          username: username.trim(),
          parola,
          nume: nume.trim(),
          prenume: prenume.trim(),
          grad: grad.trim(),
          sectieId: Number(sectieId),
        })
        const createdId = res?.id ?? res?.Id ?? res?.agent?.id
        toast.success('Agent creat')
        navigate(createdId ? `/agenti/${createdId}` : '/agenti')
      }
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare la salvare')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-slate-900">{editId ? 'Editeaza agent' : 'Agent nou'}</h2>
        <div className="flex gap-2">
          <button className="rounded border px-3 py-2 text-sm" onClick={() => navigate('/agenti')}>
            Anuleaza
          </button>
          <button disabled={loading} className="rounded bg-navy px-3 py-2 text-sm text-white disabled:opacity-60" onClick={save}>
            Salveaza
          </button>
        </div>
      </div>

      <div className="rounded border bg-white p-4 shadow-sm grid gap-3 md:grid-cols-2">
        <div>
          <label className="block text-sm font-medium text-slate-700">Nume *</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={nume} onChange={(e) => setNume(e.target.value)} />
          {touched && !nume.trim() ? <div className="mt-1 text-xs text-red-600">Obligatoriu</div> : null}
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700">Prenume *</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={prenume} onChange={(e) => setPrenume(e.target.value)} />
          {touched && !prenume.trim() ? <div className="mt-1 text-xs text-red-600">Obligatoriu</div> : null}
        </div>
        {!editId ? (
          <>
            <div>
              <label className="block text-sm font-medium text-slate-700">Username *</label>
              <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={username} onChange={(e) => setUsername(e.target.value)} />
              {touched && !username.trim() ? <div className="mt-1 text-xs text-red-600">Obligatoriu</div> : null}
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700">Parola *</label>
              <input type="password" className="mt-1 w-full rounded border px-3 py-2 text-sm" value={parola} onChange={(e) => setParola(e.target.value)} />
              {touched && !parola ? <div className="mt-1 text-xs text-red-600">Obligatoriu</div> : null}
            </div>
          </>
        ) : null}
        <div>
          <label className="block text-sm font-medium text-slate-700">Grad *</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={grad} onChange={(e) => setGrad(e.target.value)} />
          {touched && !grad.trim() ? <div className="mt-1 text-xs text-red-600">Obligatoriu</div> : null}
        </div>
        <div>
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
      </div>
    </div>
  )
}

