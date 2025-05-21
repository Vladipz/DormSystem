import { api } from "@/lib/utils/axios-client";

export interface DormitoryResponse {
  id: string;
  name: string;
  address: string;
}

export const dormitoryService = {
  async getDormitories(): Promise<DormitoryResponse[]> {
    console.log("Getting dormitories");
    return api.get<DormitoryResponse[]>("/dormitories");

  },
};
