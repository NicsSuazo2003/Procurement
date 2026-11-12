<script setup lang="ts">
import type { CreatePPMPViewModel } from '../viewmodels/useCreatePPMPViewModel'

type PrintPreviewViewModel = Pick<
  CreatePPMPViewModel,
  | 'activeCategories'
  | 'categoryTotal'
  | 'currentDepartment'
  | 'form'
  | 'formatCurrency'
  | 'getDescription'
  | 'getItemsForCategory'
  | 'getProjectType'
  | 'currentUserName'
>

const props = defineProps<{ viewModel: PrintPreviewViewModel }>()
const {
  activeCategories,
  categoryTotal,
  currentDepartment,
  form,
  formatCurrency,
  getDescription,
  getItemsForCategory,
  getProjectType,
  currentUserName,
} = props.viewModel
</script>

<template>
  <!-- PRINT DOCUMENTS -->
  <section
    v-for="category in activeCategories"
    :key="`doc-${category.categoryId}`"
    class="ppmp-document hidden bg-white print:block"
  >
    <div class="ppmp-header">
      <div class="ppmp-header__meta-right">
        <div>Reference Code:</div>
        <div class="ppmp-header__meta-value">HOP-QR-PRO-001</div>
        <div class="ppmp-header__meta-spacer"></div>
        <div>Revision No.:</div>
        <div class="ppmp-header__meta-value">0</div>
        <div class="ppmp-header__meta-spacer"></div>
        <div>Effective Date:</div>
        <div class="ppmp-header__meta-value">January 1, 2026</div>
      </div>

      <div class="ppmp-header__republic">Republic of the Philippines</div>
      <div class="ppmp-header__agency">DAVAO REGIONAL MEDICAL CENTER</div>
      <div class="ppmp-header__title">
        PROJECT PROCUREMENT MANAGEMENT PLAN (PPMP) NO.
        <span class="ppmp-header__placeholder">___</span>
      </div>

      <div class="ppmp-header__info-grid">
        <div class="ppmp-header__info-row">
          <span class="ppmp-header__info-label">Fiscal Year :</span>
          <span class="ppmp-header__info-value">{{ form.planningYear || '' }}</span>
        </div>
        <div class="ppmp-header__info-row">
          <span class="ppmp-header__info-label">End-User or Implementing Unit :</span>
          <span class="ppmp-header__info-value">
            {{ currentDepartment?.departmentName ?? form.department ?? '' }}
          </span>
        </div>
        <div class="ppmp-header__info-row">
          <span class="ppmp-header__info-label">Category :</span>
          <span class="ppmp-header__info-value">{{ category.categoryName }}</span>
        </div>
      </div>
    </div>

    <div
      v-if="
        getItemsForCategory(category.categoryId).filter((item) => Number(item.quantity) > 0)
          .length === 0
      "
      class="ppmp-empty-note"
    >
      No items with quantities in this category.
    </div>

    <div v-else class="ppmp-table-wrap">
      <table class="ppmp-table">
        <colgroup>
          <col style="width: 12%" />
          <col style="width: 7%" />
          <col style="width: 4%" />
          <col style="width: 5%" />
          <col style="width: 6%" />
          <col style="width: 7%" />
          <col style="width: 6%" />
          <col style="width: 7%" />
          <col style="width: 7%" />
          <col style="width: 8%" />
          <col style="width: 6%" />
          <col style="width: 9%" />
          <col style="width: 8%" />
          <col style="width: 8%" />
        </colgroup>

        <thead>
          <tr class="ppmp-head-group">
            <th colspan="5">PROCUREMENT PROJECT DETAILS</th>
            <th>RECOMMENDED MODE OF PROCUREMENT</th>
            <th>PRE-PROCUREMENT CONFERENCE (YES / NO)</th>
            <th colspan="3">PROJECTED TIMELINE (MM/YYYY)</th>
            <th colspan="2">FUNDING DETAILS</th>
            <th>ATTACHED SUPPORTING DOCUMENTS</th>
            <th>REMARKS</th>
          </tr>
          <tr class="ppmp-head-col">
            <th>General Description and Objective of the Project to be Procured</th>
            <th>Type of the Project to be Procured</th>
            <th>Quantity and Size</th>
            <th>Unit of Issue</th>
            <th>ABC / unit</th>
            <th></th>
            <th></th>
            <th>Start of Procurement Activity</th>
            <th>End of Procurement Activity</th>
            <th>Expected Delivery / Implementation Period</th>
            <th>Source of Funds</th>
            <th>Estimated Budget / Authorized Budgetary Allocation (PhP)</th>
            <th></th>
            <th></th>
          </tr>
          <tr class="ppmp-head-cols-num">
            <th>Column 1</th>
            <th>Column 2</th>
            <th>Column 3</th>
            <th></th>
            <th></th>
            <th>Column 4</th>
            <th>Column 5</th>
            <th>Column 6</th>
            <th>Column 7</th>
            <th>Column 8</th>
            <th>Column 9</th>
            <th>Column 10</th>
            <th>Column 11</th>
            <th>Column 12</th>
          </tr>
          <tr class="ppmp-head-sub">
            <th></th>
            <th></th>
            <th>Quantity</th>
            <th>Unit of Issue</th>
            <th>ABC/unit</th>
            <th></th>
            <th></th>
            <th></th>
            <th></th>
            <th></th>
            <th></th>
            <th></th>
            <th></th>
            <th></th>
          </tr>
        </thead>

        <tbody>
          <tr
            v-for="item in getItemsForCategory(category.categoryId).filter(
              (entry) => Number(entry.quantity) > 0,
            )"
            :key="item.lineId"
            class="ppmp-row"
          >
            <td class="ppmp-cell ppmp-cell--desc">
              <div class="ppmp-item-name">
                {{ getDescription(item) }}
              </div>
              <div class="ppmp-item-spec">
                Section allocations:
                <span
                  v-for="(allocation, allocationIndex) in item.sectionAllocations"
                  :key="allocation.sectionId"
                >
                  {{ allocation.sectionName }} ({{ allocation.quantity }})<span
                    v-if="allocationIndex < (item.sectionAllocations?.length ?? 0) - 1"
                    >,
                  </span>
                </span>
              </div>
            </td>

            <td class="ppmp-cell ppmp-cell--center">
              {{ getProjectType(item) || '—' }}
            </td>

            <td class="ppmp-cell ppmp-cell--center">
              {{ item.quantity }}
            </td>

            <td class="ppmp-cell ppmp-cell--center">
              {{ item.unit }}
            </td>

            <td class="ppmp-cell ppmp-cell--right">
              {{ formatCurrency(item.unitPrice) }}
            </td>

            <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>

            <td class="ppmp-cell ppmp-cell--center">
              {{ item.preProcurementConference === true ? 'Yes' : 'No' }}
            </td>

            <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>
            <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>
            <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>
            <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>

            <td class="ppmp-cell ppmp-cell--right ppmp-cell--budget">
              {{ formatCurrency(item.quantity * item.unitPrice) }}
            </td>

            <td class="ppmp-cell ppmp-cell--center ppmp-cell--muted">—</td>
            <td class="ppmp-cell ppmp-cell--center">{{ item.category }}</td>
          </tr>

          <tr class="ppmp-total-row">
            <td colspan="11" class="ppmp-total-label">TOTAL BUDGET:</td>
            <td class="ppmp-total-value">
              {{ formatCurrency(categoryTotal(category.categoryId)) }}
            </td>
            <td colspan="2"></td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="ppmp-signatures">
      <div class="ppmp-signature-col">
        <div class="ppmp-signature-label">Prepared by:</div>
        <div class="ppmp-signature-line"></div>
        <div class="ppmp-signature-name">
          {{ currentUserName }}
        </div>
        <div class="ppmp-signature-role">Signature over Printed Name</div>
        <div class="ppmp-signature-name"></div>
        <div class="ppmp-signature-role">Designation</div>
        <div class="ppmp-signature-role">
          [{{
            currentDepartment?.departmentName ?? form.department ?? 'End-User or Implementing Unit'
          }}]
        </div>
        <div class="ppmp-signature-date">Date : _________________</div>
      </div>

      <div class="ppmp-signature-col">
        <div class="ppmp-signature-label">Submitted by:</div>
        <div class="ppmp-signature-line"></div>
        <div class="ppmp-signature-name"></div>
        <div class="ppmp-signature-role">Signature over Printed Name</div>
        <div class="ppmp-signature-name"></div>
        <div class="ppmp-signature-role">Designation</div>
        <div class="ppmp-signature-role">[Head of the End-User or Implementing Unit]</div>
        <div class="ppmp-signature-date">Date : ____________</div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.ppmp-document {
  border: 1px solid #94a3b8;
  overflow-x: auto;
}

