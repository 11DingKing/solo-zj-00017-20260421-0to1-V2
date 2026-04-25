import { useState } from 'react';
import './StarRating.css';

interface StarRatingProps {
  rating: number;
  onChange?: (rating: number) => void;
  size?: 'small' | 'medium' | 'large';
  readonly?: boolean;
}

const StarRating = ({
  rating,
  onChange,
  size = 'medium',
  readonly = false,
}: StarRatingProps) => {
  const [hoverRating, setHoverRating] = useState(0);

  const sizeClasses = {
    small: 'star-small',
    medium: 'star-medium',
    large: 'star-large',
  };

  const handleMouseEnter = (index: number) => {
    if (!readonly && onChange) {
      setHoverRating(index + 1);
    }
  };

  const handleMouseLeave = () => {
    if (!readonly && onChange) {
      setHoverRating(0);
    }
  };

  const handleClick = (index: number) => {
    if (!readonly && onChange) {
      onChange(index + 1);
    }
  };

  const displayRating = hoverRating || rating;

  const renderStar = (index: number) => {
    const isFilled = index < displayRating;
    const isHalf = !isFilled && index < Math.ceil(displayRating) && displayRating % 1 !== 0;

    return (
      <span
        key={index}
        className={`star ${sizeClasses[size]} ${readonly ? 'star-readonly' : 'star-interactive'}`}
        onMouseEnter={() => handleMouseEnter(index)}
        onMouseLeave={handleMouseLeave}
        onClick={() => handleClick(index)}
      >
        <svg
          viewBox="0 0 24 24"
          fill={isFilled ? 'currentColor' : 'none'}
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2" />
        </svg>
        {isHalf && (
          <svg
            className="star-half"
            viewBox="0 0 24 24"
            fill="currentColor"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <defs>
              <clipPath id={`half-clip-${index}`}>
                <rect x="0" y="0" width="12" height="24" />
              </clipPath>
            </defs>
            <polygon
              points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"
              clipPath={`url(#half-clip-${index})`}
            />
          </svg>
        )}
      </span>
    );
  };

  return (
    <div className="star-rating">
      {[0, 1, 2, 3, 4].map(renderStar)}
    </div>
  );
};

export default StarRating;
