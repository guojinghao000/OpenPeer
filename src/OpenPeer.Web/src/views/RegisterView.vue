<template>
  <div class="register-view">
    <h2>注册</h2>
    <el-form ref="formRef" :model="form" :rules="rules" label-width="0">
      <el-form-item prop="userName">
        <el-input v-model="form.userName" placeholder="用户名" size="large" />
      </el-form-item>
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
      <el-form-item prop="confirmPassword">
        <el-input
          v-model="form.confirmPassword"
          type="password"
          placeholder="确认密码"
          size="large"
        />
      </el-form-item>
      <el-form-item>
        <el-button
          type="primary"
          size="large"
          :loading="loading"
          style="width: 100%"
          @click="handleRegister"
        >
          注册
        </el-button>
      </el-form-item>
    </el-form>
    <p class="link">已有账号？<router-link to="/login">去登录</router-link></p>
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
const form = reactive({
  userName: "",
  email: "",
  password: "",
  confirmPassword: "",
});

const validateConfirm = (_rule: any, value: string, callback: any) => {
  if (value !== form.password) callback(new Error("两次密码输入不一致"));
  else callback();
};

const rules = {
  userName: [
    { required: true, message: "请输入用户名", trigger: "blur" },
    { min: 3, max: 20, message: "用户名长度3-20字符", trigger: "blur" },
  ],
  email: [{ required: true, message: "请输入邮箱", trigger: "blur" }],
  password: [
    { required: true, message: "请输入密码", trigger: "blur" },
    { min: 8, message: "密码最少8个字符", trigger: "blur" },
  ],
  confirmPassword: [
    { required: true, message: "请确认密码", trigger: "blur" },
    { validator: validateConfirm, trigger: "blur" },
  ],
};

async function handleRegister() {
  loading.value = true;
  try {
    await auth.register(
      form.userName,
      form.email,
      form.password,
      form.confirmPassword,
    );
    ElMessage.success("注册成功");
    router.push("/login");
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message || "注册失败");
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.register-view h2 {
  text-align: center;
  margin-bottom: 24px;
}
.link {
  text-align: center;
  margin-top: 16px;
}
</style>
