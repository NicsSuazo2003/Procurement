<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  LockKeyhole,
  Mail,
  Eye,
  EyeOff,
  ShieldCheck,
  AlertCircle,
  Loader2,
  ArrowRight,
  Building2,
  FileCheck2,
  Activity,
} from '@lucide/vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const showPassword = ref(false)
const capsLockOn = ref(false)
const errorMessage = ref('')

const safeRedirectPath = computed(() => {
  const redirect = route.query.redirect
  return typeof redirect === 'string' &&
    redirect.startsWith('/') &&
    !redirect.startsWith('//') &&
    redirect !== '/login'
    ? redirect
    : null
})

function handleKeyState(event: KeyboardEvent) {
  if (typeof event.getModifierState === 'function') {
    capsLockOn.value = event.getModifierState('CapsLock')
  }
}

function clearError() {
  if (errorMessage.value) {
    errorMessage.value = ''
  }
}

async function handleLogin() {
  errorMessage.value = ''

  if (!email.value.trim() || !password.value) {
    errorMessage.value = 'Please enter both your institutional email and password.'
    return
  }

  try {
    await authStore.login({
      email: email.value.trim(),
      password: password.value,
    })

    await router.replace(safeRedirectPath.value ?? '/dashboard')
  } catch (error) {
    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to sign in. Please verify your credentials and try again.'
  }
}
</script>

