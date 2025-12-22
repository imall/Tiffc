<script setup>
import { ref, reactive } from 'vue'
import { useProducts } from './composables/useProducts'
import ProductCard from './components/ProductCard.vue'

const baseUrl = 'https://tiffc.onrender.com'

const { products, loading, error, fetchProducts, refresh } = useProducts()
const showAddForm = ref(false)
const currentImageIndex = ref({})
const downloading = ref({})

const form = reactive({
  title: '',
  priceJpyOriginal: 0,
  priceJpySale: 0,
  priceTwd: 0,
  description: '',
  url: '',
  imageUrls: [''],
  shopName: 'ZOZOTOWN',
  category: '衣服',
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

async function downloadAllImages(product) {
  if (!product || !product.imageUrls || product.imageUrls.length === 0) return;
  try {
    downloading.value[product.id] = true
    for (let i = 0; i < product.imageUrls.length; i++) {
      const url = product.imageUrls[i]
      const res = await fetch(url)
      if (!res.ok) throw new Error(`下載失敗: ${res.status} ${res.statusText}`)
      const blob = await res.blob()
      const extMatch = url.split('.').pop().split(/[#?]/)[0] || 'jpg'
      const safeTitle = (product.title || 'product').replace(/[\\/:*?"<>|]/g, '')
      const filename = `${safeTitle}-${product.id}-${i}.${extMatch}`
      const objUrl = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = objUrl
      a.download = filename
      document.body.appendChild(a)
      a.click()
      a.remove()
      URL.revokeObjectURL(objUrl)
      // small delay to reduce chance of browser blocking multiple downloads
      await new Promise(res => setTimeout(res, 200))
    }
  } catch (e) {
    alert('下載圖片失敗: ' + e.message)
  } finally {
    downloading.value[product.id] = false
  }
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



function clearForm() {
  form.title = ''
  form.priceJpyOriginal = 0
  form.priceJpySale = 0
  form.priceTwd = 0
  form.description = ''
  form.url = ''
  form.imageUrls = ['']
  form.shopName = 'ZOZOTOWN'
  form.category = '衣服'
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
    await refresh()
  } catch (e) {
    error.value = '新增商品失敗: ' + e.message
  }
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
            class="px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium text-sm">
            {{ showAddForm ? '取消' : '+ 新增商品' }}
          </button>
        </div>
      </div>
    </header>

    <!-- Add Product Form Modal -->
    <transition name="fade">
      <div v-if="showAddForm"
        class="fixed inset-0 bg-black/50 z-50 flex items-start justify-center py-8"
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
                  <span class="text-sm font-medium text-gray-700 mb-1.5"> 特價 (JPY) *</span>
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
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <ProductCard v-for="product in products" :key="product.id" :product="product"
          :downloading="downloading[product.id]" @download="downloadAllImages" />
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
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
