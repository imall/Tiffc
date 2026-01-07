<script setup>
const props = defineProps({
  visible: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    required: true
  },
  submitting: {
    type: Boolean,
    default: false
  },
  submitText: {
    type: String,
    default: '送出'
  },
  submitLoadingText: {
    type: String,
    default: '送出中...'
  },
  maxWidth: {
    type: String,
    default: '2xl' // 2xl, 4xl, etc.
  }
})

const emit = defineEmits(['close', 'submit'])

function handleClose() {
  if (props.submitting) return
  emit('close')
}

function handleSubmit() {
  emit('submit')
}
</script>

<template>
  <Transition name="modal">
    <div v-if="visible" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
      <div :class="[
        'bg-white rounded-lg w-full max-h-[90vh] overflow-hidden shadow-xl',
        {
          'max-w-2xl': maxWidth === '2xl',
          'max-w-4xl': maxWidth === '4xl',
          'max-w-6xl': maxWidth === '6xl'
        }
      ]" @click.stop>
        <!-- Header -->
        <div class="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex justify-between items-center">
          <h2 class="text-xl font-bold text-gray-900">{{ title }}</h2>
          <button @click="handleClose" :disabled="submitting"
            class="text-gray-400 hover:text-gray-600 transition-colors p-1 cursor-pointer disabled:opacity-50">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Content -->
        <div class="overflow-y-auto max-h-[calc(90vh-140px)] px-6 py-6">
          <slot></slot>
        </div>

        <!-- Footer -->
        <div class="bottom-0 bg-gray-50 border-t border-gray-200 px-6 py-4 flex justify-end gap-3">
          <button @click="handleClose" :disabled="submitting"
            class="px-6 py-2.5 bg-white border border-gray-300 text-gray-700 rounded-sm hover:bg-gray-50 transition-colors font-medium cursor-pointer disabled:opacity-50">
            取消
          </button>
          <button @click="handleSubmit" :disabled="submitting"
            class="px-6 py-2.5 bg-black text-white rounded-sm hover:bg-gray-800 transition-colors font-medium cursor-pointer disabled:opacity-50 flex items-center gap-2">
            <span v-if="submitting"
              class="inline-block animate-spin rounded-full h-4 w-4 border-b-2 border-white"></span>
            {{ submitting ? submitLoadingText : submitText }}
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .bg-white,
.modal-leave-active .bg-white {
  transition: transform 0.3s ease;
}

.modal-enter-from .bg-white,
.modal-leave-to .bg-white {
  transform: scale(0.95);
}
</style>
