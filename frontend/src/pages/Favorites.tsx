import { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import StarRating from '@/components/StarRating';
import { favoritesApi } from '@/services/api';
import { useAuthStore } from '@/store/authStore';
import type { Course } from '@/types';
import './Favorites.css';

const Favorites = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuthStore();

  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [hasMore, setHasMore] = useState(false);
  const [favoritingCourseIds, setFavoritingCourseIds] = useState<Set<string>>(new Set());
  const debounceTimerRef = useRef<Map<string, NodeJS.Timeout>>(new Map());
  const pageSize = 20;

  const loadFavorites = useCallback(async () => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }

    try {
      setLoading(true);
      const response = await favoritesApi.getList({ page, pageSize });
      const newCourses = response.data.items.map((course) => ({
        ...course,
        isFavorited: true,
      }));

      if (page === 1) {
        setCourses(newCourses);
      } else {
        setCourses((prev) => [...prev, ...newCourses]);
      }

      setTotalCount(response.data.totalCount);
      setHasMore(response.data.items.length === pageSize && response.data.totalCount > page * pageSize);
      setError('');
    } catch (err) {
      setError('加载收藏列表失败');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, navigate, page]);

  useEffect(() => {
    loadFavorites();
  }, [loadFavorites]);

  const handleCourseClick = (courseId: string) => {
    navigate(`/courses/${courseId}`);
  };

  const handleToggleFavorite = useCallback(async (courseId: string, e: React.MouseEvent) => {
    e.stopPropagation();

    if (favoritingCourseIds.has(courseId)) {
      return;
    }

    const existingTimer = debounceTimerRef.current.get(courseId);
    if (existingTimer) {
      clearTimeout(existingTimer);
    }

    const timer = setTimeout(async () => {
      setFavoritingCourseIds((prev) => new Set(prev).add(courseId));

      try {
        const response = await favoritesApi.toggle(courseId);
        const newIsFavorited = response.data.isFavorited;

        if (!newIsFavorited) {
          setCourses((prevCourses) =>
            prevCourses.filter((course) => course.id !== courseId)
          );
          setTotalCount((prev) => prev - 1);
        }
      } catch (err) {
        console.error('取消收藏失败', err);
      } finally {
        setFavoritingCourseIds((prev) => {
          const next = new Set(prev);
          next.delete(courseId);
          return next;
        });
      }
    }, 300);

    debounceTimerRef.current.set(courseId, timer);
  }, [favoritingCourseIds]);

  const handleLoadMore = () => {
    if (hasMore && !loading) {
      setPage((prev) => prev + 1);
    }
  };

  if (!isAuthenticated) {
    return null;
  }

  if (loading && courses.length === 0) {
    return (
      <div className="loading">
        <div className="spinner"></div>
      </div>
    );
  }

  return (
    <div className="favorites-page">
      <div className="page-header">
        <h1 className="page-title">我的收藏</h1>
        <span className="favorites-count">共 {totalCount} 门课程</span>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {courses.length === 0 ? (
        <div className="empty-state">
          <div className="empty-icon">❤️</div>
          <div className="empty-text">暂无收藏的课程</div>
          <div className="empty-hint">去课程列表收藏一些课程吧</div>
        </div>
      ) : (
        <>
          <div className="course-list">
            {courses.map((course) => (
              <div
                key={course.id}
                className="course-card"
                onClick={() => handleCourseClick(course.id)}
              >
                <button
                  className={`favorite-btn favorited`}
                  onClick={(e) => handleToggleFavorite(course.id, e)}
                  disabled={favoritingCourseIds.has(course.id)}
                  title="取消收藏"
                >
                  ❤️
                </button>

                <div className="course-info">
                  <h3 className="course-name">{course.name}</h3>
                  <div className="course-meta">
                    <span>👨‍🏫 {course.teacherName}</span>
                    <span>📅 {course.semester}</span>
                    <span>🎓 {course.credits}学分</span>
                  </div>
                </div>

                <div className="course-stats">
                  <div className="rating-display">
                    {course.averageRating !== null ? (
                      <>
                        <StarRating rating={course.averageRating} size="small" readonly />
                        <span className="rating-value">{course.averageRating.toFixed(1)}</span>
                      </>
                    ) : (
                      <span className="rating-value" style={{ color: '#999' }}>
                        暂无评分
                      </span>
                    )}
                  </div>
                  <span className="rating-count">{course.reviewCount} 条评价</span>
                </div>
              </div>
            ))}
          </div>

          {hasMore && (
            <div className="load-more-container">
              <button
                className="btn btn-secondary load-more-btn"
                onClick={handleLoadMore}
                disabled={loading}
              >
                {loading ? '加载中...' : '加载更多'}
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default Favorites;
