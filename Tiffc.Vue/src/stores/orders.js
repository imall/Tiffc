import { ref } from 'vue'
import { defineStore } from 'pinia'

const baseUrl = 'https://tiffc.onrender.com'

export const useOrderStore = defineStore('orders', () => {
  // State
  const orders = ref([])
  const loading = ref(false)
  const loadingDetail = ref(false)
  const error = ref('')
  const creating = ref(false)
  const isDeleting = ref(false)

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
    loadingDetail.value = true
    error.value = ''
    try {
      const res = await fetch(`${baseUrl}/api/Order/${orderNumber}`)
      if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
      return await res.json()
    } catch (e) {
      error.value = '讀取訂單詳情失敗: ' + e.message
      return null
    } finally {
      loadingDetail.value = false
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

  async function updateOrderStatus(orderId, status) {
    error.value = ''
    try {
      const res = await fetch(`${baseUrl}/api/Order/${orderId}/status/${status}`, {
        method: 'PATCH'
      })
      if (!res.ok) {
        const errorText = await res.text()
        throw new Error(errorText || `${res.status} ${res.statusText}`)
      }
      const updatedOrder = await res.json()
      // 更新本地列表中的訂單
      const index = orders.value.findIndex(o => o.id === orderId)
      if (index !== -1) {
        orders.value[index] = updatedOrder
      }
      return { success: true, order: updatedOrder }
    } catch (e) {
      error.value = '更新訂單狀態失敗: ' + e.message
      return { success: false, error: e.message }
    }
  }

  async function updateOrder(orderId, orderData) {
    error.value = ''
    try {
      const res = await fetch(`${baseUrl}/api/Order/${orderId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(orderData)
      })
      if (!res.ok) {
        const errorText = await res.text()
        throw new Error(errorText || `${res.status} ${res.statusText}`)
      }
      const updatedOrder = await res.json()
      // 更新本地列表中的訂單
      const index = orders.value.findIndex(o => o.id === orderId)
      if (index !== -1) {
        orders.value[index] = updatedOrder
      }
      return { success: true, order: updatedOrder }
    } catch (e) {
      error.value = '更新訂單失敗: ' + e.message
      return { success: false, error: e.message }
    }
  }

  async function deleteOrder(orderId) {
    isDeleting.value = true
    error.value = ''
    try {
      const res = await fetch(`${baseUrl}/api/Order/${orderId}`, {
        method: 'DELETE'
      })
      if (!res.ok) {
        const errorText = await res.text()
        throw new Error(errorText || `${res.status} ${res.statusText}`)
      }
      // 從本地列表中移除已刪除的訂單
      orders.value = orders.value.filter(o => o.id !== orderId)
      return true
    } catch (e) {
      error.value = '刪除訂單失敗: ' + e.message
      return false
    } finally {
      isDeleting.value = false
    }
  }

  // 初始化時自動載入
  fetchOrders()

  return {
    // State
    orders,
    loading,
    loadingDetail,
    error,
    creating,
    isDeleting,
    // Actions
    fetchOrders,
    fetchOrderByNumber,
    createOrder,
    updateOrder,
    updateOrderStatus,
    deleteOrder,
    refresh
  }
})
