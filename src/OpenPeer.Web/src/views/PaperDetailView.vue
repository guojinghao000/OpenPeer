<template>
  <div v-loading="loading" class="paper-detail">
    <template v-if="paper">
      <h1>{{ paper.title }}</h1>
      <div class="meta">
        <span>作者: {{ paper.author.userName }}</span>
        <StarRating :rating="paper.averageRating" size="large" show-text />
        <span>{{ paper.ratingCount }} 评分</span>
        <span>{{ paper.commentCount }} 评论</span>
        <span class="published">{{ formatDate(paper.publishedAt) }}</span>
      </div>

      <div class="categories">
        <el-tag v-for="cat in paper.categories" :key="cat.id" class="cat-tag">{{
          cat.name
        }}</el-tag>
      </div>

      <div class="actions" v-if="isAuthor">
        <el-button @click="$router.push(`/papers/${paper.id}/edit`)"
          >编辑</el-button
        >
        <el-popconfirm title="确定删除这篇论文？" @confirm="handleDelete">
          <template #reference
            ><el-button type="danger">删除</el-button></template
          >
        </el-popconfirm>
      </div>

      <h3>摘要</h3>
      <p class="abstract">{{ paper.abstract }}</p>

      <h3>PDF 预览</h3>
      <iframe v-if="paper.fileUrl" :src="paper.fileUrl" class="pdf-viewer" />

      <div v-if="paper.ratingDistribution" class="distribution">
        <h3>评分分布</h3>
        <div class="bars">
          <div v-for="star in 5" :key="star" class="bar-row">
            <span>{{ star }} 星</span>
            <el-progress :percentage="getPercentage(star)" :show-text="false" />
            <span class="count">{{ getCount(star) }}</span>
          </div>
        </div>
      </div>
    </template>
    <AppEmpty v-else-if="!loading" description="论文不存在" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";
import { papersApi } from "../api/papers";
import { ElMessage } from "element-plus";
import StarRating from "../components/common/StarRating.vue";
import AppEmpty from "../components/common/AppEmpty.vue";
import type { PaperDetailDto } from "../types/paper";

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const paper = ref<PaperDetailDto | null>(null);
const loading = ref(true);

const isAuthor = computed(() =>
  paper.value && auth.user ? paper.value.author.id === auth.user.id : false,
);

function getCount(star: number) {
  if (!paper.value?.ratingDistribution) return 0;
  const dist = paper.value.ratingDistribution as Record<string, number>;
  return dist[`star${star}`] || 0;
}

function getPercentage(star: number) {
  const count = getCount(star);
  const total = paper.value?.ratingCount || 1;
  return Math.round((count / total) * 100);
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString("zh-CN");
}

async function handleDelete() {
  if (!paper.value) return;
  try {
    await papersApi.delete(paper.value.id);
    ElMessage.success("论文已删除");
    router.push("/");
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "删除失败");
  }
}

onMounted(async () => {
  try {
    const { data } = await papersApi.getDetail(route.params.id as string);
    if (data.code === 200) paper.value = data.data;
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "加载失败");
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.meta {
  display: flex;
  align-items: center;
  gap: 16px;
  margin: 12px 0;
  font-size: 14px;
  color: #666;
}
.published {
  color: #999;
}
.categories {
  margin-bottom: 16px;
}
.cat-tag {
  margin-right: 8px;
}
.actions {
  margin-bottom: 20px;
}
.abstract {
  margin: 12px 0;
  line-height: 1.8;
}
.pdf-viewer {
  width: 100%;
  height: 600px;
  border: 1px solid #ddd;
  border-radius: 4px;
}
.distribution {
  margin-top: 24px;
}
.bars {
  max-width: 400px;
}
.bar-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}
.count {
  min-width: 30px;
  color: #999;
}
</style>
