<template>
  <div class="login-view">
    <h2>登录</h2>
    <el-form ref="formRef" :model="form" :rules="rules" label-width="0">
      <el-form-item prop="email">
        <el-input v-model="form.email" placeholder="邮箱" size="large" />
      </el-form-item>
      <el-form-item prop="password">
        <el-input
          v-model="form.password"
          type="password"
          placeholder="密码"
          size="large"
        />
      </el-form-item>
      <el-form-item>
        <el-button
          type="primary"
          size="large"
          :loading="loading"
          style="width: 100%"
          @click="handleLogin"
        >
          登录
        </el-button>
      </el-form-item>
    </el-form>
    <p class="link">
      还没有账号？<router-link to="/register">去注册</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";
import { ElMessage } from "element-plus";

const router = useRouter();
const auth = useAuthStore();
const loading = ref(false);
const form = reactive({ email: "", password: "" });
const rules = {
  email: [{ required: true, message: "请输入邮箱", trigger: "blur" }],
  password: [{ required: true, message: "请输入密码", trigger: "blur" }],
};

async function handleLogin() {
  loading.value = true;
  try {
    await auth.login(form.email, form.password);
    ElMessage.success("登录成功");
    router.push("/");
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "登录失败");
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.login-view h2 {
  text-align: center;
  margin-bottom: 24px;
}
.link {
  text-align: center;
  margin-top: 16px;
}
</style>
