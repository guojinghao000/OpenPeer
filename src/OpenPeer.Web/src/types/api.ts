export interface ApiResponse<T> {
  code: number;
  message: string;
  data: T;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}

export interface UserDto {
  id: string;
  userName: string;
  email: string;
  bio?: string;
  avatarPath?: string;
  reputationScore: number;
  role: string;
  paperCount: number;
  ratingCount: number;
  commentCount: number;
  createdAt: string;
}

export interface UserRatingItem {
  id: string;
  paperId: string;
  paperTitle: string;
  score: number;
  createdAt: string;
}

export interface UserCommentItem {
  id: string;
  paperId: string;
  paperTitle: string;
  content: string;
  createdAt: string;
  updatedAt: string | null;
}
