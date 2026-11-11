<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  Search,
  Plus,
  Pencil,
  Power,
  X,
  Package,
  RefreshCw,
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
  categories.value.filter(category => category.isActive),
)

const filteredItems = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  return items.value.filter(item => {
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

const activeCount = computed(() =>
  items.value.filter(item => item.isActive).length,
)

const inactiveCount = computed(() =>
  items.value.filter(item => !item.isActive).length,
)

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

      await masterDataApiService.updateItem(
        editingItem.value.itemId,
        updateRequest,
      )

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
    await masterDataApiService.setItemActiveStatus(
      item.itemId,
      !item.isActive,
    )

    successMessage.value = `Catalogue item ${action}d successfully.`

    await loadData()
  } catch (error) {
    errorMessage.value = getErrorMessage(error)
  }
}

onMounted(loadData)
</script>

<template>
  <div class="space-y-6">

    <!-- Page Header -->
    <div class="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
      <div>
        <div class="flex items-center gap-2 text-sm text-slate-500">
          <Package :size="17" />
          Catalogue Management
        </div>

        <h1 class="mt-2 text-2xl font-bold text-slate-900">
          Master Catalogue Items
        </h1>

        <p class="mt-1 text-sm text-slate-500">
          Manage the central catalogue of procurement items.
        </p>
      </div>

      <button
        type="button"
        class="flex items-center justify-center gap-2 rounded-lg bg-emerald-700 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-emerald-800"
        @click="openCreateModal"
      >
        <Plus :size="18" />
        Add Catalogue Item
      </button>
    </div>

    <!-- Notifications -->
    <div
      v-if="successMessage"
      class="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800"
    >
      {{ successMessage }}
    </div>

    <div
      v-if="errorMessage && !showModal"
      class="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
    >
      {{ errorMessage }}
    </div>

    <!-- Summary Cards -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <div class="rounded-xl border border-slate-200 bg-white p-5">
        <p class="text-sm text-slate-500">Total Catalogue Items</p>
        <p class="mt-2 text-2xl font-bold text-slate-900">
          {{ items.length }}
        </p>
      </div>

      <div class="rounded-xl border border-slate-200 bg-white p-5">
        <p class="text-sm text-slate-500">Active Items</p>
        <p class="mt-2 text-2xl font-bold text-emerald-700">
          {{ activeCount }}
        </p>
      </div>

      <div class="rounded-xl border border-slate-200 bg-white p-5">
        <p class="text-sm text-slate-500">Inactive Items</p>
        <p class="mt-2 text-2xl font-bold text-slate-500">
          {{ inactiveCount }}
        </p>
      </div>
    </div>

    <!-- Catalogue Table -->
    <div class="overflow-hidden rounded-xl border border-slate-200 bg-white">

      <!-- Filters -->
      <div class="flex flex-col gap-3 border-b border-slate-200 p-4 lg:flex-row">
        <div class="relative flex-1">
          <Search
            :size="18"
            class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
          />

          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search Item Code, Item Name, or Category..."
            class="w-full rounded-lg border border-slate-300 py-2.5 pl-10 pr-4 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
          />
        </div>

        <select
          v-model="selectedCategory"
          class="rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-emerald-600"
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
          class="rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-emerald-600"
        >
          <option value="all">All Statuses</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
        </select>

        <button
          type="button"
          title="Refresh catalogue"
          class="flex items-center justify-center rounded-lg border border-slate-300 px-3 py-2.5 text-slate-600 hover:bg-slate-50"
          @click="loadData"
        >
          <RefreshCw :size="17" />
        </button>
      </div>

      <!-- Loading -->
      <div
        v-if="loading"
        class="flex items-center justify-center gap-2 p-12 text-sm text-slate-500"
      >
        <RefreshCw :size="18" class="animate-spin" />
        Loading catalogue items...
      </div>

      <!-- Table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="bg-slate-50 text-xs uppercase tracking-wide text-slate-500">
            <tr>
              <th class="px-5 py-4 font-semibold">Item Code</th>
              <th class="px-5 py-4 font-semibold">Item Name</th>
              <th class="px-5 py-4 font-semibold">Category</th>
              <th class="px-5 py-4 font-semibold">Type</th>
              <th class="px-5 py-4 font-semibold">Unit</th>
              <th class="px-5 py-4 font-semibold">Status</th>
              <th class="px-5 py-4 text-right font-semibold">Actions</th>
            </tr>
          </thead>

          <tbody class="divide-y divide-slate-100">
            <tr
              v-for="item in filteredItems"
              :key="item.itemId"
              class="hover:bg-slate-50"
            >
              <td class="whitespace-nowrap px-5 py-4 font-semibold text-emerald-800">
                {{ item.itemCode }}
              </td>

              <td class="min-w-56 px-5 py-4">
                <p class="font-medium text-slate-900">
                  {{ item.itemName }}
                </p>

                <p
                  v-if="item.specifications"
                  class="mt-1 max-w-xs truncate text-xs text-slate-500"
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
                  class="rounded-full px-2.5 py-1 text-xs font-semibold"
                  :class="item.isActive
                    ? 'bg-emerald-100 text-emerald-700'
                    : 'bg-slate-100 text-slate-500'"
                >
                  {{ item.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>

              <td class="whitespace-nowrap px-5 py-4">
                <div class="flex justify-end gap-2">
                  <button
                    type="button"
                    title="Edit item"
                    class="rounded-lg border border-slate-200 p-2 text-slate-600 hover:bg-emerald-50 hover:text-emerald-700"
                    @click="openEditModal(item)"
                  >
                    <Pencil :size="16" />
                  </button>

                  <button
                    type="button"
                    :title="item.isActive ? 'Deactivate item' : 'Activate item'"
                    class="rounded-lg border border-slate-200 p-2 hover:bg-slate-100"
                    :class="item.isActive
                      ? 'text-red-600'
                      : 'text-emerald-700'"
                    @click="toggleActiveStatus(item)"
                  >
                    <Power :size="16" />
                  </button>
                </div>
              </td>
            </tr>

            <tr v-if="filteredItems.length === 0">
              <td
                colspan="7"
                class="px-5 py-12 text-center text-slate-500"
              >
                <Package
                  :size="32"
                  class="mx-auto mb-3 text-slate-300"
                />

                <p class="font-medium text-slate-700">
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

      <div
        v-if="!loading"
        class="border-t border-slate-200 px-5 py-3 text-xs text-slate-500"
      >
        Showing {{ filteredItems.length }} of {{ items.length }} catalogue items
      </div>
    </div>

    <!-- Create / Edit Modal -->
    <div
      v-if="showModal"
      class="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-slate-900/50 p-4"
      @click.self="closeModal"
    >
      <div class="my-auto w-full max-w-xl rounded-xl bg-white shadow-xl">

        <div class="flex items-center justify-between border-b border-slate-200 px-6 py-4">
          <div>
            <h2 class="text-lg font-bold text-slate-900">
              {{ editingItem ? 'Edit Catalogue Item' : 'Add Catalogue Item' }}
            </h2>

            <p class="mt-1 text-xs text-slate-500">
              {{
                editingItem
                  ? 'Update the item information.'
                  : 'Register a new item in the master catalogue.'
              }}
            </p>
          </div>

          <button
            type="button"
            class="rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
            :disabled="saving"
            @click="closeModal"
          >
            <X :size="20" />
          </button>
        </div>

        <form class="space-y-4 p-6" @submit.prevent="saveItem">

          <div
            v-if="editingItem"
            class="rounded-lg bg-slate-50 p-3"
          >
            <p class="text-xs text-slate-500">Generated Item Code</p>
            <p class="mt-1 font-bold text-emerald-800">
              {{ editingItem.itemCode }}
            </p>
            <p class="mt-1 text-xs text-slate-500">
              Item Code is automatically generated and cannot be edited.
            </p>
          </div>

          <div>
            <label class="mb-1.5 block text-sm font-medium text-slate-700">
              Procurement Category <span class="text-red-500">*</span>
            </label>

            <select
              v-model.number="form.categoryId"
              required
              class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
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
            <label class="mb-1.5 block text-sm font-medium text-slate-700">
              Item Name <span class="text-red-500">*</span>
            </label>

            <input
              v-model="form.itemName"
              type="text"
              maxlength="300"
              required
              placeholder="Enter item name"
              class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"
            />
          </div>

          <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label class="mb-1.5 block text-sm font-medium text-slate-700">
                Item Type
              </label>

              <input
                v-model="form.itemType"
                type="text"
                maxlength="100"
                placeholder="e.g. Goods"
                class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600"
              />
            </div>

            <div>
              <label class="mb-1.5 block text-sm font-medium text-slate-700">
                Unit <span class="text-red-500">*</span>
              </label>

              <input
                v-model="form.unit"
                type="text"
                maxlength="50"
                required
                placeholder="e.g. pc, box, ream"
                class="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600"
              />
            </div>
          </div>

          <div>
            <label class="mb-1.5 block text-sm font-medium text-slate-700">
              Specifications
            </label>

            <textarea
              v-model="form.specifications"
              rows="3"
              maxlength="1000"
              placeholder="Enter item specifications or description"
              class="w-full resize-y rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none focus:border-emerald-600"
            />
          </div>

          <div
            v-if="errorMessage"
            class="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
          >
            {{ errorMessage }}
          </div>

          <div class="flex justify-end gap-3 border-t border-slate-100 pt-4">
            <button
              type="button"
              class="rounded-lg border border-slate-300 px-4 py-2.5 text-sm font-medium text-slate-600 hover:bg-slate-50"
              :disabled="saving"
              @click="closeModal"
            >
              Cancel
            </button>

            <button
              type="submit"
              class="flex items-center gap-2 rounded-lg bg-emerald-700 px-5 py-2.5 text-sm font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
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