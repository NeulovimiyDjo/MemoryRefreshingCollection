def sortAlg1(arr):
  for startPos in range(0, len(arr)):
    minPos = startPos
    for i, _ in enumerate(arr[startPos:], startPos):
      if arr[i] < arr[minPos]:
        minPos = i

    tmp = arr[minPos]
    arr[minPos] = arr[startPos]
    arr[startPos] = tmp


def sortAlg2(arr):
  for _ in range(0, len(arr)):
    for i, _ in enumerate(arr[1:], 1):
      if arr[i] < arr[i - 1]:
        tmp = arr[i]
        arr[i] = arr[i - 1]
        arr[i - 1] = tmp






def swap(arr, left, right):
  tmp = arr[left]
  arr[left] = arr[right]
  arr[right] = tmp


def reorder(arr, start, end, pivot):
  left = start
  right = end

  while left < right:
    while (arr[left] < pivot):
      left += 1
    while (arr[right] > pivot):
      right -= 1

    if left <= right:
      swap(arr, left, right)
      left += 1
      right -= 1
  
  return right, left


def quickSortRange(arr, start, end):
  pivot = arr[start + (end - start) // 2]

  firstEnd, secondStart = reorder(arr, start, end, pivot)

  if firstEnd > start:
    quickSortRange(arr, start, firstEnd)
  if secondStart < end:
    quickSortRange(arr, secondStart, end)


def quickSort(arr):
  quickSortRange(arr, 0, len(arr) - 1)




array1 = [-23, 12, -1, -1, 14, -3]
array2 = [3, 7, 34, 1, 1, 7, 13]
quickSort(array1)
quickSort(array2)
print(array1)
print(array2)