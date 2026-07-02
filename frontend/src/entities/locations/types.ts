export type Location = {
  id: string;
  name: string;
  timeZone: string;
  isActive: boolean;
  createdAt: string;
  departmentIds: string[];
  address: LocationAddress;
};

export type LocationAddress = {
  country: string;
  city: string;
  street: string;
  apartment?: string | null;
  postalCode: number;
  buildingNumber: number;
};
