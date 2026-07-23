import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export type DepartmentListId = string;

type DepartmentListState = {
  search?: string;
  isActive?: boolean;
  parentId?: string;
  locationIds?: string[];
  excludeIds?: string[];
  sortBy?: string;
  sortDirection?: string;
  pageSize: number;
};

type DepartmentListStates = Record<
  DepartmentListId,
  DepartmentListState | undefined
>;

const DEFAULT_STATE_ID = "__default__";

const initialState: DepartmentListState = {
  search: "",
  isActive: undefined,
  parentId: undefined,
  locationIds: undefined,
  excludeIds: undefined,
  sortBy: undefined,
  sortDirection: undefined,
  pageSize: 10,
};

const initialStates: DepartmentListStates = {};

const resolvedStateId = (stateId?: DepartmentListId) =>
  stateId ?? DEFAULT_STATE_ID;

const getOrCreate = (
  state: DepartmentListStates,
  stateId?: DepartmentListId,
): DepartmentListState => {
  const id = resolvedStateId(stateId);
  if (!state[id]) state[id] = { ...initialState };

  return state[id];
};

const useDepartmentListStore = create<DepartmentListStates>()(
  persist(
    () => ({
      ...initialStates,
    }),
    {
      name: "department-list-storage",
      storage: createJSONStorage(() => localStorage),
      partialize: (state) =>
        Object.fromEntries(
          Object.entries(state).filter(([key]) => key === DEFAULT_STATE_ID),
        ),
    },
  ),
);

export const useDepartmentSearch = (stateId?: DepartmentListId) =>
  useDepartmentListStore((states) => getOrCreate(states, stateId).search);

export const setDepartmentSearch = (
  search: string,
  stateId?: DepartmentListId,
) => {
  useDepartmentListStore.setState((states) => ({
    [resolvedStateId(stateId)]: {
      ...getOrCreate(states, stateId),
      search,
    },
  }));
};

export const useDepartmentActive = (stateId?: DepartmentListId) =>
  useDepartmentListStore((states) => getOrCreate(states, stateId).isActive);

export const setDepartmentActive = (
  isActive?: boolean,
  stateId?: DepartmentListId,
) => {
  useDepartmentListStore.setState((states) => ({
    [resolvedStateId(stateId)]: {
      ...getOrCreate(states, stateId),
      isActive,
    },
  }));
};

export const useDepartmentParentId = (stateId?: DepartmentListId) =>
  useDepartmentListStore((states) => getOrCreate(states, stateId).parentId);

export const setDepartmentParentId = (
  parentId?: string,
  stateId?: DepartmentListId,
) => {
  useDepartmentListStore.setState((states) => ({
    [resolvedStateId(stateId)]: {
      ...getOrCreate(states, stateId),
      parentId,
    },
  }));
};

export const useDepartmentLocationIds = (stateId?: DepartmentListId) =>
  useDepartmentListStore((states) => getOrCreate(states, stateId).locationIds);

export const setDepartmentLocationIds = (
  locationIds?: string[],
  stateId?: DepartmentListId,
) => {
  useDepartmentListStore.setState((states) => ({
    [resolvedStateId(stateId)]: {
      ...getOrCreate(states, stateId),
      locationIds,
    },
  }));
};

export const useDepartmentExcludeIds = (stateId?: DepartmentListId) =>
  useDepartmentListStore((states) => getOrCreate(states, stateId).excludeIds);

export const setDepartmentExcludeIds = (
  excludeIds?: string[],
  stateId?: DepartmentListId,
) => {
  useDepartmentListStore.setState((states) => ({
    [resolvedStateId(stateId)]: {
      ...getOrCreate(states, stateId),
      excludeIds,
    },
  }));
};

export const useDepartmentSortBy = (stateId?: DepartmentListId) =>
  useDepartmentListStore((states) => getOrCreate(states, stateId).sortBy);

export const setDepartmentSortBy = (
  sortBy?: string,
  stateId?: DepartmentListId,
) => {
  useDepartmentListStore.setState((states) => ({
    [resolvedStateId(stateId)]: {
      ...getOrCreate(states, stateId),
      sortBy,
    },
  }));
};

export const useDepartmentSortDirection = (stateId?: DepartmentListId) =>
  useDepartmentListStore(
    (states) => getOrCreate(states, stateId).sortDirection,
  );

export const setDepartmentSortDirection = (
  sortDirection?: string,
  stateId?: DepartmentListId,
) => {
  useDepartmentListStore.setState((states) => ({
    [resolvedStateId(stateId)]: {
      ...getOrCreate(states, stateId),
      sortDirection,
    },
  }));
};

export const useDepartmentPageSize = (stateId?: DepartmentListId) =>
  useDepartmentListStore((states) => getOrCreate(states, stateId).pageSize);

export const setDepartmentPageSize = (
  pageSize: number,
  stateId?: DepartmentListId,
) => {
  useDepartmentListStore.setState((states) => ({
    [resolvedStateId(stateId)]: {
      ...getOrCreate(states, stateId),
      pageSize,
    },
  }));
};

/** Reset state for a given instance (used on unmount to clear filters/search) */
export const resetDepartmentState = (stateId?: DepartmentListId) => {
  useDepartmentListStore.setState((states) => {
    const id = resolvedStateId(stateId);
    const { [id]: _, ...rest } = states;
    return rest;
  });
};
