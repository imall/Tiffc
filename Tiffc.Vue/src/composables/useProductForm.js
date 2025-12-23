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
    variants: [{ variantName: '', variantValue: '' }]
  })

  const error = ref('')
  const isSubmitting = ref(false)

  function addImage() {
    form.imageUrls.push('')
  }
  function removeImage(index) {
    form.imageUrls.splice(index, 1)
  }

  function addVariant() {
    form.variants.push({ variantName: '', variantValue: '' })
  }
  function removeVariant(index) {
    form.variants.splice(index, 1)
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
    form.variants = [{ variantName: '', variantValue: '' }]
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
      variants: form.variants
        .filter(v => (v.variantName && v.variantName.trim() !== '') || (v.variantValue && v.variantValue.trim() !== ''))
        .map(v => ({ variantName: v.variantName, variantValue: v.variantValue }))
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

  return { form, error, isSubmitting, addImage, removeImage, addVariant, removeVariant, clearForm, submitProduct }
}
