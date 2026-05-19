<template>
  <div class="comment-form">
    <div v-if="replyTo" class="reply-hint">
      回复 {{ replyTo }}：
      <el-button size="small" link @click="$emit('cancel')">取消</el-button>
    </div>
    <el-input
      v-model="content"
      type="textarea"
      :rows="3"
      maxlength="5000"
      show-word-limit
      :placeholder="replyTo ? '输入回复内容...' : '发表评论...'"
    />
    <div class="form-footer">
      <el-button
        type="primary"
        size="small"
        :loading="submitting"
        :disabled="!content.trim()"
        @click="handleSubmit"
      >
        发表
      </el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";

const props = withDefaults(
  defineProps<{
    replyTo?: string | null;
    initialContent?: string;
  }>(),
  {
    replyTo: null,
    initialContent: "",
  },
);

const emit = defineEmits<{
  submit: [content: string];
  cancel: [];
}>();

const content = ref(props.initialContent);
const submitting = ref(false);

async function handleSubmit() {
  if (!content.value.trim()) return;
  submitting.value = true;
  try {
    emit("submit", content.value.trim());
    content.value = "";
  } finally {
    submitting.value = false;
  }
}
</script>

<style scoped>
.comment-form {
  margin: 12px 0;
}
.reply-hint {
  font-size: 13px;
  color: #666;
  margin-bottom: 6px;
}
.form-footer {
  margin-top: 8px;
  text-align: right;
}
</style>
