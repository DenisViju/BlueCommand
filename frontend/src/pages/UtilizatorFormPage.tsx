import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import * as utilizatoriApi from '../api/utilizatoriApi'
import * as sectiiApi from '../api/sectiiApi'

export function UtilizatorFormPage() {
  const navigate = useNavigate()
  const [sectii, setSectii] = useState<sectiiApi.Sectie[]>([])
  const [showPass, setShowPass] = useState(false)

  const [username, setUsername] = useState('')
  const [parola, setParola] = useState('')
  const [nume, setNume] = useState('')
  const [prenume, setPrenume] = useState('')
  const [grad, setGrad] = useState('')
  const [rolId, setRolId] = useState<number>(3)
  const [sectieId, setSectieId] = useState<number | ''>('')
  const [touched, setTouched] = useState(false)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    sectiiApi.listSectii().then(setSectii)
  }, [])

  const save = async () => {
    setTouched(true)
    if (!username.trim() || !parola || !nume.trim() || !prenume.trim()) return
    if (rolId === 3 && !sectieId) return

    setLoading(true)
    try {
      const created = await utilizatoriApi.createUtilizator({
        username: username.trim(),
        parola,
        nume: nume.trim(),
        prenume: prenume.trim(),
        grad: grad.trim() || undefined,
        rolId,
        sectieId: rolId === 3 ? Number(sectieId) : undefined,
      })
      toast.success('Utilizator creat')
      navigate(`/utilizatori/${created.id}`)
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-slate-900">Utilizator nou</h2>
        <div className="flex gap-2">
          <button className="rounded border px-3 py-2 text-sm" onClick={() => navigate('/utilizatori')}>
            Anuleaza
          </button>
          <button disabled={loading} className="rounded bg-navy px-3 py-2 text-sm text-white disabled:opacity-60" onClick={save}>
            Salveaza
          </button>
        </div>
      </div>

      <div className="rounded border bg-white p-4 shadow-sm grid gap-3 md:grid-cols-2">
        <div>
          <label className="block text-sm font-medium text-slate-700">Username *</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={username} onChange={(e) => setUsername(e.target.value)} />
          {touched && !username.trim() ? <div className="mt-1 text-xs text-red-600">Obligatoriu</div> : null}
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700">Parola *</label>
          <div className="mt-1 flex gap-2">
            <input type={showPass ? 'text' : 'password'} className="w-full rounded border px-3 py-2 text-sm" value={parola} onChange={(e) => setParola(e.target.value)} />
            <button type="button" className="rounded border px-3 py-2 text-sm" onClick={() => setShowPass((s) => !s)}>
              {showPass ? 'Ascunde' : 'Arata'}
            </button>
          </div>
          {touched && !parola ? <div className="mt-1 text-xs text-red-600">Obligatoriu</div> : null}
        </div>
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
        <div>
          <label className="block text-sm font-medium text-slate-700">Grad</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={grad} onChange={(e) => setGrad(e.target.value)} />
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700">Rol *</label>
          <select className="mt-1 w-full rounded border px-3 py-2 text-sm" value={rolId} onChange={(e) => setRolId(Number(e.target.value))}>
            <option value={1}>Administrator</option>
            <option value={2}>SefInspectorat</option>
            <option value={3}>AgentPolitie</option>
          </select>
        </div>
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-slate-700">Sectie {rolId === 3 ? '*' : ''}</label>
          <select className="mt-1 w-full rounded border px-3 py-2 text-sm" value={sectieId} onChange={(e) => setSectieId(e.target.value ? Number(e.target.value) : '')} disabled={rolId !== 3}>
            <option value="">Selecteaza...</option>
            {sectii.map((s) => (
              <option key={s.id} value={s.id}>
                {s.nume}
              </option>
            ))}
          </select>
          {touched && rolId === 3 && !sectieId ? <div className="mt-1 text-xs text-red-600">Obligatoriu pentru AgentPolitie</div> : null}
        </div>
      </div>
    </div>
  )
}

