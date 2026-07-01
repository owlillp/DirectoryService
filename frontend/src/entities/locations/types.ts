export type Location = {
  Id: string;
  Name: string;
  TimeZone: string;
  IsActive: boolean;
  CreatedAt: Date;
  DepartmentIds: string[];
  Address: LocationAdress;
};

export type LocationAdress = {
  Country: string;
  City: string;
  Street: string;
  Apartment?: string;
  PostalCode: number;
  BuildingNumber: number;
};
