import axios from "axios";
import qs from "qs";

export const apiClient = axios.create({
  baseURL: "http://localhost:5209/api",
  headers: { "Content-Type": "application/json" },
  paramsSerializer: (params) =>
    qs.stringify(params, { indices: false, allowDots: true }),
});
