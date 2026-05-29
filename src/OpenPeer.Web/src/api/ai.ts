import client from "./client";

export const aiApi = {
  getConfig: () => client.get("/users/me/ai-config"),

  updateConfig: (data: { provider: string; apiKey: string; model: string }) =>
    client.put("/users/me/ai-config", data),

  generateLatex: (data: { title: string; dataIds: string[]; prompt: string }) =>
    client.post("/papers/generate", data),
};
