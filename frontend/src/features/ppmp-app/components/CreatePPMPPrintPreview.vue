<script setup lang="ts">
import type { CreatePPMPViewModel } from '../viewmodels/useCreatePPMPViewModel'

type PrintPreviewViewModel = Pick<
  CreatePPMPViewModel,
  | 'printCategories'
  | 'printCategoryTotal'
  | 'currentDepartment'
  | 'submissionType'
  | 'form'
  | 'formatCurrency'
  | 'getDescription'
  | 'getItemsForPrintCategory'
  | 'getProjectType'
  | 'currentUserName'
  | 'currentUserPosition'
>

const props = defineProps<{ viewModel: PrintPreviewViewModel }>()
const {
  printCategories,
  printCategoryTotal,
  currentDepartment,
  submissionType,
  form,
  formatCurrency,
  getDescription,
  getItemsForPrintCategory,
  getProjectType,
  currentUserName,
  currentUserPosition,
} = props.viewModel

function formatMonth(month: string): string {
  const match = /^(\d{4})-(\d{2})$/.exec(month)
  return match ? `${match[2]}/${match[1]}` : month || ''
}
</script>

<template>
  <!-- PRINT DOCUMENTS (Matches HOP-QR-PRO-001 Excel Template) -->
  <section
    v-for="category in printCategories"
    :key="`doc-${category.categoryId}`"
    class="ppmp-document hidden bg-white print:block"
  >
    <!-- TOP HEADER + DOCUMENT CONTROL BOX -->
    <div class="ppmp-header">
      <div class="ppmp-header__center">
        <div class="ppmp-header__republic">Republic of the Philippines</div>
        <div class="ppmp-header__dept">Department of Health</div>
        <div class="ppmp-header__agency">DAVAO REGIONAL MEDICAL CENTER</div>
        <div class="ppmp-header__title">
          PROJECT PROCUREMENT MANAGEMENT PLAN (PPMP) NO.
          <span class="ppmp-header__underline">______</span>
        </div>
        <div class="mt-1 text-xs font-bold uppercase tracking-wide">
          {{ category.categoryName }}
        </div>

        <div class="ppmp-header__status-row">
          <span class="ppmp-status-option">
            <span class="ppmp-checkbox">✓</span> INDICATIVE
          </span>
          <span class="ppmp-status-option">
            <span class="ppmp-checkbox">&nbsp;</span> FINAL
          </span>
        </div>
      </div>

      <!-- Top-Right Reference Box (Matches N1:N9 in Excel) -->
      <table class="ppmp-doc-control">
        <tbody>
          <tr>
            <td>
              <div class="ppmp-doc-control__label">Reference Code:</div>
              <div class="ppmp-doc-control__value">HOP-QR-PRO-001</div>
            </td>
          </tr>
          <tr>
            <td>
              <div class="ppmp-doc-control__label">Revision No.:</div>
              <div class="ppmp-doc-control__value">0</div>
            </td>
          </tr>
          <tr>
            <td>
              <div class="ppmp-doc-control__label">Effective Date:</div>
              <div class="ppmp-doc-control__value">January 1, 2026</div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- FISCAL YEAR & END-USER UNIT (Rows 14-15 in Excel) -->
    <div class="ppmp-meta-lines">
      <div class="ppmp-meta-row">
        <span class="ppmp-meta-label">Fiscal Year :</span>
        <span class="ppmp-meta-value">{{ form.planningYear || '' }}</span>
      </div>
      <div class="ppmp-meta-row">
        <span class="ppmp-meta-label">PPMP Submission Type:</span>
        <span class="ppmp-meta-value">{{ submissionType }}</span>
      </div>
      <div class="ppmp-meta-row">
        <span class="ppmp-meta-label">End-User or Implementing Unit:</span>
        <span class="ppmp-meta-value ppmp-meta-value--wide">
          {{ currentDepartment?.departmentName ?? form.department ?? '' }}
        </span>
      </div>
    </div>

    <div
      v-if="
        getItemsForPrintCategory(category.categoryId).filter((item) => Number(item.quantity) > 0)
          .length === 0
      "
      class="ppmp-empty-note"
    >
      No items with quantities in this category ({{ category.categoryName }}).
    </div>

    <!-- MAIN 14-COLUMN TABLE (Matches Rows 18-21 in Excel) -->
    <div v-else class="ppmp-table-wrap">
      <table class="ppmp-table">
        <colgroup>
          <col style="width: 19%" /><!-- Col A: General Description -->
          <col style="width: 7%" /><!-- Col B: Type -->
          <col style="width: 5%" /><!-- Col C: Quantity -->
          <col style="width: 5%" /><!-- Col D: Unit of Issue -->
          <col style="width: 7%" /><!-- Col E: ABC/unit -->
          <col style="width: 8%" /><!-- Col F: Mode -->
          <col style="width: 5%" /><!-- Col G: Pre-Proc -->
          <col style="width: 6%" /><!-- Col H: Start -->
          <col style="width: 6%" /><!-- Col I: End -->
          <col style="width: 7%" /><!-- Col J: Delivery -->
          <col style="width: 5%" /><!-- Col K: Source of Funds -->
          <col style="width: 8%" /><!-- Col L: Estimated Budget -->
          <col style="width: 6%" /><!-- Col M: Supporting Docs -->
          <col style="width: 6%" /><!-- Col N: Remarks -->
        </colgroup>

        <thead>
          <!-- Row 18: Group Headers (A18:G18, H18:J18, K18:L18, M18:M19, N18:N19) -->
          <tr class="ppmp-head-group">
            <th colspan="7">PROCUREMENT PROJECT DETAILS</th>
            <th colspan="3">PROJECTED TIMELINE (MM/YYYY)</th>
            <th colspan="2">FUNDING DETAILS</th>
            <th rowspan="2">ATTACHED SUPPORTING DOCUMENTS</th>
            <th rowspan="2">REMARKS</th>
          </tr>

          <!-- Row 19: Column Titles -->
          <tr class="ppmp-head-col">
            <th>General Description and Objective of the Project to be Procured</th>
            <th>Type of the Project to be Procured<br />(Goods, Infrastructure, Consulting)</th>
            <th colspan="3">Quantity and Size of the Project to be Procured</th>
            <th>Recommended Mode of Procurement</th>
            <th>Pre-Procurement Conference, if applicable (Yes/No)</th>
            <th>Start of Procurement Activity</th>
            <th>End of Procurement Activity</th>
            <th>Expected Delivery / Implementation Period</th>
            <th>Source of Funds</th>
            <th>Estimated Budget / Authorized Budgetary Allocation (PhP)</th>
          </tr>

          <!-- Row 20: Column 1..12 Numbers -->
          <tr class="ppmp-head-cols-num">
            <th rowspan="2">Column 1</th>
            <th rowspan="2">Column 2</th>
            <th colspan="3">Column 3</th>
            <th rowspan="2">Column 4</th>
            <th rowspan="2">Column 5</th>
            <th rowspan="2">Column 6</th>
            <th rowspan="2">Column 7</th>
            <th rowspan="2">Column 8</th>
            <th rowspan="2">Column 9</th>
            <th rowspan="2">Column 10</th>
            <th rowspan="2">Column 11</th>
            <th rowspan="2">Column 12</th>
          </tr>

          <!-- Row 21: Sub-headers under Column 3 (C21:E21) -->
          <tr class="ppmp-head-sub">
            <th>Quantity</th>
            <th>Unit of Issue</th>
            <th>ABC/unit</th>
          </tr>
        </thead>

        <tbody>
          <tr
            v-for="item in getItemsForPrintCategory(category.categoryId).filter(
              (entry) => Number(entry.quantity) > 0,
            )"
            :key="item.lineId"
            class="ppmp-row"
          >
            <td class="ppmp-cell ppmp-cell--desc">
              <div class="ppmp-item-name">
                {{ getDescription(item) }}
              </div>
              <div v-if="item.sectionAllocations?.length" class="ppmp-item-spec">
                Sub-Unit allocations:
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
              {{ getProjectType(item) || 'Goods' }}
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

            <td class="ppmp-cell ppmp-cell--center">
              {{ item.recommendedModeOfProcurement || '' }}
            </td>

            <td class="ppmp-cell ppmp-cell--center">
              {{ item.preProcurementConference === true ? 'YES' : 'NO' }}
            </td>

            <td class="ppmp-cell ppmp-cell--center">
              {{ formatMonth(item.startOfProcurementActivity) }}
            </td>

            <td class="ppmp-cell ppmp-cell--center">
              {{ formatMonth(item.endOfProcurementActivity) }}
            </td>

            <td class="ppmp-cell ppmp-cell--center">
              {{ item.expectedDeliveryImplementationPeriod || '' }}
            </td>

            <td class="ppmp-cell ppmp-cell--center">
              {{ item.sourceOfFunds || 'GAA' }}
            </td>

            <td class="ppmp-cell ppmp-cell--right ppmp-cell--budget">
              {{ formatCurrency(item.quantity * item.unitPrice) }}
            </td>

            <td class="ppmp-cell ppmp-cell--center">
              {{ item.attachedSupportingDocuments || '' }}
            </td>

            <!-- Column 12 (Remarks): Category Name in Excel template -->
            <td class="ppmp-cell ppmp-cell--center">
              {{ category.categoryName }}
            </td>
          </tr>

          <!-- Blank Spacer Row + TOTAL BUDGET Row (Row 26 in Excel) -->
          <tr class="ppmp-spacer-row">
            <td colspan="14">&nbsp;</td>
          </tr>
          <tr class="ppmp-total-row">
            <td colspan="11" class="ppmp-total-label">TOTAL BUDGET:</td>
            <td class="ppmp-total-value">
              {{ formatCurrency(printCategoryTotal(category.categoryId)) }}
            </td>
            <td colspan="2"></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- SIGNATURES (Matches Rows 29-36 in Excel) -->
    <div class="ppmp-signatures">
      <div class="ppmp-signature-col">
        <div class="ppmp-signature-label">Prepared by:</div>
        <div class="ppmp-signature-space"></div>
        <div class="ppmp-signature-name">{{ currentUserName }}</div>
        <div class="ppmp-signature-line"></div>
        <div class="ppmp-signature-role">Signature over Printed Name</div>
        <div class="ppmp-signature-line ppmp-signature-line--spaced"></div>
        <div class="ppmp-signature-role">Designation</div>
        <div class="ppmp-signature-role">{{ currentUserPosition }}</div>
        <div class="ppmp-signature-role">
          [{{
            currentDepartment?.departmentName ?? form.department ?? 'End-User or Implementing Unit'
          }}]
        </div>
        <div class="ppmp-signature-date">Date : _________________</div>
      </div>

      <div class="ppmp-signature-col">
        <div class="ppmp-signature-label">Submitted by:</div>
        <div class="ppmp-signature-space"></div>
        <div class="ppmp-signature-name">&nbsp;</div>
        <div class="ppmp-signature-line"></div>
        <div class="ppmp-signature-role">Signature over Printed Name</div>
        <div class="ppmp-signature-line ppmp-signature-line--spaced"></div>
        <div class="ppmp-signature-role">Designation</div>
        <div class="ppmp-signature-role">[Head of the End-User or Implementing Unit]</div>
        <div class="ppmp-signature-date">Date : ____________</div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.ppmp-document {
  font-family: Arial, Helvetica, sans-serif;
  color: #000;
  background: #fff;
}

