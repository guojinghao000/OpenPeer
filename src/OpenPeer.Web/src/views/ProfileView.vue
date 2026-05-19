<template>
  <div class="profile-view" v-loading="loading">
    <template v-if="profile">
      <div class="profile-header">
        <h2>{{ profile.userName }}</h2>
        <p class="role">{{ profile.role }}</p>
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
          <AppEmpty v-if="!profile.paperCount" description="还没有发布论文" />
          <el-button
            v-else
            type="primary"
            size="small"
            @click="$router.push('/')"
            >查看论文列表</el-button
          >
        </el-tab-pane>

        <el-tab-pane label="我的评分" name="ratings">
          <AppEmpty v-if="!profile.ratingCount" description="还没有评分" />
          <span v-else>共 {{ profile.ratingCount }} 条评分（M3 开放）</span>
        </el-tab-pane>

        <el-tab-pane label="我的评论" name="comments">
          <AppEmpty v-if="!profile.commentCount" description="还没有评论" />
          <span v-else>共 {{ profile.commentCount }} 条评论（M3 开放）</span>
        </el-tab-pane>
      </el-tabs>
    </template>
    <AppEmpty v-else-if="!loading" description="加载失败" />
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from "vue";
import { usersApi } from "../api/users";
import { ElMessage } from "element-plus";
import AppEmpty from "../components/common/AppEmpty.vue";

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

function formatDate(d: string) {
  return new Date(d).toLocaleDateString("zh-CN");
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
  margin: 0;
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
</style>