.ppmp-header {
  position: relative;
  padding: 16px 24px 20px;
  border-bottom: 1px solid #94a3b8;
  text-align: center;
  font-family: Arial, Helvetica, sans-serif;
}

.ppmp-header__meta-right {
  position: absolute;
  top: 12px;
  right: 16px;
  width: 150px;
  text-align: left;
  font-size: 9px;
  line-height: 1.35;
  color: #334155;
}

.ppmp-header__meta-value {
  font-weight: 600;
  color: #0f172a;
}

.ppmp-header__meta-spacer {
  height: 8px;
}

.ppmp-header__republic {
  font-size: 11px;
  letter-spacing: 0.04em;
}

.ppmp-header__agency {
  margin-top: 2px;
  font-size: 15px;
  font-weight: 700;
  letter-spacing: 0.02em;
  color: #0f172a;
}

.ppmp-header__title {
  margin-top: 10px;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.02em;
  color: #0f172a;
}

.ppmp-header__placeholder {
  font-weight: 400;
}

.ppmp-header__info-grid {
  margin-top: 14px;
  text-align: left;
  font-size: 11px;
}

.ppmp-header__info-row {
  display: flex;
  gap: 6px;
  padding: 1px 0;
}

.ppmp-header__info-label {
  min-width: 200px;
  font-weight: 700;
  color: #0f172a;
}

