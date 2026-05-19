<template>
  <div v-loading="loading" class="paper-detail">
    <template v-if="paper">
      <h1>{{ paper.title }}</h1>
      <div class="meta">
        <span>作者: {{ paper.author.userName }}</span>
        <StarRating :model-value="paper.averageRating" size="large" show-text />
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
        <el-button
          @click="$router.push(`/papers/${paper.id}/edit`)"
          v-if="paper.status !== 'Retracted'"
          >编辑</el-button
        >
        <el-popconfirm title="确定删除这篇论文？" @confirm="handleDelete">
          <template #reference
            ><el-button type="danger">删除</el-button></template
          >
        </el-popconfirm>
        <el-popconfirm
          title="确定撤回这篇论文？撤回后论文将不再公开可见。"
          @confirm="showRetractDialog = true"
          v-if="paper.status === 'Published'"
        >
          <template #reference
            ><el-button type="warning">撤回</el-button></template
          >
        </el-popconfirm>
      </div>
      <div v-if="paper.status === 'Retracted'" class="retracted-banner">
        <el-alert
          title="该论文已被作者撤回"
          type="warning"
          show-icon
          :closable="false"
        />
      </div>

      <el-dialog v-model="showRetractDialog" title="撤回论文">
        <el-input
          v-model="retractReason"
          type="textarea"
          placeholder="请输入撤回原因..."
        />
        <template #footer>
          <el-button @click="showRetractDialog = false">取消</el-button>
          <el-button type="primary" @click="handleRetract">确认撤回</el-button>
        </template>
      </el-dialog>

      <h3>摘要</h3>
      <p class="abstract">{{ paper.abstract }}</p>

      <h3>PDF 预览</h3>
      <iframe v-if="paper.fileUrl" :src="paper.fileUrl" class="pdf-viewer" />

      <h3>评分</h3>
      <div class="rating-section">
        <div v-if="isAuthenticated" class="interactive-rating">
          <span class="rating-label">{{
            userRating > 0 ? "我的评分" : "给这篇论文打分"
          }}</span>
          <StarRating
            v-model="userRating"
            size="large"
            show-text
            interactive
            @update:model-value="handleRatingSubmit"
          />
        </div>
        <div v-else class="login-tip">
          <el-button link @click="$router.push('/login')">登录</el-button
          >后给论文评分
        </div>
      </div>

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

      <CommentList
        :paper-id="paper.id"
        @comment-count-change="(count) => paper && (paper.commentCount = count)"
      />
    </template>
    <AppEmpty v-else-if="!loading" description="论文不存在" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";
import { papersApi } from "../api/papers";
import { ratingsApi } from "../api/ratings";
import { ElMessage } from "element-plus";
import StarRating from "../components/common/StarRating.vue";
import AppEmpty from "../components/common/AppEmpty.vue";
import CommentList from "../components/comment/CommentList.vue";
import type { PaperDetailDto } from "../types/paper";

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const paper = ref<PaperDetailDto | null>(null);
const loading = ref(true);
const showRetractDialog = ref(false);
const retractReason = ref("");
const userRating = ref(0);

const isAuthenticated = computed(() => auth.isAuthenticated);
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

async function handleRetract() {
  if (!paper.value) return;
  try {
    await papersApi.retract(paper.value.id, retractReason.value || "作者撤回");
    ElMessage.success("论文已撤回");
    paper.value.status = "Retracted";
    showRetractDialog.value = false;
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "撤回失败");
  }
}

async function handleRatingSubmit(score: number) {
  if (!paper.value) return;
  try {
    const { data } = await ratingsApi.submitOrUpdate(paper.value.id, score);
    if (data.code === 200) {
      ElMessage.success("评分成功");
      userRating.value = score;
      const detailRes = await papersApi.getDetail(paper.value.id);
      if (detailRes.data.code === 200) {
        paper.value = detailRes.data.data;
      }
    }
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "评分失败");
  }
}

onMounted(async () => {
  try {
    const { data } = await papersApi.getDetail(route.params.id as string);
    if (data.code === 200) {
      paper.value = data.data;
      userRating.value = data.data.currentUserRating || 0;
    }
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
.retracted-banner {
  margin-bottom: 20px;
}
.rating-section {
  margin: 12px 0;
}
.interactive-rating {
  display: flex;
  align-items: center;
  gap: 12px;
}
.rating-label {
  font-size: 14px;
  color: #666;
}
.login-tip {
  font-size: 14px;
  color: #999;
}
.distribution {
  margin-top: 12px;
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
