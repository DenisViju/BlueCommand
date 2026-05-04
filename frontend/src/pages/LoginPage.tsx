import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useAuth } from '../context/AuthContext'

const schema = z.object({
  username: z.string().min(1, 'Username este obligatoriu'),
  parola: z.string().min(1, 'Parola este obligatorie'),
})

type FormValues = z.infer<typeof schema>

export function LoginPage() {
  const { login, isAuthenticated } = useAuth()
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) })

  if (isAuthenticated) {
    window.location.href = '/dashboard'
    return null
  }

  const onSubmit = async (values: FormValues) => {
    await login(values.username, values.parola)
    window.location.href = '/dashboard'
  }

  return (
    <div className="min-h-full flex items-center justify-center bg-slate-50 p-6">
      <div className="w-full max-w-sm rounded-lg border bg-white p-6 shadow">
        <div className="text-center">
          <div className="mx-auto mb-2 h-10 w-10 rounded bg-navy" />
          <h1 className="text-xl font-semibold text-slate-900">BlueCommand</h1>
          <p className="text-sm text-slate-600">Autentificare</p>
        </div>

        <form className="mt-6 space-y-4" onSubmit={handleSubmit(onSubmit)}>
          <div>
            <label className="block text-sm font-medium text-slate-700">Username</label>
            <input
              className="mt-1 w-full rounded border px-3 py-2 text-sm"
              {...register('username')}
            />
            {errors.username ? (
              <p className="mt-1 text-xs text-red-600">{errors.username.message}</p>
            ) : null}
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700">Parola</label>
            <input
              type="password"
              className="mt-1 w-full rounded border px-3 py-2 text-sm"
              {...register('parola')}
            />
            {errors.parola ? (
              <p className="mt-1 text-xs text-red-600">{errors.parola.message}</p>
            ) : null}
          </div>
          <button
            disabled={isSubmitting}
            className="w-full rounded bg-navy px-3 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-60"
          >
            {isSubmitting ? 'Se autentifica...' : 'Login'}
          </button>
        </form>
      </div>
    </div>
  )
}

