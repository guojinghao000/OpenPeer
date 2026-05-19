<template>
  <div class="edit-view" v-loading="loading">
    <h2>编辑论文</h2>
    <el-form
      v-if="paper"
      :model="form"
      :rules="rules"
      label-position="top"
      class="edit-form"
    >
      <el-form-item label="标题" prop="title">
        <el-input v-model="form.title" maxlength="200" show-word-limit />
      </el-form-item>
      <el-form-item label="摘要" prop="abstract">
        <el-input
          v-model="form.abstract"
          type="textarea"
          :rows="6"
          maxlength="2000"
          show-word-limit
        />
      </el-form-item>
      <el-form-item label="分类">
        <el-select v-model="form.categoryIds" multiple placeholder="选择分类">
          <el-option
            v-for="cat in categories"
            :key="cat.id"
            :label="cat.name"
            :value="cat.id"
          />
        </el-select>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" :loading="saving" @click="handleSubmit"
          >保存</el-button
        >
        <el-button @click="$router.push(`/papers/${id}`)">取消</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { papersApi } from "../api/papers";
import { categoriesApi } from "../api/categories";
import { ElMessage } from "element-plus";
import type { CategoryDto } from "../types/paper";

const route = useRoute();
const router = useRouter();
const id = route.params.id as string;
const loading = ref(true);
const saving = ref(false);
const categories = ref<CategoryDto[]>([]);
const paper = ref<any>(null);

const form = reactive({ title: "", abstract: "", categoryIds: [] as string[] });

const rules = {
  title: [
    { required: true, message: "请输入标题", trigger: "blur" },
    { min: 5, max: 200, message: "标题5-200字符", trigger: "blur" },
  ],
  abstract: [
    { required: true, message: "请输入摘要", trigger: "blur" },
    { min: 20, max: 2000, message: "摘要20-2000字符", trigger: "blur" },
  ],
};

async function handleSubmit() {
  saving.value = true;
  try {
    await papersApi.update(id, {
      title: form.title,
      abstract: form.abstract,
      categoryIds: form.categoryIds.length > 0 ? form.categoryIds : null,
    });
    ElMessage.success("论文已更新");
    router.push(`/papers/${id}`);
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "更新失败");
  } finally {
    saving.value = false;
  }
}

onMounted(async () => {
  try {
    const [pRes, cRes] = await Promise.all([
      papersApi.getDetail(id),
      categoriesApi.getList(),
    ]);
    if (pRes.data.code === 200) {
      paper.value = pRes.data.data;
      form.title = paper.value.title;
      form.abstract = paper.value.abstract;
      form.categoryIds = paper.value.categories.map((c: any) => c.id);
    }
    if (cRes.data.code === 200) categories.value = cRes.data.data;
  } catch {
    ElMessage.error("加载失败");
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.edit-form {
  max-width: 700px;
  margin: 0 auto;
}
</style>
