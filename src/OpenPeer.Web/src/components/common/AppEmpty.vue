<template>
  <div class="app-empty">
    <el-empty :description="description" />
  </div>
</template>

<script setup lang="ts">
defineProps<{ description?: string }>();
</script>
