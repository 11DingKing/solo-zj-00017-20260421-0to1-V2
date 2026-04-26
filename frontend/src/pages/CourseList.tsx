import { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import StarRating from '@/components/StarRating';
import { coursesApi, favoritesApi } from '@/services/api';
import { useAuthStore } from '@/store/authStore';
import type { Course, CreateCourseRequest } from '@/types';
import './CourseList.css';

const CourseList = () => {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuthStore();
  
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('');
  const [sortDescending, setSortDescending] = useState(true);
  const [showAddModal, setShowAddModal] = useState(false);
  const [newCourse, setNewCourse] = useState<CreateCourseRequest>({
    name: '',
    teacherName: '',
    semester: '',
    credits: 3,
  });
  const [addingCourse, setAddingCourse] = useState(false);
  const [favoritingCourseIds, setFavoritingCourseIds] = useState<Set<string>>(new Set());
  const debounceTimerRef = useRef<Map<string, ReturnType<typeof setTimeout>>>(new Map());

  const loadCourses = async () => {
    try {
      setLoading(true);
      const response = await coursesApi.getList({
        searchTerm: searchTerm || undefined,
        sortBy: sortBy || undefined,
        sortDescending,
        pageSize: 100,
      });
      setCourses(response.data.items);
      setError('');
    } catch (err) {
      setError('加载课程列表失败');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const timer = setTimeout(() => {
      loadCourses();
    }, 300);
    return () => clearTimeout(timer);
  }, [searchTerm, sortBy, sortDescending]);

  const handleCourseClick = (courseId: string) => {
    navigate(`/courses/${courseId}`);
  };

  const handleToggleFavorite = useCallback(async (courseId: string, e: React.MouseEvent) => {
    e.stopPropagation();
    
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }

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

        setCourses((prevCourses) =>
          prevCourses.map((course) =>
            course.id === courseId
              ? { ...course, isFavorited: newIsFavorited }
              : course
          )
        );
      } catch (err) {
        console.error('收藏失败', err);
      } finally {
        setFavoritingCourseIds((prev) => {
          const next = new Set(prev);
          next.delete(courseId);
          return next;
        });
      }
    }, 300);

    debounceTimerRef.current.set(courseId, timer);
  }, [isAuthenticated, navigate, favoritingCourseIds]);

  const handleAddCourse = async () => {
    if (!newCourse.name || !newCourse.teacherName || !newCourse.semester) {
      setError('请填写完整的课程信息');
      return;
    }

    try {
      setAddingCourse(true);
      await coursesApi.create(newCourse);
      setShowAddModal(false);
      setNewCourse({
        name: '',
        teacherName: '',
        semester: '',
        credits: 3,
      });
      loadCourses();
    } catch (err) {
      setError('添加课程失败');
      console.error(err);
    } finally {
      setAddingCourse(false);
    }
  };

  if (loading && courses.length === 0) {
    return (
      <div className="loading">
        <div className="spinner"></div>
      </div>
    );
  }

  return (
    <div className="course-list-page">
      <div className="page-header">
        <h1 className="page-title">课程列表</h1>
        {user?.role === 'Admin' && (
          <button className="btn btn-primary" onClick={() => setShowAddModal(true)}>
            + 添加课程
          </button>
        )}
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      <div className="search-bar">
        <input
          type="text"
          className="form-input search-input"
          placeholder="搜索课程名或教师名..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        
        <select
          className="form-select"
          value={sortBy}
          onChange={(e) => setSortBy(e.target.value)}
        >
          <option value="">默认排序</option>
          <option value="rating">按评分排序</option>
          <option value="name">按课程名排序</option>
          <option value="reviewcount">按评价数排序</option>
        </select>

        <button
          className="btn btn-secondary"
          onClick={() => setSortDescending(!sortDescending)}
        >
          {sortDescending ? '↓ 降序' : '↑ 升序'}
        </button>
      </div>

      {courses.length === 0 ? (
        <div className="empty-state">
          <div className="empty-icon">📚</div>
          <div className="empty-text">暂无课程数据</div>
        </div>
      ) : (
        <div className="course-list">
          {courses.map((course) => (
            <div
              key={course.id}
              className="course-card"
              onClick={() => handleCourseClick(course.id)}
            >
              <button
                className={`favorite-btn ${course.isFavorited ? 'favorited' : ''}`}
                onClick={(e) => handleToggleFavorite(course.id, e)}
                disabled={favoritingCourseIds.has(course.id)}
                title={course.isFavorited ? '取消收藏' : '收藏课程'}
              >
                {course.isFavorited ? '❤️' : '🤍'}
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
                      <StarRating
                        rating={course.averageRating}
                        size="small"
                        readonly
                      />
                      <span className="rating-value">{course.averageRating.toFixed(1)}</span>
                    </>
                  ) : (
                    <span className="rating-value" style={{ color: '#999' }}>暂无评分</span>
                  )}
                </div>
                <span className="rating-count">{course.reviewCount} 条评价</span>
              </div>
            </div>
          ))}
        </div>
      )}

      {showAddModal && (
        <div className="modal-overlay" onClick={() => setShowAddModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2 className="modal-title">添加新课程</h2>
              <button className="modal-close" onClick={() => setShowAddModal(false)}>
                ×
              </button>
            </div>

            <div className="form-group">
              <label className="form-label">课程名</label>
              <input
                type="text"
                className="form-input"
                value={newCourse.name}
                onChange={(e) => setNewCourse({ ...newCourse, name: e.target.value })}
                placeholder="请输入课程名"
              />
            </div>

            <div className="form-group">
              <label className="form-label">教师名</label>
              <input
                type="text"
                className="form-input"
                value={newCourse.teacherName}
                onChange={(e) => setNewCourse({ ...newCourse, teacherName: e.target.value })}
                placeholder="请输入教师名"
              />
            </div>

            <div className="form-group">
              <label className="form-label">学期</label>
              <input
                type="text"
                className="form-input"
                value={newCourse.semester}
                onChange={(e) => setNewCourse({ ...newCourse, semester: e.target.value })}
                placeholder="例如：2024-2025学年第一学期"
              />
            </div>

            <div className="form-group">
              <label className="form-label">学分</label>
              <select
                className="form-select"
                value={newCourse.credits}
                onChange={(e) => setNewCourse({ ...newCourse, credits: parseInt(e.target.value) })}
              >
                {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((n) => (
                  <option key={n} value={n}>{n}学分</option>
                ))}
              </select>
            </div>

            <div className="modal-footer">
              <button
                className="btn btn-secondary"
                onClick={() => setShowAddModal(false)}
              >
                取消
              </button>
              <button
                className="btn btn-primary"
                onClick={handleAddCourse}
                disabled={addingCourse}
              >
                {addingCourse ? '添加中...' : '添加'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CourseList;
