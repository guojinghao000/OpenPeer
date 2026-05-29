<template>
  <div class="user-profile" v-loading="loading">
    <template v-if="profile">
      <div class="profile-header">
        <el-avatar :size="80" :src="avatarUrl" shape="circle">
          {{ profile.userName[0] }}
        </el-avatar>
        <h2>{{ profile.userName }}</h2>
        <p class="role">{{ roleLabel(profile.role) }}</p>
        <p v-if="profile.bio" class="bio">{{ profile.bio }}</p>
        <p class="stats">
          <span>{{ profile.paperCount }} 论文</span>
        </p>
        <p class="date">注册于 {{ formatDate(profile.createdAt) }}</p>
      </div>

      <h3>论文列表</h3>
      <div v-if="papers.length" class="paper-list">
        <div
          v-for="p in papers"
          :key="p.id"
          class="paper-card"
          @click="$router.push(`/papers/${p.id}`)"
        >
          <h4>{{ p.title }}</h4>
          <div class="paper-meta">
            <StarRating :model-value="p.averageRating" show-text />
            <span>{{ p.ratingCount }} 评分</span>
            <span>{{ formatDate(p.publishedAt) }}</span>
          </div>
        </div>
        <AppPagination
          v-if="papersTotal > pageSize"
          v-model:current="page"
          :total="papersTotal"
          @change="fetchPapers"
        />
      </div>
      <AppEmpty v-else description="暂无论文" />
    </template>
    <AppEmpty v-else-if="!loading" description="用户不存在" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useRoute } from "vue-router";
import { usersApi } from "../api/users";
import { papersApi } from "../api/papers";
import { ElMessage } from "element-plus";
import StarRating from "../components/common/StarRating.vue";
import AppEmpty from "../components/common/AppEmpty.vue";
import AppPagination from "../components/common/AppPagination.vue";

const route = useRoute();

const loading = ref(true);
const profile = ref<any>(null);
const papers = ref<any[]>([]);
const page = ref(1);
const pageSize = ref(20);
const papersTotal = ref(0);

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

function formatDate(d: string) {
  return new Date(d).toLocaleDateString("zh-CN");
}

function getFileExtension(fileName: string) {
  const i = fileName.lastIndexOf(".");
  return i >= 0 ? fileName.slice(i) : "";
}

const avatarUrl = computed(() => {
  if (!profile.value?.avatarPath) return "";
  const ext = getFileExtension(profile.value.avatarPath);
  return `/api/files/avatars/${profile.value.id}${ext}`;
});

async function fetchPapers() {
  try {
    const { data } = await papersApi.getList({
      authorId: route.params.id as string,
      page: page.value,
      pageSize: pageSize.value,
    });
    if (data.code === 200) {
      papers.value = data.data.items;
      papersTotal.value = data.data.total;
    }
  } catch {
    /* ignore */
  }
}

onMounted(async () => {
  try {
    const { data } = await usersApi.getPublicProfile(route.params.id as string);
    if (data.code === 200) {
      profile.value = data.data;
      await fetchPapers();
    }
  } catch {
    ElMessage.error("加载用户信息失败");
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.profile-header {
  text-align: center;
  margin-bottom: 24px;
}
.profile-header h2 {
  margin: 8px 0 0;
}
.role {
  color: #409eff;
  margin: 4px 0;
}
.bio {
  color: #666;
  margin: 8px 0;
}
.stats span {
  margin: 0 12px;
  color: #666;
}
.date {
  color: #999;
  font-size: 13px;
}
.paper-card {
  border: 1px solid #eee;
  border-radius: 8px;
  padding: 12px 16px;
  margin-bottom: 8px;
  cursor: pointer;
  transition: background 0.15s;
}
.paper-card:hover {
  background: #f5f7fa;
}
.paper-card h4 {
  margin: 0 0 8px;
}
.paper-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 13px;
  color: #666;
}
</style>
