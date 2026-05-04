export function PlaceholderPage({ title }: { title: string }) {
  return (
    <div className="rounded border bg-white p-6 shadow-sm">
      <h2 className="text-lg font-semibold text-slate-900">{title}</h2>
      <p className="mt-2 text-sm text-slate-600">
        Pagina este disponibila ca structura; poti extinde rapid functionalitatea UI aici.
      </p>
    </div>
  )
}

