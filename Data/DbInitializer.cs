using LMS.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndSuperAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = new[] { "Super Admin", "Admin", "Student" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole(roleName, $"Default system role: {roleName}"));
                }
            }

            // Seed Super Admin
            string adminEmail = "admin@lms.com";
            var superAdminUser = await userManager.FindByEmailAsync(adminEmail);
            if (superAdminUser == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Super",
                    LastName = "Admin",
                    Designation = "System Administrator",
                    Bio = "Default System Administrator Account",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Super Admin");
                }
            }

            // Seed Sample Categories if empty
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Web Development", IconClass = "fa-solid fa-code", Description = "Master HTML, CSS, JavaScript, React, ASP.NET Core and modern backend frameworks." },
                    new Category { Name = "Data Science & AI", IconClass = "fa-solid fa-brain", Description = "Explore Machine Learning, Deep Learning, Python, and Data Analytics." },
                    new Category { Name = "Mobile Development", IconClass = "fa-solid fa-mobile-screen-button", Description = "Build cross-platform iOS & Android mobile apps with Flutter and React Native." },
                    new Category { Name = "Cloud & DevOps", IconClass = "fa-solid fa-cloud-arrow-up", Description = "Learn AWS, Azure, Docker, Kubernetes, and CI/CD deployment pipelines." },
                    new Category { Name = "UI/UX Design", IconClass = "fa-solid fa-palette", Description = "Design sleek user interfaces, wireframes, and interactive prototypes with Figma." }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Seed Sample Courses if empty
            if (!await context.Courses.AnyAsync())
            {
                var webCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Web Development");
                var dataCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Data Science & AI");
                var mobileCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Mobile Development");
                var cloudCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Cloud & DevOps");
                var designCat = await context.Categories.FirstOrDefaultAsync(c => c.Name == "UI/UX Design");

                var adminId = superAdminUser?.Id ?? "";

                var sampleCourses = new List<Course>
                {
                    new Course
                    {
                        Title = "Complete ASP.NET Core 8 MVC & Clean Architecture",
                        ShortDescription = "Master modern C#, ASP.NET Core 8 Web Apps, Entity Framework Core, Identity, and Clean Architecture best practices.",
                        Description = "Dive deep into modern Web Development using ASP.NET Core 8. You will build a production-grade Learning Management System from scratch using Repository Pattern, Dependency Injection, Identity Authentication, and Bootstrap 5.",
                        Price = 5000.00m,
                        DiscountPrice = 3500.00m,
                        Level = "Intermediate",
                        DurationHours = 18.5,
                        StartDate = DateTime.UtcNow.AddDays(-5),
                        EndDate = DateTime.UtcNow.AddDays(55),
                        TotalLectures = 12,
                        ThumbnailUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=800",
                        IntroVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
                        Rating = 4.9,
                        TotalReviews = 128,
                        IsPublished = true,
                        CategoryId = webCat?.Id ?? 1,
                        InstructorId = adminId
                    },
                    new Course
                    {
                        Title = "Modern React & Redux Toolkit Complete Guide",
                        ShortDescription = "Build scalable single-page applications with React 18, Hooks, TypeScript, Context API, and Redux Toolkit.",
                        Description = "Comprehensive step-by-step React course. Build interactive front-end dashboards, integrate REST APIs, and master modern state management techniques.",
                        Price = 6000.00m,
                        DiscountPrice = 4000.00m,
                        Level = "Beginner",
                        DurationHours = 22.0,
                        StartDate = DateTime.UtcNow.AddDays(2),
                        EndDate = DateTime.UtcNow.AddDays(60),
                        TotalLectures = 10,
                        ThumbnailUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=800",
                        IntroVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4",
                        Rating = 4.8,
                        TotalReviews = 94,
                        IsPublished = true,
                        CategoryId = webCat?.Id ?? 1,
                        InstructorId = adminId
                    },
                    new Course
                    {
                        Title = "Python for Data Science, Machine Learning & AI",
                        ShortDescription = "Learn NumPy, Pandas, Matplotlib, Scikit-Learn, and PyTorch for real-world predictive modeling.",
                        Description = "From basic Python data structures to training deep neural networks. Implement hands-on classification models, regression analytics, and natural language processing pipelines.",
                        Price = 7000.00m,
                        DiscountPrice = 4500.00m,
                        Level = "Advanced",
                        DurationHours = 30.5,
                        StartDate = DateTime.UtcNow.AddDays(-10),
                        EndDate = DateTime.UtcNow.AddDays(50),
                        TotalLectures = 15,
                        ThumbnailUrl = "https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?w=800",
                        IntroVideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4",
                        Rating = 4.9,
                        TotalReviews = 210,
                        IsPublished = true,
                        CategoryId = dataCat?.Id ?? 2,
                        InstructorId = adminId
                    }
                };


                await context.Courses.AddRangeAsync(sampleCourses);
                await context.SaveChangesAsync();
            }

            // Seed Modules & Lessons if empty
            if (!await context.Modules.AnyAsync())
            {
                var courses = await context.Courses.ToListAsync();

                foreach (var course in courses)
                {
                    var module1 = new Module
                    {
                        CourseId = course.Id,
                        Title = "Module 1: Introduction & Fundamentals",
                        OrderIndex = 1
                    };

                    var module2 = new Module
                    {
                        CourseId = course.Id,
                        Title = "Module 2: Advanced Architecture & Core Concepts",
                        OrderIndex = 2
                    };

                    await context.Modules.AddRangeAsync(module1, module2);
                    await context.SaveChangesAsync();

                    var lesson1 = new Lesson
                    {
                        ModuleId = module1.Id,
                        Title = "1.1 Course Overview & Development Setup",
                        DurationMinutes = 12,
                        VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",
                        ContentSummary = "Welcome to the course! In this lesson, we cover the course objectives, software installation, and initial project structure.",
                        OrderIndex = 1,
                        IsFreePreview = true
                    };

                    var lesson2 = new Lesson
                    {
                        ModuleId = module1.Id,
                        Title = "1.2 Core Concepts & Project Architecture",
                        DurationMinutes = 25,
                        VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4",
                        ContentSummary = "Understanding clean architecture patterns, repository abstractions, and dependency injection.",
                        OrderIndex = 2
                    };

                    var lesson3 = new Lesson
                    {
                        ModuleId = module2.Id,
                        Title = "2.1 Building Enterprise Features & Security",
                        DurationMinutes = 30,
                        VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4",
                        ContentSummary = "Implementing user authentication, role-based authorization, and secure API endpoints.",
                        OrderIndex = 1
                    };

                    var lesson4 = new Lesson
                    {
                        ModuleId = module2.Id,
                        Title = "2.2 Final Deployment & Performance Optimization",
                        DurationMinutes = 20,
                        VideoUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4",
                        ContentSummary = "Deploying web applications to production cloud environments, configuring SSL certificates, and tuning query performance.",
                        OrderIndex = 2
                    };

                    await context.Lessons.AddRangeAsync(lesson1, lesson2, lesson3, lesson4);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
