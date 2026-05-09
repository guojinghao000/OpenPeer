<template>
  <div class="default-layout">
    <header>
      <div class="header-content">
        <router-link to="/" class="logo">OpenPeer</router-link>
        <nav>
          <template v-if="auth.isAuthenticated">
            <router-link to="/upload">上传论文</router-link>
            <router-link to="/profile">{{ auth.user?.userName }}</router-link>
            <a @click="handleLogout">退出</a>
          </template>
          <template v-else>
            <router-link to="/login">登录</router-link>
            <router-link to="/register">注册</router-link>
          </template>
        </nav>
      </div>
    </header>
    <main>
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from "../stores/auth";
import { useRouter } from "vue-router";

const auth = useAuthStore();
const router = useRouter();

async function handleLogout() {
  await auth.logout();
  router.push("/");
}
</script>

<style scoped>
.default-layout header {
  background: #fff;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.1);
  padding: 0 24px;
}
.header-content {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 60px;
}
.logo {
  font-size: 20px;
  font-weight: bold;
  color: #409eff;
  text-decoration: none;
}
nav {
  display: flex;
  gap: 16px;
  align-items: center;
}
nav a {
  color: #333;
  text-decoration: none;
  cursor: pointer;
}
nav a:hover {
  color: #409eff;
}
main {
  max-width: 1200px;
  margin: 24px auto;
  padding: 0 24px;
}
</style>
