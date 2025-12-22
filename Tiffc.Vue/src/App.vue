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
  <div class="app">
    <h1>商品管理</h1>

    <section class="form-section">
      <h2>新增商品</h2>
      <div class="form-grid">
        <label>標題
          <input v-model="form.title" />
        </label>
        <label>原價 (JPY)
          <input type="number" v-model.number="form.priceJpyOriginal" />
        </label>
        <label>售價 (JPY)
          <input type="number" v-model.number="form.priceJpySale" />
        </label>
        <label>售價 (TWD)
          <input type="number" v-model.number="form.priceTwd" />
        </label>
        <label>商店
          <input v-model="form.shopName" />
        </label>
        <label>分類
          <input v-model="form.category" />
        </label>
        <label>網址
          <input v-model="form.url" />
        </label>
        <label>備註
          <input v-model="form.notes" />
        </label>
        <label class="full">描述
          <textarea v-model="form.description"></textarea>
        </label>

        <div class="full">
          <div class="sub-header">圖片網址</div>
          <div v-for="(url, idx) in form.imageUrls" :key="idx" class="row">
            <input v-model="form.imageUrls[idx]" placeholder="https://..." />
            <button type="button" @click="removeImage(idx)" :disabled="form.imageUrls.length === 1">移除</button>
          </div>
          <button type="button" @click="addImage">新增圖片網址</button>
        </div>

        <div class="full">
          <div class="sub-header">Variants</div>
          <div v-for="(v, idx) in form.variants" :key="idx" class="variant-row">
            <input v-model="v.variantName" placeholder="variantName (e.g. 顏色)" />
            <input v-model="v.variantValue" placeholder="variantValue (e.g. 黑色)" />
            <button type="button" @click="removeVariant(idx)" :disabled="form.variants.length === 1">移除</button>
          </div>
          <button type="button" @click="addVariant">新增 Variant</button>
        </div>

        <div class="full actions">
          <button @click.prevent="submitProduct">送出新增</button>
          <button @click.prevent="clearForm">清除表單</button>
        </div>
      </div>
      <div class="status">
        <div v-if="error" class="error">{{ error }}</div>
      </div>
    </section>

    <section class="list-section">
      <h2>商品列表</h2>
      <div v-if="loading">讀取中...</div>
      <div v-else>
        <div v-if="products.length === 0">沒有商品</div>
        <ul class="product-list">
          <li v-for="p in products" :key="p.id" class="product">
            <div class="product-left">
              <img v-if="p.imageUrls && p.imageUrls[0]" :src="p.imageUrls[0]" alt="img" />
            </div>
            <div class="product-right">
              <h3>{{ p.title }}</h3>
              <div class="meta">店家: {{ p.shopName }} | 分類: {{ p.category }}</div>
              <div class="prices">JPY: {{ p.priceJpySale }} / TWD: {{ p.priceTwd }}</div>
              <p class="desc">{{ p.description }}</p>
              <div class="variants">
                <strong>Variants:</strong>
                <ul>
                  <li v-for="v in p.variants" :key="v.id">{{ v.variantName }}：{{ v.variantValue }}</li>
                </ul>
              </div>
              <a v-if="p.url" :href="p.url" target="_blank">商品連結</a>
            </div>
          </li>
        </ul>
      </div>
    </section>
  </div>
</template>

<style scoped>
:root {
  --gap: 12px
}

.app {
  max-width: 1000px;
  margin: 24px auto;
  padding: 12px;
  font-family: system-ui, Segoe UI, Roboto
}

h1 {
  margin-bottom: 8px
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--gap)
}

label {
  display: flex;
  flex-direction: column
}

label.full {
  grid-column: 1/-1
}

input,
textarea {
  padding: 6px;
  border: 1px solid #ccc;
  border-radius: 4px
}

.row {
  display: flex;
  gap: 8px;
  align-items: center;
  margin-bottom: 6px
}

.variant-row {
  display: flex;
  gap: 8px;
  align-items: center;
  margin-bottom: 6px
}

.sub-header {
  font-weight: 600;
  margin-bottom: 6px
}

.actions {
  display: flex;
  gap: 8px
}

.product-list {
  list-style: none;
  padding: 0;
  margin: 0
}

.product {
  display: flex;
  gap: 12px;
  padding: 12px;
  border-bottom: 1px solid #eee
}

.product-left img {
  width: 120px;
  height: 120px;
  object-fit: cover;
  border-radius: 6px
}

.product-right h3 {
  margin: 0
}

.meta {
  color: #666;
  font-size: 13px
}

.prices {
  margin-top: 6px;
  font-weight: 600
}

.desc {
  margin-top: 8px
}

.error {
  color: #b00020
}
</style>
