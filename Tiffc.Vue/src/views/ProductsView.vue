<script setup>
import { ref } from 'vue'
import { useProductStore } from '../stores/products'
import { useImageDownloader } from '../composables/useImageDownloader'
import ProductFormModal from '../components/ProductFormModal.vue'
import ProductCard from '../components/ProductCard.vue'
import ConfirmDialog from '../components/ConfirmDialog.vue'

const productStore = useProductStore()
const { downloading, downloadAllImages } = useImageDownloader()

const showProductForm = ref(false)
const formMode = ref('add')
const productToEdit = ref(null)
const productToDelete = ref(null)

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
    alert(productStore.error || '刪除商品失敗，請稍後再試')
  }
  productToDelete.value = null
}
</script>

<template>
  <div>
    <!-- Header Action Button Slot -->
    <teleport to="#header-action">
      <button @click="toggleAddForm"
        class="px-3 sm:px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium text-sm cursor-pointer whitespace-nowrap flex items-center justify-center gap-1.5">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
        </svg>
        <span class="hidden sm:inline">新增商品</span>
      </button>
    </teleport>

    <!-- Product Form Modal -->
    <ProductFormModal :visible="showProductForm" :mode="formMode" :product="productToEdit" @close="closeProductForm"
      @submitted="onProductFormSubmitted" />

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
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
    </main>

    <!-- Delete Product Confirmation Dialog -->
    <ConfirmDialog :visible="!!productToDelete" title="確認刪除商品" confirm-text="確認刪除" cancel-text="取消"
      confirm-button-class="bg-red-500 hover:bg-red-600" :loading="productStore.isDeleting" @confirm="confirmDelete"
      @cancel="cancelDelete">
      <p class="text-gray-600 mb-2">確定要刪除以下商品嗎？</p>
      <p class="text-gray-900 font-medium mb-4">「{{ productToDelete?.title }}」</p>
      <p class="text-sm text-gray-500">此操作無法復原，商品及其所有規格資料將被永久刪除。</p>
    </ConfirmDialog>
  </div>
</template>
