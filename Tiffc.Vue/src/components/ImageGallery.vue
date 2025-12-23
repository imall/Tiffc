<script setup>
import { ref } from 'vue'

const props = defineProps({
  product: { type: Object, required: true }
})

const current = ref(0)

function set(index) {
  current.value = index
}
function prev() {
  if (!props.product.imageUrls || props.product.imageUrls.length === 0) return
  current.value = current.value === 0 ? props.product.imageUrls.length - 1 : current.value - 1
}
function next() {
  if (!props.product.imageUrls || props.product.imageUrls.length === 0) return
  current.value = (current.value + 1) % props.product.imageUrls.length
}
</script>

<template>
  <div class="relative aspect-square bg-gray-100">
    <img v-if="product.imageUrls && product.imageUrls.length > 0" :src="product.imageUrls[current]" :alt="product.title" class="w-full h-full object-cover" />
    <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
      <span class="text-4xl">🖼️</span>
    </div>

    <div v-if="product.imageUrls && product.imageUrls.length > 1" class="absolute inset-0 flex items-center justify-between px-2 opacity-0 group-hover:opacity-100 transition-opacity">
      <button @click.prevent="prev" class="bg-white/80 hover:bg-white rounded-full p-2 shadow-lg transition-colors cursor-pointer">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
      </button>
      <button @click.prevent="next" class="bg-white/80 hover:bg-white rounded-full p-2 shadow-lg transition-colors cursor-pointer">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
        </svg>
      </button>
    </div>

    <div v-if="product.imageUrls && product.imageUrls.length > 1" class="absolute bottom-3 left-0 right-0 flex justify-center gap-1.5">
      <button v-for="(img, idx) in product.imageUrls" :key="idx" @click.prevent="set(idx)"
        :class="['w-1.5 h-1.5 rounded-full transition-all', current === idx ? 'bg-white w-4' : 'bg-white/50']">
      </button>
    </div>
  </div>
</template>
