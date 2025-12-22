<script setup>
import { ref, reactive, onMounted } from 'vue'

const baseUrl = 'https://tiffc.onrender.com'

const products = ref([])
const loading = ref(false)
const error = ref('')

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
  <div class="max-w-[1000px] mx-auto my-6 px-3">
    <h1 class="mb-2 text-3xl font-bold">商品管理</h1>

    <section class="mb-8">
      <h2 class="text-2xl font-semibold mb-4">新增商品</h2>
      <div class="grid grid-cols-2 gap-3">
        <label class="flex flex-col">
          <span class="mb-1">標題</span>
          <input v-model="form.title" class="px-2 py-1.5 border border-gray-300 rounded" />
        </label>
        <label class="flex flex-col">
          <span class="mb-1">原價 (JPY)</span>
          <input type="number" v-model.number="form.priceJpyOriginal"
            class="px-2 py-1.5 border border-gray-300 rounded" />
        </label>
        <label class="flex flex-col">
          <span class="mb-1">售價 (JPY)</span>
          <input type="number" v-model.number="form.priceJpySale" class="px-2 py-1.5 border border-gray-300 rounded" />
        </label>
        <label class="flex flex-col">
          <span class="mb-1">售價 (TWD)</span>
          <input type="number" v-model.number="form.priceTwd" class="px-2 py-1.5 border border-gray-300 rounded" />
        </label>
        <label class="flex flex-col">
          <span class="mb-1">商店</span>
          <input v-model="form.shopName" class="px-2 py-1.5 border border-gray-300 rounded" />
        </label>
        <label class="flex flex-col">
          <span class="mb-1">分類</span>
          <input v-model="form.category" class="px-2 py-1.5 border border-gray-300 rounded" />
        </label>
        <label class="flex flex-col">
          <span class="mb-1">網址</span>
          <input v-model="form.url" class="px-2 py-1.5 border border-gray-300 rounded" />
        </label>
        <label class="flex flex-col">
          <span class="mb-1">備註</span>
          <input v-model="form.notes" class="px-2 py-1.5 border border-gray-300 rounded" />
        </label>
        <label class="flex flex-col col-span-2">
          <span class="mb-1">描述</span>
          <textarea v-model="form.description" class="px-2 py-1.5 border border-gray-300 rounded min-h-20"></textarea>
        </label>

        <div class="col-span-2">
          <div class="font-semibold mb-1.5">圖片網址</div>
          <div v-for="(url, idx) in form.imageUrls" :key="idx" class="flex gap-2 items-center mb-1.5">
            <input v-model="form.imageUrls[idx]" placeholder="https://..."
              class="flex-1 px-2 py-1.5 border border-gray-300 rounded" />
            <button type="button" @click="removeImage(idx)" :disabled="form.imageUrls.length === 1"
              class="px-3 py-1.5 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed">移除</button>
          </div>
          <button type="button" @click="addImage"
            class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">新增圖片網址</button>
        </div>

        <div class="col-span-2">
          <div class="font-semibold mb-1.5">Variants</div>
          <div v-for="(v, idx) in form.variants" :key="idx" class="flex gap-2 items-center mb-1.5">
            <input v-model="v.variantName" placeholder="variantName (e.g. 顏色)"
              class="flex-1 px-2 py-1.5 border border-gray-300 rounded" />
            <input v-model="v.variantValue" placeholder="variantValue (e.g. 黑色)"
              class="flex-1 px-2 py-1.5 border border-gray-300 rounded" />
            <button type="button" @click="removeVariant(idx)" :disabled="form.variants.length === 1"
              class="px-3 py-1.5 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed">移除</button>
          </div>
          <button type="button" @click="addVariant"
            class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">新增 Variant</button>
        </div>

        <div class="col-span-2 flex gap-2">
          <button @click.prevent="submitProduct"
            class="px-6 py-2 bg-green-600 text-white rounded hover:bg-green-700">送出新增</button>
          <button @click.prevent="clearForm"
            class="px-6 py-2 bg-gray-600 text-white rounded hover:bg-gray-700">清除表單</button>
        </div>
      </div>
      <div class="mt-2">
        <div v-if="error" class="text-red-600 font-medium">{{ error }}</div>
      </div>
    </section>

    <section class="mt-8">
      <h2 class="text-2xl font-semibold mb-4">商品列表</h2>
      <div v-if="loading" class="text-gray-600">讀取中...</div>
      <div v-else>
        <div v-if="products.length === 0" class="text-gray-500">沒有商品</div>
        <ul class="list-none p-0 m-0">
          <li v-for="p in products" :key="p.id" class="flex gap-3 p-3 border-b border-gray-200">
            <div class="shrink-0">
              <img v-if="p.imageUrls && p.imageUrls[0]" :src="p.imageUrls[0]" alt="img"
                class="w-30 h-30 object-cover rounded-md" />
            </div>
            <div class="flex-1">
              <h3 class="text-xl font-semibold m-0">{{ p.title }}</h3>
              <div class="text-gray-600 text-sm mt-1">店家: {{ p.shopName }} | 分類: {{ p.category }}</div>
              <div class="mt-1.5 font-semibold">JPY: {{ p.priceJpySale }} / TWD: {{ p.priceTwd }}</div>
              <p class="mt-2 text-gray-700">{{ p.description }}</p>
              <div class="mt-2">
                <strong>Variants:</strong>
                <ul class="ml-5 mt-1">
                  <li v-for="v in p.variants" :key="v.id">{{ v.variantName }}：{{ v.variantValue }}</li>
                </ul>
              </div>
              <a v-if="p.url" :href="p.url" target="_blank"
                class="inline-block mt-2 text-blue-600 hover:text-blue-800 underline">商品連結</a>
            </div>
          </li>
        </ul>
      </div>
    </section>
  </div>
</template>
