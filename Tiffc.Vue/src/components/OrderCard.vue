<script setup>
import { ref, computed } from 'vue'
import { useOrderStore } from '../stores/orders'

const orderStore = useOrderStore()

const props = defineProps({
  order: {
    type: Object,
    required: true
  }
})

const emit = defineEmits(['view-detail'])

const updating = ref(false)

const statusOptions = [
  { value: 1, label: '待付款', color: 'bg-yellow-100 text-yellow-800' },
  { value: 2, label: '已付款', color: 'bg-blue-100 text-blue-800' },
  { value: 3, label: '處理中', color: 'bg-purple-100 text-purple-800' },
  { value: 4, label: '已出貨', color: 'bg-green-100 text-green-800' },
  { value: 5, label: '已完成', color: 'bg-gray-100 text-gray-800' },
  { value: 6, label: '已取消', color: 'bg-red-100 text-red-800' }
]

const statusColors = {
  '待付款': 'bg-yellow-100 text-yellow-800',
  '已付款': 'bg-blue-100 text-blue-800',
  '處理中': 'bg-purple-100 text-purple-800',
  '已出貨': 'bg-green-100 text-green-800',
  '已完成': 'bg-gray-100 text-gray-800',
  '已取消': 'bg-red-100 text-red-800'
}

const statusColor = computed(() => {
  return statusColors[props.order.status] || 'bg-gray-100 text-gray-800'
})

const formattedDate = computed(() => {
  return new Date(props.order.createdAt).toLocaleString('zh-TW', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
})

const itemCount = computed(() => {
  return props.order.items?.reduce((sum, item) => sum + item.quantity, 0) || 0
})

async function handleStatusChange(e) {
  const newStatus = parseInt(e.target.value)
  if (updating.value) return

  updating.value = true
  const result = await orderStore.updateOrderStatus(props.order.id, newStatus)
  updating.value = false

  if (!result.success) {
    alert('更新訂單狀態失敗: ' + result.error)
    // 恢復原狀態
    e.target.value = getStatusValue(props.order.status)
  }
}

function getStatusValue(statusLabel) {
  const status = statusOptions.find(s => s.label === statusLabel)
  return status ? status.value : 1
}

const currentStatusValue = computed(() => getStatusValue(props.order.status))
</script>

<template>
  <div class="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-lg transition-shadow">
    <!-- Header -->
    <div class="flex justify-between items-start mb-4">
      <div>
        <div class="text-sm text-gray-500 mb-1">訂單編號</div>
        <div class="font-mono text-lg font-semibold text-gray-900">{{ order.orderNumber }}</div>
      </div>
      <div class="relative">
        <select :value="currentStatusValue" @change="handleStatusChange" :disabled="updating"
          :class="[statusColor, 'px-3 py-1 rounded-full text-xs font-medium cursor-pointer border-0 appearance-none pr-7 disabled:opacity-50 disabled:cursor-not-allowed focus:outline-none focus:ring-2 focus:ring-gray-400']">
          <option v-for="status in statusOptions" :key="status.value" :value="status.value">
            {{ status.label }}
          </option>
        </select>
        <div class="absolute inset-y-0 right-2 flex items-center pointer-events-none">
          <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd"
              d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z"
              clip-rule="evenodd" />
          </svg>
        </div>
      </div>
    </div>

    <!-- Customer Info -->
    <div class="mb-4 space-y-1">
      <div class="flex items-center text-sm">
        <span class="text-gray-500 w-16">顧客</span>
        <span class="text-gray-900 font-medium">{{ order.customerName }}</span>
      </div>
      <div v-if="order.customerPhone" class="flex items-center text-sm">
        <span class="text-gray-500 w-16">電話</span>
        <span class="text-gray-700">{{ order.customerPhone }}</span>
      </div>
      <div v-if="order.customerEmail" class="flex items-center text-sm">
        <span class="text-gray-500 w-16">信箱</span>
        <span class="text-gray-700">{{ order.customerEmail }}</span>
      </div>
    </div>

    <!-- Order Summary -->
    <div class="border-t border-gray-100 pt-4 mb-4">
      <div class="flex justify-between items-center mb-2">
        <span class="text-sm text-gray-600">商品數量</span>
        <span class="text-sm font-medium text-gray-900">{{ itemCount }} 件</span>
      </div>
      <div class="flex justify-between items-center">
        <span class="text-sm text-gray-600">總金額</span>
        <span class="text-lg font-bold text-gray-900">NT$ {{ order.totalAmount.toLocaleString() }}</span>
      </div>
    </div>

    <!-- Footer -->
    <div class="flex justify-between items-center pt-4 border-t border-gray-100">
      <span class="text-xs text-gray-500">{{ formattedDate }}</span>
      <button @click="emit('view-detail', order)"
        class="text-sm text-black hover:text-gray-700 font-medium cursor-pointer">
        查看詳情 →
      </button>
    </div>
  </div>
</template>
