using CourseEvaluation.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseEvaluation.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Users.Any())
        {
            return;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        var users = new User[]
        {
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = passwordHash,
                Nickname = "管理员",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Username = "student1",
                Email = "student1@example.com",
                PasswordHash = passwordHash,
                Nickname = "张三",
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Username = "student2",
                Email = "student2@example.com",
                PasswordHash = passwordHash,
                Nickname = "李四",
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                Username = "student3",
                Email = "student3@example.com",
                PasswordHash = passwordHash,
                Nickname = "王五",
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                Username = "student4",
                Email = "student4@example.com",
                PasswordHash = passwordHash,
                Nickname = "赵六",
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        foreach (var user in users)
        {
            context.Users.Add(user);
        }

        var courses = new Course[]
        {
            new Course
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "数据结构与算法",
                TeacherName = "张教授",
                Semester = "2024-2025学年第一学期",
                Credits = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new Course
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "操作系统",
                TeacherName = "李教授",
                Semester = "2024-2025学年第一学期",
                Credits = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-55)
            },
            new Course
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = "计算机网络",
                TeacherName = "王教授",
                Semester = "2024-2025学年第二学期",
                Credits = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            },
            new Course
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Name = "数据库原理",
                TeacherName = "赵教授",
                Semester = "2024-2025学年第二学期",
                Credits = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new Course
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Name = "软件工程",
                TeacherName = "刘教授",
                Semester = "2024-2025学年第一学期",
                Credits = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-40)
            }
        };

        foreach (var course in courses)
        {
            context.Courses.Add(course);
        }

        context.SaveChanges();

        var reviews = new Review[]
        {
            new Review
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Rating = 5,
                Content = "张教授讲课非常清晰，算法讲解深入浅出，作业虽然有难度但很有挑战性，收获很大！",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Review
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Rating = 4,
                Content = "课程内容丰富，但考试有点难，建议多复习往年试卷。",
                CreatedAt = DateTime.UtcNow.AddDays(-18),
                UpdatedAt = DateTime.UtcNow.AddDays(-18)
            },
            new Review
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Rating = 5,
                Content = "强烈推荐！这门课是我上过最好的专业课之一，张教授不仅学术功底深厚，而且非常负责任。",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Review
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Rating = 3,
                Content = "操作系统概念比较抽象，需要花很多时间理解。老师讲得中规中矩吧。",
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                UpdatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new Review
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000005"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Rating = 4,
                Content = "实验部分很有帮助，通过写代码加深了对进程、内存管理的理解。",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Review
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000006"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Rating = 5,
                Content = "计算机网络非常实用！TCP/IP协议讲得很透彻，做的抓包实验很有意思。",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new Review
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000007"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Rating = 4,
                Content = "数据库原理这门课让我对SQL有了更深的理解，不过事务和锁的部分有点难。",
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                UpdatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new Review
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000008"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Rating = 5,
                Content = "软件工程的实践项目非常棒！小组合作完成一个项目，学到了很多实际开发经验。",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Review
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000009"),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Rating = 3,
                Content = "项目报告要求有点多，小组协作有时候会遇到协调问题。",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-4)
            }
        };

        foreach (var review in reviews)
        {
            context.Reviews.Add(review);
        }

        context.SaveChanges();
    }
}
