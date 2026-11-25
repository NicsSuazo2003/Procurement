<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import {
  Search,
  Plus,
  Pencil,
  Power,
  X,
  Package,
  RefreshCw,
  ChevronLeft,
  ChevronRight,
  CheckCircle2,
  Ban,
} from '@lucide/vue'

import { masterDataApiService } from '@/features/ppmp-app/services/masterDataApiService'

import type {
  ItemDto,
  ProcurementCategoryDto,
  CreateItemRequest,
  UpdateItemRequest,
} from '@/features/ppmp-app/models/master-data.types'

const items = ref<ItemDto[]>([])
const categories = ref<ProcurementCategoryDto[]>([])

const loading = ref(false)
const saving = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const searchQuery = ref('')
const selectedCategory = ref('all')
const selectedStatus = ref('all')

// Pagination State
const currentPage = ref(1)
const itemsPerPage = ref(10)

const showModal = ref(false)
const editingItem = ref<ItemDto | null>(null)

const form = ref({
  categoryId: 0,
  itemName: '',
  itemType: '',
  unit: '',
  specifications: '',
})

const activeCategories = computed(() =>
  categories.value.filter((category) => category.isActive),
)

const filteredItems = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  return items.value.filter((item) => {
    const matchesSearch =
      !query ||
      item.itemCode.toLowerCase().includes(query) ||
      item.itemName.toLowerCase().includes(query) ||
      item.categoryName.toLowerCase().includes(query)

    const matchesCategory =
      selectedCategory.value === 'all' ||
      item.categoryId === Number(selectedCategory.value)

    const matchesStatus =
      selectedStatus.value === 'all' ||
      (selectedStatus.value === 'active' && item.isActive) ||
      (selectedStatus.value === 'inactive' && !item.isActive)

    return matchesSearch && matchesCategory && matchesStatus
  })
})

watch([searchQuery, selectedCategory, selectedStatus], () => {
  currentPage.value = 1
})

const totalPages = computed(() => {
  return Math.ceil(filteredItems.value.length / itemsPerPage.value) || 1
})

const paginatedItems = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  const end = start + itemsPerPage.value
  return filteredItems.value.slice(start, end)
})

const paginationStartItem = computed(() => {
  if (filteredItems.value.length === 0) return 0
  return (currentPage.value - 1) * itemsPerPage.value + 1
})

const paginationEndItem = computed(() => {
  return Math.min(currentPage.value * itemsPerPage.value, filteredItems.value.length)
})

const activeCount = computed(() => items.value.filter((item) => item.isActive).length)
const inactiveCount = computed(() => items.value.filter((item) => !item.isActive).length)

function getErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unexpected error occurred. Please try again.'
}

async function loadData() {
  loading.value = true
  errorMessage.value = ''

  try {
    const [itemResults, categoryResults] = await Promise.all([
      masterDataApiService.getAllItems(),
      masterDataApiService.getProcurementCategories(),
    ])

    items.value = itemResults
    categories.value = categoryResults
  } catch (error) {
    errorMessage.value = getErrorMessage(error)
  } finally {
    loading.value = false
  }
}

function resetForm() {
  form.value = {
    categoryId: activeCategories.value[0]?.categoryId ?? 0,
    itemName: '',
    itemType: '',
    unit: '',
    specifications: '',
  }
}

function openCreateModal() {
  editingItem.value = null
  resetForm()
  errorMessage.value = ''
  successMessage.value = ''
  showModal.value = true
}

function openEditModal(item: ItemDto) {
  editingItem.value = item

  form.value = {
    categoryId: item.categoryId,
    itemName: item.itemName,
    itemType: item.itemType ?? '',
    unit: item.unit,
    specifications: item.specifications ?? '',
  }

  errorMessage.value = ''
  successMessage.value = ''
  showModal.value = true
}

function closeModal() {
  if (saving.value) return
  showModal.value = false
  editingItem.value = null
}

async function saveItem() {
  errorMessage.value = ''
  successMessage.value = ''

  if (!form.value.categoryId) {
    errorMessage.value = 'Please select a category.'
    return
  }

  const request = {
    categoryId: form.value.categoryId,
    itemName: form.value.itemName.trim(),
    itemType: form.value.itemType.trim() || null,
    unit: form.value.unit.trim(),
    specifications: form.value.specifications.trim() || null,
  }

  if (!request.itemName || !request.unit) {
    errorMessage.value = 'Item name and unit are required.'
    return
  }

  saving.value = true

  try {
    if (editingItem.value) {
      const updateRequest: UpdateItemRequest = request
      await masterDataApiService.updateItem(editingItem.value.itemId, updateRequest)
      successMessage.value = 'Catalogue item updated successfully.'
    } else {
      const createRequest: CreateItemRequest = request
      await masterDataApiService.createItem(createRequest)
      successMessage.value = 'Catalogue item created successfully.'
    }

    showModal.value = false
    editingItem.value = null
    await loadData()
  } catch (error) {
    errorMessage.value = getErrorMessage(error)
  } finally {
    saving.value = false
  }
}

