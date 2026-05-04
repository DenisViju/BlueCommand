import 'leaflet/dist/leaflet.css'

import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import toast from 'react-hot-toast'
import { MapContainer, Marker, TileLayer, useMapEvents } from 'react-leaflet'
import L from 'leaflet'
import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png'
import markerIcon from 'leaflet/dist/images/marker-icon.png'
import markerShadow from 'leaflet/dist/images/marker-shadow.png'
import * as sectiiApi from '../api/sectiiApi'

delete (L.Icon.Default.prototype as any)._getIconUrl
L.Icon.Default.mergeOptions({ iconRetinaUrl: markerIcon2x, iconUrl: markerIcon, shadowUrl: markerShadow })

function PickLocation({ onPick }: { onPick: (lat: number, lon: number) => void }) {
  useMapEvents({
    click(e) {
      onPick(e.latlng.lat, e.latlng.lng)
    },
  })
  return null
}

export function SectieFormPage() {
  const { id } = useParams()
  const editId = id ? Number(id) : null
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [nume, setNume] = useState('')
  const [adresa, setAdresa] = useState('')
  const [zona, setZona] = useState('')
  const [latitudine, setLatitudine] = useState<number | ''>('')
  const [longitudine, setLongitudine] = useState<number | ''>('')
  const [touched, setTouched] = useState(false)

  const center = useMemo<[number, number]>(() => {
    if (typeof latitudine === 'number' && typeof longitudine === 'number') return [latitudine, longitudine]
    return [44.3302, 23.7949]
  }, [latitudine, longitudine])

  useEffect(() => {
    if (!editId) return
    setLoading(true)
    sectiiApi
      .getSectie(editId)
      .then((res) => {
        const s = res.sectie ?? res
        setNume(s.nume ?? '')
        setAdresa(s.adresa ?? '')
        setZona(s.zona ?? '')
        setLatitudine(s.latitudine ?? '')
        setLongitudine(s.longitudine ?? '')
      })
      .catch(() => toast.error('Nu pot incarca sectia'))
      .finally(() => setLoading(false))
  }, [editId])

  const save = async () => {
    setTouched(true)
    if (!nume.trim()) return
    setLoading(true)
    try {
      const body: sectiiApi.CreateSectieRequest = {
        nume: nume.trim(),
        adresa: adresa.trim() || undefined,
        zona: zona.trim() || undefined,
        latitudine: typeof latitudine === 'number' ? latitudine : undefined,
        longitudine: typeof longitudine === 'number' ? longitudine : undefined,
      }
      if (editId) {
        await sectiiApi.updateSectie(editId, body)
        toast.success('Sectie actualizata')
        navigate(`/sectii/${editId}`)
      } else {
        await sectiiApi.createSectie(body)
        toast.success('Sectie creata')
        navigate('/sectii')
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
        <h2 className="text-xl font-semibold text-slate-900">{editId ? 'Editeaza sectie' : 'Sectie noua'}</h2>
        <div className="flex gap-2">
          <button className="rounded border px-3 py-2 text-sm" onClick={() => navigate('/sectii')}>
            Anuleaza
          </button>
          <button disabled={loading} className="rounded bg-navy px-3 py-2 text-sm text-white disabled:opacity-60" onClick={save}>
            Salveaza
          </button>
        </div>
      </div>

      <div className="rounded border bg-white p-4 shadow-sm grid gap-3 md:grid-cols-2">
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-slate-700">Nume *</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={nume} onChange={(e) => setNume(e.target.value)} />
          {touched && !nume.trim() ? <div className="mt-1 text-xs text-red-600">Numele este obligatoriu</div> : null}
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700">Adresa</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={adresa} onChange={(e) => setAdresa(e.target.value)} />
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700">Zona</label>
          <input className="mt-1 w-full rounded border px-3 py-2 text-sm" value={zona} onChange={(e) => setZona(e.target.value)} />
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700">Latitudine</label>
          <input
            type="number"
            className="mt-1 w-full rounded border px-3 py-2 text-sm"
            value={latitudine}
            onChange={(e) => setLatitudine(e.target.value === '' ? '' : Number(e.target.value))}
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700">Longitudine</label>
          <input
            type="number"
            className="mt-1 w-full rounded border px-3 py-2 text-sm"
            value={longitudine}
            onChange={(e) => setLongitudine(e.target.value === '' ? '' : Number(e.target.value))}
          />
        </div>
      </div>

      <div className="rounded border bg-white p-4 shadow-sm">
        <div className="text-sm font-medium text-slate-700">Alege locatie (click pe harta)</div>
        <div className="mt-3 h-[300px] overflow-hidden rounded border">
          <MapContainer center={center} zoom={13} style={{ height: '300px', width: '100%' }}>
            <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
            <PickLocation
              onPick={(lat, lon) => {
                setLatitudine(lat)
                setLongitudine(lon)
              }}
            />
            {typeof latitudine === 'number' && typeof longitudine === 'number' ? (
              <Marker position={[latitudine, longitudine]} />
            ) : null}
          </MapContainer>
        </div>
      </div>
    </div>
  )
}

