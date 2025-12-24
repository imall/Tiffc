<script setup>
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white border-b border-gray-200 sticky top-0 z-50 shadow-sm">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16 gap-2">
          <!-- Logo/Title -->
          <h1 class="text-xl md:text-2xl font-bold tracking-tight text-gray-900 whitespace-nowrap">
            <span class="hidden sm:inline">TIFFC 代購清單</span>
            <span class="sm:hidden">TIFFC</span>
          </h1>

          <!-- Navigation Tabs -->
          <div class="flex items-center gap-6 sm:gap-6">
            <router-link to="/" class="py-2 font-medium transition-colors cursor-pointer flex items-center gap-1.5"
              :class="$route.path === '/' ? 'text-black border-b-2 border-black' : 'text-gray-500 hover:text-gray-700'"
              :title="'商品管理'">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
              </svg>
              <span class="hidden sm:inline">商品管理</span>
            </router-link>
            <router-link to="/orders"
              class="py-2 font-medium transition-colors cursor-pointer flex items-center gap-1.5"
              :class="$route.path === '/orders' ? 'text-black border-b-2 border-black' : 'text-gray-500 hover:text-gray-700'"
              :title="'訂單管理'">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
              </svg>
              <span class="hidden sm:inline">訂單管理</span>
            </router-link>
          </div>

          <!-- Action Button Placeholder -->
          <div id="header-action"></div>
        </div>
      </div>
    </header>

    <!-- Router View -->
    <router-view />
  </div>
</template>

<style scoped>
/* Router link active state is handled by :class binding */
</style>
