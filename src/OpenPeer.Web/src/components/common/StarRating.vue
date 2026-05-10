<template>
  <span class="star-rating">
    <el-icon
      v-for="i in 5"
      :key="i"
      :size="size === 'large' ? 18 : 14"
      :class="{ filled: i <= Math.round(rating) }"
    >
      <StarFilled v-if="i <= Math.round(rating)" />
      <Star v-else />
    </el-icon>
    <span v-if="showText" class="rating-text">{{ rating.toFixed(1) }}</span>
  </span>
</template>

<script setup lang="ts">
import { Star, StarFilled } from "@element-plus/icons-vue";

withDefaults(
  defineProps<{
    rating: number;
    size?: "small" | "large";
    showText?: boolean;
  }>(),
  {
    size: "small",
    showText: false,
  },
);
</script>

<style scoped>
.star-rating {
  display: inline-flex;
  align-items: center;
  gap: 2px;
}
.filled {
  color: #f7ba2a;
}
.rating-text {
  margin-left: 4px;
  font-weight: bold;
  color: #f7ba2a;
}
</style>
