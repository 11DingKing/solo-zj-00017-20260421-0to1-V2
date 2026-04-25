import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import type { RatingDistribution } from '@/types';
import './RatingDistributionChart.css';

interface RatingDistributionChartProps {
  distribution: RatingDistribution;
}

const RatingDistributionChart = ({ distribution }: RatingDistributionChartProps) => {
  const data = [
    { name: '1星', value: distribution.star1 },
    { name: '2星', value: distribution.star2 },
    { name: '3星', value: distribution.star3 },
    { name: '4星', value: distribution.star4 },
    { name: '5星', value: distribution.star5 },
  ];

  const colors = ['#ff4d4f', '#faad14', '#faad14', '#52c41a', '#52c41a'];
  const total = distribution.star1 + distribution.star2 + distribution.star3 + distribution.star4 + distribution.star5;

  if (total === 0) {
    return (
      <div className="rating-distribution">
        <h3 className="chart-title">评分分布</h3>
        <div className="empty-state">
          <div className="empty-icon">📊</div>
          <div className="empty-text">暂无评价数据</div>
        </div>
      </div>
    );
  }

  return (
    <div className="rating-distribution">
      <h3 className="chart-title">评分分布</h3>
      
      <div className="rating-bars">
        {[5, 4, 3, 2, 1].map((star) => {
          const key = `star${star}` as keyof RatingDistribution;
          const count = distribution[key];
          const percentage = total > 0 ? (count / total) * 100 : 0;
          
          return (
            <div key={star} className="rating-bar">
              <span className="rating-bar-label">{star}星</span>
              <div className="rating-bar-track">
                <div
                  className="rating-bar-fill"
                  style={{ width: `${percentage}%` }}
                />
              </div>
              <span className="rating-bar-count">{count}人</span>
            </div>
          );
        })}
      </div>
      
      <div className="chart-container">
        <ResponsiveContainer width="100%" height={200}>
          <BarChart data={data} layout="vertical" margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis type="number" />
            <YAxis dataKey="name" type="category" width={60} />
            <Tooltip
              formatter={(value: number) => [`${value} 人`, '评价数']}
              cursor={{ fill: 'rgba(0, 0, 0, 0.05)' }}
            />
            <Bar dataKey="value" radius={[0, 4, 4, 0]}>
              {data.map((_, index) => (
                <Cell key={`cell-${index}`} fill={colors[index]} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};

export default RatingDistributionChart;
