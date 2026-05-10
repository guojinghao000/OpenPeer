import { defineStore } from "pinia";
import { ref } from "vue";
import { papersApi } from "../api/papers";
import type { PaperDto, PaperListParams } from "../types/paper";

export const usePapersStore = defineStore("papers", () => {
  const papers = ref<PaperDto[]>([]);
  const total = ref(0);
  const loading = ref(false);
  const filters = ref<PaperListParams>({
    page: 1,
    pageSize: 20,
    sortBy: "publishedAt",
    order: "desc",
    categoryId: undefined,
    keyword: "",
  });

  async function fetchPapers() {
    loading.value = true;
    try {
      const { data } = await papersApi.getList(filters.value);
      if (data.code === 200) {
        papers.value = data.data.items;
        total.value = data.data.total;
      }
    } finally {
      loading.value = false;
    }
  }

  function setPage(page: number) {
    filters.value.page = page;
    fetchPapers();
  }

  function setKeyword(keyword: string) {
    filters.value.keyword = keyword;
    filters.value.page = 1;
    fetchPapers();
  }

  function setCategory(categoryId: string | undefined) {
    filters.value.categoryId = categoryId;
    filters.value.page = 1;
    fetchPapers();
  }

  function setSort(sortBy: string, order: string) {
    filters.value.sortBy = sortBy;
    filters.value.order = order;
    filters.value.page = 1;
    fetchPapers();
  }

  return {
    papers,
    total,
    loading,
    filters,
    fetchPapers,
    setPage,
    setKeyword,
    setCategory,
    setSort,
  };
});
