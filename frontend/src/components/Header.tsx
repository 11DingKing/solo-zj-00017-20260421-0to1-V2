import { Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';

const Header = () => {
  const { user, isAuthenticated, clearAuth } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    clearAuth();
    navigate('/');
  };

  return (
    <header className="header">
      <div className="container">
        <div className="header-content">
          <Link to="/" className="logo">
            📚 课程评价系统
          </Link>
          
          <nav className="nav">
            <Link to="/" className="nav-link">
              课程列表
            </Link>
            
            {isAuthenticated ? (
              <>
                <div className="user-info">
                  <span className="user-name">{user?.nickname}</span>
                  <span className="user-role">
                    {user?.role === 'Admin' ? '管理员' : '学生'}
                  </span>
                  <button className="btn btn-secondary" onClick={handleLogout}>
                    退出登录
                  </button>
                </div>
              </>
            ) : (
              <Link to="/login" className="btn btn-primary">
                登录 / 注册
              </Link>
            )}
          </nav>
        </div>
      </div>
    </header>
  );
};

export default Header;
