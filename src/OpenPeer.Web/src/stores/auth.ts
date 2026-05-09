import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { authApi } from "../api/auth";

export interface User {
  id: string;
  userName: string;
  email: string;
  bio?: string;
  avatarPath?: string;
  reputationScore: number;
  role: string;
}

export const useAuthStore = defineStore("auth", () => {
  const user = ref<User | null>(null);
  const accessToken = ref(localStorage.getItem("accessToken"));
  const refreshToken = ref(localStorage.getItem("refreshToken"));

  const isAuthenticated = computed(() => !!accessToken.value);

  async function login(email: string, password: string) {
    const { data } = await authApi.login({ email, password });
    if (data.code === 200) {
      accessToken.value = data.data.accessToken;
      refreshToken.value = data.data.refreshToken;
      user.value = data.data.user;
      localStorage.setItem("accessToken", data.data.accessToken);
      localStorage.setItem("refreshToken", data.data.refreshToken);
    }
  }

  async function register(
    userName: string,
    email: string,
    password: string,
    confirmPassword: string,
  ) {
    const { data } = await authApi.register({
      userName,
      email,
      password,
      confirmPassword,
    });
    return data;
  }

  async function logout() {
    if (refreshToken.value) {
      await authApi.logout(refreshToken.value).catch(() => {});
    }
    accessToken.value = null;
    refreshToken.value = null;
    user.value = null;
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
  }

  return {
    user,
    accessToken,
    refreshToken,
    isAuthenticated,
    login,
    register,
    logout,
  };
});
