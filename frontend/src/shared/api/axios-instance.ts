import axios from "axios";

export const apiClient = axios.create({
  baseURL: "http://localhost:5209/api",
  headers: { "Content-Type": "application/json" },
});
