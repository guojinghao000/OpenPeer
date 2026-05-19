<template>
  <div class="admin-layout">
    <el-container>
      <el-aside width="200px">
        <el-menu router :default-active="$route.path">
          <el-menu-item index="/admin/categories">
            <el-icon><Folder /></el-icon><span>分类管理</span>
          </el-menu-item>
          <el-menu-item index="/admin/users">
            <el-icon><User /></el-icon><span>用户管理</span>
          </el-menu-item>
        </el-menu>
      </el-aside>
      <el-main>
        <router-view />
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { Folder, User } from "@element-plus/icons-vue";
</script>

<style scoped>
.admin-layout {
  min-height: calc(100vh - 60px);
}
.el-aside {
  border-right: 1px solid #eee;
}
</style>
