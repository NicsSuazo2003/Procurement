<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { RouterLink, RouterView, useRouter } from 'vue-router'

import {
  LayoutDashboard,
  ClipboardList,
  FolderKanban,
  Bell,
  Package,
  LogOut,
  Settings,
  Menu,
  X,
} from '@lucide/vue'

import { useAuthStore } from '@/stores/auth'
import { useUnitHeadApprovalsStore } from '@/stores/unit-head-approvals'
import { useProcurementReviewCountStore } from '@/stores/procurement-review-count'

const router = useRouter()
const authStore = useAuthStore()
const unitHeadApprovals = useUnitHeadApprovalsStore()
const procurementReview = useProcurementReviewCountStore()

const isMobileMenuOpen = ref(false)

const currentUser = computed(() => authStore.user)

const userInitials = computed(() => {
  const name = currentUser.value?.fullName ?? 'User'

  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map(part => part.charAt(0).toUpperCase())
    .join('')
})

const isProcurementOrAdmin = computed(() =>
  authStore.hasAnyRole([
    'Procurement Staff',
    'System Administrator',
  ]),
)

const isEndUser = computed(() =>
  authStore.hasRole('End-user'),
)

const isUnitHead = computed(() =>
  authStore.hasRole('Unit Head'),
)

const isProcurementStaff = computed(() =>
  authStore.hasRole('Procurement Staff'),
)

const unitHeadPendingCount = computed(() =>
  isUnitHead.value ? unitHeadApprovals.pendingCount : 0,
)

const procurementPendingCount = computed(() =>
  isProcurementStaff.value ? procurementReview.pendingCount : 0,
)

let badgePollInterval: number | null = null

function closeMobileMenu() {
  isMobileMenuOpen.value = false
}

watch(
  () => router.currentRoute.value.fullPath,
  closeMobileMenu,
)

// Refresh both badges when the route changes (e.g., after a Unit Head
// approves a PPMP or Procurement starts a review, then navigates away).
watch(
  () => router.currentRoute.value.fullPath,
  () => {
    if (isUnitHead.value) {
      void unitHeadApprovals.refresh()
    }

    if (isProcurementStaff.value) {
      void procurementReview.refresh()
    }
  },
)

function handleUnauthorized() {
  authStore.logout()
  unitHeadApprovals.reset()
  procurementReview.reset()
  closeMobileMenu()

  if (router.currentRoute.value.name !== 'login') {
    router.replace({
      name: 'login',
      query: {
        reason: 'session-expired',
      },
    })
  }
}

function handleLogout() {
  authStore.logout()
  unitHeadApprovals.reset()
  procurementReview.reset()
  closeMobileMenu()
  router.replace({ name: 'login' })
}

onMounted(() => {
  window.addEventListener('drmc:unauthorized', handleUnauthorized)

  const shouldPoll =
    isUnitHead.value || isProcurementStaff.value

  if (shouldPoll) {
    if (isUnitHead.value) {
      void unitHeadApprovals.refresh()
    }

    if (isProcurementStaff.value) {
      void procurementReview.refresh()
    }

    badgePollInterval = window.setInterval(() => {
      if (isUnitHead.value) {
        void unitHeadApprovals.refresh()
      }

      if (isProcurementStaff.value) {
        void procurementReview.refresh()
      }
    }, 60_000)
  }
})

onUnmounted(() => {
  window.removeEventListener('drmc:unauthorized', handleUnauthorized)

  if (badgePollInterval !== null) {
    window.clearInterval(badgePollInterval)
    badgePollInterval = null
  }
})
</script>

