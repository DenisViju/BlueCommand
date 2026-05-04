import 'leaflet/dist/leaflet.css'

import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import { MapContainer, Marker, Popup, TileLayer } from 'react-leaflet'
import L from 'leaflet'
import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png'
import markerIcon from 'leaflet/dist/images/marker-icon.png'
import markerShadow from 'leaflet/dist/images/marker-shadow.png'
import * as sectiiApi from '../api/sectiiApi'

delete (L.Icon.Default.prototype as any)._getIconUrl
L.Icon.Default.mergeOptions({ iconRetinaUrl: markerIcon2x, iconUrl: markerIcon, shadowUrl: markerShadow })

type Tab = 'lista' | 'harta'

export function SectiiPage() {
  const navigate = useNavigate()
  const [tab, setTab] = useState<Tab>('lista')
  const [search, setSearch] = useState('')
  const [items, setItems] = useState<sectiiApi.Sectie[]>([])
  const [mapItems, setMapItems] = useState<sectiiApi.Sectie[]>([])
  const [loading, setLoading] = useState(false)

  const center = useMemo<[number, number]>(() => [44.3302, 23.7949], [])

  const load = async () => {
    setLoading(true)
    try {
      const data = await sectiiApi.listSectii(search)
      setItems(data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load().catch(() => toast.error('Nu pot incarca sectiile'))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (tab !== 'harta') return
    sectiiApi
      .getHartaSectii()
      .then(setMapItems)
      .catch(() => toast.error('Nu pot incarca harta'))
  }, [tab])

  const onDelete = async (id: number) => {
    if (!confirm('Stergi sectia?')) return
    try {
      await sectiiApi.deleteSectie(id)
      toast.success('Sters')
      await load()
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare la stergere')
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-slate-900">Sectii</h2>
        <button className="rounded bg-navy px-3 py-2 text-sm text-white" onClick={() => navigate('/sectii/nou')}>
          Adauga Sectie
        </button>
      </div>

      <div className="flex items-center justify-between gap-3">
        <div className="flex gap-2 text-sm">
          <button className={`rounded border px-3 py-2 ${tab === 'lista' ? 'bg-slate-50' : ''}`} onClick={() => setTab('lista')}>
            Lista
          </button>
          <button className={`rounded border px-3 py-2 ${tab === 'harta' ? 'bg-slate-50' : ''}`} onClick={() => setTab('harta')}>
            Harta
          </button>
        </div>

        {tab === 'lista' ? (
          <div className="flex gap-2">
            <input
              className="rounded border px-3 py-2 text-sm"
              placeholder="Cauta dupa nume sau zona..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') load()
              }}
            />
            <button className="rounded border px-3 py-2 text-sm" onClick={load} disabled={loading}>
              Cauta
            </button>
          </div>
        ) : null}
      </div>

      {tab === 'lista' ? (
        <div className="rounded border bg-white shadow-sm overflow-auto">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 text-slate-700">
              <tr>
                <th className="p-2 text-left">Nume</th>
                <th className="p-2 text-left">Adresa</th>
                <th className="p-2 text-left">Zona</th>
                <th className="p-2 text-left">Actiuni</th>
              </tr>
            </thead>
            <tbody>
              {items.map((s) => (
                <tr key={s.id} className="border-t">
                  <td className="p-2">{s.nume}</td>
                  <td className="p-2">{s.adresa ?? '-'}</td>
                  <td className="p-2">{s.zona ?? '-'}</td>
                  <td className="p-2">
                    <div className="flex gap-2">
                      <button className="rounded border px-2 py-1 text-xs" onClick={() => navigate(`/sectii/${s.id}`)}>
                        Detalii
                      </button>
                      <button className="rounded border px-2 py-1 text-xs" onClick={() => onDelete(s.id)}>
                        Sterge
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {items.length === 0 ? (
                <tr>
                  <td className="p-3 text-sm text-slate-600" colSpan={4}>
                    Nicio sectie.
                  </td>
                </tr>
              ) : null}
            </tbody>
          </table>
        </div>
      ) : null}

      {tab === 'harta' ? (
        <div className="rounded border bg-white p-4 shadow-sm">
          <div className="h-[520px] overflow-hidden rounded border">
            <MapContainer center={center} zoom={13} style={{ height: '520px', width: '100%' }}>
              <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
              {mapItems
                .filter((s) => typeof s.latitudine === 'number' && typeof s.longitudine === 'number')
                .map((s) => (
                  <Marker key={s.id} position={[s.latitudine as number, s.longitudine as number]}>
                    <Popup>
                      <div className="text-sm">
                        <div className="font-medium">{s.nume}</div>
                        <div>{s.adresa ?? '-'}</div>
                        <div>{s.zona ?? '-'}</div>
                        <div className="mt-1">
                          <Link className="text-blue-600 hover:underline" to={`/sectii/${s.id}`}>
                            Detalii
                          </Link>
                        </div>
                      </div>
                    </Popup>
                  </Marker>
                ))}
            </MapContainer>
          </div>
        </div>
      ) : null}
    </div>
  )
}
