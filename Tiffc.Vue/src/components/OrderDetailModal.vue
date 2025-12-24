<script setup>
import { computed } from 'vue'

const props = defineProps({
  visible: {
    type: Boolean,
    default: false
  },
  order: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['close'])

const statusColors = {
  '待付款': 'bg-yellow-100 text-yellow-800',
  '已付款': 'bg-blue-100 text-blue-800',
  '處理中': 'bg-purple-100 text-purple-800',
  '已出貨': 'bg-green-100 text-green-800',
  '已完成': 'bg-gray-100 text-gray-800',
  '已取消': 'bg-red-100 text-red-800'
}

const statusColor = computed(() => {
  if (!props.order) return 'bg-gray-100 text-gray-800'
  return statusColors[props.order.status] || 'bg-gray-100 text-gray-800'
})

const formattedDate = computed(() => {
  if (!props.order) return ''
  return new Date(props.order.createdAt).toLocaleString('zh-TW', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
})

function handleClose() {
  emit('close')
}

function handleBackdropClick(e) {
  if (e.target === e.currentTarget) {
    handleClose()
  }
}
</script>

<template>
  <Transition name="modal">
    <div v-if="visible && order" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50"
      @click="handleBackdropClick">
      <div class="bg-white rounded-lg max-w-3xl w-full max-h-[90vh] overflow-hidden shadow-xl" @click.stop>
        <!-- Header -->
        <div class="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex justify-between items-center">
          <div class="flex items-center gap-4">
            <h2 class="text-xl font-bold text-gray-900">訂單詳情</h2>
            <span :class="statusColor" class="px-3 py-1 rounded-full text-xs font-medium">
              {{ order.status }}
            </span>
          </div>
          <button @click="handleClose" class="text-gray-400 hover:text-gray-600 transition-colors p-1 cursor-pointer">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Content -->
        <div class="overflow-y-auto max-h-[calc(90vh-80px)] px-6 py-6">
          <!-- Order Info -->
          <div class="mb-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-3">訂單資訊</h3>
            <div class="bg-gray-50 rounded-lg p-4 space-y-2">
              <div class="flex justify-between">
                <span class="text-sm text-gray-600">訂單編號</span>
                <span class="text-sm font-mono font-medium text-gray-900">{{ order.orderNumber }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-sm text-gray-600">建立時間</span>
                <span class="text-sm text-gray-900">{{ formattedDate }}</span>
              </div>
            </div>
          </div>

          <!-- Customer Info -->
          <div class="mb-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-3">顧客資訊</h3>
            <div class="bg-gray-50 rounded-lg p-4 space-y-2">
              <div class="flex justify-between">
                <span class="text-sm text-gray-600">姓名</span>
                <span class="text-sm font-medium text-gray-900">{{ order.customerName }}</span>
              </div>
              <div v-if="order.customerPhone" class="flex justify-between">
                <span class="text-sm text-gray-600">電話</span>
                <span class="text-sm text-gray-900">{{ order.customerPhone }}</span>
              </div>
              <div v-if="order.customerEmail" class="flex justify-between">
                <span class="text-sm text-gray-600">信箱</span>
                <span class="text-sm text-gray-900">{{ order.customerEmail }}</span>
              </div>
            </div>
          </div>

          <!-- Order Items -->
          <div class="mb-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-3">訂單明細</h3>
            <div class="space-y-3">
              <div v-for="item in order.items" :key="item.id" class="bg-gray-50 rounded-lg p-4 border border-gray-200">
                <div class="flex justify-between items-start mb-3">
                  <div class="flex-1">
                    <div class="font-medium text-gray-900 mb-1">商品 ID: {{ item.productId }}</div>
                    <div class="text-sm text-gray-600">數量: {{ item.quantity }}</div>
                  </div>
                  <div class="text-right">
                    <div class="text-sm text-gray-600">單價: NT$ {{ item.unitPrice.toLocaleString() }}</div>
                    <div class="font-semibold text-gray-900">小計: NT$ {{ item.subtotal.toLocaleString() }}</div>
                  </div>
                </div>

                <!-- Variants -->
                <div v-if="item.variants && item.variants.length > 0" class="mt-3 pt-3 border-t border-gray-200">
                  <div class="text-xs font-medium text-gray-700 mb-2">規格:</div>
                  <div class="flex flex-wrap gap-2">
                    <span v-for="variant in item.variants" :key="variant.id"
                      class="inline-flex items-center px-2 py-1 bg-white border border-gray-300 rounded text-xs">
                      <span class="text-gray-600">{{ variant.variantName }}:</span>
                      <span class="ml-1 font-medium text-gray-900">{{ variant.variantValue }}</span>
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Total -->
          <div class="border-t border-gray-200 pt-4">
            <div class="flex justify-between items-center">
              <span class="text-lg font-semibold text-gray-900">訂單總額</span>
              <span class="text-2xl font-bold text-gray-900">NT$ {{ order.totalAmount.toLocaleString() }}</span>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="sticky bottom-0 bg-gray-50 border-t border-gray-200 px-6 py-4 flex justify-end">
          <button @click="handleClose"
            class="px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium cursor-pointer">
            關閉
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .bg-white,
.modal-leave-active .bg-white {
  transition: transform 0.3s ease;
}

.modal-enter-from .bg-white,
.modal-leave-to .bg-white {
  transform: scale(0.95);
}
</style>
