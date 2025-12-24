import { reactive, ref } from 'vue'

const baseUrl = 'https://tiffc.onrender.com'

export function useProductForm() {
  const form = reactive({
    title: '',
    priceJpyOriginal: '',
    priceJpySale: '',
    priceTwd: '',
    description: '',
    url: '',
    imageUrls: [''],
    shopName: 'ZOZOTOWN',
    category: '衣服',
    notes: '',
    variantGroups: [{ name: '', values: [''] }]
  })

  const error = ref('')
  const isSubmitting = ref(false)

  function addImage() {
    form.imageUrls.push('')
  }
  function removeImage(index) {
    form.imageUrls.splice(index, 1)
  }

  function addVariantGroup() {
    form.variantGroups.push({ name: '', values: [''] })
  }
  function removeVariantGroup(index) {
    form.variantGroups.splice(index, 1)
  }
  function addVariantValue(groupIndex) {
    form.variantGroups[groupIndex].values.push('')
  }
  function removeVariantValue(groupIndex, valueIndex) {
    form.variantGroups[groupIndex].values.splice(valueIndex, 1)
  }

  function clearForm() {
    form.title = ''
    form.priceJpyOriginal = ''
    form.priceJpySale = ''
    form.priceTwd = ''
    form.description = ''
    form.url = ''
    form.imageUrls = ['']
    form.shopName = 'ZOZOTOWN'
    form.category = '衣服'
    form.notes = ''
    form.variantGroups = [{ name: '', values: [''] }]
    error.value = ''
  }

  function loadProduct(product) {
    if (!product) return

    form.title = product.title || ''
    form.priceJpyOriginal = product.priceJpyOriginal || ''
    form.priceJpySale = product.priceJpySale || ''
    form.priceTwd = product.priceTwd || ''
    form.description = product.description || ''
    form.url = product.url || ''
    form.imageUrls = product.imageUrls && product.imageUrls.length > 0 ? [...product.imageUrls] : ['']
    form.shopName = product.shopName || 'ZOZOTOWN'
    form.category = product.category || '衣服'
    form.notes = product.notes || ''

    // 將舊的 variants 格式轉換為新的 variantGroups 格式
    if (product.variants && product.variants.length > 0) {
      const grouped = {}
      product.variants.forEach(v => {
        if (!grouped[v.variantName]) grouped[v.variantName] = []
        grouped[v.variantName].push(v.variantValue)
      })
      form.variantGroups = Object.entries(grouped).map(([name, values]) => ({ name, values }))
    } else {
      form.variantGroups = [{ name: '', values: [''] }]
    }
    error.value = ''
  }

  async function submitProduct() {
    error.value = ''
    isSubmitting.value = true

    const payload = {
      title: form.title,
      priceJpyOriginal: Number(form.priceJpyOriginal) || 0,
      priceJpySale: Number(form.priceJpySale) || 0,
      priceTwd: Number(form.priceTwd) || 0,
      description: form.description,
      url: form.url,
      imageUrls: form.imageUrls.filter(u => u && u.trim() !== ''),
      shopName: form.shopName,
      category: form.category,
      notes: form.notes,
      variants: form.variantGroups
        .filter(g => g.name && g.name.trim() !== '')
        .flatMap(g => g.values
          .filter(v => v && v.trim() !== '')
          .map(v => ({ variantName: g.name, variantValue: v }))
        )
    }

    try {
      const res = await fetch(baseUrl + '/Product', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      })
      if (!res.ok) {
        const txt = await res.text()
        error.value = `新增商品失敗: ${res.status} ${res.statusText} - ${txt}`
        return false
      }
      clearForm()
      return true
    } catch (e) {
      error.value = '新增商品失敗: ' + e.message
      return false
    } finally {
      isSubmitting.value = false
    }
  }

  async function updateProduct(productId) {
    error.value = ''
    isSubmitting.value = true

    const payload = {
      title: form.title,
      priceJpyOriginal: Number(form.priceJpyOriginal) || 0,
      priceJpySale: Number(form.priceJpySale) || 0,
      priceTwd: Number(form.priceTwd) || 0,
      description: form.description,
      url: form.url,
      imageUrls: form.imageUrls.filter(u => u && u.trim() !== ''),
      shopName: form.shopName,
      category: form.category,
      notes: form.notes,
      variants: form.variantGroups
        .filter(g => g.name && g.name.trim() !== '')
        .flatMap(g => g.values
          .filter(v => v && v.trim() !== '')
          .map(v => ({ variantName: g.name, variantValue: v }))
        )
    }

    try {
      const res = await fetch(`${baseUrl}/Product/${productId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      })
      if (!res.ok) {
        const txt = await res.text()
        error.value = `更新商品失敗: ${res.status} ${res.statusText} - ${txt}`
        return false
      }
      clearForm()
      return true
    } catch (e) {
      error.value = '更新商品失敗: ' + e.message
      return false
    } finally {
      isSubmitting.value = false
    }
  }

  return { form, error, isSubmitting, addImage, removeImage, addVariantGroup, removeVariantGroup, addVariantValue, removeVariantValue, clearForm, submitProduct, updateProduct, loadProduct }
}
