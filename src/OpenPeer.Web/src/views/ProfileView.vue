<template>
  <div class="profile-view" v-loading="loading">
    <template v-if="profile">
      <div class="profile-header">
        <div class="avatar-section">
          <el-avatar :size="80" :src="avatarUrl" shape="circle">
            {{ profile.userName[0] }}
          </el-avatar>
          <el-upload
            :show-file-list="false"
            :http-request="handleAvatarUpload"
            accept="image/jpeg,image/png,image/webp,image/gif"
          >
            <el-button size="small" style="margin-top: 8px">更换头像</el-button>
          </el-upload>
        </div>
        <h2>{{ profile.userName }}</h2>
        <p class="role">{{ roleLabel(profile.role) }}</p>
        <p class="stats">
          <span>{{ profile.paperCount }} 论文</span>
          <span>{{ profile.ratingCount }} 评分</span>
          <span>{{ profile.commentCount }} 评论</span>
        </p>
        <p class="date">注册于 {{ formatDate(profile.createdAt) }}</p>
      </div>

      <el-tabs v-model="activeTab">
        <el-tab-pane label="个人信息" name="info">
          <el-form :model="editForm" label-width="80px">
            <el-form-item label="用户名">
              <el-input :model-value="profile.userName" disabled />
            </el-form-item>
            <el-form-item label="个人简介">
              <el-input
                v-model="editForm.bio"
                type="textarea"
                :rows="3"
                maxlength="500"
                show-word-limit
                placeholder="介绍一下自己..."
              />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" :loading="saving" @click="handleSave"
                >保存</el-button
              >
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <el-tab-pane label="修改密码" name="password">
          <el-form
            :model="pwForm"
            :rules="pwRules"
            label-width="100px"
            style="max-width: 400px"
          >
            <el-form-item label="当前密码" prop="currentPassword">
              <el-input v-model="pwForm.currentPassword" type="password" />
            </el-form-item>
            <el-form-item label="新密码" prop="newPassword">
              <el-input v-model="pwForm.newPassword" type="password" />
            </el-form-item>
            <el-form-item label="确认新密码" prop="confirmNewPassword">
              <el-input v-model="pwForm.confirmNewPassword" type="password" />
            </el-form-item>
            <el-form-item>
              <el-button
                type="primary"
                :loading="pwSaving"
                @click="handleChangePassword"
                >修改密码</el-button
              >
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <el-tab-pane label="我的论文" name="papers">
          <div v-loading="papersLoading">
            <el-table
              v-if="papers.length"
              :data="papers"
              border
              @row-click="(row: any) => goPaperById(row.id)"
            >
              <el-table-column
                prop="title"
                label="标题"
                min-width="250"
                show-overflow-tooltip
              />
              <el-table-column label="评分" width="120">
                <template #default="{ row }">
                  <span
                    >{{ row.averageRating.toFixed(1) }} ({{
                      row.ratingCount
                    }})</span
                  >
                </template>
              </el-table-column>
              <el-table-column prop="publishedAt" label="发布时间" width="160">
                <template #default="{ row }">{{
                  formatDate(row.publishedAt)
                }}</template>
              </el-table-column>
            </el-table>
            <AppEmpty v-else description="还没有发布论文" />
            <AppPagination
              v-if="papersTotal > 20"
              v-model:current="papersPage"
              :total="papersTotal"
              @change="fetchPapers"
            />
          </div>
        </el-tab-pane>

        <el-tab-pane label="我的评分" name="ratings">
          <div v-loading="ratingsLoading">
            <div v-if="ratings.length" class="rating-list">
              <div
                v-for="r in ratings"
                :key="r.id"
                class="rating-item"
                @click="goPaperById(r.paperId)"
              >
                <span class="paper-title">{{ r.paperTitle }}</span>
                <StarRating :model-value="r.score" :show-text="false" />
                <span class="rating-date">{{ formatDate(r.createdAt) }}</span>
              </div>
            </div>
            <AppEmpty v-else description="还没有评分" />
            <AppPagination
              v-if="ratingsTotal > 20"
              v-model:current="ratingsPage"
              :total="ratingsTotal"
              @change="fetchRatings"
            />
          </div>
        </el-tab-pane>

        <el-tab-pane label="我的评论" name="comments">
          <div v-loading="commentsLoading">
            <div v-if="comments.length" class="comment-list">
              <div
                v-for="c in comments"
                :key="c.id"
                class="comment-item"
                @click="goPaperById(c.paperId)"
              >
                <span class="paper-title">{{ c.paperTitle }}</span>
                <p class="comment-content">{{ c.content }}</p>
                <span class="comment-date">{{ formatDate(c.createdAt) }}</span>
              </div>
            </div>
            <AppEmpty v-else description="还没有评论" />
            <AppPagination
              v-if="commentsTotal > 20"
              v-model:current="commentsPage"
              :total="commentsTotal"
              @change="fetchComments"
            />
          </div>
        </el-tab-pane>
      </el-tabs>
    </template>
    <AppEmpty v-else-if="!loading" description="加载失败" />
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, computed, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
import { usersApi } from "../api/users";
import { ElMessage } from "element-plus";
import AppEmpty from "../components/common/AppEmpty.vue";
import AppPagination from "../components/common/AppPagination.vue";
import StarRating from "../components/common/StarRating.vue";

