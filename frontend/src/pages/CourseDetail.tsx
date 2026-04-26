import { useState, useEffect, useRef, useCallback } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import StarRating from "@/components/StarRating";
import RatingDistributionChart from "@/components/RatingDistributionChart";
import { coursesApi, reviewsApi, favoritesApi } from "@/services/api";
import { useAuthStore } from "@/store/authStore";
import type { CourseDetail, Review, CreateReviewRequest } from "@/types";
import "./CourseDetail.css";

const CourseDetail = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuthStore();

  const [courseDetail, setCourseDetail] = useState<CourseDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [newRating, setNewRating] = useState(5);
  const [newContent, setNewContent] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [showLoginPrompt, setShowLoginPrompt] = useState(false);
  const [favoriting, setFavoriting] = useState(false);
  const [reviewSortBy, setReviewSortBy] = useState<string>("createdAt");
  const [reviewSortDescending, setReviewSortDescending] = useState(true);
  const [sortedReviews, setSortedReviews] = useState<Review[]>([]);
  const debounceTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const loadCourseDetail = async () => {
    if (!id) return;

    try {
      setLoading(true);
      const response = await coursesApi.getById(id);
      setCourseDetail(response.data);
      setSortedReviews(response.data.reviews);
      setError("");

      if (response.data.userReview) {
        setNewRating(response.data.userReview.rating);
        setNewContent(response.data.userReview.content || "");
      }
    } catch (err) {
      setError("加载课程详情失败");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadReviews = useCallback(async () => {
    if (!id) return;

    try {
      const response = await reviewsApi.getByCourse(id, {
        sortBy: reviewSortBy,
        sortDescending: reviewSortDescending,
      });
      setSortedReviews(response.data);
    } catch (err) {
      console.error("加载评价失败", err);
    }
  }, [id, reviewSortBy, reviewSortDescending]);

  useEffect(() => {
    loadCourseDetail();
  }, [id]);

  useEffect(() => {
    if (courseDetail) {
      loadReviews();
    }
  }, [courseDetail, loadReviews]);

  const handleToggleFavorite = useCallback(async () => {
    if (!isAuthenticated) {
      navigate("/login");
      return;
    }

    if (!id || favoriting) return;

    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }

    debounceTimerRef.current = setTimeout(async () => {
      setFavoriting(true);

      try {
        const response = await favoritesApi.toggle(id);
        const newIsFavorited = response.data.isFavorited;

        setCourseDetail((prev) =>
          prev
            ? {
                ...prev,
                isFavorited: newIsFavorited,
                course: { ...prev.course, isFavorited: newIsFavorited },
              }
            : null
        );
      } catch (err) {
        console.error("收藏失败", err);
      } finally {
        setFavoriting(false);
      }
    }, 300);
  }, [id, isAuthenticated, navigate, favoriting]);

  const handleSubmitReview = async () => {
    if (!isAuthenticated) {
      setShowLoginPrompt(true);
      return;
    }

    if (!id) return;

    try {
      setSubmitting(true);

      const request: CreateReviewRequest = {
        courseId: id,
        rating: newRating,
        content: newContent.trim() || null,
      };

      await reviewsApi.createOrUpdate(request);
      loadCourseDetail();
      setError("");
    } catch (err) {
      setError("提交评价失败");
      console.error(err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteReview = async (reviewId: string) => {
    if (!window.confirm("确定要删除这条评价吗？")) {
      return;
    }

    try {
      await reviewsApi.delete(reviewId);
      loadCourseDetail();
    } catch (err) {
      setError("删除评价失败");
      console.error(err);
    }
  };

  const canDeleteReview = (review: Review) => {
    if (!user) return false;
    if (user.role === "Admin") return true;
    return review.userId === user.id;
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("zh-CN", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  if (loading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
      </div>
    );
  }

  if (!courseDetail) {
    return (
      <div className="empty-state">
        <div className="empty-icon">📚</div>
        <div className="empty-text">课程不存在</div>
        <Link to="/" className="btn btn-primary" style={{ marginTop: 16 }}>
          返回课程列表
        </Link>
      </div>
    );
  }

  const { course, ratingDistribution, hasUserReviewed } = courseDetail;

  return (
    <div className="course-detail-page">
      <Link to="/" className="back-link">
        ← 返回课程列表
      </Link>

      {error && <div className="alert alert-error">{error}</div>}

      <div className="course-header card">
        <div className="course-header-top">
          <h1 className="page-title">{course.name}</h1>
          <button
            className={`favorite-btn ${courseDetail.isFavorited ? "favorited" : ""}`}
            onClick={handleToggleFavorite}
            disabled={favoriting}
            title={courseDetail.isFavorited ? "取消收藏" : "收藏课程"}
          >
            {courseDetail.isFavorited ? "❤️" : "🤍"}
          </button>
        </div>
        <div className="course-meta">
          <span>👨‍🏫 {course.teacherName}</span>
          <span>📅 {course.semester}</span>
          <span>🎓 {course.credits}学分</span>
        </div>

        <div className="course-rating-summary">
          {course.averageRating !== null ? (
            <>
              <StarRating rating={course.averageRating} size="large" readonly />
              <span className="average-rating">
                {course.averageRating.toFixed(1)}
              </span>
              <span className="review-count">
                ({course.reviewCount} 条评价)
              </span>
            </>
          ) : (
            <span className="no-rating">暂无评分</span>
          )}
        </div>
      </div>

      <div className="review-form-section">
        <div className="review-form card">
          <h3 className="review-form-title">
            {hasUserReviewed ? "修改我的评价" : "发表评价"}
          </h3>

          {showLoginPrompt && (
            <div className="alert alert-error">
              请先
              <Link
                to="/login"
                style={{ marginLeft: 8, color: "var(--primary-color)" }}
              >
                登录
              </Link>
              后再发表评价
            </div>
          )}

          <div className="form-group">
            <label className="form-label">评分</label>
            <StarRating
              rating={newRating}
              onChange={setNewRating}
              size="large"
              readonly={!isAuthenticated}
            />
          </div>

          <div className="form-group">
            <label className="form-label">评价内容</label>
            <textarea
              className="form-input form-textarea"
              value={newContent}
              onChange={(e) => setNewContent(e.target.value)}
              placeholder="分享你对这门课的感受..."
              disabled={!isAuthenticated}
            />
          </div>

          <div className="form-actions">
            <button
              className="btn btn-primary"
              onClick={handleSubmitReview}
              disabled={submitting || !isAuthenticated}
            >
              {submitting
                ? "提交中..."
                : hasUserReviewed
                  ? "修改评价"
                  : "发表评价"}
            </button>
          </div>
        </div>
      </div>

      {course.reviewCount > 0 && (
        <RatingDistributionChart distribution={ratingDistribution} />
      )}

      <div className="reviews-section">
        <div className="reviews-header">
          <h2 className="section-title">全部评价 ({sortedReviews.length})</h2>
          <div className="review-sort-controls">
            <select
              className="form-select sort-select"
              value={reviewSortBy}
              onChange={(e) => setReviewSortBy(e.target.value)}
            >
              <option value="createdAt">按时间排序</option>
              <option value="rating">按评分排序</option>
            </select>
            <button
              className="btn btn-secondary sort-order-btn"
              onClick={() => setReviewSortDescending(!reviewSortDescending)}
            >
              {reviewSortDescending ? "↓ 降序" : "↑ 升序"}
            </button>
          </div>
        </div>

        {sortedReviews.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">💬</div>
            <div className="empty-text">暂无评价，快来发表第一条评价吧！</div>
          </div>
        ) : (
          <div className="reviews-list">
            {sortedReviews.map((review) => (
              <div key={review.id} className="review-card card">
                <div className="review-header">
                  <div className="review-user">
                    <div className="review-avatar">
                      {review.userNickname.charAt(0)}
                    </div>
                    <div>
                      <div className="review-nickname">
                        {review.userNickname}
                      </div>
                      <div className="review-time">
                        {formatDate(review.createdAt)}
                      </div>
                    </div>
                  </div>

                  {canDeleteReview(review) && (
                    <button
                      className="btn btn-secondary"
                      onClick={() => handleDeleteReview(review.id)}
                      style={{ fontSize: 12, padding: "4px 8px" }}
                    >
                      删除
                    </button>
                  )}
                </div>

                <div className="review-rating">
                  <StarRating rating={review.rating} size="small" readonly />
                </div>

                {review.content && (
                  <div className="review-content">{review.content}</div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default CourseDetail;
