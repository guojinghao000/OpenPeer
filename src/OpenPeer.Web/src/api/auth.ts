import client from "./client";

interface LoginRequest {
  email: string;
  password: string;
}

interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export const authApi = {
  login: (data: LoginRequest) => client.post("/auth/login", data),
  register: (data: RegisterRequest) => client.post("/auth/register", data),
  refresh: (refreshToken: string) =>
    client.post("/auth/refresh", { refreshToken }),
  logout: (refreshToken: string) =>
    client.post("/auth/logout", { refreshToken }),
};
