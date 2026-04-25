import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Header from '@/components/Header';
import CourseList from '@/pages/CourseList';
import CourseDetail from '@/pages/CourseDetail';
import Login from '@/pages/Login';
import Favorites from '@/pages/Favorites';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <div className="app">
        <Header />
        <main className="main-content">
          <div className="container">
            <Routes>
              <Route path="/" element={<CourseList />} />
              <Route path="/courses/:id" element={<CourseDetail />} />
              <Route path="/login" element={<Login />} />
              <Route path="/favorites" element={<Favorites />} />
            </Routes>
          </div>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
