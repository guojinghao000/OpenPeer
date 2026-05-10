import client from "./client";
import type { PaperListParams } from "../types/paper";

export const papersApi = {
  getList: (params: PaperListParams) => client.get("/papers", { params }),

  getDetail: (id: string) => client.get(`/papers/${id}`),

  upload: (formData: FormData) =>
    client.post("/papers", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    }),

  update: (
    id: string,
    data: { title: string; abstract: string; categoryIds: string[] | null },
  ) => client.put(`/papers/${id}`, data),

  delete: (id: string) => client.delete(`/papers/${id}`),

  retract: (id: string, reason: string) =>
    client.post(`/papers/${id}/retract`, { reason }),
};
