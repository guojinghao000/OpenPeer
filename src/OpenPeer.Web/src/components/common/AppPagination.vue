<template>
  <div class="pagination">
    <el-pagination
      v-model:current-page="current"
      v-model:page-size="size"
      :total="total"
      :page-sizes="[20]"
      layout="total, prev, pager, next"
      background
      @current-change="$emit('change', $event)"
    />
  </div>
</template>

<script setup lang="ts">
defineProps<{ total: number }>();
defineEmits<{ change: [page: number] }>();

const current = defineModel<number>("current", { default: 1 });
const size = defineModel<number>("size", { default: 20 });
</script>

<style scoped>
.pagination {
  display: flex;
  justify-content: center;
  margin-top: 24px;
}
</style>
