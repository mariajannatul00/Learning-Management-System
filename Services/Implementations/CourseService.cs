using LMS.Web.Data;
using LMS.Web.Models;
using LMS.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Web.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISslCommerzService _sslCommerzService;

        public CourseService(ApplicationDbContext context, ISslCommerzService sslCommerzService)
        {
            _context = context;
            _sslCommerzService = sslCommerzService;
        }

        public async Task<IEnumerable<Course>> GetPublishedCoursesAsync(string? search = null, int? categoryId = null, string? level = null, string? sortBy = null)
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .Where(c => c.IsPublished && !c.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(term) 
                                      || c.ShortDescription.ToLower().Contains(term) 
                                      || c.Description.ToLower().Contains(term));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(level) && level.ToLower() != "all")
            {
                query = query.Where(c => c.Level.ToLower() == level.Trim().ToLower());
            }

            query = sortBy switch
            {
                "price-low" => query.OrderBy(c => c.DiscountPrice ?? c.Price),
                "price-high" => query.OrderByDescending(c => c.DiscountPrice ?? c.Price),
                "popular" => query.OrderByDescending(c => c.Enrollments.Count),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };


            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Courses)
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<bool> IsStudentEnrolledAsync(string studentId, int courseId)
        {
            if (string.IsNullOrEmpty(studentId)) return false;

            return await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId && e.PaymentStatus == "Completed" && e.DropStatus != "Approved");
        }

        public async Task<(bool Success, string Message, Enrollment? Enrollment)> BuyCourseAsync(string studentId, int courseId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return (false, "Please log in to purchase this course.", null);
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted && c.IsPublished);
            if (course == null)
            {
                return (false, "The requested course could not be found or is no longer available.", null);
            }

            var alreadyEnrolled = await IsStudentEnrolledAsync(studentId, courseId);
            if (alreadyEnrolled)
            {
                return (false, "You are already enrolled in this course!", null);
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                PricePaid = course.DiscountPrice ?? course.Price,
                TransactionId = $"TXN-{Guid.NewGuid().ToString("N")[..10].ToUpper()}",
                PaymentStatus = "Completed",
                DropStatus = "None"
            };

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            return (true, $"Congratulations! You have successfully enrolled in '{course.Title}'.", enrollment);
        }

        // Student Dashboard & Content Methods
        public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(string studentId)
        {
            return await _context.Enrollments
                .Include(e => e.Course!)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course!)
                    .ThenInclude(c => c.Instructor)
                .Where(e => e.StudentId == studentId && !e.IsDeleted)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseContentAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

            if (course == null) return null;

            // Load Modules and Lessons sorted by OrderIndex
            var modules = await _context.Modules
                .Include(m => m.Lessons)
                .Where(m => m.CourseId == courseId && !m.IsDeleted)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();

            foreach (var m in modules)
            {
                m.Lessons = m.Lessons.Where(l => !l.IsDeleted).OrderBy(l => l.OrderIndex).ToList();
            }

            return course;
        }

        public async Task<IEnumerable<int>> GetCompletedLessonIdsAsync(string studentId, int courseId)
        {
            var lessonIdsInCourse = await _context.Modules
                .Where(m => m.CourseId == courseId && !m.IsDeleted)
                .SelectMany(m => m.Lessons)
                .Where(l => !l.IsDeleted)
                .Select(l => l.Id)
                .ToListAsync();

            return await _context.LessonProgresses
                .Where(lp => lp.StudentId == studentId && lessonIdsInCourse.Contains(lp.LessonId) && lp.IsCompleted)
                .Select(lp => lp.LessonId)
                .ToListAsync();
        }

        public async Task<bool> ToggleLessonProgressAsync(string studentId, int lessonId)
        {
            var existing = await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LessonId == lessonId);

            if (existing != null)
            {
                existing.IsCompleted = !existing.IsCompleted;
                existing.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                var progress = new LessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };
                await _context.LessonProgresses.AddAsync(progress);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public static (decimal RefundAmount, int DeductionPercentage, string PolicyNote) CalculateDropRefund(decimal pricePaid, DateTime? startDate, DateTime? endDate, DateTime requestTime)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return (Math.Round(pricePaid * 0.90m, 2), 10, "10% deduction (Default Policy)");
            }

            var start = startDate.Value;
            var end = endDate.Value;

            // 1. 10% deduction of total amount before starting the course
            if (requestTime < start)
            {
                return (Math.Round(pricePaid * 0.90m, 2), 10, "10% deduction (Requested before course start date)");
            }

            var totalDays = (end - start).TotalDays;
            if (totalDays <= 0) totalDays = 1;

            var elapsedDays = (requestTime - start).TotalDays;
            var progressRatio = elapsedDays / totalDays;

            // 2. 25% deduction before 1/4th of total days of a course timeline
            if (progressRatio <= 0.25)
            {
                return (Math.Round(pricePaid * 0.75m, 2), 25, "25% deduction (Requested before 1/4th of course timeline)");
            }
            // 3. 50% deduction before 1/2th of total days of a course timeline
            else if (progressRatio <= 0.50)
            {
                return (Math.Round(pricePaid * 0.50m, 2), 50, "50% deduction (Requested before 1/2th of course timeline)");
            }
            // 4. 80% deduction before 3/4th of total days of a course timeline
            else if (progressRatio <= 0.75)
            {
                return (Math.Round(pricePaid * 0.20m, 2), 80, "80% deduction (Requested before 3/4th of course timeline)");
            }
            // Otherwise no money will be returned for dropping any course
            else
            {
                return (0m, 100, "0% refund (Course timeline progress exceeded refund window)");
            }
        }

        public async Task<(bool Success, string Message)> RequestCourseDropAsync(string studentId, int enrollmentId, string reason)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentId && !e.IsDeleted);

            if (enrollment == null)
            {
                return (false, "Enrollment record not found.");
            }

            var course = enrollment.Course ?? await _context.Courses.FirstOrDefaultAsync(c => c.Id == enrollment.CourseId);
            if (course != null && course.EndDate.HasValue && DateTime.UtcNow > course.EndDate.Value)
            {
                return (false, "Cannot request course drop because the course timeline has ended.");
            }

            if (enrollment.DropStatus == "Pending")
            {
                return (false, "A drop request is already pending approval for this course.");
            }

            if (enrollment.DropStatus == "Approved")
            {
                return (false, "This course has already been dropped.");
            }

            var requestTime = DateTime.UtcNow;
            var (refundAmt, deductionPct, policyNote) = CalculateDropRefund(enrollment.PricePaid, course?.StartDate, course?.EndDate, requestTime);

            enrollment.DropStatus = "Pending";
            enrollment.DropRequestedAt = requestTime;
            enrollment.DropReason = reason;
            enrollment.RefundAmount = refundAmt;
            enrollment.DeductionPercentage = deductionPct;

            await _context.SaveChangesAsync();
            return (true, $"Course drop request submitted successfully. Estimated refund: ৳{refundAmt:N2} ({deductionPct}% deduction applied).");
        }

        // Admin Management Methods
        public async Task<IEnumerable<Course>> GetAllCoursesForAdminAsync()
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CreateCourseAsync(Course course)
        {
            course.CreatedAt = DateTime.UtcNow;
            course.IsPublished = true;
            await _context.Courses.AddAsync(course);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            var existing = await _context.Courses.FirstOrDefaultAsync(c => c.Id == course.Id && !c.IsDeleted);
            if (existing == null) return false;

            existing.Title = course.Title;
            existing.ShortDescription = course.ShortDescription;
            existing.Description = course.Description;
            existing.Price = course.Price;
            existing.DiscountPrice = course.DiscountPrice;
            existing.Level = course.Level;
            existing.DurationHours = course.DurationHours;
            existing.StartDate = course.StartDate;
            existing.EndDate = course.EndDate;
            existing.TotalLectures = course.TotalLectures;
            existing.ThumbnailUrl = course.ThumbnailUrl;
            existing.IntroVideoUrl = course.IntroVideoUrl;
            existing.CategoryId = course.CategoryId;
            existing.IsPublished = course.IsPublished;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return false;

            course.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Enrollment>> GetPendingDropRequestsAsync()
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course!)
                    .ThenInclude(c => c.Category)
                .Where(e => e.DropStatus == "Pending" && !e.IsDeleted)
                .OrderByDescending(e => e.DropRequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetAllDropRequestsAsync()
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course!)
                    .ThenInclude(c => c.Category)
                .Where(e => e.DropStatus != "None" && !e.IsDeleted)
                .OrderByDescending(e => e.DropRequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetAllEnrollmentsForStatementAsync()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course!)
                    .ThenInclude(c => c.Category)
                .Where(e => !e.IsDeleted && (e.PaymentStatus == "Completed" || e.PaymentStatus == "Refunded" || e.DropStatus == "Approved"))
                .OrderByDescending(e => e.DropApprovedAt ?? e.EnrolledAt)
                .ToListAsync();

            var modified = false;
            foreach (var item in enrollments)
            {
                if (item.DropStatus == "Approved" && item.PaymentStatus != "Refunded")
                {
                    item.PaymentStatus = "Refunded";
                    modified = true;
                }
            }

            if (modified)
            {
                await _context.SaveChangesAsync();
            }

            return enrollments;
        }

        public async Task<(bool Success, string Message)> ApproveDropRequestAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && !e.IsDeleted);
            if (enrollment == null) return (false, "Enrollment not found.");

            if (enrollment.DropStatus == "Approved")
            {
                return (false, "This course drop request has already been approved.");
            }

            if (enrollment.RefundAmount == 0 && enrollment.DeductionPercentage == 0 && enrollment.DropRequestedAt.HasValue)
            {
                var (refundAmt, deductionPct, _) = CalculateDropRefund(enrollment.PricePaid, enrollment.Course?.StartDate, enrollment.Course?.EndDate, enrollment.DropRequestedAt.Value);
                enrollment.RefundAmount = refundAmt;
                enrollment.DeductionPercentage = deductionPct;
            }

            if (string.IsNullOrEmpty(enrollment.BankTranId))
            {
                enrollment.BankTranId = $"BANK-SSL-REF-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            }

            var refundRemarks = $"Refund for dropped course: {enrollment.Course?.Title ?? "Course ID " + enrollment.CourseId} ({enrollment.DeductionPercentage}% deduction)";
            await _sslCommerzService.RefundPaymentAsync(enrollment.BankTranId, enrollment.RefundAmount, refundRemarks);

            enrollment.DropStatus = "Approved";
            enrollment.DropApprovedAt = DateTime.UtcNow;
            enrollment.PaymentStatus = "Refunded";

            _context.Enrollments.Update(enrollment);
            await _context.SaveChangesAsync();

            return (true, $"Course drop request approved and ৳{enrollment.RefundAmount:N2} refunded to SSLCommerz statement ({enrollment.DeductionPercentage}% deduction applied).");
        }

        public async Task<(bool Success, string Message)> RejectDropRequestAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId && !e.IsDeleted);
            if (enrollment == null) return (false, "Enrollment not found.");

            enrollment.DropStatus = "Rejected";
            await _context.SaveChangesAsync();

            return (true, "Course drop request rejected.");
        }

        public async Task<(bool Success, string Message, Enrollment? Enrollment)> InitiateEnrollmentAsync(string studentId, int courseId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return (false, "Please log in to purchase this course.", null);
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted && c.IsPublished);
            if (course == null)
            {
                return (false, "The requested course could not be found or is no longer available.", null);
            }

            if (course.EndDate.HasValue && DateTime.UtcNow > course.EndDate.Value)
            {
                return (false, "Cannot buy this course because the course timeline has ended.", null);
            }

            var alreadyEnrolled = await IsStudentEnrolledAsync(studentId, courseId);
            if (alreadyEnrolled)
            {
                return (false, "You are already enrolled in this course!", null);
            }

            var pendingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId && e.PaymentStatus == "Pending" && !e.IsDeleted);

            if (pendingEnrollment != null)
            {
                pendingEnrollment.TransactionId = $"TXN-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";
                pendingEnrollment.PricePaid = course.DiscountPrice ?? course.Price;
                pendingEnrollment.EnrolledAt = DateTime.UtcNow;
                _context.Enrollments.Update(pendingEnrollment);
                await _context.SaveChangesAsync();
                return (true, "Enrollment payment initiated.", pendingEnrollment);
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                PricePaid = course.DiscountPrice ?? course.Price,
                TransactionId = $"TXN-{Guid.NewGuid().ToString("N")[..10].ToUpper()}",
                PaymentStatus = "Pending",
                DropStatus = "None"
            };

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            return (true, "Enrollment payment initiated.", enrollment);
        }

        public async Task<bool> CompleteEnrollmentAsync(string transactionId, string bankTranId)
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.TransactionId == transactionId && !e.IsDeleted);
            if (enrollment == null) return false;

            enrollment.PaymentStatus = "Completed";
            enrollment.BankTranId = bankTranId;
            enrollment.EnrolledAt = DateTime.UtcNow;
            
            _context.Enrollments.Update(enrollment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddModuleAsync(Module module)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == module.CourseId && !c.IsDeleted);
            if (course != null && course.EndDate.HasValue && DateTime.UtcNow > course.EndDate.Value)
            {
                return false; // Course timeline ended
            }

            module.CreatedAt = DateTime.UtcNow;
            await _context.Modules.AddAsync(module);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteModuleAsync(int moduleId)
        {
            var module = await _context.Modules.FirstOrDefaultAsync(m => m.Id == moduleId);
            if (module == null) return false;

            module.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddLessonAsync(Lesson lesson)
        {
            var module = await _context.Modules.FirstOrDefaultAsync(m => m.Id == lesson.ModuleId);
            if (module != null)
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == module.CourseId && !c.IsDeleted);
                if (course != null && course.EndDate.HasValue && DateTime.UtcNow > course.EndDate.Value)
                {
                    return false; // Course timeline ended
                }
            }

            lesson.CreatedAt = DateTime.UtcNow;
            await _context.Lessons.AddAsync(lesson);
            return await _context.SaveChangesAsync() > 0;
        }


        public async Task<bool> DeleteLessonAsync(int lessonId)
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null) return false;

            lesson.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Module>> GetModulesByCourseIdAsync(int courseId)
        {
            var list = await _context.Modules
                .Include(m => m.Lessons)
                .Where(m => m.CourseId == courseId && !m.IsDeleted)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();
            foreach (var m in list)
            {
                m.Lessons = m.Lessons.Where(l => !l.IsDeleted).OrderBy(l => l.OrderIndex).ToList();
            }
            return list;
        }
    }
}

