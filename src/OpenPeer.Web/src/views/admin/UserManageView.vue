<template>
  <div class="user-manage">
    <h3>用户管理</h3>
    <div style="margin-bottom: 12px; display: flex; gap: 8px">
      <el-input
        v-model="search"
        placeholder="搜索用户名或邮箱"
        clearable
        style="max-width: 300px"
        @clear="handleSearch"
        @keyup.enter="handleSearch"
      />
      <el-button type="primary" @click="handleSearch">搜索</el-button>
    </div>
    <el-table :data="users" border v-loading="loading">
      <el-table-column prop="userName" label="用户名" width="150" />
      <el-table-column prop="email" label="邮箱" min-width="200" />
      <el-table-column prop="role" label="角色" width="100">
        <template #default="{ row }">
          <el-tag
            :type="
              row.role === 'Admin'
                ? 'danger'
                : row.role === 'Author'
                  ? 'warning'
                  : 'info'
            "
          >
            {{ roleLabel(row.role) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="paperCount" label="论文数" width="80" />
      <el-table-column prop="reputationScore" label="信誉分" width="100">
        <template #default="{ row }">{{
          row.reputationScore.toFixed(2)
        }}</template>
      </el-table-column>
      <el-table-column prop="createdAt" label="注册时间" width="120">
        <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="140">
        <template #default="{ row }">
          <el-select
            :model-value="row.role"
            size="small"
            @change="(val: string) => handleRoleChange(row, val)"
          >
            <el-option label="读者" value="Reader" />
            <el-option label="作者" value="Author" />
            <el-option label="管理员" value="Admin" />
          </el-select>
        </template>
      </el-table-column>
    </el-table>
    <AppPagination
      v-if="total > pageSize"
      v-model:current="page"
      :total="total"
      @change="fetchUsers"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { usersApi } from "../../api/users";
import { ElMessage, ElMessageBox } from "element-plus";
import AppPagination from "../../components/common/AppPagination.vue";

const users = ref<any[]>([]);
const loading = ref(false);
const page = ref(1);
const pageSize = ref(20);
const total = ref(0);
const search = ref("");

function formatDate(d: string) {
  return new Date(d).toLocaleDateString("zh-CN");
}

function roleLabel(role: string) {
  switch (role) {
    case "Admin":
      return "管理员";
    case "Author":
      return "作者";
    default:
      return "读者";
  }
}

function handleSearch() {
  page.value = 1;
  fetchUsers();
}

async function fetchUsers() {
  loading.value = true;
  try {
    const { data } = await usersApi.getAdminList({
      page: page.value,
      pageSize: pageSize.value,
      search: search.value || undefined,
    });
    if (data.code === 200) {
      users.value = data.data.items;
      total.value = data.data.total;
    }
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "加载失败");
  } finally {
    loading.value = false;
  }
}

async function handleRoleChange(row: any, newRole: string) {
  if (newRole === row.role) return;
  try {
    await ElMessageBox.confirm(
      `确定将用户「${row.userName}」的角色改为「${roleLabel(newRole)}」?`,
      "确认修改",
      { confirmButtonText: "确定", cancelButtonText: "取消", type: "warning" },
    );
    await usersApi.updateUserRole(row.id, { role: newRole });
    ElMessage.success("角色已更新");
    row.role = newRole;
  } catch {
    /* cancelled or error */
  }
}

onMounted(fetchUsers);
</script>
