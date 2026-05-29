<template>
  <div class="ai-config-view" v-loading="loading">
    <h2>AI 论文生成配置</h2>
    <p class="desc">
      配置 AI 服务商以使用 LaTeX 论文自动生成功能。API Key
      将加密存储，仅用于论文生成请求。
    </p>

    <el-form :model="form" label-width="100px" style="max-width: 500px">
      <el-form-item label="服务商" required>
        <el-select v-model="form.provider" placeholder="选择 AI 服务商">
          <el-option label="OpenAI" value="openai" />
          <el-option label="DeepSeek" value="deepseek" />
          <el-option label="Anthropic" value="anthropic" />
        </el-select>
      </el-form-item>

      <el-form-item label="API Key" required>
        <el-input
          v-model="form.apiKey"
          type="password"
          show-password
          :placeholder="
            hasConfig ? '输入新 Key 以替换（留空则不修改）' : '输入 API Key'
          "
        />
      </el-form-item>

      <el-form-item label="模型" required>
        <el-input v-model="form.model" :placeholder="modelPlaceholder" />
        <div class="tip">
          例如: gpt-4o, deepseek-chat, claude-3-5-sonnet-20241022
        </div>
      </el-form-item>

      <el-form-item>
        <el-button type="primary" :loading="saving" @click="handleSave"
          >保存配置</el-button
        >
        <el-button @click="handleTest" :disabled="!hasConfig || !form.apiKey"
          >测试连接</el-button
        >
      </el-form-item>
    </el-form>

    <template v-if="latexResult">
      <el-divider />
      <h3>生成的 LaTeX 源码</h3>
      <el-input
        v-model="latexResult"
        type="textarea"
        :rows="20"
        readonly
        style="font-family: monospace; font-size: 13px"
      />
      <el-button style="margin-top: 12px" @click="copyLatex"
        >复制到剪贴板</el-button
      >
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { ElMessage } from "element-plus";
import { aiApi } from "../api/ai";
import type { AiConfig } from "../types/data";

const loading = ref(false);
const saving = ref(false);
const hasConfig = ref(false);
const latexResult = ref("");
const modelPlaceholder = computed(() => {
  switch (form.value.provider) {
    case "openai":
      return "gpt-4o";
    case "deepseek":
      return "deepseek-chat";
    case "anthropic":
      return "claude-3-5-sonnet-20241022";
    default:
      return "";
  }
});

const form = ref({
  provider: "openai",
  apiKey: "",
  model: "gpt-4o",
});

onMounted(async () => {
  loading.value = true;
  try {
    const { data: res } = await aiApi.getConfig();
    const config = res.data as AiConfig;
    if (config.provider) {
      form.value.provider = config.provider;
      form.value.model = config.model;
      form.value.apiKey = "";
      hasConfig.value = true;
    }
  } catch {
    // not configured yet
  } finally {
    loading.value = false;
  }
});

async function handleSave() {
  if (!form.value.provider) {
    ElMessage.warning("请选择服务商");
    return;
  }
  if (!form.value.apiKey && !hasConfig.value) {
    ElMessage.warning("请输入 API Key");
    return;
  }
  if (!form.value.model) {
    ElMessage.warning("请输入模型名称");
    return;
  }

  saving.value = true;
  try {
    await aiApi.updateConfig({
      provider: form.value.provider,
      apiKey: form.value.apiKey,
      model: form.value.model,
    });
    ElMessage.success("AI 配置已保存");
    hasConfig.value = true;
  } catch (err: any) {
    ElMessage.error(err.response?.data?.message || "保存失败");
  } finally {
    saving.value = false;
  }
}

async function handleTest() {
  ElMessage.info("连接测试功能将在后续版本中完善");
}

function copyLatex() {
  navigator.clipboard.writeText(latexResult.value);
  ElMessage.success("已复制到剪贴板");
}
</script>

<style scoped>
.ai-config-view {
  max-width: 800px;
  margin: 0 auto;
  padding: 24px;
}

.desc {
  color: #666;
  margin-bottom: 24px;
}

.tip {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}
</style>
