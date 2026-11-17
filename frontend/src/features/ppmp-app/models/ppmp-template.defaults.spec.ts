import { describe, expect, it } from 'vitest'

import {
  getPPMPTemplateDefaults,
  isPPMPProjectType,
  normalizePPMPProjectType,
  PPMP_PROJECT_TYPES,
} from './ppmp-template.defaults'

describe('PPMP template defaults', () => {
  it.each([
    ['Accessories & Consumables', '2026-10', '2027-01', 'January 2027'],
    ['Maintenance Supplies', '2026-10', '2027-01', 'February 2027'],
    ['Office Equipment', '2027-01', '2027-03', 'April 2027'],
    ['Laboratory Reagents & Supplies', '2027-01', '2027-03', 'March 2027'],
    ['Accountable Forms', '2027-01', '2027-01', 'January 2027'],
    ['Textiles', '2026-10', '2027-01', 'February 2027'],
  ])(
    'uses the workbook schedule for %s',
    (category, start, end, delivery) => {
      expect(getPPMPTemplateDefaults(category)).toEqual({
        recommendedModeOfProcurement: 'Public Bidding',
        startOfProcurementActivity: start,
        endOfProcurementActivity: end,
        expectedDeliveryImplementationPeriod: delivery,
        sourceOfFunds: 'GAA',
        attachedSupportingDocuments: 'Purchase Request, Market Scoping',
      })
    },
  )

  it('uses the equipment schedule for the NBSS category alias', () => {
    expect(
      getPPMPTemplateDefaults('NBSS (Hearing Screening Services)')
        .startOfProcurementActivity,
    ).toBe('2027-01')
  })

  it('offers only the three official project types', () => {
    expect(PPMP_PROJECT_TYPES).toEqual([
      'Goods',
      'Infrastructure',
      'Consulting Services',
    ])
    expect(isPPMPProjectType('Goods')).toBe(true)
    expect(isPPMPProjectType('Services')).toBe(false)
    expect(normalizePPMPProjectType(' consulting services ')).toBe(
      'Consulting Services',
    )
  })
})
