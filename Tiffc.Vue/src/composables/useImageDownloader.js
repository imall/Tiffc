import { ref } from 'vue'

export function useImageDownloader() {
  const downloading = ref({})

  async function downloadAllImages(product) {
    if (!product || !product.imageUrls || product.imageUrls.length === 0) return
    try {
      downloading.value[product.id] = true
      for (let i = 0; i < product.imageUrls.length; i++) {
        const url = product.imageUrls[i]
        const res = await fetch(url)
        if (!res.ok) throw new Error(`下載失敗: ${res.status} ${res.statusText}`)
        const blob = await res.blob()
        const extMatch = url.split('.').pop().split(/[#?]/)[0] || 'jpg'
        const safeTitle = (product.title || 'product').replace(/[\\/:*?"<>|]/g, '')
        const filename = `${safeTitle}-${product.id}-${i}.${extMatch}`
        const objUrl = URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = objUrl
        a.download = filename
        document.body.appendChild(a)
        a.click()
        a.remove()
        URL.revokeObjectURL(objUrl)
        // small delay to reduce chance of browser blocking multiple downloads
        await new Promise(res => setTimeout(res, 200))
      }
    } catch (e) {
      alert('下載圖片失敗: ' + e.message)
    } finally {
      downloading.value[product.id] = false
    }
  }

  return { downloading, downloadAllImages }
}
