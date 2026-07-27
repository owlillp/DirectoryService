import axios from "axios";
import qs from "qs";
import { Envelope } from "./envelope";
import { EnvelopeError } from "./errors";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export const apiClient = axios.create({
  baseURL: BASE_URL,
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
