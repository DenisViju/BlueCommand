import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import toast from 'react-hot-toast'
import * as agentiApi from '../api/agentiApi'

type Tab = 'info' | 'istoric'

export function AgentDetailPage() {
  const { id } = useParams()
  const agentId = Number(id)
  const navigate = useNavigate()
  const [tab, setTab] = useState<Tab>('info')
  const [agent, setAgent] = useState<any>(null)
  const [istoric, setIstoric] = useState<any[]>([])

  useEffect(() => {
    agentiApi
      .getAgent(agentId)
      .then((res) => setAgent(res.agent ?? res))
      .catch(() => toast.error('Nu pot incarca agentul'))
  }, [agentId])

  useEffect(() => {
    if (tab !== 'istoric') return
    agentiApi
      .getAgentIstoric(agentId)
      .then(setIstoric)
      .catch(() => toast.error('Nu pot incarca istoricul'))
  }, [tab, agentId])

  if (!agent) return <div>Se incarca...</div>

  const del = async () => {
    if (!confirm('Stergi agentul?')) return
    try {
      await agentiApi.deleteAgent(agentId)
      toast.success('Sters')
      navigate('/agenti')
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare')
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold text-slate-900">
            {agent.nume ?? ''} {agent.prenume ?? ''} ({agent.username})
          </h2>
          <div className="text-sm text-slate-600">
            {agent.grad ?? '-'} • {agent.sectieNume ?? '-'}
          </div>
        </div>
        <div className="flex gap-2">
          <button className="rounded border px-3 py-2 text-sm" onClick={() => navigate('/agenti')}>
            Inapoi
          </button>
          <button className="rounded bg-navy px-3 py-2 text-sm text-white" onClick={() => navigate(`/agenti/${agentId}/edit`)}>
            Editeaza
          </button>
          <button className="rounded border px-3 py-2 text-sm" onClick={del}>
            Sterge
          </button>
        </div>
      </div>

      <div className="flex gap-2 text-sm">
        <button className={`rounded border px-3 py-2 ${tab === 'info' ? 'bg-slate-50' : ''}`} onClick={() => setTab('info')}>
          Informatii
        </button>
        <button className={`rounded border px-3 py-2 ${tab === 'istoric' ? 'bg-slate-50' : ''}`} onClick={() => setTab('istoric')}>
          Istoric
        </button>
      </div>

      {tab === 'info' ? (
        <div className="rounded border bg-white p-4 shadow-sm text-sm">
          <div>
            <span className="font-medium">Username:</span> {agent.username}
          </div>
          <div>
            <span className="font-medium">Nume:</span> {agent.nume} {agent.prenume}
          </div>
          <div>
            <span className="font-medium">Grad:</span> {agent.grad ?? '-'}
          </div>
          <div>
            <span className="font-medium">Sectie:</span> {agent.sectieNume ?? '-'}
          </div>
        </div>
      ) : null}

      {tab === 'istoric' ? (
        <div className="rounded border bg-white shadow-sm overflow-auto">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 text-slate-700">
              <tr>
                <th className="p-2 text-left">Data</th>
                <th className="p-2 text-left">Camp</th>
                <th className="p-2 text-left">Vechi</th>
                <th className="p-2 text-left">Nou</th>
              </tr>
            </thead>
            <tbody>
              {istoric.map((i) => (
                <tr key={i.id} className="border-t">
                  <td className="p-2">{i.modificatLa}</td>
                  <td className="p-2">{i.campModificat}</td>
                  <td className="p-2">{i.valoareVeche ?? '-'}</td>
                  <td className="p-2">{i.valoareNoua ?? '-'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}
    </div>
  )
}

