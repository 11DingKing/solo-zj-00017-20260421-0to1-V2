import axios from 'axios';
import type {
  Course,
  Review,
  CourseDetail,
  PagedResult,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  CreateReviewRequest,
  CreateCourseRequest,
  User,
} from '@/types';

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authApi = {
  login: (data: LoginRequest) =>
    api.post<LoginResponse>('/auth/login', data),
  
  register: (data: RegisterRequest) =>
    api.post<User>('/auth/register', data),
  
  getCurrentUser: () =>
    api.get<User>('/auth/me'),
};

export const coursesApi = {
  getList: (params?: {
    searchTerm?: string;
    sortBy?: string;
    sortDescending?: boolean;
    page?: number;
    pageSize?: number;
  }) =>
    api.get<PagedResult<Course>>('/courses', { params }),
  
  getById: (id: string) =>
    api.get<CourseDetail>(`/courses/${id}`),
  
  create: (data: CreateCourseRequest) =>
    api.post<Course>('/courses', data),
  
  delete: (id: string) =>
    api.delete(`/courses/${id}`),
};

export const reviewsApi = {
  createOrUpdate: (data: CreateReviewRequest) =>
    api.post<Review>('/reviews', data),
  
  delete: (id: string) =>
    api.delete(`/reviews/${id}`),
  
  getByCourse: (courseId: string) =>
    api.get<Review[]>(`/reviews/course/${courseId}`),
  
  getMyReview: (courseId: string) =>
    api.get<Review>(`/reviews/my/${courseId}`),
};

export default api;
