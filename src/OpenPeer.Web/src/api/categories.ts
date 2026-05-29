import client from "./client";

export const categoriesApi = {
  getList: () => client.get("/categories"),
  create: (data: { name: string; description?: string }) =>
    client.post("/categories", data),
  update: (id: string, data: { name: string; description?: string }) =>
    client.put(`/categories/${id}`, data),
  delete: (id: string) => client.delete(`/categories/${id}`),
};
