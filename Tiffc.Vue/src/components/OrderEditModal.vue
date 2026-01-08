<script setup>
import { ref, watch, computed } from 'vue'
import { useOrderStore } from '../stores/orders'
import { useProductStore } from '../stores/products'
import { statusOptions } from '../constants/orderStatus'
import BaseFormModal from './BaseFormModal.vue'

const orderStore = useOrderStore()
const productStore = useProductStore()

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

const emit = defineEmits(['close', 'updated'])

// Form data
const formData = ref({
  customerName: '',
  customerEmail: '',
  customerPhone: '',
  status: 1,
  items: []
})

const saving = ref(false)
const errors = ref({})
const editingItemIndex = ref(null)
const itemBeingEdited = ref(null)

// Watch for order changes
watch(() => props.order, (newOrder) => {
  if (newOrder) {
    formData.value = {
      customerName: newOrder.customerName || '',
      customerEmail: newOrder.customerEmail || '',
      customerPhone: newOrder.customerPhone || '',
      status: getStatusValue(newOrder.status),
      items: newOrder.items?.map(item => ({
        id: item.id,
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        productInfo: item.productInfo,
        variants: item.variants?.map(v => ({
          variantName: v.variantName,
          variantValue: v.variantValue
        })) || []
      })) || []
    }
  }
}, { immediate: true })

function getStatusValue(statusStr) {
  const statusMap = {
    '待付款': 1,
    '已付款': 2,
    '處理中': 3,
    '已出貨': 4,
    '已完成': 5,
    '已取消': 6
  }
  return statusMap[statusStr] || 1
}

const totalAmount = computed(() => {
  return formData.value.items.reduce((sum, item) => {
    return sum + (item.quantity * item.unitPrice)
  }, 0)
})

// 取得商品的規格選項
function getVariantOptions(productId) {
  const product = productStore.products.find(p => p.id === productId)
  if (!product?.variants) return {}

  const options = {}
  product.variants.forEach(v => {
    if (!options[v.variantName]) {
      options[v.variantName] = []
    }
    if (!options[v.variantName].includes(v.variantValue)) {
      options[v.variantName].push(v.variantValue)
    }
  })

  return options
}

// 取得商品的所有規格名稱
function getVariantNames(productId) {
  const product = productStore.products.find(p => p.id === productId)
  if (!product?.variants) return []
  return [...new Set(product.variants.map(v => v.variantName))]
}

// 開始編輯商品規格
function startEditingVariants(index) {
  const item = formData.value.items[index]
  const variantNames = getVariantNames(item.productId)

  // 建立編輯用的規格資料
  const editingVariants = variantNames.map(name => {
    const existingVariant = item.variants.find(v => v.variantName === name)
    return {
      variantName: name,
      variantValue: existingVariant?.variantValue || ''
    }
  })

  itemBeingEdited.value = {
    ...item,
    variants: editingVariants
  }
  editingItemIndex.value = index
}

// 取消編輯規格
function cancelEditingVariants() {
  editingItemIndex.value = null
  itemBeingEdited.value = null
}

// 儲存編輯的規格
function saveEditedVariants() {
  if (editingItemIndex.value !== null && itemBeingEdited.value) {
    // 只保留有值的規格
    const validVariants = itemBeingEdited.value.variants.filter(v => v.variantValue)

    formData.value.items[editingItemIndex.value] = {
      ...formData.value.items[editingItemIndex.value],
      quantity: itemBeingEdited.value.quantity,
      unitPrice: itemBeingEdited.value.unitPrice,
      variants: validVariants
    }

    cancelEditingVariants()
  }
}

function validateForm() {
  errors.value = {}

  if (!formData.value.customerName?.trim()) {
    errors.value.customerName = '請輸入顧客姓名'
  }

  if (formData.value.items.length === 0) {
    errors.value.items = '至少需要一個訂單項目'
  }

  formData.value.items.forEach((item, index) => {
    if (item.quantity <= 0) {
      errors.value[`quantity_${index}`] = '數量必須大於 0'
    }
    if (item.unitPrice <= 0) {
      errors.value[`unitPrice_${index}`] = '單價必須大於 0'
    }
  })

  return Object.keys(errors.value).length === 0
}

async function handleSubmit() {
  if (!validateForm()) return

  saving.value = true

  const updateData = {
    customerName: formData.value.customerName,
    customerEmail: formData.value.customerEmail || null,
    customerPhone: formData.value.customerPhone || null,
    status: formData.value.status,
    items: formData.value.items.map(item => ({
      productId: item.productId,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
      variants: item.variants
    }))
  }

  const result = await orderStore.updateOrder(props.order.id, updateData)
  saving.value = false

  if (result.success) {
    emit('updated', result.order)
    handleClose()
  } else {
    alert('更新訂單失敗: ' + result.error)
  }
}