async function toggleActiveStatus(item: ItemDto) {
  const action = item.isActive ? 'deactivate' : 'activate'
  const confirmed = window.confirm(
    `Are you sure you want to ${action} "${item.itemName}"?`,
  )

  if (!confirmed) return

  errorMessage.value = ''
  successMessage.value = ''

  try {
    await masterDataApiService.setItemActiveStatus(item.itemId, !item.isActive)
    successMessage.value = `Catalogue item ${action}d successfully.`
    await loadData()
  } catch (error) {
    errorMessage.value = getErrorMessage(error)
  }
}

onMounted(loadData)
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-6">
    <!-- Page Header -->
    <div class="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
      <div class="flex items-start gap-3">
        <div class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <Package :size="22" />
        </div>

        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            Master Catalogue Items
          </h1>
          <p class="mt-1 text-sm text-slate-500">
            Manage the central catalogue of procurement items.
          </p>
        </div>
      </div>

      <button
        type="button"
        class="drmc-btn-primary w-full sm:w-auto"
        @click="openCreateModal"
      >
        <Plus :size="17" />
        Add Catalogue Item
      </button>
    </div>

    <!-- Notifications -->
    <div
      v-if="successMessage"
      role="status"
      class="rounded-xl border border-brand-200 bg-brand-50 px-4 py-3 text-sm font-medium text-brand-800"
    >
      {{ successMessage }}
    </div>

    <div
      v-if="errorMessage && !showModal"
      role="alert"
      class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800"
    >
      {{ errorMessage }}
    </div>

    <!-- Summary Cards -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <div class="drmc-card flex items-center justify-between p-5">
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Total Catalogue Items
          </p>
          <p class="mt-2 text-2xl font-bold tracking-tight text-slate-900 tabular-nums sm:text-3xl">
            {{ items.length }}
          </p>
        </div>
        <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-slate-100 text-slate-600">
          <Package :size="20" />
        </div>
      </div>

      <div class="drmc-card flex items-center justify-between p-5">
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Active Items
          </p>
          <p class="mt-2 text-2xl font-bold tracking-tight text-brand-700 tabular-nums sm:text-3xl">
            {{ activeCount }}
          </p>
        </div>
        <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <CheckCircle2 :size="20" />
        </div>
      </div>

      <div class="drmc-card flex items-center justify-between p-5">
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Inactive Items
          </p>
          <p class="mt-2 text-2xl font-bold tracking-tight text-slate-500 tabular-nums sm:text-3xl">
            {{ inactiveCount }}
          </p>
        </div>
        <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-slate-100 text-slate-400">
          <Ban :size="20" />
        </div>
      </div>
    </div>

    <!-- Catalogue Table -->
    <div class="drmc-card overflow-hidden">
      <!-- Filters -->
      <div class="flex flex-col gap-3 border-b border-slate-200/80 p-5 lg:flex-row">
        <div class="relative flex-1">
          <Search
            :size="16"
            class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
          />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search Item Code, Item Name, or Category..."
            class="drmc-input pl-9"
          />
        </div>

        <select
          v-model="selectedCategory"
          class="drmc-input lg:w-64"
        >
          <option value="all">All Categories</option>
          <option
            v-for="category in categories"
            :key="category.categoryId"
            :value="String(category.categoryId)"
          >
            {{ category.categoryName }}
          </option>
        </select>

        <select
          v-model="selectedStatus"
          class="drmc-input lg:w-44"
        >
          <option value="all">All Statuses</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
        </select>

        <button
          type="button"
          title="Refresh catalogue"
          :disabled="loading"
          class="drmc-btn-secondary px-3"
          @click="loadData"
        >
          <RefreshCw :size="16" :class="{ 'animate-spin': loading }" />
        </button>
      </div>

      <!-- Loading -->
      <div
        v-if="loading"
        class="flex items-center justify-center gap-2 p-14 text-sm font-medium text-slate-500"
      >
        <RefreshCw :size="18" class="animate-spin text-brand-700" />
        Loading catalogue items...
      </div>

      <!-- Table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="border-b border-slate-200/80 bg-slate-50/80 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
            <tr>
              <th class="whitespace-nowrap px-5 py-3.5">Item Code</th>
              <th class="whitespace-nowrap px-5 py-3.5">Item Name</th>
              <th class="whitespace-nowrap px-5 py-3.5">Category</th>
              <th class="whitespace-nowrap px-5 py-3.5">Type</th>
              <th class="whitespace-nowrap px-5 py-3.5">Unit</th>
              <th class="whitespace-nowrap px-5 py-3.5">Status</th>
              <th class="whitespace-nowrap px-5 py-3.5 text-right">Actions</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="item in paginatedItems"
              :key="item.itemId"
              class="transition-colors hover:bg-slate-50/80"
            >
              <td class="whitespace-nowrap px-5 py-4 font-mono text-xs font-semibold text-brand-700">
                {{ item.itemCode }}
              </td>

              <td class="min-w-56 px-5 py-4">
                <p class="font-semibold text-slate-900">
                  {{ item.itemName }}
                </p>

                <p
                  v-if="item.specifications"
                  class="mt-0.5 max-w-xs truncate text-xs text-slate-500"
                  :title="item.specifications"
                >
                  {{ item.specifications }}
                </p>
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-slate-600">
                {{ item.categoryName }}
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-slate-600">
                {{ item.itemType || '—' }}
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-slate-600">
                {{ item.unit }}
              </td>

              <td class="whitespace-nowrap px-5 py-4">
                <span
                  :class="[
                    'inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset',
                    item.isActive
                      ? 'bg-brand-50 text-brand-700 ring-brand-600/20'
                      : 'bg-slate-100 text-slate-600 ring-slate-500/20',
                  ]"
                >
                  <span
                    :class="[
                      'h-1.5 w-1.5 rounded-full',
                      item.isActive ? 'bg-brand-600' : 'bg-slate-400',
                    ]"
                  />
                  {{ item.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>

              <td class="whitespace-nowrap px-5 py-4">
                <div class="flex justify-end gap-2">
                  <button
                    type="button"
                    title="Edit item"
                    class="rounded-lg border border-slate-200 p-2 text-slate-600 transition hover:border-brand-300 hover:bg-brand-50 hover:text-brand-700"
                    @click="openEditModal(item)"
                  >
                    <Pencil :size="15" />
                  </button>

                  <button
                    type="button"
                    :title="item.isActive ? 'Deactivate item' : 'Activate item'"
                    class="rounded-lg border border-slate-200 p-2 transition hover:bg-slate-100"
                    :class="item.isActive ? 'text-red-600' : 'text-brand-700'"
                    @click="toggleActiveStatus(item)"
                  >
                    <Power :size="15" />
                  </button>
                </div>
              </td>
            </tr>

            <tr v-if="filteredItems.length === 0">
              <td
                colspan="7"
                class="px-5 py-14 text-center text-slate-500"
              >
                <Package :size="32" class="mx-auto mb-3 text-slate-300" />
                <p class="font-semibold text-slate-700">
                  No catalogue items found.
                </p>
                <p class="mt-1 text-xs">
                  Try changing your search or filters.
                </p>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Footer -->
      <div
        v-if="!loading && filteredItems.length > 0"
        class="flex flex-col items-center justify-between gap-4 border-t border-slate-100 bg-slate-50/50 px-5 py-3.5 sm:flex-row"
      >
        <div class="flex flex-wrap items-center gap-3 text-xs text-slate-500 tabular-nums">
          <span>
            Showing <span class="font-semibold text-slate-700">{{ paginationStartItem }}</span> to
            <span class="font-semibold text-slate-700">{{ paginationEndItem }}</span> of
            <span class="font-semibold text-slate-700">{{ filteredItems.length }}</span> items
          </span>

          <div class="flex items-center gap-1.5 border-l border-slate-200 pl-3">
            <span>Per page:</span>
            <select
              v-model.number="itemsPerPage"
              class="rounded-lg border border-slate-300 bg-white px-2 py-1 text-xs outline-none focus:border-brand-600"
              @change="currentPage = 1"
            >
              <option :value="10">10</option>
              <option :value="25">25</option>
              <option :value="50">50</option>
              <option :value="100">100</option>
            </select>
          </div>
        </div>

        <div class="flex items-center gap-2">
          <button
            type="button"
            class="drmc-btn-secondary px-3 py-1.5 text-xs"
            :disabled="currentPage === 1"
            @click="currentPage--"
          >
            <ChevronLeft :size="15" />
            Previous
          </button>

          <span class="px-2 text-xs text-slate-600 tabular-nums">
            Page <span class="font-semibold text-slate-900">{{ currentPage }}</span> of
            <span class="font-semibold text-slate-900">{{ totalPages }}</span>
          </span>

          <button
            type="button"
            class="drmc-btn-secondary px-3 py-1.5 text-xs"
            :disabled="currentPage >= totalPages"
            @click="currentPage++"
          >
            Next
            <ChevronRight :size="15" />
          </button>
        </div>
      </div>
    </div>

    <!-- Create / Edit Modal -->
    <div
      v-if="showModal"
      class="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-slate-900/50 p-4 backdrop-blur-2xs"
      @click.self="closeModal"
    >
      <div class="my-auto w-full max-w-xl rounded-2xl border border-slate-200 bg-white shadow-xl">
        <div class="flex items-center justify-between border-b border-slate-200/80 px-6 py-4">
          <div>
            <h2 class="text-lg font-bold text-slate-900">
              {{ editingItem ? 'Edit Catalogue Item' : 'Add Catalogue Item' }}
            </h2>
            <p class="mt-0.5 text-xs text-slate-500">
              {{
                editingItem
                  ? 'Update the item information.'
                  : 'Register a new item in the master catalogue.'
              }}
            </p>
          </div>

          <button
            type="button"
            class="rounded-lg p-2 text-slate-400 transition hover:bg-slate-100 hover:text-slate-700"
            :disabled="saving"
            @click="closeModal"
          >
            <X :size="20" />
          </button>
        </div>

        <form class="space-y-4 p-6" @submit.prevent="saveItem">
          <div
            v-if="editingItem"
            class="rounded-xl border border-slate-200/80 bg-slate-50 p-3.5"
          >
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Generated Item Code
            </p>
            <p class="mt-1 font-mono text-sm font-bold text-brand-700">
              {{ editingItem.itemCode }}
            </p>
            <p class="mt-1 text-xs text-slate-500">
              Item Code is automatically generated and cannot be edited.
            </p>
          </div>

          <div>
            <label class="mb-1.5 block text-xs font-semibold text-slate-700">
              Procurement Category <span class="text-red-600">*</span>
            </label>

            <select
              v-model.number="form.categoryId"
              required
              class="drmc-input"
            >
              <option :value="0" disabled>Select category</option>
              <option
                v-for="category in activeCategories"
                :key="category.categoryId"
                :value="category.categoryId"
              >
                {{ category.categoryCode }} - {{ category.categoryName }}
              </option>
            </select>
          </div>

          <div>
            <label class="mb-1.5 block text-xs font-semibold text-slate-700">
              Item Name <span class="text-red-600">*</span>
            </label>

            <input
              v-model="form.itemName"
              type="text"
              maxlength="300"
              required
              placeholder="Enter item name"
              class="drmc-input"
            />
          </div>

          <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                Item Type
              </label>

              <input
                v-model="form.itemType"
                type="text"
                maxlength="100"
                placeholder="e.g. Goods"
                class="drmc-input"
              />
            </div>

            <div>
              <label class="mb-1.5 block text-xs font-semibold text-slate-700">
                Unit <span class="text-red-600">*</span>
              </label>

              <input
                v-model="form.unit"
                type="text"
                maxlength="50"
                required
                placeholder="e.g. pc, box, ream"
                class="drmc-input"
              />
            </div>
          </div>

          <div>
            <label class="mb-1.5 block text-xs font-semibold text-slate-700">
              Specifications
            </label>

            <textarea
              v-model="form.specifications"
              rows="3"
              maxlength="1000"
              placeholder="Enter item specifications or description"
              class="drmc-input resize-y"
            />
          </div>

          <div
            v-if="errorMessage"
            role="alert"
            class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800"
          >
            {{ errorMessage }}
          </div>

          <div class="flex justify-end gap-2.5 border-t border-slate-100 pt-4">
            <button
              type="button"
              class="drmc-btn-secondary"
              :disabled="saving"
              @click="closeModal"
            >
              Cancel
            </button>

            <button
              type="submit"
              class="drmc-btn-primary"
              :disabled="saving"
            >
              <RefreshCw
                v-if="saving"
                :size="16"
                class="animate-spin"
              />
              {{ saving ? 'Saving...' : editingItem ? 'Save Changes' : 'Create Item' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>