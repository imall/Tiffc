import { ref } from 'vue'
import { defineStore } from 'pinia'

const baseUrl = 'https://tiffc.onrender.com'

export const useProductStore = defineStore('products', () => {
  // State
  const products = ref([])
  const loading = ref(false)
  const error = ref('')
  const isDeleting = ref(false)

  // Actions
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

  async function deleteProduct(productId) {
    isDeleting.value = true
    try {
      const res = await fetch(`${baseUrl}/product/${productId}`, {
        method: 'DELETE'
      })

      if (!res.ok) {
        // 嘗試解析錯誤訊息
        let errorMsg = `${res.status} ${res.statusText}`

        const errorData = await res.json()
        errorMsg = errorData.reason || errorData.message || errorMsg

        throw new Error(errorMsg)
      }
      // 從本地列表中移除已刪除的商品
      products.value = products.value.filter(p => p.id !== productId)
      return true
    } catch (e) {
      error.value = '刪除商品失敗: ' + e.message
      return false
    } finally {
      isDeleting.value = false
    }
  }

  function refresh() {
    return fetchProducts()
  }

  // 初始化時自動載入
  fetchProducts()

  return {
    // State
    products,
    loading,
    error,
    isDeleting,
    // Actions
    fetchProducts,
    deleteProduct,
    refresh
  }
})
