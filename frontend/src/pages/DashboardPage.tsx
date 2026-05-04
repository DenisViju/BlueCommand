import { useEffect, useMemo, useState } from 'react'
import {
  Bar, BarChart, Cell, Legend, Pie, PieChart,
  ResponsiveContainer, Tooltip, XAxis, YAxis,
} from 'recharts'
import * as dashboardApi from '../api/dashboardApi'
import * as rapoarteApi from '../api/rapoarteApi'
import type { DashboardStats } from '../types/dashboard'

const STATUS_COLORS: Record<string, string> = {
  DESCHIS: '#22c55e',
  IN_LUCRU: '#f59e0b',
  INCHIS: '#64748b',
}

export function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [sectiiData, setSectiiData] = useState<any[]>([])

  useEffect(() => {
    dashboardApi.getStats().then(setStats)

    // Incarca dosare pe sectii pentru tot anul curent
    const an = new Date().getFullYear()
    rapoarteApi
      .genereazaRaport({
        tip: 'STATISTICI_SECTII',
        dataStart: `${an - 1}-01-01`,
        dataEnd: `${an}-12-31`,
      })
      .then((res) => {
        const raw: any[] = Array.isArray(res?.data) ? res.data : Array.isArray(res) ? res : []
        setSectiiData(raw)
      })
      .catch(() => {})
  }, [])

  const sectiiChartData = useMemo(
    () =>
      sectiiData.map((x: any) => ({
        name: x.sectieNume ?? x.numesectie ?? `Sectia ${x.sectieId}`,
        Deschise: x.deschise ?? 0,
        'În Lucru': x.inLucru ?? 0,
        Inchise: x.inchise ?? 0,
      })),
    [sectiiData],
  )

  if (!stats) return <div>Se incarca...</div>

  const statusData = [
    { name: 'DESCHIS', value: stats.dosareDeschise },
    { name: 'IN_LUCRU', value: stats.dosareInLucru },
    { name: 'INCHIS', value: stats.dosareInchise },
  ].filter((d) => d.value > 0)

  const cards = [
    { label: 'Total Dosare', value: stats.totalDosare, color: 'text-slate-900' },
    { label: 'Dosare Deschise', value: stats.dosareDeschise, color: 'text-green-700' },
    { label: 'Dosare În Lucru', value: stats.dosareInLucru, color: 'text-amber-600' },
    { label: 'Dosare Inchise', value: stats.dosareInchise, color: 'text-slate-500' },
    { label: 'Total Agenti', value: stats.totalAgenti, color: 'text-slate-900' },
    { label: 'Total Sectii', value: stats.totalSectii, color: 'text-slate-900' },
  ]

  return (
    <div className="space-y-6">
      <h2 className="text-xl font-semibold text-slate-900">Dashboard</h2>

      {/* Cards */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        {cards.map(({ label, value, color }) => (
          <div key={label} className="rounded border bg-white p-4 shadow-sm">
            <div className="text-sm text-slate-600">{label}</div>
            <div className={`mt-1 text-2xl font-semibold ${color}`}>{value}</div>
          </div>
        ))}
      </div>

      {/* Grafice rând 1 */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Pie chart status */}
        <div className="rounded border bg-white p-4 shadow-sm">
          <div className="mb-3 text-sm font-medium text-slate-700">Cazuri pe status</div>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={statusData}
                  dataKey="value"
                  nameKey="name"
                  cx="50%"
                  cy="50%"
                  outerRadius={85}
                  label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                  labelLine
                >
                  {statusData.map((entry) => (
                    <Cell key={entry.name} fill={STATUS_COLORS[entry.name] ?? '#3b82f6'} />
                  ))}
                </Pie>
                <Tooltip formatter={(value: number, name: string) => [value, name]} />
                <Legend
                  formatter={(value) => (
                    <span style={{ color: STATUS_COLORS[value] ?? '#3b82f6', fontWeight: 500 }}>
                      {value}
                    </span>
                  )}
                />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Bar chart incidente 30 zile */}
        <div className="rounded border bg-white p-4 shadow-sm">
          <div className="mb-3 text-sm font-medium text-slate-700">Incidente (ultimele 30 zile)</div>
          <div className="h-64 flex flex-col items-center justify-center">
            {stats.incidentePeUltimele30Zile === 0 ? (
              <div className="text-center">
                <div className="text-3xl font-bold text-slate-300">0</div>
                <div className="mt-1 text-sm text-slate-400">Niciun incident înregistrat în ultimele 30 de zile</div>
              </div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={[{ name: 'Ultimele 30 zile', count: stats.incidentePeUltimele30Zile }]}>
                  <XAxis dataKey="name" />
                  <YAxis allowDecimals={false} />
                  <Tooltip />
                  <Bar dataKey="count" fill="#1e3a5f" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>
      </div>

      {/* Grafic dosare pe sectii */}
      {sectiiChartData.length > 0 && (
        <div className="rounded border bg-white p-4 shadow-sm">
          <div className="mb-3 text-sm font-medium text-slate-700">
            Dosare pe secții — {new Date().getFullYear() - 1}–{new Date().getFullYear()}
          </div>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={sectiiChartData} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
                <Tooltip />
                <Legend />
                <Bar dataKey="Deschise" stackId="a" fill="#22c55e" radius={[0, 0, 0, 0]} />
                <Bar dataKey="În Lucru" stackId="a" fill="#f59e0b" />
                <Bar dataKey="Inchise" stackId="a" fill="#64748b" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}
    </div>
  )
}
