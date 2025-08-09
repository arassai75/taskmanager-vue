# TaskManager v1.1.0 Release Notes

**Release Date:** August 2025  
**Version:** 1.1.0

## What's New

- **Dark/Light Theme Toggle** - Switch between light and dark modes
- **Due Dates & Notifications** - Set task deadlines with browser alerts
- **Time Estimates** - Add hour estimates to tasks for better planning

## New Features

### Dark/Light Theme
- Toggle between light and dark themes
- Theme preference saved automatically
- Smooth transitions between themes
- Works across all components

### Due Dates & Notifications
- Add due dates to any task
- Browser notifications for upcoming deadlines
- Visual indicators for overdue tasks
- Smart status badges (Today, Tomorrow, Overdue)

### Time Estimates
- Add hour estimates to tasks (0.1 to 999.99 hours)
- Optional field - not required
- Displayed in task cards and lists
- Foundation for time tracking

## Technical Changes

### Backend (.NET)
- New database fields: dueDate, estimatedHours, notificationsEnabled
- Enhanced API endpoints support new fields
- Improved validation and error handling
- AutoMapper updates for new data

### Frontend (Vue.js)
- New form fields for due dates and estimates
- Theme toggle component
- Notification permission handling
- Enhanced task display with new information

### Database
- New columns in Tasks table
- Updated indexes for better performance
- Check constraints for data validation

## API Updates

**Tasks endpoints now support:**
- `dueDate` - Optional deadline
- `estimatedHours` - Optional time estimate
- `notificationsEnabled` - Enable/disable alerts

**New computed fields:**
- `isOverdue` - True if past due date
- `isDueSoon` - True if due within 24 hours
- `dueStatus` - Text status (Today, Tomorrow, Overdue, etc.)

## UI Improvements

**Task Form:**
- Date picker for due dates
- Number input for time estimates
- Toggle for notifications
- Better validation feedback

**Task Display:**
- Due date badges with colors
- Time estimate display
- Overdue task highlighting
- Compact layout improvements

**Theme System:**
- Theme toggle in header
- Consistent styling across all components
- Smooth color transitions

## Database Schema

**New Task Fields:**
- `DueDate` - DateTime, optional
- `EstimatedHours` - Decimal, optional
- `NotificationsEnabled` - Boolean, default true

**New Indexes:**
- Due date queries
- Time estimate filtering
- Notification status

## Setup Instructions

**Update Database:**
```bash
./deploy-db.sh
```

**Start Application:**
```bash
./quick-start.sh
```

**No new environment variables needed**

## Browser Compatibility

- Modern browsers with notification support
- Graceful fallback for older browsers
- Responsive design maintained
- Progressive enhancement approach

## Known Issues

- None reported


## Support

- API docs: https://localhost:7001/swagger
- Setup guide: README.md
- Database schema: database/schema/

---

**Built for interview demonstration**  
**Showcasing modern full-stack development**
