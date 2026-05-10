<template>
  <el-card
    class="paper-card"
    shadow="hover"
    @click="$router.push(`/papers/${paper.id}`)"
  >
    <h3>{{ paper.title }}</h3>
    <p class="abstract">
      {{ paper.abstract.slice(0, 200)
      }}{{ paper.abstract.length > 200 ? "..." : "" }}
    </p>
    <div class="meta">
      <span class="author">{{ paper.author.userName }}</span>
      <StarRating :rating="paper.averageRating" />
      <span>{{ paper.commentCount }} 评论</span>
      <span>{{ formatDate(paper.publishedAt) }}</span>
    </div>
  </el-card>
</template>

<script setup lang="ts">
import type { PaperDto } from "../../types/paper";
import StarRating from "../common/StarRating.vue";

defineProps<{ paper: PaperDto }>();

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString("zh-CN");
}
</script>

<style scoped>
.paper-card {
  margin-bottom: 16px;
  cursor: pointer;
}
.paper-card h3 {
  margin: 0 0 8px;
  font-size: 18px;
}
.abstract {
  color: #666;
  font-size: 14px;
  margin-bottom: 12px;
}
.meta {
  display: flex;
  align-items: center;
  gap: 16px;
  font-size: 13px;
  color: #999;
}
.author {
  color: #409eff;
}
</style>
