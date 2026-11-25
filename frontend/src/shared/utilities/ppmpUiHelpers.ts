export interface StatusBadgeStyle {
  pill: string
  dot: string
}

export function getPPMPStatusStyle(status: string): StatusBadgeStyle {
  switch (status.trim().toLowerCase()) {
    case 'draft':
      return {
        pill: 'bg-slate-100 text-slate-700 ring-slate-600/20',
        dot: 'bg-slate-400',
      }
    case 'for unit head approval':
      return {
        pill: 'bg-amber-50 text-amber-800 ring-amber-600/25',
        dot: 'bg-amber-500',
      }
    case 'approved by unit head':
      return {
        pill: 'bg-blue-50 text-blue-700 ring-blue-600/25',
        dot: 'bg-blue-500',
      }
    case 'under review':
      return {
        pill: 'bg-purple-50 text-purple-700 ring-purple-600/25',
        dot: 'bg-purple-500',
      }
    case 'updated':
      return {
        pill: 'bg-orange-50 text-orange-800 ring-orange-600/25',
        dot: 'bg-orange-500',
      }
    case 'reviewed':
      return {
        pill: 'bg-brand-50 text-brand-700 ring-brand-600/25',
        dot: 'bg-brand-600',
      }
    case 'included in app':
      return {
        pill: 'bg-teal-50 text-teal-800 ring-teal-600/25',
        dot: 'bg-teal-600',
      }
    default:
      return {
        pill: 'bg-slate-100 text-slate-700 ring-slate-600/20',
        dot: 'bg-slate-400',
      }
  }
}

export function formatPHP(amount: number): string {
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

export function formatDatePH(
  value: string | null | undefined,
  options: { includeTime?: boolean; fallback?: string } = {},
): string {
  const { includeTime = false, fallback = '—' } = options
  if (!value) return fallback

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  return new Intl.DateTimeFormat('en-PH', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    ...(includeTime ? { hour: 'numeric', minute: '2-digit' } : {}),
  }).format(date)
}