// 訂單狀態定義 - 與後端 StatusEnum 保持一致
export const ORDER_STATUS = {
  無: 0,
  待付款: 1,
  已付款: 2,
  處理中: 3,
  已出貨: 4,
  已完成: 5,
  已取消: 6
}

export const statusOptions = [
  { value: ORDER_STATUS.待付款, label: '待付款', color: 'bg-yellow-100 text-yellow-800' },
  { value: ORDER_STATUS.已付款, label: '已付款', color: 'bg-blue-100 text-blue-800' },
  { value: ORDER_STATUS.處理中, label: '處理中', color: 'bg-purple-100 text-purple-800' },
  { value: ORDER_STATUS.已出貨, label: '已出貨', color: 'bg-green-100 text-green-800' },
  { value: ORDER_STATUS.已完成, label: '已完成', color: 'bg-gray-100 text-gray-800' },
  { value: ORDER_STATUS.已取消, label: '已取消', color: 'bg-red-100 text-red-800' }
]

export function getStatusColor(statusLabel) {
  const status = statusOptions.find(s => s.label === statusLabel)
  return status ? status.color : 'bg-gray-100 text-gray-800'
}

export function getStatusValue(statusLabel) {
  const status = statusOptions.find(s => s.label === statusLabel)
  return status ? status.value : ORDER_STATUS.待付款
}