function handleClose() {
  if (saving.value) return
  cancelEditingVariants()
  emit('close')
}

function removeItem(index) {
  if (formData.value.items.length === 1) {
    alert('至少需要保留一個訂單項目')
    return
  }
  formData.value.items.splice(index, 1)
}

const modalTitle = '編輯訂單'
const submitText = '儲存變更'
const submitLoadingText = '儲存中...'
</script>

<template>
  <BaseFormModal :visible="visible" :title="modalTitle" :submitting="saving" :submit-text="submitText"
    :submit-loading-text="submitLoadingText" max-width="4xl" @close="handleClose" @submit="handleSubmit">

    <!-- Order Info (Read-only) -->
    <div class="mb-6">
      <h3 class="text-sm font-semibold text-gray-900 mb-3">訂單資訊</h3>
      <div class="bg-gray-50 rounded-lg p-4">
        <div class="text-sm text-gray-600">訂單編號</div>
        <div class="text-sm font-mono font-medium text-gray-900 mt-1">{{ order?.orderNumber }}</div>
      </div>
    </div>

    <!-- Customer Info -->
    <div class="mb-6">
      <h3 class="text-sm font-semibold text-gray-900 mb-4">顧客資訊</h3>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            姓名 <span class="text-red-500">*</span>
          </label>
          <input v-model="formData.customerName" type="text" placeholder="請輸入顧客姓名"
            class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none"
            :class="{ 'border-red-500': errors.customerName }" />
          <p v-if="errors.customerName" class="mt-1 text-xs text-red-500">{{ errors.customerName }}</p>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">電話</label>
          <input v-model="formData.customerPhone" type="tel" placeholder="請輸入電話"
            class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none" />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">信箱</label>
          <input v-model="formData.customerEmail" type="email" placeholder="請輸入信箱"
            class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none" />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">訂單狀態</label>
          <select v-model.number="formData.status"
            class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none cursor-pointer">
            <option v-for="status in statusOptions" :key="status.value" :value="status.value">
              {{ status.label }}
            </option>
          </select>
        </div>
      </div>
    </div>

    <!-- Order Items -->
    <div class="mb-6">
      <div class="flex justify-between items-center mb-4">
        <h3 class="text-sm font-semibold text-gray-900">訂單明細</h3>
        <span class="text-sm text-gray-600">共 {{ formData.items.length }} 項商品</span>
      </div>
      <p v-if="errors.items" class="mb-3 text-sm text-red-500">{{ errors.items }}</p>

      <div class="space-y-4">
        <div v-for="(item, itemIndex) in formData.items" :key="itemIndex"
          class="bg-gray-50 rounded-lg p-4 border border-gray-200">

          <!-- 一般顯示模式 -->
          <div v-if="editingItemIndex !== itemIndex" class="flex gap-4">
            <!-- Product Image -->
            <div v-if="item.productInfo?.imageUrl" class="shrink-0">
              <a v-if="item.productInfo.url" :href="item.productInfo.url" target="_blank" rel="noopener noreferrer"
                class="block w-20 h-20 rounded overflow-hidden bg-white border border-gray-200 hover:border-gray-400 transition-colors">
                <img :src="item.productInfo.imageUrl" :alt="item.productInfo.title"
                  class="w-full h-full object-cover" />
              </a>
              <div v-else class="w-20 h-20 rounded overflow-hidden bg-white border border-gray-200">
                <img :src="item.productInfo.imageUrl" :alt="item.productInfo.title"
                  class="w-full h-full object-cover" />
              </div>
            </div>
            <div v-else class="shrink-0 w-20 h-20 rounded bg-gray-200 flex items-center justify-center">
              <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>

            <!-- Product Info -->
            <div class="flex-1 min-w-0">
              <div class="mb-2">
                <a v-if="item.productInfo?.url" :href="item.productInfo.url" target="_blank" rel="noopener noreferrer"
                  class="font-medium text-gray-900 hover:text-blue-600 transition-colors inline-flex items-start gap-1">
                  <span class="line-clamp-2">{{ item.productInfo.title }}</span>
                  <svg class="w-4 h-4 shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                  </svg>
                </a>
                <div v-else class="font-medium text-gray-900 line-clamp-2">
                  {{ item.productInfo?.title || '商品' }}
                </div>
              </div>
              <div class="text-sm text-gray-600 space-y-1">
                <div>數量: {{ item.quantity }}</div>
                <div>單價: NT$ {{ item.unitPrice.toLocaleString() }}</div>
                <div v-if="item.variants && item.variants.length > 0" class="flex flex-wrap gap-2 mt-2">
                  <span v-for="(variant, vIndex) in item.variants" :key="vIndex"
                    class="inline-flex items-center px-2 py-1 bg-white border border-gray-300 rounded text-xs">
                    {{ variant.variantName }}: {{ variant.variantValue }}
                  </span>
                </div>
              </div>
            </div>

            <!-- Actions and Pricing -->
            <div class="flex flex-col justify-between items-end gap-2 shrink-0">
              <div class="flex gap-2">
                <button @click="startEditingVariants(itemIndex)" type="button"
                  class="text-blue-500 hover:text-blue-700 transition-colors p-1 cursor-pointer" title="編輯">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </button>
                <button @click="removeItem(itemIndex)" type="button" :class="[
                  formData.items.length === 1
                    ? 'text-gray-300 cursor-not-allowed'
                    : 'text-red-500 hover:text-red-700 cursor-pointer'
                ]" class="transition-colors p-1" :disabled="formData.items.length === 1"
                  :title="formData.items.length === 1 ? '至少需保留一個項目' : '刪除'">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                </button>
              </div>
              <div class="text-right">
                <div class="text-sm text-gray-600">小計</div>
                <div class="font-semibold text-gray-900">
                  NT$ {{ (item.quantity * item.unitPrice).toLocaleString() }}
                </div>
              </div>
            </div>
          </div>

          <!-- 編輯模式 -->
          <div v-else class="space-y-4">
            <div class="flex gap-3 items-start pb-3 border-b border-gray-200">
              <!-- Product Image -->
              <div v-if="item.productInfo?.imageUrl"
                class="shrink-0 w-16 h-16 rounded overflow-hidden bg-white border border-gray-200">
                <img :src="item.productInfo.imageUrl" :alt="item.productInfo.title"
                  class="w-full h-full object-cover" />
              </div>
              <div v-else class="shrink-0 w-16 h-16 rounded bg-gray-200 flex items-center justify-center">
                <svg class="w-6 h-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>

              <!-- Product Title -->
              <div class="flex-1 min-w-0">
                <div class="text-sm font-medium text-gray-900 line-clamp-2">
                  {{ item.productInfo?.title || '商品' }}
                </div>
              </div>
            </div>

            <!-- Edit Fields -->
            <div class="grid grid-cols-2 gap-3">
              <!-- Quantity -->
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">數量</label>
                <input v-model.number="itemBeingEdited.quantity" type="number" min="1"
                  class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none text-sm" />
              </div>

              <!-- Unit Price -->
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">單價 (NT$)</label>
                <input v-model.number="itemBeingEdited.unitPrice" type="number" min="0" step="0.01"
                  class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none text-sm" />
              </div>
            </div>

            <!-- Variants -->
            <div v-if="itemBeingEdited.variants.length > 0">
              <label class="block text-xs font-medium text-gray-700 mb-2">商品規格</label>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                <div v-for="(variant, vIndex) in itemBeingEdited.variants" :key="vIndex">
                  <label class="block text-xs text-gray-600 mb-1">{{ variant.variantName }}</label>
                  <select v-model="variant.variantValue"
                    class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none text-sm cursor-pointer">
                    <option value="">請選擇{{ variant.variantName }}</option>
                    <option v-for="value in getVariantOptions(item.productId)[variant.variantName]" :key="value"
                      :value="value">
                      {{ value }}
                    </option>
                  </select>
                </div>
              </div>
            </div>

            <!-- Subtotal -->
            <div class="flex justify-between items-center pt-3 border-t border-gray-200">
              <span class="text-xs text-gray-600">小計</span>
              <span class="text-sm font-semibold text-gray-900">
                NT$ {{ (itemBeingEdited.quantity * itemBeingEdited.unitPrice).toLocaleString() }}
              </span>
            </div>

            <!-- Action Buttons -->
            <div class="flex justify-end gap-2 pt-2">
              <button @click="cancelEditingVariants" type="button"
                class="px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-sm hover:bg-gray-50 transition-colors text-sm font-medium cursor-pointer">
                取消
              </button>
              <button @click="saveEditedVariants" type="button"
                class="px-4 py-2 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors text-sm font-medium cursor-pointer">
                確認
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Total -->
    <div class="bg-gray-100 rounded-lg p-4">
      <div class="flex justify-between items-center">
        <span class="text-lg font-semibold text-gray-900">訂單總額</span>
        <span class="text-2xl font-bold text-gray-900">NT$ {{ totalAmount.toLocaleString() }}</span>
      </div>
    </div>
  </BaseFormModal>
</template>
