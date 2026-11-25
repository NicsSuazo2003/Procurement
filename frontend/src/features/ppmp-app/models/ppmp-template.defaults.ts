import type { PPMPPlanningFields } from './ppmp.types'

export const PPMP_PROJECT_TYPES = ['Goods', 'Infrastructure', 'Consulting Services'] as const
export type PPMPProjectType = (typeof PPMP_PROJECT_TYPES)[number]

const OCTOBER_JANUARY_JANUARY = {
  startOfProcurementActivity: '2026-10',
  endOfProcurementActivity: '2027-01',
  expectedDeliveryImplementationPeriod: 'January 2027',
}

const OCTOBER_JANUARY_FEBRUARY = {
  startOfProcurementActivity: '2026-10',
  endOfProcurementActivity: '2027-01',
  expectedDeliveryImplementationPeriod: 'February 2027',
}

const JANUARY_JANUARY_JANUARY = {
  startOfProcurementActivity: '2027-01',
  endOfProcurementActivity: '2027-01',
  expectedDeliveryImplementationPeriod: 'January 2027',
}

const JANUARY_MARCH_MARCH = {
  startOfProcurementActivity: '2027-01',
  endOfProcurementActivity: '2027-03',
  expectedDeliveryImplementationPeriod: 'March 2027',
}

const JANUARY_MARCH_APRIL = {
  startOfProcurementActivity: '2027-01',
  endOfProcurementActivity: '2027-03',
  expectedDeliveryImplementationPeriod: 'April 2027',
}

const CATEGORY_SCHEDULES: Record<
  string,
  Pick<
    PPMPPlanningFields,
    | 'startOfProcurementActivity'
    | 'endOfProcurementActivity'
    | 'expectedDeliveryImplementationPeriod'
  >
> = {
  'accountable forms': JANUARY_JANUARY_JANUARY,
  'accessories and consumables': OCTOBER_JANUARY_JANUARY,
  anesthetics: OCTOBER_JANUARY_JANUARY,
  books: JANUARY_MARCH_APRIL,
  courier: JANUARY_MARCH_APRIL,
  'consumables - pharmacy use only': OCTOBER_JANUARY_JANUARY,
  'drugs and medicines': OCTOBER_JANUARY_JANUARY,
  'drugs and medicines - ophtha use': OCTOBER_JANUARY_JANUARY,
  'dietary supplies and equipment': JANUARY_MARCH_APRIL,
  'electrical equipment': JANUARY_MARCH_APRIL,
  'electrical supplies': OCTOBER_JANUARY_JANUARY,
  'environmental services': JANUARY_MARCH_APRIL,
  'fire fighting': JANUARY_MARCH_APRIL,
  'food supplies (dietary only)': OCTOBER_JANUARY_JANUARY,
  fluids: OCTOBER_JANUARY_JANUARY,
  fuel: JANUARY_MARCH_APRIL,
  'furniture and fixtures': JANUARY_MARCH_APRIL,
  'laboratory equipment': JANUARY_MARCH_APRIL,
  'laboratory reagents and supplies': JANUARY_MARCH_MARCH,
  'housekeeping equipment': JANUARY_MARCH_APRIL,
  'housekeeping supplies': OCTOBER_JANUARY_JANUARY,
  informercial: JANUARY_MARCH_APRIL,
  'it equipment': JANUARY_MARCH_APRIL,
  'it supplies': JANUARY_MARCH_APRIL,
  lpg: OCTOBER_JANUARY_JANUARY,
  'maintenance equipment': JANUARY_MARCH_APRIL,
  'maintenance supplies': OCTOBER_JANUARY_FEBRUARY,
  'medical equipment': JANUARY_MARCH_APRIL,
  'medical supplies - ophtha use': OCTOBER_JANUARY_JANUARY,
  'medical supplies - pharmacy use': OCTOBER_JANUARY_JANUARY,
  'medical supplies - wards and ofcs': OCTOBER_JANUARY_JANUARY,
  nbss: JANUARY_MARCH_APRIL,
  'nbss (hearing screening services)': JANUARY_MARCH_APRIL,
  'office equipment': JANUARY_MARCH_APRIL,
  'office supplies': OCTOBER_JANUARY_JANUARY,
  'peritonial dialysis solutions': OCTOBER_JANUARY_JANUARY,
  'peritonial dialysis supplies': OCTOBER_JANUARY_JANUARY,
  'plumbing equipment': JANUARY_MARCH_APRIL,
  'plumbing supplies': OCTOBER_JANUARY_FEBRUARY,
  'printed forms': JANUARY_MARCH_APRIL,
  'repair and maintenance of he': JANUARY_MARCH_APRIL,
  'security services': JANUARY_MARCH_APRIL,
  'spare parts': OCTOBER_JANUARY_FEBRUARY,
  'sports supplies and materials': JANUARY_MARCH_APRIL,
  subscription: JANUARY_MARCH_APRIL,
  // The workbook's later Textiles rows contain an auto-fill year error.
  textiles: OCTOBER_JANUARY_FEBRUARY,
  'training suppl - iec materials': JANUARY_MARCH_APRIL,
  'training exp - others': JANUARY_MARCH_APRIL,
  'upgrading of physical plant': JANUARY_MARCH_APRIL,
}

function normalizeCategoryName(categoryName: string): string {
  return categoryName
    .trim()
    .toLowerCase()
    .replace(/&/g, 'and')
    .replace(/\s+/g, ' ')
}

export function normalizePPMPProjectType(value: string | null | undefined): string {
  const projectType = value?.trim().toLowerCase()
  return PPMP_PROJECT_TYPES.find((type) => type.toLowerCase() === projectType) ?? 'Goods'
}

export function isPPMPProjectType(value: string): value is PPMPProjectType {
  return PPMP_PROJECT_TYPES.some((type) => type === value)
}

export function getPPMPTemplateDefaults(
  categoryName: string,
  overrides?: Partial<PPMPPlanningFields>,
): PPMPPlanningFields {
  const schedule =
    CATEGORY_SCHEDULES[normalizeCategoryName(categoryName)] ?? JANUARY_MARCH_APRIL

  return {
    recommendedModeOfProcurement: 'Public Bidding',
    ...schedule,
    sourceOfFunds: 'GAA',
    attachedSupportingDocuments: '',
    ...overrides,
  }
}