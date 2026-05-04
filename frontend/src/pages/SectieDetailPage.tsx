import 'leaflet/dist/leaflet.css'

import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import toast from 'react-hot-toast'
import { MapContainer, Marker, Popup, TileLayer } from 'react-leaflet'
import L from 'leaflet'
import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png'
import markerIcon from 'leaflet/dist/images/marker-icon.png'
import markerShadow from 'leaflet/dist/images/marker-shadow.png'
import * as sectiiApi from '../api/sectiiApi'
import * as agentiApi from '../api/agentiApi'

delete (L.Icon.Default.prototype as any)._getIconUrl
L.Icon.Default.mergeOptions({ iconRetinaUrl: markerIcon2x, iconUrl: markerIcon, shadowUrl: markerShadow })

type Tab = 'info' | 'agenti' | 'istoric'

export function SectieDetailPage() {
  const { id } = useParams()
  const sectieId = Number(id)
  const navigate = useNavigate()
  const [tab, setTab] = useState<Tab>('info')
  const [sectie, setSectie] = useState<any>(null)
  const [agenti, setAgenti] = useState<agentiApi.Agent[]>([])
  const [istoric, setIstoric] = useState<sectiiApi.IstoricEntry[]>([])

  const center = useMemo<[number, number]>(() => [44.3302, 23.7949], [])

  useEffect(() => {
    if (!Number.isFinite(sectieId)) return
    sectiiApi
      .getSectie(sectieId)
      .then((res) => setSectie(res.sectie ?? res))
      .catch(() => toast.error('Nu pot incarca sectia'))
  }, [sectieId])

  useEffect(() => {
    if (tab === 'agenti') {
      agentiApi
        .listAgenti({ sectieId })
        .then(setAgenti)
        .catch(() => toast.error('Nu pot incarca agentii'))
    }
    if (tab === 'istoric') {
      sectiiApi
        .getSectieIstoric(sectieId)
        .then(setIstoric)
        .catch(() => toast.error('Nu pot incarca istoricul'))
    }
  }, [tab, sectieId])

  if (!sectie) return <div>Se incarca...</div>

  const hasCoords = sectie.latitudine != null && sectie.longitudine != null

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold text-slate-900">{sectie.nume}</h2>
          <div className="text-sm text-slate-600">
            {sectie.zona ?? '-'} • {sectie.adresa ?? '-'}
          </div>
        </div>
        <div className="flex gap-2">
          <button className="rounded border px-3 py-2 text-sm" onClick={() => navigate('/sectii')}>
            Inapoi
          </button>
          <button className="rounded bg-navy px-3 py-2 text-sm text-white" onClick={() => navigate(`/sectii/${sectieId}/edit`)}>
            Editeaza
          </button>
        </div>
      </div>

      <div className="flex gap-2 text-sm">
        <button className={`rounded border px-3 py-2 ${tab === 'info' ? 'bg-slate-50' : ''}`} onClick={() => setTab('info')}>
          Informatii
        </button>
        <button className={`rounded border px-3 py-2 ${tab === 'agenti' ? 'bg-slate-50' : ''}`} onClick={() => setTab('agenti')}>
          Agenti
        </button>
        <button className={`rounded border px-3 py-2 ${tab === 'istoric' ? 'bg-slate-50' : ''}`} onClick={() => setTab('istoric')}>
          Istoric
        </button>
      </div>

      {tab === 'info' ? (
        <div className="grid gap-4 lg:grid-cols-2">
          <div className="rounded border bg-white p-4 shadow-sm text-sm">
            <div>
              <span className="font-medium">Nume:</span> {sectie.nume}
            </div>
            <div>
              <span className="font-medium">Zona:</span> {sectie.zona ?? '-'}
            </div>
            <div>
              <span className="font-medium">Adresa:</span> {sectie.adresa ?? '-'}
            </div>
            <div>
              <span className="font-medium">Lat/Lon:</span>{' '}
              {hasCoords ? `${sectie.latitudine}, ${sectie.longitudine}` : '-'}
            </div>
          </div>
          <div className="rounded border bg-white p-4 shadow-sm">
            <div className="text-sm font-medium text-slate-700">Harta</div>
            <div className="mt-3 h-[300px] overflow-hidden rounded border">
              <MapContainer
                center={hasCoords ? [sectie.latitudine, sectie.longitudine] : center}
                zoom={13}
                style={{ height: '300px', width: '100%' }}
              >
                <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
                {hasCoords ? (
                  <Marker position={[sectie.latitudine, sectie.longitudine]}>
                    <Popup>
                      <div className="text-sm">
                        <div className="font-medium">{sectie.nume}</div>
                        <div>{sectie.adresa ?? '-'}</div>
                        <div>{sectie.zona ?? '-'}</div>
                      </div>
                    </Popup>
                  </Marker>
                ) : null}
              </MapContainer>
            </div>
          </div>
        </div>
      ) : null}

      {tab === 'agenti' ? (
        <div className="rounded border bg-white shadow-sm overflow-auto">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 text-slate-700">
              <tr>
                <th className="p-2 text-left">Nume</th>
                <th className="p-2 text-left">Prenume</th>
                <th className="p-2 text-left">Grad</th>
                <th className="p-2 text-left">Username</th>
              </tr>
            </thead>
            <tbody>
              {agenti.map((a) => (
                <tr key={a.id} className="border-t">
                  <td className="p-2">{a.nume ?? '-'}</td>
                  <td className="p-2">{a.prenume ?? '-'}</td>
                  <td className="p-2">{a.grad ?? '-'}</td>
                  <td className="p-2">
                    <Link className="text-blue-600 hover:underline" to={`/agenti/${a.id}`}>
                      {a.username}
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
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

