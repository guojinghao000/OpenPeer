import client from "./client";

export const ratingsApi = {
  submitOrUpdate: (paperId: string, score: number) =>
    client.post(`/papers/${paperId}/ratings`, { score }),

  getList: (paperId: string, params?: { page?: number; pageSize?: number }) =>
    client.get(`/papers/${paperId}/ratings`, { params }),

  delete: (paperId: string) => client.delete(`/papers/${paperId}/ratings`),
};
