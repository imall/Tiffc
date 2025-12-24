import { ref } from 'vue'
import { defineStore } from 'pinia'

const baseUrl = 'https://tiffc.onrender.com'

export const useOrderStore = defineStore('orders', () => {
  // State
  const orders = ref([])
  const loading = ref(false)
  const error = ref('')
  const creating = ref(false)

  // Actions
  async function fetchOrders() {
    loading.value = true
    error.value = ''
    try {
      const res = await fetch(baseUrl + '/api/Order')
      if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
      orders.value = await res.json()
    } catch (e) {
      error.value = '讀取訂單失敗: ' + e.message
      orders.value = []
    } finally {
      loading.value = false
    }
  }

  async function fetchOrderByNumber(orderNumber) {
    loading.value = true
    error.value = ''
    try {
      const res = await fetch(`${baseUrl}/api/Order/${orderNumber}`)
      if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
      return await res.json()
    } catch (e) {
      error.value = '讀取訂單詳情失敗: ' + e.message
      return null
    } finally {
      loading.value = false
    }
  }

  async function createOrder(orderData) {
    creating.value = true
    error.value = ''
    try {
      const res = await fetch(`${baseUrl}/api/Order`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(orderData)
      })
      if (!res.ok) {
        const errorText = await res.text()
        throw new Error(errorText || `${res.status} ${res.statusText}`)
      }
      const newOrder = await res.json()
      // 將新訂單加入列表
      orders.value.unshift(newOrder)
      return { success: true, order: newOrder }
    } catch (e) {
      error.value = '建立訂單失敗: ' + e.message
      return { success: false, error: e.message }
    } finally {
      creating.value = false
    }
  }

  function refresh() {
    return fetchOrders()
  }

  // 初始化時自動載入
  fetchOrders()

  return {
    // State
    orders,
    loading,
    error,
    creating,
    // Actions
    fetchOrders,
    fetchOrderByNumber,
    createOrder,
    refresh
  }
})
