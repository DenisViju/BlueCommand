import { useEffect, useMemo, useState } from 'react'
import toast from 'react-hot-toast'
import { Bar, BarChart, Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import * as rapoarteApi from '../api/rapoarteApi'
import * as sectiiApi from '../api/sectiiApi'

function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

function formatDate(iso: string) {
  try {
    return new Date(iso).toLocaleString('ro-RO', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit',
    })
  } catch {
    return iso
  }
}

function formatPeriod(filtru: string | null) {
  if (!filtru) return '-'
  const [start, end] = filtru.split('|')
  if (start && end) return `${start} → ${end}`
  return filtru
}

export function RapoartePage() {
  const [tip, setTip] = useState('INCIDENTE')
  const [dataStart, setDataStart] = useState('')
  const [dataEnd, setDataEnd] = useState('')
  const [sectieId, setSectieId] = useState<number | ''>('')
  const [sectii, setSectii] = useState<sectiiApi.Sectie[]>([])
  const [result, setResult] = useState<any>(null)
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState<any[]>([])
  const [exportingId, setExportingId] = useState<number | null>(null)

  useEffect(() => {
    const now = new Date()
    const y = now.getFullYear()
    setDataStart(`${y - 1}-01-01`) // implicit: 1 Ian anul trecut, ca sa includa datele seed
    setDataEnd(now.toISOString().slice(0, 10))
    sectiiApi.listSectii().then(setSectii)
    rapoarteApi.listRapoarte().then(setSaved).catch(() => {})
  }, [])

  const gen = async () => {
    setSaving(true)
    try {
      const data = await rapoarteApi.genereazaRaport({
        tip, dataStart, dataEnd,
        sectieId: typeof sectieId === 'number' ? sectieId : undefined,
      })
      setResult(data.data ?? data)
      const list = await rapoarteApi.listRapoarte()
      setSaved(list)
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare')
    } finally {
      setSaving(false)
    }
  }

  const exp = async (format: 'pdf' | 'excel') => {
    try {
      const blob = await rapoarteApi.exportRaport({
        tip, dataStart, dataEnd,
        sectieId: typeof sectieId === 'number' ? sectieId : undefined,
        format,
      })
      downloadBlob(blob, `raport_${tip}.${format === 'pdf' ? 'pdf' : 'xlsx'}`)
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare export')
    }
  }

  const expSaved = async (r: any, format: 'pdf' | 'excel') => {
    setExportingId(r.id)
    try {
      const [start, end] = (r.filtruPerioada ?? '').split('|')
      const blob = await rapoarteApi.exportRaport({
        tip: r.tip,
        dataStart: start ?? dataStart,
        dataEnd: end ?? dataEnd,
        format,
      })
      downloadBlob(blob, `raport_${r.tip}_${r.id}.${format === 'pdf' ? 'pdf' : 'xlsx'}`)
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare export')
    } finally {
      setExportingId(null)
    }
  }

  const incidenteData = useMemo(() => {
    if (!Array.isArray(result)) return []
    return result.map((x: any) => ({ luna: x.luna, count: x.count }))
  }, [result])

  const tipIncidentData = useMemo(() => {
    if (!Array.isArray(result)) return []
    const total = result.reduce((s: number, x: any) => s + (x.count ?? 0), 0) || 1
    return result.map((x: any) => ({ name: x.tip, value: x.count, pct: Math.round(((x.count ?? 0) / total) * 100) }))
  }, [result])

  const colorPalette = ['#1e3a5f', '#3b82f6', '#22c55e', '#f59e0b', '#ef4444', '#a855f7', '#14b8a6']

  const statisticiSectiiData = useMemo(() => {
    if (!Array.isArray(result)) return []
    const map = new Map<number, string>()
    sectii.forEach((s) => map.set(s.id, s.nume))
    return result.map((x: any) => ({
      name: map.get(x.sectieId) ?? `Sectia ${x.sectieId}`,
      Deschise: x.deschise ?? 0,
      'În Lucru': x.inLucru ?? 0,
      Inchise: x.inchise ?? 0,
      total: x.total ?? 0,
    }))
  }, [result, sectii])

  const activitateAgenti = useMemo(() => {
    if (!Array.isArray(result)) return []
    return result
  }, [result])

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-semibold text-slate-900">Rapoarte</h2>

      {/* Filtre */}
      <div className="rounded border bg-white p-4 shadow-sm">
        <div className="grid gap-3 md:grid-cols-5">
          <select className="rounded border px-3 py-2 text-sm" value={tip} onChange={(e) => setTip(e.target.value)}>
            <option value="INCIDENTE">INCIDENTE</option>
            <option value="ACTIVITATE_AGENTI">ACTIVITATE AGENTI</option>
            <option value="STATISTICI_SECTII">STATISTICI SECTII</option>
            <option value="TIP_INCIDENT">TIP INCIDENT</option>
          </select>
          <input className="rounded border px-3 py-2 text-sm" type="date" value={dataStart} onChange={(e) => setDataStart(e.target.value)} />
          <input className="rounded border px-3 py-2 text-sm" type="date" value={dataEnd} onChange={(e) => setDataEnd(e.target.value)} />
          <select className="rounded border px-3 py-2 text-sm" value={sectieId} onChange={(e) => setSectieId(e.target.value ? Number(e.target.value) : '')}>
            <option value="">Toate sectiile</option>
            {sectii.map((s) => (
              <option key={s.id} value={s.id}>{s.nume}</option>
            ))}
          </select>
          <button
            disabled={saving}
            className="rounded bg-navy px-3 py-2 text-sm text-white disabled:opacity-60"
            onClick={gen}
          >
            {saving ? 'Se generează...' : 'Generează'}
          </button>
        </div>
      </div>

      {/* Rezultat */}
      {result ? (
        <div className="rounded border bg-white p-4 shadow-sm space-y-4">
          <div className="flex items-center justify-between">
            <div className="text-sm font-medium text-slate-700">Rezultat — {tip}</div>
            <div className="flex gap-2">
              <button className="rounded border px-3 py-2 text-sm hover:bg-slate-50" onClick={() => exp('pdf')}>
                Export PDF
              </button>
              <button className="rounded border px-3 py-2 text-sm hover:bg-slate-50" onClick={() => exp('excel')}>
                Export Excel
              </button>
            </div>
          </div>

          {tip === 'INCIDENTE' && incidenteData.length > 0 ? (
            <div className="h-80">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={incidenteData}>
                  <XAxis dataKey="luna" tick={{ fontSize: 12 }} />
                  <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
                  <Tooltip />
                  <Bar dataKey="count" name="Incidente" fill="#1e3a5f" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          ) : tip === 'INCIDENTE' ? (
            <div className="py-8 text-center text-sm text-slate-400">Niciun incident în perioada selectată.</div>
          ) : null}

          {tip === 'ACTIVITATE_AGENTI' ? (
            <div className="overflow-auto">
              <table className="w-full text-sm">
                <thead className="bg-slate-50 text-slate-700">
                  <tr>
                    <th className="p-2 text-left">Nume agent</th>
                    <th className="p-2 text-right">Total</th>
                    <th className="p-2 text-right">Deschise</th>
                    <th className="p-2 text-right">Inchise</th>
                  </tr>
                </thead>
                <tbody>
                  {activitateAgenti.map((x: any) => (
                    <tr key={x.agentId} className="border-t">
                      <td className="p-2">{x.nume || x.username}</td>
                      <td className="p-2 text-right">{x.total}</td>
                      <td className="p-2 text-right">{x.deschise}</td>
                      <td className="p-2 text-right">{x.inchise}</td>
                    </tr>
                  ))}
                  {activitateAgenti.length === 0 && (
                    <tr><td colSpan={4} className="p-3 text-center text-slate-400">Niciun rezultat.</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          ) : null}

          {tip === 'STATISTICI_SECTII' && statisticiSectiiData.length > 0 ? (
            <div className="h-80">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={statisticiSectiiData}>
                  <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                  <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="Deschise" stackId="a" fill="#22c55e" />
                  <Bar dataKey="În Lucru" stackId="a" fill="#f59e0b" />
                  <Bar dataKey="Inchise" stackId="a" fill="#94a3b8" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          ) : null}

          {tip === 'TIP_INCIDENT' && tipIncidentData.length > 0 ? (
            <div className="h-80">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={tipIncidentData}
                    dataKey="value"
                    nameKey="name"
                    outerRadius={120}
                    label={(p: any) => `${p.name} ${p.payload.pct}%`}
                  >
                    {tipIncidentData.map((_: any, idx: number) => (
                      <Cell key={idx} fill={colorPalette[idx % colorPalette.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            </div>
          ) : null}
        </div>
      ) : null}

      {/* Rapoarte salvate */}
      <div className="rounded border bg-white p-4 shadow-sm">
        <div className="text-sm font-medium text-slate-700">Rapoarte salvate</div>
        <div className="mt-3 overflow-auto">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 text-slate-700">
              <tr>
                <th className="p-2 text-left">Tip</th>
                <th className="p-2 text-left">Perioadă</th>
                <th className="p-2 text-left">Data generare</th>
                <th className="p-2 text-left">Descarcă</th>
              </tr>
            </thead>
            <tbody>
              {saved.map((r) => (
                <tr key={r.id} className="border-t">
                  <td className="p-2 font-medium">{r.tip}</td>
                  <td className="p-2 text-slate-600">{formatPeriod(r.filtruPerioada)}</td>
                  <td className="p-2 text-slate-600">{formatDate(r.dataGenerare)}</td>
                  <td className="p-2">
                    <div className="flex gap-1">
                      <button
                        disabled={exportingId === r.id}
                        onClick={() => expSaved(r, 'pdf')}
                        className="rounded border px-2 py-1 text-xs hover:bg-slate-50 disabled:opacity-50"
                      >
                        PDF
                      </button>
                      <button
                        disabled={exportingId === r.id}
                        onClick={() => expSaved(r, 'excel')}
                        className="rounded border px-2 py-1 text-xs hover:bg-slate-50 disabled:opacity-50"
                      >
                        Excel
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {saved.length === 0 && (
                <tr>
                  <td colSpan={4} className="p-3 text-sm text-slate-400">Niciun raport salvat.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
