<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
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
  ChevronDown,
  Check,
  CheckCircle2,
  Ban,
  Eye,
  Copy,
  FilterX,
  AlertTriangle,
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
const selectedStatus = ref<'all' | 'active' | 'inactive'>('all')

// Toolbar searchable category filter state
const isFilterCategoryOpen = ref(false)
const filterCategorySearch = ref('')
const filterCategoryRef = ref<HTMLElement | null>(null)
const filterCategoryInputRef = ref<HTMLInputElement | null>(null)

// Pagination State
const currentPage = ref(1)
const itemsPerPage = ref(10)

// Create / Edit Modal State
const showModal = ref(false)
const editingItem = ref<ItemDto | null>(null)

// Item Full Details Modal State
const viewingItem = ref<ItemDto | null>(null)

// Status Toggle Confirmation Modal State
const pendingStatusItem = ref<ItemDto | null>(null)
const isTogglingStatus = ref(false)

// Copy code feedback
const copiedItemId = ref<number | null>(null)

// Searchable Category Dropdown State (Modal)
const isCategoryDropdownOpen = ref(false)
const categorySearchQuery = ref('')
const highlightedCategoryIndex = ref(0)
const categoryDropdownRef = ref<HTMLElement | null>(null)
const categorySearchInputRef = ref<HTMLInputElement | null>(null)

const commonItemTypes = ['Goods', 'Infrastructure', 'Consulting Services']
const commonUnits = ['pc', 'box', 'ream', 'pack', 'bottle', 'set', 'unit', 'lot']

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

const selectedFilterCategoryObj = computed(() => {
  if (selectedCategory.value === 'all') return null
  return categories.value.find((c) => c.categoryId === Number(selectedCategory.value)) ?? null
})

const filteredToolbarCategories = computed(() => {
  const query = filterCategorySearch.value.trim().toLowerCase()
  if (!query) return categories.value

  return categories.value.filter(
    (category) =>
      category.categoryCode.toLowerCase().includes(query) ||
      category.categoryName.toLowerCase().includes(query),
  )
})

const selectedModalCategory = computed(() =>
  activeCategories.value.find((category) => category.categoryId === form.value.categoryId) ??
  categories.value.find((category) => category.categoryId === form.value.categoryId) ??
  null,
)

const filteredModalCategories = computed(() => {
  const query = categorySearchQuery.value.trim().toLowerCase()
  if (!query) return activeCategories.value

  return activeCategories.value.filter(
    (category) =>
      category.categoryCode.toLowerCase().includes(query) ||
      category.categoryName.toLowerCase().includes(query),
  )
})

watch(filteredModalCategories, () => {
  highlightedCategoryIndex.value = 0
})

const hasActiveFilters = computed(
  () =>
    searchQuery.value.trim() !== '' ||
    selectedCategory.value !== 'all' ||
    selectedStatus.value !== 'all',
)

