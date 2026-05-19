import client from "./client";

export const usersApi = {
  getProfile: () => client.get("/users/me"),
  updateProfile: (data: { bio: string }) => client.put("/users/me", data),
  changePassword: (data: {
    currentPassword: string;
    newPassword: string;
    confirmNewPassword: string;
  }) => client.post("/users/me/change-password", data),
  getPublicProfile: (id: string) => client.get(`/users/${id}`),
};