<template>
  <div class="flex min-h-screen bg-slate-50 text-slate-900">
    <!-- LEFT BRAND / INSTITUTIONAL PANEL (Desktop) -->
    <aside class="relative hidden w-1/2 flex-col justify-between overflow-hidden bg-emerald-950 p-12 text-white lg:flex xl:w-[46%]">
      <!-- Subtle architectural grid & radial glow -->
      <div
        class="pointer-events-none absolute inset-0 opacity-[0.07]"
        style="background-image: radial-gradient(#ffffff 1px, transparent 1px); background-size: 24px 24px;"
      />
      <div class="pointer-events-none absolute -bottom-32 -left-32 h-96 w-96 rounded-full bg-emerald-600/25 blur-3xl" />
      <div class="pointer-events-none absolute -right-24 top-16 h-72 w-72 rounded-full bg-teal-500/15 blur-3xl" />

      <!-- Top Hospital Identity -->
      <div class="relative z-10 flex items-center gap-3.5">
        <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-emerald-600/90 text-xl font-bold tracking-tight text-white ring-1 ring-white/20 shadow-lg">
          D
        </div>
        <div>
          <p class="text-sm font-semibold tracking-wide text-white">
            Davao Regional Medical Center
          </p>
          <p class="text-xs text-emerald-300/80">
            Department of Health • Tagum City
          </p>
        </div>
      </div>

      <!-- Center System Value Proposition -->
      <div class="relative z-10 my-auto max-w-md space-y-6">
        <div class="inline-flex items-center gap-2 rounded-full bg-emerald-900/80 px-3 py-1 text-xs font-medium text-emerald-200 ring-1 ring-emerald-700/60">
          <Building2 :size="14" class="text-emerald-400" />
          <span>Hospital Procurement & Supply Chain Portal</span>
        </div>

        <h1 class="text-3xl font-bold leading-tight tracking-tight text-white xl:text-4xl">
          Procurement Records &amp; Monitoring System
        </h1>

        <p class="text-sm leading-relaxed text-emerald-100/75">
          Centralized digital workspace for Project Procurement Management Plans (PPMP), unit budget allocations, and real-time review tracking across medical and administrative departments.
        </p>

        <!-- Feature Highlights -->
        <div class="grid grid-cols-2 gap-4 pt-2">
          <div class="rounded-xl border border-emerald-800/70 bg-emerald-900/40 p-3.5">
            <FileCheck2 :size="18" class="text-emerald-400" />
            <p class="mt-2 text-xs font-semibold text-white">PPMP Tracking</p>
            <p class="mt-0.5 text-[11px] leading-normal text-emerald-200/70">
              End-to-end submission &amp; unit head approvals
            </p>
          </div>

          <div class="rounded-xl border border-emerald-800/70 bg-emerald-900/40 p-3.5">
            <Activity :size="18" class="text-emerald-400" />
            <p class="mt-2 text-xs font-semibold text-white">Audit &amp; Oversight</p>
            <p class="mt-0.5 text-[11px] leading-normal text-emerald-200/70">
              Transparent status monitoring &amp; budget visibility
            </p>
          </div>
        </div>
      </div>

      <!-- Bottom Institutional Footer -->
      <div class="relative z-10 flex items-center justify-between border-t border-emerald-800/60 pt-6 text-xs text-emerald-300/70">
        <span>DRMC Procurement Management Office</span>
        <span>Internal Enterprise System</span>
      </div>
    </aside>

    <!-- RIGHT AUTHENTICATION WORKSPACE -->
    <main class="flex flex-1 flex-col justify-between px-4 py-8 sm:px-8 lg:px-12">
      <!-- Top Spacer / Mobile Brand Header -->
      <div class="mx-auto w-full max-w-md lg:hidden">
        <div class="flex flex-col items-center text-center">
          <div class="flex h-14 w-14 items-center justify-center rounded-2xl bg-emerald-700 text-2xl font-bold text-white shadow-md ring-4 ring-emerald-700/10">
            D
          </div>
          <h1 class="mt-3.5 text-xl font-bold tracking-tight text-slate-900">
            DRMC Procurement System
          </h1>
          <p class="mt-0.5 text-xs text-slate-500">
            Davao Regional Medical Center
          </p>
        </div>
      </div>

      <div class="hidden lg:block" />

      <!-- Form Container -->
      <div class="mx-auto my-8 w-full max-w-md">
        <div class="rounded-2xl border border-slate-200/90 bg-white p-7 shadow-sm sm:p-9">
          <div class="mb-7">
            <h2 class="text-2xl font-bold tracking-tight text-slate-900">
              Welcome back
            </h2>
            <p class="mt-1.5 text-sm text-slate-500">
              Sign in with your DRMC credentials to access your workspace.
            </p>
          </div>

          <!-- Redirect Notice (if bounced from a protected route) -->
          <div
            v-if="safeRedirectPath"
            class="mb-5 flex items-center gap-2 rounded-lg border border-blue-200/80 bg-blue-50/70 px-3.5 py-2.5 text-xs text-blue-800"
          >
            <LockKeyhole :size="14" class="shrink-0 text-blue-600" />
            <span>Sign in to continue to your requested page.</span>
          </div>

          <form class="space-y-5" novalidate @submit.prevent="handleLogin">
            <!-- Email Field -->
            <div>
              <label
                for="email"
                class="mb-1.5 block text-sm font-medium text-slate-700"
              >
                Email Address
              </label>

              <div class="relative">
                <Mail
                  :size="18"
                  class="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
                />

                <input
                  id="email"
                  v-model="email"
                  type="email"
                  autocomplete="username"
                  required
                  autofocus
                  :disabled="authStore.isLoading"
                  placeholder="name@drmc.doh.gov.ph"
                  class="w-full rounded-xl border border-slate-300 bg-white py-2.5 pl-10 pr-4 text-sm text-slate-900 placeholder:text-slate-400 transition focus:border-emerald-600 focus:outline-none focus:ring-3 focus:ring-emerald-600/15 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-500"
                  @input="clearError"
                />
              </div>
            </div>

            <!-- Password Field -->
            <div>
              <div class="mb-1.5 flex items-center justify-between">
                <label
                  for="password"
                  class="block text-sm font-medium text-slate-700"
                >
                  Password
                </label>

                <span
                  v-if="capsLockOn"
                  class="rounded bg-amber-100 px-1.5 py-0.5 text-[11px] font-medium text-amber-800"
                >
                  Caps Lock is on
                </span>
              </div>

              <div class="relative">
                <LockKeyhole
                  :size="18"
                  class="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
                />

                <input
                  id="password"
                  v-model="password"
                  :type="showPassword ? 'text' : 'password'"
                  autocomplete="current-password"
                  required
                  :disabled="authStore.isLoading"
                  placeholder="Enter your password"
                  class="w-full rounded-xl border border-slate-300 bg-white py-2.5 pl-10 pr-11 text-sm text-slate-900 placeholder:text-slate-400 transition focus:border-emerald-600 focus:outline-none focus:ring-3 focus:ring-emerald-600/15 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-500"
                  @input="clearError"
                  @keydown="handleKeyState"
                  @keyup="handleKeyState"
                />

                <button
                  type="button"
                  :disabled="authStore.isLoading"
                  class="absolute right-2.5 top-1/2 -translate-y-1/2 rounded-lg p-1.5 text-slate-400 transition hover:bg-slate-100 hover:text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-600/30"
                  :aria-label="showPassword ? 'Hide password' : 'Show password'"
                  @click="showPassword = !showPassword"
                >
                  <EyeOff v-if="showPassword" :size="17" />
                  <Eye v-else :size="17" />
                </button>
              </div>
            </div>

            <!-- Error Alert -->
            <div
              v-if="errorMessage"
              class="flex items-start gap-2.5 rounded-xl border border-red-200 bg-red-50/90 px-3.5 py-3 text-sm text-red-700"
              role="alert"
              aria-live="polite"
            >
              <AlertCircle :size="18" class="mt-0.5 shrink-0 text-red-600" />
              <span>{{ errorMessage }}</span>
            </div>

            <!-- Submit Button -->
            <button
              type="submit"
              :disabled="authStore.isLoading"
              class="group flex w-full items-center justify-center gap-2 rounded-xl bg-emerald-700 px-4 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-emerald-800 focus:outline-none focus:ring-3 focus:ring-emerald-600/30 active:scale-[0.99] disabled:cursor-not-allowed disabled:opacity-65"
            >
              <template v-if="authStore.isLoading">
                <Loader2 :size="18" class="animate-spin" />
                <span>Signing in...</span>
              </template>

              <template v-else>
                <span>Sign In to Workspace</span>
                <ArrowRight
                  :size="16"
                  class="transition-transform group-hover:translate-x-0.5"
                />
              </template>
            </button>
          </form>

          <!-- Security Footer Inside Card -->
          <div class="mt-7 flex items-center justify-between border-t border-slate-100 pt-5 text-xs text-slate-400">
            <span class="inline-flex items-center gap-1.5 font-medium text-slate-500">
              <ShieldCheck :size="15" class="text-emerald-600" />
              Authorized personnel only
            </span>
            <span>Need access? Contact IHOMP</span>
          </div>
        </div>
      </div>

      <!-- Bottom Copyright / Support Note -->
      <footer class="mx-auto w-full max-w-md text-center text-xs text-slate-400">
        &copy; {{ new Date().getFullYear() }} Davao Regional Medical Center. All rights reserved.
      </footer>
    </main>
  </div>
</template>