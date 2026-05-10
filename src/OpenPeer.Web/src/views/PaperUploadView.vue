<template>
  <div class="upload-view">
    <h2>上传论文</h2>
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-position="top"
      class="upload-form"
    >
      <el-form-item label="标题" prop="title">
        <el-input
          v-model="form.title"
          maxlength="200"
          show-word-limit
          placeholder="论文标题 (5-200字符)"
        />
      </el-form-item>
      <el-form-item label="摘要" prop="abstract">
        <el-input
          v-model="form.abstract"
          type="textarea"
          :rows="6"
          maxlength="2000"
          show-word-limit
          placeholder="论文摘要 (20-2000字符)"
        />
      </el-form-item>
      <el-form-item label="分类">
        <el-select
          v-model="form.categoryIds"
          multiple
          placeholder="选择分类 (可选)"
        >
          <el-option
            v-for="cat in categories"
            :key="cat.id"
            :label="cat.name"
            :value="cat.id"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="PDF 文件" prop="file">
        <el-upload
          ref="uploadRef"
          :auto-upload="false"
          :limit="1"
          accept=".pdf"
          :on-change="onFileChange"
          :on-remove="onFileRemove"
          drag
        >
          <el-icon class="el-icon--upload"><UploadFilled /></el-icon>
          <div class="el-upload__text">拖拽或 <em>点击上传</em></div>
          <template #tip
            ><div class="el-upload__tip">
              仅支持 PDF 文件，最大 10MB
            </div></template
          >
        </el-upload>
      </el-form-item>
      <el-form-item>
        <el-button
          type="primary"
          size="large"
          :loading="submitting"
          @click="handleSubmit"
        >
          发布论文
        </el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { papersApi } from "../api/papers";
import { categoriesApi } from "../api/categories";
import { ElMessage } from "element-plus";
import type { CategoryDto } from "../types/paper";
import type { UploadFile } from "element-plus";

const router = useRouter();
const submitting = ref(false);
const categories = ref<CategoryDto[]>([]);
const selectedFile = ref<File | null>(null);

const form = reactive({
  title: "",
  abstract: "",
  categoryIds: [] as string[],
});

const rules = {
  title: [
    { required: true, message: "请输入标题", trigger: "blur" },
    { min: 5, max: 200, message: "标题长度5-200字符", trigger: "blur" },
  ],
  abstract: [
    { required: true, message: "请输入摘要", trigger: "blur" },
    { min: 20, max: 2000, message: "摘要长度20-2000字符", trigger: "blur" },
  ],
};

function onFileChange(file: UploadFile) {
  if (file.raw) {
    if (file.raw.type !== "application/pdf") {
      ElMessage.error("仅接受 PDF 格式文件");
      return;
    }
    if (file.raw.size > 10 * 1024 * 1024) {
      ElMessage.error("文件大小不能超过 10MB");
      return;
    }
    selectedFile.value = file.raw;
  }
}

function onFileRemove() {
  selectedFile.value = null;
}

async function handleSubmit() {
  if (!selectedFile.value) {
    ElMessage.warning("请选择 PDF 文件");
    return;
  }
  submitting.value = true;
  try {
    const fd = new FormData();
    fd.append("title", form.title);
    fd.append("abstract", form.abstract);
    if (form.categoryIds.length > 0) {
      fd.append("categoryIds", form.categoryIds.join(","));
    }
    fd.append("file", selectedFile.value);

    const { data } = await papersApi.upload(fd);
    if (data.code === 201) {
      ElMessage.success("论文发布成功");
      router.push(`/papers/${data.data.id}`);
    }
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "上传失败");
  } finally {
    submitting.value = false;
  }
}

onMounted(async () => {
  try {
    const { data } = await categoriesApi.getList();
    if (data.code === 200) categories.value = data.data;
  } catch {
    /* ignore */
  }
});
</script>

<style scoped>
.upload-form {
  max-width: 700px;
  margin: 0 auto;
}
</style>
