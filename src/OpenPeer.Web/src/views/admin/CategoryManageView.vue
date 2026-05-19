<template>
  <div class="category-manage">
    <h3>分类管理</h3>
    <div style="margin-bottom: 12px">
      <el-button type="primary" @click="openCreate">新增分类</el-button>
    </div>
    <el-table :data="categories" border>
      <el-table-column prop="name" label="名称" />
      <el-table-column prop="description" label="描述" />
      <el-table-column prop="paperCount" label="论文数" width="80" />
      <el-table-column label="操作" width="150">
        <template #default="{ row }">
          <el-button size="small" @click="openEdit(row)">编辑</el-button>
          <el-popconfirm title="确定删除？" @confirm="handleDelete(row.id)">
            <template #reference
              ><el-button size="small" type="danger">删除</el-button></template
            >
          </el-popconfirm>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑分类' : '新增分类'"
    >
      <el-form :model="form" label-width="80px">
        <el-form-item label="名称">
          <el-input v-model="form.name" maxlength="100" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input
            v-model="form.description"
            type="textarea"
            maxlength="500"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import client from "../../api/client";
import { ElMessage } from "element-plus";

const categories = ref<any[]>([]);
const dialogVisible = ref(false);
const isEdit = ref(false);
const editId = ref("");
const form = reactive({ name: "", description: "" });

async function fetchCategories() {
  const { data } = await client.get("/categories");
  if (data.code === 200) categories.value = data.data;
}

function openCreate() {
  isEdit.value = false;
  form.name = "";
  form.description = "";
  dialogVisible.value = true;
}

function openEdit(row: any) {
  isEdit.value = true;
  editId.value = row.id;
  form.name = row.name;
  form.description = row.description || "";
  dialogVisible.value = true;
}

async function handleSave() {
  try {
    if (isEdit.value) {
      await client.put(`/categories/${editId.value}`, form);
    } else {
      await client.post("/categories", form);
    }
    ElMessage.success(isEdit.value ? "分类已更新" : "分类已创建");
    dialogVisible.value = false;
    await fetchCategories();
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "保存失败");
  }
}

async function handleDelete(id: string) {
  try {
    await client.delete(`/categories/${id}`);
    ElMessage.success("已删除");
    await fetchCategories();
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "删除失败");
  }
}

onMounted(fetchCategories);
</script>
