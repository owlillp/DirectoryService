import axios from "axios";
import qs from "qs";
import { Envelope } from "./envelope";
import { EnvelopeError } from "./errors";

export const apiClient = axios.create({
  baseURL: "http://localhost:5209/api",
  headers: { "Content-Type": "application/json" },
  paramsSerializer: (params) =>
    qs.stringify(params, { indices: false, allowDots: true }),
});

apiClient.interceptors.response.use(
  (response) => {
    const envelope = response.data as Envelope;

    if (envelope.isFailure && envelope.errors) {
      return Promise.reject(new EnvelopeError(envelope.errors));
    }

    return response;
  },
  (error) => {
    if (axios.isAxiosError(error) && error.response?.data) {
      const envelope = error.response.data as Envelope;

      if (envelope.isFailure && envelope.errors) {
        return Promise.reject(new EnvelopeError(envelope.errors));
      }
    }

    return Promise.reject(error);
  },
);
