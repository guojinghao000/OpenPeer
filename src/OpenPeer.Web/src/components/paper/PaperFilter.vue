<template>
  <div class="paper-filter">
    <el-input
      v-model="keyword"
      placeholder="搜索论文..."
      clearable
      size="large"
      @change="onSearch"
      @clear="onSearch('')"
    >
      <template #prefix
        ><el-icon><Search /></el-icon
      ></template>
    </el-input>
    <div class="filter-row">
      <div class="categories">
        <el-tag
          v-for="cat in categories"
          :key="cat.id"
          :type="selectedCategory === cat.id ? 'primary' : 'info'"
          class="category-tag"
          @click="selectCategory(cat.id)"
          >{{ cat.name }}</el-tag
        >
        <el-tag
          v-if="selectedCategory"
          type="danger"
          class="category-tag"
          @click="selectCategory(undefined)"
          >清除筛选</el-tag
        >
      </div>
      <el-select
        v-model="sortValue"
        size="small"
        style="width: 140px"
        @change="onSortChange"
      >
        <el-option label="最新发布" value="publishedAt_desc" />
        <el-option label="最高评分" value="averageRating_desc" />
        <el-option label="最多评论" value="commentCount_desc" />
      </el-select>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { CategoryDto } from "../../types/paper";

const props = defineProps<{ categories: CategoryDto[] }>();
const emit = defineEmits<{
  search: [keyword: string];
  categoryChange: [categoryId: string | undefined];
  sortChange: [sortBy: string, order: string];
}>();

const keyword = ref("");
const selectedCategory = ref<string | undefined>();
const sortValue = ref("publishedAt_desc");

function onSearch(val: string | number) {
  emit("search", String(val));
}

function selectCategory(id: string | undefined) {
  selectedCategory.value = selectedCategory.value === id ? undefined : id;
  emit("categoryChange", selectedCategory.value);
}

function onSortChange(val: string) {
  const [sortBy, order] = val.split("_");
  emit("sortChange", sortBy, order);
}
</script>

<style scoped>
.filter-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 12px;
}
.categories {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.category-tag {
  cursor: pointer;
}
</style>