const router = useRouter();

const loading = ref(true);
const saving = ref(false);
const pwSaving = ref(false);
const activeTab = ref("info");
const profile = ref<any>(null);

const editForm = reactive({ bio: "" });
const pwForm = reactive({
  currentPassword: "",
  newPassword: "",
  confirmNewPassword: "",
});

const pwRules = {
  currentPassword: [
    { required: true, message: "请输入当前密码", trigger: "blur" },
  ],
  newPassword: [
    { required: true, message: "请输入新密码", trigger: "blur" },
    { min: 8, message: "密码最少8个字符", trigger: "blur" },
  ],
  confirmNewPassword: [
    { required: true, message: "请确认新密码", trigger: "blur" },
  ],
};

const papers = ref<any[]>([]);
const papersLoading = ref(false);
const papersPage = ref(1);
const papersTotal = ref(0);

const ratings = ref<any[]>([]);
const ratingsLoading = ref(false);
const ratingsPage = ref(1);
const ratingsTotal = ref(0);

const comments = ref<any[]>([]);
const commentsLoading = ref(false);
const commentsPage = ref(1);
const commentsTotal = ref(0);

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

const avatarVersion = ref(0);
function getFileExtension(fileName: string) {
  const i = fileName.lastIndexOf(".");
  return i >= 0 ? fileName.slice(i) : "";
}

const avatarUrl = computed(() => {
  if (!profile.value?.avatarPath) return "";
  const ext = getFileExtension(profile.value.avatarPath);
  return `/api/files/avatars/${profile.value.id}${ext}${avatarVersion.value > 0 ? "?v=" + avatarVersion.value : ""}`;
});

function goPaperById(id: string) {
  router.push(`/papers/${id}`);
}

async function handleAvatarUpload(uploadOptions: any) {
  try {
    const { data } = await usersApi.uploadAvatar(uploadOptions.file);
    if (data.code === 200) {
      ElMessage.success("头像已更新");
      avatarVersion.value++;
      profile.value.avatarPath = data.data.avatarPath;
    }
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "头像上传失败");
  }
}

async function handleSave() {
  saving.value = true;
  try {
    await usersApi.updateProfile({ bio: editForm.bio });
    ElMessage.success("保存成功");
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "保存失败");
  } finally {
    saving.value = false;
  }
}

async function handleChangePassword() {
  pwSaving.value = true;
  try {
    await usersApi.changePassword({
      currentPassword: pwForm.currentPassword,
      newPassword: pwForm.newPassword,
      confirmNewPassword: pwForm.confirmNewPassword,
    });
    ElMessage.success("密码已修改");
    pwForm.currentPassword = "";
    pwForm.newPassword = "";
    pwForm.confirmNewPassword = "";
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "修改失败");
  } finally {
    pwSaving.value = false;
  }
}

async function fetchPapers() {
  papersLoading.value = true;
  try {
    const { data } = await usersApi.getMyPapers({ page: papersPage.value });
    if (data.code === 200) {
      papers.value = data.data.items;
      papersTotal.value = data.data.total;
    }
  } catch {
    /* ignore */
  } finally {
    papersLoading.value = false;
  }
}

async function fetchRatings() {
  ratingsLoading.value = true;
  try {
    const { data } = await usersApi.getMyRatings({ page: ratingsPage.value });
    if (data.code === 200) {
      ratings.value = data.data.items;
      ratingsTotal.value = data.data.total;
    }
  } catch {
    /* ignore */
  } finally {
    ratingsLoading.value = false;
  }
}

async function fetchComments() {
  commentsLoading.value = true;
  try {
    const { data } = await usersApi.getMyComments({ page: commentsPage.value });
    if (data.code === 200) {
      comments.value = data.data.items;
      commentsTotal.value = data.data.total;
    }
  } catch {
    /* ignore */
  } finally {
    commentsLoading.value = false;
  }
}

const loadedTabs = ref(new Set<string>());

watch(activeTab, (tab) => {
  if (loadedTabs.value.has(tab)) return;
  loadedTabs.value.add(tab);
  if (tab === "papers") fetchPapers();
  else if (tab === "ratings") fetchRatings();
  else if (tab === "comments") fetchComments();
});

onMounted(async () => {
  try {
    const { data } = await usersApi.getProfile();
    if (data.code === 200) {
      profile.value = data.data;
      editForm.bio = data.data.bio || "";
    }
  } catch {
    /* ignore */
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
.avatar-section {
  display: flex;
  flex-direction: column;
  align-items: center;
}
.role {
  color: #409eff;
  margin: 4px 0;
}
.stats span {
  margin: 0 12px;
  color: #666;
}
.date {
  color: #999;
  font-size: 13px;
  margin-top: 4px;
}
.rating-list,
.comment-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.rating-item,
.comment-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  border: 1px solid #eee;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.15s;
}
.rating-item:hover,
.comment-item:hover {
  background: #f5f7fa;
}
.paper-title {
  flex: 1;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.comment-content {
  flex: 1;
  margin: 0;
  color: #666;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.rating-date,
.comment-date {
  color: #999;
  font-size: 12px;
  white-space: nowrap;
}
.el-table {
  cursor: pointer;
}
</style>
