<script setup>
import { ref, reactive, computed } from 'vue'
import { useProducts } from '../composables/useProducts'

const props = defineProps({
  visible: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'submitted'])

const { products } = useProducts()

const form = reactive({
  customerName: '',
  customerEmail: '',
  customerPhone: '',
  status: 1, // 待付款
  items: []
})

const currentItem = reactive({
  productId: '',
  quantity: 1,
  unitPrice: 0,
  variants: []
})

const selectedProduct = ref(null)
const submitting = ref(false)
const errors = ref({})

const statusOptions = [
  { value: 1, label: '待付款' },
  { value: 2, label: '已付款' },
  { value: 3, label: '處理中' },
  { value: 4, label: '已出貨' },
  { value: 5, label: '已完成' }
]

const totalAmount = computed(() => {
  return form.items.reduce((sum, item) => {
    return sum + (item.unitPrice * item.quantity)
  }, 0)
})

const canAddItem = computed(() => {
  return currentItem.productId && currentItem.quantity > 0 && currentItem.unitPrice > 0
})

// 整理規格選項：按 variantName 分組，提供可選的 variantValue
const variantOptions = computed(() => {
  if (!selectedProduct.value?.variants) return {}

  const options = {}
  selectedProduct.value.variants.forEach(v => {
    if (!options[v.variantName]) {
      options[v.variantName] = []
    }
    // 避免重複的值
    if (!options[v.variantName].includes(v.variantValue)) {
      options[v.variantName].push(v.variantValue)
    }
  })

  return options
})

function handleProductSelect(e) {
  const productId = e.target.value
  selectedProduct.value = products.value.find(p => p.id === productId)
  currentItem.productId = productId

  // 自動帶入台幣定價
  if (selectedProduct.value?.priceTwd) {
    currentItem.unitPrice = selectedProduct.value.priceTwd
  } else {
    currentItem.unitPrice = 0
  }

  // 清空規格
  currentItem.variants = []

  // 如果商品有規格，準備規格選項（按 variantName 分組）
  if (selectedProduct.value?.variants?.length > 0) {
    const variantNames = [...new Set(selectedProduct.value.variants.map(v => v.variantName))]
    currentItem.variants = variantNames.map(name => ({
      variantName: name,
      variantValue: ''
    }))
  }
}

function addItemToOrder() {
  if (!canAddItem.value) return

  const product = products.value.find(p => p.id === currentItem.productId)

  form.items.push({
    productId: currentItem.productId,
    productTitle: product?.title || '未知商品',
    productImageUrl: product?.images?.[0] || null,
    productUrl: product?.url || null,
    quantity: currentItem.quantity,
    unitPrice: currentItem.unitPrice,
    variants: [...currentItem.variants.filter(v => v.variantValue)]
  })

  // 重置當前項目
  currentItem.productId = ''
  currentItem.quantity = 1
  currentItem.unitPrice = 0
  currentItem.variants = []
  selectedProduct.value = null
}

function removeItem(index) {
  form.items.splice(index, 1)
}

function validateForm() {
  errors.value = {}

  if (!form.customerName.trim()) {
    errors.value.customerName = '請輸入顧客姓名'
  }

  if (form.items.length === 0) {
    errors.value.items = '請至少添加一個商品'
  }

  return Object.keys(errors.value).length === 0
}

async function handleSubmit() {
  if (!validateForm()) return

  submitting.value = true

  try {
    // 準備送出的資料格式
    const orderData = {
      customerName: form.customerName,
      customerEmail: form.customerEmail || null,
      customerPhone: form.customerPhone || null,
      status: form.status,
      items: form.items.map(item => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        variants: item.variants.length > 0 ? item.variants : null
      }))
    }

    emit('submitted', orderData)
  } finally {
    submitting.value = false
  }
}

function handleClose() {
  if (submitting.value) return
  resetForm()
  emit('close')
}