<template>
  <div class="min-h-screen bg-slate-100 text-slate-800">

    <!-- Mobile Sidebar Overlay -->
    <Transition name="fade">
      <div
        v-if="isMobileMenuOpen"
        class="fixed inset-0 z-40 bg-slate-950/50 lg:hidden"
        @click="closeMobileMenu"
      />
    </Transition>

    <!-- Sidebar -->
    <aside
      class="fixed inset-y-0 left-0 z-50 flex w-72 max-w-[85vw] flex-col border-r border-slate-200 bg-white transition-transform duration-300 lg:z-30 lg:w-64 lg:max-w-none lg:translate-x-0"
      :class="isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'"
    >

      <!-- Sidebar Header -->
      <div class="flex h-16 shrink-0 items-center justify-between border-b border-slate-200 px-6">
        <div class="flex items-center gap-3">
          <div class="flex h-10 w-10 items-center justify-center rounded-lg bg-emerald-700 font-bold text-white">
            D
          </div>

          <div>
            <h1 class="font-bold text-slate-900">DRMC</h1>
            <p class="text-xs text-slate-500">Procurement System</p>
          </div>
        </div>

        <!-- Mobile Close Button -->
        <button
          type="button"
          class="rounded-lg p-2 text-slate-500 hover:bg-slate-100 lg:hidden"
          aria-label="Close navigation menu"
          @click="closeMobileMenu"
        >
          <X :size="21" />
        </button>
      </div>

      <!-- Navigation -->
      <nav class="flex-1 space-y-1 overflow-y-auto p-4">

        <p class="mb-3 px-3 text-xs font-semibold uppercase tracking-wider text-slate-400">
          Main Menu
        </p>

        <RouterLink
          to="/dashboard"
          class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-emerald-50 hover:text-emerald-700"
          active-class="bg-emerald-50 text-emerald-700"
        >
          <LayoutDashboard :size="19" />
          Dashboard
        </RouterLink>

        <RouterLink
          to="/ppmp"
          class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-emerald-50 hover:text-emerald-700"
          active-class="bg-emerald-50 text-emerald-700"
        >
          <ClipboardList :size="19" />
          PPMP Management
        </RouterLink>

        <RouterLink
          v-if="isEndUser"
          to="/ppmp/preferences"
          class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-emerald-50 hover:text-emerald-700"
          active-class="bg-emerald-50 text-emerald-700"
        >
          <Settings :size="19" />
          Preferred Categories
        </RouterLink>

        <RouterLink
          v-if="isProcurementOrAdmin"
          to="/catalogue/items"
          class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-emerald-50 hover:text-emerald-700"
          active-class="bg-emerald-50 text-emerald-700"
        >
          <Package :size="19" />
          Catalogue Management
        </RouterLink>

        <RouterLink
          v-if="isProcurementOrAdmin"
          to="/procurement-projects"
          class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-emerald-50 hover:text-emerald-700"
          active-class="bg-emerald-50 text-emerald-700"
        >
          <FolderKanban :size="19" />
          Procurement Projects
        </RouterLink>

        <RouterLink
          v-if="isProcurementOrAdmin"
          to="/procurement-review"
          class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-emerald-50 hover:text-emerald-700"
          active-class="bg-emerald-50 text-emerald-700"
        >
          <ClipboardList :size="19" />
          <span class="flex-1">Procurement Review</span>

          <span
            v-if="procurementPendingCount > 0"
            class="inline-flex min-w-[1.5rem] items-center justify-center rounded-full bg-red-500 px-2 py-0.5 text-[11px] font-bold text-white shadow-sm"
            :aria-label="`${procurementPendingCount} PPMP(s) awaiting Procurement review`"
          >
            {{ procurementPendingCount > 99 ? '99+' : procurementPendingCount }}
          </span>
        </RouterLink>

        <RouterLink
          v-if="isUnitHead"
          to="/unit-head/ppmp-approvals"
          class="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-emerald-50 hover:text-emerald-700"
          active-class="bg-emerald-50 text-emerald-700"
        >
          <ClipboardList :size="19" />
          <span class="flex-1">PPMP Approval Queue</span>

          <span
            v-if="unitHeadPendingCount > 0"
            class="inline-flex min-w-[1.5rem] items-center justify-center rounded-full bg-red-500 px-2 py-0.5 text-[11px] font-bold text-white shadow-sm"
            :aria-label="`${unitHeadPendingCount} PPMP(s) awaiting your approval`"
          >
            {{ unitHeadPendingCount > 99 ? '99+' : unitHeadPendingCount }}
          </span>
        </RouterLink>

      </nav>

      <!-- Sidebar Footer -->
      <div class="shrink-0 border-t border-slate-200 px-4 py-3">
        <p class="text-xs text-slate-400">
          DRMC Procurement System
        </p>
        <p class="text-xs text-slate-400">
          Davao Regional Medical Center
        </p>
      </div>

    </aside>

    <!-- Main Area -->
    <div class="min-h-screen lg:pl-64">

      <!-- Topbar -->
      <header class="sticky top-0 z-20 flex min-h-16 items-center justify-between gap-3 border-b border-slate-200 bg-white px-4 py-3 sm:px-6">

        <div class="flex min-w-0 items-center gap-3">

          <!-- Mobile Menu Button -->
          <button
            type="button"
            class="shrink-0 rounded-lg p-2 text-slate-600 transition hover:bg-slate-100 hover:text-emerald-700 lg:hidden"
            aria-label="Open navigation menu"
            :aria-expanded="isMobileMenuOpen"
            @click="isMobileMenuOpen = !isMobileMenuOpen"
          >
            <Menu v-if="!isMobileMenuOpen" :size="22" />
            <X v-else :size="22" />
          </button>

          <div class="min-w-0">
            <h2 class="truncate text-xs font-semibold text-slate-800 sm:text-sm">
              Procurement Records and Monitoring System
            </h2>

            <p class="truncate text-[11px] text-slate-500 sm:text-xs">
              Davao Regional Medical Center
            </p>
          </div>

        </div>

        <div class="flex shrink-0 items-center gap-2 sm:gap-5">

          <!-- Notifications -->
          <button
            type="button"
            class="relative rounded-lg p-2 text-slate-500 transition hover:bg-emerald-50 hover:text-emerald-700"
            aria-label="Notifications"
          >
            <Bell :size="20" />
          </button>

          <!-- User Information -->
          <div class="flex items-center gap-2 sm:gap-3">

            <div class="hidden text-right md:block">
              <p class="max-w-40 truncate text-sm font-semibold text-slate-800">
                {{ currentUser?.fullName }}
              </p>

              <p class="max-w-40 truncate text-xs text-slate-500">
                {{ currentUser?.roleNames?.join(', ') }}
              </p>
            </div>

            <div class="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-emerald-100 text-sm font-semibold text-emerald-700">
              {{ userInitials }}
            </div>

          </div>

          <!-- Logout -->
          <button
            type="button"
            class="flex items-center gap-2 rounded-lg p-2 text-sm font-medium text-slate-600 transition hover:bg-red-50 hover:text-red-700 sm:px-3 sm:py-2"
            aria-label="Logout"
            @click="handleLogout"
          >
            <LogOut :size="18" />
            <span class="hidden sm:inline">Logout</span>
          </button>

        </div>

      </header>

      <!-- Page Content -->
      <main class="min-w-0 p-4 sm:p-6">
        <RouterView />
      </main>

    </div>
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>