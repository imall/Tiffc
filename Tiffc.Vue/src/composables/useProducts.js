import { ref } from 'vue'

const baseUrl = 'https://tiffc.onrender.com'

export function useProducts() {
  const products = ref([])
  const loading = ref(false)
  const error = ref('')

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

  function refresh() {
    return fetchProducts()
  }

  // 初始載入
  fetchProducts()

  return { products, loading, error, fetchProducts, refresh }
}
