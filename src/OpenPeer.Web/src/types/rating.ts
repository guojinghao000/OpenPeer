export interface RatingDto {
  id: string;
  user: { id: string; userName: string; avatarPath?: string | null };
  score: number;
  createdAt: string;
}

export interface CreateRatingRequest {
  score: number;
}

export interface RatingListResponse {
  items: RatingDto[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
  distribution: {
    star1: number;
    star2: number;
    star3: number;
    star4: number;
    star5: number;
  } | null;
}
