
<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  LockKeyhole,
  Mail,
  Eye,
  EyeOff,
  ShieldCheck,
} from '@lucide/vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const showPassword = ref(false)
const errorMessage = ref('')

async function handleLogin() {
  errorMessage.value = ''

  if (!email.value.trim() || !password.value) {
    errorMessage.value = 'Please enter your email and password.'
    return
  }

  try {
    await authStore.login({
      email: email.value.trim(),
      password: password.value,
    })

    const redirect = route.query.redirect

    const destination =
      typeof redirect === 'string' &&
      redirect.startsWith('/') &&
      !redirect.startsWith('//') &&
      redirect !== '/login'
        ? redirect
        : '/dashboard'

    await router.replace(destination)
  } catch (error) {
    errorMessage.value =
      error instanceof Error
        ? error.message
        : 'Unable to sign in. Please try again.'
  }
}
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-slate-100 px-4 py-10">

    <div class="w-full max-w-md">

      <!-- Branding -->
      <div class="mb-8 text-center">
        <div class="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-emerald-700 text-3xl font-bold text-white shadow-lg">
          D
        </div>

        <h1 class="text-2xl font-bold text-slate-900">
          DRMC
        </h1>

        <p class="mt-1 text-sm text-slate-500">
          Procurement Records and Monitoring System
        </p>

        <p class="mt-1 text-xs text-slate-400">
          Davao Regional Medical Center
        </p>
      </div>

      <!-- Login Card -->
      <div class="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">

        <div class="mb-7">
          <h2 class="text-xl font-bold text-slate-900">
            Welcome Back
          </h2>

          <p class="mt-1 text-sm text-slate-500">
            Sign in to access your procurement workspace.
          </p>
        </div>

        <form class="space-y-5" @submit.prevent="handleLogin">

          <!-- Email -->
          <div>
            <label
              for="email"
              class="mb-2 block text-sm font-medium text-slate-700"
            >
              Email Address
            </label>

            <div class="relative">
              <Mail
                :size="18"
                class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
              />

              <input
                id="email"
                v-model="email"
                type="email"
                autocomplete="username"
                required
                placeholder="Enter your email"
                class="w-full rounded-lg border border-slate-300 py-3 pl-10 pr-4 text-sm outline-none transition focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
              />
            </div>
          </div>

          <!-- Password -->
          <div>
            <label
              for="password"
              class="mb-2 block text-sm font-medium text-slate-700"
            >
              Password
            </label>

            <div class="relative">
              <LockKeyhole
                :size="18"
                class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
              />

              <input
                id="password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                autocomplete="current-password"
                required
                placeholder="Enter your password"
                class="w-full rounded-lg border border-slate-300 py-3 pl-10 pr-12 text-sm outline-none transition focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
              />

              <button
                type="button"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-700"
                :aria-label="showPassword ? 'Hide password' : 'Show password'"
                @click="showPassword = !showPassword"
              >
                <EyeOff v-if="showPassword" :size="18" />
                <Eye v-else :size="18" />
              </button>
            </div>
          </div>

          <!-- Error -->
          <div
            v-if="errorMessage"
            class="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
            role="alert"
          >
            {{ errorMessage }}
          </div>

          <!-- Submit -->
          <button
            type="submit"
            :disabled="authStore.isLoading"
            class="flex w-full items-center justify-center gap-2 rounded-lg bg-emerald-700 px-4 py-3 text-sm font-semibold text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <span v-if="authStore.isLoading">
              Signing in...
            </span>

            <span v-else>
              Sign In
            </span>
          </button>
        </form>

        <div class="mt-7 flex items-center justify-center gap-2 border-t border-slate-100 pt-5 text-xs text-slate-400">
          <ShieldCheck :size="15" />
          Authorized personnel only
        </div>
      </div>

      <p class="mt-6 text-center text-xs text-slate-400">
        Davao Regional Medical Center
      </p>

    </div>
  </div>
</template>
