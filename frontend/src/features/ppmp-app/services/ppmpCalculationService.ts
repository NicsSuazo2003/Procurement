
import type { PPMPItem } from '../models/ppmp.types'

export interface PPMPSummary {
  totalItems: number
  estimatedBudget: number
}

export const ppmpCalculationService = {
  calculateItemTotal(item: PPMPItem): number {
    return Number(item.quantity || 0) * Number(item.unitPrice || 0)
  },

  calculateEstimatedBudget(items: PPMPItem[]): number {
    return items.reduce(
      (total, item) => total + this.calculateItemTotal(item),
      0,
    )
  },

  calculateTotalItems(items: PPMPItem[]): number {
    return items.length
  },

  calculateTotalQuantity(items: PPMPItem[]): number {
    return items.reduce(
      (total, item) => total + Number(item.quantity || 0),
      0,
    )
  },

  calculateSummary(items: PPMPItem[]): PPMPSummary {
    return {
      totalItems: this.calculateTotalItems(items),
      estimatedBudget: this.calculateEstimatedBudget(items),
    }
  },
}