.ppmp-header__info-value {
  border-bottom: 1px solid #94a3b8;
  min-width: 260px;
  padding: 0 6px;
  color: #0f172a;
}

.ppmp-table {
  width: 100%;
  table-layout: fixed;
  border-collapse: collapse;
  font-family: Arial, Helvetica, sans-serif;
  font-size: 9.5px;
  color: #0f172a;
}

.ppmp-table th,
.ppmp-table td {
  border: 1px solid #475569;
  padding: 3px 4px;
  vertical-align: middle;
}

.ppmp-head-group th {
  background: #e2e8f0;
  font-size: 8.5px;
  font-weight: 700;
  text-align: center;
  text-transform: uppercase;
  letter-spacing: 0.02em;
  line-height: 1.2;
}

.ppmp-head-col th,
.ppmp-head-cols-num th,
.ppmp-head-sub th {
  background: #f1f5f9;
  font-size: 8px;
  font-weight: 600;
  text-align: center;
  line-height: 1.15;
}

.ppmp-head-cols-num th {
  font-style: italic;
  font-weight: 500;
  color: #475569;
}

.ppmp-head-sub th {
  background: #f8fafc;
}

.ppmp-row td.ppmp-cell {
  vertical-align: top;
  font-size: 9.5px;
}

.ppmp-cell--center {
  text-align: center;
  vertical-align: middle;
}

.ppmp-cell--right {
  text-align: right;
  vertical-align: middle;
  white-space: nowrap;
}

.ppmp-cell--desc {
  padding: 4px 5px;
}

.ppmp-cell--muted {
  color: #94a3b8;
  vertical-align: middle;
}

.ppmp-cell--budget {
  font-weight: 700;
  color: #065f46;
}

.ppmp-item-name {
  font-weight: 700;
  color: #0f172a;
  line-height: 1.2;
}

.ppmp-item-spec {
  margin-top: 2px;
  font-size: 8.5px;
  color: #475569;
  line-height: 1.25;
}

.ppmp-total-row td {
  background: #f1f5f9;
  font-weight: 700;
  font-size: 10px;
  padding: 6px;
}

.ppmp-total-label {
  text-align: right;
  letter-spacing: 0.03em;
}

.ppmp-total-value {
  text-align: right;
  color: #065f46;
  font-size: 11px;
  white-space: nowrap;
}

.ppmp-empty-note {
  padding: 20px;
  text-align: center;
  font-size: 12px;
  color: #64748b;
  border-top: 1px solid #cbd5e1;
}

.ppmp-signatures {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  padding: 20px 24px 28px;
  border-top: 1px solid #94a3b8;
  font-family: Arial, Helvetica, sans-serif;
  font-size: 10px;
  color: #0f172a;
}

.ppmp-signature-col {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.ppmp-signature-label {
  font-weight: 700;
  margin-bottom: 12px;
}

.ppmp-signature-line {
  height: 0;
  border-bottom: 1px solid #475569;
  margin: 4px 0 2px;
}

.ppmp-signature-name {
  font-weight: 700;
  text-align: center;
  min-height: 12px;
}

.ppmp-signature-role {
  font-size: 9px;
  text-align: center;
  color: #334155;
}

.ppmp-signature-date {
  margin-top: 10px;
  font-size: 10px;
}

@media print {
  @page {
    size: A4 landscape;
    margin: 10mm;
  }

  .ppmp-document {
    border: none;
    overflow: visible;
    page-break-inside: auto;
  }

  .ppmp-header {
    page-break-after: avoid;
  }

  .ppmp-table {
    font-size: 8pt;
  }

  .ppmp-table thead {
    display: table-header-group;
  }

  .ppmp-row {
    page-break-inside: avoid;
  }

  .ppmp-signatures {
    page-break-inside: avoid;
    page-break-before: avoid;
  }
}
</style>
