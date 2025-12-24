<script setup>
import { watch, computed } from 'vue'
import { useExchangeRates } from '../composables/useExchangeRates'
import { useProductForm } from '../composables/useProductForm'
import BaseFormModal from './BaseFormModal.vue'

const props = defineProps({
  visible: { type: Boolean, default: false },
  mode: { type: String, default: 'add', validator: (v) => ['add', 'edit'].includes(v) },
  product: { type: Object, default: null }
})
const emit = defineEmits(['close', 'submitted'])

const { form, error, isSubmitting, addImage, removeImage, addVariantGroup, removeVariantGroup, addVariantValue, removeVariantValue, clearForm, submitProduct, updateProduct, loadProduct } = useProductForm()

// 使用 composable 取得匯率
const { fetchExchangeRates, getSitePrices } = useExchangeRates()

import { onMounted } from 'vue'
onMounted(() => {
  fetchExchangeRates()
})

const sitePrices = computed(() => getSitePrices(form.priceJpyOriginal, form.priceJpySale));

const isEditMode = computed(() => props.mode === 'edit')
const modalTitle = computed(() => isEditMode.value ? '編輯代購商品' : '新增代購商品')
const submitButtonText = computed(() => isEditMode.value ? '更新商品' : '送出新增')
const submitButtonLoadingText = computed(() => isEditMode.value ? '更新中...' : '送出中...')

watch(() => props.visible, (v) => {
  if (v && isEditMode.value && props.product) {
    loadProduct(props.product)
  } else if (!v) {
    clearForm()
  }
})

async function onSubmit() {
  let ok = false
  if (isEditMode.value) {
    if (!props.product?.id) return
    ok = await updateProduct(props.product.id)
  } else {
    ok = await submitProduct()
  }

  if (ok) {
    emit('submitted')
    emit('close')
  }
}

function onCancel() {
  clearForm()
  emit('close')
}
</script>

<template>
  <BaseFormModal :visible="visible" :title="modalTitle" :submitting="isSubmitting" :submit-text="submitButtonText"
    :submit-loading-text="submitButtonLoadingText" max-width="2xl" @close="onCancel" @submit="onSubmit">
    <div class="space-y-4">
      <!-- 基本資訊 -->
      <div class="grid grid-cols-2 gap-4">

        <label class="flex flex-col col-span-2">
          <span class="text-sm font-medium text-gray-700 mb-1.5">商品網址 *</span>
          <input v-model="form.url"
            class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
            placeholder="https://..." />
        </label>

        <label class="flex flex-col col-span-2">
          <span class="text-sm font-medium text-gray-700 mb-1.5">商品標題 *</span>
          <input v-model="form.title"
            class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
            placeholder="請輸入商品標題" />
        </label>
        <label class="flex flex-col">
          <span class="text-sm font-medium text-gray-700 mb-1.5">分類 *</span>
          <input v-model="form.category"
            class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
            placeholder="衣服" />
        </label>

        <label class="flex flex-col">
          <span class="text-sm font-medium text-gray-700 mb-1.5">商店名稱 *</span>
          <input v-model="form.shopName"
            class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
            placeholder="ZOZOTOWN" />
        </label>


        <label class="flex flex-col">
          <span class="text-sm font-medium text-gray-700 mb-1.5">日幣原價 * </span>
          <input type="number" v-model.number="form.priceJpyOriginal" @focus="$event.target.select()"
            class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
            placeholder="0" />
        </label>



        <label class="flex flex-col">
          <span class="text-sm font-medium text-red-700 mb-1.5">日幣特價 *</span>
          <input type="number" v-model.number="form.priceJpySale" @focus="$event.target.select()"
            class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
            placeholder="0" />
        </label>

        <div class="flex flex-col justify-center col-span-2">
          <span class="text-sm font-medium text-gray-700 mb-1.5">各代購網站資訊</span>
          <div v-if="sitePrices.length">
            <div v-for="site in sitePrices" :key="site.source" class="text-xs text-gray-600 mb-1">
              <span class="inline-block w-13">{{ site.source }}</span>
              <span class="mr-3">日圓匯率 <span class="font-semibold">{{ site.rate.toFixed(4) }}</span></span>
              <span>代購價 <span class="font-semibold">{{ site.price }}</span></span>
            </div>
          </div>
          <div v-else class="text-xs text-gray-400">無法取得匯率資料</div>
        </div>

        <label class="flex flex-col">
          <span class="text-sm font-bold  text-black-700 mb-1.5">台幣代購定價 *</span>
          <input type="number" v-model.number="form.priceTwd" @focus="$event.target.select()"
            class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
            placeholder="0" />
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
          <button type="button" @click="addImage"
            class="text-sm text-black hover:text-gray-700 font-medium cursor-pointer">+
            新增圖片</button>
        </div>
        <div class="space-y-2">
          <div v-for="(url, idx) in form.imageUrls" :key="idx" class="flex gap-1 sm:gap-2">
            <input v-model="form.imageUrls[idx]" placeholder="圖片網址 https://..."
              class="flex-1 min-w-0 px-2 sm:px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none text-sm" />
            <button type="button" @click="removeImage(idx)" :disabled="form.imageUrls.length === 1"
              class="shrink-0 px-2 sm:px-3 text-red-600 hover:text-red-800 disabled:opacity-30 disabled:cursor-not-allowed text-sm font-medium">刪除</button>
          </div>
        </div>
      </div>

      <!-- Variants -->
      <div class="border-t border-gray-200 pt-4">
        <div class="flex justify-between items-center mb-3">
          <span class="text-sm font-medium text-gray-700">規格選項 (顏色、尺寸等)</span>
          <button type="button" @click="addVariantGroup"
            class="text-sm text-black hover:text-gray-700 font-medium cursor-pointer">+
            新增規格</button>
        </div>
        <div class="space-y-4">
          <div v-for="(group, groupIdx) in form.variantGroups" :key="groupIdx"
            class="border border-gray-200 rounded-sm p-3 bg-gray-50">
            <div class="flex gap-2 items-center mb-2">
              <input v-model="group.name" placeholder="規格名稱 (如：顏色)"
                class="flex-1 px-2 sm:px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none text-sm bg-white" />
              <button type="button" @click="removeVariantGroup(groupIdx)" :disabled="form.variantGroups.length === 1"
                class="shrink-0 px-2 sm:px-3 text-red-600 hover:text-red-800 disabled:opacity-30 disabled:cursor-not-allowed text-sm font-medium">刪除</button>
            </div>
            <div class="space-y-2 pl-2">
              <div v-for="(val, valIdx) in group.values" :key="valIdx" class="flex gap-2 items-center">
                <input v-model="group.values[valIdx]" placeholder="規格值 (如：藍色)"
                  class="flex-1 px-2 sm:px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none text-sm bg-white" />
                <button type="button" @click="removeVariantValue(groupIdx, valIdx)"
                  :disabled="group.values.length === 1"
                  class="shrink-0 px-2 text-red-600 hover:text-red-800 disabled:opacity-30 disabled:cursor-not-allowed text-sm">×</button>
              </div>
              <button type="button" @click="addVariantValue(groupIdx)"
                class="text-xs text-gray-600 hover:text-black font-medium cursor-pointer">+ 新增選項</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Error Message -->
      <div v-if="error" class="bg-red-50 border border-red-200 rounded-sm px-4 py-3">
        <p class="text-sm text-red-800">{{ error }}</p>
      </div>
    </div>
  </BaseFormModal>
</template>