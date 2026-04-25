import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authApi } from '@/services/api';
import { useAuthStore } from '@/store/authStore';
import type { LoginRequest, RegisterRequest, User } from '@/types';
import './Login.css';

type AuthMode = 'login' | 'register';

const Login = () => {
  const navigate = useNavigate();
  const { setAuth } = useAuthStore();
  
  const [mode, setMode] = useState<AuthMode>('login');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [nickname, setNickname] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!username || !password) {
      setError('请填写用户名和密码');
      return;
    }

    if (mode === 'register' && (!email || !nickname)) {
      setError('请填写完整的注册信息');
      return;
    }

    try {
      setLoading(true);

      if (mode === 'login') {
        const loginRequest: LoginRequest = { username, password };
        const response = await authApi.login(loginRequest);
        
        const user: User = {
          id: '',
          username: response.data.username,
          email: '',
          nickname: response.data.nickname,
          role: response.data.role as 'Student' | 'Admin',
          createdAt: '',
        };
        
        setAuth(user, response.data.token);
        navigate('/');
      } else {
        const registerRequest: RegisterRequest = {
          username,
          email,
          password,
          nickname,
        };
        await authApi.register(registerRequest);
        setMode('login');
        setError('');
      }
    } catch (err: unknown) {
      const errorMessage = 
        err instanceof Error 
          ? err.message 
          : (err as { response?: { data?: { message?: string } } })?.response?.data?.message || '操作失败';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-container">
        <div className="card">
          <div className="auth-tabs">
            <button
              className={`auth-tab ${mode === 'login' ? 'active' : ''}`}
              onClick={() => setMode('login')}
            >
              登录
            </button>
            <button
              className={`auth-tab ${mode === 'register' ? 'active' : ''}`}
              onClick={() => setMode('register')}
            >
              注册
            </button>
          </div>

          {error && <div className="alert alert-error">{error}</div>}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label className="form-label">用户名</label>
              <input
                type="text"
                className="form-input"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="请输入用户名"
              />
            </div>

            {mode === 'register' && (
              <>
                <div className="form-group">
                  <label className="form-label">邮箱</label>
                  <input
                    type="email"
                    className="form-input"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="请输入邮箱"
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">昵称</label>
                  <input
                    type="text"
                    className="form-input"
                    value={nickname}
                    onChange={(e) => setNickname(e.target.value)}
                    placeholder="请输入昵称"
                  />
                </div>
              </>
            )}

            <div className="form-group">
              <label className="form-label">密码</label>
              <input
                type="password"
                className="form-input"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="请输入密码"
              />
            </div>

            <div className="form-actions">
              <button
                type="submit"
                className="btn btn-primary"
                style={{ width: '100%' }}
                disabled={loading}
              >
                {loading ? '处理中...' : (mode === 'login' ? '登录' : '注册')}
              </button>
            </div>
          </form>

          <div style={{ marginTop: 16, textAlign: 'center' }}>
            <Link to="/" style={{ color: 'var(--primary-color)' }}>
              返回课程列表
            </Link>
          </div>

          <div style={{ marginTop: 16, padding: 12, backgroundColor: '#f5f5f5', borderRadius: 4, fontSize: 12 }}>
            <p style={{ fontWeight: 500, marginBottom: 8 }}>测试账号：</p>
            <p>管理员：admin / Password123!</p>
            <p>学生：student1 / Password123!</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
