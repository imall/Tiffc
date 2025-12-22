<script setup>
const props = defineProps({ variants: { type: Array, default: () => [] } })

function groupVariants(variants) {
  const grouped = {}
  variants.forEach(v => {
    if (!grouped[v.variantName]) grouped[v.variantName] = []
    grouped[v.variantName].push(v.variantValue)
  })
  return grouped
}
</script>

<template>
  <div v-if="variants && variants.length > 0" class="mb-3 space-y-2">
    <div v-for="(values, name) in groupVariants(variants)" :key="name" class="text-sm">
      <span class="text-gray-600">{{ name }}:</span>
      <div class="flex flex-wrap gap-1 mt-1">
        <span v-for="(value, idx) in values" :key="idx" class="inline-block px-2 py-1 bg-gray-100 text-gray-700 rounded text-xs">{{ value }}</span>
      </div>
    </div>
  </div>
</template>
