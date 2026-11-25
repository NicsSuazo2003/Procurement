<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Plus, Users, RefreshCw, UserPlus } from '@lucide/vue'

import { useAuthStore } from '@/stores/auth'
import { teamApiService, type TeamUserDto } from '../services/teamApiService'
import type { TeamDto } from '../models/team.types'

const authStore = useAuthStore()
const departmentId = computed(() => authStore.user?.departmentId ?? null)

const teams = ref<TeamDto[]>([])
const users = ref<TeamUserDto[]>([])
const selectedTeamId = ref<number | null>(null)
const selectedUserId = ref<number | null>(null)
const teamCode = ref('')
const teamName = ref('')
const isLoading = ref(true)
const isSaving = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

async function load(): Promise<void> {
  if (!departmentId.value) {
    errorMessage.value = 'Your account has no assigned department.'
    isLoading.value = false
    return
  }

  isLoading.value = true
  errorMessage.value = ''

  try {
    const [loadedTeams, loadedUsers] = await Promise.all([
      teamApiService.getByDepartment(departmentId.value),
      teamApiService.getDepartmentUsers(departmentId.value),
    ])
    teams.value = loadedTeams
    users.value = loadedUsers
    selectedTeamId.value = loadedTeams[0]?.teamId ?? null
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to load department teams.'
  } finally {
    isLoading.value = false
  }
}

async function createTeam(): Promise<void> {
  if (!departmentId.value || isSaving.value) return

  const code = teamCode.value.trim()
  const name = teamName.value.trim()
  if (!code || !name) {
    errorMessage.value = 'Team code and team name are required.'
    return
  }

  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const team = await teamApiService.create(departmentId.value, code, name)
    teams.value.push(team)
    selectedTeamId.value = team.teamId
    teamCode.value = ''
    teamName.value = ''
    successMessage.value = `${team.teamName} was created.`
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to create the team.'
  } finally {
    isSaving.value = false
  }
}

async function addMember(): Promise<void> {
  if (!selectedTeamId.value || !selectedUserId.value || isSaving.value) return

  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await teamApiService.addMember(selectedTeamId.value, selectedUserId.value)
    const user = users.value.find(value => value.userId === selectedUserId.value)
    successMessage.value = user ? `${user.fullName} was assigned to the selected team.` : 'User assigned to the team.'
    selectedUserId.value = null
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Unable to assign the user.'
  } finally {
    isSaving.value = false
  }
}

onMounted(() => void load())
</script>

<template>
  <div class="mx-auto max-w-5xl space-y-6">
    <!-- PAGE HEADER -->
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div class="flex items-start gap-3">
        <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <Users :size="22" />
        </div>

        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">Department Teams</h1>
          <p class="mt-1 text-sm text-slate-500">
            Create teams and assign active users within your department.
          </p>
        </div>
      </div>

      <button
        type="button"
        class="drmc-btn-secondary"
        :disabled="isLoading || isSaving"
        @click="load"
      >
        <RefreshCw :size="16" :class="{ 'animate-spin': isLoading }" />
        Refresh
      </button>
    </div>

    <!-- ALERTS -->
    <div
      v-if="successMessage"
      role="status"
      class="rounded-xl border border-brand-200 bg-brand-50 px-4 py-3 text-sm font-medium text-brand-800"
    >
      {{ successMessage }}
    </div>
    <div
      v-if="errorMessage"
      role="alert"
      class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800"
    >
      {{ errorMessage }}
    </div>

    <!-- LOADING -->
    <div v-if="isLoading" class="drmc-card p-14 text-center">
      <RefreshCw :size="26" class="mx-auto mb-3 animate-spin text-brand-700" />
      <p class="text-sm font-medium text-slate-600">Loading department teams...</p>
    </div>

    <template v-else>
      <!-- CREATE TEAM -->
      <section class="drmc-card p-5">
        <h2 class="text-base font-semibold text-slate-900">Create Team</h2>
        <p class="mt-0.5 text-xs text-slate-500">Define a sub-group or working team inside your department.</p>

        <div class="mt-4 grid gap-3 sm:grid-cols-3">
          <input
            v-model="teamCode"
            type="text"
            placeholder="Team code, e.g. HRIS"
            class="drmc-input"
            :disabled="isSaving"
          />
          <input
            v-model="teamName"
            type="text"
            placeholder="Team name, e.g. HRIS Team"
            class="drmc-input"
            :disabled="isSaving"
          />
          <button
            type="button"
            class="drmc-btn-primary"
            :disabled="isSaving"
            @click="createTeam"
          >
            <Plus :size="16" />
            Create Team
          </button>
        </div>
      </section>

      <!-- ASSIGN USER -->
      <section class="drmc-card p-5">
        <h2 class="text-base font-semibold text-slate-900">Assign Department User</h2>
        <p class="mt-0.5 text-xs text-slate-500">Add an active department member to an existing team.</p>

        <div class="mt-4 grid gap-3 sm:grid-cols-3">
          <select
            v-model="selectedTeamId"
            class="drmc-input"
            :disabled="isSaving || teams.length === 0"
          >
            <option :value="null" disabled>Select team</option>
            <option v-for="team in teams" :key="team.teamId" :value="team.teamId">
              {{ team.teamCode }} — {{ team.teamName }}
            </option>
          </select>

          <select
            v-model="selectedUserId"
            class="drmc-input"
            :disabled="isSaving || users.length === 0"
          >
            <option :value="null" disabled>Select user</option>
            <option v-for="user in users" :key="user.userId" :value="user.userId">
              {{ user.fullName }} ({{ user.email }})
            </option>
          </select>

          <button
            type="button"
            class="drmc-btn-secondary border-brand-600/40 text-brand-700 hover:bg-brand-50"
            :disabled="isSaving || !selectedTeamId || !selectedUserId"
            @click="addMember"
          >
            <UserPlus :size="16" />
            Assign User
          </button>
        </div>
      </section>

      <!-- TEAMS LIST -->
      <section class="drmc-card overflow-hidden">
        <div class="flex items-center justify-between border-b border-slate-200/80 px-5 py-4">
          <h2 class="text-base font-semibold text-slate-900">Active Teams</h2>
          <span class="rounded-full bg-slate-100 px-3 py-0.5 text-xs font-semibold text-slate-700 tabular-nums">
            {{ teams.length }} team(s)
          </span>
        </div>

        <div v-if="teams.length === 0" class="p-12 text-center">
          <Users :size="32" class="mx-auto mb-3 text-slate-300" />
          <p class="text-sm font-semibold text-slate-800">No teams created yet</p>
          <p class="mt-1 text-xs text-slate-500">Use the form above to create your first department team.</p>
        </div>

        <div v-else class="divide-y divide-slate-100">
          <div
            v-for="team in teams"
            :key="team.teamId"
            class="flex items-center justify-between px-5 py-4 transition-colors hover:bg-slate-50/80"
          >
            <div class="flex items-center gap-3">
              <span class="rounded-lg bg-brand-50 px-2.5 py-1 font-mono text-xs font-bold text-brand-700 ring-1 ring-inset ring-brand-600/20">
                {{ team.teamCode }}
              </span>
              <p class="font-semibold text-slate-900">{{ team.teamName }}</p>
            </div>
            <span class="text-xs font-medium text-slate-400 tabular-nums">ID #{{ team.teamId }}</span>
          </div>
        </div>
      </section>
    </template>
  </div>
</template>