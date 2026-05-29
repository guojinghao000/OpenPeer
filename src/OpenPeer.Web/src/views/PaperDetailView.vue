<template>
  <div v-loading="loading" class="paper-detail">
    <template v-if="paper">
      <h1>{{ paper.title }}</h1>
      <div class="meta">
        <span
          >作者:
          <router-link :to="`/users/${paper.author.id}`" class="author-link">{{
            paper.author.userName
          }}</router-link></span
        >
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
      <PdfViewer v-if="paper.fileUrl" :src="paper.fileUrl" />

      <h3>支撑数据</h3>
      <div class="supporting-data-section">
        <div v-if="isAuthor" class="upload-data">
          <el-upload
            :show-file-list="false"
            :http-request="handleDataUpload"
            accept=".jpg,.jpeg,.png,.webp,.pdf,.doc,.docx,.csv,.xlsx,.json,.zip"
          >
            <el-button size="small" type="primary">上传支撑数据</el-button>
          </el-upload>
          <el-input
            v-model="dataDescription"
            placeholder="文件说明（可选）"
            size="small"
            style="width: 300px; margin-left: 8px"
          />
        </div>
        <div v-loading="dataLoading" class="data-list">
          <div v-for="item in dataItems" :key="item.id" class="data-item">
            <el-icon><Document /></el-icon>
            <a :href="getDataFileUrl(item)" target="_blank" class="data-name">
              {{ item.fileName }}
            </a>
            <span class="data-size">{{ formatFileSize(item.fileSize) }}</span>
            <span class="data-desc" v-if="item.description">{{
              item.description
            }}</span>
            <el-button
              v-if="isAuthor"
              type="danger"
              link
              size="small"
              @click="handleDataDelete(item.id)"
            >
              删除
            </el-button>
          </div>
          <AppEmpty
            v-if="!dataItems.length && !dataLoading"
            description="暂无支撑数据"
          />
        </div>
      </div>

      <div v-if="isAuthenticated" class="latex-generation">
        <el-button type="success" @click="showLatexDialog = true">
          使用 AI 生成 LaTeX 论文
        </el-button>
      </div>

      <el-dialog
        v-model="showLatexDialog"
        title="AI 生成 LaTeX 论文"
        width="600px"
      >
        <el-form label-width="80px">
          <el-form-item label="论文标题" required>
            <el-input v-model="latexTitle" placeholder="输入论文标题" />
          </el-form-item>
          <el-form-item label="生成提示" required>
            <el-input
              v-model="latexPrompt"
              type="textarea"
              :rows="4"
              placeholder="描述你想生成的论文内容..."
            />
          </el-form-item>
          <el-form-item label="选择数据">
            <el-checkbox-group
              v-model="selectedDataIds"
              v-if="dataItems.length"
            >
              <el-checkbox
                v-for="item in dataItems"
                :key="item.id"
                :value="item.id"
              >
                {{ item.fileName }}
              </el-checkbox>
            </el-checkbox-group>
            <span v-else class="no-data-tip">请先上传支撑数据</span>
          </el-form-item>
        </el-form>
        <template #footer>
          <el-button @click="showLatexDialog = false">取消</el-button>
          <el-button
            type="primary"
            :loading="generating"
            :disabled="!dataItems.length"
            @click="handleGenerateLatex"
          >
            生成
          </el-button>
        </template>
        <div v-if="generatedLatex" style="margin-top: 16px">
          <h4>生成结果</h4>
          <el-input
            v-model="generatedLatex"
            type="textarea"
            :rows="15"
            readonly
            style="font-family: monospace; font-size: 13px"
          />
          <el-button size="small" style="margin-top: 8px" @click="copyLatex"
            >复制 LaTeX</el-button
          >
        </div>
      </el-dialog>

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

      <div class="rating-list" v-if="ratingItems.length">
        <h3>最近评分</h3>
        <div v-for="r in ratingItems" :key="r.id" class="rating-item">
          <span class="rating-user">{{ r.user.userName }}</span>
          <StarRating :model-value="r.score" />
          <span class="rating-date">{{ formatDate(r.createdAt) }}</span>
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
import { ref, computed, onMounted, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";
import { papersApi } from "../api/papers";
import { ratingsApi } from "../api/ratings";
import { dataApi } from "../api/data";
import { aiApi } from "../api/ai";
import { ElMessage } from "element-plus";
import { Document } from "@element-plus/icons-vue";
import StarRating from "../components/common/StarRating.vue";
import PdfViewer from "../components/common/PdfViewer.vue";
import AppEmpty from "../components/common/AppEmpty.vue";
import CommentList from "../components/comment/CommentList.vue";
import type { PaperDetailDto } from "../types/paper";
import type { RatingDto } from "../types/rating";
import type { SupportingDataItem } from "../types/data";

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const paper = ref<PaperDetailDto | null>(null);
const loading = ref(true);
const showRetractDialog = ref(false);
const retractReason = ref("");
const userRating = ref(0);
const ratingItems = ref<RatingDto[]>([]);

const dataItems = ref<SupportingDataItem[]>([]);
const dataLoading = ref(false);
const dataDescription = ref("");
const showLatexDialog = ref(false);
const latexTitle = ref("");
const latexPrompt = ref("");
const selectedDataIds = ref<string[]>([]);
const generating = ref(false);
const generatedLatex = ref("");

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

function getDataFileUrl(item: SupportingDataItem) {
  const ext = item.fileName.includes(".")
    ? item.fileName.substring(item.fileName.lastIndexOf("."))
    : "";
  return `/api/files/data/${item.id}${ext}`;
}

function formatFileSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

async function fetchData() {
  if (!paper.value) return;
  dataLoading.value = true;
  try {
    const { data } = await dataApi.getList(paper.value.id);
    if (data.code === 200) {
      dataItems.value = data.data;
    }
  } catch {
    // ignore
  } finally {
    dataLoading.value = false;
  }
}

async function handleDataUpload(upload: any) {
  if (!paper.value) return;
  try {
    const { data } = await dataApi.upload(
      paper.value.id,
      upload.file,
      dataDescription.value || undefined,
    );
    if (data.code === 201) {
      ElMessage.success("支撑数据上传成功");
      dataDescription.value = "";
      await fetchData();
    }
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "上传失败");
  }
}

