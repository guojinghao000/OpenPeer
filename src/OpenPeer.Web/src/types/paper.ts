export interface PaperDto {
  id: string;
  title: string;
  abstract: string;
  author: { id: string; userName: string };
  categories: { id: string; name: string }[];
  averageRating: number;
  ratingCount: number;
  commentCount: number;
  status: string;
  publishedAt: string;
}

export interface PaperDetailDto {
  id: string;
  title: string;
  abstract: string;
  fileUrl: string;
  fileSize: number;
  author: { id: string; userName: string };
  categories: { id: string; name: string }[];
  averageRating: number;
  ratingCount: number;
  ratingDistribution: {
    star1: number;
    star2: number;
    star3: number;
    star4: number;
    star5: number;
  } | null;
  currentUserRating: number | null;
  commentCount: number;
  viewCount: number;
  status: string;
  publishedAt: string;
  updatedAt: string | null;
}

export interface PaperListParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  order?: string;
  categoryId?: string;
  keyword?: string;
  authorId?: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  description: string | null;
  paperCount: number;
  createdAt: string;
}
