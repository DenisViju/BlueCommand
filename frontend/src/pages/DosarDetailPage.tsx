import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import toast from 'react-hot-toast'
import * as dosareApi from '../api/dosareApi'
import { useAuth } from '../context/AuthContext'

function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

const STATUS_COLORS: Record<string, string> = {
  DESCHIS: 'bg-green-100 text-green-800 border-green-200',
  IN_LUCRU: 'bg-amber-100 text-amber-800 border-amber-200',
  INCHIS: 'bg-slate-100 text-slate-700 border-slate-200',
}

function getNextStatuses(currentStatus: string, role: string): string[] {
  if (currentStatus === 'INCHIS') return []
  if (role === 'SefInspectorat' || role === 'Administrator') {
    return ['DESCHIS', 'IN_LUCRU', 'INCHIS'].filter((s) => s !== currentStatus)
  }
  if (role === 'AgentPolitie') {
    if (currentStatus === 'DESCHIS') return ['IN_LUCRU']
    if (currentStatus === 'IN_LUCRU') return ['DESCHIS']
  }
  return []
}

const STATUS_LABELS: Record<string, string> = {
  DESCHIS: 'Marchează Deschis',
  IN_LUCRU: 'Marchează În Lucru',
  INCHIS: 'Închide Dosarul',
}

