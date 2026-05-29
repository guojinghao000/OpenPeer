<template>
  <div class="comment-list">
    <h3>评论 ({{ total }})</h3>

    <CommentForm v-if="isAuthenticated" @submit="handleCreate" />

    <div v-if="!isAuthenticated" class="login-tip">
      <el-button link @click="$router.push('/login')">登录</el-button>后发表评论
    </div>

    <div v-if="loading" v-loading="loading" class="loading-area" />

    <template v-else-if="items.length">
      <div v-for="comment in items" :key="comment.id" class="comment-wrapper">
        <CommentItem
          :comment="comment"
          :current-user-id="currentUserId"
          :show-reply-btn="isAuthenticated"
          @reply="(id, name) => (replyingTo = { id, name })"
          @delete="handleDelete"
          @update="handleUpdate"
        />
        <CommentForm
          v-if="replyingTo?.id === comment.id"
          :reply-to="replyingTo.name"
          @submit="(content) => handleReply(comment.id, content)"
          @cancel="replyingTo = null"
        />
      </div>
      <AppPagination
        v-if="totalPages > 1"
        v-model:current="page"
        :total="total"
        @change="handlePageChange"
      />
    </template>
    <AppEmpty v-else description="暂无评论" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from "vue";
import { commentsApi } from "../../api/comments";
import { useAuthStore } from "../../stores/auth";
import { ElMessage } from "element-plus";
import CommentItem from "./CommentItem.vue";
import CommentForm from "./CommentForm.vue";
import AppPagination from "../common/AppPagination.vue";
import AppEmpty from "../common/AppEmpty.vue";
import type { CommentDto } from "../../types/comment";

const props = defineProps<{
  paperId: string;
}>();

const auth = useAuthStore();
const isAuthenticated = computed(() => auth.isAuthenticated);
const currentUserId = computed(() => auth.user?.id);

const items = ref<CommentDto[]>([]);
const loading = ref(true);
const page = ref(1);
const pageSize = 20;
const total = ref(0);
const totalPages = computed(() => Math.ceil(total.value / pageSize) || 1);
const replyingTo = ref<{ id: string; name: string } | null>(null);

const emit = defineEmits<{
  commentCountChange: [count: number];
}>();

async function fetchComments() {
  loading.value = true;
  try {
    const { data } = await commentsApi.getList(props.paperId, {
      page: page.value,
      pageSize,
    });
    if (data.code === 200) {
      items.value = data.data.items;
      total.value = data.data.total;
    }
  } catch {
    ElMessage.error("加载评论失败");
  } finally {
    loading.value = false;
  }
}

function handlePageChange(p: number) {
  page.value = p;
  fetchComments();
}

async function handleCreate(content: string) {
  try {
    await commentsApi.create(props.paperId, content);
    ElMessage.success("评论发表成功");
    emit("commentCountChange", total.value + 1);
    page.value = 1;
    await fetchComments();
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "发表失败");
  }
}

async function handleReply(parentId: string, content: string) {
  try {
    await commentsApi.create(props.paperId, content, parentId);
    ElMessage.success("回复成功");
    replyingTo.value = null;
    await fetchComments();
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "回复失败");
  }
}

async function handleDelete(id: string) {
  try {
    await commentsApi.delete(id);
    ElMessage.success("评论已删除");
    emit("commentCountChange", total.value - 1);
    await fetchComments();
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "删除失败");
  }
}

async function handleUpdate(id: string, content: string) {
  try {
    await commentsApi.update(id, content);
    ElMessage.success("评论已更新");
    await fetchComments();
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "更新失败");
  }
}

onMounted(fetchComments);

watch(
  () => props.paperId,
  () => {
    page.value = 1;
    fetchComments();
  },
);
</script>

<style scoped>
.comment-list {
  margin-top: 32px;
}
.login-tip {
  margin: 12px 0;
  font-size: 14px;
  color: #999;
}
.loading-area {
  min-height: 60px;
}
.comment-wrapper {
  margin-bottom: 4px;
}
</style>