const filteredItems = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()

  return items.value.filter((item) => {
    const matchesSearch =
      !query ||
      item.itemCode.toLowerCase().includes(query) ||
      item.itemName.toLowerCase().includes(query) ||
      item.categoryName.toLowerCase().includes(query) ||
      (item.specifications?.toLowerCase().includes(query) ?? false)

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

function clearAllFilters() {
  searchQuery.value = ''
  selectedCategory.value = 'all'
  selectedStatus.value = 'all'
}

async function copyItemCode(item: ItemDto) {
  try {
    await navigator.clipboard.writeText(item.itemCode)
    copiedItemId.value = item.itemId
    window.setTimeout(() => {
      if (copiedItemId.value === item.itemId) {
        copiedItemId.value = null
      }
    }, 1500)
  } catch {
    // Fallback ignored if clipboard permissions are restricted
  }
}

// Toolbar Category Dropdown Helpers
function toggleFilterCategoryDropdown() {
  isFilterCategoryOpen.value = !isFilterCategoryOpen.value
  if (isFilterCategoryOpen.value) {
    filterCategorySearch.value = ''
    void nextTick(() => {
      filterCategoryInputRef.value?.focus()
    })
  }
}

function selectToolbarCategory(value: string) {
  selectedCategory.value = value
  isFilterCategoryOpen.value = false
  filterCategorySearch.value = ''
}

// Modal Category Dropdown Helpers
function toggleCategoryDropdown() {
  isCategoryDropdownOpen.value = !isCategoryDropdownOpen.value
  if (isCategoryDropdownOpen.value) {
    categorySearchQuery.value = ''
    highlightedCategoryIndex.value = 0
    void nextTick(() => {
      categorySearchInputRef.value?.focus()
    })
  }
}

function selectModalCategory(categoryId: number) {
  form.value.categoryId = categoryId
  isCategoryDropdownOpen.value = false
  categorySearchQuery.value = ''
}

function handleCategorySearchKeydown(event: KeyboardEvent) {
  const list = filteredModalCategories.value
  if (!list.length) return

  if (event.key === 'ArrowDown') {
    event.preventDefault()
    highlightedCategoryIndex.value = (highlightedCategoryIndex.value + 1) % list.length
  } else if (event.key === 'ArrowUp') {
    event.preventDefault()
    highlightedCategoryIndex.value =
      (highlightedCategoryIndex.value - 1 + list.length) % list.length
  } else if (event.key === 'Enter') {
    event.preventDefault()
    const picked = list[highlightedCategoryIndex.value]
    if (picked) {
      selectModalCategory(picked.categoryId)
    }
  }
}

function handleDocumentClick(event: MouseEvent) {
  const target = event.target as Node | null
  if (!target) return

  if (
    isCategoryDropdownOpen.value &&
    categoryDropdownRef.value &&
    !categoryDropdownRef.value.contains(target)
  ) {
    isCategoryDropdownOpen.value = false
  }

  if (
    isFilterCategoryOpen.value &&
    filterCategoryRef.value &&
    !filterCategoryRef.value.contains(target)
  ) {
    isFilterCategoryOpen.value = false
  }
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
  isCategoryDropdownOpen.value = false
  categorySearchQuery.value = ''
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

  isCategoryDropdownOpen.value = false
  categorySearchQuery.value = ''
  errorMessage.value = ''
  successMessage.value = ''
  showModal.value = true
}

function closeModal() {
  if (saving.value) return
  showModal.value = false
  editingItem.value = null
  isCategoryDropdownOpen.value = false
  categorySearchQuery.value = ''
}

async function saveItem() {
  errorMessage.value = ''
  successMessage.value = ''

  if (!form.value.categoryId) {
    errorMessage.value = 'Please select a procurement category.'
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
    isCategoryDropdownOpen.value = false
    await loadData()
  } catch (error) {
    errorMessage.value = getErrorMessage(error)
  } finally {
    saving.value = false
  }
}

function requestToggleActiveStatus(item: ItemDto) {
  pendingStatusItem.value = item
}

async function confirmToggleActiveStatus() {
  const item = pendingStatusItem.value
  if (!item || isTogglingStatus.value) return

  const action = item.isActive ? 'deactivate' : 'activate'
  isTogglingStatus.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await masterDataApiService.setItemActiveStatus(item.itemId, !item.isActive)
    pendingStatusItem.value = null
    successMessage.value = `Catalogue item "${item.itemName}" ${action}d successfully.`
    await loadData()
  } catch (error) {
    errorMessage.value = getErrorMessage(error)
  } finally {
    isTogglingStatus.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleDocumentClick)
  void loadData()
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleDocumentClick)
})
</script>

