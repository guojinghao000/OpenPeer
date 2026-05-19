<template>
  <span
    class="star-rating"
    :class="{ interactive }"
    @mouseleave="hoverRating = 0"
  >
    <el-icon
      v-for="i in 5"
      :key="i"
      :size="size === 'large' ? 22 : 16"
      :class="{
        filled: i <= currentDisplay,
        hover: interactive && i <= hoverRating,
      }"
      @click="interactive && $emit('update:modelValue', i)"
      @mouseenter="interactive && (hoverRating = i)"
    >
      <StarFilled
        v-if="i <= currentDisplay || (interactive && i <= hoverRating)"
      />
      <Star v-else />
    </el-icon>
    <span v-if="showText" class="rating-text">{{ modelValue.toFixed(1) }}</span>
  </span>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { Star, StarFilled } from "@element-plus/icons-vue";

const props = withDefaults(
  defineProps<{
    modelValue: number;
    size?: "small" | "large";
    showText?: boolean;
    interactive?: boolean;
  }>(),
  {
    size: "small",
    showText: false,
    interactive: false,
  },
);

defineEmits<{
  "update:modelValue": [value: number];
}>();

const hoverRating = ref(0);

const currentDisplay = computed(() => {
  if (props.interactive && hoverRating.value > 0) return hoverRating.value;
  return Math.round(props.modelValue);
});
</script>

<style scoped>
.star-rating {
  display: inline-flex;
  align-items: center;
  gap: 2px;
}
.star-rating.interactive {
  cursor: pointer;
}
.star-rating.interactive .el-icon {
  transition: color 0.15s;
}
.star-rating.interactive .el-icon:hover {
  transform: scale(1.1);
}
.filled,
.hover {
  color: #f7ba2a;
}
.rating-text {
  margin-left: 4px;
  font-weight: bold;
  color: #f7ba2a;
}
</style>
