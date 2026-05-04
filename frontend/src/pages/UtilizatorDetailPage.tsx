import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import toast from 'react-hot-toast'
import * as utilizatoriApi from '../api/utilizatoriApi'
import * as sectiiApi from '../api/sectiiApi'

export function UtilizatorDetailPage() {
  const { id } = useParams()
  const userId = Number(id)
  const navigate = useNavigate()
  const [user, setUser] = useState<utilizatoriApi.Utilizator | null>(null)
  const [newPass, setNewPass] = useState('')
  const [saving, setSaving] = useState(false)
  const [editing, setEditing] = useState(false)
  const [sectii, setSectii] = useState<sectiiApi.Sectie[]>([])
  const [form, setForm] = useState({
    nume: '',
    prenume: '',
    grad: '',
    rol: 'AgentPolitie',
    sectieId: '' as number | '',
  })

  const reload = () => utilizatoriApi.getUtilizator(userId).then(setUser)

  useEffect(() => {
    reload().catch(() => toast.error('Nu pot incarca utilizatorul'))
  }, [userId])

  useEffect(() => {
    sectiiApi.listSectii().then(setSectii).catch(() => {})
  }, [])

  useEffect(() => {
    if (!user) return
    setForm({
      nume: user.nume ?? '',
      prenume: user.prenume ?? '',
      grad: user.grad ?? '',
      rol: user.rol,
      sectieId: user.sectieId ?? '',
    })
  }, [user])

  if (!user) return <div>Se incarca...</div>

  const saveProfile = async () => {
    setSaving(true)
    try {
      const rolId = form.rol === 'Administrator' ? 1 : form.rol === 'SefInspectorat' ? 2 : 3
      await utilizatoriApi.updateUtilizatorAdmin(userId, {
        nume: form.nume.trim(),
        prenume: form.prenume.trim(),
        grad: form.grad.trim() || undefined,
        rolId,
        sectieId: form.rol === 'AgentPolitie' ? (form.sectieId ? Number(form.sectieId) : null) : null,
      })
      toast.success('Salvat')
      setEditing(false)
      await reload()
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare')
    } finally {
      setSaving(false)
    }
  }

  const reset = async () => {
    if (!newPass) return
    setSaving(true)
    try {
      await utilizatoriApi.resetPassword(userId, newPass)
      toast.success('Parola resetata')
      setNewPass('')
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare')
    } finally {
      setSaving(false)
    }
  }

  const deactivate = async () => {
    if (!confirm(user.esteActiv ? 'Dezactivezi utilizatorul?' : 'Activezi utilizatorul?')) return
    setSaving(true)
    try {
      if (user.esteActiv) {
        await utilizatoriApi.deactivateUser(userId)
      } else {
        await utilizatoriApi.updateUtilizatorAdmin(userId, { esteActiv: true })
      }
      toast.success('Actualizat')
      await reload()
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-slate-900">{user.username}</h2>
        <div className="flex gap-2">
          <button className="rounded border px-3 py-2 text-sm" onClick={() => navigate('/utilizatori')}>
            Inapoi
          </button>
          <button className="rounded border px-3 py-2 text-sm" onClick={() => setEditing((e) => !e)}>
            {editing ? 'Renunta' : 'Editeaza'}
          </button>
        </div>
      </div>

      {editing ? (
        <div className="rounded border bg-white p-4 shadow-sm grid gap-3 md:grid-cols-2">
          <div>
            <label className="block text-sm font-medium text-slate-700">Nume</label>
            <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={form.nume} onChange={(e) => setForm((f) => ({ ...f, nume: e.target.value }))} />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700">Prenume</label>
            <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={form.prenume} onChange={(e) => setForm((f) => ({ ...f, prenume: e.target.value }))} />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700">Grad</label>
            <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={form.grad} onChange={(e) => setForm((f) => ({ ...f, grad: e.target.value }))} />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700">Rol</label>
            <select className="mt-1 w-full rounded border px-3 py-2 text-sm" value={form.rol} onChange={(e) => setForm((f) => ({ ...f, rol: e.target.value }))}>
              <option value="Administrator">Administrator</option>
              <option value="SefInspectorat">SefInspectorat</option>
              <option value="AgentPolitie">AgentPolitie</option>
            </select>
          </div>
          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-slate-700">Sectie</label>
            <select
              className="mt-1 w-full rounded border px-3 py-2 text-sm"
              disabled={form.rol !== 'AgentPolitie'}
              value={form.sectieId}
              onChange={(e) => setForm((f) => ({ ...f, sectieId: e.target.value ? Number(e.target.value) : '' }))}
            >
              <option value="">-</option>
              {sectii.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.nume}
                </option>
              ))}
            </select>
          </div>
          <div className="md:col-span-2">
            <button disabled={saving} className="rounded bg-navy px-3 py-2 text-sm text-white disabled:opacity-60" onClick={saveProfile}>
              Salveaza
            </button>
          </div>
        </div>
      ) : (
        <div className="rounded border bg-white p-4 shadow-sm text-sm">
          <div>
            <span className="font-medium">Nume:</span> {user.nume} {user.prenume}
          </div>
          <div>
            <span className="font-medium">Grad:</span> {user.grad ?? '-'}
          </div>
          <div>
            <span className="font-medium">Rol:</span> {user.rol}
          </div>
          <div>
            <span className="font-medium">Sectie:</span> {user.sectieNume ?? '-'}
          </div>
          <div>
            <span className="font-medium">Status:</span> {user.esteActiv ? 'Activ' : 'Inactiv'}
          </div>
          <div>
            <span className="font-medium">Data creare:</span> {user.dataCreare}
          </div>
        </div>
      )}

      <div className="rounded border bg-white p-4 shadow-sm">
        <div className="text-sm font-medium text-slate-700">Reseteaza parola</div>
        <div className="mt-3 flex gap-2 max-w-md">
          <input className="w-full rounded border px-3 py-2 text-sm" type="password" value={newPass} onChange={(e) => setNewPass(e.target.value)} placeholder="Parola noua" />
          <button disabled={saving} className="rounded bg-navy px-3 py-2 text-sm text-white disabled:opacity-60" onClick={reset}>
            Salveaza
          </button>
        </div>
      </div>

      <div className="rounded border bg-white p-4 shadow-sm">
        <div className="flex items-center justify-between">
          <div className="text-sm font-medium text-slate-700">Activare / Dezactivare</div>
          <button disabled={saving} className="rounded border px-3 py-2 text-sm disabled:opacity-60" onClick={deactivate}>
            {user.esteActiv ? 'Dezactiveaza' : 'Activeaza'}
          </button>
        </div>
        <div className="mt-2 text-xs text-slate-600">Backend-ul actual foloseste DELETE ca soft-deactivate.</div>
      </div>
    </div>
  )
}
