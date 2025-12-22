<script setup>
import { ref, reactive, onMounted } from 'vue'

const baseUrl = 'https://tiffc.onrender.com'

const products = ref([])
const loading = ref(false)
const error = ref('')
const showAddForm = ref(false)
const currentImageIndex = ref({})

const form = reactive({
  title: '',
  priceJpyOriginal: 0,
  priceJpySale: 0,
  priceTwd: 0,
  description: '',
  url: '',
  imageUrls: [''],
  shopName: '',
  category: '',
  notes: '',
  variants: [{ variantName: '', variantValue: '' }]
})

// 計算折扣百分比
function getDiscountPercent(original, sale) {
  if (!original || !sale || original <= sale) return 0
  return Math.round(((original - sale) / original) * 100)
}

// 圖片輪播相關
function setCurrentImage(productId, index) {
  currentImageIndex.value[productId] = index
}

function getCurrentImage(productId) {
  return currentImageIndex.value[productId] || 0
}

function nextImage(product) {
  const current = getCurrentImage(product.id)
  const next = (current + 1) % product.imageUrls.length
  setCurrentImage(product.id, next)
}

function prevImage(product) {
  const current = getCurrentImage(product.id)
  const prev = current === 0 ? product.imageUrls.length - 1 : current - 1
  setCurrentImage(product.id, prev)
}

// 將 variants 按照名稱分組
function groupVariants(variants) {
  const grouped = {}
  variants.forEach(v => {
    if (!grouped[v.variantName]) {
      grouped[v.variantName] = []
    }
    grouped[v.variantName].push(v.variantValue)
  })
  return grouped
}

function addImage() {
  form.imageUrls.push('')
}
function removeImage(index) {
  form.imageUrls.splice(index, 1)
}

function addVariant() {
  form.variants.push({ variantName: '', variantValue: '' })
}
function removeVariant(index) {
  form.variants.splice(index, 1)
}

function toggleAddForm() {
  showAddForm.value = !showAddForm.value
  if (!showAddForm.value) {
    clearForm()
  }
}

async function fetchProducts() {
  loading.value = true
  error.value = ''
  try {
    const res = await fetch(baseUrl + '/product')
    if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
    products.value = await res.json()
  } catch (e) {
    error.value = '讀取商品失敗: ' + e.message
    products.value = []
  } finally {
    loading.value = false
  }
}

function clearForm() {
  form.title = ''
  form.priceJpyOriginal = 0
  form.priceJpySale = 0
  form.priceTwd = 0
  form.description = ''
  form.url = ''
  form.imageUrls = ['']
  form.shopName = ''
  form.category = ''
  form.notes = ''
  form.variants = [{ variantName: '', variantValue: '' }]
}

