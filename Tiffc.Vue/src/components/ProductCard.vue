<script setup>
import { ref } from 'vue'
import ImageGallery from './ImageGallery.vue'
import VariantsList from './VariantsList.vue'

const props = defineProps({ product: { type: Object, required: true }, downloading: { type: Boolean, default: false } })
const emit = defineEmits(['download', 'delete', 'edit'])

const isDescriptionExpanded = ref(false)
const isCopied = ref(false)

function toggleDescription() {
  isDescriptionExpanded.value = !isDescriptionExpanded.value
}

function onDownload() {
  emit('download', props.product)
}

function handleDelete() {
  emit('delete', props.product)
}

function handleEdit() {
  emit('edit', props.product)
}

function groupVariants(variants) {
  if (!variants || variants.length === 0) return {}
  const grouped = {}
  variants.forEach(v => {
    if (!grouped[v.variantName]) grouped[v.variantName] = []
    grouped[v.variantName].push(v.variantValue)
  })
  return grouped
}

async function copyToClipboard() {
  const { product } = props

  // 生成文案
  let text = product.title + '\n'

  // 款式
  if (product.variants && product.variants.length > 0) {
    const grouped = groupVariants(product.variants)
    for (const [name, values] of Object.entries(grouped)) {
      text += `${name}: ${values.join('、')}\n`
    }
  }

  // 說明
  if (product.description) {
    text += product.description + '\n'
  }

  // 價格
  text += `$${product.priceTwd || 0}`
  if (product.priceJpyOriginal && product.priceJpyOriginal > 0) {
    text += `  (日幣原價${product.priceJpyOriginal})`
  }

  // 複製到剪貼簿
  try {
    await navigator.clipboard.writeText(text)
    isCopied.value = true
    setTimeout(() => {
      isCopied.value = false
    }, 2000)
  } catch (err) {
    // 降級方案
    const textarea = document.createElement('textarea')
    textarea.value = text
    textarea.style.position = 'fixed'
    textarea.style.opacity = '0'
    document.body.appendChild(textarea)
    textarea.select()
    document.execCommand('copy')
    document.body.removeChild(textarea)
    isCopied.value = true
    setTimeout(() => {
      isCopied.value = false
    }, 2000)
  }
}
</script>

<template>
  <article
    class="bg-white rounded-lg overflow-hidden shadow-sm hover:shadow-lg transition-shadow duration-300 group flex flex-col relative">
    <button @click="handleEdit"
      class="absolute top-2 left-2 z-10 w-7 h-7 bg-blue-500 text-white rounded-full hover:bg-blue-600 transition-colors flex items-center justify-center text-sm cursor-pointer"
      title="編輯商品">
      ✎
    </button>
    <button @click="handleDelete"
      class="absolute top-2 right-2 z-10 w-7 h-7 bg-gray-200 text-gray-500 rounded-full hover:bg-gray-300 hover:text-gray-700 transition-colors flex items-center justify-center text-lg cursor-pointer"
      title="刪除商品">
      ×
    </button>
    <ImageGallery :product="product" />

    <div class="p-4 flex flex-col flex-1">
      <div class="flex items-center gap-2 text-xs text-gray-500 mb-2">
        <span v-if="product.category" class="bg-gray-100 px-2 py-1 rounded">{{ product.category }}</span>
        <span v-if="product.shopName">{{ product.shopName }}</span>
      </div>

      <h3 class="font-medium text-gray-900 mb-3 line-clamp-2 min-h-12">
        <a :href="product.url" target="_blank" class="hover:text-gray-600">
          {{ product.title }}</a>
      </h3>

      <div class="mb-3">
        <div class="flex items-baseline gap-2">
          <span class="text-2xl font-bold text-gray-900">¥{{ (product.priceJpySale || 0).toLocaleString() }}</span>
          <span v-if="product.priceJpyOriginal && product.priceJpyOriginal > product.priceJpySale"
            class="text-sm text-gray-400 line-through">¥{{ product.priceJpyOriginal.toLocaleString() }}</span>
        </div>
        <div class="text-sm text-gray-600 mt-1">NT${{ (product.priceTwd || 0).toLocaleString() }}</div>
      </div>

      <VariantsList :variants="product.variants" />

      <div v-if="product.description" class="mb-3">
        <p @click="toggleDescription"
          :class="['text-sm text-gray-600 whitespace-pre-line cursor-pointer hover:text-gray-800 transition-colors', isDescriptionExpanded ? '' : 'line-clamp-2']"
          :title="isDescriptionExpanded ? '點擊收合' : '點擊展開完整描述'">{{ product.description }}</p>
      </div>
      <div v-if="product.notes" class="my-3 py-3 border-y border-gray-100   mt-auto">
        <p class="text-xs text-gray-500"><span class="font-medium">備註:</span> {{ product.notes }}</p>
      </div>

      <div class="flex gap-2 mt-2" :class="product.notes ? '' : 'mt-auto'">
        <button @click="copyToClipboard"
          class="flex-1 text-center px-4 py-2 bg-blue-50 border border-blue-300 text-blue-700 rounded-sm hover:bg-blue-100 transition-colors text-sm font-medium cursor-pointer">
          {{ isCopied ? '✓ 已複製' : '複製文案' }}
        </button>
        <button @click="onDownload" :disabled="downloading"
          class="flex-1 text-center px-4 py-2 bg-gray-50 text-gray-700 hover:bg-gray-100  border border-gray-300 rounded-sm  transition-colors text-sm font-medium disabled:opacity-60 cursor-pointer">{{
            downloading ? '下載中...' : '下載圖片' }}</button>
      </div>


    </div>
  </article>
</template>
