import { apiClient } from "@/src/shared/api/axios-instance";
import { Envelope } from "@/src/shared/api/envelope";
import { Position } from "./types";
import {
  CursorPagedResult,
  CursorPaginationRequest,
} from "@/src/shared/api/types";

export type GetCursorPositionsRequest = {
  cursorRequest: CursorPaginationRequest;
  search?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
};

export const positionsApi = {
  getPositionsCursor: async (request: GetCursorPositionsRequest) => {
    const response = await apiClient.get<Envelope<CursorPagedResult<Position>>>(
      "/Positions",
      { params: request },
    );
    return response.data.result;
  },
};

export const positionsQueryOptions = {
  baseKey: "positions",
};
