
export interface Department {
  id: string
  name: string
  isActive: boolean
}

export interface DepartmentSection {
  id: string
  departmentId: string
  name: string
  isActive: boolean
}
