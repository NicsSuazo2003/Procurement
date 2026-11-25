<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'

import {
  LayoutDashboard,
  ClipboardList,
  FolderKanban,
  Bell,
  Package,
  LogOut,
  Settings,
  Users,
  Menu,
  X,
  FileSpreadsheet,
  FileSearch,
  CheckSquare,
  Building2,
  ChevronLeft,
  ChevronRight,
} from '@lucide/vue'

import { useAuthStore } from '@/stores/auth'
import { useUnitHeadApprovalsStore } from '@/stores/unit-head-approvals'
import { useProcurementReviewCountStore } from '@/stores/procurement-review-count'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const unitHeadApprovals = useUnitHeadApprovalsStore()
const procurementReview = useProcurementReviewCountStore()

const isMobileMenuOpen = ref(false)
const isSidebarCollapsed = ref(false)

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

const isTeamManager = computed(() =>
  authStore.hasAnyRole(['Unit PPMP Coordinator', 'System Administrator']),
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

const totalPendingAlerts = computed(
  () => unitHeadPendingCount.value + procurementPendingCount.value,
)

const navSections = computed(() => [
  {
    title: 'Overview',
    items: [
      {
        label: 'Dashboard',
        to: '/dashboard',
        icon: LayoutDashboard,
        visible: true,
        badge: 0,
        badgeAria: '',
      },
    ],
  },
  {
    title: 'Procurement Planning',
    items: [
      {
        label: 'Procurement Requests',
        to: '/ppmp',
        icon: ClipboardList,
        visible: true,
        badge: 0,
        badgeAria: '',
      },
      {
        label: 'PPMP Approval Queue',
        to: '/unit-head/ppmp-approvals',
        icon: CheckSquare,
        visible: isUnitHead.value,
        badge: unitHeadPendingCount.value,
        badgeAria: `${unitHeadPendingCount.value} PPMP(s) awaiting your approval`,
      },
      {
        label: 'Procurement Review',
        to: '/procurement-review',
        icon: FileSearch,
        visible: isProcurementOrAdmin.value,
        badge: procurementPendingCount.value,
        badgeAria: `${procurementPendingCount.value} PPMP(s) awaiting Procurement review`,
      },
      {
        label: 'Annual Procurement Plans',
        to: '/app',
        icon: FileSpreadsheet,
        visible: isProcurementOrAdmin.value,
        badge: 0,
        badgeAria: '',
      },
      {
        label: 'Procurement Projects',
        to: '/procurement-projects',
        icon: FolderKanban,
        visible: isProcurementOrAdmin.value,
        badge: 0,
        badgeAria: '',
      },
    ],
  },
  {
    title: 'Catalogue & Configuration',
    items: [
      {
        label: 'Catalogue Management',
        to: '/catalogue/items',
        icon: Package,
        visible: isProcurementOrAdmin.value,
        badge: 0,
        badgeAria: '',
      },
      {
        label: 'Preferred Categories',
        to: '/ppmp/preferences',
        icon: Settings,
        visible: isEndUser.value,
        badge: 0,
        badgeAria: '',
      },
      {
        label: 'Department Teams',
        to: '/ppmp/teams',
        icon: Users,
        visible: isTeamManager.value,
        badge: 0,
        badgeAria: '',
      },
    ],
  },
])

function isNavActive(to: string): boolean {
  if (to === '/ppmp') {
    return (
      route.path === '/ppmp' ||
      (route.path.startsWith('/ppmp/') &&
        !route.path.startsWith('/ppmp/preferences') &&
        !route.path.startsWith('/ppmp/teams'))
    )
  }
  return route.path === to || route.path.startsWith(`${to}/`)
}

let badgePollInterval: number | null = null

function closeMobileMenu() {
  isMobileMenuOpen.value = false
}

function refreshRoleBadges() {
  if (isUnitHead.value) {
    void unitHeadApprovals.refresh()
  }
  if (isProcurementStaff.value) {
    void procurementReview.refresh()
  }
}

watch(
  () => router.currentRoute.value.fullPath,
  closeMobileMenu,
)

function handleUnauthorized() {
  authStore.logout(false)
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

  if (isUnitHead.value || isProcurementStaff.value) {
    refreshRoleBadges()
    badgePollInterval = window.setInterval(refreshRoleBadges, 60_000)
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
  <div class="min-h-screen bg-surface-muted text-slate-900">
    <!-- Mobile Sidebar Overlay -->
    <Transition name="fade">
      <div
        v-if="isMobileMenuOpen"
        class="print:hidden fixed inset-0 z-40 bg-slate-950/60 backdrop-blur-xs lg:hidden"
        @click="closeMobileMenu"
      />
    </Transition>

    <!-- Sidebar -->
    <aside
      class="print:hidden fixed inset-y-0 left-0 z-50 flex w-72 max-w-[85vw] flex-col bg-brand-950 text-white shadow-xl transition-[width,transform] duration-300 lg:z-30 lg:max-w-none lg:translate-x-0 lg:shadow-none"
      :class="[
        isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full',
        isSidebarCollapsed ? 'lg:w-20' : 'lg:w-64',
      ]"
    >
      <!-- Sidebar Header -->
      <div
        class="flex h-16 shrink-0 items-center border-b border-brand-800/70"
        :class="isSidebarCollapsed ? 'justify-between px-5 lg:justify-center lg:px-2' : 'justify-between px-5'"
      >
        <RouterLink
          to="/dashboard"
          class="flex min-w-0 items-center gap-3"
          :class="{ 'lg:justify-center': isSidebarCollapsed }"
          :title="isSidebarCollapsed ? 'DRMC Procurement System' : undefined"
        >
          <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-600 text-lg font-bold text-white ring-1 ring-white/20 shadow-sm">
            D
          </div>

          <div class="min-w-0" :class="{ 'lg:hidden': isSidebarCollapsed }">
            <h1 class="truncate text-sm font-bold tracking-tight text-white">DRMC</h1>
            <p class="truncate text-[11px] text-brand-300/80">Procurement System</p>
          </div>
        </RouterLink>

        <!-- Mobile Close Button -->
        <button
          type="button"
          class="rounded-lg p-1.5 text-brand-300 transition hover:bg-brand-900 hover:text-white lg:hidden"
          aria-label="Close navigation menu"
          @click="closeMobileMenu"
        >
          <X :size="20" />
        </button>
      </div>

      <!-- Navigation -->
      <nav class="flex-1 space-y-6 overflow-y-auto px-3.5 py-5" aria-label="Sidebar Navigation">
        <template v-for="section in navSections" :key="section.title">
          <div v-if="section.items.some(item => item.visible)" class="space-y-1">
            <p
              class="px-3 pb-1.5 text-[10px] font-bold uppercase tracking-wider text-brand-400/80"
              :class="{ 'lg:hidden': isSidebarCollapsed }"
            >
              {{ section.title }}
            </p>

            <RouterLink
              v-for="item in section.items.filter(i => i.visible)"
              :key="item.to"
              :to="item.to"
              :class="[
                'group relative flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all duration-150',
                isSidebarCollapsed ? 'lg:justify-center' : '',
                isNavActive(item.to)
                  ? 'bg-brand-700 text-white shadow-xs ring-1 ring-white/15'
                  : 'text-brand-100/80 hover:bg-brand-900/80 hover:text-white',
              ]"
              :title="isSidebarCollapsed ? item.label : undefined"
            >
              <component
                :is="item.icon"
                :size="18"
                :class="[
                  'shrink-0 transition-colors',
                  isNavActive(item.to)
                    ? 'text-white'
                    : 'text-brand-400 group-hover:text-brand-200',
                ]"
              />

              <span
                class="flex-1 truncate"
                :class="{ 'lg:hidden': isSidebarCollapsed }"
              >{{ item.label }}</span>

              <span
                v-if="item.badge > 0"
                :class="[
                  'inline-flex min-w-[1.5rem] items-center justify-center rounded-full bg-amber-400 px-1.5 py-0.5 text-[11px] font-bold text-slate-950 shadow-xs tabular-nums',
                  isSidebarCollapsed
                    ? 'lg:absolute lg:-right-1 lg:-top-1 lg:min-w-4 lg:px-1 lg:text-[9px]'
                    : '',
                ]"
                :aria-label="item.badgeAria"
              >
                {{ item.badge > 99 ? '99+' : item.badge }}
              </span>
            </RouterLink>
          </div>
        </template>
      </nav>

      <!-- Sidebar Footer -->
      <div
        class="shrink-0 border-t border-brand-800/70 p-4"
        :class="{ 'lg:p-3': isSidebarCollapsed }"
      >
        <div
          class="flex items-center gap-2.5 rounded-xl bg-brand-900/60 px-3 py-2.5 ring-1 ring-inset ring-brand-800/60"
          :class="{ 'lg:justify-center lg:px-0': isSidebarCollapsed }"
          :title="isSidebarCollapsed ? 'Davao Regional Medical Center' : undefined"
        >
          <Building2 :size="16" class="shrink-0 text-brand-400" />
          <div class="min-w-0" :class="{ 'lg:hidden': isSidebarCollapsed }">
            <p class="truncate text-xs font-semibold text-white">
              DRMC Procurement
            </p>
            <p class="truncate text-[11px] text-brand-300/75">
              Davao Regional Medical Center
            </p>
          </div>
        </div>
      </div>
    </aside>

    <!-- Main Area -->
    <div
      class="flex min-h-screen flex-col print:pl-0"
      :class="isSidebarCollapsed ? 'lg:pl-20' : 'lg:pl-64'"
    >
      <!-- Topbar -->
      <header class="print:hidden sticky top-0 z-20 flex h-16 shrink-0 items-center justify-between gap-3 border-b border-slate-200/90 bg-white/90 px-4 backdrop-blur-md sm:px-6">
        <div class="flex min-w-0 items-center gap-3">
          <button
            type="button"
            class="hidden shrink-0 rounded-lg border border-slate-200 bg-white p-2 text-slate-600 transition hover:bg-slate-50 hover:text-brand-700 lg:inline-flex"
            :aria-label="isSidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'"
            :aria-expanded="!isSidebarCollapsed"
            @click="isSidebarCollapsed = !isSidebarCollapsed"
          >
            <ChevronRight v-if="isSidebarCollapsed" :size="18" />
            <ChevronLeft v-else :size="18" />
          </button>

          <!-- Mobile Menu Button -->
          <button
            type="button"
            class="shrink-0 rounded-lg border border-slate-200 bg-white p-2 text-slate-600 transition hover:bg-slate-50 hover:text-brand-700 lg:hidden"
            aria-label="Open navigation menu"
            :aria-expanded="isMobileMenuOpen"
            @click="isMobileMenuOpen = !isMobileMenuOpen"
          >
            <Menu v-if="!isMobileMenuOpen" :size="20" />
            <X v-else :size="20" />
          </button>

          <div class="min-w-0">
            <h2 class="truncate text-xs font-semibold text-slate-900 sm:text-sm">
              Procurement Records and Monitoring System
            </h2>
            <p class="truncate text-[11px] text-slate-500">
              Davao Regional Medical Center
            </p>
          </div>
        </div>

        <div class="flex shrink-0 items-center gap-2 sm:gap-3.5">
          <!-- Notifications Shortcut -->
          <RouterLink
            v-if="isUnitHead || isProcurementStaff"
            :to="isUnitHead ? '/unit-head/ppmp-approvals' : '/procurement-review'"
            class="relative rounded-xl border border-transparent p-2 text-slate-500 transition hover:border-slate-200 hover:bg-slate-50 hover:text-brand-700"
            aria-label="Pending workflow notifications"
          >
            <Bell :size="19" />
            <span
              v-if="totalPendingAlerts > 0"
              class="absolute right-1.5 top-1.5 h-2 w-2 rounded-full bg-amber-500 ring-2 ring-white"
            />
          </RouterLink>

          <div class="hidden h-6 w-px bg-slate-200 sm:block" />

          <!-- User Profile Chip -->
          <div class="flex items-center gap-2.5 rounded-xl border border-slate-200/80 bg-slate-50/70 py-1.5 pl-3 pr-1.5">
            <div class="hidden text-right md:block">
              <p class="max-w-44 truncate text-xs font-semibold text-slate-900">
                {{ currentUser?.fullName }}
              </p>
              <p class="max-w-44 truncate text-[11px] text-slate-500">
                {{ currentUser?.roleNames?.join(', ') }}
              </p>
            </div>

            <div class="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-brand-700 text-xs font-bold text-white shadow-2xs">
              {{ userInitials }}
            </div>
          </div>

          <!-- Logout -->
          <button
            type="button"
            class="inline-flex items-center gap-1.5 rounded-xl border border-slate-200 bg-white px-2.5 py-2 text-xs font-semibold text-slate-600 transition hover:border-red-200 hover:bg-red-50 hover:text-red-700 sm:px-3"
            aria-label="Logout"
            @click="handleLogout"
          >
            <LogOut :size="15" />
            <span class="hidden sm:inline">Logout</span>
          </button>
        </div>
      </header>

      <!-- Page Content -->
      <main class="min-w-0 flex-1 p-4 sm:p-6">
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