export function DosarDetailPage() {
  const { id } = useParams()
  const dosarId = Number(id)
  const { user } = useAuth()
  const [dosar, setDosar] = useState<dosareApi.DosarDetail | null>(null)
  const [loadingStatus, setLoadingStatus] = useState(false)
  const [editingDesc, setEditingDesc] = useState(false)
  const [descValue, setDescValue] = useState('')
  const [savingDesc, setSavingDesc] = useState(false)

  const reload = () =>
    dosareApi.getDosar(dosarId).then((d) => {
      setDosar(d)
      setDescValue(d.descriere ?? '')
    })

  useEffect(() => {
    if (!Number.isFinite(dosarId)) return
    reload()
  }, [dosarId])

  if (!dosar) return <div>Se incarca...</div>

  const canEdit = dosar.status !== 'INCHIS'
  const canReopen =
    dosar.status === 'INCHIS' &&
    (user?.rol === 'SefInspectorat' || user?.rol === 'Administrator')

  const onUpload = async (file: File) => {
    try {
      await dosareApi.uploadDocument(dosarId, file)
      toast.success('Document incarcat')
      reload()
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare upload')
    }
  }

  const onExport = async (format: 'pdf' | 'excel') => {
    const blob = await dosareApi.exportDosar(dosarId, format)
    downloadBlob(blob, `dosar_${dosar.numarDosar.replaceAll('/', '_')}.${format === 'pdf' ? 'pdf' : 'xlsx'}`)
  }

  const onSchimbaStatus = async (newStatus: string) => {
    setLoadingStatus(true)
    try {
      if (newStatus === 'INCHIS') {
        await dosareApi.closeDosar(dosarId)
      } else {
        await dosareApi.schimbaStatus(dosarId, newStatus)
      }
      toast.success(`Status schimbat la ${newStatus}`)
      reload()
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare la schimbarea statusului')
    } finally {
      setLoadingStatus(false)
    }
  }

  const onRedeschide = async () => {
    setLoadingStatus(true)
    try {
      await dosareApi.redeschideDosar(dosarId)
      toast.success('Dosarul a fost redeschis')
      reload()
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare la redeschidere')
    } finally {
      setLoadingStatus(false)
    }
  }

  const onSaveDesc = async () => {
    setSavingDesc(true)
    try {
      await dosareApi.updateDosar(dosarId, { descriere: descValue })
      toast.success('Descriere actualizata')
      setEditingDesc(false)
      reload()
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare la salvare')
    } finally {
      setSavingDesc(false)
    }
  }

  const nextStatuses = user ? getNextStatuses(dosar.status, user.rol) : []

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold text-slate-900">Dosar {dosar.numarDosar}</h2>
          <div className="mt-1 flex flex-wrap items-center gap-2 text-sm text-slate-600">
            <span>{dosar.titlu}</span>
            <span>•</span>
            <span className={`rounded border px-2 py-0.5 text-xs font-medium ${STATUS_COLORS[dosar.status] ?? ''}`}>
              {dosar.status}
            </span>
            <span>•</span>
            <span>{dosar.sectieNume}</span>
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
          <button className="rounded bg-navy px-3 py-2 text-sm text-white" onClick={() => onExport('pdf')}>
            Export PDF
          </button>
          <button className="rounded border px-3 py-2 text-sm" onClick={() => onExport('excel')}>
            Export Excel
          </button>
        </div>
      </div>

      {/* Schimbare status (pentru dosare deschise/in lucru) */}
      {nextStatuses.length > 0 && (
        <div className="flex flex-wrap items-center gap-2 rounded border bg-white p-3 shadow-sm">
          <span className="text-sm font-medium text-slate-700">Schimbă status:</span>
          {nextStatuses.map((s) => (
            <button
              key={s}
              disabled={loadingStatus}
              onClick={() => onSchimbaStatus(s)}
              className={`rounded border px-3 py-1.5 text-xs font-medium transition-opacity disabled:opacity-50
                ${s === 'INCHIS'
                  ? 'border-slate-300 bg-slate-100 text-slate-700 hover:bg-slate-200'
                  : s === 'IN_LUCRU'
                    ? 'border-amber-200 bg-amber-100 text-amber-800 hover:bg-amber-200'
                    : 'border-green-200 bg-green-100 text-green-800 hover:bg-green-200'
                }`}
            >
              {STATUS_LABELS[s] ?? s}
            </button>
          ))}
        </div>
      )}

      {/* Banner dosar inchis + buton redeschide */}
      {dosar.status === 'INCHIS' && (
        <div className="flex items-center justify-between rounded border border-slate-200 bg-slate-50 p-3">
          <span className="text-sm text-slate-700">Acest dosar este inchis si nu poate fi editat.</span>
          {canReopen && (
            <button
              disabled={loadingStatus}
              onClick={onRedeschide}
              className="rounded border border-green-300 bg-green-50 px-3 py-1.5 text-xs font-medium text-green-800 hover:bg-green-100 disabled:opacity-50"
            >
              Redeschide Dosarul
            </button>
          )}
        </div>
      )}

      {/* Descriere */}
      <div className="rounded border bg-white p-4 shadow-sm">
        <div className="flex items-center justify-between">
          <div className="text-sm font-medium text-slate-700">Descriere</div>
          {canEdit && !editingDesc && (
            <button
              onClick={() => {
                setDescValue(dosar.descriere ?? '')
                setEditingDesc(true)
              }}
              className="rounded border px-2 py-1 text-xs text-slate-600 hover:bg-slate-50"
            >
              Editează
            </button>
          )}
        </div>

        {editingDesc ? (
          <div className="mt-2 space-y-2">
            <textarea
              className="w-full rounded border px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-navy"
              rows={4}
              value={descValue}
              onChange={(e) => setDescValue(e.target.value)}
              autoFocus
            />
            <div className="flex gap-2">
              <button
                disabled={savingDesc}
                onClick={onSaveDesc}
                className="rounded bg-navy px-3 py-1.5 text-xs text-white disabled:opacity-60"
              >
                {savingDesc ? 'Se salvează...' : 'Salvează'}
              </button>
              <button
                onClick={() => setEditingDesc(false)}
                className="rounded border px-3 py-1.5 text-xs"
              >
                Anulează
              </button>
            </div>
          </div>
        ) : (
          <div className="mt-2 text-sm text-slate-700 whitespace-pre-wrap">
            {dosar.descriere || <span className="text-slate-400 italic">Fără descriere</span>}
          </div>
        )}
      </div>

      {/* Documente */}
      <div className="rounded border bg-white p-4 shadow-sm">
        <div className="text-sm font-medium text-slate-700">Documente</div>
        <div className="mt-3 flex items-center gap-3">
          <input
            type="file"
            onChange={(e) => {
              const f = e.target.files?.[0]
              if (f) onUpload(f)
              e.currentTarget.value = ''
            }}
          />
        </div>
        <ul className="mt-3 space-y-2 text-sm">
          {dosar.documente.map((d) => (
            <li key={d.id} className="flex items-center justify-between gap-3 rounded border px-3 py-2">
              <a className="text-blue-600 hover:underline" href={d.caleFisier} target="_blank" rel="noreferrer">
                {d.numeFisier}
              </a>
              <button
                className="rounded border px-2 py-1 text-xs"
                onClick={async () => {
                  await dosareApi.deleteDocument(dosarId, d.id)
                  toast.success('Sters')
                  reload()
                }}
              >
                Sterge
              </button>
            </li>
          ))}
          {dosar.documente.length === 0 && (
            <li className="text-sm text-slate-400 italic">Niciun document atașat.</li>
          )}
        </ul>
      </div>

      {/* Agenti */}
      <div className="rounded border bg-white p-4 shadow-sm">
        <div className="text-sm font-medium text-slate-700">Agenti</div>
        <ul className="mt-2 text-sm text-slate-700 list-disc pl-5">
          {dosar.agenti.map((a) => (
            <li key={a.id}>
              {a.username} - {(a.nume ?? '') + ' ' + (a.prenume ?? '')} ({a.grad ?? '-'})
            </li>
          ))}
        </ul>
      </div>
    </div>
  )
}
