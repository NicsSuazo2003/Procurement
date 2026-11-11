import type { DepartmentSection } from '../models/organization.types'

export const departmentSections: DepartmentSection[] = []

/*
  Example structure for a future confirmed section:

  {
    id: 'section-001',
    departmentId: 'confirmed-department-id',
    name: 'Confirmed Section Name',
    isActive: true,
  }

  Do not add HRMS divisions as universal sections.
  Each section must be associated with its correct department.
*/