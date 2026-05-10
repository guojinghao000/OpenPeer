<template>
  <div class="home-view">
    <PaperFilter
      :categories="categories"
      @search="onSearch"
      @category-change="onCategoryChange"
      @sort-change="onSortChange"
    />

    <div v-loading="store.loading" class="paper-list">
      <PaperCard v-for="paper in store.papers" :key="paper.id" :paper="paper" />
      <AppEmpty
        v-if="!store.loading && store.papers.length === 0"
        description="暂无论文"
      />
    </div>

    <AppPagination
      v-if="store.total > 20"
      v-model:current="currentPage"
      :total="store.total"
      @change="store.setPage($event)"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { usePapersStore } from "../stores/papers";
import { categoriesApi } from "../api/categories";
import type { CategoryDto } from "../types/paper";
import PaperFilter from "../components/paper/PaperFilter.vue";
import PaperCard from "../components/paper/PaperCard.vue";
import AppPagination from "../components/common/AppPagination.vue";
import AppEmpty from "../components/common/AppEmpty.vue";

const store = usePapersStore();
const categories = ref<CategoryDto[]>([]);
const currentPage = ref(1);

onMounted(async () => {
  await store.fetchPapers();
  try {
    const { data } = await categoriesApi.getList();
    if (data.code === 200) categories.value = data.data;
  } catch {
    /* ignore */
  }
});

function onSearch(keyword: string) {
  store.setKeyword(keyword);
  currentPage.value = 1;
}

function onCategoryChange(categoryId: string | undefined) {
  store.setCategory(categoryId);
  currentPage.value = 1;
}

function onSortChange(sortBy: string, order: string) {
  store.setSort(sortBy, order);
  currentPage.value = 1;
}
</script>

<style scoped>
.paper-list {
  margin-top: 20px;
  min-height: 200px;
}
</style>
