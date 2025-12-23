import { ref } from 'vue'
const baseUrl = 'https://tiffc.onrender.com'

export function useExchangeRates() {
  const exchangeRates = ref([])
  const loading = ref(false)
  const error = ref(null)

  async function fetchExchangeRates() {
    loading.value = true
    error.value = null
    try {
      const res = await fetch(`${baseUrl}/api/ExchangeRate/latest`)
      if (!res.ok) throw new Error('匯率 API 失敗')
      const data = await res.json()
      exchangeRates.value = data.filter(e => e.currency.includes('日幣'))
    } catch (e) {
      error.value = e
      exchangeRates.value = []
    } finally {
      loading.value = false
    }
  }

  // 計算台幣售價，優先用特價，否則用原價
  function getSitePrices(jpyOriginal, jpySale) {
    const priceJpy = jpySale > 0 ? jpySale : jpyOriginal
    return exchangeRates.value.map(rate => ({
      source: rate.source,
      price: Math.round((priceJpy || 0) * rate.rate)
    }))
  }

  return {
    exchangeRates,
    loading,
    error,
    fetchExchangeRates,
    getSitePrices
  }
}
