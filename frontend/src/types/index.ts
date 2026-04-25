export interface User {
  id: string;
  username: string;
  email: string;
  nickname: string;
  role: 'Student' | 'Admin';
  createdAt: string;
}

export interface Course {
  id: string;
  name: string;
  teacherName: string;
  semester: string;
  credits: number;
  averageRating: number | null;
  reviewCount: number;
  createdAt: string;
  isFavorited?: boolean | null;
}

export interface Review {
  id: string;
  courseId: string;
  userId: string;
  userNickname: string;
  rating: number;
  content: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface RatingDistribution {
  star1: number;
  star2: number;
  star3: number;
  star4: number;
  star5: number;
}

export interface CourseDetail {
  course: Course;
  reviews: Review[];
  ratingDistribution: RatingDistribution;
  hasUserReviewed: boolean;
  userReview: Review | null;
  isFavorited?: boolean | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  nickname: string;
  role: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  nickname: string;
}

export interface CreateReviewRequest {
  courseId: string;
  rating: number;
  content: string | null;
}

export interface CreateCourseRequest {
  name: string;
  teacherName: string;
  semester: string;
  credits: number;
}
