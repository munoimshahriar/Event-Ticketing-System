# Assignment 2 - Complete Implementation Roadmap

## ✅ Completed Foundation

1. **Custom ApplicationUser Model** - FullName, PhoneNumber, DateOfBirth, ProfilePictureUrl
2. **Updated Models** - Event, Purchase, PurchaseItem with user relationships
3. **IdentityDbContext** - ApplicationDbContext now uses Identity
4. **Program.cs** - Identity, Authorization Policies, Serilog configured
5. **AccountController** - Registration, Login, Logout, Forgot/Reset Password
6. **ViewModels** - Register, Login, ForgotPassword, ResetPassword

## 📋 Implementation Checklist

### Phase 1: Authentication Views (Next Priority)
- [ ] Views/Account/Register.cshtml
- [ ] Views/Account/Login.cshtml
- [ ] Views/Account/ForgotPassword.cshtml
- [ ] Views/Account/ResetPassword.cshtml
- [ ] Views/Account/AccessDenied.cshtml
- [ ] Update _Layout.cshtml with Login/Logout links

### Phase 2: Role-Based Authorization
- [ ] Update EventsController with [Authorize] attributes
- [ ] Add OrganizerId to Events when created
- [ ] Restrict event editing to Organizer/Admin
- [ ] Create AdminController for admin-only features
- [ ] Test authorization policies

### Phase 3: Dashboard Controller & Views
- [ ] Controllers/DashboardController.cs
- [ ] Views/Dashboard/Index.cshtml with 4 sections:
  - My Tickets (with QR codes)
  - Purchase History (with ratings)
  - My Events (Organizers only)
  - Profile Management
- [ ] QR Code generation service
- [ ] PDF ticket generation service

### Phase 4: AJAX Functionality
- [ ] HomeController - Add SearchEvents action (returns partial view)
- [ ] Views/Shared/_EventPartial.cshtml
- [ ] JavaScript for live search (homepage)
- [ ] Shopping cart with AJAX updates
- [ ] Purchase confirmation modal with confetti

### Phase 5: Analytics Dashboard
- [ ] Controllers/AnalyticsController.cs
- [ ] JSON endpoints for chart data:
  - TicketSalesByCategory
  - RevenueByMonth
  - Top5Events
- [ ] Views/Analytics/Index.cshtml with Chart.js
- [ ] Chart.js integration

### Phase 6: Error Handling & Logging
- [ ] Views/Shared/Error.cshtml (500)
- [ ] Views/Shared/NotFound.cshtml (404)
- [ ] Update Program.cs error handling
- [ ] Verify Serilog logging in all controllers

### Phase 7: Unit Tests
- [ ] Create Tests project
- [ ] Model validation tests
- [ ] Controller action tests
- [ ] Service layer tests (if applicable)

## 🔧 Key Files to Create

### Services
- Services/QRCodeService.cs
- Services/PDFTicketService.cs
- Services/FileUploadService.cs

### ViewModels
- DashboardViewModel.cs
- AnalyticsViewModel.cs
- CartViewModel.cs

### JavaScript
- wwwroot/js/liveSearch.js
- wwwroot/js/cart.js
- wwwroot/js/confetti.js

## 📝 Notes

- Email confirmation is configured but requires email service setup
- Profile picture uploads go to wwwroot/uploads/
- QR codes should contain purchase/event information
- PDF tickets should be downloadable
- All authentication attempts are logged via Serilog

## 🚀 Next Steps

1. Create authentication views
2. Update navigation with user info
3. Implement Dashboard
4. Add AJAX functionality
5. Create Analytics
6. Add error pages
7. Write unit tests

