export interface SupportingDataItem {
  id: string;
  fileName: string;
  fileType: string;
  fileSize: number;
  description: string | null;
  userName: string;
  createdAt: string;
}

export interface AiConfig {
  provider: string;
  model: string;
  hasApiKey: boolean;
}