function resetForm() {
  form.customerName = ''
  form.customerEmail = ''
  form.customerPhone = ''
  form.status = 1
  form.items = []
  currentItem.productId = ''
  currentItem.quantity = 1
  currentItem.unitPrice = 0
  currentItem.variants = []
  selectedProduct.value = null
  errors.value = {}
}

function handleBackdropClick(e) {
  if (e.target === e.currentTarget) {
    handleClose()
  }
}
</script>

<template>
  <Transition name="modal">
    <div v-if="visible" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50"
      @click="handleBackdropClick">
      <div class="bg-white rounded-lg max-w-4xl w-full max-h-[90vh] overflow-hidden shadow-xl" @click.stop>
        <!-- Header -->
        <div class="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex justify-between items-center">
          <h2 class="text-xl font-bold text-gray-900">建立訂單</h2>
          <button @click="handleClose" :disabled="submitting"
            class="text-gray-400 hover:text-gray-600 transition-colors p-1 cursor-pointer disabled:opacity-50">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Content -->
        <div class="overflow-y-auto max-h-[calc(90vh-140px)] px-6 py-6">
          <!-- Customer Info -->
          <div class="mb-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-4">顧客資訊</h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">
                  姓名 <span class="text-red-500">*</span>
                </label>
                <input v-model="form.customerName" type="text" placeholder="請輸入顧客姓名"
                  class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-non"
                  :class="{ 'border-red-500': errors.customerName }" />
                <p v-if="errors.customerName" class="mt-1 text-xs text-red-500">{{ errors.customerName }}</p>
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">電話</label>
                <input v-model="form.customerPhone" type="tel" placeholder="請輸入電話"
                  class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none" />
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">信箱</label>
                <input v-model="form.customerEmail" type="email" placeholder="請輸入信箱"
                  class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none " />
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">訂單狀態</label>
                <select v-model="form.status"
                  class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none  cursor-pointer">
                  <option v-for="status in statusOptions" :key="status.value" :value="status.value">
                    {{ status.label }}
                  </option>
                </select>
              </div>
            </div>
          </div>

          <!-- Add Item Section -->
          <div class="mb-6 pb-6 border-b border-gray-200">
            <h3 class="text-sm font-semibold text-gray-900 mb-4">添加商品</h3>

            <div class="space-y-4">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">選擇商品</label>
                  <select v-model="currentItem.productId" @change="handleProductSelect"
                    class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none  cursor-pointer">
                    <option value="">請選擇商品</option>
                    <option v-for="product in products" :key="product.id" :value="product.id">
                      {{ product.title }}
                    </option>
                  </select>
                </div>

                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">數量</label>
                    <input v-model.number="currentItem.quantity" type="number" min="1" placeholder="1"
                      class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none " />
                  </div>

                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">單價</label>
                    <input v-model.number="currentItem.unitPrice" type="number" min="0" placeholder="0"
                      class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none " />
                  </div>
                </div>
              </div>

              <!-- Variants -->
              <div v-if="currentItem.variants.length > 0" class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div v-for="(variant, index) in currentItem.variants" :key="index">
                  <label class="block text-sm font-medium text-gray-700 mb-2">{{ variant.variantName }}</label>
                  <select v-model="variant.variantValue"
                    class="w-full px-3 py-2 border border-gray-300 rounded-sm focus:outline-none focus:ring-black cursor-pointer">
                    <option value="">請選擇{{ variant.variantName }}</option>
                    <option v-for="value in variantOptions[variant.variantName]" :key="value" :value="value">
                      {{ value }}
                    </option>
                  </select>
                </div>
              </div>

              <button @click="addItemToOrder" :disabled="!canAddItem"
                class="w-full py-2.5 bg-gray-100 text-gray-700 rounded-sm hover:bg-gray-200 transition-colors font-medium cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed">
                + 添加至訂單
              </button>
            </div>
          </div>

          <!-- Order Items List -->
          <div class="mb-6">
            <div class="flex justify-between items-center mb-4">
              <h3 class="text-sm font-semibold text-gray-900">訂單明細</h3>
              <span class="text-sm text-gray-600">共 {{ form.items.length }} 項商品</span>
            </div>

            <p v-if="errors.items" class="mb-3 text-sm text-red-500">{{ errors.items }}</p>

            <div v-if="form.items.length === 0" class="text-center py-8 text-gray-500 bg-gray-50 rounded-lg">
              尚未添加商品
            </div>

            <div v-else class="space-y-3">
              <div v-for="(item, index) in form.items" :key="index"
                class="bg-gray-50 rounded-lg p-4 border border-gray-200">
                <div class="flex gap-4">
                  <!-- Product Image -->
                  <div v-if="item.productImageUrl" class="shrink-0">
                    <a v-if="item.productUrl" :href="item.productUrl" target="_blank" rel="noopener noreferrer"
                      class="block w-20 h-20 rounded overflow-hidden bg-white border border-gray-200 hover:border-gray-400 transition-colors">
                      <img :src="item.productImageUrl" :alt="item.productTitle" class="w-full h-full object-cover" />
                    </a>
                    <div v-else class="w-20 h-20 rounded overflow-hidden bg-white border border-gray-200">
                      <img :src="item.productImageUrl" :alt="item.productTitle" class="w-full h-full object-cover" />
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
                      <a v-if="item.productUrl" :href="item.productUrl" target="_blank" rel="noopener noreferrer"
                        class="font-medium text-gray-900 hover:text-blue-600 transition-colors inline-flex items-start gap-1">
                        <span class="line-clamp-2">{{ item.productTitle }}</span>
                        <svg class="w-4 h-4 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                            d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                        </svg>
                      </a>
                      <div v-else class="font-medium text-gray-900 line-clamp-2">{{ item.productTitle }}</div>
                    </div>
                    <div class="text-sm text-gray-600 space-y-1">
                      <div>數量: {{ item.quantity }}</div>
                      <div>單價: NT$ {{ item.unitPrice.toLocaleString() }}</div>
                      <div v-if="item.variants.length > 0" class="flex flex-wrap gap-2 mt-2">
                        <span v-for="(variant, vIndex) in item.variants" :key="vIndex"
                          class="inline-flex items-center px-2 py-1 bg-white border border-gray-300 rounded text-xs">
                          {{ variant.variantName }}: {{ variant.variantValue }}
                        </span>
                      </div>
                    </div>
                  </div>

                  <!-- Pricing and Remove -->
                  <div class="flex flex-col justify-between items-end gap-2 flex-shrink-0 h-full">
                    <button @click="removeItem(index)"
                      class="text-red-500 hover:text-red-700 transition-colors p-1 cursor-pointer">
                      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                          d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                    <div class="text-right">
                      <div class="text-sm text-gray-600">小計</div>
                      <div class="font-semibold text-gray-900">NT$ {{ (item.unitPrice * item.quantity).toLocaleString()
                        }}</div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Items Subtotal -->
              <div class="bg-white rounded-lg p-4 border-2 border-gray-300">
                <div class="flex justify-between items-center">
                  <span class="text-base font-semibold text-gray-900">訂單總額</span>
                  <span class="text-xl font-bold text-gray-900">NT$ {{ form.items.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0).toLocaleString() }}</span>
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
        </div>

        <!-- Footer -->
        <div class="sticky bottom-0 bg-gray-50 border-t border-gray-200 px-6 py-4 flex justify-end gap-3">
          <button @click="handleClose" :disabled="submitting"
            class="px-6 py-2.5 bg-white border border-gray-300 text-gray-700 rounded-sm hover:bg-gray-50 transition-colors font-medium cursor-pointer disabled:opacity-50">
            取消
          </button>
          <button @click="handleSubmit" :disabled="submitting"
            class="px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium cursor-pointer disabled:opacity-50 flex items-center gap-2">
            <span v-if="submitting"
              class="inline-block animate-spin rounded-full h-4 w-4 border-b-2 border-white"></span>
            {{ submitting ? '建立中...' : '建立訂單' }}
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
