<script setup>
const props = defineProps({
  visible: { type: Boolean, default: false },
  title: { type: String, default: '確認' },
  message: { type: String, required: false },
  confirmText: { type: String, default: '確認' },
  cancelText: { type: String, default: '取消' },
  confirmButtonClass: { type: String, default: 'bg-red-500 hover:bg-red-600' },
  loading: { type: Boolean, default: false }
})

const emit = defineEmits(['confirm', 'cancel'])

function handleConfirm() {
  emit('confirm')
}

function handleCancel() {
  emit('cancel')
}
</script>

<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="visible" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 px-4"
        @click.self="handleCancel">
        <div class="bg-white p-6 rounded-lg shadow-2xl max-w-md w-full" @click.stop>
          <h3 class="text-xl font-semibold text-gray-900 mb-3">{{ title }}</h3>
          <div class="text-gray-600 mb-6">
            <slot>{{ message }}</slot>
          </div>
          <div class="flex gap-3 justify-end">
            <button @click="handleCancel" :disabled="loading"
              class="px-5 py-2.5 bg-gray-200 text-gray-700 rounded hover:bg-gray-300 transition-colors font-medium disabled:opacity-60 disabled:cursor-not-allowed">
              {{ cancelText }}
            </button>
            <button @click="handleConfirm" :disabled="loading"
              :class="['px-5 py-2.5 text-white rounded transition-colors font-medium flex items-center justify-center gap-2 disabled:opacity-60 disabled:cursor-not-allowed', confirmButtonClass]">
              <span v-if="loading"
                class="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              {{ loading ? '處理中...' : confirmText }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