async function handleDataDelete(id: string) {
  if (!paper.value) return;
  try {
    await dataApi.delete(paper.value.id, id);
    ElMessage.success("已删除");
    dataItems.value = dataItems.value.filter((d) => d.id !== id);
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "删除失败");
  }
}

async function handleGenerateLatex() {
  if (!latexTitle.value || !latexPrompt.value) {
    ElMessage.warning("请填写标题和提示");
    return;
  }
  generating.value = true;
  try {
    const { data } = await aiApi.generateLatex({
      title: latexTitle.value,
      dataIds: selectedDataIds.value,
      prompt: latexPrompt.value,
    });
    if (data.code === 200) {
      generatedLatex.value = data.data.latex;
      ElMessage.success("LaTeX 论文生成成功");
    }
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "生成失败");
  } finally {
    generating.value = false;
  }
}

function copyLatex() {
  navigator.clipboard.writeText(generatedLatex.value);
  ElMessage.success("已复制到剪贴板");
}

watch(
  () => route.params.id,
  () => {
    paper.value = null;
    dataItems.value = [];
    generatedLatex.value = "";
  },
);

async function fetchRatings() {
  try {
    const { data } = await ratingsApi.getList(route.params.id as string, {
      page: 1,
      pageSize: 10,
    });
    if (data.code === 200) {
      ratingItems.value = data.data.items;
    }
  } catch {
    /* ignore */
  }
}

onMounted(async () => {
  try {
    const { data } = await papersApi.getDetail(route.params.id as string);
    if (data.code === 200) {
      paper.value = data.data;
      userRating.value = data.data.currentUserRating || 0;
    }
    await fetchRatings();
    await fetchData();
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
.rating-list {
  margin-top: 24px;
}
.rating-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 0;
}
.author-link {
  color: #409eff;
  text-decoration: none;
}
.author-link:hover {
  text-decoration: underline;
}
.rating-user {
  font-weight: 500;
  min-width: 80px;
}
.rating-date {
  color: #999;
  font-size: 12px;
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
.supporting-data-section {
  margin: 12px 0;
}
.upload-data {
  display: flex;
  align-items: center;
  margin-bottom: 12px;
}
.data-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}
.data-name {
  color: #409eff;
  text-decoration: none;
}
.data-name:hover {
  text-decoration: underline;
}
.data-size {
  color: #999;
  font-size: 12px;
}
.data-desc {
  color: #666;
  font-size: 13px;
}
.latex-generation {
  margin: 16px 0;
}
.no-data-tip {
  color: #999;
  font-size: 13px;
}
</style>
