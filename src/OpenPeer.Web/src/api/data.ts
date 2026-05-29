import client from "./client";

export const dataApi = {
  upload: (paperId: string, file: File, description?: string) => {
    const form = new FormData();
    form.append("file", file);
    if (description) form.append("description", description);
    return client.post(`/papers/${paperId}/data`, form, {
      headers: { "Content-Type": "multipart/form-data" },
    });
  },

  getList: (paperId: string) => client.get(`/papers/${paperId}/data`),

  delete: (paperId: string, id: string) =>
    client.delete(`/papers/${paperId}/data/${id}`),
};
