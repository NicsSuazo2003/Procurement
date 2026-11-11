
import { createRouter, createWebHistory } from 'vue-router'

import MainLayout from '@/app/layouts/MainLayout.vue'
import LoginView from '@/features/auth/views/LoginView.vue'
import CatalogueItemsView from '@/features/catalogue-management/views/CatalogueItemsView.vue'

import DashboardView from '@/features/dashboard/views/DashboardView.vue'
import PPMPListView from '@/features/ppmp-app/views/PPMPListView.vue'
import ProcurementProjectsView from '@/features/procurement-projects/views/ProcurementProjectsView.vue'
import CreatePPMPView from '@/features/ppmp-app/views/CreatePPMPView.vue'
import PPMPDetailsView from '@/features/ppmp-app/views/PPMPDetailsView.vue'
import ProcurementPPMPReviewView from '@/features/ppmp-app/views/ProcurementPPMPReviewView.vue'
import ProcurementEditPPMPView from '@/features/ppmp-app/views/ProcurementEditPPMPView.vue'
import UnitHeadApprovalQueueView from '@/features/ppmp-app/views/UnitHeadApprovalQueueView.vue'
import ManagePreferredCategoriesView from '@/features/ppmp-app/views/ManagePreferredCategoriesView.vue'

import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),

  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: {
        requiresAuth: false,
      },
    },

    {
      path: '/',
      component: MainLayout,
      children: [
        {
          path: '',
          redirect: '/dashboard',
        },
        {
          path: 'dashboard',
          name: 'dashboard',
          component: DashboardView,
        },
        {
          path: 'ppmp',
          name: 'ppmp',
          component: PPMPListView,
        },
        {
          path: 'ppmp/create/:id?',
          name: 'ppmp-create',
          component: CreatePPMPView,
        },
        {
          path: 'ppmp/:id',
          name: 'ppmp-details',
          component: PPMPDetailsView,
        },
        {
          path: 'procurement-review',
          name: 'procurement-review',
          component: ProcurementPPMPReviewView,
          meta: {
            roles: ['Procurement Staff'],
          },
        },
        {
          path: 'procurement-review/:id/edit',
          name: 'procurement-edit-ppmp',
          component: ProcurementEditPPMPView,
          meta: {
            roles: ['Procurement Staff'],
          },
        },
        {
  path: 'catalogue/items',
  name: 'catalogue-items',
  component: CatalogueItemsView,
  meta: {
    roles: ['Procurement Staff', 'System Administrator'],
  },
},
        {
          path: 'procurement-projects',
          name: 'procurement-projects',
          component: ProcurementProjectsView,
          meta: {
            roles: ['Procurement Staff', 'System Administrator'],
          },
        },
        {
          path: 'unit-head/ppmp-approvals',
          name: 'unit-head-ppmp-approvals',
          component: UnitHeadApprovalQueueView,
          meta: {
            roles: ['Unit Head'],
          },
        },
        {
  path: 'ppmp/preferences',
  name: 'ppmp-preferences',
  component: ManagePreferredCategoriesView,
  meta: {
    roles: ['End-user'],
  },
},
      ],
    },
  ],
})

router.beforeEach((to) => {
  const authStore = useAuthStore()

  const isPublicRoute = to.matched.some(
    record => record.meta.requiresAuth === false,
  )

  if (!authStore.isAuthenticated && !isPublicRoute) {
    return {
      name: 'login',
      query: {
        redirect: to.fullPath,
      },
    }
  }

  if (authStore.isAuthenticated && to.name === 'login') {
    return { name: 'dashboard' }
  }

  if (authStore.isAuthenticated) {
    const requiredRoles = to.matched.flatMap(record => {
      const roles = record.meta.roles

      return Array.isArray(roles)
        ? roles as string[]
        : []
    })

    if (
      requiredRoles.length > 0 &&
      !authStore.hasAnyRole(requiredRoles)
    ) {
      return { name: 'dashboard' }
    }
  }

  return true
})

export default router
