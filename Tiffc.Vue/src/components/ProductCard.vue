<script setup>
import { ref } from 'vue'
import ImageGallery from './ImageGallery.vue'
import VariantsList from './VariantsList.vue'

const props = defineProps({ product: { type: Object, required: true }, downloading: { type: Boolean, default: false } })
const emit = defineEmits(['download'])

const isDescriptionExpanded = ref(false)

function toggleDescription() {
  isDescriptionExpanded.value = !isDescriptionExpanded.value
}

function onDownload() {
  emit('download', props.product)
}
</script>

<template>
  <article
    class="bg-white rounded-lg overflow-hidden shadow-sm hover:shadow-lg transition-shadow duration-300 group flex flex-col">
    <ImageGallery :product="product" />

    <div class="p-4 flex flex-col flex-1">
      <div class="flex items-center gap-2 text-xs text-gray-500 mb-2">
        <span v-if="product.category" class="bg-gray-100 px-2 py-1 rounded">{{ product.category }}</span>
        <span v-if="product.shopName">{{ product.shopName }}</span>
      </div>

      <h3 class="font-medium text-gray-900 mb-3 line-clamp-2 min-h-12">{{ product.title }}</h3>

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
        <p 
          @click="toggleDescription" 
          :class="['text-sm text-gray-600 whitespace-pre-line cursor-pointer hover:text-gray-800 transition-colors', isDescriptionExpanded ? '' : 'line-clamp-2']"
          :title="isDescriptionExpanded ? '點擊收合' : '點擊展開完整描述'"
        >{{ product.description }}</p>
      </div>
      <div v-if="product.notes" class="my-3 py-3 border-y border-gray-100   mt-auto">
        <p class="text-xs text-gray-500"><span class="font-medium">備註:</span> {{ product.notes }}</p>
      </div>


      <div class="flex gap-2" :class="product.notes ? '' : 'mt-auto'">
        <a v-if="product.url" :href="product.url" target="_blank"
          class="flex-1 text-center px-4 py-2 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors text-sm font-medium">查看商品</a>
        <button @click="onDownload" :disabled="downloading"
          class="flex-1 text-center px-4 py-2 bg-white border border-gray-300 rounded-sm hover:bg-gray-50 transition-colors text-sm font-medium disabled:opacity-60 cursor-pointer">{{
            downloading ? '下載中...' : '下載圖片' }}</button>
      </div>


    </div>
  </article>
</template>
