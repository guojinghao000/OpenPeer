export interface CommentDto {
  id: string;
  user: { id: string; userName: string; avatarPath?: string | null };
  content: string;
  parentId: string | null;
  replies: CommentDto[];
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCommentRequest {
  content: string;
  parentId?: string | null;
}

export interface UpdateCommentRequest {
  content: string;
}
