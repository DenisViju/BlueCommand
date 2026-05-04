import { useEffect, useState } from 'react'
import * as auditApi from '../api/auditApi'

export function AuditPage() {
  const [data, setData] = useState<{ items: any[]; total: number; page: number; pageSize: number } | null>(null)

  useEffect(() => {
    auditApi.listAudit({ page: 1 }).then(setData)
  }, [])

  if (!data) return <div>Se incarca...</div>

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-semibold text-slate-900">Audit Log</h2>
      <div className="rounded border bg-white shadow-sm overflow-auto">
        <table className="w-full text-sm">
          <thead className="bg-slate-50 text-slate-700">
            <tr>
              <th className="p-2 text-left">Timestamp</th>
              <th className="p-2 text-left">UserId</th>
              <th className="p-2 text-left">Actiune</th>
              <th className="p-2 text-left">IP</th>
            </tr>
          </thead>
          <tbody>
            {data.items.map((a) => (
              <tr key={a.id} className="border-t">
                <td className="p-2">{a.creatLa}</td>
                <td className="p-2">{a.utilizatorId ?? '-'}</td>
                <td className="p-2">{a.actiune}</td>
                <td className="p-2">{a.ipAdresa ?? '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

