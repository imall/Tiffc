import { createRouter, createWebHistory } from 'vue-router'
import ProductsView from '../views/ProductsView.vue'
import OrdersView from '../views/OrdersView.vue'

const routes = [
  {
    name: 'products',
    path: '/',
    alias: '/products',  
    component: ProductsView
  },
  {
    name: 'orders',
    path: '/orders',
    component: OrdersView
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
