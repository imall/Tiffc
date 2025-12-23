<script setup>
import { watch } from 'vue'
import { useProductForm } from '../composables/useProductForm'

const props = defineProps({
  visible: { type: Boolean, default: false },
  product: { type: Object, default: null }
})
const emit = defineEmits(['close', 'submitted'])

const { form, error, isSubmitting, addImage, removeImage, addVariant, removeVariant, clearForm, updateProduct, loadProduct } = useProductForm()

watch(() => props.visible, (v) => {
  if (v && props.product) {
    loadProduct(props.product)
  } else if (!v) {
    clearForm()
  }
})

async function onSubmit() {
  if (!props.product?.id) return
  const ok = await updateProduct(props.product.id)
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
  <transition name="fade">
    <div v-if="visible" class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4 overflow-y-auto"
      @click.self="onCancel">
      <div class="bg-white rounded-lg shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col my-auto overflow-x-hidden"
        @click.stop>
        <div class="shrink-0 bg-white border-b border-gray-200 px-3 sm:px-6 py-4 rounded-t-lg">
          <h2 class="text-xl font-bold text-gray-900">編輯代購商品</h2>
        </div>

        <div class="flex-1 overflow-y-auto overflow-x-hidden px-3 sm:px-6 py-6">
          <div class="space-y-4">
            <!-- 基本資訊 -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">

              <label class="flex flex-col col-span-2">
                <span class="text-sm font-medium text-gray-700 mb-1.5">商品網址</span>
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
                <span class="text-sm font-medium text-gray-700 mb-1.5">分類</span>
                <input v-model="form.category"
                  class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                  placeholder="衣服" />
              </label>

              <label class="flex flex-col">
                <span class="text-sm font-medium text-gray-700 mb-1.5">商店名稱</span>
                <input v-model="form.shopName"
                  class="px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none"
                  placeholder="ZOZOTOWN" />
              </label>


              <label class="flex flex-col">
                <span class="text-sm font-medium text-gray-700 mb-1.5">日幣原價 </span>
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

              <label class="flex flex-col">
                <span class="text-sm font-bold  text-black-700 mb-1.5">台幣售價 *</span>
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
                <button type="button" @click="addVariant"
                  class="text-sm text-black hover:text-gray-700 font-medium cursor-pointer">+
                  新增規格</button>
              </div>
              <div class="space-y-2">
                <div v-for="(v, idx) in form.variants" :key="idx" class="flex gap-1 sm:gap-2">
                  <input v-model="v.variantName" placeholder="規格名稱 (如：顏色)"
                    class="flex-1 min-w-0 px-2 sm:px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none text-sm" />
                  <input v-model="v.variantValue" placeholder="規格值 (如：黑色)"
                    class="flex-1 min-w-0 px-2 sm:px-3 py-2 border border-gray-300 rounded-sm focus:ring-2 focus:ring-black focus:border-transparent outline-none text-sm" />
                  <button type="button" @click="removeVariant(idx)" :disabled="form.variants.length === 1"
                    class="shrink-0 px-2 sm:px-3 text-red-600 hover:text-red-800 disabled:opacity-30 disabled:cursor-not-allowed text-sm font-medium">刪除</button>
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
        <div class="shrink-0 bg-gray-50 border-t border-gray-200 px-3 sm:px-6 py-4 flex gap-3 rounded-b-lg">
          <button @click="onSubmit" :disabled="isSubmitting"
            class="flex-1 px-6 py-3 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium disabled:opacity-60 disabled:cursor-not-allowed flex items-center justify-center gap-2 cursor-pointer">
            <span v-if="isSubmitting"
              class="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin "></span>
            {{ isSubmitting ? '更新中...' : '更新商品' }}
          </button>
          <button @click="onCancel" :disabled="isSubmitting"
            class="px-6 py-3 bg-white border border-gray-300 rounded-sm hover:bg-gray-50 transition-colors font-medium disabled:opacity-60 disabled:cursor-not-allowed cursor-pointer">取消</button>
        </div>
      </div>
    </div>
  </transition>
</template>
