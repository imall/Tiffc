<script setup>
import { ref } from 'vue'
import { useProductStore } from './stores/products'
import { useOrderStore } from './stores/orders'
import { useImageDownloader } from './composables/useImageDownloader'
import ProductFormModal from './components/ProductFormModal.vue'
import ProductCard from './components/ProductCard.vue'
import OrderCard from './components/OrderCard.vue'
import OrderFormModal from './components/OrderFormModal.vue'
import OrderDetailModal from './components/OrderDetailModal.vue'
import ConfirmDialog from './components/ConfirmDialog.vue'

// 頁面切換
const currentView = ref('products') // 'products' or 'orders'

// 使用 Pinia stores
const productStore = useProductStore()
const orderStore = useOrderStore()
const { downloading, downloadAllImages } = useImageDownloader()

// 商品相關
const showProductForm = ref(false)
const formMode = ref('add')
const productToEdit = ref(null)
const productToDelete = ref(null)

// 訂單相關
const showOrderForm = ref(false)
const showOrderDetail = ref(false)
const selectedOrder = ref(null)

// 商品功能
function toggleAddForm() {
  formMode.value = 'add'
  productToEdit.value = null
  showProductForm.value = !showProductForm.value
}

function handleDownload(product) {
  downloadAllImages(product)
}

function handleEdit(product) {
  formMode.value = 'edit'
  productToEdit.value = product
  showProductForm.value = true
}

function closeProductForm() {
  showProductForm.value = false
  productToEdit.value = null
}

async function onProductFormSubmitted() {
  await productStore.refresh()
}

function handleDelete(product) {
  productToDelete.value = product
}

function cancelDelete() {
  productToDelete.value = null
}

async function confirmDelete() {
  if (!productToDelete.value) return

  const success = await productStore.deleteProduct(productToDelete.value.id)
  if (!success) {
    alert('刪除商品失敗，請稍後再試')
  }
  productToDelete.value = null
}

// 訂單功能
function toggleOrderForm() {
  showOrderForm.value = !showOrderForm.value
}

function closeOrderForm() {
  showOrderForm.value = false
}

async function handleOrderFormSubmit() {
  // 訂單建立成功後重新載入訂單列表
  await orderStore.refresh()
}

async function handleViewOrderDetail(order) {
  // 獲取完整訂單資料
  const fullOrder = await orderStore.fetchOrderByNumber(order.orderNumber)
  if (fullOrder) {
    selectedOrder.value = fullOrder
    showOrderDetail.value = true
  } else {
    alert('無法載入訂單詳情')
  }
}

function closeOrderDetail() {
  showOrderDetail.value = false
  selectedOrder.value = null
}
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white border-b border-gray-200 sticky top-0 z-50 shadow-sm">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16 gap-2">
          <!-- Logo/Title -->
          <h1 class="text-xl md:text-2xl font-bold tracking-tight text-gray-900 whitespace-nowrap">
            <span class="hidden sm:inline">TIFFC 代購清單</span>
            <span class="sm:hidden">TIFFC</span>
          </h1>

          <!-- Navigation Tabs -->
          <div class="flex items-center gap-6 sm:gap-6">
            <button @click="currentView = 'products'"
              :class="currentView === 'products' ? 'text-black border-b-2 border-black' : 'text-gray-500 hover:text-gray-700'"
              class="py-2 font-medium transition-colors cursor-pointer flex items-center gap-1.5" :title="'商品管理'">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
              </svg>
              <span class="hidden sm:inline">商品管理</span>
            </button>
            <button @click="currentView = 'orders'"
              :class="currentView === 'orders' ? 'text-black border-b-2 border-black' : 'text-gray-500 hover:text-gray-700'"
              class="py-2 font-medium transition-colors cursor-pointer flex items-center gap-1.5" :title="'訂單管理'">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
              </svg>
              <span class="hidden sm:inline">訂單管理</span>
            </button>
          </div>

          <!-- Action Button -->
          <button v-if="currentView === 'products'" @click="toggleAddForm"
            class="px-3 sm:px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium text-sm cursor-pointer whitespace-nowrap flex items-center justify-center gap-1.5">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            <span class="hidden sm:inline">新增商品</span>
          </button>
          <button v-else @click="toggleOrderForm"
            class="px-3 sm:px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium text-sm cursor-pointer whitespace-nowrap flex items-center justify-center gap-1.5">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            <span class="hidden sm:inline">建立訂單</span>
          </button>
        </div>
      </div>
    </header>

    <!-- Product Form Modal -->
    <ProductFormModal :visible="showProductForm" :mode="formMode" :product="productToEdit" @close="closeProductForm"
      @submitted="onProductFormSubmitted" />

    <!-- Order Form Modal -->
    <OrderFormModal :visible="showOrderForm" @close="closeOrderForm" @submitted="handleOrderFormSubmit" />

    <!-- Order Detail Modal -->
    <OrderDetailModal :visible="showOrderDetail" :order="selectedOrder" @close="closeOrderDetail" />

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Products View -->
      <div v-if="currentView === 'products'">
        <!-- Loading State -->
        <div v-if="productStore.loading" class="text-center py-20">
          <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900"></div>
          <p class="mt-4 text-gray-600">讀取中...</p>
        </div>

        <!-- Empty State -->
        <div v-else-if="productStore.products.length === 0" class="text-center py-20">
          <div class="text-6xl mb-4">📦</div>
          <h3 class="text-xl font-medium text-gray-900 mb-2">尚無商品</h3>
          <p class="text-gray-600 mb-6">開始新增您的第一個代購商品</p>
          <button @click="toggleAddForm"
            class="px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium cursor-pointer">
            + 新增商品
          </button>
        </div>

        <!-- Products Grid -->
        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          <ProductCard v-for="product in productStore.products" :key="product.id" :product="product"
            :downloading="downloading[product.id] || false" @download="handleDownload" @edit="handleEdit"
            @delete="handleDelete" />
        </div>
      </div>

      <!-- Orders View -->
      <div v-else-if="currentView === 'orders'">
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
      </div>
    </main>

    <!-- Delete Confirmation Dialog -->
    <ConfirmDialog :visible="!!productToDelete" title="確認刪除商品" confirm-text="確認刪除" cancel-text="取消"
      confirm-button-class="bg-red-500 hover:bg-red-600" :loading="productStore.isDeleting" @confirm="confirmDelete"
      @cancel="cancelDelete">
      <p class="text-gray-600 mb-2">確定要刪除以下商品嗎？</p>
      <p class="text-gray-900 font-medium mb-4">「{{ productToDelete?.title }}」</p>
      <p class="text-sm text-gray-500">此操作無法復原，商品及其所有規格資料將被永久刪除。</p>
    </ConfirmDialog>
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
