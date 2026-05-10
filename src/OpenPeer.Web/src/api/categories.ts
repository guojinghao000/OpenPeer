import client from "./client";

export const categoriesApi = {
  getList: () => client.get("/categories"),
};
