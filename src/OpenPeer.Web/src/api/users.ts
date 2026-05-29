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
  getMyPapers: (params?: { page?: number; pageSize?: number }) =>
    client.get("/users/me/papers", { params }),
  getMyRatings: (params?: { page?: number; pageSize?: number }) =>
    client.get("/users/me/ratings", { params }),
  getMyComments: (params?: { page?: number; pageSize?: number }) =>
    client.get("/users/me/comments", { params }),
  getAdminList: (params?: {
    page?: number;
    pageSize?: number;
    search?: string;
  }) => client.get("/users/admin/list", { params }),
  updateUserRole: (id: string, data: { role: string }) =>
    client.put(`/users/admin/${id}/role`, data),
  uploadAvatar: (file: File) => {
    const form = new FormData();
    form.append("file", file);
    return client.post("/users/me/avatar", form, {
      headers: { "Content-Type": "multipart/form-data" },
    });
  },
};
