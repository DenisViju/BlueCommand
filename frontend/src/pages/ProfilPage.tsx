import { useEffect, useState } from 'react'
import toast from 'react-hot-toast'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import * as utilizatoriApi from '../api/utilizatoriApi'
import * as authApi from '../api/authApi'

const schema = z
  .object({
    parolaActuala: z.string().min(1, 'Obligatoriu'),
    parolaNoua: z.string().min(8, 'Minim 8 caractere'),
    confirma: z.string().min(1, 'Obligatoriu'),
  })
  .refine((x) => x.parolaNoua === x.confirma, { message: 'Parolele nu coincid', path: ['confirma'] })

type FormValues = z.infer<typeof schema>

export function ProfilPage() {
  const [me, setMe] = useState<any>(null)
  const { register, handleSubmit, formState } = useForm<FormValues>({ resolver: zodResolver(schema) })

  useEffect(() => {
    utilizatoriApi.profil().then(setMe)
  }, [])

  const onSubmit = async (v: FormValues) => {
    try {
      await authApi.changePassword(v.parolaActuala, v.parolaNoua)
      toast.success('Parola schimbata')
    } catch (e: any) {
      toast.error(e?.response?.data?.error ?? 'Eroare')
    }
  }

  return (
    <div className="space-y-6">
      <h2 className="text-xl font-semibold text-slate-900">Profil</h2>
      {me ? (
        <div className="rounded border bg-white p-4 shadow-sm text-sm">
          <div>
            <span className="font-medium">Username:</span> {me.username}
          </div>
          <div>
            <span className="font-medium">Nume:</span> {me.nume} {me.prenume}
          </div>
          <div>
            <span className="font-medium">Rol:</span> {me.rol}
          </div>
          <div>
            <span className="font-medium">Sectie:</span> {me.sectieNume ?? '-'}
          </div>
        </div>
      ) : (
        <div>Se incarca...</div>
      )}

      <div className="rounded border bg-white p-4 shadow-sm">
        <div className="text-sm font-medium text-slate-700">Schimba parola</div>
        <form className="mt-4 grid gap-3 max-w-md" onSubmit={handleSubmit(onSubmit)}>
          <input className="rounded border px-3 py-2 text-sm" type="password" placeholder="Parola curenta" {...register('parolaActuala')} />
          {formState.errors.parolaActuala && <div className="text-xs text-red-600">{formState.errors.parolaActuala.message}</div>}
          <input className="rounded border px-3 py-2 text-sm" type="password" placeholder="Parola noua" {...register('parolaNoua')} />
          {formState.errors.parolaNoua && <div className="text-xs text-red-600">{formState.errors.parolaNoua.message}</div>}
          <input className="rounded border px-3 py-2 text-sm" type="password" placeholder="Confirma parola" {...register('confirma')} />
          {formState.errors.confirma && <div className="text-xs text-red-600">{formState.errors.confirma.message}</div>}
          <button disabled={formState.isSubmitting} className="rounded bg-navy px-3 py-2 text-sm text-white disabled:opacity-60">
            Salveaza
          </button>
        </form>
      </div>
    </div>
  )
}