/* HEADER */
.ppmp-header {
  display: grid;
  grid-template-columns: 150px 1fr 150px;
  align-items: start;
  padding-top: 4px;
}

.ppmp-header__center {
  grid-column: 2;
  text-align: center;
  padding-top: 8px;
}

.ppmp-header__republic {
  font-size: 10pt;
  line-height: 1.25;
}

.ppmp-header__dept {
  font-size: 10pt;
  line-height: 1.25;
}

.ppmp-header__agency {
  font-size: 12pt;
  font-weight: 700;
  letter-spacing: 0.02em;
  line-height: 1.3;
}

.ppmp-header__title {
  margin-top: 8px;
  font-size: 10.5pt;
  font-weight: 700;
}

.ppmp-header__underline {
  display: inline-block;
  min-width: 48px;
}

.ppmp-header__status-row {
  margin-top: 10px;
  display: flex;
  justify-content: center;
  gap: 36px;
  font-size: 9.5pt;
  font-weight: 700;
}

.ppmp-status-option {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.ppmp-checkbox {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 13px;
  height: 13px;
  border: 1px solid #000;
  font-size: 9pt;
  line-height: 1;
}

/* TOP-RIGHT DOCUMENT CONTROL BOX (N1:N9) */
.ppmp-doc-control {
  grid-column: 3;
  width: 100%;
  border-collapse: collapse;
  font-size: 8pt;
  text-align: left;
}

.ppmp-doc-control td {
  border: 1px solid #000;
  padding: 3px 6px;
  line-height: 1.25;
}

.ppmp-doc-control__label {
  font-weight: 400;
}

.ppmp-doc-control__value {
  font-weight: 700;
}

/* FISCAL YEAR & END-USER UNIT */
.ppmp-meta-lines {
  margin: 10px 0 8px;
  font-size: 9.5pt;
}

.ppmp-meta-row {
  display: flex;
  align-items: baseline;
  gap: 6px;
  margin-bottom: 3px;
}

.ppmp-meta-label {
  font-weight: 700;
  white-space: nowrap;
}

.ppmp-meta-value {
  display: inline-block;
  min-width: 110px;
  border-bottom: 1px solid #000;
  padding: 0 6px;
  font-weight: 600;
}

.ppmp-meta-value--wide {
  min-width: 280px;
}

/* TABLE */
.ppmp-table {
  width: 100%;
  table-layout: fixed;
  border-collapse: collapse;
  font-size: 8pt;
  color: #000;
}

.ppmp-table th,
.ppmp-table td {
  border: 1px solid #000;
  padding: 3px 4px;
  word-wrap: break-word;
  overflow-wrap: break-word;
}

.ppmp-head-group th {
  background: #d9e1f2;
  font-size: 7.5pt;
  font-weight: 700;
  text-align: center;
  text-transform: uppercase;
  line-height: 1.2;
}

.ppmp-head-col th {
  background: #ededed;
  font-size: 7.5pt;
  font-weight: 700;
  text-align: center;
  line-height: 1.15;
}

.ppmp-head-cols-num th {
  background: #f9fafb;
  font-size: 7pt;
  font-weight: 700;
  text-align: center;
}

.ppmp-head-sub th {
  background: #ededed;
  font-size: 7pt;
  font-weight: 700;
  text-align: center;
}

.ppmp-row td.ppmp-cell {
  vertical-align: middle;
  font-size: 8pt;
  line-height: 1.25;
}

.ppmp-cell--desc {
  vertical-align: top !important;
  text-align: left;
  padding: 4px 5px;
}

.ppmp-cell--center {
  text-align: center;
}

.ppmp-cell--right {
  text-align: right;
  white-space: nowrap;
}

.ppmp-cell--budget {
  font-weight: 700;
}

.ppmp-item-name {
  font-weight: 400;
}

.ppmp-item-spec {
  margin-top: 3px;
  font-size: 7pt;
  color: #334155;
}

.ppmp-spacer-row td {
  height: 10px;
  padding: 0;
}

.ppmp-total-row td {
  font-weight: 700;
  font-size: 8.5pt;
  padding: 4px 6px;
}

.ppmp-total-label {
  text-align: right;
}

.ppmp-total-value {
  text-align: right;
  white-space: nowrap;
}

.ppmp-empty-note {
  padding: 16px;
  text-align: center;
  font-size: 9pt;
  border: 1px solid #000;
}

/* SIGNATURES */
.ppmp-signatures {
  display: grid;
  grid-template-columns: 260px 260px;
  gap: 80px;
  padding: 18px 0 8px 32px;
  font-size: 8.5pt;
}

.ppmp-signature-col {
  display: flex;
  flex-direction: column;
}

.ppmp-signature-label {
  font-weight: 400;
  margin-bottom: 18px;
}

.ppmp-signature-name {
  font-weight: 700;
  text-align: center;
  text-transform: uppercase;
  min-height: 14px;
}

.ppmp-signature-line {
  border-bottom: 1px solid #000;
  margin-top: 2px;
}

.ppmp-signature-line--spaced {
  margin-top: 16px;
}

.ppmp-signature-role {
  font-size: 8pt;
  text-align: center;
  line-height: 1.25;
}

.ppmp-signature-date {
  margin-top: 12px;
  font-size: 8.5pt;
}

@media print {
  @page {
    size: A4 landscape;
    margin: 8mm;
  }

  .ppmp-document {
    page-break-after: always;
    break-after: page;
  }

  .ppmp-document:last-child {
    page-break-after: auto;
    break-after: auto;
  }

  .ppmp-header {
    page-break-after: avoid;
    break-after: avoid;
  }

  .ppmp-table thead {
    display: table-header-group;
  }

  .ppmp-row {
    page-break-inside: avoid;
    break-inside: avoid;
  }

  .ppmp-signatures {
    page-break-inside: avoid;
    break-inside: avoid;
  }
}
</style>