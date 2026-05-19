<template>
  <div class="comment-item" :class="{ reply: isReply }">
    <div class="comment-header">
      <el-avatar :size="28">{{ comment.user.userName[0] }}</el-avatar>
      <span class="username">{{ comment.user.userName }}</span>
      <span class="time">{{ formatDate(comment.createdAt) }}</span>
      <span v-if="comment.updatedAt" class="edited">已编辑</span>
    </div>
    <div class="comment-content">{{ comment.content }}</div>
    <div class="comment-actions">
      <el-button
        v-if="showReplyBtn"
        size="small"
        link
        @click="$emit('reply', comment.id, comment.user.userName)"
      >
        回复
      </el-button>
      <el-button v-if="isOwner" size="small" link @click="editing = true">
        编辑
      </el-button>
      <el-popconfirm
        v-if="isOwner"
        title="确定删除此评论？"
        @confirm="$emit('delete', comment.id)"
      >
        <template #reference>
          <el-button size="small" link type="danger">删除</el-button>
        </template>
      </el-popconfirm>
    </div>
    <div v-if="editing" class="edit-form">
      <el-input
        v-model="editContent"
        type="textarea"
        :rows="2"
        maxlength="5000"
      />
      <div class="edit-actions">
        <el-button size="small" @click="editing = false">取消</el-button>
        <el-button
          size="small"
          type="primary"
          :loading="saving"
          @click="handleSave"
          >保存</el-button
        >
      </div>
    </div>
    <div v-if="comment.replies?.length" class="replies">
      <CommentItem
        v-for="reply in comment.replies"
        :key="reply.id"
        :comment="reply"
        :is-reply="true"
        :current-user-id="currentUserId"
        :show-reply-btn="showReplyBtn"
        @reply="(id, name) => $emit('reply', id, name)"
        @delete="(id) => $emit('delete', id)"
        @update="(id, content) => $emit('update', id, content)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { CommentDto } from "../../types/comment";

const props = defineProps<{
  comment: CommentDto;
  isReply?: boolean;
  currentUserId?: string | null;
  showReplyBtn?: boolean;
}>();

const emit = defineEmits<{
  reply: [id: string, userName: string];
  delete: [id: string];
  update: [id: string, content: string];
}>();

const editing = ref(false);
const editContent = ref(props.comment.content);
const saving = ref(false);

const isOwner =
  props.currentUserId != null && props.comment.user.id === props.currentUserId;

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleString("zh-CN");
}

async function handleSave() {
  if (!editContent.value.trim()) return;
  saving.value = true;
  try {
    emit("update", props.comment.id, editContent.value.trim());
    editing.value = false;
  } finally {
    saving.value = false;
  }
}
</script>

<style scoped>
.comment-item {
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;
}
.comment-item.reply {
  margin-left: 36px;
  border-bottom: none;
  padding: 8px 0;
}
.comment-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}
.username {
  font-weight: 600;
  font-size: 14px;
}
.time {
  font-size: 12px;
  color: #999;
}
.edited {
  font-size: 12px;
  color: #999;
  font-style: italic;
}
.comment-content {
  font-size: 14px;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
}
.comment-actions {
  margin-top: 6px;
}
.replies {
  margin-top: 4px;
}
.edit-form {
  margin-top: 8px;
}
.edit-actions {
  margin-top: 6px;
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}
</style>