<template>
  <div class="mx-auto max-w-7xl space-y-6">
    <!-- Page Header -->
    <div class="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
      <div class="flex items-start gap-3.5">
        <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-600/20">
          <Package :size="24" />
        </div>

        <div>
          <h1 class="text-2xl font-bold tracking-tight text-slate-900">
            Master Catalogue Items
          </h1>
          <p class="mt-0.5 text-sm text-slate-500">
            Manage standardized procurement items, units of measure, and technical specifications.
          </p>
        </div>
      </div>

      <button
        type="button"
        class="drmc-btn-primary w-full shadow-xs sm:w-auto"
        @click="openCreateModal"
      >
        <Plus :size="17" />
        Add Catalogue Item
      </button>
    </div>

    <!-- Dismissible Notifications -->
    <div
      v-if="successMessage"
      role="status"
      class="flex items-center justify-between gap-3 rounded-xl border border-brand-200 bg-brand-50 px-4 py-3 text-sm font-medium text-brand-800"
    >
      <div class="flex items-center gap-2">
        <CheckCircle2 :size="18" class="shrink-0 text-brand-600" />
        <span>{{ successMessage }}</span>
      </div>

      <button
        type="button"
        class="rounded-lg p-1 text-brand-700 transition hover:bg-brand-100"
        @click="successMessage = ''"
      >
        <X :size="15" />
      </button>
    </div>

    <div
      v-if="errorMessage && !showModal"
      role="alert"
      class="flex items-center justify-between gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800"
    >
      <div class="flex items-center gap-2">
        <AlertTriangle :size="18" class="shrink-0 text-red-600" />
        <span>{{ errorMessage }}</span>
      </div>

      <button
        type="button"
        class="rounded-lg p-1 text-red-700 transition hover:bg-red-100"
        @click="errorMessage = ''"
      >
        <X :size="15" />
      </button>
    </div>

    <!-- Interactive Summary Cards (Click to Filter) -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <button
        type="button"
        class="drmc-card flex items-center justify-between p-5 text-left transition-all hover:border-slate-300"
        :class="selectedStatus === 'all' ? 'ring-2 ring-slate-900/10' : 'opacity-85 hover:opacity-100'"
        @click="selectedStatus = 'all'"
      >
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
      </button>

      <button
        type="button"
        class="drmc-card flex items-center justify-between p-5 text-left transition-all hover:border-brand-300"
        :class="selectedStatus === 'active' ? 'border-brand-300 ring-2 ring-brand-600/20' : 'opacity-85 hover:opacity-100'"
        @click="selectedStatus = selectedStatus === 'active' ? 'all' : 'active'"
      >
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
      </button>

      <button
        type="button"
        class="drmc-card flex items-center justify-between p-5 text-left transition-all hover:border-slate-300"
        :class="selectedStatus === 'inactive' ? 'border-slate-400 ring-2 ring-slate-500/20' : 'opacity-85 hover:opacity-100'"
        @click="selectedStatus = selectedStatus === 'inactive' ? 'all' : 'inactive'"
      >
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">
            Inactive Items
          </p>
          <p class="mt-2 text-2xl font-bold tracking-tight text-slate-600 tabular-nums sm:text-3xl">
            {{ inactiveCount }}
          </p>
        </div>
        <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-slate-100 text-slate-500">
          <Ban :size="20" />
        </div>
      </button>
    </div>

    <!-- Catalogue Table Card -->
    <div class="drmc-card overflow-hidden">
      <!-- Search & Filter Toolbar -->
      <div class="space-y-3 border-b border-slate-200/80 bg-slate-50/40 p-5">
        <div class="flex flex-col gap-3 lg:flex-row">
          <!-- Search Input with Clear Button -->
          <div class="relative flex-1">
            <Search
              :size="16"
              class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Search by item code, name, category, or specifications..."
              class="drmc-input pl-9 pr-8"
            />
            <button
              v-if="searchQuery"
              type="button"
              title="Clear search"
              class="absolute right-2.5 top-1/2 -translate-y-1/2 rounded-md p-0.5 text-slate-400 hover:bg-slate-200/70 hover:text-slate-600"
              @click="searchQuery = ''"
            >
              <X :size="14" />
            </button>
          </div>

          <!-- Searchable Toolbar Category Filter -->
          <div ref="filterCategoryRef" class="relative lg:w-72">
            <button
              type="button"
              :aria-expanded="isFilterCategoryOpen"
              class="drmc-input flex w-full items-center justify-between gap-2 text-left"
              @click="toggleFilterCategoryDropdown"
              @keydown.esc.stop="isFilterCategoryOpen = false"
            >
              <span
                class="truncate"
                :class="selectedFilterCategoryObj ? 'font-medium text-slate-900' : 'text-slate-600'"
              >
                {{
                  selectedFilterCategoryObj
                    ? `${selectedFilterCategoryObj.categoryCode} - ${selectedFilterCategoryObj.categoryName}`
                    : 'All Categories'
                }}
              </span>
              <ChevronDown
                :size="15"
                class="shrink-0 text-slate-400 transition-transform"
                :class="{ 'rotate-180': isFilterCategoryOpen }"
              />
            </button>

            <div
              v-if="isFilterCategoryOpen"
              class="absolute left-0 right-0 z-30 mt-1.5 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-lg"
              @keydown.esc.stop="isFilterCategoryOpen = false"
            >
              <div class="border-b border-slate-100 p-2">
                <div class="relative">
                  <Search
                    :size="14"
                    class="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400"
                  />
                  <input
                    ref="filterCategoryInputRef"
                    v-model="filterCategorySearch"
                    type="text"
                    placeholder="Filter categories..."
                    class="w-full rounded-lg border border-slate-200 bg-slate-50 py-1.5 pl-8 pr-7 text-xs text-slate-900 outline-none focus:border-brand-600 focus:bg-white"
                  />
                  <button
                    v-if="filterCategorySearch"
                    type="button"
                    class="absolute right-2 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                    @click="filterCategorySearch = ''"
                  >
                    <X :size="13" />
                  </button>
                </div>
              </div>

              <ul class="max-h-56 overflow-y-auto py-1 text-xs">
                <li
                  class="flex cursor-pointer items-center justify-between px-3.5 py-2 transition-colors hover:bg-brand-50/70"
                  :class="selectedCategory === 'all' ? 'bg-brand-50 font-semibold text-brand-700' : 'text-slate-700'"
                  @click="selectToolbarCategory('all')"
                >
                  <span>All Categories</span>
                  <Check v-if="selectedCategory === 'all'" :size="14" class="text-brand-700" />
                </li>

                <li
                  v-for="category in filteredToolbarCategories"
                  :key="category.categoryId"
                  class="flex cursor-pointer items-center justify-between gap-2 px-3.5 py-2 transition-colors hover:bg-brand-50/70"
                  :class="
                    selectedCategory === String(category.categoryId)
                      ? 'bg-brand-50 font-semibold text-brand-700'
                      : 'text-slate-700'
                  "
                  @click="selectToolbarCategory(String(category.categoryId))"
                >
                  <span class="truncate">
                    <span class="font-mono font-semibold">{{ category.categoryCode }}</span>
                    - {{ category.categoryName }}
                  </span>
                  <Check
                    v-if="selectedCategory === String(category.categoryId)"
                    :size="14"
                    class="shrink-0 text-brand-700"
                  />
                </li>

                <li
                  v-if="filteredToolbarCategories.length === 0"
                  class="px-3.5 py-3 text-center text-slate-500"
                >
                  No categories match.
                </li>
              </ul>
            </div>
          </div>

          <!-- Segmented Status Filter -->
          <div class="inline-flex rounded-xl border border-slate-200 bg-white p-1">
            <button
              type="button"
              class="rounded-lg px-3 py-1.5 text-xs font-semibold transition"
              :class="
                selectedStatus === 'all'
                  ? 'bg-slate-900 text-white shadow-2xs'
                  : 'text-slate-600 hover:text-slate-900'
              "
              @click="selectedStatus = 'all'"
            >
              All
            </button>
            <button
              type="button"
              class="rounded-lg px-3 py-1.5 text-xs font-semibold transition"
              :class="
                selectedStatus === 'active'
                  ? 'bg-brand-700 text-white shadow-2xs'
                  : 'text-slate-600 hover:text-slate-900'
              "
              @click="selectedStatus = 'active'"
            >
              Active
            </button>
            <button
              type="button"
              class="rounded-lg px-3 py-1.5 text-xs font-semibold transition"
              :class="
                selectedStatus === 'inactive'
                  ? 'bg-slate-600 text-white shadow-2xs'
                  : 'text-slate-600 hover:text-slate-900'
              "
              @click="selectedStatus = 'inactive'"
            >
              Inactive
            </button>
          </div>

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

        <!-- Active Filter Chips Bar -->
        <div
          v-if="hasActiveFilters"
          class="flex flex-wrap items-center gap-2 pt-1 text-xs"
        >
          <span class="font-medium text-slate-500">Active filters:</span>

          <span
            v-if="searchQuery.trim()"
            class="inline-flex items-center gap-1 rounded-full bg-slate-200/70 px-2.5 py-0.5 font-medium text-slate-700"
          >
            Search: "{{ searchQuery.trim() }}"
            <button type="button" class="hover:text-slate-950" @click="searchQuery = ''">
              <X :size="12" />
            </button>
          </span>

          <span
            v-if="selectedFilterCategoryObj"
            class="inline-flex items-center gap-1 rounded-full bg-brand-50 px-2.5 py-0.5 font-medium text-brand-700 ring-1 ring-inset ring-brand-600/20"
          >
            Category: {{ selectedFilterCategoryObj.categoryName }}
            <button type="button" class="hover:text-brand-950" @click="selectedCategory = 'all'">
              <X :size="12" />
            </button>
          </span>

          <span
            v-if="selectedStatus !== 'all'"
            class="inline-flex items-center gap-1 rounded-full bg-slate-200/70 px-2.5 py-0.5 font-medium capitalize text-slate-700"
          >
            Status: {{ selectedStatus }}
            <button type="button" class="hover:text-slate-950" @click="selectedStatus = 'all'">
              <X :size="12" />
            </button>
          </span>

          <button
            type="button"
            class="ml-auto inline-flex items-center gap-1 font-semibold text-slate-500 hover:text-brand-700"
            @click="clearAllFilters"
          >
            <FilterX :size="13" />
            Clear all
          </button>
        </div>
      </div>

      <!-- Loading Skeletons -->
      <div v-if="loading" class="divide-y divide-slate-100 p-5">
        <div
          v-for="n in 6"
          :key="n"
          class="flex animate-pulse items-center justify-between gap-4 py-4"
        >
          <div class="h-4 w-24 rounded bg-slate-200" />
          <div class="h-4 w-64 rounded bg-slate-200" />
          <div class="h-4 w-36 rounded bg-slate-200" />
          <div class="h-4 w-16 rounded bg-slate-200" />
          <div class="h-6 w-20 rounded-full bg-slate-200" />
          <div class="h-8 w-24 rounded-lg bg-slate-200" />
        </div>
      </div>

      <!-- Table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="border-b border-slate-200/80 bg-slate-50/80 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
            <tr>
              <th class="whitespace-nowrap px-5 py-3.5">Item Code</th>
              <th class="px-5 py-3.5">Item Name & Specifications</th>
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
              class="group transition-colors hover:bg-slate-50/90"
            >
              <!-- Item Code + Quick Copy -->
              <td class="whitespace-nowrap px-5 py-4">
                <div class="inline-flex items-center gap-1.5">
                  <span class="rounded-md bg-brand-50/80 px-2 py-0.5 font-mono text-xs font-semibold text-brand-700 ring-1 ring-inset ring-brand-600/15">
                    {{ item.itemCode }}
                  </span>

                  <button
                    type="button"
                    :title="copiedItemId === item.itemId ? 'Copied!' : 'Copy item code'"
                    class="rounded p-1 text-slate-400 opacity-0 transition hover:bg-slate-200/70 hover:text-slate-700 group-hover:opacity-100 focus:opacity-100"
                    @click="copyItemCode(item)"
                  >
                    <Check v-if="copiedItemId === item.itemId" :size="13" class="text-brand-600" />
                    <Copy v-else :size="13" />
                  </button>
                </div>
              </td>

              <!-- Item Name (Clamped for clean scanability) -->
              <td class="max-w-md px-5 py-4">
                <p
                  class="line-clamp-2 font-semibold text-slate-900"
                  :title="item.itemName"
                >
                  {{ item.itemName }}
                </p>

                <p
                  v-if="item.specifications"
                  class="mt-0.5 line-clamp-1 text-xs text-slate-500"
                  :title="item.specifications"
                >
                  {{ item.specifications }}
                </p>
              </td>

              <td class="whitespace-nowrap px-5 py-4 text-slate-600">
                {{ item.categoryName }}
              </td>

              <td class="whitespace-nowrap px-5 py-4">
                <span
                  v-if="item.itemType"
                  class="rounded-md bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-700"
                >
                  {{ item.itemType }}
                </span>
                <span v-else class="text-slate-400">—</span>
              </td>

              <td class="whitespace-nowrap px-5 py-4 font-medium text-slate-700">
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
                <div class="flex justify-end gap-1.5">
                  <button
                    type="button"
                    title="View full item details"
                    class="rounded-lg border border-slate-200 p-2 text-slate-600 transition hover:border-blue-300 hover:bg-blue-50 hover:text-blue-700"
                    @click="viewingItem = item"
                  >
                    <Eye :size="15" />
                  </button>

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
                    class="rounded-lg border border-slate-200 p-2 transition"
                    :class="
                      item.isActive
                        ? 'text-red-600 hover:border-red-200 hover:bg-red-50'
                        : 'text-brand-700 hover:border-brand-300 hover:bg-brand-50'
                    "
                    @click="requestToggleActiveStatus(item)"
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
                <Package :size="34" class="mx-auto mb-3 text-slate-300" />
                <p class="font-semibold text-slate-700">
                  No catalogue items found.
                </p>
                <p class="mt-1 text-xs">
                  Try adjusting your search query or clearing active filters.
                </p>
                <button
                  v-if="hasActiveFilters"
                  type="button"
                  class="drmc-btn-secondary mt-4 px-3.5 py-1.5 text-xs"
                  @click="clearAllFilters"
                >
                  <FilterX :size="14" />
                  Reset Filters
                </button>
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

    <!-- ================================================================ -->
    <!-- ITEM FULL DETAILS MODAL                                          -->
    <!-- ================================================================ -->
    <div
      v-if="viewingItem"
      class="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-slate-900/55 p-4 backdrop-blur-2xs"
      @click.self="viewingItem = null"
    >
      <div class="my-auto w-full max-w-xl rounded-2xl border border-slate-200 bg-white shadow-2xl">
        <div class="flex items-start justify-between gap-4 border-b border-slate-200/80 bg-slate-50/70 px-6 py-4">
          <div>
            <div class="flex flex-wrap items-center gap-2">
              <span class="rounded-md bg-brand-50 px-2.5 py-0.5 font-mono text-xs font-bold text-brand-700 ring-1 ring-inset ring-brand-600/20">
                {{ viewingItem.itemCode }}
              </span>
              <span
                :class="[
                  'inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset',
                  viewingItem.isActive
                    ? 'bg-brand-50 text-brand-700 ring-brand-600/20'
                    : 'bg-slate-100 text-slate-600 ring-slate-500/20',
                ]"
              >
                <span
                  :class="[
                    'h-1.5 w-1.5 rounded-full',
                    viewingItem.isActive ? 'bg-brand-600' : 'bg-slate-400',
                  ]"
                />
                {{ viewingItem.isActive ? 'Active' : 'Inactive' }}
              </span>
            </div>

            <h2 class="mt-1.5 text-lg font-bold text-slate-900">
              Catalogue Item Details
            </h2>
          </div>

          <button
            type="button"
            class="rounded-lg p-1.5 text-slate-400 transition hover:bg-slate-200/70 hover:text-slate-700"
            @click="viewingItem = null"
          >
            <X :size="20" />
          </button>
        </div>

        <div class="space-y-4 p-6">
          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Item Name
            </p>
            <p class="mt-1 whitespace-pre-wrap text-sm font-semibold leading-relaxed text-slate-900">
              {{ viewingItem.itemName }}
            </p>
          </div>

          <div class="grid grid-cols-1 gap-3 rounded-xl border border-slate-200/80 bg-slate-50/70 p-4 text-xs sm:grid-cols-3">
            <div>
              <p class="font-semibold uppercase tracking-wider text-slate-400">Category</p>
              <p class="mt-1 font-medium text-slate-800">{{ viewingItem.categoryName }}</p>
            </div>
            <div>
              <p class="font-semibold uppercase tracking-wider text-slate-400">Item Type</p>
              <p class="mt-1 font-medium text-slate-800">{{ viewingItem.itemType || '—' }}</p>
            </div>
            <div>
              <p class="font-semibold uppercase tracking-wider text-slate-400">Unit</p>
              <p class="mt-1 font-medium text-slate-800">{{ viewingItem.unit }}</p>
            </div>
          </div>

          <div>
            <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              Technical Specifications
            </p>
            <div class="mt-1.5 max-h-48 overflow-y-auto rounded-xl border border-slate-200/80 bg-slate-50/50 p-3.5 text-xs leading-relaxed text-slate-700">
              <p v-if="viewingItem.specifications" class="whitespace-pre-wrap">
                {{ viewingItem.specifications }}
              </p>
              <p v-else class="italic text-slate-400">
                No specifications provided for this item.
              </p>
            </div>
          </div>
        </div>

        <div class="flex justify-end gap-2.5 border-t border-slate-100 bg-slate-50/60 px-6 py-3.5">
          <button
            type="button"
            class="drmc-btn-secondary px-4 py-2 text-xs"
            @click="viewingItem = null"
          >
            Close
          </button>
          <button
            type="button"
            class="drmc-btn-primary px-4 py-2 text-xs"
            @click="
              () => {
                const target = viewingItem!
                viewingItem = null
                openEditModal(target)
              }
            "
          >
            <Pencil :size="14" />
            Edit Item
          </button>
        </div>
      </div>
    </div>

    <!-- ================================================================ -->
    <!-- CREATE / EDIT MODAL                                              -->
    <!-- ================================================================ -->
    <div
      v-if="showModal"
      class="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-slate-900/55 p-4 backdrop-blur-2xs"
      @click.self="closeModal"
    >
      <div class="my-auto w-full max-w-xl rounded-2xl border border-slate-200 bg-white shadow-2xl">
        <div class="flex items-center justify-between border-b border-slate-200/80 px-6 py-4">
          <div>
            <h2 class="text-lg font-bold text-slate-900">
              {{ editingItem ? 'Edit Catalogue Item' : 'Add Catalogue Item' }}
            </h2>
            <p class="mt-0.5 text-xs text-slate-500">
              {{
                editingItem
                  ? 'Update the master catalogue item details below.'
                  : 'Register a new standardized item in the master catalogue.'
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
            class="flex items-center justify-between rounded-xl border border-slate-200/80 bg-slate-50 px-4 py-3"
          >
            <div>
              <p class="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                Generated Item Code
              </p>
              <p class="mt-0.5 font-mono text-sm font-bold text-brand-700">
                {{ editingItem.itemCode }}
              </p>
            </div>
            <span class="rounded-md bg-slate-200/70 px-2 py-1 text-[11px] font-medium text-slate-600">
              Auto-generated
            </span>
          </div>

          <!-- Searchable Procurement Category Dropdown -->
          <div ref="categoryDropdownRef" class="relative">
            <label class="mb-1.5 block text-xs font-semibold text-slate-700">
              Procurement Category <span class="text-red-600">*</span>
            </label>

            <button
              type="button"
              :aria-expanded="isCategoryDropdownOpen"
              class="drmc-input flex w-full items-center justify-between gap-2 text-left"
              @click="toggleCategoryDropdown"
              @keydown.esc.stop="isCategoryDropdownOpen = false"
            >
              <span
                class="truncate"
                :class="selectedModalCategory ? 'text-slate-900' : 'text-slate-400'"
              >
                {{
                  selectedModalCategory
                    ? `${selectedModalCategory.categoryCode} - ${selectedModalCategory.categoryName}`
                    : 'Select category'
                }}
              </span>

              <ChevronDown
                :size="16"
                class="shrink-0 text-slate-400 transition-transform"
                :class="{ 'rotate-180': isCategoryDropdownOpen }"
              />
            </button>

            <div
              v-if="isCategoryDropdownOpen"
              class="absolute left-0 right-0 z-30 mt-1.5 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-xl"
              @keydown.esc.stop="isCategoryDropdownOpen = false"
            >
              <div class="border-b border-slate-100 p-2">
                <div class="relative">
                  <Search
                    :size="14"
                    class="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400"
                  />
                  <input
                    ref="categorySearchInputRef"
                    v-model="categorySearchQuery"
                    type="text"
                    placeholder="Type category code or name (↑↓ to navigate, Enter to select)..."
                    class="w-full rounded-lg border border-slate-200 bg-slate-50 py-1.5 pl-8 pr-7 text-xs text-slate-900 outline-none focus:border-brand-600 focus:bg-white"
                    @keydown="handleCategorySearchKeydown"
                  />
                  <button
                    v-if="categorySearchQuery"
                    type="button"
                    class="absolute right-2 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                    @click="categorySearchQuery = ''"
                  >
                    <X :size="13" />
                  </button>
                </div>
              </div>

              <ul
                role="listbox"
                class="max-h-56 overflow-y-auto py-1 text-sm"
              >
                <li
                  v-for="(category, idx) in filteredModalCategories"
                  :key="category.categoryId"
                  role="option"
                  :aria-selected="category.categoryId === form.categoryId"
                  class="flex cursor-pointer items-center justify-between gap-2 px-3.5 py-2 transition-colors"
                  :class="[
                    category.categoryId === form.categoryId
                      ? 'bg-brand-50 font-semibold text-brand-700'
                      : idx === highlightedCategoryIndex
                        ? 'bg-slate-100 text-slate-900'
                        : 'text-slate-700 hover:bg-brand-50/70',
                  ]"
                  @mouseenter="highlightedCategoryIndex = idx"
                  @click="selectModalCategory(category.categoryId)"
                >
                  <span class="truncate">
                    <span class="font-mono text-xs font-semibold">{{ category.categoryCode }}</span>
                    - {{ category.categoryName }}
                  </span>

                  <Check
                    v-if="category.categoryId === form.categoryId"
                    :size="15"
                    class="shrink-0 text-brand-700"
                  />
                </li>

                <li
                  v-if="filteredModalCategories.length === 0"
                  class="px-3.5 py-4 text-center text-xs text-slate-500"
                >
                  No matching categories found.
                </li>
              </ul>
            </div>
          </div>

          <!-- Item Name + Character Counter -->
          <div>
            <div class="mb-1.5 flex items-center justify-between">
              <label class="text-xs font-semibold text-slate-700">
                Item Name <span class="text-red-600">*</span>
              </label>
              <span class="text-[11px] text-slate-400 tabular-nums">
                {{ form.itemName.length }}/300
              </span>
            </div>

            <input
              v-model="form.itemName"
              type="text"
              maxlength="300"
              required
              placeholder="Enter standardized item name"
              class="drmc-input"
            />
          </div>

          <!-- Item Type & Unit with Quick-Select Pills -->
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

              <div class="mt-1.5 flex flex-wrap gap-1">
                <button
                  v-for="typeOption in commonItemTypes"
                  :key="typeOption"
                  type="button"
                  class="rounded-md border px-2 py-0.5 text-[11px] font-medium transition"
                  :class="
                    form.itemType === typeOption
                      ? 'border-brand-300 bg-brand-50 text-brand-700'
                      : 'border-slate-200 bg-slate-50 text-slate-600 hover:bg-slate-100'
                  "
                  @click="form.itemType = typeOption"
                >
                  {{ typeOption }}
                </button>
              </div>
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

              <div class="mt-1.5 flex flex-wrap gap-1">
                <button
                  v-for="unitOption in commonUnits"
                  :key="unitOption"
                  type="button"
                  class="rounded-md border px-2 py-0.5 text-[11px] font-medium transition"
                  :class="
                    form.unit === unitOption
                      ? 'border-brand-300 bg-brand-50 text-brand-700'
                      : 'border-slate-200 bg-slate-50 text-slate-600 hover:bg-slate-100'
                  "
                  @click="form.unit = unitOption"
                >
                  {{ unitOption }}
                </button>
              </div>
            </div>
          </div>

          <!-- Specifications + Character Counter -->
          <div>
            <div class="mb-1.5 flex items-center justify-between">
              <label class="text-xs font-semibold text-slate-700">
                Specifications
              </label>
              <span class="text-[11px] text-slate-400 tabular-nums">
                {{ form.specifications.length }}/1000
              </span>
            </div>

            <textarea
              v-model="form.specifications"
              rows="3"
              maxlength="1000"
              placeholder="Enter technical specifications, dimensions, or packaging details..."
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

    <!-- ================================================================ -->
    <!-- ACTIVATE / DEACTIVATE CONFIRMATION MODAL                         -->
    <!-- ================================================================ -->
    <div
      v-if="pendingStatusItem"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/55 p-4 backdrop-blur-2xs"
      @click.self="!isTogglingStatus && (pendingStatusItem = null)"
    >
      <div class="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-6 shadow-2xl">
        <div class="flex items-start gap-3.5">
          <div
            class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl"
            :class="
              pendingStatusItem.isActive
                ? 'bg-red-50 text-red-600'
                : 'bg-brand-50 text-brand-700'
            "
          >
            <Power :size="20" />
          </div>

          <div>
            <h3 class="text-base font-bold text-slate-900">
              {{ pendingStatusItem.isActive ? 'Deactivate Catalogue Item?' : 'Activate Catalogue Item?' }}
            </h3>
            <p class="mt-1.5 text-sm leading-relaxed text-slate-600">
              Are you sure you want to
              <strong>{{ pendingStatusItem.isActive ? 'deactivate' : 'activate' }}</strong>
              <span class="font-semibold text-slate-900">"{{ pendingStatusItem.itemName }}"</span>?
              <span v-if="pendingStatusItem.isActive" class="mt-1 block text-xs text-slate-500">
                Inactive items will no longer appear when end-users create new PPMP requests.
              </span>
            </p>
          </div>
        </div>

        <div class="mt-6 flex justify-end gap-2.5">
          <button
            type="button"
            class="drmc-btn-secondary px-4 py-2 text-xs"
            :disabled="isTogglingStatus"
            @click="pendingStatusItem = null"
          >
            Cancel
          </button>

          <button
            type="button"
            class="inline-flex items-center gap-1.5 rounded-xl px-4 py-2 text-xs font-semibold text-white shadow-xs transition disabled:opacity-50"
            :class="
              pendingStatusItem.isActive
                ? 'bg-red-600 hover:bg-red-700'
                : 'bg-brand-700 hover:bg-brand-800'
            "
            :disabled="isTogglingStatus"
            @click="confirmToggleActiveStatus"
          >
            <RefreshCw v-if="isTogglingStatus" :size="14" class="animate-spin" />
            {{ pendingStatusItem.isActive ? 'Yes, Deactivate' : 'Yes, Activate' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>