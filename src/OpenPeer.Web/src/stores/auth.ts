import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { authApi } from "../api/auth";
import client from "../api/client";

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
      localStorage.setItem("user", JSON.stringify(data.data.user));
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

  async function initialize() {
    const token = localStorage.getItem("accessToken");
    if (!token) return;
    accessToken.value = token;
    refreshToken.value = localStorage.getItem("refreshToken");

    const cached = localStorage.getItem("user");
    if (cached) {
      try {
        user.value = JSON.parse(cached);
      } catch {
        /* ignore */
      }
    }

    try {
      const { data } = await client.get("/users/me");
      if (data.code === 200) {
        user.value = data.data;
        localStorage.setItem("user", JSON.stringify(data.data));
      }
    } catch {
      await logout();
    }
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
    localStorage.removeItem("user");
  }

  return {
    user,
    accessToken,
    refreshToken,
    isAuthenticated,
    login,
    register,
    initialize,
    logout,
  };
});