async function submitProduct() {
  error.value = ''
  const payload = {
    title: form.title,
    priceJpyOriginal: Number(form.priceJpyOriginal) || 0,
    priceJpySale: Number(form.priceJpySale) || 0,
    priceTwd: Number(form.priceTwd) || 0,
    description: form.description,
    url: form.url,
    imageUrls: form.imageUrls.filter(u => u && u.trim() !== ''),
    shopName: form.shopName,
    category: form.category,
    notes: form.notes,
    variants: form.variants
      .filter(v => (v.variantName && v.variantName.trim() !== '') || (v.variantValue && v.variantValue.trim() !== ''))
      .map(v => ({ variantName: v.variantName, variantValue: v.variantValue }))
  }

  try {
    const res = await fetch(baseUrl + '/Product', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
    if (!res.ok) {
      const txt = await res.text()
      throw new Error(`${res.status} ${res.statusText} - ${txt}`)
    }
    clearForm()
    showAddForm.value = false
    await fetchProducts()
  } catch (e) {
    error.value = '新增商品失敗: ' + e.message
  }
}

onMounted(() => {
  fetchProducts()
})
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white border-b border-gray-200 sticky top-0 z-50 shadow-sm">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <h1 class="text-2xl font-bold tracking-tight text-gray-900">TIFFC 代購清單</h1>
          <button @click="toggleAddForm"
            class="px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium text-sm">
            {{ showAddForm ? '取消' : '+ 新增商品' }}
          </button>
        </div>
      </div>
    </header>

    <!-- Add Product Form Modal -->
    <transition name="fade">
      <div v-if="showAddForm"
        class="fixed inset-0 bg-black/50 z-50 flex items-start justify-center overflow-y-auto py-8"
        @click.self="toggleAddForm">
        <div class="bg-white rounded-lg shadow-2xl w-full max-w-2xl mx-4" @click.stop>
          <div class="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 rounded-t-lg">
            <h2 class="text-xl font-bold text-gray-900">新增代購商品</h2>
          </div>

          <div class="px-6 py-6 max-h-[calc(100vh-200px)] overflow-y-auto">
            <div class="space-y-4">
              <!-- 基本資訊 -->
              <div class="grid grid-cols-2 gap-4">
                <label class="flex flex-col col-span-2">
                  <span class="text-sm font-medium text-gray-700 mb-1.5">商品標題 *</span>
                  <input v-model="form.title"
                    class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                    placeholder="請輸入商品標題" />
                </label>

                <label class="flex flex-col">
                  <span class="text-sm font-medium text-gray-700 mb-1.5">原價 (JPY)</span>
                  <input type="number" v-model.number="form.priceJpyOriginal"
                    class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                    placeholder="0" />
                </label>

                <label class="flex flex-col">
                  <span class="text-sm font-medium text-gray-700 mb-1.5">售價 (JPY) *</span>
                  <input type="number" v-model.number="form.priceJpySale"
                    class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                    placeholder="0" />
                </label>

                <label class="flex flex-col">
                  <span class="text-sm font-medium text-gray-700 mb-1.5">售價 (TWD) *</span>
                  <input type="number" v-model.number="form.priceTwd"
                    class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                    placeholder="0" />
                </label>

                <label class="flex flex-col">
                  <span class="text-sm font-medium text-gray-700 mb-1.5">商店名稱</span>
                  <input v-model="form.shopName"
                    class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                    placeholder="ZOZOTOWN" />
                </label>

                <label class="flex flex-col">
                  <span class="text-sm font-medium text-gray-700 mb-1.5">分類</span>
                  <input v-model="form.category"
                    class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                    placeholder="衣服" />
                </label>

                <label class="flex flex-col col-span-2">
                  <span class="text-sm font-medium text-gray-700 mb-1.5">商品網址</span>
                  <input v-model="form.url"
                    class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                    placeholder="https://..." />
                </label>

                <label class="flex flex-col col-span-2">
                  <span class="text-sm font-medium text-gray-700 mb-1.5">備註</span>
                  <input v-model="form.notes"
                    class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                    placeholder="特殊需求或備註" />
                </label>

                <label class="flex flex-col col-span-2">
                  <span class="text-sm font-medium text-gray-700 mb-1.5">商品描述</span>
                  <textarea v-model="form.description" rows="3"
                    class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none resize-none"
                    placeholder="商品描述、特色等..."></textarea>
                </label>
              </div>

              <!-- 圖片網址 -->
              <div class="border-t border-gray-200 pt-4">
                <div class="flex justify-between items-center mb-3">
                  <span class="text-sm font-medium text-gray-700">商品圖片</span>
                  <button type="button" @click="addImage" class="text-sm text-black hover:text-gray-700 font-medium">+
                    新增圖片</button>
                </div>
                <div class="space-y-2">
                  <div v-for="(url, idx) in form.imageUrls" :key="idx" class="flex gap-2">
                    <input v-model="form.imageUrls[idx]" placeholder="圖片網址 https://..."
                      class="flex-1 px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none text-sm" />
                    <button type="button" @click="removeImage(idx)" :disabled="form.imageUrls.length === 1"
                      class="px-3 text-red-600 hover:text-red-800 disabled:opacity-30 disabled:cursor-not-allowed text-sm font-medium">
                      刪除
                    </button>
                  </div>
                </div>
              </div>

              <!-- Variants -->
              <div class="border-t border-gray-200 pt-4">
                <div class="flex justify-between items-center mb-3">
                  <span class="text-sm font-medium text-gray-700">規格選項 (顏色、尺寸等)</span>
                  <button type="button" @click="addVariant" class="text-sm text-black hover:text-gray-700 font-medium">+
                    新增規格</button>
                </div>
                <div class="space-y-2">
                  <div v-for="(v, idx) in form.variants" :key="idx" class="flex gap-2">
                    <input v-model="v.variantName" placeholder="規格名稱 (如：顏色)"
                      class="flex-1 px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none text-sm" />
                    <input v-model="v.variantValue" placeholder="規格值 (如：黑色)"
                      class="flex-1 px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none text-sm" />
                    <button type="button" @click="removeVariant(idx)" :disabled="form.variants.length === 1"
                      class="px-3 text-red-600 hover:text-red-800 disabled:opacity-30 disabled:cursor-not-allowed text-sm font-medium">
                      刪除
                    </button>
                  </div>
                </div>
              </div>

              <!-- Error Message -->
              <div v-if="error" class="bg-red-50 border border-red-200 rounded-sm px-4 py-3">
                <p class="text-sm text-red-800">{{ error }}</p>
              </div>
            </div>
          </div>

          <!-- Form Actions -->
          <div class="sticky bottom-0 bg-gray-50 border-t border-gray-200 px-6 py-4 flex gap-3 rounded-b-lg">
            <button @click="submitProduct"
              class="flex-1 px-6 py-3 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium">
              送出新增
            </button>
            <button @click="toggleAddForm"
              class="px-6 py-3 bg-white border border-gray-300 rounded-sm hover:bg-gray-50 transition-colors font-medium">
              取消
            </button>
          </div>
        </div>
      </div>
    </transition>

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
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <article v-for="product in products" :key="product.id"
          class="bg-white rounded-lg overflow-hidden shadow-sm hover:shadow-lg transition-shadow duration-300 group">
          <!-- Image Gallery -->
          <div class="relative aspect-square bg-gray-100">
            <img v-if="product.imageUrls && product.imageUrls.length > 0"
              :src="product.imageUrls[getCurrentImage(product.id)]" :alt="product.title"
              class="w-full h-full object-cover" />
            <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
              <span class="text-4xl">🖼️</span>
            </div>

            <!-- Discount Badge -->
            <div v-if="getDiscountPercent(product.priceJpyOriginal, product.priceJpySale) > 0"
              class="absolute top-3 left-3 bg-red-600 text-white text-xs font-bold px-2 py-1 rounded">
              -{{ getDiscountPercent(product.priceJpyOriginal, product.priceJpySale) }}%
            </div>

            <!-- Image Navigation -->
            <div v-if="product.imageUrls && product.imageUrls.length > 1"
              class="absolute inset-0 flex items-center justify-between px-2 opacity-0 group-hover:opacity-100 transition-opacity">
              <button @click="prevImage(product)"
                class="bg-white/80 hover:bg-white rounded-full p-2 shadow-lg transition-colors">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <button @click="nextImage(product)"
                class="bg-white/80 hover:bg-white rounded-full p-2 shadow-lg transition-colors">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                </svg>
              </button>
            </div>

            <!-- Image Indicators -->
            <div v-if="product.imageUrls && product.imageUrls.length > 1"
              class="absolute bottom-3 left-0 right-0 flex justify-center gap-1.5">
              <button v-for="(img, idx) in product.imageUrls" :key="idx" @click="setCurrentImage(product.id, idx)"
                :class="['w-1.5 h-1.5 rounded-full transition-all', getCurrentImage(product.id) === idx ? 'bg-white w-4' : 'bg-white/50']">
              </button>
            </div>
          </div>

          <!-- Product Info -->
          <div class="p-4">
            <!-- Category & Shop -->
            <div class="flex items-center gap-2 text-xs text-gray-500 mb-2">
              <span v-if="product.category" class="bg-gray-100 px-2 py-1 rounded">{{ product.category }}</span>
              <span v-if="product.shopName">{{ product.shopName }}</span>
            </div>

            <!-- Title -->
            <h3 class="font-medium text-gray-900 mb-3 line-clamp-2 min-h-[3rem]">
              {{ product.title }}
            </h3>

            <!-- Price -->
            <div class="mb-3">
              <div class="flex items-baseline gap-2">
                <span class="text-2xl font-bold text-gray-900">¥{{ (product.priceJpySale || 0).toLocaleString()
                  }}</span>
                <span v-if="product.priceJpyOriginal && product.priceJpyOriginal > product.priceJpySale"
                  class="text-sm text-gray-400 line-through">¥{{ product.priceJpyOriginal.toLocaleString() }}</span>
              </div>
              <div class="text-sm text-gray-600 mt-1">
                NT${{ (product.priceTwd || 0).toLocaleString() }}
              </div>
            </div>

            <!-- Variants -->
            <div v-if="product.variants && product.variants.length > 0" class="mb-3 space-y-2">
              <div v-for="(values, name) in groupVariants(product.variants)" :key="name" class="text-sm">
                <span class="text-gray-600">{{ name }}:</span>
                <div class="flex flex-wrap gap-1 mt-1">
                  <span v-for="(value, idx) in values" :key="idx"
                    class="inline-block px-2 py-1 bg-gray-100 text-gray-700 rounded text-xs">
                    {{ value }}
                  </span>
                </div>
              </div>
            </div>

            <!-- Description -->
            <p v-if="product.description" class="text-sm text-gray-600 mb-3 line-clamp-2">
              {{ product.description }}
            </p>

            <!-- Actions -->
            <div class="flex gap-2">
              <a v-if="product.url" :href="product.url" target="_blank"
                class="flex-1 text-center px-4 py-2 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors text-sm font-medium">
                查看商品
              </a>
            </div>

            <!-- Notes -->
            <div v-if="product.notes" class="mt-3 pt-3 border-t border-gray-100">
              <p class="text-xs text-gray-500">
                <span class="font-medium">備註:</span> {{ product.notes }}
              </p>
            </div>
          </div>
        </article>
      </div>
    </main>
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
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
