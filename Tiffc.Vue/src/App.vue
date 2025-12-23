<script setup>
import { ref } from 'vue'
import { useProducts } from './composables/useProducts'
import { useImageDownloader } from './composables/useImageDownloader'
import AddProductModal from './components/AddProductModal.vue'
import EditProductModal from './components/EditProductModal.vue'
import ProductCard from './components/ProductCard.vue'
import ConfirmDialog from './components/ConfirmDialog.vue'

const { products, loading, refresh, deleteProduct, isDeleting } = useProducts()
const { downloading, downloadAllImages } = useImageDownloader()
const showAddForm = ref(false)
const showEditForm = ref(false)
const productToEdit = ref(null)
const productToDelete = ref(null)

function toggleAddForm() {
  showAddForm.value = !showAddForm.value
}

async function onProductSubmitted() {
  await refresh()
}

function handleDownload(product) {
  downloadAllImages(product)
}

function handleEdit(product) {
  productToEdit.value = product
  showEditForm.value = true
}

function closeEditForm() {
  showEditForm.value = false
  productToEdit.value = null
}

async function onProductUpdated() {
  await refresh()
}

function handleDelete(product) {
  productToDelete.value = product
}

function cancelDelete() {
  productToDelete.value = null
}

async function confirmDelete() {
  if (!productToDelete.value) return

  const success = await deleteProduct(productToDelete.value.id)
  if (!success) {
    alert('刪除商品失敗，請稍後再試')
  }
  productToDelete.value = null
}
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white border-b border-gray-200 sticky top-0 z-50 shadow-sm">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <h1 class="text-2xl font-bold tracking-tight text-gray-900">TIFFC 代購清單</h1>
          <button @click="toggleAddForm"
            class="px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium text-sm cursor-pointer">
            {{ showAddForm ? '取消' : '+ 新增商品' }}
          </button>
        </div>
      </div>
    </header>

    <!-- Add Product Modal -->
    <AddProductModal :visible="showAddForm" @close="toggleAddForm" @submitted="onProductSubmitted" />

    <!-- Edit Product Modal -->
    <EditProductModal :visible="showEditForm" :product="productToEdit" @close="closeEditForm"
      @submitted="onProductUpdated" />

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Loading State -->
      <div v-if="loading" class="text-center py-20">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900"></div>
        <p class="mt-4 text-gray-600">讀取中...</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="products.length === 0" class="text-center py-20">
        <div class="text-6xl mb-4">📦</div>
        <h3 class="text-xl font-medium text-gray-900 mb-2">尚無商品</h3>
        <p class="text-gray-600 mb-6">開始新增您的第一個代購商品</p>
        <button @click="toggleAddForm"
          class="px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium">
          + 新增商品
        </button>
      </div>

      <!-- Products Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        <ProductCard v-for="product in products" :key="product.id" :product="product"
          :downloading="downloading[product.id] || false" @download="handleDownload" @edit="handleEdit"
          @delete="handleDelete" />
      </div>
    </main>

    <!-- Delete Confirmation Dialog -->
    <ConfirmDialog :visible="!!productToDelete" title="確認刪除商品" confirm-text="確認刪除" cancel-text="取消"
      confirm-button-class="bg-red-500 hover:bg-red-600" :loading="isDeleting" @confirm="confirmDelete"
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
