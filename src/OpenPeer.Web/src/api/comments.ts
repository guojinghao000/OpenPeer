import client from "./client";

export const commentsApi = {
  getList: (paperId: string, params?: { page?: number; pageSize?: number }) =>
    client.get(`/papers/${paperId}/comments`, { params }),

  create: (paperId: string, content: string, parentId?: string | null) =>
    client.post(`/papers/${paperId}/comments`, { content, parentId }),

  update: (id: string, content: string) =>
    client.put(`/comments/${id}`, { content }),

  delete: (id: string) => client.delete(`/comments/${id}`),
};
