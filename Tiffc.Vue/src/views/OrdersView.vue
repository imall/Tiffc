<script setup>
import { ref } from 'vue'
import { useOrderStore } from '../stores/orders'
import OrderCard from '../components/OrderCard.vue'
import OrderFormModal from '../components/OrderFormModal.vue'
import OrderDetailModal from '../components/OrderDetailModal.vue'
import OrderEditModal from '../components/OrderEditModal.vue'
import ConfirmDialog from '../components/ConfirmDialog.vue'

const orderStore = useOrderStore()

const showOrderForm = ref(false)
const showOrderDetail = ref(false)
const showOrderEdit = ref(false)
const selectedOrder = ref(null)
const orderToDelete = ref(null)

function toggleOrderForm() {
  showOrderForm.value = !showOrderForm.value
}

function closeOrderForm() {
  showOrderForm.value = false
}

async function handleOrderFormSubmit() {
  await orderStore.refresh()
}

async function handleViewOrderDetail(order) {
  showOrderDetail.value = true
  selectedOrder.value = null

  const fullOrder = await orderStore.fetchOrderByNumber(order.orderNumber)
  if (fullOrder) {
    selectedOrder.value = fullOrder
  } else {
    alert('無法載入訂單詳情')
    closeOrderDetail()
  }
}

function closeOrderDetail() {
  showOrderDetail.value = false
  selectedOrder.value = null
}

function handleOrderDelete(order) {
  orderToDelete.value = order
}

function handleOrderEdit(order) {
  selectedOrder.value = order
  showOrderDetail.value = false
  showOrderEdit.value = true
}

function closeOrderEdit() {
  showOrderEdit.value = false
}

async function handleOrderUpdated(updatedOrder) {
  // 更新 selectedOrder 以便重新打開詳情頁時顯示最新數據
  selectedOrder.value = updatedOrder
  // 關閉編輯視窗並重新打開詳情頁
  showOrderEdit.value = false
  showOrderDetail.value = true
}

function cancelOrderDelete() {
  orderToDelete.value = null
}

async function confirmOrderDelete() {
  if (!orderToDelete.value) return

  const success = await orderStore.deleteOrder(orderToDelete.value.id)
  if (success) {
    if (selectedOrder.value?.id === orderToDelete.value.id) {
      closeOrderDetail()
    }
  } else {
    alert('刪除訂單失敗，請稍後再試')
  }
  orderToDelete.value = null
}
</script>

<template>
  <div>
    <!-- Header Action Button Slot -->
    <teleport to="#header-action">
      <button @click="toggleOrderForm"
        class="px-3 sm:px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium text-sm cursor-pointer whitespace-nowrap flex items-center justify-center gap-1.5">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
        </svg>
        <span class="hidden sm:inline">建立訂單</span>
      </button>
    </teleport>

    <!-- Order Form Modal -->
    <OrderFormModal :visible="showOrderForm" @close="closeOrderForm" @submitted="handleOrderFormSubmit" />

    <!-- Order Detail Modal -->
    <OrderDetailModal :visible="showOrderDetail" :order="selectedOrder" :loading="orderStore.loadingDetail"
      @close="closeOrderDetail" @delete="handleOrderDelete" @edit="handleOrderEdit" />

    <!-- Order Edit Modal -->
    <OrderEditModal :visible="showOrderEdit" :order="selectedOrder" @close="closeOrderEdit"
      @updated="handleOrderUpdated" />

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Loading State -->
      <div v-if="orderStore.loading" class="text-center py-20">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900"></div>
        <p class="mt-4 text-gray-600">讀取中...</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="orderStore.orders.length === 0" class="text-center py-20">
        <div class="text-6xl mb-4">📋</div>
        <h3 class="text-xl font-medium text-gray-900 mb-2">尚無訂單</h3>
        <p class="text-gray-600 mb-6">開始建立您的第一筆訂單</p>
        <button @click="toggleOrderForm"
          class="px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium cursor-pointer">
          + 建立訂單
        </button>
      </div>

      <!-- Orders Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <OrderCard v-for="order in orderStore.orders" :key="order.id" :order="order"
          @view-detail="handleViewOrderDetail" />
      </div>
    </main>

    <!-- Delete Order Confirmation Dialog -->
    <ConfirmDialog :visible="!!orderToDelete" title="確認刪除訂單" confirm-text="確認刪除" cancel-text="取消"
      confirm-button-class="bg-red-500 hover:bg-red-600" :loading="orderStore.isDeleting" @confirm="confirmOrderDelete"
      @cancel="cancelOrderDelete">
      <p class="text-gray-600 mb-2">確定要刪除以下訂單嗎？</p>
      <p class="text-gray-900 font-medium mb-4">訂單編號：<span class="font-mono">{{ orderToDelete?.orderNumber }}</span></p>
      <p class="text-gray-600 mb-2">顧客：{{ orderToDelete?.customerName }}</p>
      <p class="text-sm text-gray-500">此操作無法復原，訂單及其所有明細將被永久刪除。</p>
    </ConfirmDialog>
  </div>
</template>
